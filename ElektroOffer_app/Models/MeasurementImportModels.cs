using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ElektroOffer_app.Models;

public enum MeasurementImportRowKind
{
    Unknown,
    Work,
    Material
}

public sealed class MeasurementImportPreview
{
    public Guid ExportId { get; init; }
    public string SourcePath { get; init; } = string.Empty;
    public string ProjectName { get; init; } = string.Empty;
    public string CustomerName { get; init; } = string.Empty;
    public string SiteAddress { get; init; } = string.Empty;
    public string TechnicianName { get; init; } = string.Empty;
    public List<MeasurementImportPreviewItem> Items { get; init; } = [];
    public List<MeasurementImportAttachment> Attachments { get; init; } = [];
    public int ResolvedCount => Items.Count(item => item.CanImport);
    public int UnresolvedCount => Items.Count(item => !item.CanImport);
}

public sealed class MeasurementImportPreviewItem : INotifyPropertyChanged
{
    private bool _isSelected;

    public Guid SourceItemId { get; init; }
    public string AreaName { get; init; } = string.Empty;
    public MeasurementImportRowKind Kind { get; init; }
    public string SourceDescription { get; init; } = string.Empty;
    public string SuggestedMapping { get; init; } = string.Empty;
    public double Quantity { get; init; }
    public string Unit { get; init; } = string.Empty;
    public decimal Confidence { get; init; }
    public bool CanImport { get; init; }
    public string Warning { get; init; } = string.Empty;

    public string? WorkTask { get; init; }
    public string? WorkSpecification { get; init; }
    public string? BaseMaterial { get; init; }
    public string? WorkPosition { get; init; }
    public string? MaterialCategory { get; init; }
    public string? MaterialName { get; init; }
    public string? Supplier { get; init; }
    public string? Offer { get; init; }

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            var next = CanImport && value;
            if (_isSelected == next) return;
            _isSelected = next;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}

public sealed class MeasurementImportAttachment : INotifyPropertyChanged
{
    private bool _isSelected = true;
    public Guid Id { get; init; }
    public Guid? SourceItemId { get; init; }
    public string AreaName { get; init; } = string.Empty;
    public string ItemName { get; init; } = string.Empty;
    public string FileName { get; init; } = string.Empty;
    public string RelativePath { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public long Size { get; init; }
    public string Sha256 { get; init; } = string.Empty;

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected == value) return;
            _isSelected = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}

public sealed class ImportedMeasurementData
{
    public Guid ExportId { get; set; }
    public string SourceProjectName { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string SiteAddress { get; set; } = string.Empty;
    public string TechnicianName { get; set; } = string.Empty;
    public string SourceFileName { get; set; } = string.Empty;
    public string StoredPackageRelativePath { get; set; } = string.Empty;
    public DateTime ImportedAtUtc { get; set; } = DateTime.UtcNow;
    public List<ImportedMeasurementMapping> Mappings { get; set; } = [];
    public List<ProjectAttachmentData> Attachments { get; set; } = [];
}

public sealed class ImportedMeasurementMapping
{
    public Guid SourceItemId { get; set; }
    public string AreaName { get; set; } = string.Empty;
    public string Kind { get; set; } = string.Empty;
    public string SourceDescription { get; set; } = string.Empty;
    public string TargetDescription { get; set; } = string.Empty;
    public double Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal Confidence { get; set; }
}

public sealed class ProjectAttachmentData
{
    public Guid Id { get; set; }
    public Guid ExportId { get; set; }
    public Guid? SourceItemId { get; set; }
    public string AreaName { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string RelativePath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long Size { get; set; }
    public string Sha256 { get; set; } = string.Empty;
}
