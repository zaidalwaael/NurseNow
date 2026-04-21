namespace NurseNow.Models
{
    public class PatientProfile
    {
        public int PatientProfileId { get; set; }

        public string UserId { get; set; }

        public string? Gender { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public string? BloodType { get; set; }

        public string? Governorate { get; set; }

        public string? Area { get; set; }

        public string? Address { get; set; }

        public string? Conditions { get; set; }

        public string? Allergies { get; set; }

        public string? Notes { get; set; }

        public string? PhoneNumber { get; set; }

        public ApplicationUser User { get; set; }
    }
}