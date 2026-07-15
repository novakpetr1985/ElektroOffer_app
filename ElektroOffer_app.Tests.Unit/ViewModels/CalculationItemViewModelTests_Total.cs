using ElektroOffer_app.Models;
using ElektroOffer_app.ViewModels.Items;
using NUnit.Framework;

namespace ElektroOffer_app.Tests.Unit.ViewModels
{
    [TestFixture]
    public class CalculationItemViewModelTests_Total : TestBase
    {
        private CalculationItemViewModel CreateWorkVm(double quantity = 2)
        {
            return new CalculationItemViewModel(_db)
            {
                SelectedWorkTaskEntity = new WorkTask { BasePrice = 100m },
                SelectedBaseMaterialEntity = new BaseMaterial { BaseMaterialCoef = 1.2m },
                SelectedWorkPositionEntity = new WorkPosition { PositionCoef = 1.5m },
                Quantity = quantity
            };
        }

        [Test]
        public void Total_Should_Calculate_Work_From_New_Cascade_Entities()
        {
            var vm = CreateWorkVm();

            Assert.That(vm.Total, Is.EqualTo(360d).Within(0.0001));
        }

        [Test]
        public void Total_Should_Calculate_Material_From_SelectedMaterialPrice()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                SelectedMaterialPrice = new MaterialPrice { Price = 50m },
                Quantity = 3
            };

            Assert.That(vm.Total, Is.EqualTo(150d).Within(0.0001));
        }

        [Test]
        public void Total_Should_Apply_Discount_To_Work()
        {
            var vm = CreateWorkVm();
            vm.IsDiscountEnabled = true;
            vm.DiscountPercent = 10;

            Assert.That(vm.Total, Is.EqualTo(324d).Within(0.0001));
        }

        [Test]
        public void Total_Should_Apply_Discount_To_Material()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                SelectedMaterialPrice = new MaterialPrice { Price = 50m },
                Quantity = 2,
                IsDiscountEnabled = true,
                DiscountPercent = 25
            };

            Assert.That(vm.Total, Is.EqualTo(75d).Within(0.0001));
        }

        [Test]
        public void Total_Should_Be_Zero_When_Work_Selection_Is_Incomplete()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                SelectedWorkTaskEntity = new WorkTask { BasePrice = 100m },
                Quantity = 2
            };

            Assert.That(vm.Total, Is.EqualTo(0d));
        }
    }
}
