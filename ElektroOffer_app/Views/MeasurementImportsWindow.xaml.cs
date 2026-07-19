using System.Diagnostics;
using System.IO;
using System.Windows;
using ElektroOffer_app.Models;
using ElektroOffer_app.Services;

namespace ElektroOffer_app.Views;

public partial class MeasurementImportsWindow : Window
{
    private readonly string _assetsRoot;

    public MeasurementImportsWindow(IEnumerable<ImportedMeasurementData> imports, string projectPath)
    {
        InitializeComponent();
        _assetsRoot = MeasurementImportService.GetProjectAssetsDirectory(projectPath);
        DataContext = imports.SelectMany(import => import.Attachments.Count > 0
            ? import.Attachments.Select(attachment => AttachmentRow.From(import, attachment))
            : [AttachmentRow.From(import, null)]).ToList();
    }

    private void OpenAttachment_Click(object sender, RoutedEventArgs e)
    {
        var row = ImportsGrid.SelectedItem as AttachmentRow;
        OpenRelativePath(row?.AttachmentRelativePath, "Vyberte řádek s přílohou.");
    }

    private void OpenPackage_Click(object sender, RoutedEventArgs e)
    {
        var row = ImportsGrid.SelectedItem as AttachmentRow;
        OpenRelativePath(row?.PackageRelativePath, "Vyberte importované měření.");
    }

    private void OpenRelativePath(string? relativePath, string emptyMessage)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            MessageBox.Show(emptyMessage, "Terénní přílohy", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var fullPath = Path.GetFullPath(Path.Combine(_assetsRoot, relativePath));
        if (!fullPath.StartsWith(Path.GetFullPath(_assetsRoot) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
            || !File.Exists(fullPath))
        {
            MessageBox.Show("Soubor přílohy nebyl nalezen. Zkontrolujte doprovodnou složku projektu.", "Terénní přílohy", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (Path.GetExtension(fullPath).Equals(".eofmeasure", StringComparison.OrdinalIgnoreCase))
            Process.Start(new ProcessStartInfo("explorer.exe", $"/select,\"{fullPath}\"") { UseShellExecute = true });
        else
            Process.Start(new ProcessStartInfo(fullPath) { UseShellExecute = true });
    }

    private sealed record AttachmentRow(
        DateTime ImportedAt,
        string ProjectName,
        string AreaName,
        string ItemName,
        string FileName,
        string SizeText,
        string PackageRelativePath,
        string? AttachmentRelativePath)
    {
        public static AttachmentRow From(ImportedMeasurementData import, ProjectAttachmentData? attachment) => new(
            import.ImportedAtUtc.ToLocalTime(),
            import.SourceProjectName,
            attachment?.AreaName ?? string.Empty,
            attachment?.ItemName ?? string.Empty,
            attachment?.FileName ?? "(bez příloh)",
            attachment == null ? string.Empty : $"{attachment.Size / 1024d:N0} kB",
            import.StoredPackageRelativePath,
            attachment?.RelativePath);
    }
}
