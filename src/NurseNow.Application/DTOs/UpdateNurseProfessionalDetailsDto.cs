namespace NurseNow.DTOs
{
    public class UpdateNurseProfessionalDetailsDto
    {
        public string LicenseNumber { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;
        public int ExperienceYears { get; set; }
    }
}
