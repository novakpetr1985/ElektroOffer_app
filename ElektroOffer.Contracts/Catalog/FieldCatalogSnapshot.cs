using System.Text.Json;

namespace ElektroOffer.Contracts.Catalog;

public sealed class FieldCatalogSnapshot
{
    public const int CurrentSchemaVersion = 1;
    public int SchemaVersion { get; set; } = CurrentSchemaVersion;
    public Guid ExportId { get; set; } = Guid.NewGuid();
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public string CatalogVersion { get; set; } = string.Empty;
    public List<FieldCatalogOption> Options { get; set; } = [];
}

public sealed class FieldCatalogOption
{
    public string Code { get; set; } = string.Empty;
    public FieldCatalogOptionKind Kind { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
}

public enum FieldCatalogOptionKind
{
    Work,
    Material
}

public static class FieldCatalogSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public static string Serialize(FieldCatalogSnapshot catalog) => JsonSerializer.Serialize(catalog, Options);

    public static FieldCatalogSnapshot Deserialize(string json)
    {
        var catalog = JsonSerializer.Deserialize<FieldCatalogSnapshot>(json, Options)
            ?? throw new InvalidDataException("Catalog is empty or invalid.");
        if (catalog.SchemaVersion != FieldCatalogSnapshot.CurrentSchemaVersion)
            throw new InvalidDataException("Unsupported field catalog schema version.");
        if (catalog.ExportId == Guid.Empty || catalog.Options.Count == 0)
            throw new InvalidDataException("Catalog has no identity or options.");
        if (catalog.Options.Any(option => string.IsNullOrWhiteSpace(option.Code) || string.IsNullOrWhiteSpace(option.Name))
            || catalog.Options.Select(option => option.Code).Distinct(StringComparer.OrdinalIgnoreCase).Count() != catalog.Options.Count)
            throw new InvalidDataException("Catalog contains invalid or duplicate option codes.");
        return catalog;
    }
}
