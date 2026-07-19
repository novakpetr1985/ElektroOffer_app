using System.Globalization;
using System.IO;
using System.Text;
using ElektroOffer.Contracts.Measurements;
using ElektroOffer.Contracts.Catalog;
using ElektroOffer_app.Data;
using ElektroOffer_app.Models;
using Microsoft.EntityFrameworkCore;

namespace ElektroOffer_app.Services;

public sealed class MeasurementImportService
{
    private readonly AppDbContext _db;

    public MeasurementImportService(AppDbContext db) => _db = db;

    public FieldCatalogSnapshot CreateFieldCatalog()
    {
        var options = _db.Tasks.AsNoTracking().OrderBy(task => task.Name).Select(task => new FieldCatalogOption
        {
            Code = $"WORK-{task.Id}",
            Kind = FieldCatalogOptionKind.Work,
            Name = task.Name,
            Category = "Práce",
            Unit = _db.TaskSpecifications.Where(link => link.TaskId == task.Id).Select(link => link.Specification!.Unit).FirstOrDefault() ?? string.Empty
        }).ToList();
        options.AddRange(_db.Materials.AsNoTracking().Include(material => material.Category).OrderBy(material => material.Name).Select(material => new FieldCatalogOption
        {
            Code = $"MAT-{material.Id}",
            Kind = FieldCatalogOptionKind.Material,
            Name = material.Name,
            Category = material.Category != null ? material.Category.Name : "Materiál",
            Unit = material.Unit
        }));
        return new FieldCatalogSnapshot
        {
            CatalogVersion = $"{DateTime.UtcNow:yyyyMMdd-HHmmss}",
            Options = options
        };
    }

    public FieldCatalogSnapshot ExportFieldCatalog(string path)
    {
        var catalog = CreateFieldCatalog();
        File.WriteAllText(path, FieldCatalogSerializer.Serialize(catalog));
        return catalog;
    }

    public async Task<MeasurementImportPreview> PrepareAsync(
        string sourcePath,
        IEnumerable<Guid> alreadyImportedExportIds,
        CancellationToken cancellationToken = default)
    {
        var inspection = await MeasurementArchiveService.InspectAsync(sourcePath, cancellationToken);
        var package = inspection.Package;
        if (alreadyImportedExportIds.Contains(package.ExportId))
            throw new InvalidOperationException("Tento export již byl do projektu importován. Opakovaný import je zablokován.");

        var preview = new MeasurementImportPreview
        {
            ExportId = package.ExportId,
            SourcePath = sourcePath,
            ProjectName = package.Project.Name,
            CustomerName = package.Project.CustomerName,
            SiteAddress = package.Project.SiteAddress,
            TechnicianName = package.Project.TechnicianName
        };

        foreach (var area in package.Project.Areas)
        foreach (var item in area.Items)
        {
            var workHints = item.WorkHints.Count > 0
                ? item.WorkHints
                : item.CatalogCode.StartsWith("MAT-", StringComparison.OrdinalIgnoreCase) ? [] : [InferWork(item)];
            foreach (var hint in workHints)
                preview.Items.Add(MapWork(area.Name, item, hint));

            foreach (var requirement in item.MaterialRequirements)
                preview.Items.Add(MapMaterial(area.Name, item, requirement));
        }

        var areaNames = package.Project.Areas.ToDictionary(area => area.Id, area => area.Name);
        var itemNames = package.Project.Areas.SelectMany(area => area.Items).ToDictionary(item => item.Id, item => item.DisplayName);
        foreach (var attachment in package.Attachments)
        {
            preview.Attachments.Add(new MeasurementImportAttachment
            {
                Id = attachment.Id,
                SourceItemId = attachment.ItemId,
                AreaName = attachment.AreaId.HasValue && areaNames.TryGetValue(attachment.AreaId.Value, out var areaName) ? areaName : string.Empty,
                ItemName = attachment.ItemId.HasValue && itemNames.TryGetValue(attachment.ItemId.Value, out var itemName) ? itemName : string.Empty,
                FileName = string.IsNullOrWhiteSpace(attachment.Note) ? Path.GetFileName(attachment.RelativePath) : attachment.Note,
                RelativePath = attachment.RelativePath,
                ContentType = attachment.ContentType,
                Size = attachment.Size,
                Sha256 = attachment.Sha256
            });
        }

        return preview;
    }

