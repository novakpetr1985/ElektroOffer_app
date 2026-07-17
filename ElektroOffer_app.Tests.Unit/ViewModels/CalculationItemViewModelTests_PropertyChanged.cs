using System.Collections.Generic;
using ElektroOffer_app.Models;
using ElektroOffer_app.ViewModels.Items;
using NUnit.Framework;

namespace ElektroOffer_app.Tests.Unit.ViewModels
{
    [TestFixture]
    /// <summary>Ověřuje notifikace změn a přepočet závislých vlastností řádku.</summary>
    public class CalculationItemViewModelTests_PropertyChanged : TestBase
    {
        [Test]
        public void Quantity_Should_Raise_Total_And_IsEmpty()
        {
            var vm = new CalculationItemViewModel(_db);
            var changed = new List<string?>();
            vm.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

            vm.Quantity = 2;

            Assert.That(changed, Does.Contain(nameof(vm.Quantity)));
            Assert.That(changed, Does.Contain(nameof(vm.Total)));
            Assert.That(changed, Does.Contain(nameof(vm.IsEmpty)));
        }

        [Test]
        public void SelectedWorkTask_Should_Raise_Total_IsEmpty_And_CanSelectWorkSpecification()
        {
            var vm = new CalculationItemViewModel(_db);
            var changed = new List<string?>();
            vm.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

            vm.SelectedWorkTask = "Montaz";

            Assert.That(changed, Does.Contain(nameof(vm.SelectedWorkTask)));
            Assert.That(changed, Does.Contain(nameof(vm.CanSelectWorkSpecification)));
            Assert.That(changed, Does.Contain(nameof(vm.Total)));
            Assert.That(changed, Does.Contain(nameof(vm.IsEmpty)));
        }

        [Test]
        public void SelectedBaseMaterialEntity_Should_Raise_Total()
        {
            var vm = new CalculationItemViewModel(_db);
            var changed = new List<string?>();
            vm.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

            vm.SelectedBaseMaterialEntity = new BaseMaterial { BaseMaterialCoef = 1.2m };

            Assert.That(changed, Does.Contain(nameof(vm.SelectedBaseMaterialEntity)));
            Assert.That(changed, Does.Contain(nameof(vm.Total)));
        }

        [Test]
        public void SelectedMaterialPrice_Should_Raise_Total()
        {
            var vm = new CalculationItemViewModel(_db);
            var changed = new List<string?>();
            vm.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

            vm.SelectedMaterialPrice = new MaterialPrice { Price = 20m };

            Assert.That(changed, Does.Contain(nameof(vm.SelectedMaterialPrice)));
            Assert.That(changed, Does.Contain(nameof(vm.Total)));
        }
    }
}
