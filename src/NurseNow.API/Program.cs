using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NurseNow.API.Middleware;
using NurseNow.API.Services;
using NurseNow.Application;
using NurseNow.Application.Abstractions;
using NurseNow.Infrastructure;
using NurseNow.Data;
using NurseNow.Models;
using NurseNow.Seed;

var builder = WebApplication.CreateBuilder(args);
var jwt = builder.Configuration.GetSection("JWT");
var secret = jwt["Secret"];
if (string.IsNullOrWhiteSpace(secret) || Encoding.UTF8.GetByteCount(secret) < 32)
    throw new InvalidOperationException("Configure JWT:Secret with at least 32 random bytes through user secrets or environment variables.");
if (string.IsNullOrWhiteSpace(jwt["Issuer"]) || string.IsNullOrWhiteSpace(jwt["Audience"]))
    throw new InvalidOperationException("Configure JWT:Issuer and JWT:Audience.");
if (!int.TryParse(jwt["DurationInMinutes"], out var duration) || duration is < 1 or > 120)
    throw new InvalidOperationException("JWT:DurationInMinutes must be between 1 and 120.");
if (string.IsNullOrWhiteSpace(builder.Configuration.GetConnectionString("DefaultConnection")))
    throw new InvalidOperationException("Configure ConnectionStrings:DefaultConnection.");
if (!Uri.TryCreate(builder.Configuration["PublicBaseUrl"], UriKind.Absolute, out var origin) || origin.Scheme is not ("http" or "https"))
    throw new InvalidOperationException("Configure an absolute PublicBaseUrl.");
builder.Configuration["Storage:Root"] = Path.GetFullPath(builder.Configuration["Storage:Root"] ?? "App_Data/uploads", builder.Environment.ContentRootPath);
builder.Services.AddApplication().AddInfrastructure(builder.Configuration);
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IRequestContext, RequestContext>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwt["Issuer"],
        ValidAudience = jwt["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
        ClockSkew = TimeSpan.FromSeconds(30)
    };
    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = async context =>
        {
            var id = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
            var users = context.HttpContext.RequestServices.GetRequiredService<NurseNow.Application.Abstractions.IIdentityService>();
            var user = id is null ? null : await users.FindByIdAsync(id);
            if (user is null || user.AccountStatus != "Active" || (user.RoleType == "Administrator" && user.AdminApprovalStatus != "Active") || user.SecurityStamp != context.Principal?.FindFirstValue("security_stamp") || (user.AdminRole ?? "") != context.Principal?.FindFirstValue("admin_role"))
                context.Fail("Account or token is no longer valid.");
        }
    };
});
builder.Services.AddAuthorization();
builder.Services.AddCors(options => options.AddPolicy("Frontend", policy =>
{
    var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
    if (origins.Length > 0)
        policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod();
}));
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("auth", context => RateLimitPartition.GetFixedWindowLimiter(context.Connection.RemoteIpAddress?.ToString() ?? "unknown", _ => new FixedWindowRateLimiterOptions { PermitLimit = 20, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
});
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme { Name = "Authorization", Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http, Scheme = "bearer", BearerFormat = "JWT" });
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement { [new Microsoft.OpenApi.Models.OpenApiSecurityScheme { Reference = new Microsoft.OpenApi.Models.OpenApiReference { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer" } }] = Array.Empty<string>() });
});
var app = builder.Build();
app.UseExceptionHandler();
if (!app.Environment.IsDevelopment())
    app.UseHsts();
app.UseHttpsRedirection();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseCors("Frontend");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/health/live", () => Results.Ok(new { status = "ok" })).AllowAnonymous();
if (builder.Configuration.GetValue<bool>("Database:SeedOnStartup"))
{
    await using var scope = app.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await using var transaction = await db.Database.BeginTransactionAsync();
    await RoleSeeder.SeedRolesAndAdminAsync(scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>(), scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>(), builder.Configuration, scope.ServiceProvider.GetRequiredService<IUnitOfWork>());
    await ServiceCatalogSeeder.SeedServiceCatalogAsync(db);
    await transaction.CommitAsync();
}

app.Run();
public partial class Program
{
}
