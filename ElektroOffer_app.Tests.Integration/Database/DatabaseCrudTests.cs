using System.Linq;
using ElektroOffer_app.Data;
using ElektroOffer_app.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace ElektroOffer_app.Tests.Integration.Database;

[TestFixture]
/// <summary>Ověřuje zápis, načtení a vazby základních databázových entit.</summary>
public class DatabaseCrudTests
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
        _db.Database.EnsureCreated();
    }

    [TearDown]
    public void TearDown()
    {
        _db.Dispose();
        _connection.Dispose();
    }

    [Test]
    public void Should_Insert_And_Read_WorkTask()
    {
        _db.Tasks.Add(new WorkTask { Name = "Montaz zasuvky", BasePrice = 100m });
        _db.SaveChanges();

        var loadedItem = _db.Tasks.First();

        Assert.That(loadedItem.Name, Is.EqualTo("Montaz zasuvky"));
        Assert.That(loadedItem.BasePrice, Is.EqualTo(100m));
    }

    [Test]
    public void Should_Update_BaseMaterial()
    {
        var item = new BaseMaterial { Name = "Cihla", BaseMaterialCoef = 1.2m };
        _db.BaseMaterials.Add(item);
        _db.SaveChanges();

        item.BaseMaterialCoef = 1.5m;
        _db.SaveChanges();

        Assert.That(_db.BaseMaterials.First().BaseMaterialCoef, Is.EqualTo(1.5m));
    }

    [Test]
    public void Should_Delete_WorkPosition()
    {
        var item = new WorkPosition { Name = "Stena", PositionCoef = 1m };
        _db.Positions.Add(item);
        _db.SaveChanges();

        _db.Positions.Remove(item);
        _db.SaveChanges();

        Assert.That(_db.Positions.Count(), Is.EqualTo(0));
    }

    [Test]
    public void Should_Save_TaskSpecification_Pair()
    {
        var task = new WorkTask { Name = "Montaz", BasePrice = 100m };
        var specification = new WorkSpecification { Name = "Kabel", Unit = "m" };

        _db.Tasks.Add(task);
        _db.Specifications.Add(specification);
        _db.SaveChanges();

        _db.TaskSpecifications.Add(new TaskSpecification
        {
            TaskId = task.Id,
            SpecificationId = specification.Id
        });
        _db.SaveChanges();

        Assert.That(_db.TaskSpecifications.Count(), Is.EqualTo(1));
    }
}
