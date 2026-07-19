using ElektroOffer.Contracts.Measurements;
using ElektroOffer.Contracts.Catalog;
using ElektroOffer_app.Models;
using ElektroOffer_app.Services;
using NUnit.Framework;
using System.Security.Cryptography;

namespace ElektroOffer_app.Tests.Unit.Services;

[TestFixture]
public sealed class MeasurementImportServiceTests : TestBase
{
    [Test]
    public async Task Prepare_MapsWorkAndChoosesLowestMaterialOffer()
    {
        SeedCatalog();
        var package = CreatePackage();
        var archivePath = await WritePackageAsync(package);

        try
        {
            var preview = await new MeasurementImportService(_db).PrepareAsync(archivePath, []);

            Assert.Multiple(() =>
            {
                Assert.That(preview.Items, Has.Count.EqualTo(2));
                Assert.That(preview.Items.All(item => item.CanImport), Is.True);
                Assert.That(preview.Items.Single(item => item.Kind == MeasurementImportRowKind.Work).WorkTask, Is.EqualTo("Drážkování"));
                Assert.That(preview.Items.Single(item => item.Kind == MeasurementImportRowKind.Material).Supplier, Is.EqualTo("Levný dodavatel"));
                Assert.That(preview.Items.Single(item => item.Kind == MeasurementImportRowKind.Material).Quantity, Is.EqualTo(22d).Within(0.001));
            });
        }
        finally
        {
            File.Delete(archivePath);
        }
    }

    [Test]
    public async Task Prepare_RejectsAlreadyImportedExportId()
    {
        SeedCatalog();
        var package = CreatePackage();
        var archivePath = await WritePackageAsync(package);

        try
        {
            Assert.That(
                async () => await new MeasurementImportService(_db).PrepareAsync(archivePath, [package.ExportId]),
                Throws.TypeOf<InvalidOperationException>().With.Message.Contains("již byl"));
        }
        finally
        {
            File.Delete(archivePath);
        }
    }

    [Test]
    public async Task StoreApprovedImport_CopiesPackageAndExtractsSelectedPhoto()
    {
        SeedCatalog();
        var photo = "photo-content"u8.ToArray();
        var package = CreatePackage();
        package.Attachments.Add(new AttachmentReference
        {
            AreaId = package.Project.Areas[0].Id,
            ItemId = package.Project.Areas[0].Items[0].Id,
            RelativePath = "attachments/test.jpg",
            ContentType = "image/jpeg",
            Size = photo.Length,
            Sha256 = Convert.ToHexString(SHA256.HashData(photo)).ToLowerInvariant(),
            Note = "test.jpg"
        });
        var archivePath = Path.Combine(Path.GetTempPath(), $"import-{Guid.NewGuid():N}.eofmeasure");
        var projectDirectory = Path.Combine(Path.GetTempPath(), $"project-{Guid.NewGuid():N}");
        var projectPath = Path.Combine(projectDirectory, "test.eof");
        Directory.CreateDirectory(projectDirectory);

        try
        {
            await MeasurementArchiveService.WriteAsync(package, archivePath, (_, _) => Task.FromResult<Stream>(new MemoryStream(photo)));
            var service = new MeasurementImportService(_db);
            var preview = await service.PrepareAsync(archivePath, []);
            var imported = await service.StoreApprovedImportAsync(preview, projectPath);
            var assets = MeasurementImportService.GetProjectAssetsDirectory(projectPath);

            Assert.Multiple(() =>
            {
                Assert.That(imported.Attachments, Has.Count.EqualTo(1));
                Assert.That(File.Exists(Path.Combine(assets, imported.StoredPackageRelativePath)), Is.True);
                Assert.That(File.ReadAllBytes(Path.Combine(assets, imported.Attachments[0].RelativePath)), Is.EqualTo(photo));
            });
        }
        finally
        {
            if (File.Exists(archivePath)) File.Delete(archivePath);
            if (Directory.Exists(projectDirectory)) Directory.Delete(projectDirectory, recursive: true);
        }
    }

    [Test]
    public void CreateFieldCatalog_ProducesStableUniqueCodesAndRoundTrips()
    {
        SeedCatalog();

        var catalog = new MeasurementImportService(_db).CreateFieldCatalog();
        var restored = FieldCatalogSerializer.Deserialize(FieldCatalogSerializer.Serialize(catalog));

        Assert.Multiple(() =>
        {
            Assert.That(restored.Options, Has.Count.EqualTo(2));
            Assert.That(restored.Options.Select(option => option.Code), Is.Unique);
            Assert.That(restored.Options.Any(option => option.Code.StartsWith("WORK-") && option.Name == "Drážkování"), Is.True);
            Assert.That(restored.Options.Any(option => option.Code.StartsWith("MAT-") && option.Name == "CYKY-J 3x2,5"), Is.True);
        });
    }

    private void SeedCatalog()
    {
        var task = new WorkTask { Name = "Drážkování", BasePrice = 80m };
        var specification = new WorkSpecification { Name = "Spára", Unit = "m" };
        _db.AddRange(task, specification, new BaseMaterial { Name = "-", BaseMaterialCoef = 1m }, new WorkPosition { Name = "Stěna", PositionCoef = 1.2m });
        _db.SaveChanges();
        _db.TaskSpecifications.Add(new TaskSpecification { TaskId = task.Id, SpecificationId = specification.Id });

        var category = new Category { Name = "Kabel" };
        var material = new Material { Name = "CYKY-J 3x2,5", Unit = "m", Category = category };
        var cheap = new Supplier { Name = "Levný dodavatel" };
        var expensive = new Supplier { Name = "Drahý dodavatel" };
        _db.AddRange(category, material, cheap, expensive);
        _db.SaveChanges();
        _db.MaterialPrices.AddRange(
            new MaterialPrice { MaterialId = material.Id, SupplierId = cheap.Id, SupplierCode = "C1", SupplierName = "CYKY levně", Unit = "m", Price = 30m },
            new MaterialPrice { MaterialId = material.Id, SupplierId = expensive.Id, SupplierCode = "E1", SupplierName = "CYKY draze", Unit = "m", Price = 40m });
        _db.SaveChanges();
    }

    private static MeasurementPackage CreatePackage()
    {
        var item = new MeasurementItem
        {
            Kind = MeasurementKind.CableRoute,
            DisplayName = "Kabelová trasa CYKY 3x2,5",
            Quantity = 20,
            Unit = "m",
            WorkHints = [new WorkHint { DisplayName = "Drážkování ve zdivu", Quantity = 20, Unit = "m", Confidence = 0.9m }],
            MaterialRequirements = [new MaterialRequirement { MaterialCode = "MAT-CYKY-3X2.5", Specification = "CYKY-J 3x2,5", Quantity = 20, Unit = "m", ReservePercent = 10 }]
        };
        return new MeasurementPackage
        {
            Project = new MeasurementProject
            {
                Name = "Test import",
                Areas = [new MeasurementArea { Name = "Kuchyně", Items = [item] }]
            }
        };
    }

    private static async Task<string> WritePackageAsync(MeasurementPackage package)
    {
        var path = Path.Combine(Path.GetTempPath(), $"import-{Guid.NewGuid():N}.eofmeasure");
        await MeasurementArchiveService.WriteAsync(package, path, (_, _) => throw new InvalidOperationException());
        return path;
    }
}
