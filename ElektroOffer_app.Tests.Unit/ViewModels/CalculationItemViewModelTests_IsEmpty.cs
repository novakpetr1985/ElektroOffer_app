using ElektroOffer_app.Models;
using ElektroOffer_app.ViewModels.Items;
using NUnit.Framework;

namespace ElektroOffer_app.Tests.Unit.ViewModels
{
    [TestFixture]
    /// <summary>Ověřuje rozpoznání prázdných a částečně vyplněných řádků.</summary>
    public class CalculationItemViewModelTests_IsEmpty : TestBase
    {
        [Test]
        public void IsEmpty_Should_Return_True_For_Default_Row()
        {
            var vm = new CalculationItemViewModel(_db);

            Assert.That(vm.IsEmpty, Is.True);
        }

        [Test]
        public void IsEmpty_Should_Return_False_When_WorkTask_Is_Selected()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                SelectedWorkTask = "Montaz"
            };

            Assert.That(vm.IsEmpty, Is.False);
        }

        [Test]
        public void IsEmpty_Should_Return_False_When_BaseMaterial_Is_Selected()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                SelectedBaseMaterial = "Cihla"
            };

            Assert.That(vm.IsEmpty, Is.False);
        }

        [Test]
        public void IsEmpty_Should_Return_False_When_WorkPosition_Is_Selected()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                SelectedWorkPosition = "Stena"
            };

            Assert.That(vm.IsEmpty, Is.False);
        }

        [Test]
        public void IsEmpty_Should_Return_False_When_MaterialPrice_Is_Selected()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                SelectedMaterialPrice = new MaterialPrice { Price = 50m }
            };

            Assert.That(vm.IsEmpty, Is.False);
        }
    }
}
