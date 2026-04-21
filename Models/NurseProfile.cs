namespace NurseNow.Models
{
    public class NurseProfile
    {
        public int NurseProfileId { get; set; }

        public string UserId { get; set; }

        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? Location { get; set; }
        public string? Bio { get; set; }

        public string? LicenseNumber { get; set; }
        public string? Specialization { get; set; }
        public int? ExperienceYears { get; set; }

        public string? NationalId { get; set; }
        public string? NationalIdImagePath { get; set; }

        public string? ProfileImagePath { get; set; }
        public string? CertificatePath { get; set; }

        public string VerificationStatus { get; set; } = "Pending";

        public ApplicationUser User { get; set; }

        public string? RejectionReason { get; set; }

    }
}