    public async Task<ImportedMeasurementData> StoreApprovedImportAsync(
        MeasurementImportPreview preview,
        string projectPath,
        CancellationToken cancellationToken = default)
    {
        var assetsRoot = GetProjectAssetsDirectory(projectPath);
        var importRelativeRoot = Path.Combine("imports", preview.ExportId.ToString("N"));
        var importRoot = Path.Combine(assetsRoot, importRelativeRoot);
        Directory.CreateDirectory(importRoot);

        var packageRelativePath = Path.Combine(importRelativeRoot, "source.eofmeasure");
        var packageDestination = Path.Combine(assetsRoot, packageRelativePath);
        var selectedAttachments = preview.Attachments.Where(attachment => attachment.IsSelected).ToArray();
        await MeasurementArchiveService.ExtractAttachmentsAsync(
            preview.SourcePath,
            importRoot,
            selectedAttachments.Select(attachment => attachment.Id),
            cancellationToken);
        File.Copy(preview.SourcePath, packageDestination, overwrite: true);

        return new ImportedMeasurementData
        {
            ExportId = preview.ExportId,
            SourceProjectName = preview.ProjectName,
            CustomerName = preview.CustomerName,
            SiteAddress = preview.SiteAddress,
            TechnicianName = preview.TechnicianName,
            SourceFileName = Path.GetFileName(preview.SourcePath),
            StoredPackageRelativePath = packageRelativePath,
            Mappings = preview.Items.Where(item => item.IsSelected).Select(item => new ImportedMeasurementMapping
            {
                SourceItemId = item.SourceItemId,
                AreaName = item.AreaName,
                Kind = item.Kind.ToString(),
                SourceDescription = item.SourceDescription,
                TargetDescription = item.SuggestedMapping,
                Quantity = item.Quantity,
                Unit = item.Unit,
                Confidence = item.Confidence
            }).ToList(),
            Attachments = selectedAttachments.Select(attachment => new ProjectAttachmentData
            {
                Id = attachment.Id,
                ExportId = preview.ExportId,
                SourceItemId = attachment.SourceItemId,
                AreaName = attachment.AreaName,
                ItemName = attachment.ItemName,
                FileName = attachment.FileName,
                RelativePath = Path.Combine(importRelativeRoot, attachment.RelativePath),
                ContentType = attachment.ContentType,
                Size = attachment.Size,
                Sha256 = attachment.Sha256
            }).ToList()
        };
    }

    public static string GetProjectAssetsDirectory(string projectPath) =>
        Path.Combine(Path.GetDirectoryName(projectPath)!, Path.GetFileNameWithoutExtension(projectPath) + ".assets");

