namespace ElektroOffer.Contracts.Measurements;

public sealed class MeasurementPackage
{
    public const int CurrentSchemaVersion = 1;

    public int SchemaVersion { get; set; } = CurrentSchemaVersion;
    public Guid ExportId { get; set; } = Guid.NewGuid();
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public string SourceAppVersion { get; set; } = string.Empty;
    public string CatalogVersion { get; set; } = string.Empty;
    public MeasurementProject Project { get; set; } = new();
    public List<AttachmentReference> Attachments { get; set; } = [];
}

public sealed class MeasurementProject
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string SiteAddress { get; set; } = string.Empty;
    public string TechnicianName { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
    public MeasurementProjectStatus Status { get; set; } = MeasurementProjectStatus.Draft;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    public List<MeasurementArea> Areas { get; set; } = [];
}

public enum MeasurementProjectStatus
{
    Draft,
    ReadyForExport,
    Exported
}

public sealed class MeasurementArea
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    public List<MeasurementCircuit> Circuits { get; set; } = [];
    public List<MeasurementItem> Items { get; set; } = [];
}

public sealed class MeasurementCircuit
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string DistributionBoard { get; set; } = string.Empty;
    public string Protection { get; set; } = string.Empty;
    public string Phase { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
}

public sealed class MeasurementItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public MeasurementKind Kind { get; set; }
    public string CatalogCode { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal ReservePercent { get; set; }
    public string WorkPositionCode { get; set; } = string.Empty;
    public string BaseMaterialCode { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
    public string Origin { get; set; } = "field";
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    public List<WorkHint> WorkHints { get; set; } = [];
    public List<MaterialRequirement> MaterialRequirements { get; set; } = [];
}

public sealed class WorkHint
{
    public string CatalogCode { get; set; } = string.Empty;
    public string WorkPositionCode { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public string RuleId { get; set; } = string.Empty;
    public decimal Confidence { get; set; }
}

public sealed class MaterialRequirement
{
    public string MaterialCode { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Specification { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal ReservePercent { get; set; }
    public string PreferredManufacturer { get; set; } = string.Empty;
}

public enum MeasurementKind
{
    Custom,
    CableRoute,
    Socket,
    Switch,
    Light,
    DistributionBoard
}

public sealed class AttachmentReference
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? AreaId { get; set; }
    public Guid? ItemId { get; set; }
    public string RelativePath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long Size { get; set; }
    public string Sha256 { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
}
