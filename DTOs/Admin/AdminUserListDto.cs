namespace NurseNow.DTOs.Admin
{
    public class AdminUserListDto
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Role { get; set; }
        public string Status { get; set; } // Active / Suspended
        public DateTime CreatedAt { get; set; }
    }
}