namespace NurseNow.Application.DTOs.Persistence;

public sealed class NotificationItem
{
    public int NotificationId { get; init; }
    public string Title { get; init; } = default!;
    public string Message { get; init; } = default!;
    public string Type { get; init; } = default!;
    public int? BookingId { get; init; }
    public bool IsRead { get; init; }
    public DateTime CreatedAt { get; init; }
    public string? targetScreen { get; init; } = default!;
}
