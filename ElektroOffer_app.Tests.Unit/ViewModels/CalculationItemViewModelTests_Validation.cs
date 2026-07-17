using ElektroOffer_app.Data;
using ElektroOffer_app.Models;
using ElektroOffer_app.ViewModels.Items;
using NUnit.Framework;

namespace ElektroOffer_app.Tests.Unit.ViewModels
{
    // =====================================================================
    // ✅ UNIT TESTS – CalculationItemViewModel – VALIDACE VSTUPŮ
    // =====================================================================
    // Tento soubor obsahuje testy ověřující ochranná pravidla (clamping)
    // na vstupních vlastnostech CalculationItemViewModel:
    //
    //   • Quantity nikdy nesmí být záporné (clamp na 0)
    //   • DiscountPercent musí zůstat v rozsahu 0–100 (clamp)
    //   • SelectedMaterialPrice má bezpečný fallback při nevalidní hodnotě
    //
    // Rozsah testů v tomto souboru:
    //   T_01–T_04
    //
    // Sdílený databázový kontext (_db) a SetUp/TearDown jsou definované
    // v TestBase.cs, ze kterého tato třída dědí.
    // =====================================================================
    [TestFixture]
    /// <summary>Ověřuje validaci množství, slev a povinných voleb kalkulačního řádku.</summary>
    public class CalculationItemViewModelTests_Validation : TestBase
    {
        
        // -----------------------------------------------------------------
        // 🧪 TEST 097: Quantity – záporné množství se má přepnout na 0
        // -----------------------------------------------------------------
        // Ověřuje:
        //   • že záporné množství neprojde do výpočtu
        //   • že ViewModel automaticky nastaví Quantity = 0
        // -----------------------------------------------------------------
        [Test]
        [Order(097)]
        public void T_097_Validation_Quantity_Should_Clamp_Negative_To_Zero()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                Quantity = -5
            };

            Assert.AreEqual(0, vm.Quantity);
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 098: DiscountPercent – hodnoty nad 100 % se mají přepnout na 100
        // -----------------------------------------------------------------
        // Ověřuje:
        //   • že sleva nemůže být vyšší než 100 %
        //   • že ViewModel automaticky nastaví DiscountPercent = 100
        // -----------------------------------------------------------------
        [Test]
        [Order(098)]
        public void T_098_Validation_DiscountPercent_Should_Clamp_Above_100_To_100()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                IsDiscountEnabled = true,
                DiscountPercent = 150
            };

            Assert.AreEqual(100, vm.DiscountPercent);
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 099: DiscountPercent – záporné hodnoty se mají přepnout na 0
        // -----------------------------------------------------------------
        // Ověřuje:
        //   • že sleva nemůže být záporná
        //   • že ViewModel automaticky nastaví DiscountPercent = 0
        // -----------------------------------------------------------------
        [Test]
        [Order(099)]
        public void T_099_Validation_DiscountPercent_Should_Clamp_Negative_To_Zero()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                IsDiscountEnabled = true,
                DiscountPercent = -20
            };

            Assert.AreEqual(0, vm.DiscountPercent);
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 100: SelectedMaterialPrice – fallback při neplatné ceně
        // -----------------------------------------------------------------
        // Ověřuje:
        //   • že ViewModel zvládne neplatnou nebo smazanou cenu
        //   • že Total se vrátí na 0 a nevyhodí výjimku
        // -----------------------------------------------------------------
        [Test]
        [Order(100)]
        public void T_100_Validation_SelectedMaterialPrice_Should_Fallback_When_Invalid()
        {
            var vm = new CalculationItemViewModel(_db);

            vm.SelectedMaterialPrice = null;

            Assert.IsNull(vm.SelectedMaterialPrice);
            Assert.AreEqual(0, vm.Total);
        }
    }
}
