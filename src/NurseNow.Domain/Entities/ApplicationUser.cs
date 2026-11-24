
namespace NurseNow.Models
{
    public class ApplicationUser
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string? UserName { get; set; }
        public string? NormalizedUserName { get; set; }
        public string? Email { get; set; }
        public string? NormalizedEmail { get; set; }
        public bool EmailConfirmed { get; set; }
        public string? PasswordHash { get; set; }
        public string? SecurityStamp { get; set; } = Guid.NewGuid().ToString();
        public string? ConcurrencyStamp { get; set; } = Guid.NewGuid().ToString();
        public string? PhoneNumber { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }
        public bool LockoutEnabled { get; set; }
        public int AccessFailedCount { get; set; }

        public string FullName { get; set; } = string.Empty;
        public string RoleType { get; set; } = ""; // Patient / Nurse / Admin
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string AccountStatus { get; set; } = "Active";
        public string AdminRole { get; set; } = "";
        // SuperAdmin | VerificationAdmin | SupportAdmin
        public string AdminApprovalStatus { get; set; } = "Active";
        public string? RejectionReason { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public string? Location { get; set; }
    }
}
