using ElektroOffer_app.Models;
using ElektroOffer_app.Services;
using ElektroOffer_app.Services.Abstractions;
using ElektroOffer_app.Services.Implementations;
using Moq;
using NUnit.Framework;

namespace ElektroOffer_app.Tests.Unit.Services;

[TestFixture]
public sealed class ProjectMeasurementPersistenceTests
{
    [Test]
    public void SaveAs_CopiesMeasurementAssetsAndPersistsImportHistory()
    {
        var root = Path.Combine(Path.GetTempPath(), $"project-save-{Guid.NewGuid():N}");
        var sourceProject = Path.Combine(root, "source.eof");
        var destinationProject = Path.Combine(root, "copy.eof");
        var sourceAssets = MeasurementImportService.GetProjectAssetsDirectory(sourceProject);
        var assetRelative = Path.Combine("imports", "test", "attachments", "photo.jpg");
        Directory.CreateDirectory(Path.GetDirectoryName(Path.Combine(sourceAssets, assetRelative))!);
        File.WriteAllText(Path.Combine(sourceAssets, assetRelative), "photo");

        var dialogs = new Mock<IFileDialogService>();
        dialogs.Setup(dialog => dialog.ShowSaveFileDialog(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(destinationProject);
        dialogs.Setup(dialog => dialog.ShowOpenFileDialog(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(destinationProject);
        var messages = new Mock<IMessageBoxService>();
        var service = new ProjectService(dialogs.Object, new RealFileSystemService(), messages.Object);
        var data = new ProjectData
        {
            ProjectName = "Import test",
            MeasurementImports = [new ImportedMeasurementData { ExportId = Guid.NewGuid(), SourceProjectName = "Terén" }]
        };

        try
        {
            var saved = service.SaveAs(data, sourceProject);
            var (loaded, _) = service.Load();
            var destinationAsset = Path.Combine(MeasurementImportService.GetProjectAssetsDirectory(destinationProject), assetRelative);

            Assert.Multiple(() =>
            {
                Assert.That(saved, Is.EqualTo(destinationProject));
                Assert.That(File.Exists(destinationAsset), Is.True);
                Assert.That(File.ReadAllText(destinationAsset), Is.EqualTo("photo"));
                Assert.That(loaded?.MeasurementImports, Has.Count.EqualTo(1));
            });
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
    }
}
