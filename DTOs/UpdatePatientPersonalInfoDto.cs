namespace NurseNow.DTOs
{
    public class UpdatePatientPersonalInfoDto
    {
        public string? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? BloodType { get; set; }
    }
}