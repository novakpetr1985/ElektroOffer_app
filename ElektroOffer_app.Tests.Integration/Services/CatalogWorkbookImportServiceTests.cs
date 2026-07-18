using ElektroOffer_app.Data;
using ElektroOffer_app.Models;
using ElektroOffer_app.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System.IO.Compression;
using System.Xml.Linq;

namespace ElektroOffer_app.Tests.Integration.Services
{
    [TestFixture]
    /// <summary>Ověřuje validaci a transakční import katalogového XLSX bez částečných zápisů.</summary>
    public class CatalogWorkbookImportServiceTests
    {
        private SqliteConnection _connection = null!;
        private AppDbContext _db = null!;

        [SetUp]
        public void SetUp()
        {
            _connection = new SqliteConnection("Data Source=:memory:");
            _connection.Open();
            _db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options);
            _db.Database.EnsureCreated();
        }

        [TearDown]
        public void TearDown()
        {
            _db.Dispose();
            _connection.Dispose();
        }

        [Test]
        public void Import_Should_Create_All_Normalized_Catalog_Data()
        {
            var result = new CatalogWorkbookImportService(_db).Import(FindTemplate());

            Assert.Multiple(() =>
            {
                Assert.That(result.Success, Is.True, FormatIssues(result));
                Assert.That(_db.Tasks.Count(), Is.EqualTo(2));
                Assert.That(_db.Specifications.Count(), Is.EqualTo(2));
                Assert.That(_db.TaskSpecifications.Count(), Is.EqualTo(2));
                Assert.That(_db.BaseMaterials.Count(), Is.EqualTo(2));
                Assert.That(_db.Positions.Count(), Is.EqualTo(2));
                Assert.That(_db.Categories.Count(), Is.EqualTo(2));
                Assert.That(_db.Suppliers.Count(), Is.EqualTo(2));
                Assert.That(_db.Materials.Count(), Is.EqualTo(2));
                Assert.That(_db.MaterialPrices.Count(), Is.EqualTo(2));
            });
        }

        [Test]
        public void Reimport_Should_Update_Without_Creating_Duplicates()
        {
            var service = new CatalogWorkbookImportService(_db);
            var first = service.Import(FindTemplate());
            var second = service.Import(FindTemplate());

            Assert.Multiple(() =>
            {
                Assert.That(first.Success, Is.True, FormatIssues(first));
                Assert.That(second.Success, Is.True, FormatIssues(second));
                Assert.That(second.Inserted, Is.EqualTo(0));
                Assert.That(_db.Tasks.Count(), Is.EqualTo(2));
                Assert.That(_db.MaterialPrices.Count(), Is.EqualTo(2));
            });
        }

        [Test]
        public void Import_With_Unknown_Category_Should_Not_Change_Database()
        {
            _db.Categories.Add(new Category { Name = "Původní kategorie" });
            _db.SaveChanges();
            var invalidWorkbook = CreateWorkbookWithUnknownMaterialCategory();

            var result = new CatalogWorkbookImportService(_db).Import(invalidWorkbook);

            Assert.Multiple(() =>
            {
                Assert.That(result.Success, Is.False);
                Assert.That(result.Issues, Has.Some.Matches<CatalogImportIssue>(x =>
                    x.Sheet == "Materials" && x.Column == "CategoryName"));
                Assert.That(_db.Categories.Select(x => x.Name).ToList(), Is.EqualTo(new[] { "Původní kategorie" }));
                Assert.That(_db.Materials.Count(), Is.Zero);
                Assert.That(_db.Tasks.Count(), Is.Zero);
            });
        }

        private static string CreateWorkbookWithUnknownMaterialCategory()
        {
            var path = Path.Combine(TestContext.CurrentContext.WorkDirectory, $"invalid-catalog-{Guid.NewGuid():N}.xlsx");
            File.Copy(FindTemplate(), path);

            using var archive = ZipFile.Open(path, ZipArchiveMode.Update);
            var entry = archive.GetEntry("xl/worksheets/sheet9.xml")
                ?? throw new InvalidDataException("List Materials nebyl v testovací šabloně nalezen.");
            XDocument document;
            using (var stream = entry.Open()) document = XDocument.Load(stream);

            XNamespace spreadsheet = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
            var categoryCell = document.Descendants(spreadsheet + "c")
                .Single(x => (string?)x.Attribute("r") == "B2");
            categoryCell.Element(spreadsheet + "v")!.Value = "Neexistující kategorie";

            entry.Delete();
            var replacement = archive.CreateEntry("xl/worksheets/sheet9.xml");
            using var output = replacement.Open();
            document.Save(output);
            return path;
        }

        private static string FindTemplate()
        {
            var directory = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
            while (directory != null)
            {
                var path = Path.Combine(directory.FullName, "docs", "templates", "ElektroOffer_Catalog_Import_Template_1.0.xlsx");
                if (File.Exists(path)) return path;
                directory = directory.Parent;
            }
            throw new FileNotFoundException("Testovací XLSX šablona nebyla nalezena.");
        }

        private static string FormatIssues(CatalogImportResult result)
            => string.Join(Environment.NewLine, result.Issues.Select(x => $"{x.Sheet}:{x.Row}:{x.Column} {x.Message}"));
    }
}
