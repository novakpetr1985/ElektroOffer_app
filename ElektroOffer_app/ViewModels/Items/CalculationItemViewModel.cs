using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ElektroOffer_app.Data;
using ElektroOffer_app.Models;
using ElektroOffer_app.Services;

namespace ElektroOffer_app.ViewModels.Items
{
    public class CalculationItemViewModel : INotifyPropertyChanged
    {
        // =====================================================================
        // DEPENDENCY SERVICES
        // ---------------------------------------------------------------------
        // CalculationItemViewModel používá tři moderní služby:
        //
        // • MaterialCascadeService
        //      – produktová kaskáda materiálu (Category → Name → Supplier → Offer)
        //
        // • WorkCascadeService
        //      – typově bezpečná kaskáda práce (WorkTask → WorkSpecification → BaseMaterial → Position)
        //
        // • CalculationPriceService
        //      – výpočet ceny práce i materiálu (včetně slevy)
        //
        // Poznámka:
        // ---------
        // Stará textová kaskáda práce (Task → Specification → Material → Location)
        // byla kompletně odstraněna. CalculationCascadeService se již nepoužívá.
        // =====================================================================

        private readonly AppDbContext _db;
        private readonly MaterialCascadeService _materialCascade;      // produktová kaskáda materiálu
        private readonly CalculationPriceService _price;               // výpočet ceny práce + materiálu

        // 🔴 NOVÉ – typově bezpečná kaskáda Práce (WorkTask → WorkSpecification → BaseMaterial → Position)
        private readonly WorkCascadeService _workCascade;

        public CalculationItemViewModel(AppDbContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _materialCascade = new MaterialCascadeService(db);
            _price = new CalculationPriceService();

            // 🔴 NOVÉ – moderní kaskáda práce
            _workCascade = new WorkCascadeService(db);

            // =====================================================================
            // MATERIÁL – načtení prvního kroku produktové kaskády
            // =====================================================================
            _materialCascade.LoadCategories(this);

            // =====================================================================
            // PRÁCE – načtení nezávislých seznamů nové kaskády
            // ---------------------------------------------------------------------
            // • WorkTask je první krok (úkon)
            // • BaseMaterial a Position jsou nezávislé seznamy → načítají se ihned
            // =====================================================================
            _workCascade.LoadTasks(this);
            _workCascade.LoadBaseMaterials(this);
            _workCascade.LoadPositions(this);
        }

        public static CalculationItemViewModel CreateDefault()
            => new CalculationItemViewModel(new AppDbContext());


        // =========================================================
        // FIELDS
        // =========================================================

        private double _quantity;            // množství (společné pro práci i materiál)

        // sleva (společné)
        private bool _isDiscountEnabled = false;
        private double? _discountPercent = null;

        // =====================================================================
        // 🆔 Id – propojení PRÁCE/MATERIÁL ↔ SPOLEČNÉ
        // ---------------------------------------------------------------------
        // • Každý řádek má lidsky čitelné ID:
        //       PRÁCE    → W-1, W-2, W-3...
        //       MATERIÁL → M-1, M-2, M-3...
        //
        // • ID se generuje při ukládání projektu v BuildProjectData().
        // • Stejné ID se ukládá do:
        //       WorkItemData.Id
        //       MaterialItemData.Id
        //       CalculationItemData.Id
        //
        // • Díky tomu lze při načítání projektu jednoznačně spárovat:
        //       pracovní položku ↔ společné hodnoty
        //       materiálovou položku ↔ společné hodnoty
        // =====================================================================
        public string Id { get; set; } = string.Empty;

        // =========================================================
        // 💸 SLEVA – společná pro PRÁCI i MATERIÁL
        // =========================================================
        //
        // • IsDiscountEnabled – zapíná/vypíná slevu.
        // • DiscountPercent   – hodnota slevy v procentech.
        // • CalculationPriceService i InvoiceTemplateService tyto
        //   properties vyžadují.
        // • Backing fields jsou definované výše.
        //

