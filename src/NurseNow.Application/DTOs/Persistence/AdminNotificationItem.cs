namespace NurseNow.Application.DTOs.Persistence;

public sealed class AdminNotificationItem
{
    public int notificationId { get; init; }
    public string title { get; init; } = default!;
    public string message { get; init; } = default!;
    public string type { get; init; } = default!;
    public bool isRead { get; init; }
    public DateTime createdAt { get; init; }
}
