namespace NurseNow.Application.Abstractions;
public sealed record UploadFile(string FileName, long Length, Stream Content);
public interface IFileStorage
{
    Task<string> SaveAsync(UploadFile file, bool pdf, CancellationToken cancellationToken);
}
