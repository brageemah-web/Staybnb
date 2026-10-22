namespace staybnb.Services;

public interface IFileService
{
    Task<string> SaveAsync(IFormFile file, string folder);
}

public class FileService : IFileService
{
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
    private const long MaxBytes = 5 * 1024 * 1024; // 5 MB

    private readonly IWebHostEnvironment _env;
    public FileService(IWebHostEnvironment env) => _env = env;

    public static bool IsAllowedImage(IFormFile file) =>
        file.Length > 0 && file.Length <= MaxBytes &&
        AllowedExtensions.Contains(Path.GetExtension(file.FileName).ToLowerInvariant());

    public async Task<string> SaveAsync(IFormFile file, string folder)
    {
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var dir = Path.Combine(_env.WebRootPath, "uploads", folder);
        Directory.CreateDirectory(dir);

        var name = $"{Guid.NewGuid()}{ext}";   // never trust the original file name
        using var stream = new FileStream(Path.Combine(dir, name), FileMode.Create);
        await file.CopyToAsync(stream);

        return $"/uploads/{folder}/{name}";
    }
}