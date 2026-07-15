using System.Linq;
using ElektroOffer_app.Data;
using ElektroOffer_app.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace ElektroOffer_app.Tests.Unit.RepositoryTests
{
    [TestFixture]
    public class WorkCatalogRepositoryTests
    {
        private static AppDbContext CreateInMemoryContext(SqliteConnection connection)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(connection)
                .Options;

            var context = new AppDbContext(options);
            context.Database.EnsureCreated();
            return context;
        }

        [Test]
        public void WorkCatalog_Should_Save_New_Work_Cascade_Entities()
        {
            using var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            using var db = CreateInMemoryContext(connection);

            var task = new WorkTask { Name = "Montaz", BasePrice = 100m };
            var specification = new WorkSpecification { Name = "Zasuvka", Unit = "ks" };
            var baseMaterial = new BaseMaterial { Name = "Cihla", BaseMaterialCoef = 1.2m };
            var position = new WorkPosition { Name = "Stena", PositionCoef = 1.1m };

            db.Tasks.Add(task);
            db.Specifications.Add(specification);
            db.BaseMaterials.Add(baseMaterial);
            db.Positions.Add(position);
            db.SaveChanges();

            db.TaskSpecifications.Add(new TaskSpecification
            {
                TaskId = task.Id,
                SpecificationId = specification.Id
            });
            db.SaveChanges();

            Assert.That(db.Tasks.Count(), Is.EqualTo(1));
            Assert.That(db.Specifications.Count(), Is.EqualTo(1));
            Assert.That(db.BaseMaterials.Count(), Is.EqualTo(1));
            Assert.That(db.Positions.Count(), Is.EqualTo(1));
            Assert.That(db.TaskSpecifications.Count(), Is.EqualTo(1));
        }
    }
}
