using Microsoft.AspNetCore.Identity;
namespace NurseNow.Models

{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
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