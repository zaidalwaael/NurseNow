namespace NurseNow.Application.DTOs.Persistence;

public sealed class NurseDocumentItem
{
    public int id { get; init; }
    public string documentName { get; init; } = default!;
    public string fileUrl { get; init; } = default!;
}
