using System.IO.Compression;
using System.Security.Cryptography;

namespace ElektroOffer.Contracts.Measurements;

public sealed record MeasurementArchiveInspection(MeasurementPackage Package, IReadOnlyList<string> Entries);

public static class MeasurementArchiveService
{
    public const string ManifestName = "measurement.json";
    public const long MaximumManifestSize = 5L * 1024 * 1024;
    public const long MaximumArchiveContentSize = 250L * 1024 * 1024;
    public const int MaximumEntryCount = 500;

    public static async Task WriteAsync(
        MeasurementPackage package,
        string destinationPath,
        Func<AttachmentReference, CancellationToken, Task<Stream>> openAttachment,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(package);
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationPath);
        ArgumentNullException.ThrowIfNull(openAttachment);

        var validation = MeasurementPackageValidator.Validate(package);
        if (!validation.IsValid)
            throw InvalidPackage(validation);
        if (package.Attachments.Sum(attachment => attachment.Size) > MaximumArchiveContentSize)
            throw new InvalidDataException("Declared attachment content exceeds the 250 MB safety limit.");

        var directory = Path.GetDirectoryName(destinationPath);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        var temporaryPath = destinationPath + $".{Guid.NewGuid():N}.tmp";
        try
        {
            await using (var output = File.Create(temporaryPath))
            using (var archive = new ZipArchive(output, ZipArchiveMode.Create, leaveOpen: false))
            {
                var manifest = archive.CreateEntry(ManifestName, CompressionLevel.Optimal);
                await using (var manifestStream = manifest.Open())
                await using (var writer = new StreamWriter(manifestStream))
                    await writer.WriteAsync(MeasurementPackageSerializer.Serialize(package).AsMemory(), cancellationToken);

                foreach (var attachment in package.Attachments)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var entry = archive.CreateEntry(NormalizeEntryPath(attachment.RelativePath), CompressionLevel.Optimal);
                    await using var source = await openAttachment(attachment, cancellationToken);
                    await using var target = entry.Open();
                    var (size, hash) = await CopyAndHashAsync(source, target, cancellationToken);
                    EnsureAttachmentMatches(attachment, size, hash);
                }
            }

