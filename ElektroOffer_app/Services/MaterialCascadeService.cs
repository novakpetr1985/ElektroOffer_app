using System.Linq;
using ElektroOffer_app.Data;
using ElektroOffer_app.Models;
using ElektroOffer_app.ViewModels.Items;

namespace ElektroOffer_app.Services
{
    // =========================================================================
    // 🔗 MaterialCascadeService – kaskádový výběr materiálu, dodavatele a ceny
    // =========================================================================
    //
    // Kaskáda materiálu: Kategorie → Název → Dodavatel → Nabídka.
    // Poslední krok zobrazuje název položky od dodavatele; cena a jednotka
    // se načtou z MaterialPrice pro výpočet řádku.
    // =========================================================================
    public class MaterialCascadeService
    {
        private readonly AppDbContext _db;

        public MaterialCascadeService(AppDbContext db)
        {
            _db = db;
        }

        // =========================================================
        // KATEGORIE (počáteční seznam, nezávisí na ničem)
        // =========================================================
        //
        // Vrací jen kategorie, které mají alespoň 1 přiřazený Material -
        // prázdná kategorie (bez materiálu) se v seznamu neobjeví,
        // stejně jako u Práce neplatné kombinace prostě neexistují.
        // =========================================================
        public void LoadCategories(CalculationItemViewModel vm)
        {
            vm.AvailableCategories.Clear();

            var list = _db.Materials
                .Where(m => m.Category != null)
                .Select(m => m.Category!.Name)
                .Distinct()
                .OrderBy(n => n)
                .ToList();

            foreach (var name in list)
                vm.AvailableCategories.Add(name);
        }

        // =========================================================
        // NÁZVY MATERIÁLU (filtrované podle vybrané Kategorie)
        // =========================================================
        public void LoadMaterialNames(CalculationItemViewModel vm)
        {
            vm.AvailableMaterialNames.Clear();

            if (string.IsNullOrWhiteSpace(vm.SelectedCategory))
                return;

            var list = _db.Materials
                .Where(m => m.Category != null && m.Category.Name == vm.SelectedCategory)
                .Select(m => m.Name)
                .Distinct()
                .OrderBy(n => n)
                .ToList();

            foreach (var name in list)
                vm.AvailableMaterialNames.Add(name);
        }

        // =========================================================
        // DODAVATELÉ (jen ti, co mají cenu pro vybraný Nazev)
        // =========================================================
        public void LoadSuppliers(CalculationItemViewModel vm)
        {
            vm.AvailableSuppliers.Clear();

            if (string.IsNullOrWhiteSpace(vm.SelectedProductName))
                return;

            var list = _db.MaterialPrices
                .Where(mp => mp.Material.Name == vm.SelectedProductName)
                .Select(mp => mp.Supplier.Name)
                .Distinct()
                .OrderBy(n => n)
                .ToList();

            foreach (var supplierName in list)
                vm.AvailableSuppliers.Add(supplierName);
        }

        // =========================================================
        // MATERIÁL (název položky OD DODAVATELE - dřív "Nabídka")
        // =========================================================
        //
        // ZMĚNA: zobrazuje se JEN SupplierName (přesný název od
        // dodavatele), BEZ kódu a BEZ ceny. Kód a cena zůstávají
        // v MaterialPrice a zobrazí se až v detailním rozpočtu.
        //
        // Pokud je pro dvojici Nazev+Dodavatel jen JEDNA položka,
        // vybere se automaticky - uživatel nemusí nic navíc klikat.
        // =========================================================
        public void LoadOffers(CalculationItemViewModel vm)
        {
            vm.AvailableOffers.Clear();

            if (string.IsNullOrWhiteSpace(vm.SelectedProductName) ||
                string.IsNullOrWhiteSpace(vm.SelectedSupplier))
                return;

            var nazvy = _db.MaterialPrices
                .Where(mp => mp.Material.Name == vm.SelectedProductName &&
                             mp.Supplier.Name == vm.SelectedSupplier)
                .Select(mp => mp.SupplierName)
                .Distinct()
                .OrderBy(n => n)
                .ToList();

            foreach (var nazev in nazvy)
                vm.AvailableOffers.Add(nazev);

            // Pokud je jen jedna možnost, vybereme ji automaticky
            if (vm.AvailableOffers.Count == 1)
                vm.SelectedOffer = vm.AvailableOffers[0];
        }

        // =========================================================
        // VÝSLEDNÁ CENA (podle finálně vybraného Materiálu/SupplierName)
        // =========================================================
        public void UpdateSelectedPrice(CalculationItemViewModel vm)
        {
            if (string.IsNullOrWhiteSpace(vm.SelectedProductName) ||
                string.IsNullOrWhiteSpace(vm.SelectedSupplier) ||
                string.IsNullOrWhiteSpace(vm.SelectedOffer))
            {
                vm.SelectedMaterialPrice = null;
                return;
            }

            vm.SelectedMaterialPrice = _db.MaterialPrices
                .FirstOrDefault(mp =>
                    mp.Material.Name == vm.SelectedProductName &&
                    mp.Supplier.Name == vm.SelectedSupplier &&
                    mp.SupplierName == vm.SelectedOffer);
        }

        // =========================================================
        // RESETY (stejný vzor jako ResetBelowTask/Specification/Material)
        // =========================================================

        public void ResetBelowCategory(CalculationItemViewModel vm)
        {
            vm.SelectedProductName = null;
            vm.SelectedSupplier = null;
            vm.SelectedOffer = null;

            vm.AvailableMaterialNames.Clear();
            vm.AvailableSuppliers.Clear();
            vm.AvailableOffers.Clear();

            vm.SelectedMaterialPrice = null;
        }

        public void ResetBelowMaterialName(CalculationItemViewModel vm)
        {
            vm.SelectedSupplier = null;
            vm.SelectedOffer = null;

            vm.AvailableSuppliers.Clear();
            vm.AvailableOffers.Clear();

            vm.SelectedMaterialPrice = null;
        }

        public void ResetBelowSupplier(CalculationItemViewModel vm)
        {
            vm.SelectedOffer = null;
            vm.AvailableOffers.Clear();
            vm.SelectedMaterialPrice = null;
        }
    }
}
