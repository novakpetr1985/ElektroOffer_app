using System.Linq;
using ElektroOffer_app.Data;
using ElektroOffer_app.Models;
using ElektroOffer_app.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace ElektroOffer_app.Tests.Integration.Services
{
    [TestFixture]
    public class CatalogServiceTests_Advanced
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
        public void IsCatalogEmpty_Should_Return_True_When_No_Data()
        {
            Assert.That(_service.IsCatalogEmpty(_db), Is.True);
        }

        [Test]
        public void GetWorkSpecifications_Should_Use_TaskSpecifications_Relationship()
        {
            var task = new WorkTask { Name = "Montaz kabelu", BasePrice = 50m };
            var specification = new WorkSpecification { Name = "CYKY 3x2.5", Unit = "m" };

            _db.Tasks.Add(task);
            _db.Specifications.Add(specification);
            _db.SaveChanges();
            _db.TaskSpecifications.Add(new TaskSpecification { TaskId = task.Id, SpecificationId = specification.Id });
            _db.SaveChanges();

            var result = _service.GetWorkSpecifications(_db, task.Id);

            Assert.That(result.Select(x => x.Name).ToArray(), Is.EqualTo(new[] { "CYKY 3x2.5" }));
        }
    }
}