            File.Move(temporaryPath, destinationPath, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporaryPath))
                File.Delete(temporaryPath);
        }
    }

    public static async Task<MeasurementArchiveInspection> InspectAsync(
        string archivePath,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(archivePath);
        await using var input = File.OpenRead(archivePath);
        using var archive = new ZipArchive(input, ZipArchiveMode.Read);

        if (archive.Entries.Count > MaximumEntryCount)
            throw new InvalidDataException($"Archive contains more than {MaximumEntryCount} entries.");

        var entryNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        long declaredTotal = 0;
        foreach (var entry in archive.Entries)
        {
            var normalized = NormalizeEntryPath(entry.FullName);
            if (!entryNames.Add(normalized))
                throw new InvalidDataException($"Archive contains duplicate entry {normalized}.");
            declaredTotal = checked(declaredTotal + entry.Length);
            if (declaredTotal > MaximumArchiveContentSize)
                throw new InvalidDataException("Archive content exceeds the 250 MB safety limit.");
        }

        var manifest = archive.GetEntry(ManifestName)
            ?? throw new InvalidDataException($"Archive does not contain {ManifestName}.");
        if (manifest.Length > MaximumManifestSize)
            throw new InvalidDataException("Measurement manifest is too large.");

        string json;
        await using (var stream = manifest.Open())
        using (var reader = new StreamReader(stream))
            json = await reader.ReadToEndAsync(cancellationToken);

        var package = MeasurementPackageSerializer.Deserialize(json);
        var validation = MeasurementPackageValidator.Validate(package);
        if (!validation.IsValid)
            throw InvalidPackage(validation);

        var expectedEntries = package.Attachments
            .Select(attachment => NormalizeEntryPath(attachment.RelativePath))
            .Append(ManifestName)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var unexpected = entryNames.Where(name => !expectedEntries.Contains(name)).ToArray();
        if (unexpected.Length > 0)
            throw new InvalidDataException($"Archive contains unexpected entry {unexpected[0]}.");

        long actualAttachmentTotal = 0;
        foreach (var attachment in package.Attachments)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var entryName = NormalizeEntryPath(attachment.RelativePath);
            var entry = archive.GetEntry(entryName)
                ?? throw new InvalidDataException($"Archive is missing attachment {entryName}.");
            await using var source = entry.Open();
            var (size, hash) = await CopyAndHashAsync(source, Stream.Null, cancellationToken);
            actualAttachmentTotal = checked(actualAttachmentTotal + size);
            if (actualAttachmentTotal > MaximumArchiveContentSize)
                throw new InvalidDataException("Attachment content exceeds the 250 MB safety limit.");
            EnsureAttachmentMatches(attachment, size, hash);
        }

        return new MeasurementArchiveInspection(package, entryNames.OrderBy(name => name).ToArray());
    }

    public static async Task<IReadOnlyList<string>> ExtractAttachmentsAsync(
        string archivePath,
        string destinationRoot,
        IEnumerable<Guid>? attachmentIds = null,
        CancellationToken cancellationToken = default)
    {
        var inspection = await InspectAsync(archivePath, cancellationToken);
        var selectedIds = attachmentIds?.ToHashSet();
        var root = Path.GetFullPath(destinationRoot);
        Directory.CreateDirectory(root);
        var extracted = new List<string>();

        await using var input = File.OpenRead(archivePath);
        using var archive = new ZipArchive(input, ZipArchiveMode.Read);
        foreach (var attachment in inspection.Package.Attachments.Where(item => selectedIds == null || selectedIds.Contains(item.Id)))
        {
            var entryName = NormalizeEntryPath(attachment.RelativePath);
            var destination = Path.GetFullPath(Path.Combine(root, entryName.Replace('/', Path.DirectorySeparatorChar)));
            if (!destination.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException($"Attachment path escapes destination: {entryName}.");
            Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
            var temporary = destination + ".tmp";
            try
            {
                var entry = archive.GetEntry(entryName)!;
                await using (var source = entry.Open())
                await using (var target = File.Create(temporary))
                {
                    var (size, hash) = await CopyAndHashAsync(source, target, cancellationToken);
                    EnsureAttachmentMatches(attachment, size, hash);
                }
                File.Move(temporary, destination, overwrite: true);
                extracted.Add(destination);
            }
            finally
            {
                if (File.Exists(temporary))
                    File.Delete(temporary);
            }
        }

        return extracted;
    }

    public static async Task<MeasurementPackage> ReadManifestAsync(string archivePath, CancellationToken cancellationToken = default) =>
        (await InspectAsync(archivePath, cancellationToken)).Package;

    private static string NormalizeEntryPath(string path)
    {
        var normalized = path.Replace('\\', '/').TrimStart('/');
        if (string.IsNullOrWhiteSpace(normalized)
            || Path.IsPathRooted(path)
            || normalized.Split('/').Any(part => part is "" or "." or ".."))
            throw new InvalidDataException($"Unsafe archive path: {path}.");
        return normalized;
    }

    private static async Task<(long Size, string Hash)> CopyAndHashAsync(Stream source, Stream target, CancellationToken cancellationToken)
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        var buffer = new byte[81920];
        long size = 0;
        int read;
        while ((read = await source.ReadAsync(buffer, cancellationToken)) > 0)
        {
            size += read;
            if (size > MeasurementPackageValidator.MaximumAttachmentSize)
                throw new InvalidDataException("Attachment exceeds the maximum size.");
            hash.AppendData(buffer, 0, read);
            await target.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
        }
        return (size, Convert.ToHexString(hash.GetHashAndReset()));
    }

    private static void EnsureAttachmentMatches(AttachmentReference attachment, long size, string hash)
    {
        if (size != attachment.Size || !hash.Equals(attachment.Sha256, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException($"Attachment {attachment.RelativePath} does not match its declared size or checksum.");
    }

    private static InvalidDataException InvalidPackage(MeasurementValidationResult validation) =>
        new(string.Join(Environment.NewLine, validation.Issues.Select(issue => $"{issue.Path}: {issue.Message}")));
}
