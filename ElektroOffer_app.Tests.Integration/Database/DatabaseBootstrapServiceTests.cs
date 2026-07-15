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
}
