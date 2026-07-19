using System.Text.RegularExpressions;

namespace ElektroOffer.Contracts.Measurements;

public sealed record MeasurementValidationIssue(string Code, string Path, string Message);

public sealed class MeasurementValidationResult
{
    public List<MeasurementValidationIssue> Issues { get; } = [];
    public bool IsValid => Issues.Count == 0;
}

public static partial class MeasurementPackageValidator
{
    public const long MaximumAttachmentSize = 50L * 1024 * 1024;

    public static MeasurementValidationResult Validate(MeasurementPackage package)
    {
        ArgumentNullException.ThrowIfNull(package);
        var result = new MeasurementValidationResult();

        AddIf(package.SchemaVersion != MeasurementPackage.CurrentSchemaVersion, "schema.unsupported", "schemaVersion", "Unsupported measurement schema version.");
        AddIf(package.ExportId == Guid.Empty, "export.id.empty", "exportId", "Export ID is required.");
        AddIf(package.Project.Id == Guid.Empty, "project.id.empty", "project.id", "Project ID is required.");
        AddIf(string.IsNullOrWhiteSpace(package.Project.Name), "project.name.empty", "project.name", "Project name is required.");

        var ids = new HashSet<Guid>();
        foreach (var (area, areaIndex) in package.Project.Areas.Select((value, index) => (value, index)))
        {
            AddIf(area.Id == Guid.Empty || !ids.Add(area.Id), "area.id.invalid", $"project.areas[{areaIndex}].id", "Area ID must be non-empty and unique.");
            AddIf(string.IsNullOrWhiteSpace(area.Name), "area.name.empty", $"project.areas[{areaIndex}].name", "Area name is required.");

            foreach (var (item, itemIndex) in area.Items.Select((value, index) => (value, index)))
            {
                var path = $"project.areas[{areaIndex}].items[{itemIndex}]";
                AddIf(item.Id == Guid.Empty || !ids.Add(item.Id), "item.id.invalid", $"{path}.id", "Item ID must be non-empty and unique.");
                AddIf(string.IsNullOrWhiteSpace(item.DisplayName), "item.name.empty", $"{path}.displayName", "Item name is required.");
                AddIf(item.Quantity <= 0, "item.quantity.invalid", $"{path}.quantity", "Quantity must be greater than zero.");
                AddIf(string.IsNullOrWhiteSpace(item.Unit), "item.unit.empty", $"{path}.unit", "Unit is required.");
                AddIf(item.ReservePercent is < 0 or > 100, "item.reserve.invalid", $"{path}.reservePercent", "Reserve must be between 0 and 100 percent.");

                foreach (var (work, workIndex) in item.WorkHints.Select((value, index) => (value, index)))
                {
                    AddIf(work.Quantity <= 0, "work.quantity.invalid", $"{path}.workHints[{workIndex}].quantity", "Suggested work quantity must be greater than zero.");
                    AddIf(string.IsNullOrWhiteSpace(work.Unit), "work.unit.empty", $"{path}.workHints[{workIndex}].unit", "Suggested work unit is required.");
                    AddIf(work.Confidence is < 0 or > 1, "work.confidence.invalid", $"{path}.workHints[{workIndex}].confidence", "Confidence must be between zero and one.");
                }

                foreach (var (material, materialIndex) in item.MaterialRequirements.Select((value, index) => (value, index)))
                {
                    AddIf(material.Quantity <= 0, "material.quantity.invalid", $"{path}.materialRequirements[{materialIndex}].quantity", "Material quantity must be greater than zero.");
                    AddIf(string.IsNullOrWhiteSpace(material.Unit), "material.unit.empty", $"{path}.materialRequirements[{materialIndex}].unit", "Material unit is required.");
                    AddIf(material.ReservePercent is < 0 or > 100, "material.reserve.invalid", $"{path}.materialRequirements[{materialIndex}].reservePercent", "Material reserve must be between 0 and 100 percent.");
                }
            }
        }

        var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (attachment, index) in package.Attachments.Select((value, position) => (value, position)))
        {
            var path = $"attachments[{index}]";
            var unsafePath = string.IsNullOrWhiteSpace(attachment.RelativePath)
                || Path.IsPathRooted(attachment.RelativePath)
                || attachment.RelativePath.Split('/', '\\').Any(part => part == "..");
            AddIf(attachment.Id == Guid.Empty || !ids.Add(attachment.Id), "attachment.id.invalid", $"{path}.id", "Attachment ID must be non-empty and unique.");
            AddIf(unsafePath, "attachment.path.unsafe", $"{path}.relativePath", "Attachment path must be a safe relative path.");
            AddIf(!unsafePath && !paths.Add(attachment.RelativePath), "attachment.path.duplicate", $"{path}.relativePath", "Attachment path must be unique.");
            AddIf(attachment.Size < 0, "attachment.size.invalid", $"{path}.size", "Attachment size cannot be negative.");
            AddIf(attachment.Size > MaximumAttachmentSize, "attachment.size.exceeded", $"{path}.size", "Attachment is larger than 50 MB.");
            var extension = Path.GetExtension(attachment.RelativePath);
            AddIf(extension.Equals(".exe", StringComparison.OrdinalIgnoreCase)
                || extension.Equals(".dll", StringComparison.OrdinalIgnoreCase)
                || extension.Equals(".js", StringComparison.OrdinalIgnoreCase)
                || extension.Equals(".xaml", StringComparison.OrdinalIgnoreCase),
                "attachment.type.forbidden", $"{path}.relativePath", "Executable attachment types are forbidden.");
            AddIf(!Sha256Regex().IsMatch(attachment.Sha256), "attachment.hash.invalid", $"{path}.sha256", "SHA-256 must contain 64 hexadecimal characters.");
        }

        return result;

        void AddIf(bool condition, string code, string path, string message)
        {
            if (condition)
                result.Issues.Add(new MeasurementValidationIssue(code, path, message));
        }
    }

    [GeneratedRegex("^[0-9a-fA-F]{64}$", RegexOptions.CultureInvariant)]
    private static partial Regex Sha256Regex();
}
