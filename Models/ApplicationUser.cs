using Microsoft.AspNetCore.Identity;
namespace NurseNow.Models

{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
        public string RoleType { get; set; } // Patient / Nurse / Admin
    }
}