        public bool IsDiscountEnabled
        {
            get => _isDiscountEnabled;
            set
            {
                if (_isDiscountEnabled == value)
                    return;

                _isDiscountEnabled = value;

                OnPropertyChanged();
                NotifyCalculatedProperties();
                UpdateTotal();
            }
        }

        public double? DiscountPercent
        {
            get => _discountPercent;
            set
            {
                if (_discountPercent == value)
                    return;

                _discountPercent = value;

                OnPropertyChanged();
                NotifyCalculatedProperties();
                UpdateTotal();
            }
        }

        // =====================================================================
        // MATERIÁL – produktová kaskáda (Category → Name → Supplier → Offer → Price)
        // ---------------------------------------------------------------------
        // Backing fields pro vstupní kroky materiálové kaskády.
        // Tyto hodnoty se nastavují přes veřejné properties níže.
        // =====================================================================
        private string? _selectedCategory;
        private string? _selectedProductName;
        private string? _selectedSupplier;
        private string? _selectedOffer;
        private MaterialPrice? _selectedMaterialPrice;

        // =====================================================================
        // 🔴 NOVÉ – normalizovaná kaskáda Práce
        // ---------------------------------------------------------------------
        // Backing fields pro typově bezpečnou kaskádu práce.
        // =====================================================================
        private WorkTask? _selectedWorkTask;
        private WorkSpecification? _selectedWorkSpecification;
        private BaseMaterial? _selectedBaseMaterial;
        private Position? _selectedPosition;
        private decimal? _calculatedWorkPrice;

        // =========================================================
        // PUBLIC COLLECTIONS (UI)
        // =========================================================
        //
        // Tyto kolekce jsou naplňovány MaterialCascadeService a WorkCascadeService.
        // UI je používá jako ItemsSource pro ComboBoxy.
        // =========================================================

        // Produktová kaskáda materiálu
        public ObservableCollection<string> AvailableCategories { get; } = new();
        public ObservableCollection<string> AvailableMaterialNames { get; } = new();
        public ObservableCollection<string> AvailableSuppliers { get; } = new();
        public ObservableCollection<string> AvailableOffers { get; } = new();

        // 🔴 NOVÉ – normalizovaná kaskáda Práce
        public ObservableCollection<WorkTask> AvailableWorkTasks { get; } = new();
        public ObservableCollection<WorkSpecification> AvailableWorkSpecifications { get; } = new();
        public ObservableCollection<BaseMaterial> AvailableBaseMaterials { get; } = new();
        public ObservableCollection<Position> AvailablePositions { get; } = new();

        // =========================================================
        // CASCADE ENABLE FLAGS (UI)
        // =========================================================
        //
        // UI povoluje ComboBoxy podle toho, zda je vybrán WorkTask.
        // BaseMaterial a Position jsou nezávislé → povolují se ihned po výběru WorkTask.
        // =========================================================

        public bool CanSelectWorkSpecification => SelectedWorkTask != null;
        public bool CanSelectBaseMaterial => SelectedWorkTask != null;
        public bool CanSelectPosition => SelectedWorkTask != null;

        // =========================================================
        // MATERIÁL – ENABLE FLAGS (UI)
        // =========================================================
        // Tyto vlastnosti odemykají ComboBoxy v materiálové sekci.
        // Pokud chybí, ComboBoxy jsou trvale vypnuté.
        // =========================================================

        public bool CanSelectProductName => !string.IsNullOrWhiteSpace(SelectedCategory);
        public bool CanSelectSupplier => !string.IsNullOrWhiteSpace(SelectedProductName);
        public bool CanSelectOffer => !string.IsNullOrWhiteSpace(SelectedSupplier);

