namespace Convos.API.Core.Services;

public class FileService
{
    private readonly string _uploadedDirPath;

    public FileService(IHostEnvironment environment)
    {
        _uploadedDirPath = Path.Combine(environment.EnvironmentName, "uploadedImages");
        if (!Directory.Exists(_uploadedDirPath)) Directory.CreateDirectory(_uploadedDirPath);
    }

    public async Task<string> UploadFile(IFormFile file)
    {
        var fileName = DateTime.Now.Ticks + "_" + Guid.NewGuid() + Path.GetExtension(file.FileName);

        await using var stream = new FileStream(Path.Combine(_uploadedDirPath, fileName), FileMode.Create);
        await file.CopyToAsync(stream);

        return fileName;
    }

    public async Task<byte[]?> GetFile(string fileName)
    {
        var path = Path.Combine(_uploadedDirPath, fileName);
        if (!File.Exists(path)) return null;

        return await File.ReadAllBytesAsync(path);
    }

    public void DeleteFile(string fileName)
    {
        var path = Path.Combine(_uploadedDirPath, fileName);
        if (!File.Exists(path)) return;

        File.Delete(path);
    }

    public static bool IsValidImage(IFormFile file)
    {
        if (file.ContentType.ToLower() != "image/jpg" &&
            file.ContentType.ToLower() != "image/jpeg" &&
            file.ContentType.ToLower() != "image/pjpeg" &&
            file.ContentType.ToLower() != "image/x-png" &&
            file.ContentType.ToLower() != "image/png") return false;

        return Path.GetExtension(file.FileName).ToLower() is ".jpg" or ".png" or ".jpeg";
    }
}