    private MeasurementImportPreviewItem MapWork(string areaName, MeasurementItem item, WorkHint hint)
    {
        var tasks = _db.Tasks.AsNoTracking().ToList();
        var desired = WorkAlias(item.Kind, hint.DisplayName + " " + hint.WorkPositionCode + " " + item.DisplayName);
        var codedTask = ParseCatalogId(hint.CatalogCode, "WORK-") ?? ParseCatalogId(item.CatalogCode, "WORK-");
        var task = codedTask.HasValue ? tasks.FirstOrDefault(candidate => candidate.Id == codedTask.Value) : null;
        var taskScore = task != null ? 1d : 0d;
        if (task == null)
            (task, taskScore) = Best(tasks, candidate => candidate.Name, desired);
        if (task == null)
            return Unresolved(item, areaName, MeasurementImportRowKind.Work, hint.DisplayName, hint.Quantity, hint.Unit, "Pracovní úkon nebyl nalezen v aktuálním katalogu.");

        var specifications = _db.TaskSpecifications.AsNoTracking()
            .Where(link => link.TaskId == task.Id)
            .Select(link => link.Specification!)
            .ToList();
        var specificationAlias = SpecificationAlias(item.Kind, task.Name);
        var (specification, specificationScore) = Best(specifications, value => value.Name, specificationAlias);
        specification ??= specifications.FirstOrDefault(value => string.Equals(value.Unit, hint.Unit, StringComparison.OrdinalIgnoreCase));
        if (specification == null)
            return Unresolved(item, areaName, MeasurementImportRowKind.Work, hint.DisplayName, hint.Quantity, hint.Unit, "Pro pracovní úkon nebyla nalezena kompatibilní specifikace.");

        var baseMaterial = _db.BaseMaterials.AsNoTracking().OrderBy(value => value.Id).FirstOrDefault(value => value.Name == "-")
            ?? _db.BaseMaterials.AsNoTracking().OrderBy(value => value.Id).FirstOrDefault();
        var positions = _db.Positions.AsNoTracking().ToList();
        var (position, positionScore) = Best(positions, value => value.Name, "stena");
        position ??= positions.FirstOrDefault();
        if (baseMaterial == null || position == null)
            return Unresolved(item, areaName, MeasurementImportRowKind.Work, hint.DisplayName, hint.Quantity, hint.Unit, "Katalog neobsahuje podklad nebo umístění práce.");

        var confidence = Math.Clamp(Math.Min(hint.Confidence <= 0 ? 0.7m : hint.Confidence, (decimal)Math.Min(taskScore, Math.Max(specificationScore, 0.7))), 0, 1);
        return new MeasurementImportPreviewItem
        {
            SourceItemId = item.Id,
            AreaName = areaName,
            Kind = MeasurementImportRowKind.Work,
            SourceDescription = string.IsNullOrWhiteSpace(hint.DisplayName) ? item.DisplayName : hint.DisplayName,
            SuggestedMapping = $"{task.Name} → {specification.Name} → {baseMaterial.Name} → {position.Name}",
            Quantity = (double)(hint.Quantity > 0 ? hint.Quantity : item.Quantity),
            Unit = specification.Unit ?? hint.Unit,
            Confidence = confidence,
            CanImport = true,
            IsSelected = true,
            Warning = confidence < 0.75m ? "Mapování má nižší jistotu – před importem jej zkontrolujte." : string.Empty,
            WorkTask = task.Name,
            WorkSpecification = specification.Name,
            BaseMaterial = baseMaterial.Name,
            WorkPosition = position.Name
        };
    }

    private MeasurementImportPreviewItem MapMaterial(string areaName, MeasurementItem item, MaterialRequirement requirement)
    {
        var materials = _db.Materials.AsNoTracking().Include(material => material.Category).ToList();
        var desired = $"{requirement.MaterialCode} {requirement.Specification} {item.DisplayName}";
        var codedMaterial = ParseCatalogId(requirement.MaterialCode, "MAT-") ?? ParseCatalogId(item.CatalogCode, "MAT-");
        var material = codedMaterial.HasValue ? materials.FirstOrDefault(candidate => candidate.Id == codedMaterial.Value) : null;
        var score = material != null ? 1d : 0d;
        if (material == null)
            (material, score) = Best(materials, value => value.Name, desired);
        if (material == null || score < 0.28)
            return Unresolved(item, areaName, MeasurementImportRowKind.Material, requirement.Specification, requirement.Quantity, requirement.Unit, "Materiál nebyl bezpečně spárován s aktuálním katalogem.");

        var offer = _db.MaterialPrices.AsNoTracking()
            .Include(price => price.Supplier)
            .Where(price => price.MaterialId == material.Id)
            .OrderBy(price => price.Price)
            .FirstOrDefault();
        if (material.Category == null || offer == null)
            return Unresolved(item, areaName, MeasurementImportRowKind.Material, requirement.Specification, requirement.Quantity, requirement.Unit, "Materiál nemá kategorii nebo platnou nabídku dodavatele.");

        var quantity = requirement.Quantity * (1 + requirement.ReservePercent / 100m);
        return new MeasurementImportPreviewItem
        {
            SourceItemId = item.Id,
            AreaName = areaName,
            Kind = MeasurementImportRowKind.Material,
            SourceDescription = requirement.Specification,
            SuggestedMapping = $"{material.Category.Name} → {material.Name} → {offer.Supplier.Name} → {offer.SupplierName}",
            Quantity = (double)quantity,
            Unit = offer.Unit,
            Confidence = (decimal)Math.Clamp(score, 0, 1),
            CanImport = true,
            IsSelected = true,
            Warning = score < 0.65 ? "Mapování má nižší jistotu – před importem jej zkontrolujte." : string.Empty,
            MaterialCategory = material.Category.Name,
            MaterialName = material.Name,
            Supplier = offer.Supplier.Name,
            Offer = offer.SupplierName
        };
    }

