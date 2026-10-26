namespace NurseNow.Application.Common;
public static class UploadPolicy
{
    public const long MaxBytes = 5 * 1024 * 1024;
    public static bool IsAllowed(string name, long length, bool pdf)
    {
        if (length <= 0 || length > MaxBytes)
            return false;
        var extension = Path.GetExtension(name).ToLowerInvariant();
        return pdf ? extension == ".pdf" : extension is ".jpg" or ".jpeg" or ".png";
    }
}
