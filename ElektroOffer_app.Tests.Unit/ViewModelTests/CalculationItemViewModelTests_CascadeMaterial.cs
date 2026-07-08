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
    // Tato část třídy (partial) ověřuje produktovou kaskádu pro MATERIÁL:
    //   Category → ProductName → Supplier → Offer → SelectedMaterialPrice
    //
    // Zaměřuje se na:
    //   • ResetBelowX metody (MaterialCascadeService) – že se při změně
    //     vyšší úrovně vyčistí všechny nižší úrovně (Names/Suppliers/Offers)
    //   • PropertyChanged pro CanSelectX vlastnosti (např. CanSelectProductName)
    //   • že se po vyplnění celé kaskády včetně Offer správně dohledá
    //     odpovídající MaterialPrice záznam z DB (UpdateSelectedPrice)
    //
    // Rozsah testů v tomto souboru:
    //   T_080–T_086
    //
    // Setup/TearDown a sdílená pole _db/_connection jsou definované
    // v CalculationItemViewModelTests_Base.cs (partial class).
    // =====================================================================
    public partial class CalculationItemViewModelTests
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
        // Co tento test ověřuje:
        //   • že ViewModel správně reaguje na výběr nabídky (SelectedOffer)
        //   • že po nastavení SelectedOffer se vybere odpovídající MaterialPrice z databáze
        //   • že SelectedMaterialPrice není null
        //   • že SelectedMaterialPrice obsahuje správnou cenu
        //
        // Jak test funguje:
        //   • vytvoří Category, Supplier a Material a uloží je do databáze
        //   • vytvoří MaterialPrice odkazující na Material (MaterialId) a Supplier (SupplierId)
        //   • projede CELOU produktovou kaskádu ve ViewModelu:
        //       SelectedCategory → SelectedProductName → SelectedSupplier → SelectedOffer
        //   • ověří, že SelectedMaterialPrice je správně načtený z databáze
        //
        // 🔥 OPRAVA:
        //   • UpdateSelectedPrice() hledá MaterialPrice podle SelectedProductName +
        //     SelectedSupplier + SelectedOffer (NE podle SelectedMaterial – to je
        //     vlastnost z kaskády PRÁCE/PriceItems, jiná kaskáda).
        //   • Test dřív nastavoval jen SelectedMaterial, takže SelectedProductName
        //     a SelectedSupplier zůstaly null a UpdateSelectedPrice() rovnou vracel null.
        //   • Teď se nastavuje celá produktová kaskáda a Material má i CategoryId,
        //     aby ho LoadMaterialNames/LoadSuppliers vůbec našly.
        //
        // Důležité:
        //   • Test vyžaduje, aby testovací databáze měla vytvořené tabulky
        //     (Categories, Materials, Suppliers, MaterialPrices). To zajistí
        //     EnsureCreated() v testovací fixture.
        // -----------------------------------------------------------------------------
        [Test]
        [Order(07)]
        public void T_07_CascadeMaterial_SelectedOffer_Should_Update_SelectedMaterialPrice()
        {
            // 1) Uložíme Category
            var category = new Category
            {
                Name = "TestCategory"
            };
            _db.Categories.Add(category);
            _db.SaveChanges();

            // 2) Uložíme Supplier
            var supplier = new Supplier
            {
                Name = "TestSupplier"
            };
            _db.Suppliers.Add(supplier);
            _db.SaveChanges(); // 🔥 nutné kvůli FK

            // 3) Uložíme Material s vazbou na Category
            var material = new Material
            {
                Name = "TestMaterial",
                CategoryId = category.Id   // 🔥 OPRAVA – aby LoadMaterialNames našel materiál v kategorii
            };
            _db.Materials.Add(material);
            _db.SaveChanges(); // 🔥 nutné kvůli FK

            // 4) Uložíme MaterialPrice s MaterialId i SupplierId
            var materialPrice = new MaterialPrice
            {
                MaterialId = material.Id,   // 🔥 validní FK
                SupplierId = supplier.Id,   // 🔥 validní FK
                Price = 100,
                Unit = "ks",
                SupplierName = "TestSupplier"
            };
            _db.MaterialPrices.Add(materialPrice);
            _db.SaveChanges();

            // 5) Načteme MaterialPrice včetně Material a Supplier
            var mp = _db.MaterialPrices
                .Include(x => x.Material)
                .Include(x => x.Supplier)
                .First();

            // 6) ViewModel – projedeme CELOU produktovou kaskádu
            var vm = new CalculationItemViewModel(_db)
            {
                SelectedCategory = category.Name,       // 🔥 OPRAVA – 1. krok kaskády
                SelectedProductName = mp.Material.Name,  // 🔥 OPRAVA – 2. krok kaskády
                SelectedSupplier = mp.Supplier.Name      // 🔥 OPRAVA – 3. krok kaskády
            };

            vm.SelectedOffer = mp.SupplierName;          // 4. krok – tady se má natáhnout cena

            // 7) Ověření
            Assert.That(vm.SelectedMaterialPrice, Is.Not.Null);
            Assert.That(vm.SelectedMaterialPrice.Price, Is.EqualTo(100));
        }
    }
}