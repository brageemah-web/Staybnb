namespace staybnb.Services;

public interface IFileService
{
    Task<string> SaveAsync(IFormFile file, string folder);  
    Task<string> SaveDocumentAsync(IFormFile file);         
    string GetDocumentPath(string fileName);
}

public class FileService : IFileService
{
    private static readonly string[] ImageExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
    private static readonly string[] DocumentExtensions = { ".jpg", ".jpeg", ".png", ".pdf" };
    private const long MaxBytes = 5 * 1024 * 1024;

    private readonly IWebHostEnvironment _env;
    public FileService(IWebHostEnvironment env) { 
        _env = env;
    }

    public static bool IsAllowedImage(IFormFile file) =>
        file.Length > 0 && file.Length <= MaxBytes &&
        ImageExtensions.Contains(Path.GetExtension(file.FileName).ToLowerInvariant());

    public static bool IsAllowedDocument(IFormFile file) =>
        file.Length > 0 && file.Length <= MaxBytes &&
        DocumentExtensions.Contains(Path.GetExtension(file.FileName).ToLowerInvariant());

    public async Task<string> SaveAsync(IFormFile file, string folder){
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var dir = Path.Combine(_env.WebRootPath, "uploads", folder);
        Directory.CreateDirectory(dir);

        var name = $"{Guid.NewGuid()}{ext}";
        using var stream = new FileStream(Path.Combine(dir, name), FileMode.Create);
        await file.CopyToAsync(stream);
        return $"/uploads/{folder}/{name}";
    }

    public async Task<string> SaveDocumentAsync(IFormFile file){
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var dir = Path.Combine(_env.ContentRootPath, "App_Data", "documents");
        Directory.CreateDirectory(dir);

        var name = $"{Guid.NewGuid()}{ext}";
        using var stream = new FileStream(Path.Combine(dir, name), FileMode.Create);
        await file.CopyToAsync(stream);
        return name;   
    }

    public string GetDocumentPath(string fileName) =>
        Path.Combine(_env.ContentRootPath, "App_Data", "documents", Path.GetFileName(fileName));


}