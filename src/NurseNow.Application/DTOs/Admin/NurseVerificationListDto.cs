namespace NurseNow.DTOs.Admin
{
    public class NurseVerificationListDto
    {
        public string Id { get; set; } = string.Empty;
        public string NurseId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public DateTime? RegistrationDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
