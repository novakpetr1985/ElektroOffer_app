using ElektroOffer_app.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace ElektroOffer_app.Tests.Integration.Database;

[TestFixture]
public class DatabaseSchemaTests
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
    public void Should_Create_AppDbContext_Tables()
    {
        Assert.Multiple(() =>
        {
            Assert.That(_db.Tasks, Is.Not.Null);
            Assert.That(_db.Specifications, Is.Not.Null);
            Assert.That(_db.BaseMaterials, Is.Not.Null);
            Assert.That(_db.Positions, Is.Not.Null);
            Assert.That(_db.TaskSpecifications, Is.Not.Null);
            Assert.That(_db.Materials, Is.Not.Null);
            Assert.That(_db.Database.CanConnect(), Is.True);
        });
    }
}
