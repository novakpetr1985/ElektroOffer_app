using System.Linq;
using ElektroOffer_app.Data;
using ElektroOffer_app.Models;
using ElektroOffer_app.ViewModels.Items;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace ElektroOffer_app.Tests.Integration.ViewModels
{
    [TestFixture]
    public class CalculationItemViewModel_CascadeTests
    {
        private SqliteConnection _connection = null!;
        private AppDbContext _db = null!;

        [SetUp]
        public void SetUp()
        {
            _connection = new SqliteConnection("Data Source=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(_connection)
                .Options;

            _db = new AppDbContext(options);
            _db.Database.EnsureCreated();
            SeedTestData();
        }

        [TearDown]
        public void TearDown()
        {
            _db.Dispose();
            _connection.Dispose();
        }

        private void SeedTestData()
        {
            var mount = new WorkTask { Name = "Montaz", BasePrice = 100m };
            var revision = new WorkTask { Name = "Revize", BasePrice = 300m };
            var cable = new WorkSpecification { Name = "Kabel", Unit = "m" };
            var socket = new WorkSpecification { Name = "Zasuvka", Unit = "ks" };

            _db.Tasks.AddRange(mount, revision);
            _db.Specifications.AddRange(cable, socket);
            _db.BaseMaterials.AddRange(
                new BaseMaterial { Name = "Cihla", BaseMaterialCoef = 1.2m },
                new BaseMaterial { Name = "Beton", BaseMaterialCoef = 1.5m });
            _db.Positions.AddRange(
                new WorkPosition { Name = "Stena", PositionCoef = 1.0m },
                new WorkPosition { Name = "Strop", PositionCoef = 1.3m });
            _db.SaveChanges();

            _db.TaskSpecifications.AddRange(
                new TaskSpecification { TaskId = mount.Id, SpecificationId = cable.Id },
                new TaskSpecification { TaskId = mount.Id, SpecificationId = socket.Id },
                new TaskSpecification { TaskId = revision.Id, SpecificationId = socket.Id });
            _db.SaveChanges();
        }

        [Test]
        public void Changing_WorkTask_Should_Load_Only_Valid_Specifications()
        {
            var vm = new CalculationItemViewModel(_db);

            vm.SelectedWorkTask = "Montaz";

            Assert.That(vm.AvailableWorkSpecifications, Has.Count.EqualTo(2));
            Assert.That(vm.AvailableWorkSpecifications.ToArray(), Does.Contain("Kabel"));
            Assert.That(vm.AvailableWorkSpecifications.ToArray(), Does.Contain("Zasuvka"));
        }

        [Test]
        public void WorkSpecification_Should_Load_Unit()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                SelectedWorkTask = "Montaz"
            };

            vm.SelectedWorkSpecification = "Kabel";

            Assert.That(vm.WorkUnit, Is.EqualTo("m"));
        }

        [Test]
        public void New_Work_Cascade_Should_Calculate_Total()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                SelectedWorkTask = "Montaz",
                SelectedWorkSpecification = "Kabel",
                SelectedBaseMaterial = "Beton",
                SelectedWorkPosition = "Strop",
                Quantity = 2
            };

            Assert.That(vm.Total, Is.EqualTo(390d).Within(0.0001));
        }

        [Test]
        public void Work_Cascade_Should_Preserve_Each_Selected_Display_Name()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                SelectedWorkTask = "Montaz",
                SelectedWorkSpecification = "Kabel",
                SelectedBaseMaterial = "Beton",
                SelectedWorkPosition = "Strop"
            };

            Assert.Multiple(() =>
            {
                Assert.That(vm.SelectedWorkTask, Is.EqualTo("Montaz"));
                Assert.That(vm.SelectedWorkSpecification, Is.EqualTo("Kabel"));
                Assert.That(vm.SelectedBaseMaterial, Is.EqualTo("Beton"));
                Assert.That(vm.SelectedWorkPosition, Is.EqualTo("Strop"));
            });
        }

        [Test]
        public void Work_Cascade_Should_Enable_Selections_Sequentially()
        {
            var vm = new CalculationItemViewModel(_db);

            Assert.That(vm.CanSelectWorkSpecification, Is.False);
            Assert.That(vm.CanSelectBaseMaterial, Is.False);
            Assert.That(vm.CanSelectWorkPosition, Is.False);

            vm.SelectedWorkTask = "Montaz";
            vm.SelectedWorkSpecification = "Kabel";
            vm.SelectedBaseMaterial = "Beton";

            Assert.That(vm.CanSelectWorkSpecification, Is.True);
            Assert.That(vm.CanSelectBaseMaterial, Is.True);
            Assert.That(vm.CanSelectWorkPosition, Is.True);
        }
    }
}
