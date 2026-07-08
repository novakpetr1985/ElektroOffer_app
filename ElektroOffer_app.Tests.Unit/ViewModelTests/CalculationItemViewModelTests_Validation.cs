using ElektroOffer_app.Data;
using ElektroOffer_app.Models;
using ElektroOffer_app.ViewModels.Items;
using NUnit.Framework;

namespace ElektroOffer_app.Tests.Unit.ViewModels
{
    // =====================================================================
    // ✅ UNIT TESTS – CalculationItemViewModel – VALIDACE VSTUPŮ
    // =====================================================================
    // Tato část třídy (partial) ověřuje ochranná pravidla (clamping) na
    // vstupních vlastnostech:
    //   • Quantity nikdy nesmí být záporné (clamp na 0)
    //   • DiscountPercent musí zůstat v rozsahu 0–100 (clamp)
    //   • SelectedMaterialPrice má bezpečný fallback při nevalidní hodnotě
    //
    // Rozsah testů v tomto souboru:
    //   T_056–T_059
    //
    // Setup/TearDown a sdílená pole _db/_connection jsou definované
    // v CalculationItemViewModelTests_Base.cs (partial class).
    // =====================================================================
    public partial class CalculationItemViewModelTests
    {

        // -----------------------------------------------------------------
        // 🧪 TEST 01: Quantity – záporné množství se má přepnout na 0
        // -----------------------------------------------------------------
        // Co testujeme:
        //  • že záporné množství neprojde do výpočtu
        //  • že ViewModel automaticky nastaví Quantity = 0
        // -----------------------------------------------------------------
        [Test]
        [Order(01)]
        public void T_01_Validation_Quantity_Should_Clamp_Negative_To_Zero()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                Quantity = -5
            };

            Assert.AreEqual(0, vm.Quantity);
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 02: DiscountPercent – hodnoty nad 100 % se mají přepnout na 100
        // -----------------------------------------------------------------
        // Co testujeme:
        //  • že sleva nemůže být vyšší než 100 %
        //  • že ViewModel automaticky nastaví DiscountPercent = 100
        // -----------------------------------------------------------------
        [Test]
        [Order(02)]
        public void T_02_Validation_DiscountPercent_Should_Clamp_Above_100_To_100()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                IsDiscountEnabled = true,
                DiscountPercent = 150
            };

            Assert.AreEqual(100, vm.DiscountPercent);
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 03: DiscountPercent – záporné hodnoty se mají přepnout na 0
        // -----------------------------------------------------------------
        // Co testujeme:
        //  • že sleva nemůže být záporná
        //  • že ViewModel automaticky nastaví DiscountPercent = 0
        // -----------------------------------------------------------------
        [Test]
        [Order(03)]
        public void T_03_Validation_DiscountPercent_Should_Clamp_Negative_To_Zero()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                IsDiscountEnabled = true,
                DiscountPercent = -20
            };

            Assert.AreEqual(0, vm.DiscountPercent);
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 04: SelectedMaterialPrice – fallback při neplatné ceně
        // -----------------------------------------------------------------
        // Co testujeme:
        //  • že ViewModel zvládne neplatnou nebo smazanou cenu
        //  • že Total se vrátí na 0 a nevyhodí výjimku
        // -----------------------------------------------------------------
        [Test]
        [Order(04)]
        public void T_04_Validation_SelectedMaterialPrice_Should_Fallback_When_Invalid()
        {
            var vm = new CalculationItemViewModel(_db);

            vm.SelectedMaterialPrice = null;

            Assert.IsNull(vm.SelectedMaterialPrice);
            Assert.AreEqual(0, vm.Total);
        }
    }
}