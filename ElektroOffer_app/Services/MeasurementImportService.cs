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
        options.AddRange(_db.Categories.AsNoTracking().Include(category => category.Materials).OrderBy(category => category.Name).Select(category => new FieldCatalogOption
        {
            Code = $"MATCAT-{category.Id}",
            Kind = FieldCatalogOptionKind.MaterialCategory,
            Name = category.Name,
            Category = "Materiál",
            Unit = category.Materials.Select(material => material.Unit).Distinct().Count() == 1
                ? category.Materials.Select(material => material.Unit).FirstOrDefault() ?? string.Empty
                : string.Empty
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
            if (item.WorkHints.Count == 0 && item.MaterialRequirements.Count == 0 && string.IsNullOrWhiteSpace(item.CatalogCode))
            {
                preview.Items.Add(Unresolved(
                    item,
                    area.Name,
                    MeasurementImportRowKind.Unknown,
                    item.DisplayName,
                    item.Quantity,
                    item.Unit,
                    "Položka byla v terénu zadána bez načteného katalogu. Nelze bezpečně určit, zda jde o práci nebo materiál."));
                continue;
            }

            var workHints = item.WorkHints.Count > 0
                ? item.WorkHints
                : item.CatalogCode.StartsWith("WORK-", StringComparison.OrdinalIgnoreCase) ? [InferWork(item)] : [];
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

        if (codedTask == null && taskScore < 0.4)
            return Unresolved(item, areaName, MeasurementImportRowKind.Work, hint.DisplayName, hint.Quantity, hint.Unit, "Pracovní úkon nebyl bezpečně spárován s aktuálním katalogem.");

        var confidence = Math.Clamp(Math.Min(hint.Confidence <= 0 ? 0.7m : hint.Confidence, (decimal)taskScore), 0, 1);
        return new MeasurementImportPreviewItem
        {
            SourceItemId = item.Id,
            AreaName = areaName,
            Kind = MeasurementImportRowKind.Work,
            SourceDescription = string.IsNullOrWhiteSpace(hint.DisplayName) ? item.DisplayName : hint.DisplayName,
            SuggestedMapping = $"{task.Name} → upřesnění, podklad a umístění doplníte v kalkulaci",
            Quantity = (double)(hint.Quantity > 0 ? hint.Quantity : item.Quantity),
            Unit = hint.Unit,
            Confidence = confidence,
            CanImport = true,
            IsSelected = true,
            Warning = "Doplňte upřesnění práce, podklad a umístění.",
            WorkTask = task.Name
        };
    }

    private MeasurementImportPreviewItem MapMaterial(string areaName, MeasurementItem item, MaterialRequirement requirement)
    {
        var categories = _db.Categories.AsNoTracking().ToList();
        var categoryId = ParseCatalogId(requirement.CategoryCode, "MATCAT-") ?? ParseCatalogId(item.CatalogCode, "MATCAT-");
        var category = categoryId.HasValue ? categories.FirstOrDefault(candidate => candidate.Id == categoryId.Value) : null;
        var score = category != null ? 1d : 0d;

        if (category == null)
        {
            var legacyMaterialId = ParseCatalogId(requirement.MaterialCode, "MAT-") ?? ParseCatalogId(item.CatalogCode, "MAT-");
            if (legacyMaterialId.HasValue)
                category = _db.Materials.AsNoTracking().Include(material => material.Category)
                    .Where(material => material.Id == legacyMaterialId.Value)
                    .Select(material => material.Category)
                    .FirstOrDefault();
            if (category != null) score = 1d;
        }

        if (category == null)
            (category, score) = Best(categories, value => value.Name, $"{requirement.Category} {requirement.Specification} {item.DisplayName}");
        if (category == null || score < 0.28)
            return Unresolved(item, areaName, MeasurementImportRowKind.Material, requirement.Specification, requirement.Quantity, requirement.Unit, "Kategorie materiálu nebyla bezpečně spárována s aktuálním katalogem.");

        var quantity = requirement.Quantity * (1 + requirement.ReservePercent / 100m);
        return new MeasurementImportPreviewItem
        {
            SourceItemId = item.Id,
            AreaName = areaName,
            Kind = MeasurementImportRowKind.Material,
            SourceDescription = string.IsNullOrWhiteSpace(requirement.Specification)
                ? string.IsNullOrWhiteSpace(requirement.Category) ? item.DisplayName : requirement.Category
                : requirement.Specification,
            SuggestedMapping = $"{category.Name} → konkrétní materiál a dodavatele doplníte v kalkulaci",
            Quantity = (double)quantity,
            Unit = requirement.Unit,
            Confidence = (decimal)Math.Clamp(score, 0, 1),
            CanImport = true,
            IsSelected = true,
            Warning = "Doplňte konkrétní materiál, dodavatele a nabídku.",
            MaterialCategory = category.Name
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
