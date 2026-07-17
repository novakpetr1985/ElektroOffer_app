using System.Linq;
using ElektroOffer_app.Models;
using ElektroOffer_app.ViewModels.Items;
using NUnit.Framework;

namespace ElektroOffer_app.Tests.Unit.ViewModels
{
    [TestFixture]
    /// <summary>Ověřuje reset a doplňování navazujících voleb práce.</summary>
    public class CalculationItemViewModelTests_CascadeWork : TestBase
    {
        private void SeedWorkCatalog()
        {
            var task = new WorkTask { Name = "Montaz", BasePrice = 100m };
            var otherTask = new WorkTask { Name = "Demontaz", BasePrice = 50m };
            var specification = new WorkSpecification { Name = "Zasuvka", Unit = "ks" };
            var otherSpecification = new WorkSpecification { Name = "Kabel", Unit = "m" };

            _db.Tasks.AddRange(task, otherTask);
            _db.Specifications.AddRange(specification, otherSpecification);
            _db.BaseMaterials.Add(new BaseMaterial { Name = "Cihla", BaseMaterialCoef = 1.2m });
            _db.Positions.Add(new WorkPosition { Name = "Stena", PositionCoef = 1.1m });
            _db.SaveChanges();

            _db.TaskSpecifications.AddRange(
                new TaskSpecification { TaskId = task.Id, SpecificationId = specification.Id },
                new TaskSpecification { TaskId = task.Id, SpecificationId = otherSpecification.Id });
            _db.SaveChanges();
        }

        [Test]
        public void SelectedWorkTask_Should_Reset_All_Lower_Work_Selections()
        {
            SeedWorkCatalog();
            var vm = new CalculationItemViewModel(_db)
            {
                SelectedWorkSpecification = "Zasuvka",
                SelectedBaseMaterial = "Cihla",
                SelectedWorkPosition = "Stena"
            };

            vm.SelectedWorkTask = "Montaz";

            Assert.That(vm.SelectedWorkSpecification, Is.Null);
            Assert.That(vm.SelectedBaseMaterial, Is.Null);
            Assert.That(vm.SelectedWorkPosition, Is.Null);
        }

        [Test]
        public void SelectedWorkTask_Should_Load_Available_WorkSpecifications()
        {
            SeedWorkCatalog();
            var vm = new CalculationItemViewModel(_db);

            vm.SelectedWorkTask = "Montaz";

            Assert.That(vm.AvailableWorkSpecifications, Has.Count.EqualTo(2));
            Assert.That(vm.AvailableWorkSpecifications.ToArray(), Does.Contain("Zasuvka"));
            Assert.That(vm.AvailableWorkSpecifications.ToArray(), Does.Contain("Kabel"));
        }

        [Test]
        public void WorkSelections_Should_Resolve_Entities_For_Total()
        {
            SeedWorkCatalog();
            var vm = new CalculationItemViewModel(_db)
            {
                SelectedWorkTask = "Montaz",
                SelectedBaseMaterial = "Cihla",
                SelectedWorkPosition = "Stena",
                Quantity = 2
            };

            Assert.That(vm.SelectedWorkTaskEntity, Is.Not.Null);
            Assert.That(vm.SelectedBaseMaterialEntity, Is.Not.Null);
            Assert.That(vm.SelectedWorkPositionEntity, Is.Not.Null);
            Assert.That(vm.Total, Is.EqualTo(264d).Within(0.0001));
        }

        [Test]
        public void SelectedWorkTask_Should_Raise_CanSelectWorkSpecification()
        {
            var vm = new CalculationItemViewModel(_db);
            var raised = false;

            vm.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(vm.CanSelectWorkSpecification))
                    raised = true;
            };

            vm.SelectedWorkTask = "Montaz";

            Assert.That(raised, Is.True);
        }

        [Test]
        public void WorkCascade_Should_Enable_Lower_Selections_Sequentially()
        {
            var vm = new CalculationItemViewModel(_db);

            Assert.That(vm.CanSelectWorkSpecification, Is.False);
            Assert.That(vm.CanSelectBaseMaterial, Is.False);
            Assert.That(vm.CanSelectWorkPosition, Is.False);

            vm.SelectedWorkTask = "Montaz";
            Assert.That(vm.CanSelectWorkSpecification, Is.True);
            Assert.That(vm.CanSelectBaseMaterial, Is.False);

            vm.SelectedWorkSpecification = "Zasuvka";
            Assert.That(vm.CanSelectBaseMaterial, Is.True);
            Assert.That(vm.CanSelectWorkPosition, Is.False);

            vm.SelectedBaseMaterial = "Cihla";
            Assert.That(vm.CanSelectWorkPosition, Is.True);
        }
    }
}
