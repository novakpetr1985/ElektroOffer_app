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
                OnPropertyChanged(nameof(Total));
            }
        }

        public double? DiscountPercent
        {
            get => _discountPercent;
            set
            {
                if (_discountPercent == value) return;
                _discountPercent = value;

                OnPropertyChanged();
                OnPropertyChanged(nameof(Total));
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
                OnPropertyChanged(nameof(Total));
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
                OnPropertyChanged(nameof(Total));
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
                OnPropertyChanged(nameof(Total));
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
                OnPropertyChanged(nameof(Total));
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
                OnPropertyChanged(nameof(Total));
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
                OnPropertyChanged(nameof(Total));
            }
        }

        public Material? MaterialItem
        {
            get => _materialItem;
            set
            {
                _materialItem = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Total));
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
                OnPropertyChanged(nameof(Total));
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
                if (Math.Abs(_quantity - value) < 0.0001) return;
                _quantity = value;

                OnPropertyChanged();
                OnPropertyChanged(nameof(Total));
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
        //   • Není vybrán Task, Specification, Material ani Location.
        //   • Množství je null nebo 0.
        //   • Sleva není aktivní nebo nemá hodnotu.
        //
        // Poznámka:
        //   • Pokud později přidáš další pole (např. poznámku), stačí je doplnit sem.
        // ============================================================================
        public bool IsEmpty =>
            string.IsNullOrWhiteSpace(SelectedTask)
            && string.IsNullOrWhiteSpace(SelectedSpecification)
            && string.IsNullOrWhiteSpace(SelectedMaterial)
            && string.IsNullOrWhiteSpace(SelectedLocation)
            && Quantity == 0
            && (!IsDiscountEnabled || DiscountPercent == null);

        // =========================================================
        // NOTIFY
        // =========================================================

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

