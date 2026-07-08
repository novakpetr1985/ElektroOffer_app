using ElektroOffer_app.Data;
using ElektroOffer_app.Models;
using ElektroOffer_app.ViewModels.Items;
using Microsoft.EntityFrameworkCore; // potřebné pro .Include()
using NUnit.Framework;

namespace ElektroOffer_app.Tests.Unit.ViewModels
{
    // =====================================================================
    // 🧱 UNIT TESTS – CalculationItemViewModel – KASKÁDA MATERIÁLU
    // =====================================================================
    // Tento soubor obsahuje testy ověřující produktovou kaskádu pro MATERIÁL:
    //
    //   Category → ProductName → Supplier → Offer → SelectedMaterialPrice
    //
    // Zaměřuje se na:
    //   • ResetBelowX metody (MaterialCascadeService)
    //   • PropertyChanged pro CanSelectX vlastnosti
    //   • správné dohledání MaterialPrice z DB po vyplnění celé kaskády
    //
    // Rozsah testů v tomto souboru:
    //   T_080–T_086
    //
    // Sdílený databázový kontext (_db) a SetUp/TearDown jsou definované
    // v TestBase.cs, ze kterého tato třída dědí.
    // =====================================================================
    [TestFixture]
    public class CalculationItemViewModelTests_CascadeMaterial : TestBase
    {

        // -----------------------------------------------------------------
        // 🧪 TEST 01: ResetBelowCategory – musí vymazat Names, Suppliers, Offers
        // -----------------------------------------------------------------
        // Co testujeme:
        //  • že ResetBelowCategory vymaže všechny kroky produktové kaskády
        // -----------------------------------------------------------------
        [Test]
        [Order(01)]
        public void T_01_CascadeMaterial_ResetBelowCategory_Should_Clear_Names_Suppliers_Offers()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                SelectedCategory = "Kabely",
                SelectedProductName = "CYKY 3x2.5",
                SelectedSupplier = "ElektroSupplier",
                SelectedOffer = "Nabídka A"
            };

            vm.SelectedCategory = "Jiná kategorie"; // vyvolá ResetBelowCategory