        // =========================================================
        // 🔵 MATERIÁL – VSTUPNÍ KROKY PRODUKTOVÉ KASKÁDY
        // =========================================================
        //
        // POZOR: Tento blok MUSÍ být umístěn přesně zde:
        //
        //   ✔ POD WorkUnit (poslední vlastnost práce)
        //   ✔ NAD SelectedMaterialPrice (výstup materiálové kaskády)
        //
        // Důvod:
        //   • SelectedCategory, SelectedProductName, SelectedSupplier, SelectedOffer jsou VSTUPY.
        //   • SelectedMaterialPrice je VÝSTUP.
        //   • Vstupy musí být definované dříve než výstup.
        //
        // Používají je:
        //   • MaterialCascadeService (načítání seznamů)
        //   • ProjectService (serializace)
        //   • IsEmpty (detekce prázdných řádků)
        //   • UI ComboBoxy (IsEnabled + ItemsSource)
        //
        // Pokud tyto properties chybí → vzniká CS0103.
        // =========================================================

        // =========================================================
        // CATEGORY – první krok materiálové kaskády
        // =========================================================
        // 👉 Uživatel vybírá kategorii (např. "Kabely", "Jističe")
        // 👉 Po změně se:
        //    • načtou dostupné názvy materiálu
        //    • shodí všechny nižší kroky (ProductName, Supplier, Offer, Price)
        //    • přepočítá Total
        //    • 🔵 MUSÍ se vyvolat notify pro CanSelectProductName, CanSelectSupplier, CanSelectOffer
        //      jinak se ComboBoxy v UI NEODEMKNOU
        // =========================================================
        public string? SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (_selectedCategory == value) return;
                _selectedCategory = value;

                // Reset nižších úrovní (včetně ceny)
                _materialCascade.ResetBelowCategory(this);

                // Načtení názvů materiálu podle vybrané kategorie
                _materialCascade.LoadMaterialNames(this);

                OnPropertyChanged();

                // 🔵 DOPLNĚNO — odemknutí dalších kroků kaskády
                OnPropertyChanged(nameof(CanSelectProductName));
                OnPropertyChanged(nameof(CanSelectSupplier));
                OnPropertyChanged(nameof(CanSelectOffer));

