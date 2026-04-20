namespace NurseNow.DTOs.Admin
{
    public class AdminUserDetailsDto
    {
        public string Id { get; set; } = "";
        public string UserCode { get; set; } = "";
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Phone { get; set; } = "";
        public DateTime? JoinDate { get; set; }
        public string Status { get; set; } = "";
        public string Role { get; set; } = "";
        public string? Address { get; set; }
    }
}