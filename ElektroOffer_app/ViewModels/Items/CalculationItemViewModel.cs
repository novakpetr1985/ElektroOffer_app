using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using ElektroOffer_app.Data;
using ElektroOffer_app.Models;

namespace ElektroOffer_app.ViewModels.Items
{

    // =========================================================
    // 🧮 CALCULATION ITEM VIEWMODEL
    // =========================================================
    // 👉 ViewModel pro jeden řádek kalkulace v sekci PRÁCE
    // 👉 Řídí kaskádu ComboBoxů (Task → Specification → Material → Location)
    // 👉 Po výběru všech hodnot spočítá celkovou cenu (Total)
    // =========================================================
    public class CalculationItemViewModel : INotifyPropertyChanged
    {
        private readonly AppDbContext _db;

    // Konstruktor pro aplikaci – používá elektrooffer.db
    public CalculationItemViewModel() : this(new AppDbContext())
    {
    }

    // Konstruktor pro testy – dostane SQLite InMemory DB
    public CalculationItemViewModel(AppDbContext db)
    {
        _db = db;
    }

        private PriceItems? _workItem;
        private Material? _materialItem;
        private double _quantity;

        private string? _selectedTask;
        private string? _selectedSpecification;
        private string? _selectedMaterial;
        private string? _selectedLocation;

        private string? _workUnit;

        // =========================================================
        // 💰 SLEVA
        // =========================================================
        // 👉 IsDiscountEnabled – přepíná aktivaci slevy
        //    při vypnutí automaticky vynuluje DiscountPercent
        //    a vyvolá přepočet Total
        // 👉 DiscountPercent – procentuální hodnota slevy
        //    null = sleva není zadána
        //    změna vždy vyvolá přepočet Total
        // =========================================================

        private bool _isDiscountEnabled;

        /// <summary>
        /// Přepínač aktivace slevy na tomto řádku.
        /// Při vypnutí (false) automaticky vynuluje DiscountPercent → null.
        /// Změna vždy vyvolá přepočet Total v UI.
        /// </summary>
        public bool IsDiscountEnabled
        {
            get => _isDiscountEnabled;
            set
            {
                if (_isDiscountEnabled == value) return;
                _isDiscountEnabled = value;

                // Vypnutí slevy → vynuluj procento
                // Píšeme přímo do _discountPercent (privátní field),
                // aby nedošlo ke zbytečnému dvojímu volání OnPropertyChanged
                if (!value)
                {
                    _discountPercent = null;
                    OnPropertyChanged(nameof(DiscountPercent));
                }

                OnPropertyChanged();
                OnPropertyChanged(nameof(Total)); // ← přepočet ceny v UI
            }
        }

        private double? _discountPercent;

        /// <summary>
        /// Procentuální výše slevy (0–100).
        /// Null = sleva není zadána.
        /// Změna vždy vyvolá přepočet Total v UI.
        /// </summary>
        public double? DiscountPercent
        {
            get => _discountPercent;
            set
            {
                if (_discountPercent == value) return;
                _discountPercent = value;

                OnPropertyChanged();
                OnPropertyChanged(nameof(Total)); // ← přepočet ceny v UI
            }
        }

        // =========================================================
        // 📦 DYNAMICKÉ LISTY PRO COMBOBOXY
        // =========================================================
        public ObservableCollection<string> AvailableSpecifications { get; } = new();
        public ObservableCollection<string> AvailableMaterials { get; } = new();
        public ObservableCollection<string> AvailableLocations { get; } = new();

        // =========================================================
        // 📏 UNIT (MĚRNÁ JEDNOTKA PRÁCE)
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
        // 🔒 KASKÁDA POVOLENÍ
        // =========================================================
        // 👉 Určuje, zda jsou jednotlivé ComboBoxy povolené
        // 👉 Závisí na předchozích výběrech
        // =========================================================
        public bool CanSelectSpecification => !string.IsNullOrWhiteSpace(SelectedTask);
        public bool CanSelectMaterial => !string.IsNullOrWhiteSpace(SelectedSpecification);
        public bool CanSelectLocation => !string.IsNullOrWhiteSpace(SelectedMaterial);

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

