using System.Security.Cryptography;
using ElektroOffer.Contracts.Measurements;

namespace ElektroOffer.Field.Services;

public sealed class FieldStorageService
{
    private const string AutosaveFileName = "measurement-autosave.json";
    private readonly string _rootDirectory;
    private readonly string _attachmentDirectory;

    public FieldStorageService()
    {
        _rootDirectory = Path.Combine(FileSystem.AppDataDirectory, "measurements");
        _attachmentDirectory = Path.Combine(_rootDirectory, "attachments");
        Directory.CreateDirectory(_attachmentDirectory);
    }

    public async Task<MeasurementPackage?> LoadAsync(CancellationToken cancellationToken = default)
    {
        var path = Path.Combine(_rootDirectory, AutosaveFileName);
        if (!File.Exists(path))
            return null;

        return MeasurementPackageSerializer.Deserialize(await File.ReadAllTextAsync(path, cancellationToken));
    }

    public async Task SaveAsync(MeasurementPackage package, CancellationToken cancellationToken = default)
    {
        package.Project.UpdatedAtUtc = DateTime.UtcNow;
        var path = Path.Combine(_rootDirectory, AutosaveFileName);
        var temporaryPath = path + ".tmp";
        await File.WriteAllTextAsync(temporaryPath, MeasurementPackageSerializer.Serialize(package), cancellationToken);
        File.Move(temporaryPath, path, overwrite: true);
    }

    public async Task<AttachmentReference> AddPhotoAsync(
        FileResult photo,
        Guid? areaId,
        Guid? itemId,
        CancellationToken cancellationToken = default)
    {
        var id = Guid.NewGuid();
        var extension = Path.GetExtension(photo.FileName).ToLowerInvariant();
        if (extension is not (".jpg" or ".jpeg" or ".png" or ".heic"))
            extension = ".jpg";

        var fileName = $"{id:N}{extension}";
        var destination = Path.Combine(_attachmentDirectory, fileName);
        await using (var source = await photo.OpenReadAsync())
        await using (var target = File.Create(destination))
            await source.CopyToAsync(target, cancellationToken);

        await using var hashStream = File.OpenRead(destination);
        var hash = Convert.ToHexString(await SHA256.HashDataAsync(hashStream, cancellationToken)).ToLowerInvariant();
        return new AttachmentReference
        {
            Id = id,
            AreaId = areaId,
            ItemId = itemId,
            RelativePath = $"attachments/{fileName}",
            ContentType = photo.ContentType ?? GetContentType(extension),
            Size = new FileInfo(destination).Length,
            Sha256 = hash,
            Note = photo.FileName
        };
    }

    public async Task<string> ExportAsync(MeasurementPackage package, CancellationToken cancellationToken = default)
    {
        package.ExportId = Guid.NewGuid();
        package.CreatedAtUtc = DateTime.UtcNow;
        package.Project.Status = MeasurementProjectStatus.ReadyForExport;
        await SaveAsync(package, cancellationToken);

        var safeName = string.Concat(package.Project.Name.Select(character => Path.GetInvalidFileNameChars().Contains(character) ? '_' : character));
        if (string.IsNullOrWhiteSpace(safeName))
            safeName = "mereni";
        var path = Path.Combine(FileSystem.CacheDirectory, $"{safeName}-{DateTime.Now:yyyyMMdd-HHmm}.eofmeasure");
        await MeasurementArchiveService.WriteAsync(package, path, OpenAttachmentAsync, cancellationToken);
        package.Project.Status = MeasurementProjectStatus.Exported;
        await SaveAsync(package, cancellationToken);
        return path;
    }

    private Task<Stream> OpenAttachmentAsync(AttachmentReference attachment, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var fullPath = Path.GetFullPath(Path.Combine(_rootDirectory, attachment.RelativePath.Replace('/', Path.DirectorySeparatorChar)));
        if (!fullPath.StartsWith(Path.GetFullPath(_attachmentDirectory) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("Attachment is outside the application storage.");
        return Task.FromResult<Stream>(File.OpenRead(fullPath));
    }

    private static string GetContentType(string extension) => extension switch
    {
        ".png" => "image/png",
        ".heic" => "image/heic",
        _ => "image/jpeg"
    };
}
