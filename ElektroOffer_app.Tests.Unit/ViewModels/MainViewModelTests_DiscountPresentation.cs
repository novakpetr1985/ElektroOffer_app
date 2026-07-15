using ElektroOffer_app.Models;
using ElektroOffer_app.Services;
using ElektroOffer_app.ViewModels;
using Moq;
using NUnit.Framework;

namespace ElektroOffer_app.Tests.Unit.ViewModels
{
    [TestFixture]
    public class MainViewModelTests_DiscountPresentation : TestBase
    {
        [Test]
        public void Recalculate_Should_Keep_Prices_Before_And_After_Discount_Separate()
        {
            var productionAssembly = typeof(MainViewModel).Assembly;
            var projectServiceType = productionAssembly.GetType("ElektroOffer_app.Services.ProjectService")!;
            var projectService = Activator.CreateInstance(projectServiceType)!;
            var vm = (MainViewModel)Activator.CreateInstance(
                typeof(MainViewModel),
                projectService,
                new CatalogService(),
                new CalculationPriceService(),
                _db,
                Mock.Of<IMessageService>(),
                Mock.Of<IPrintService>(),
                Mock.Of<IApplicationService>(),
                Mock.Of<IWindowService>())!;

            var item = vm.WorkCalcItems[0];
            item.SelectedWorkTaskEntity = new WorkTask { BasePrice = 960m };
            item.SelectedBaseMaterialEntity = new BaseMaterial { BaseMaterialCoef = 1m };
            item.SelectedWorkPositionEntity = new WorkPosition { PositionCoef = 1m };
            item.Quantity = 1;
            item.IsDiscountEnabled = true;
            item.DiscountPercent = 10;

            vm.Recalculate();

            var budgetItem = vm.BudgetItems.Single();
            Assert.Multiple(() =>
            {
                Assert.That(budgetItem.PriceBeforeDiscount, Is.EqualTo(960d));
                Assert.That(budgetItem.DiscountPercent, Is.EqualTo(10d));
                Assert.That(budgetItem.DiscountAmount, Is.EqualTo(96d).Within(0.0001));
                Assert.That(budgetItem.Price, Is.EqualTo(864d).Within(0.0001));
                Assert.That(vm.WorkTotal, Is.EqualTo(864d).Within(0.0001));
                Assert.That(vm.GrandTotalBeforeDiscount, Is.EqualTo(960d).Within(0.0001));
                Assert.That(vm.TotalDiscount, Is.EqualTo(96d).Within(0.0001));
                Assert.That(vm.GrandTotal, Is.EqualTo(864d).Within(0.0001));
            });
        }
    }
}
