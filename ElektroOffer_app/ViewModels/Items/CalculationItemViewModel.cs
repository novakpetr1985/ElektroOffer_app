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
        private readonly MaterialCascadeService _materialCascade; // NOVĚ - kaskáda produktového materiálu
        private readonly CalculationPriceService _price;

        public CalculationItemViewModel(AppDbContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _cascade = new CalculationCascadeService(db);
            _materialCascade = new MaterialCascadeService(db);
            _price = new CalculationPriceService();

            // ZMĚNA: LoadCategories místo LoadMaterialNames -
            // Kategorie je teď první krok kaskády, ne Nazev
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

        private string? _selectedTask;
        private string? _selectedSpecification;
        private string? _selectedMaterial;
        private string? _selectedLocation;

        private string? _workUnit;

        private bool _isDiscountEnabled;
        private double? _discountPercent;

        // ---------------------------------------------------------
        // NOVĚ: Fields pro kaskádu PRODUKTOVÉHO materiálu
        // (Kategorie → Nazev → Dodavatel → Materiál)
        //
        // ZMĚNA: Kategorie je nyní AKTIVNÍ VÝBĚR (první krok kaskády),
        // ne jen zobrazení - proto "SelectedCategory" místo dřívějšího
        // "CategoryDisplay". CategoryDisplay byl odstraněn.
        // ---------------------------------------------------------
        private string? _selectedCategory;
        private string? _selectedProductName;
        private string? _selectedSupplier;
        private string? _selectedOffer;
        private MaterialPrice? _selectedMaterialPrice;

        // =========================================================
        // PUBLIC COLLECTIONS (UI)
        // =========================================================

        public ObservableCollection<string> AvailableSpecifications { get; } = new();
        public ObservableCollection<string> AvailableMaterials { get; } = new();
        public ObservableCollection<string> AvailableLocations { get; } = new();

        // ---------------------------------------------------------
        // NOVĚ: Kolekce pro kaskádu produktového materiálu
        // ---------------------------------------------------------

        /// <summary>
        /// Seznam kategorií (Category.Name) - první krok kaskády,
        /// nezávisí na ničem. Plní se jednorázově v konstruktoru.
        /// </summary>
        public ObservableCollection<string> AvailableCategories { get; } = new();

        /// <summary>
        /// Seznam kanonických názvů materiálu (Material.Name), FILTROVANÝ
        /// podle vybrané SelectedCategory.
        /// </summary>
        public ObservableCollection<string> AvailableMaterialNames { get; } = new();

        /// <summary>
        /// Seznam dodavatelů (Supplier.Name), kteří nabízejí vybraný
        /// produktový materiál (SelectedProductName).
        /// </summary>
        public ObservableCollection<string> AvailableSuppliers { get; } = new();

        /// <summary>
        /// Seznam názvů položky OD DODAVATELE (MaterialPrice.SupplierName)
        /// pro dvojici SelectedProductName + SelectedSupplier. Zobrazuje se
        /// JEN název, bez kódu a bez ceny (ty se zobrazí až v detailním
        /// rozpočtu později).
        /// </summary>
        public ObservableCollection<string> AvailableOffers { get; } = new();

        // =========================================================
        // DISCOUNT
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
        // UNIT
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

        public bool CanSelectSpecification => !string.IsNullOrWhiteSpace(SelectedTask);
        public bool CanSelectMaterial => !string.IsNullOrWhiteSpace(SelectedSpecification);
        public bool CanSelectLocation => !string.IsNullOrWhiteSpace(SelectedMaterial);

        // ---------------------------------------------------------
        // NOVĚ: Enable flagy pro kaskádu produktového materiálu
        // ---------------------------------------------------------
        public bool CanSelectProductName => !string.IsNullOrWhiteSpace(SelectedCategory);
        public bool CanSelectSupplier => !string.IsNullOrWhiteSpace(SelectedProductName);
        public bool CanSelectOffer => !string.IsNullOrWhiteSpace(SelectedSupplier);

        // =========================================================
        // TASK
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
        // SPECIFICATION
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
        // MATERIAL (typ materiálu v rámci ceníku PRÁCE - beze změny)
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
        // LOCATION
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
        // NOVĚ: SELECTED CATEGORY (první krok - výběr kategorie)
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
        // SELECTED PRODUCT NAME (výběr konkrétního produktu)
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
        // SELECTED SUPPLIER (výběr dodavatele pro vybraný produkt)
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
        // SELECTED OFFER (finální výběr - název položky od dodavatele)
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
        // WORK ITEM / MATERIAL ITEM
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
        // NOVĚ: SELECTED MATERIAL PRICE (výsledek celé nové kaskády)
        // =========================================================
        //
        // Obsahuje finální vybranou cenu OD KONKRÉTNÍHO DODAVATELE
        // (objekt MaterialPrice - nese Price, SupplierCode,
        // SupplierName, Unit, Currency). Nastavuje ho výhradně
        // MaterialCascadeService.UpdateSelectedPrice().
        //
        // PROČ SE TADY ZÁROVEŇ NASTAVUJE MaterialItem:
        // - CalculationPriceService (výpočet Total) byl původně
        //   napsaný tak, že čte cenu ze STARÉHO pole MaterialItem.Price
        //   (jedna pevná cena na Material, bez dodavatelů). Aby appka
        //   dál fungovala i BEZ úpravy CalculationPriceService, tenhle
        //   setter si při každé změně SelectedMaterialPrice zároveň
        //   dotáhne odpovídající Material entitu do MaterialItem -
        //   je to dočasné "přemostění" mezi starým a novým modelem.
        //
        // TODO (budoucí patch): Až bude CalculationPriceService
        // upravený, aby počítal cenu přímo z SelectedMaterialPrice.Price
        // (cena od KONKRÉTNÍHO vybraného dodavatele) místo starého
        // MaterialItem.Price (jediná univerzální cena bez dodavatele),
        // půjde tohle přemostění bezpečně odstranit.
        // =========================================================

        public MaterialPrice? SelectedMaterialPrice
        {
            get => _selectedMaterialPrice;
            set
            {
                if (_selectedMaterialPrice == value) return;
                _selectedMaterialPrice = value;

                MaterialItem = value?.Material;

                OnPropertyChanged();
                OnPropertyChanged(nameof(Total));
            }
        }

        // =========================================================
        // QUANTITY
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

        // =========================================================
        // NOTIFY
        // =========================================================

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}