                NotifyCalculatedProperties();
                UpdateTotal();
            }
        }


        // =========================================================
        // PRODUCT NAME – druhý krok materiálové kaskády
        // =========================================================
        // 👉 Uživatel vybírá konkrétní produkt (např. "CYKY 3×2,5")
        // 👉 Po změně se:
        //    • načtou dostupní dodavatelé
        //    • shodí Supplier + Offer + Price
        //    • 🔵 MUSÍ se vyvolat notify pro CanSelectSupplier, CanSelectOffer
        //      jinak se ComboBoxy v UI NEODEMKNOU
        // =========================================================
        public string? SelectedProductName
        {
            get => _selectedProductName;
            set
            {
                if (_selectedProductName == value) return;
                _selectedProductName = value;

                _materialCascade.ResetBelowMaterialName(this);
                _materialCascade.LoadSuppliers(this);

                OnPropertyChanged();

                // 🔵 DOPLNĚNO — odemknutí dodavatele + nabídky
                OnPropertyChanged(nameof(CanSelectSupplier));
                OnPropertyChanged(nameof(CanSelectOffer));

                NotifyCalculatedProperties();
                UpdateTotal();
            }
        }


        // =========================================================
        // SUPPLIER – třetí krok materiálové kaskády
        // =========================================================
        // 👉 Uživatel vybírá dodavatele (např. "DEK", "Elkov")
        // 👉 Po změně se:
        //    • načtou dostupné nabídky (SupplierName)
        //    • shodí Offer + Price
        //    • 🔵 MUSÍ se vyvolat notify pro CanSelectOffer
        //      jinak se poslední ComboBox NEODEMKNĚ
        // =========================================================
        public string? SelectedSupplier
        {
            get => _selectedSupplier;
            set
            {
                if (_selectedSupplier == value) return;
                _selectedSupplier = value;

                _materialCascade.ResetBelowSupplier(this);
                _materialCascade.LoadOffers(this);

                OnPropertyChanged();

                // 🔵 DOPLNĚNO — odemknutí nabídky
                OnPropertyChanged(nameof(CanSelectOffer));

                NotifyCalculatedProperties();
                UpdateTotal();
            }
        }


        // =========================================================
        // OFFER – čtvrtý krok materiálové kaskády
        // =========================================================
        // 👉 Uživatel vybírá konkrétní nabídku dodavatele
        //    (např. "CYKY 3×2,5 – balení 100 m")
        // 👉 Po změně se:
        //    • načte MaterialPrice (kód, cena, jednotka)
        //    • přepočítá Total
        //    • 🔵 Zde se notify nepřidává — není potřeba odemykat další krok
        // =========================================================
        public string? SelectedOffer
        {
            get => _selectedOffer;
            set
            {
                if (_selectedOffer == value) return;
                _selectedOffer = value;

                // Finální krok – načtení ceny podle kombinace:
                // Category + ProductName + Supplier + Offer
                _materialCascade.UpdateSelectedPrice(this);

                OnPropertyChanged();
                NotifyCalculatedProperties();
                UpdateTotal();
            }
        }

        // =========================================================
        // 🔴 SELECTED WORK SPECIFICATION – druhý krok kaskády PRÁCE
        // =========================================================
        public WorkSpecification? SelectedWorkSpecification
        {
            get => _selectedWorkSpecification;
            set
            {
                if (_selectedWorkSpecification == value) return;
                _selectedWorkSpecification = value;

                // Reset nižších kroků kaskády
                SelectedBaseMaterial = null;
                SelectedPosition = null;

                OnPropertyChanged();
                NotifyCalculatedProperties();
                UpdateTotal();
            }
        }


        // =========================================================
        // 🔴 SELECTED BASE MATERIAL – třetí krok kaskády PRÁCE
        // =========================================================
        public BaseMaterial? SelectedBaseMaterial
        {
            get => _selectedBaseMaterial;
            set
            {
                if (_selectedBaseMaterial == value) return;
                _selectedBaseMaterial = value;

                OnPropertyChanged();
                NotifyCalculatedProperties();
                UpdateTotal();
            }
        }


        // =========================================================
        // 🔴 SELECTED POSITION – čtvrtý krok kaskády PRÁCE
        // =========================================================
        public Position? SelectedPosition
        {
            get => _selectedPosition;
            set
            {
                if (_selectedPosition == value) return;
                _selectedPosition = value;

                OnPropertyChanged();
                NotifyCalculatedProperties();
                UpdateTotal();
            }
        }


        // =========================================================
        // 💰 WORK PRICE – uložená cena práce (z ProjectData)
        // =========================================================
        private decimal? _workPrice;
        public decimal? WorkPrice
        {
            get => _workPrice;
            set
            {
                _workPrice = value;
                OnPropertyChanged();
                NotifyCalculatedProperties();
                UpdateTotal();
            }
        }


        // =========================================================
        // 📏 WORK UNIT – jednotka práce (z ProjectData)
        // =========================================================
        private string? _workUnit;
        public string? WorkUnit
        {
            get => _workUnit;
            set
            {
                _workUnit = value;
                OnPropertyChanged();
                NotifyCalculatedProperties();
            }
        }


        // =========================================================
        // 🔵 SELECTED MATERIAL PRICE – výsledek materiálové kaskády
        // =========================================================
        public MaterialPrice? SelectedMaterialPrice
        {
            get => _selectedMaterialPrice;
            set
            {
                if (_selectedMaterialPrice == value) return;
                _selectedMaterialPrice = value;

                // Přenos hodnot do ViewModelu
                MaterialPrice = value?.Price;
                MaterialUnit = value?.Unit;

                OnPropertyChanged();
                NotifyCalculatedProperties();
                UpdateTotal();
            }
        }



        // =========================================================
        // 💰 MATERIAL PRICE – uložená cena materiálu
        // =========================================================
        //
        // Účel:
        //   • Uchovává cenu materiálu (decimal?).
        //   • Hodnota se ukládá do CalculationItemData.MaterialPrice.
        //   • Pokud je null → cena se dopočítá z materiálové kaskády.
        //
        // Poznámka:
        //   • Staré SelectedMaterialPriceValue bylo odstraněno.
        //   • Nově se používá MaterialPrice (decimal?).
        // =========================================================

        private decimal? _materialPrice;
        public decimal? MaterialPrice
        {
            get => _materialPrice;
            set
            {
                _materialPrice = value;
                OnPropertyChanged();
                NotifyCalculatedProperties();
                UpdateTotal();
            }
        }

        // =========================================================
        // 📏 MATERIAL UNIT – uložená jednotka materiálu
        // =========================================================
        //
        // Účel:
        //   • Uchovává jednotku materiálu (ks, m, bm…).
        //   • Hodnota se ukládá do CalculationItemData.MaterialUnit.
        //
        // Poznámka:
        //   • Staré SelectedMaterialUnit bylo odstraněno.
        //   • Nově se používá MaterialUnit (string?).
        // =========================================================

        private string? _materialUnit;
        public string? MaterialUnit
        {
            get => _materialUnit;
            set
            {
                _materialUnit = value;
                OnPropertyChanged();
                NotifyCalculatedProperties();
            }
        }

        // =========================================================
        // 🔴 SELECTED WORK TASK – první krok normalizované kaskády PRÁCE
        // =========================================================
        //
        // Účel:
        //   • Nastaví hlavní úkon (WorkTask).
        //   • Po výběru WorkTask se dynamicky načtou:
        //         – dostupné WorkSpecification
        //         – dostupné BaseMaterial
        //         – dostupné Position
        //   • Resetují se nižší kroky kaskády.
        //   • Spustí se přepočet ceny práce.
        //
        // Poznámka:
        //   • Starý SelectedTask (string) byl odstraněn.
        //   • Nově se používá typově bezpečný objekt WorkTask.
        // =========================================================

        public WorkTask? SelectedWorkTask
        {
            get => _selectedWorkTask;
            set
            {
                if (_selectedWorkTask == value) return;
                _selectedWorkTask = value;

                // Načtení dostupných hodnot podle WorkTask
                _workCascade.LoadSpecifications(this);
                _workCascade.LoadBaseMaterials(this);
                _workCascade.LoadPositions(this);

                // Reset nižších kroků
                SelectedWorkSpecification = null;
                SelectedBaseMaterial = null;
                SelectedPosition = null;

                // UI – povolení dalších kroků
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanSelectWorkSpecification));
                OnPropertyChanged(nameof(CanSelectBaseMaterial));
                OnPropertyChanged(nameof(CanSelectPosition));

                NotifyCalculatedProperties();
                UpdateTotal();
            }
        }

        // =========================================================
        // 🔴 CALCULATED WORK PRICE – BasePrice × MaterialCoef × PositionCoef
        // =========================================================
        //
        // Účel:
        //   • Uchovává vypočtenou cenu práce.
        //   • Hodnota se nastavuje výhradně CalculationPriceService.
        //   • Setter je veřejný kvůli přístupu z WorkCascadeService.
        //
        // Poznámka:
        //   • Staré SelectedWorkPrice bylo odstraněno.
        // =========================================================

        public decimal? CalculatedWorkPrice
        {
            get => _calculatedWorkPrice;
            set
            {
                if (_calculatedWorkPrice == value) return;
                _calculatedWorkPrice = value;

                OnPropertyChanged();
                NotifyCalculatedProperties();
            }
        }


        // =========================================================
        // QUANTITY – množství položky
        // =========================================================
        //
        // Účel:
        //   • Uchovává množství položky (double).
        //   • Ovlivňuje výpočet Total.
        //   • Hodnota se ukládá do CalculationItemData.Quantity.
        //
        // Poznámka:
        //   • Hodnota je clampována na >= 0.
        //   • Každá změna vyvolá přepočet Total.
        // =========================================================

        public double Quantity
        {
            get => _quantity;
            set
            {
                value = Math.Max(0, value); // clamp

                if (Math.Abs(_quantity - value) < 0.0001)
                    return;

                _quantity = value;

                OnPropertyChanged();
                NotifyCalculatedProperties();
                UpdateTotal();
            }
        }


        // =========================================================
        // TOTAL – celková cena řádku (delegováno do služby)
        // =========================================================
        //
        // Účel:
        //   • Vrací celkovou cenu řádku.
        //   • Výpočet je delegován do CalculationPriceService.
        //   • ViewModel pouze notifikací oznamuje změny.
        //
        // Poznámka:
        //   • Total se přepočítá při změně kaskády nebo Quantity.
        // =========================================================

        public double Total => _price.CalculateTotal(this);


        // ============================================================================
        // 🧩 IsEmpty – pomocná vlastnost pro filtrování prázdných řádků při ukládání
        // ----------------------------------------------------------------------------
        // Účel:
        //   • Umožňuje ProjectService odfiltrovat prázdné řádky před serializací.
        //   • Díky tomu se do JSONu neukládají placeholder řádky, které UI automaticky
        //     vytváří (např. poslední prázdný řádek).
        //
        // Definice „prázdného řádku“:
        //
        //   • PRÁCE (nová kaskáda WorkTask → WorkSpecification → BaseMaterial → Position):
        //       - Řádek Práce je prázdný, pokud:
        //           SelectedWorkTask == null
        //           AND SelectedWorkSpecification == null
        //           AND Quantity == 0
        //
        //       - BaseMaterial a Position se do prázdnosti NEZAPOČÍTÁVAJÍ.
        //         (Jejich seznam je vždy plný, uživatel je může doplnit kdykoli.)
        //
        //   • MATERIÁL (kaskáda Category → Název → Dodavatel → Nabídka → Cena):
        //       - Řádek Materiálu je prázdný, pokud:
        //           SelectedCategory == null/empty
        //           AND SelectedProductName == null/empty
        //           AND SelectedSupplier == null/empty
        //           AND SelectedOffer == null/empty
        //           AND SelectedMaterialPrice == null
        //           AND Quantity == 0
        //
        //   • SPOLEČNÉ:
        //       - Řádek je prázdný pouze tehdy, pokud jsou prázdná VŠECHNA pole
        //         z obou skupin zároveň.
        //
        // Poznámka:
        //   • Sleva (IsDiscountEnabled / DiscountPercent) se do IsEmpty nezapočítává.
        // ============================================================================

        public bool IsEmpty =>
            // 🔴 PRÁCE – nová kaskáda
            SelectedWorkTask == null
            && SelectedWorkSpecification == null
            && Quantity == 0

            // 🔴 MATERIÁL – původní kaskáda
            && string.IsNullOrWhiteSpace(SelectedCategory)
            && string.IsNullOrWhiteSpace(SelectedProductName)
            && string.IsNullOrWhiteSpace(SelectedSupplier)
            && string.IsNullOrWhiteSpace(SelectedOffer)
            && SelectedMaterialPrice == null;


        // =========================================================
        // 🔄 UpdateTotal – přepočet celkové ceny řádku
        // ---------------------------------------------------------
        // Účel:
        //   • Vyvolá přepočet TOTAL po změně kaskády nebo množství.
        //   • TOTAL se počítá v CalculationPriceService → stačí notifikace.
        // =========================================================

        public void UpdateTotal()
        {
            OnPropertyChanged(nameof(Total));
        }


        // =========================================================
        // 🔔 NotifyCalculatedProperties – přepočet Total + IsEmpty
        // ---------------------------------------------------------
        // Účel:
        //   • Vyvolá notifikaci všech derived properties.
        //   • Volá se po každé změně vstupních hodnot.
        // =========================================================

        public void NotifyCalculatedProperties()
        {
            OnPropertyChanged(nameof(Total));
            OnPropertyChanged(nameof(IsEmpty));
        }


        // =========================================================
        // NOTIFY – INotifyPropertyChanged
        // =========================================================

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}