                // Reset kaskády níže + načtení nových hodnot
                ResetBelowTask();
                LoadSpecifications();

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

                ResetBelowSpecification();
                LoadWorkUnit();
                LoadMaterials();

                OnPropertyChanged();
                OnPropertyChanged(nameof(CanSelectMaterial));
            }
        }

        // =========================================================
        // MATERIAL
        // =========================================================
        public string? SelectedMaterial
        {
            get => _selectedMaterial;
            set
            {
                if (_selectedMaterial == value) return;
                _selectedMaterial = value;

                // 1) Reset
                ResetBelowMaterial();   // nastaví SelectedLocation = null

                // 2) Naplnění kolekce
                LoadLocations();        // WPF zde vybere první položku

                // 3) Kritické: znovu vynutit null
                //    protože WPF po naplnění kolekce automaticky vybere první položku
                SelectedLocation = null;

                OnPropertyChanged();
                OnPropertyChanged(nameof(CanSelectLocation));
            }
        }

        // =========================================================
        // LOCATION
        // =========================================================
        // 🔴 ZMĚNA: Tady byla ve skutečnosti druhá, chybná kopie property
        //    SelectedMaterial (copy-paste chyba) – proto kompilátor hlásil
        //    "nejednoznačnost mezi SelectedMaterial a SelectedMaterial"
        //    (dvě stejnojmenné property v jedné třídě) a zároveň
        //    "SelectedLocation v aktuálním kontextu neexistuje"
        //    (protože property SelectedLocation jako taková nikdy
        //    nevznikla – existoval jen soukromý field _selectedLocation,
        //    ke kterému se nikdo přes get/set nedostal).
        //
        //    Oprava: property se teď skutečně jmenuje SelectedLocation
        //    a pracuje s vlastním fieldem _selectedLocation (ne s
        //    _selectedMaterial, který patří té předchozí property výše).
        public string? SelectedLocation
        {
            get => _selectedLocation;
            set
            {
                if (_selectedLocation == value) return;
                _selectedLocation = value;

                // Location je poslední úroveň kaskády (Task → Specification
                // → Material → Location), takže pod ní už není nic dalšího
                // k resetování ani žádná další kolekce k načtení.

                // Jakmile je vybrána Location, máme kompletní kombinaci
                // Task + Specification + Material + Location → dohledáme
                // konkrétní záznam v tabulce PriceItems, ze kterého se
                // pak počítá Total.
                UpdateWorkItem();

                OnPropertyChanged();
                OnPropertyChanged(nameof(Total));
            }
        }


        // =========================================================
        // WORK ITEM (ZÁZNAM Z TABULKY PRICEITEMS)
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

        // =========================================================
        // MATERIAL ITEM (ZÁZNAM Z TABULKY MATERIALS)
        // =========================================================
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
        // QUANTITY (MNOŽSTVÍ)
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
        // TOTAL (CELKOVÁ CENA ŘÁDKU + SLEVA)
        // =========================================================
        // 👉 Pokud je vybraná práce:
        //      BasePrice × MaterialCoef × PositionCoef × Quantity
        //
        // 👉 Pokud je vybraný materiál:
        //      Material.Price × Quantity
        //
        // 👉 Pokud není vybráno nic → 0
        //
        // 👉 Sleva:
        //      • Aplikuje se pouze pokud IsDiscountEnabled == true
        //      • DiscountPercent má hodnotu (není null)
        //      • Sleva NIKDY nesmí způsobit zápornou cenu
        //        → pokud je sleva >= 100 %, výsledná cena je 0
        //      • Záporná sleva (< 0 %) se ignoruje
        //
        // 👉 Příklady:
        //      150 Kč × 1,2 × 1,0 × 10 m = 1 800 Kč
        //      Sleva 10 % → 1 800 × 0,9 = 1 620 Kč
        //      Sleva 150 % → 0 Kč (chráněno proti záporné hodnotě)
        // =========================================================
        public double Total
        {
            get
            {
                double baseTotal;

                if (WorkItem != null)
                    baseTotal = WorkItem.BasePrice * WorkItem.MaterialCoef * WorkItem.PositionCoef * Quantity;

                else if (MaterialItem != null)
                    baseTotal = MaterialItem.Price * Quantity;

                else
                    return 0;

                // =========================
                // 💰 APLIKACE SLEVY
                // =========================
                // Sleva se aplikuje pouze pokud:
                //   1. IsDiscountEnabled == true  (přepínač je zapnutý)
                //   2. DiscountPercent má hodnotu  (není null)
                // Vzorec: baseTotal × (1 - procento / 100)
                // Příklad: 1 000 Kč × (1 - 10/100) = 1 000 × 0,9 = 900 Kč
                if (IsDiscountEnabled && DiscountPercent.HasValue)
                {
                    // Sleva >= 100 % → nesmí být záporné
                    if (DiscountPercent.Value >= 100)
                        return 0;

                    // Sleva < 0 → ignorovat (nebo můžeš udělat zdražení, pokud chceš)
                    if (DiscountPercent.Value < 0)
                        return baseTotal;

                    return baseTotal * (1 - DiscountPercent.Value / 100.0);
                }

                return baseTotal;
            }
        }

        // =========================================================
        // 📏 UNIT LOAD
        // =========================================================
        // 👉 Načte měrnou jednotku z tabulky PriceItems podle Specification
        // =========================================================
        private void LoadWorkUnit()
        {
            // 🔴 ZMĚNA: dřív se tu vytvářela NOVÁ instance `new AppDbContext()`,
            //    která se vždy připojila ke skutečné produkční databázi
            //    (elektrooffer.db) – ignorovala injektovanou testovací DB (_db).
            //    V testech (in-memory SQLite se seedovanými daty) to způsobovalo,
            //    že se dotaz ptal "špatné" databáze a vracel prázdné výsledky.
            //    Oprava: používáme vždy `_db`, které si ViewModel dostal
            //    do konstruktoru (buď produkční, nebo testovací).
            WorkUnit = _db.PriceItems
                .Where(x => x.Specification == SelectedSpecification)
                .Select(x => x.Unit)
                .FirstOrDefault();
        }

        // =========================================================
        // 🔽 FILTER 1: SPECIFICATIONS
        // =========================================================
        // 👉 Načte seznam specifikací pro vybraný Task
        // =========================================================
        private void LoadSpecifications()
        {
            AvailableSpecifications.Clear();

            // 🔴 ZMĚNA: dřív se tu vytvářela nepoužitá proměnná
            //    `using var db = new AppDbContext();`, která se nikde
            //    dál nepoužila (dotaz níže správně používal _db).
            //    Zbytečně by otevírala další připojení k databázi,
            //    proto ji odstraňuji.

            var list = _db.PriceItems
                .Where(x => x.Task == SelectedTask)
                .Select(x => x.Specification)
                .Distinct()
                .ToList();


            foreach (var item in list)
                AvailableSpecifications.Add(item);
        }

        // =========================================================
        // 🔽 FILTER 2: MATERIALS
        // =========================================================
        // 👉 Načte seznam materiálů pro vybraný Task + Specification
        // =========================================================
        private void LoadMaterials()
        {
            AvailableMaterials.Clear();

            // 🔴 ZMĚNA (HLAVNÍ OPRAVA): dřív se tu vytvářela NOVÁ instance
            //    `new AppDbContext()` a dotaz se ptal jí (`db.PriceItems`)
            //    místo injektované `_db`. To znamenalo, že se metoda vždy
            //    připojila ke skutečné produkční databázi elektrooffer.db,
            //    i když test poslal ViewModelu vlastní in-memory testovací
            //    databázi přes konstruktor.
            //    Důsledek: test "Changing_Specification_Should_Load_New_Materials"
            //    padal s "Expected: 1, But was: 0" – testovací data v seedu
            //    byla v _db, ale dotaz se ptal jiné (prázdné) databáze.
            //    Oprava: používáme vždy _db.
            var list = _db.PriceItems
                .Where(x => x.Task == SelectedTask &&
                            x.Specification == SelectedSpecification)
                .Select(x => x.Material)
                .Distinct()
                .ToList();

            foreach (var item in list)
                AvailableMaterials.Add(item);
        }

        // =========================================================
        // 🔽 FILTER 3: LOCATIONS
        // =========================================================
        // 👉 Načte seznam umístění pro vybraný Task + Specification + Material
        // =========================================================
        private void LoadLocations()
        {
            AvailableLocations.Clear();

            if (SelectedTask == null ||
                SelectedSpecification == null ||
                SelectedMaterial == null)
                return;

            var locations = _db.PriceItems
                .Where(p => p.Task == SelectedTask &&
                            p.Specification == SelectedSpecification &&
                            p.Material == SelectedMaterial)
                .Select(p => p.Location)
                .Distinct()
                .ToList();

            foreach (var loc in locations)
                AvailableLocations.Add(loc);

            // ❌ Tohle musí pryč:
            // SelectedLocation = AvailableLocations.FirstOrDefault();
        }


        // =========================================================
        // UPDATE RESULT (NAČTENÍ KONKRÉTNÍHO ZÁZNAMU PRÁCE)
        // =========================================================
        private void UpdateWorkItem()
        {
            if (string.IsNullOrWhiteSpace(SelectedTask) ||
                string.IsNullOrWhiteSpace(SelectedSpecification) ||
                string.IsNullOrWhiteSpace(SelectedMaterial) ||
                string.IsNullOrWhiteSpace(SelectedLocation))
            {
                WorkItem = null;
                return;
            }

            // 🔴 ZMĚNA: stejná chyba jako v LoadMaterials/LoadWorkUnit –
            //    nová instance `new AppDbContext()` ignorovala injektovanou
            //    testovací databázi. Používáme _db.
            WorkItem = _db.PriceItems
                .FirstOrDefault(x =>
                    x.Task == SelectedTask &&
                    x.Specification == SelectedSpecification &&
                    x.Material == SelectedMaterial &&
                    x.Location == SelectedLocation);
        }

        // =========================================================
        // RESETY KASKÁDY
        // =========================================================
        // 👉 Nově používáme PUBLIC PROPERTY settery místo přímých fieldů:
        //    • tím se vždy správně vyvolá PropertyChanged
        //    • zachová se veškerá logika v settere (např. CanSelect...)
        // 👉 Kolekce (Available*) se dál čistí ručně – to je v pořádku
        // 👉 WorkItem / WorkUnit se ruší na nejvyšší úrovni, kde to dává smysl
        // =========================================================

        private void ResetBelowTask()
        {
            // ✅ Změna: používáme settery, ne fieldy
            // Task je nejvyšší úroveň → musíme shodit vše pod ním:
            //   Specification, Material, Location, WorkItem, WorkUnit
            SelectedSpecification = null;
            SelectedMaterial = null;
            SelectedLocation = null;

            AvailableSpecifications.Clear();
            AvailableMaterials.Clear();
            AvailableLocations.Clear();

            WorkItem = null;
            WorkUnit = null;

            // ❌ OnPropertyChanged(...) už není potřeba ručně volat:
            //    settery SelectedSpecification/Material/Location ho vyvolají samy.
            //    CanSelectMaterial/Location se přepočítají přes jejich logiku.
        }

        private void ResetBelowSpecification()
        {
            // ✅ Změna: používáme settery, ne fieldy
            // Specification je druhá úroveň → shazujeme Material + Location
            SelectedMaterial = null;
            SelectedLocation = null;

            AvailableMaterials.Clear();
            AvailableLocations.Clear();

            WorkItem = null;

            // ❌ Opět není potřeba ručně volat OnPropertyChanged – settery to řeší.
        }

        private void ResetBelowMaterial()
        {
            // ✅ Tohle už máš správně – používáme setter SelectedLocation
            // Material je třetí úroveň → shazujeme jen Location
            SelectedLocation = null;

            AvailableLocations.Clear();
            WorkItem = null;

            // ❌ Žádné ruční OnPropertyChanged – setter SelectedLocation se postará.
        }

        // =========================================================
        // 🔔 NOTIFY
        // =========================================================
        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}