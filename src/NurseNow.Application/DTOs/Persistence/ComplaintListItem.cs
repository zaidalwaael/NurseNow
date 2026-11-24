namespace NurseNow.Application.DTOs.Persistence;

public sealed class ComplaintListItem
{
    public int complaintId { get; init; }
    public string submittedBy { get; init; } = default!;
    public string userRole { get; init; } = default!;
    public string category { get; init; } = default!;
    public string subject { get; init; } = default!;
    public DateTime date { get; init; }
    public string status { get; init; } = default!;
    public bool isUrgent { get; init; }
}
