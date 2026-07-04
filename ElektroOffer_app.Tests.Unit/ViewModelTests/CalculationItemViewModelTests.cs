using NUnit.Framework;
using ElektroOffer_app.ViewModels.Items;
using ElektroOffer_app.Models;

namespace ElektroOffer_app.Tests.Unit.ViewModels
{
    // =====================================================================
    // 🧮 UNIT TESTS – CalculationItemViewModel
    // =====================================================================
    // Testujeme čistou logiku výpočtů:
    //   • Total pro práci (WorkItem)
    //   • Total pro materiál (MaterialItem)
    //   • aplikaci slevy
    //   • reakci na změnu Quantity
    //   • reset slevy při vypnutí IsDiscountEnabled
    //
    // Bez databáze, bez WPF, jen ViewModel.
    // =====================================================================
    public class CalculationItemViewModelTests
    {
        // -----------------------------------------------------------------
        // 🧪 TEST 1: Výpočet ceny práce (WorkItem)
        // -----------------------------------------------------------------
        [Test]
        public void Total_Should_Calculate_WorkItem_Correctly()
        {
            // 📝 Arrange – připravíme ViewModel s WorkItem
            var vm = new CalculationItemViewModel
            {
                Quantity = 10
            };

            // PriceItems = záznam z tabulky ceníku práce
            vm.WorkItem = new PriceItems
            {
                BasePrice = 150,      // základní cena
                MaterialCoef = 1.2,   // koeficient materiálu
                PositionCoef = 1.0    // koeficient pozice
            };

            // 🔧 Act – získáme Total
            var total = vm.Total;

            // 150 × 1,2 × 1,0 × 10 = 1 800
            Assert.AreEqual(1800, total, 0.001);
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 2: Výpočet ceny materiálu (MaterialItem)
        // -----------------------------------------------------------------
        [Test]
        public void Total_Should_Calculate_MaterialItem_When_WorkItem_Is_Null()
        {
            var vm = new CalculationItemViewModel
            {
                Quantity = 5,
                WorkItem = null,
                MaterialItem = new Material { Price = 200 }
            };

            // 200 × 5 = 1 000
            Assert.AreEqual(1000, vm.Total, 0.001);
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 3: Sleva se správně aplikuje
        // -----------------------------------------------------------------
        [Test]
        public void Total_Should_Apply_Discount()
        {
            var vm = new CalculationItemViewModel
            {
                Quantity = 2,
                IsDiscountEnabled = true,
                DiscountPercent = 10
            };

            vm.WorkItem = new PriceItems
            {
                BasePrice = 500,
                MaterialCoef = 1,
                PositionCoef = 1
            };

            // baseTotal = 500 × 1 × 1 × 2 = 1 000
            // sleva 10 % → 1 000 × 0,9 = 900
            Assert.AreEqual(900, vm.Total, 0.001);
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 4: Sleva se NEaplikuje, pokud IsDiscountEnabled = false
        // -----------------------------------------------------------------
        [Test]
        public void Total_Should_Not_Apply_Discount_When_Disabled()
        {
            var vm = new CalculationItemViewModel
            {
                Quantity = 2,
                IsDiscountEnabled = false,
                DiscountPercent = 10
            };

            vm.WorkItem = new PriceItems
            {
                BasePrice = 500,
                MaterialCoef = 1,
                PositionCoef = 1
            };

            // Bez slevy → 500 × 1 × 1 × 2 = 1 000
            Assert.AreEqual(1000, vm.Total, 0.001);
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 5: Vypnutí slevy vynuluje DiscountPercent
        // -----------------------------------------------------------------
        [Test]
        public void IsDiscountEnabled_False_Should_Reset_DiscountPercent()
        {
            var vm = new CalculationItemViewModel
            {
                Quantity = 1,
                IsDiscountEnabled = true,
                DiscountPercent = 15
            };

            // 🔧 Act – vypneme slevu
            vm.IsDiscountEnabled = false;

            // DiscountPercent musí být null
            Assert.IsNull(vm.DiscountPercent);
        }
    }
}
