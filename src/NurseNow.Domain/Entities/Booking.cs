using NurseNow.Models;

public class Booking
{
    public int BookingId { get; set; }
    public string PatientId { get; set; } = string.Empty;
    public string NurseId { get; set; } = string.Empty;
    public int ServiceId { get; set; }
    public DateTime BookingDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string ServiceAddress { get; set; } = string.Empty;
    public string? AdditionalNotes { get; set; }
    public string Status { get; set; } = "Pending";
    public bool IsReviewSubmitted { get; set; } = false;
    public bool IsReviewDismissed { get; set; } = false;
    public DateTime? ReviewRemindLaterAt { get; set; }
    public ApplicationUser Patient { get; set; } = null !;
    public ApplicationUser Nurse { get; set; } = null !;
    public Service Service { get; set; } = null !;
    public Payment? Payment { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
