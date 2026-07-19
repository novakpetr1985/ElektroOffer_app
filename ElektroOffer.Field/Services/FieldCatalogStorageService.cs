using ElektroOffer.Contracts.Catalog;

namespace ElektroOffer.Field.Services;

public sealed class FieldCatalogStorageService
{
    private const long MaximumCatalogSize = 10L * 1024 * 1024;
    private readonly string _path = Path.Combine(FileSystem.AppDataDirectory, "field-catalog.json");

    public async Task<FieldCatalogSnapshot?> LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_path)) return null;
        try
        {
            return FieldCatalogSerializer.Deserialize(await File.ReadAllTextAsync(_path, cancellationToken));
        }
        catch (InvalidDataException)
        {
            return null;
        }
    }

    public async Task<FieldCatalogSnapshot> ImportAsync(FileResult file, CancellationToken cancellationToken = default)
    {
        if (!Path.GetExtension(file.FileName).Equals(".eofcatalog", StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("Vybraný soubor nemá příponu .eofcatalog.");
        await using var stream = await file.OpenReadAsync();
        if (stream.CanSeek && stream.Length > MaximumCatalogSize)
            throw new InvalidDataException("Katalog je větší než povolených 10 MB.");
        using var reader = new StreamReader(stream);
        var json = await reader.ReadToEndAsync(cancellationToken);
        if (System.Text.Encoding.UTF8.GetByteCount(json) > MaximumCatalogSize)
            throw new InvalidDataException("Katalog je větší než povolených 10 MB.");
        var catalog = FieldCatalogSerializer.Deserialize(json);
        await File.WriteAllTextAsync(_path + ".tmp", FieldCatalogSerializer.Serialize(catalog), cancellationToken);
        File.Move(_path + ".tmp", _path, overwrite: true);
        return catalog;
    }
}
