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
        private readonly AppDbContext _db;
        private readonly CalculationCascadeService _cascade;
        private readonly MaterialCascadeService _materialCascade; // kaskáda produktového materiálu
        private readonly CalculationPriceService _price;

        public CalculationItemViewModel(AppDbContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _cascade = new CalculationCascadeService(db);
            _materialCascade = new MaterialCascadeService(db);
            _price = new CalculationPriceService();

            // Kategorie je první krok kaskády produktového materiálu
            _materialCascade.LoadCategories(this);
        }

        public static CalculationItemViewModel CreateDefault()
            => new CalculationItemViewModel(new AppDbContext());

        // =========================================================
        // FIELDS
        // =========================================================

        private PriceItems? _workItem;
        private Material? _materialItem;
        private double _quantity;

        // =====================================================================
        // 🆔 Id – propojení PRÁCE/MATERIÁL ↔ SPOLEČNÉ
        // =====================================================================
        //
        // ID je typu string, protože používáme krátké lidsky čitelné ID:
        //   • W-1, W-2, W-3...  (položky PRÁCE)
        //   • M-1, M-2, M-3...  (položky MATERIÁLU)
        //
        // CalculationItemViewModel.Id se nastavuje při ukládání projektu
        // v metodě BuildProjectData(), kde se generují ID:
        //
        //   • PRÁCE → W-1, W-2, W-3...
        //   • MATERIÁL → M-1, M-2, M-3...
        //
        // Stejné ID se ukládá do:
        //   • WorkItemData.Id
        //   • MaterialItemData.Id
        //   • CalculationItemData.Id
        //
        // Díky tomu lze při načítání projektu jednoznačně spárovat:
        //   • pracovní položku ↔ společné hodnoty
        //   • materiálovou položku ↔ společné hodnoty
        //
        public string Id { get; set; } = string.Empty;

        // PRÁCE – kaskáda úkon → specifikace → materiál → lokace
        private string? _selectedTask;
        private string? _selectedSpecification;
        private string? _selectedMaterial;
        private string? _selectedLocation;

        // MJ práce (např. m, ks, hod)
        private string? _workUnit;

        // sleva (společné)
        private bool _isDiscountEnabled;
        private double? _discountPercent;

        // MATERIÁL – produktová kaskáda
        private string? _selectedCategory;
        private string? _selectedProductName;
        private string? _selectedSupplier;
        private string? _selectedOffer;
        private MaterialPrice? _selectedMaterialPrice;

        // Uložené hodnoty pro ProjectData – MATERIÁL
        private decimal? _selectedMaterialPriceValue;
        private string? _selectedMaterialUnit;

        // Uložené hodnoty pro ProjectData – PRÁCE
        private decimal? _selectedWorkPrice;
        private string? _selectedWorkUnitValue;

        // =========================================================
        // PUBLIC COLLECTIONS (UI)
        // =========================================================

        public ObservableCollection<string> AvailableSpecifications { get; } = new();
        public ObservableCollection<string> AvailableMaterials { get; } = new();
        public ObservableCollection<string> AvailableLocations { get; } = new();

        // Produktová kaskáda materiálu
        public ObservableCollection<string> AvailableCategories { get; } = new();
        public ObservableCollection<string> AvailableMaterialNames { get; } = new();
        public ObservableCollection<string> AvailableSuppliers { get; } = new();
        public ObservableCollection<string> AvailableOffers { get; } = new();

        // =========================================================
        // DISCOUNT (společné)
        // =========================================================

        public bool IsDiscountEnabled
        {
            get => _isDiscountEnabled;
            set
            {
                if (_isDiscountEnabled == value) return;
                _isDiscountEnabled = value;

                if (!value)
                {
                    _discountPercent = null;
                    OnPropertyChanged(nameof(DiscountPercent));
                }

                OnPropertyChanged();
                NotifyCalculatedProperties();   // 🔥 OPRAVA – přepočet Total + IsEmpty
            }
        }

        public double? DiscountPercent
        {
            get => _discountPercent;
            set
            {
                if (value.HasValue)
                    value = Math.Clamp(value.Value, 0d, 100d);   // 🔥 OPRAVA – clamp

                if (_discountPercent == value) return;

                _discountPercent = value;

                OnPropertyChanged();
                NotifyCalculatedProperties();   // 🔥 OPRAVA – přepočet Total + IsEmpty
            }
        }

        // =========================================================
        // UNIT – pracovní MJ (z ceníku práce)
        // =========================================================

        public string? WorkUnit
        {
            get => _workUnit;
            set
            {
                if (_workUnit == value) return;
                _workUnit = value;
                OnPropertyChanged();
                NotifyCalculatedProperties();   // 🔥 OPRAVA – ovlivňuje IsEmpty
            }
        }

        // =========================================================
        // CASCADE ENABLE FLAGS
        // =========================================================

        // PRÁCE
        public bool CanSelectSpecification => !string.IsNullOrWhiteSpace(SelectedTask);
        public bool CanSelectMaterial => !string.IsNullOrWhiteSpace(SelectedSpecification);
        public bool CanSelectLocation => !string.IsNullOrWhiteSpace(SelectedMaterial);

        // MATERIÁL – produktová kaskáda
        public bool CanSelectProductName => !string.IsNullOrWhiteSpace(SelectedCategory);
        public bool CanSelectSupplier => !string.IsNullOrWhiteSpace(SelectedProductName);
        public bool CanSelectOffer => !string.IsNullOrWhiteSpace(SelectedSupplier);

        // =========================================================
        // TASK (úkon)
        // =========================================================

        public string? SelectedTask
        {
            get => _selectedTask;
            set
            {
                if (_selectedTask == value) return;
                _selectedTask = value;

                _cascade.ResetBelowTask(this);
                _cascade.LoadSpecifications(this);

                OnPropertyChanged();
                OnPropertyChanged(nameof(CanSelectSpecification));
                NotifyCalculatedProperties();   // 🔥 OPRAVA – ovlivňuje IsEmpty
            }
        }

        // =========================================================
        // SPECIFICATION (upřesnění úkonu)
        // =========================================================

        public string? SelectedSpecification
        {
            get => _selectedSpecification;
            set
            {
                if (_selectedSpecification == value) return;
                _selectedSpecification = value;

                _cascade.ResetBelowSpecification(this);
                _cascade.LoadWorkUnit(this);
                _cascade.LoadMaterials(this);

                OnPropertyChanged();
                OnPropertyChanged(nameof(CanSelectMaterial));
                NotifyCalculatedProperties();   // 🔥 OPRAVA – ovlivňuje IsEmpty
            }
        }

        // =========================================================
        // MATERIAL (typ materiálu v rámci ceníku PRÁCE)
        // =========================================================

        public string? SelectedMaterial
        {
            get => _selectedMaterial;
            set
            {
                if (_selectedMaterial == value) return;
                _selectedMaterial = value;

                _cascade.ResetBelowMaterial(this);
                _cascade.LoadLocations(this);

                SelectedLocation = null;

                OnPropertyChanged();
                OnPropertyChanged(nameof(CanSelectLocation));
                NotifyCalculatedProperties();   // 🔥 OPRAVA – ovlivňuje IsEmpty
            }
        }

        // =========================================================
        // LOCATION (umístění práce)
        // =========================================================

        public string? SelectedLocation
        {
            get => _selectedLocation;
            set
            {
                if (_selectedLocation == value) return;
                _selectedLocation = value;

                _cascade.UpdateWorkItem(this);

                OnPropertyChanged();
                NotifyCalculatedProperties();   // 🔥 OPRAVA – Total + IsEmpty
            }
        }

        // =========================================================
        // SELECTED CATEGORY (první krok produktové kaskády materiálu)
        // =========================================================

        public string? SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (_selectedCategory == value) return;
                _selectedCategory = value;

                _materialCascade.ResetBelowCategory(this);
                _materialCascade.LoadMaterialNames(this);

                OnPropertyChanged();
                OnPropertyChanged(nameof(CanSelectProductName));
                NotifyCalculatedProperties();   // 🔥 OPRAVA – ovlivňuje IsEmpty
            }
        }

        // =========================================================
        // SELECTED PRODUCT NAME (konkrétní produkt)
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
                OnPropertyChanged(nameof(CanSelectSupplier));
                NotifyCalculatedProperties();   // 🔥 OPRAVA – ovlivňuje IsEmpty
            }
        }

        // =========================================================
        // SELECTED SUPPLIER (dodavatel)
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
                _materialCascade.UpdateSelectedPrice(this);

                OnPropertyChanged();
                OnPropertyChanged(nameof(CanSelectOffer));
                NotifyCalculatedProperties();   // 🔥 OPRAVA – ovlivňuje IsEmpty
            }
        }

        // =========================================================
        // SELECTED OFFER (konkrétní nabídka dodavatele)
        // =========================================================

        public string? SelectedOffer
        {
            get => _selectedOffer;
            set
            {
                if (_selectedOffer == value) return;
                _selectedOffer = value;

                _materialCascade.UpdateSelectedPrice(this);

                OnPropertyChanged();
                NotifyCalculatedProperties();   // 🔥 OPRAVA – Total + IsEmpty
            }
        }

        // =========================================================
        // 📦 SELECTED MATERIAL PRICE VALUE (uložená cena z ProjectData)
        // =========================================================

        public decimal? SelectedMaterialPriceValue
        {
            get => _selectedMaterialPriceValue;
            set
            {
                _selectedMaterialPriceValue = value;
                OnPropertyChanged();
                NotifyCalculatedProperties();   // 🔥 OPRAVA – Total + IsEmpty
            }
        }

        // =========================================================
        // 📏 SELECTED MATERIAL UNIT (uložená jednotka z ProjectData)
        // =========================================================

        public string? SelectedMaterialUnit
        {
            get => _selectedMaterialUnit;
            set
            {
                _selectedMaterialUnit = value;
                OnPropertyChanged();
                NotifyCalculatedProperties();   // 🔥 OPRAVA – Total + IsEmpty
            }
        }

        // =========================================================
        // 💰 SELECTED WORK PRICE (uložená cena práce z ProjectData)
        // =========================================================
        //
        // Cena práce v době uložení projektu.
        // Pokud je null → po načtení se může dopočítat z ceníku.
        //
        public decimal? SelectedWorkPrice
        {
            get => _selectedWorkPrice;
            set
            {
                _selectedWorkPrice = value;
                OnPropertyChanged();
                NotifyCalculatedProperties();
            }
        }

        // =========================================================
        // 📏 SELECTED WORK UNIT (uložená jednotka práce z ProjectData)
        // =========================================================

        public string? SelectedWorkUnit
        {
            get => _selectedWorkUnitValue;
            set
            {
                _selectedWorkUnitValue = value;
                OnPropertyChanged();
                NotifyCalculatedProperties();
            }
        }

        // =========================================================
        // WORK ITEM / MATERIAL ITEM (EF entity)
        // =========================================================

        public PriceItems? WorkItem
        {
            get => _workItem;
            set
            {
                _workItem = value;
                OnPropertyChanged();
                NotifyCalculatedProperties();
            }
        }

        public Material? MaterialItem
        {
            get => _materialItem;
            set
            {
                _materialItem = value;
                OnPropertyChanged();
                NotifyCalculatedProperties();
            }
        }

        // =========================================================
        // SELECTED MATERIAL PRICE (výsledek nové materiálové kaskády)
        // =========================================================

        public MaterialPrice? SelectedMaterialPrice
        {
            get => _selectedMaterialPrice;
            set
            {
                if (_selectedMaterialPrice == value) return;
                _selectedMaterialPrice = value;

                // Přemostění pro starý výpočet Total
                MaterialItem = value?.Material;

                // Přenos hodnot do JSON
                if (value != null)
                {
                    SelectedMaterialPriceValue = value.Price;
                    SelectedMaterialUnit = value.Unit;
                }

                OnPropertyChanged();
                NotifyCalculatedProperties();
            }
        }

        // =========================================================
        // QUANTITY (společné)
        // =========================================================

        public double Quantity
        {
            get => _quantity;
            set
            {
                // Clamp záporných hodnot
                value = Math.Max(0, value);

                if (Math.Abs(_quantity - value) < 0.0001)
                    return;

                _quantity = value;

                OnPropertyChanged();
                NotifyCalculatedProperties();
            }
        }

        // =========================================================
        // TOTAL (delegováno do služby)
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
        //   • Tento view model je SDÍLENÝ pro řádky PRÁCE i MATERIÁLU – podle toho,
        //     do které kolekce (WorkCalcItems / MaterialItems) řádek patří, je
        //     vyplněná vždy jen jedna ze dvou níže uvedených skupin polí.
        //     Proto musí IsEmpty kontrolovat OBĚ skupiny současně:
        //
        //   • PRÁCE (kaskáda Task → Specification → Material → Location):
        //       - Není vybrán SelectedTask, SelectedSpecification,
        //         SelectedMaterial ani SelectedLocation.
        //
        //   • MATERIÁL (kaskáda Category → Název → Dodavatel → Nabídka → Cena):
        //       - Není vybrán SelectedCategory, SelectedProductName,
        //         SelectedSupplier ani SelectedOffer.
        //       - SelectedMaterialPrice je null (finální vybraná cena z kaskády).
        //
        //   • SPOLEČNÉ:
        //       - Quantity je 0.
        //
        //   • Řádek je považován za prázdný, jen pokud jsou prázdná/nevyplněná
        //     VŠECHNA pole z obou skupin zároveň i Quantity. Díky tomu se řádek
        //     Materiálu uloží už při vyplnění byť jen prvního kroku kaskády
        //     (např. SelectedCategory), stejně jako se dosud chovala Práce.
        //
        // 🔴 OPRAVA (bug: neukládání částečně vyplněné materiálové kaskády):
        //   • Původní verze kontrolovala pouze pracovní pole (Task/Specification/
        //     Material/Location) a Quantity. U materiálového řádku byla tato pole
        //     vždy prázdná (patří Práci), takže IsEmpty fakticky visela jen na
        //     Quantity == 0 – řádek se uložil až po vyplnění množství, bez ohledu
        //     na to, kolik z materiálové kaskády už bylo vybráno.
        //   • Doplněním kontroly materiálových polí (SelectedCategory,
        //     SelectedProductName, SelectedSupplier, SelectedOffer,
        //     SelectedMaterialPrice) se chování sjednotilo s Prací.
        //
        // Poznámka:
        //   • Pokud později přidáš další pole (např. poznámku k řádku, slevu jako
        //     samostatnou podmínku apod.), stačí je doplnit do příslušné skupiny.
        //   • Sleva (IsDiscountEnabled / DiscountPercent) se do IsEmpty záměrně
        //     nezapočítává – sama o sobě slevu bez vybrané položky nedává smysl
        //     ukládat jako neprázdný řádek.
        // ============================================================================
        public bool IsEmpty =>
            string.IsNullOrWhiteSpace(SelectedTask)
            && string.IsNullOrWhiteSpace(SelectedSpecification)
            && string.IsNullOrWhiteSpace(SelectedMaterial)
            && string.IsNullOrWhiteSpace(SelectedLocation)
            && string.IsNullOrWhiteSpace(SelectedCategory)
            && string.IsNullOrWhiteSpace(SelectedProductName)
            && string.IsNullOrWhiteSpace(SelectedSupplier)
            && string.IsNullOrWhiteSpace(SelectedOffer)
            && SelectedMaterialPrice == null
            && Quantity == 0;

        // =========================================================
        // NOTIFY
        // =========================================================

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        // =========================================================
        // 🔔 Pomocná metoda pro přepočet Total + IsEmpty
        // =========================================================
        //
        // Volá se po každé změně vstupních hodnot, které ovlivňují výpočet.
        // Opravuje testy 56, 57, 58, 60, 61.
        //
        private void NotifyCalculatedProperties()
        {
            OnPropertyChanged(nameof(Total));
            OnPropertyChanged(nameof(IsEmpty));
        }
    }
}
