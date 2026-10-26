using Microsoft.Extensions.Configuration;
using NurseNow.Application.Abstractions;
using NurseNow.Application.Common;

namespace NurseNow.Infrastructure.Storage;
public sealed class LocalFileStorage(IConfiguration configuration) : IFileStorage
{
    public async Task<string> SaveAsync(UploadFile file, bool pdf, CancellationToken cancellationToken)
    {
        if (!UploadPolicy.IsAllowed(file.FileName, file.Length, pdf))
            throw new ArgumentException("Invalid upload type or size.");
        var root = Path.GetFullPath(configuration["Storage:Root"] ?? "App_Data/uploads");
        Directory.CreateDirectory(root);
        var name = Guid.NewGuid().ToString("N") + Path.GetExtension(file.FileName).ToLowerInvariant();
        var path = Path.Combine(root, name);
        try
        {
            await using var output = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, true);
            var buffer = new byte[81920];
            long total = 0;
            int read;
            while ((read = await file.Content.ReadAsync(buffer, cancellationToken)) > 0)
            {
                total += read;
                if (total > UploadPolicy.MaxBytes)
                    throw new ArgumentException("Upload is too large.");
                await output.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
            }

            if (total != file.Length)
                throw new ArgumentException("Upload length does not match content.");
        }
        catch
        {
            File.Delete(path);
            throw;
        }

        return "api/files/" + name;
    }
}