    private static MeasurementImportPreviewItem Unresolved(MeasurementItem item, string area, MeasurementImportRowKind kind, string description, decimal quantity, string unit, string warning) => new()
    {
        SourceItemId = item.Id,
        AreaName = area,
        Kind = kind,
        SourceDescription = string.IsNullOrWhiteSpace(description) ? item.DisplayName : description,
        SuggestedMapping = "Nevyřešeno",
        Quantity = (double)quantity,
        Unit = unit,
        Confidence = 0,
        CanImport = false,
        IsSelected = false,
        Warning = warning
    };

    private static WorkHint InferWork(MeasurementItem item) => new()
    {
        DisplayName = item.DisplayName,
        Quantity = item.Quantity,
        Unit = item.Unit,
        RuleId = $"inferred-{item.Kind.ToString().ToLowerInvariant()}-v1",
        Confidence = 0.6m
    };

    private static string WorkAlias(MeasurementKind kind, string source)
    {
        var normalized = Normalize(source);
        if (normalized.Contains("draz")) return "drazkovani";
        return kind switch
        {
            MeasurementKind.CableRoute => "kabelaz",
            MeasurementKind.Socket or MeasurementKind.Switch or MeasurementKind.Light or MeasurementKind.DistributionBoard => "osazeni",
            _ => source
        };
    }

    private static string SpecificationAlias(MeasurementKind kind, string taskName)
    {
        var task = Normalize(taskName);
        if (kind == MeasurementKind.CableRoute)
            return task.Contains("draz") ? "spara" : "vlozeni kabelu";
        if (kind == MeasurementKind.DistributionBoard) return "rozvadec";
        return "elektricka krabice";
    }

    private static (T? Item, double Score) Best<T>(IEnumerable<T> candidates, Func<T, string> selector, string desired) where T : class
    {
        var normalizedDesired = Normalize(desired);
        T? best = null;
        var bestScore = 0d;
        foreach (var candidate in candidates)
        {
            var normalizedCandidate = Normalize(selector(candidate));
            var score = Similarity(normalizedCandidate, normalizedDesired);
            if (score <= bestScore) continue;
            best = candidate;
            bestScore = score;
        }
        return (best, bestScore);
    }

    private static double Similarity(string candidate, string desired)
    {
        if (candidate.Length == 0 || desired.Length == 0) return 0;
        if (candidate == desired) return 1;
        if (desired.Contains(candidate) || candidate.Contains(desired)) return 0.88;
        var candidateTokens = candidate.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToHashSet();
        var desiredTokens = desired.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToHashSet();
        var overlap = candidateTokens.Intersect(desiredTokens).Count();
        return overlap == 0 ? 0 : (double)overlap / candidateTokens.Union(desiredTokens).Count();
    }

    private static string Normalize(string value)
    {
        var decomposed = (value ?? string.Empty).Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length);
        foreach (var character in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) == UnicodeCategory.NonSpacingMark) continue;
            builder.Append(char.IsLetterOrDigit(character) ? char.ToLowerInvariant(character) : ' ');
        }
        return string.Join(' ', builder.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }

    private static int? ParseCatalogId(string? code, string prefix) =>
        code != null && code.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
            && int.TryParse(code[prefix.Length..], out var id) ? id : null;
}
