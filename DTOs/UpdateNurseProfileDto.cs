namespace NurseNow.DTOs
{
    public class UpdateNurseProfileDto
    {
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? Location { get; set; }
        public string? Bio { get; set; }

        public string? LicenseNumber { get; set; }
        public string? Specialization { get; set; }
        public int? ExperienceYears { get; set; }

        public string? NationalId { get; set; }
    }
}