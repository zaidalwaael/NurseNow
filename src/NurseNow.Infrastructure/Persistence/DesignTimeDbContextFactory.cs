using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using NurseNow.Data;

namespace NurseNow.Infrastructure.Persistence;
public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var connection = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection") ?? "Server=(localdb)\\MSSQLLocalDB;Database=NurseNow;Trusted_Connection=True;TrustServerCertificate=True";
        return new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlServer(connection).Options);
    }
}
