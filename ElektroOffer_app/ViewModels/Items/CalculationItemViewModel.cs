using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ElektroOffer_app.Data;
using ElektroOffer_app.Models;
using ElektroOffer_app.Services;

namespace ElektroOffer_app.ViewModels.Items
{
    /// <summary>Reprezentuje jeden editovatelný řádek práce nebo materiálu včetně výpočtu a validace.</summary>
    public class CalculationItemViewModel : INotifyPropertyChanged
    {
        private readonly AppDbContext _db;

        private readonly WorkCascadeService _workCascade;
        private readonly MaterialCascadeService _materialCascade;
        private readonly CalculationPriceService _price;

        public CalculationItemViewModel(AppDbContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _workCascade = new WorkCascadeService(db);
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

        // PRÁCE: UI drží vybrané názvy pro ComboBoxy a zároveň EF entity
        // potřebné pro výpočet ceny.
        private string? _selectedWorkTask;
        private string? _selectedWorkSpecification;
        private string? _selectedBaseMaterial;
        private string? _selectedWorkPosition;

        private WorkTask? _selectedWorkTaskEntity;
        private BaseMaterial? _selectedBaseMaterialEntity;
        private WorkPosition? _selectedWorkPositionEntity;

        // MJ práce (např. m, ks) – z vybrané WorkSpecification, jen pro zobrazení
        private string? _workUnit;

        // sleva (společné)
        private bool _isDiscountEnabled;
        private double? _discountPercent;

        // MATERIÁL – produktová kaskáda (beze změny)
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

        // Jediný řádkový seznam v kaskádě PRÁCE; závisí na vybraném úkonu.
        // Podklady a umístění jsou sdílené seznamy v MainViewModelu.
        public ObservableCollection<string> AvailableWorkSpecifications { get; } = new();

        // Produktová kaskáda materiálu (beze změny)
        public ObservableCollection<string> AvailableCategories { get; } = new();
        public ObservableCollection<string> AvailableMaterialNames { get; } = new();
        public ObservableCollection<string> AvailableSuppliers { get; } = new();
        public ObservableCollection<string> AvailableOffers { get; } = new();

        // =========================================================
        // DISCOUNT (společné, beze změny)
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
                NotifyCalculatedProperties();
            }
        }

        public double? DiscountPercent
        {
            get => _discountPercent;
            set
            {
                if (value.HasValue)
                    value = Math.Clamp(value.Value, 0d, 100d);

                if (_discountPercent == value) return;

                _discountPercent = value;

                OnPropertyChanged();
                NotifyCalculatedProperties();
            }
        }

        // =========================================================
        // UNIT – pracovní MJ (ze Specifikace, jen pro zobrazení)
        // =========================================================

        public string? WorkUnit
        {
            get => _workUnit;
            set
            {
                if (_workUnit == value) return;
                _workUnit = value;
                OnPropertyChanged();
                NotifyCalculatedProperties();
            }
        }

        // =========================================================
        // CASCADE ENABLE FLAGS
        // =========================================================

        // UI vynucuje postupný výběr: Úkon → Upřesnění → Podklad → Umístění.
        // Datově jsou Podklad a Umístění nezávislé seznamy, ale uživatel nemá
        // přeskakovat předchozí kroky řádku.
        public bool CanSelectWorkSpecification => !string.IsNullOrWhiteSpace(SelectedWorkTask);
        public bool CanSelectBaseMaterial => !string.IsNullOrWhiteSpace(SelectedWorkSpecification);
        public bool CanSelectWorkPosition => !string.IsNullOrWhiteSpace(SelectedBaseMaterial);

        // MATERIÁL – produktová kaskáda (beze změny)
        public bool CanSelectProductName => !string.IsNullOrWhiteSpace(SelectedCategory);
        public bool CanSelectSupplier => !string.IsNullOrWhiteSpace(SelectedProductName);
        public bool CanSelectOffer => !string.IsNullOrWhiteSpace(SelectedSupplier);

        // =========================================================
        // WORK TASK (úkon)
        // =========================================================

        public string? SelectedWorkTask
        {
            get => _selectedWorkTask;
            set
            {
                if (_selectedWorkTask == value) return;
                _selectedWorkTask = value;

                _workCascade.ResetBelowWorkTask(this);
                _workCascade.LoadWorkSpecifications(this);
                _workCascade.UpdateSelectedWorkTask(this);

                OnPropertyChanged();
                OnPropertyChanged(nameof(CanSelectWorkSpecification));
                OnPropertyChanged(nameof(CanSelectBaseMaterial));
                OnPropertyChanged(nameof(CanSelectWorkPosition));
                NotifyCalculatedProperties();
            }
        }

        // =========================================================
        // WORK SPECIFICATION (upřesnění úkonu – jen kvůli Unit)
        // =========================================================

        public string? SelectedWorkSpecification
        {
            get => _selectedWorkSpecification;
            set
            {
                if (_selectedWorkSpecification == value) return;
                _selectedWorkSpecification = value;

                _workCascade.ResetBelowWorkSpecification(this);
                _workCascade.LoadWorkUnit(this);

                OnPropertyChanged();
                OnPropertyChanged(nameof(CanSelectBaseMaterial));
                OnPropertyChanged(nameof(CanSelectWorkPosition));
                NotifyCalculatedProperties();
            }
        }

        // =========================================================
        // BASE MATERIAL (podklad – koeficient, nezávislý na Tasku)
        // =========================================================

