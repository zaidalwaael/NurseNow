using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NurseNow.Data;
using NurseNow.DTOs;
using NurseNow.Helpers;
using NurseNow.Models;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using NurseNow.Services;


namespace NurseNow.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase

    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly JwtService _jwtService;
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;

        public AuthController(
          UserManager<ApplicationUser> userManager,
          JwtService jwtService,
          ApplicationDbContext context,
          IEmailService emailService)
        {
            _userManager = userManager;
            _jwtService = jwtService;
            _context = context;
            _emailService = emailService;
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto model)
        {
            var userExists = await _userManager.FindByEmailAsync(model.Email);
            if (userExists != null)
                return BadRequest("User already exists");

           var user = new ApplicationUser
{
    UserName = model.Email,
    Email = model.Email,
    FullName = model.FullName,
    PhoneNumber = model.PhoneNumber,
    RoleType = model.Role,
    EmailConfirmed = true
};
            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            await _userManager.AddToRoleAsync(user, model.Role);

            if (model.Role == "Nurse")
            {
                var nurseProfile = new NurseProfile
                {
                    UserId = user.Id,
                    VerificationStatus = "Pending"
                };

                _context.NurseProfiles.Add(nurseProfile);

                _context.AdminActivityLogs.Add(new AdminActivityLog
                {
                    Title = "New nurse registration",
                    Description = $"{user.FullName} registered as a nurse.",
                    ActivityType = "Registration",
                    CreatedAt = DateTime.UtcNow
                });
            }
            else if (model.Role == "Patient")
            {
                var patientProfile = new PatientProfile
                {
                    UserId = user.Id
                };

                _context.PatientProfiles.Add(patientProfile);
            }

            await _context.SaveChangesAsync();


            return Ok("User registered successfully");
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
                return Unauthorized("Invalid credentials");

            if (user.AccountStatus == "Suspended")
                return Unauthorized("Your account is suspended");

            var validPassword = await _userManager.CheckPasswordAsync(user, model.Password);

            if (!validPassword)
                return Unauthorized("Invalid credentials");

            var roles = await _userManager.GetRolesAsync(user);


            var token = _jwtService.GenerateToken(user, roles);

            return Ok(new
            {
                token,
                user.FullName,
                user.Email,
                roles
            });
        }
[HttpPost("forgot-password")]
public async Task<IActionResult> ForgotPassword(ForgotPasswordDto model)
{
    if (model == null || string.IsNullOrWhiteSpace(model.Email))
        return BadRequest("Email is required.");

    var email = model.Email.Trim().ToLower();

    var user = await _userManager.FindByEmailAsync(email);

    if (user == null)
        return NotFound("This email is not registered.");

    return Ok(new
    {
        message = "Reset request received. The Home Nurse team will contact you soon."
    });
}
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
                return BadRequest("Invalid request.");

            string decodedToken;
            try
            {
                decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Token));
            }
            catch
            {
                return BadRequest("Invalid token.");
            }

            var result = await _userManager.ResetPasswordAsync(user, decodedToken, model.NewPassword);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new
            {
                message = "Password has been reset successfully."
            });
        }




    }



}