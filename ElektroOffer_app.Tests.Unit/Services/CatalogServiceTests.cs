using System.Linq;
using ElektroOffer_app.Data;
using ElektroOffer_app.Models;
using ElektroOffer_app.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace ElektroOffer_app.Tests.Unit.Services
{
    [TestFixture]
    /// <summary>Ověřuje aplikační operace katalogu nad izolovanou SQLite databází.</summary>
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
        public void Cleanup()
        {
            _db.Dispose();
            _connection.Dispose();
        }

        [Test]
        public void GetWorkTasks_Should_Return_Tasks_Ordered_By_Name()
        {
            _db.Tasks.AddRange(
                new WorkTask { Name = "Revize", BasePrice = 300m },
                new WorkTask { Name = "Montaz", BasePrice = 100m });
            _db.SaveChanges();

            var tasks = _service.GetWorkTasks(_db);

            Assert.That(tasks.Select(t => t.Name).ToArray(), Is.EqualTo(new[] { "Montaz", "Revize" }));
        }

        [Test]
        public void GetWorkSpecifications_Should_Return_Only_Valid_Task_Pairs()
        {
            var task = new WorkTask { Name = "Montaz", BasePrice = 100m };
            var otherTask = new WorkTask { Name = "Revize", BasePrice = 300m };
            var specification = new WorkSpecification { Name = "Kabel", Unit = "m" };
            var otherSpecification = new WorkSpecification { Name = "Zasuvka", Unit = "ks" };

            _db.Tasks.AddRange(task, otherTask);
            _db.Specifications.AddRange(specification, otherSpecification);
            _db.SaveChanges();
            _db.TaskSpecifications.Add(new TaskSpecification { TaskId = task.Id, SpecificationId = specification.Id });
            _db.SaveChanges();

            var specifications = _service.GetWorkSpecifications(_db, task.Id);

            Assert.That(specifications, Has.Count.EqualTo(1));
            Assert.That(specifications[0].Name, Is.EqualTo("Kabel"));
        }

        [Test]
        public void IsCatalogEmpty_Should_Return_False_When_WorkTasks_Exist()
        {
            _db.Tasks.Add(new WorkTask { Name = "Montaz", BasePrice = 100m });
            _db.SaveChanges();

            Assert.That(_service.IsCatalogEmpty(_db), Is.False);
        }

        [Test]
        public void LoadMaterials_Should_Return_Materials()
        {
            _db.Materials.Add(new Material { Name = "CYKY", Price = 20 });
            _db.SaveChanges();

            var materials = _service.LoadMaterials(_db);

            Assert.That(materials, Has.Count.EqualTo(1));
            Assert.That(materials[0].Name, Is.EqualTo("CYKY"));
        }
    }
}