        public string? SelectedBaseMaterial
        {
            get => _selectedBaseMaterial;
            set
            {
                if (_selectedBaseMaterial == value) return;
                _selectedBaseMaterial = value;

                _workCascade.ResetBelowBaseMaterial(this);
                _workCascade.UpdateSelectedBaseMaterial(this);

                OnPropertyChanged();
                OnPropertyChanged(nameof(CanSelectWorkPosition));
                NotifyCalculatedProperties();
            }
        }

        // =========================================================
        // WORK POSITION (poloha – koeficient, nezávislá na Tasku)
        // =========================================================

        public string? SelectedWorkPosition
        {
            get => _selectedWorkPosition;
            set
            {
                if (_selectedWorkPosition == value) return;
                _selectedWorkPosition = value;

                _workCascade.UpdateSelectedWorkPosition(this);

                OnPropertyChanged();
                NotifyCalculatedProperties();
            }
        }

        // =========================================================
        // WORK ENTITY REFERENCES (pro výpočet ceny)
        // =========================================================
        //
        // Nastavuje výhradně WorkCascadeService po změně odpovídajícího
        // Selected* řetězce výše. UI se na tyhle property nebinduje –
        // slouží jen CalculationPriceService k výpočtu Total.
        //
        public WorkTask? SelectedWorkTaskEntity
        {
            get => _selectedWorkTaskEntity;
            set { _selectedWorkTaskEntity = value; OnPropertyChanged(); NotifyCalculatedProperties(); }
        }

        public BaseMaterial? SelectedBaseMaterialEntity
        {
            get => _selectedBaseMaterialEntity;
            set { _selectedBaseMaterialEntity = value; OnPropertyChanged(); NotifyCalculatedProperties(); }
        }

        public WorkPosition? SelectedWorkPositionEntity
        {
            get => _selectedWorkPositionEntity;
            set { _selectedWorkPositionEntity = value; OnPropertyChanged(); NotifyCalculatedProperties(); }
        }

        // =========================================================
        // SELECTED CATEGORY (první krok produktové kaskády materiálu) – beze změny
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
                NotifyCalculatedProperties();
            }
        }

        // =========================================================
        // SELECTED PRODUCT NAME (konkrétní produkt) – beze změny
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
                NotifyCalculatedProperties();
            }
        }

        // =========================================================
        // SELECTED SUPPLIER (dodavatel) – beze změny
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
                NotifyCalculatedProperties();
            }
        }

        // =========================================================
        // SELECTED OFFER (konkrétní nabídka dodavatele) – beze změny
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
                NotifyCalculatedProperties();
            }
        }

        // =========================================================
        // 📦 SELECTED MATERIAL PRICE VALUE (uložená cena z ProjectData) – beze změny
        // =========================================================

        public decimal? SelectedMaterialPriceValue
        {
            get => _selectedMaterialPriceValue;
            set
            {
                _selectedMaterialPriceValue = value;
                OnPropertyChanged();
                NotifyCalculatedProperties();
            }
        }

        // =========================================================
        // 📏 SELECTED MATERIAL UNIT (uložená jednotka z ProjectData) – beze změny
        // =========================================================

        public string? SelectedMaterialUnit
        {
            get => _selectedMaterialUnit;
            set
            {
                _selectedMaterialUnit = value;
                OnPropertyChanged();
                NotifyCalculatedProperties();
            }
        }

        // =========================================================
        // 💰 SELECTED WORK PRICE (uložená cena práce z ProjectData) – beze změny
        // =========================================================

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
        // 📏 SELECTED WORK UNIT (uložená jednotka práce z ProjectData) – beze změny
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
        // MATERIAL ITEM (EF entita materiálu)
        // =========================================================
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
        // SELECTED MATERIAL PRICE (výsledek materiálové kaskády) – beze změny
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
        // QUANTITY (společné, beze změny)
        // =========================================================

        public double Quantity
        {
            get => _quantity;
            set
            {
                value = Math.Max(0, value);

                if (Math.Abs(_quantity - value) < 0.0001)
                    return;

                _quantity = value;

                OnPropertyChanged();
                NotifyCalculatedProperties();
            }
        }

        // =========================================================
        // TOTAL (delegováno do služby, beze změny)
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
        // Definice „prázdného řádku":
        //   • Tento view model je SDÍLENÝ pro řádky PRÁCE i MATERIÁLU – podle toho,
        //     do které kolekce (WorkCalcItems / MaterialItems) řádek patří, je
        //     vyplněná vždy jen jedna ze dvou níže uvedených skupin polí.
        //     Proto musí IsEmpty kontrolovat OBĚ skupiny současně:
        //
        //   • PRÁCE (kaskáda WorkTask → WorkSpecification → BaseMaterial → WorkPosition):
        //       - Není vybrán SelectedWorkTask, SelectedWorkSpecification,
        //         SelectedBaseMaterial ani SelectedWorkPosition.
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
        //     VŠECHNA pole z obou skupin zároveň i Quantity.
        // ============================================================================
        public bool IsEmpty =>
            string.IsNullOrWhiteSpace(SelectedWorkTask)
            && string.IsNullOrWhiteSpace(SelectedWorkSpecification)
            && string.IsNullOrWhiteSpace(SelectedBaseMaterial)
            && string.IsNullOrWhiteSpace(SelectedWorkPosition)
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
        private void NotifyCalculatedProperties()
        {
            OnPropertyChanged(nameof(Total));
            OnPropertyChanged(nameof(IsEmpty));
        }
    }
}
