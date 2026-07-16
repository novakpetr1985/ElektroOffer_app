using System.Linq;
using ElektroOffer_app.Data;
using ElektroOffer_app.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace ElektroOffer_app.Tests.Integration.Database;

[TestFixture]
public class DatabaseBootstrapServiceTests
{
    private SqliteConnection _connection = null!;
    private AppDbContext _db = null!;

    [SetUp]
    public void Setup()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        _db = new AppDbContext(options);
    }

    [TearDown]
    public void TearDown()
    {
        _db.Dispose();
        _connection.Dispose();
    }

    [Test]
    public void EnsureReady_Should_Create_Schema_And_Seed_Test_Catalogs_When_Database_Is_Empty()
    {
        DatabaseBootstrapService.EnsureReady(_db);

        Assert.Multiple(() =>
        {
            Assert.That(_db.Database.CanConnect(), Is.True);
            Assert.That(_db.Categories.Count(), Is.EqualTo(6));
            Assert.That(_db.Suppliers.Count(), Is.EqualTo(2));
            Assert.That(_db.Materials.Count(), Is.EqualTo(10));
            Assert.That(_db.MaterialPrices.Count(), Is.EqualTo(20));
            Assert.That(_db.BaseMaterials.Count(), Is.EqualTo(7));
            Assert.That(_db.Positions.Count(), Is.EqualTo(4));
            Assert.That(_db.Tasks.Count(), Is.EqualTo(5));
            Assert.That(_db.Specifications.Count(), Is.EqualTo(5));
            Assert.That(_db.TaskSpecifications.Count(), Is.EqualTo(6));
            Assert.That(_db.Tasks.Any(t => t.Name == "Drážkování" && t.BasePrice == 80m), Is.True);
            Assert.That(_db.Specifications.Any(s => s.Name == "Spára" && s.Unit == "m"), Is.True);
            Assert.That(_db.Materials.Any(m => m.Name == "CYKY-J 3x1,5"), Is.True);
            Assert.That(_db.MaterialPrices.Any(mp => mp.SupplierCode == "17018" && mp.Price == 23.22m), Is.True);
        });
    }

    [Test]
    public void EnsureReady_Should_Be_Idempotent()
    {
        DatabaseBootstrapService.EnsureReady(_db);
        var originalCounts = new
        {
            Categories = _db.Categories.Count(),
            Materials = _db.Materials.Count(),
            Tasks = _db.Tasks.Count()
        };

        DatabaseBootstrapService.EnsureReady(_db);

        Assert.Multiple(() =>
        {
            Assert.That(_db.Categories.Count(), Is.EqualTo(originalCounts.Categories));
            Assert.That(_db.Materials.Count(), Is.EqualTo(originalCounts.Materials));
            Assert.That(_db.Tasks.Count(), Is.EqualTo(originalCounts.Tasks));
        });
    }

    [Test]
    public void EnsureReady_Should_Preserve_Existing_Catalog_Data()
    {
        _db.Database.ExecuteSqlRaw("CREATE TABLE Categories (Id INTEGER PRIMARY KEY AUTOINCREMENT, Name TEXT NOT NULL);");
        _db.Database.ExecuteSqlRaw("INSERT INTO Categories (Name) VALUES ('User category');");

        DatabaseBootstrapService.EnsureReady(_db);

        Assert.Multiple(() =>
        {
            Assert.That(_db.Categories.Select(category => category.Name), Is.EqualTo(new[] { "User category" }));
            Assert.That(_db.Materials.Count(), Is.Zero);
            Assert.That(_db.Tasks.Count(), Is.Zero);
        });
    }

    [Test]
    public void EnsureReady_Should_Not_Replace_Corrupted_Database()
    {
        var path = Path.Combine(Path.GetTempPath(), $"ElektroOffer_Corrupt_{Guid.NewGuid():N}.db");
        const string originalContent = "not-a-sqlite-database";
        File.WriteAllText(path, originalContent);
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite($"Data Source={path}")
            .Options;

        try
        {
            using (var corruptedDb = new AppDbContext(options))
            {
                Assert.Throws<SqliteException>(() => DatabaseBootstrapService.EnsureReady(corruptedDb));
            }
            SqliteConnection.ClearAllPools();
            Assert.That(File.ReadAllText(path), Is.EqualTo(originalContent));
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            if (File.Exists(path))
                File.Delete(path);
        }
    }
}
