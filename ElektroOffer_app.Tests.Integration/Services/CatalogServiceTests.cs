using System.Linq;
using ElektroOffer_app.Data;
using ElektroOffer_app.Models;
using ElektroOffer_app.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace ElektroOffer_app.Tests.Integration.Services;

[TestFixture]
/// <summary>Ověřuje základní katalogové dotazy a změny nad skutečným SQLite providerem.</summary>
public class CatalogServiceTests
{
    private SqliteConnection _connection = null!;
    private AppDbContext _db = null!;
    private CatalogService _service = null!;

    [SetUp]
    public void Setup()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        _db = new AppDbContext(options);
        _db.Database.EnsureCreated();
        _service = new CatalogService();
    }

    [TearDown]
    public void TearDown()
    {
        _db.Dispose();
        _connection.Dispose();
    }

    [Test]
    public void Should_Return_False_When_Catalog_Contains_Work_Or_Material_Data()
    {
        _db.Tasks.Add(new WorkTask { Name = "Montaz", BasePrice = 100m });
        _db.Materials.Add(new Material { Name = "Kabel", Price = 10 });
        _db.SaveChanges();

        Assert.That(_service.IsCatalogEmpty(_db), Is.False);
    }

    [Test]
    public void Should_Load_New_Work_Catalog_Lists()
    {
        _db.Tasks.Add(new WorkTask { Name = "Montaz", BasePrice = 100m });
        _db.BaseMaterials.Add(new BaseMaterial { Name = "Cihla", BaseMaterialCoef = 1.2m });
        _db.Positions.Add(new WorkPosition { Name = "Stena", PositionCoef = 1.0m });
        _db.SaveChanges();

        Assert.That(_service.GetWorkTasks(_db).Select(t => t.Name).ToArray(), Does.Contain("Montaz"));
        Assert.That(_service.GetBaseMaterials(_db).Select(b => b.Name).ToArray(), Does.Contain("Cihla"));
        Assert.That(_service.GetWorkPositions(_db).Select(p => p.Name).ToArray(), Does.Contain("Stena"));
    }
}