            Assert.IsNull(vm.SelectedProductName);
            Assert.IsNull(vm.SelectedSupplier);
            Assert.IsNull(vm.SelectedOffer);
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 02: ResetBelowMaterialName – musí vymazat Suppliers + Offers
        // -----------------------------------------------------------------
        [Test]
        [Order(02)]
        public void T_02_CascadeMaterial_ResetBelowMaterialName_Should_Clear_Suppliers_And_Offers()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                SelectedCategory = "Kabely",
                SelectedProductName = "CYKY 3x2.5",
                SelectedSupplier = "ElektroSupplier",
                SelectedOffer = "Nabídka A"
            };

            vm.SelectedProductName = "Jiný produkt"; // vyvolá ResetBelowMaterialName

            Assert.IsNull(vm.SelectedSupplier);
            Assert.IsNull(vm.SelectedOffer);
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 03: ResetBelowSupplier – musí vymazat Offers
        // -----------------------------------------------------------------
        [Test]
        [Order(03)]
        public void T_03_CascadeMaterial_ResetBelowSupplier_Should_Clear_Offers()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                SelectedCategory = "Kabely",
                SelectedProductName = "CYKY 3x2.5",
                SelectedSupplier = "ElektroSupplier",
                SelectedOffer = "Nabídka A"
            };

            vm.SelectedSupplier = "Jiný dodavatel"; // vyvolá ResetBelowSupplier

            Assert.IsNull(vm.SelectedOffer);
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 04: SelectedCategory – musí vyvolat PropertyChanged pro CanSelectProductName
        // -----------------------------------------------------------------
        [Test]
        [Order(04)]
        public void T_04_CascadeMaterial_SelectedCategory_Should_Raise_PropertyChanged_For_CanSelectProductName()
        {
            var vm = new CalculationItemViewModel(_db);

            bool raised = false;

            vm.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(vm.CanSelectProductName))
                    raised = true;
            };

            vm.SelectedCategory = "Kabely";

            Assert.IsTrue(raised);
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 05: SelectedProductName – musí vyvolat PropertyChanged pro CanSelectSupplier
        // -----------------------------------------------------------------
        [Test]
        [Order(05)]
        public void T_05_CascadeMaterial_SelectedProductName_Should_Raise_PropertyChanged_For_CanSelectSupplier()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                SelectedCategory = "Kabely"
            };

            bool raised = false;

            vm.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(vm.CanSelectSupplier))
                    raised = true;
            };

            vm.SelectedProductName = "CYKY 3x2.5";

            Assert.IsTrue(raised);
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 06: SelectedSupplier – musí vyvolat PropertyChanged pro CanSelectOffer
        // -----------------------------------------------------------------
        [Test]
        [Order(06)]
        public void T_06_CascadeMaterial_SelectedSupplier_Should_Raise_PropertyChanged_For_CanSelectOffer()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                SelectedCategory = "Kabely",
                SelectedProductName = "CYKY 3x2.5"
            };

            bool raised = false;

            vm.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(vm.CanSelectOffer))
                    raised = true;
            };

            vm.SelectedSupplier = "ElektroSupplier";

            Assert.IsTrue(raised);
        }

        // -----------------------------------------------------------------------------
        // 🧪 TEST 07: SelectedOffer – musí nastavit SelectedMaterialPrice
        // -----------------------------------------------------------------------------
        // Min. co test ověřuje:
        //   • že ViewModel správně reaguje na výběr nabídky (SelectedOffer)
        //   • že po nastavení SelectedOffer se vybere odpovídající MaterialPrice z DB
        //   • že SelectedMaterialPrice není null
        //   • že SelectedMaterialPrice.Price odpovídá ceně uložené v databázi
        // -----------------------------------------------------------------------------
        [Test]
        [Order(07)]
        public void T_07_CascadeMaterial_SelectedOffer_Should_Update_SelectedMaterialPrice()
        {
            // 1) Category
            var category = new Category
            {
                Name = "TestCategory"
            };
            _db.Categories.Add(category);
            _db.SaveChanges();

            // 2) Supplier
            var supplier = new Supplier
            {
                Name = "TestSupplier"
            };
            _db.Suppliers.Add(supplier);
            _db.SaveChanges();

            // 3) Material (musí mít CategoryId, jinak ho kaskáda nenajde)
            var material = new Material
            {
                Name = "TestMaterial",
                CategoryId = category.Id
            };
            _db.Materials.Add(material);
            _db.SaveChanges();

            // 4) MaterialPrice (musí mít MaterialId + SupplierId)
            var materialPrice = new MaterialPrice
            {
                MaterialId = material.Id,
                SupplierId = supplier.Id,
                Price = 100,
                Unit = "ks",
                SupplierName = supplier.Name
            };
            _db.MaterialPrices.Add(materialPrice);
            _db.SaveChanges();

            // 5) Načtení MaterialPrice včetně vazeb
            var mp = _db.MaterialPrices
                .Include(x => x.Material)
                .Include(x => x.Supplier)
                .First();

            // 6) ViewModel – projdeme celou produktovou kaskádu
            var vm = new CalculationItemViewModel(_db)
            {
                SelectedCategory = category.Name,
                SelectedProductName = mp.Material.Name,
                SelectedSupplier = mp.Supplier.Name
            };

            vm.SelectedOffer = mp.SupplierName; // zde se má natáhnout cena

            // 7) Ověření
            Assert.That(vm.SelectedMaterialPrice, Is.Not.Null,
                "SelectedMaterialPrice musí být nastaveno po vyplnění celé kaskády.");

            Assert.That(vm.SelectedMaterialPrice!.Price, Is.EqualTo(100),
                "SelectedMaterialPrice.Price musí odpovídat ceně uložené v DB.");
        }
    }
}