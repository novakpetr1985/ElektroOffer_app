using ElektroOffer_app.Commands;
using ElektroOffer_app.Data;
using ElektroOffer_app.Models;
using ElektroOffer_app.Services;
using ElektroOffer_app.ViewModels.Items;
using ElektroOffer_app.Views;               // ✔ kvůli InvoiceWindow

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;                       // ✔ kvůli MessageBox
using System.Windows.Input;



namespace ElektroOffer_app.ViewModels
{
    /// <summary>
    /// Hlavní ViewModel aplikace – kompletní náhrada MainWindow.xaml.cs.
    /// Obsahuje veškerou logiku: přepočty, ukládání, načítání, tisk, export,
    /// práci s kolekcemi, slevy, stav projektu, status bar, atd.
    /// UI logika je přesunuta do služeb (MessageService, PrintService, WindowService).
    /// </summary>
    public class MainViewModel : INotifyPropertyChanged
    {
        // =========================================================
        // SERVICES + DB (jediný sdílený AppDbContext)
        // =========================================================

        private readonly ProjectService _projectService;
        private readonly CatalogService _catalogService;
        private readonly CalculationPriceService _price;
        private readonly AppDbContext _db;

        // UI služby – ViewModel nevolá WPF přímo
        private readonly IMessageService _messageService;
        private readonly IPrintService _printService;
        private readonly IApplicationService _applicationService;
        private readonly IWindowService _windowService;
        
        // 🔴 NOVÉ – služba pro načítání/ukládání údajů dodavatele
        // Dodavatel se ukládá do JSON v %AppData%, ne do DB.
        private readonly SupplierSettingsService _supplierSettingsService = new();

        // =========================================================
        // COLLECTIONS
        // =========================================================

        public ObservableCollection<string> Tasks { get; } = new();
        public ObservableCollection<Material> Materials { get; } = new();
        public ObservableCollection<CalculationItemViewModel> WorkCalcItems { get; } = new();
        public ObservableCollection<CalculationItemViewModel> MaterialItems { get; } = new();
        public ObservableCollection<BudgetItem> BudgetItems { get; } = new();

        // =========================================================
        // PROJECT STATE
        // =========================================================

        private string? _currentFilePath = null;
        private bool _hasUnsavedChanges = false;

        private string _statusText = "Nový projekt";
        public string StatusText
        {
            get => _statusText;
            set { _statusText = value; OnPropertyChanged(); }
        }

        // =========================================================
        // TOTALS
        // =========================================================

        private double _grandTotal;
        public double GrandTotal
        {
            get => _grandTotal;
            set { _grandTotal = value; OnPropertyChanged(); }
        }

        private double _workTotal;
        public double WorkTotal
        {
            get => _workTotal;
            set { _workTotal = value; OnPropertyChanged(); }
        }

        private double _materialTotal;
        public double MaterialTotal
        {
            get => _materialTotal;
            set { _materialTotal = value; OnPropertyChanged(); }
        }

        private double _workDiscountTotal;
        public double WorkDiscountTotal
        {
            get => _workDiscountTotal;
            set { _workDiscountTotal = value; OnPropertyChanged(); }
        }

        private double _materialDiscountTotal;
        public double MaterialDiscountTotal
        {
            get => _materialDiscountTotal;
            set { _materialDiscountTotal = value; OnPropertyChanged(); }
        }

        private double _totalDiscount;
        public double TotalDiscount
        {
            get => _totalDiscount;
            set { _totalDiscount = value; OnPropertyChanged(); }
        }

        private double _grandTotalBeforeDiscount;
        public double GrandTotalBeforeDiscount
        {
            get => _grandTotalBeforeDiscount;
            set { _grandTotalBeforeDiscount = value; OnPropertyChanged(); }
        }

        private bool _hasAnyDiscount;
        public bool HasAnyDiscount
        {
            get => _hasAnyDiscount;
            set { _hasAnyDiscount = value; OnPropertyChanged(); }
        }

        // =========================================================
        // COMMANDS
        // =========================================================

        public ICommand NewProjectCommand { get; }
        public ICommand LoadCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand SaveAsCommand { get; }
        public ICommand PrintCommand { get; }
        public ICommand ExitCommand { get; }
        public ICommand AboutCommand { get; }

        public ICommand AddWorkItemCommand { get; }
        public ICommand AddMaterialItemCommand { get; }
        public ICommand DeleteWorkItemCommand { get; }
        public ICommand DeleteMaterialItemCommand { get; }
        public ICommand ResetWorkItemCommand { get; }
        public ICommand ResetMaterialItemCommand { get; }

        // 🔴 NOVÉ – příkaz pro generování PDF faktury
        // Tento příkaz se naváže na tlačítko v UI (MainWindow.xaml)
        public ICommand GenerateInvoicePdfCommand { get; }
        
        // 🔴 NOVÉ – otevření okna faktury (vyplnění údajů + generování PDF)
        public ICommand OpenInvoiceWindowCommand { get; }
        
        // =====================================================================
        // 🚫 _isLoading – blokace přepočtů a reakcí během načítání projektu
        // =====================================================================
        //
        // Během ApplyProjectData se nastaví na TRUE,
        // aby Item_PropertyChanged nereagoval na změny,
        // kaskády se nespouštěly,
        // a kolekce se nepřeskupovala.
        //
        // Po načtení se nastaví zpět na FALSE.
        //
        private bool _isLoading = false;

        // =========================================================
        // CONSTRUCTOR
        // =========================================================

        public MainViewModel(
            ProjectService projectService,
            CatalogService catalogService,
            CalculationPriceService price,
            AppDbContext db,
            IMessageService messageService,
            IPrintService printService,
            IApplicationService applicationService,
            IWindowService windowService)
        {
            // ============================================================
            // 1) Dependency Injection – všechny služby + jeden sdílený DbContext
            // ------------------------------------------------------------
            // ViewModel nikdy nevytváří služby sám — dostává je z MainWindow.
            // Díky tomu je testovatelný a nezávislý na UI.
            // ============================================================
            _projectService = projectService;
            _catalogService = catalogService;
            _price = price;
            _db = db;

            _messageService = messageService;
            _printService = printService;
            _applicationService = applicationService;
            _windowService = windowService;

            // ============================================================
            // 2) Načtení katalogových dat z DB (ceník práce + materiál)
            // ------------------------------------------------------------
            // Toto načítá statické seznamy, které se nemění během běhu aplikace.
            // (Kategorie materiálu, ceník práce, dodavatelé…)
            // ============================================================
            LoadCatalogDataFromDb();

            // ============================================================
            // 3) Vytvoření výchozích řádků (stejně jako původní MainWindow)
            // ------------------------------------------------------------
            // Po startu aplikace má uživatel připravené prázdné řádky
            // pro práci i materiál, aby mohl ihned začít vyplňovat.
            // ============================================================
            for (int i = 0; i < 5; i++)
            {
                AddWorkItem();
                AddMaterialItem();
            }

            // ============================================================
            // 4) Reakce na změny kolekcí
            // ------------------------------------------------------------
            // Jakmile se změní počet řádků práce nebo materiálu,
            // automaticky se přepočítá celková cena a označí projekt jako změněný.
            // ============================================================
            WorkCalcItems.CollectionChanged += (_, __) => { MarkAsChanged(); Recalculate(); };
            MaterialItems.CollectionChanged += (_, __) => { MarkAsChanged(); Recalculate(); };

            // ============================================================
            // 5) Inicializace příkazů (Commands)
            // ------------------------------------------------------------
            // ViewModel obsahuje veškerou logiku — UI pouze volá příkazy.
            // ============================================================
            NewProjectCommand = new RelayCommand(_ => ResetToNewProject());
            LoadCommand = new RelayCommand(_ => Load());
            SaveCommand = new RelayCommand(_ => Save());
            SaveAsCommand = new RelayCommand(_ => SaveAs());
            PrintCommand = new RelayCommand(_ => Print());
            ExitCommand = new RelayCommand(_ => Exit());
            AboutCommand = new RelayCommand(_ => ShowAbout());

            AddWorkItemCommand = new RelayCommand(_ => AddWorkItem());
            AddMaterialItemCommand = new RelayCommand(_ => AddMaterialItem());
            DeleteWorkItemCommand = new RelayCommand(DeleteWorkItem);
            DeleteMaterialItemCommand = new RelayCommand(DeleteMaterialItem);
            ResetWorkItemCommand = new RelayCommand(ResetWorkItem);
            ResetMaterialItemCommand = new RelayCommand(ResetMaterialItem);

            // 🔴 NOVÉ – generování PDF faktury
            GenerateInvoicePdfCommand = new RelayCommand(_ => GenerateInvoicePdf());

            // 🔴 NOVÉ – otevření okna faktury
            OpenInvoiceWindowCommand = new RelayCommand(_ => OpenInvoiceWindow());
        }


        // ============================================================
        // 6) Načtení statických dat po startu aplikace
        // ------------------------------------------------------------
        // • Volá MainWindow při Loaded.
        // • Moderní architektura:
        //      – CalculationItemViewModel načítá kaskády práce i materiálu sám.
        //      – MainViewModel načítá pouze katalog (ceník práce, dodavatelé).
        //
        // • CatalogService pracuje s databází → vyžaduje AppDbContext.
        //   Proto se předává _db, nikoli this.
        // ============================================================
        public void LoadData()
        {
            // 📘 Katalog (ceník práce, dodavatelé…)
            _catalogService.LoadCatalog(_db);

            // Pokud budeš mít další inicializace, doplníš sem:
            // _something.Init(this);
        }

        // =========================================================
        // METHODS
        // =========================================================

        private void OpenInvoiceWindow()
        {
            // 1) Sestavení řádků faktury z kalkulace
            var template = new InvoiceTemplateService();
            var lines = template.BuildInvoiceLines(WorkCalcItems, MaterialItems);

            // 2) Vytvoření dat faktury
            var invoiceData = new InvoiceItemData
            {
                Lines = lines,
                Supplier = new SupplierSettings()   // uživatel vyplní ručně v okně
            };

            // 3) Otevření okna faktury
            var vm = new InvoiceWindowViewModel(WorkCalcItems, MaterialItems);
            var window = new InvoiceWindow
            {
                DataContext = vm,
                Owner = System.Windows.Application.Current.MainWindow
            };

            window.ShowDialog();
        }

        // =========================================================
        // LOAD CATALOG
        // =========================================================

        private void LoadCatalogDataFromDb()
        {
            // Katalog se načítá z jednoho sdíleného AppDbContext (_db)
            var (tasks, materials) = _catalogService.LoadCatalog(_db);

            Tasks.Clear();
            foreach (var t in tasks) Tasks.Add(t);

            Materials.Clear();
            foreach (var m in materials) Materials.Add(m);
        }

        // =========================================================
        // ADD ITEMS
        // =========================================================

        public void AddWorkItem()
        {
            var item = new CalculationItemViewModel(_db);
            item.PropertyChanged += Item_PropertyChanged;
            WorkCalcItems.Add(item);
        }

        public void AddMaterialItem()
        {
            var item = new CalculationItemViewModel(_db);
            item.PropertyChanged += Item_PropertyChanged;
            MaterialItems.Add(item);
        }

        // =========================================================
        // DELETE ITEMS
        // =========================================================

        public void DeleteWorkItem(object? obj)
        {
            if (obj is CalculationItemViewModel item)
            {
                item.PropertyChanged -= Item_PropertyChanged;
                WorkCalcItems.Remove(item);
                Recalculate();
            }
        }

        public void DeleteMaterialItem(object? obj)
        {
            if (obj is CalculationItemViewModel item)
            {
                item.PropertyChanged -= Item_PropertyChanged;
                MaterialItems.Remove(item);
                Recalculate();
            }
        }

        // =========================================================
        // 🧹 RESET WORK ITEM – kompletní reset nové kaskády PRÁCE
        // ---------------------------------------------------------
        // Účel:
        //   • Vymaže veškerá data řádku PRÁCE (WorkTask → Specification → BaseMaterial → Position).
        //   • Resetuje množství, cenu, slevu.
        //   • Vyčistí všechny kaskádové kolekce (Available*).
        //   • Zachová logiku setterů (PropertyChanged, CanSelect...).
        //   • Přepočítá TOTAL.
        // =========================================================
        public void ResetWorkItem(object? obj)
        {
            if (obj is not CalculationItemViewModel item)
                return;

            // ---------------------------------------------------------
            // 🔍 ZJIŠTĚNÍ, zda je řádek vyplněný (pro potvrzovací dialog)
            // ---------------------------------------------------------
            bool isFilled =
                item.SelectedWorkTask != null ||
                item.SelectedWorkSpecification != null ||
                item.SelectedBaseMaterial != null ||
                item.SelectedPosition != null ||
                item.Quantity > 0 ||
                item.IsDiscountEnabled;

            if (isFilled)
            {
                if (MessageBox.Show(
                    "Opravdu chcete vymazat vyplněný řádek práce?",
                    "Potvrzení",
                    MessageBoxButton.OKCancel,
                    MessageBoxImage.Question) != MessageBoxResult.OK)
                    return;
            }

            // ---------------------------------------------------------
            // 🔴 RESET KASKÁDY PRÁCE (nová struktura)
            // ---------------------------------------------------------
            item.SelectedWorkTask = null;
            item.SelectedWorkSpecification = null;
            item.SelectedBaseMaterial = null;
            item.SelectedPosition = null;

            // ---------------------------------------------------------
            // 🧹 VYČIŠTĚNÍ KASKÁDOVÝCH KOLEKCÍ
            // ---------------------------------------------------------
            item.AvailableWorkSpecifications.Clear();
            item.AvailableBaseMaterials.Clear();
            item.AvailablePositions.Clear();

            // ---------------------------------------------------------
            // 🔢 RESET MNOŽSTVÍ
            // ---------------------------------------------------------
            item.Quantity = 0;

            // ---------------------------------------------------------
            // 💰 RESET CENY PRÁCE
            // ---------------------------------------------------------
            item.CalculatedWorkPrice = null;

            // ---------------------------------------------------------
            // 💸 RESET SLEVY
            // ---------------------------------------------------------
            item.IsDiscountEnabled = false;
            item.DiscountPercent = null;

            // ---------------------------------------------------------
            // 🔄 PŘEPOČET TOTAL
            // ---------------------------------------------------------
            item.NotifyCalculatedProperties();
            item.UpdateTotal();
        }


        // ============================================================
        // 🔄 ResetMaterialItem – vymazání jedné materiálové položky
        // ------------------------------------------------------------
        // • Vymaže všechny kroky materiálové kaskády:
        //      Category → ProductName → Supplier → Offer → Price
        // • Vymaže uložené hodnoty (MaterialUnit, MaterialPrice)
        // • Vymaže množství a slevu
        // • Starý model MaterialItem byl odstraněn → nepoužívá se
        // ============================================================
        public void ResetMaterialItem(object? obj)
        {
            if (obj is CalculationItemViewModel item)
            {
                // Kategorie + Název
                item.SelectedCategory = null;
                item.SelectedProductName = null;

                // Dodavatel + nabídka
                item.SelectedSupplier = null;
                item.SelectedOffer = null;

                // Cena od dodavatele (MaterialPrice objekt)
                item.SelectedMaterialPrice = null;

                // Jednotka + cena (nové properties)
                item.MaterialUnit = null;
                item.MaterialPrice = null;

                // Množství + sleva
                item.Quantity = 0;
                item.IsDiscountEnabled = false;
                item.DiscountPercent = null;

                Recalculate();
            }
        }

        // =========================================================
        // 🔴 GENEROVÁNÍ PDF FAKTURY Z AKTUÁLNÍ KALKULACE
        // =========================================================
        //
        // Postup:
        //  1) Z aktuálních WorkCalcItems a MaterialItems se sestaví řádky faktury
        //     pomocí InvoiceTemplateService.BuildInvoiceLines.
        //  2) Naplní se InvoiceItemData (číslo faktury, odběratel, data).
        //     Zatím je to jednoduché – údaje můžeš později napojit na ProjectData.
        //  3) Načtou se trvalé údaje dodavatele přes SupplierSettingsService.Load().
        //  4) Vytvoří se PDF přes InvoiceTemplateService.GeneratePdf().
        //  5) Do StatusText se zapíše informace o vygenerovaném souboru.
        //
        private void GenerateInvoicePdf()
        {
            // 1) Sestavení řádků faktury z kalkulace
            var template = new InvoiceTemplateService();
            var lines = template.BuildInvoiceLines(WorkCalcItems, MaterialItems);

            // 2) Naplnění základních fakturačních údajů
            //    Tady můžeš později napojit ProjectData (např. jméno zákazníka).
            var invoiceData = new InvoiceItemData
            {
                InvoiceNumber = "2026-001",              // TODO: automatické číslování
                IssueDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(14),
                CustomerName = "Zákazník",              // TODO: napojit na projekt
                CustomerAddress = "Adresa",             // TODO: napojit na projekt
                Lines = lines
            };

            // 3) Načtení dodavatele z JSON přes SupplierSettingsService
            var supplier = _supplierSettingsService.Load();

            // 4) Cesta k PDF – pro začátek na plochu
            var outputPath = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                $"Faktura_{invoiceData.InvoiceNumber}.pdf");

            // 5) Generování PDF přes InvoiceTemplateService
            template.GeneratePdf(invoiceData, supplier, outputPath);

            // 6) Stavová hláška do status baru
            StatusText = $"PDF vygenerováno: {outputPath}";
        }

        // =========================================================
        // PROPERTY CHANGED – reagujeme jen na změny ovlivňující cenu
        // =========================================================
        private void Item_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // 🔒 Během načítání projektu se nesmí nic přepočítávat
            if (_isLoading)
                return;

            // 🔧 Změny, které ovlivňují cenu práce
            if (e.PropertyName == nameof(CalculationItemViewModel.SelectedWorkTask) ||
                e.PropertyName == nameof(CalculationItemViewModel.SelectedWorkSpecification) ||
                e.PropertyName == nameof(CalculationItemViewModel.SelectedBaseMaterial) ||
                e.PropertyName == nameof(CalculationItemViewModel.SelectedPosition))
            {
                MarkAsChanged();
                Recalculate();
                return;
            }

            // 🔧 Změny, které ovlivňují cenu materiálu
            if (e.PropertyName == nameof(CalculationItemViewModel.SelectedCategory) ||
                e.PropertyName == nameof(CalculationItemViewModel.SelectedProductName) ||
                e.PropertyName == nameof(CalculationItemViewModel.SelectedSupplier) ||
                e.PropertyName == nameof(CalculationItemViewModel.SelectedOffer) ||
                e.PropertyName == nameof(CalculationItemViewModel.SelectedMaterialPrice))
            {
                MarkAsChanged();
                Recalculate();
                return;
            }

            // 🔧 Společné hodnoty (množství + sleva)
            if (e.PropertyName == nameof(CalculationItemViewModel.Quantity) ||
                e.PropertyName == nameof(CalculationItemViewModel.IsDiscountEnabled) ||
                e.PropertyName == nameof(CalculationItemViewModel.DiscountPercent))
            {
                MarkAsChanged();
                Recalculate();
                return;
            }
        }

        // =========================================================
        // RECALCULATE – přepočet celkové ceny projektu
        // ---------------------------------------------------------
        // • Práce: cena se počítá přes CalculationPriceService → x.Total.
        // • Materiál: cena se bere ze SelectedMaterialPrice.
        // • BaseTotal() počítá cenu BEZ slevy (pro výpočet slev a rozpisu).
        // • Zachovává se výpočet slev, ceny před slevou a detailní rozpočet.
        // =========================================================
        public void Recalculate()
        {
            // --------------------------------------------------------
            // Lokální funkce – základní cena položky před slevou
            // --------------------------------------------------------
            // • Vrací cenu BEZ slevy.
            // • x.Total obsahuje cenu PO slevě → proto musíme základ spočítat znovu.
            // • Práce: BasePrice × MaterialCoef × PositionCoef × Quantity.
            // • Materiál: Price × Quantity.
            // --------------------------------------------------------
            static double BaseTotal(CalculationItemViewModel x)
            {
                // =====================================================
                // 🔵 PRÁCE – cena bez slevy
                // Podmínka: všechny kroky pracovní kaskády musí být vybrané.
                // BasePrice je decimal → koeficienty jsou double → převádíme na decimal.
                // =====================================================
                if (x.SelectedWorkTask != null &&
                    x.SelectedBaseMaterial != null &&
                    x.SelectedPosition != null)
                {
                    decimal basePrice =
                        x.SelectedWorkTask.BasePrice *
                        (decimal)x.SelectedBaseMaterial.MaterialCoef *
                        (decimal)x.SelectedPosition.PositionCoef *
                        (decimal)x.Quantity;

                    return (double)basePrice; // projekt používá double pro Total
                }

                // =====================================================
                // 🔵 MATERIÁL – cena bez slevy
                // Price je decimal → Quantity převedeme na decimal.
                // =====================================================
                if (x.SelectedMaterialPrice != null)
                    return (double)(x.SelectedMaterialPrice.Price * (decimal)x.Quantity);

                return 0;
            }

            // ============================
            // 1) CELKOVÉ CENY PO SLEVĚ
            // ============================
            WorkTotal = WorkCalcItems.Sum(x => x.Total);
            MaterialTotal = MaterialItems.Sum(x => x.Total);
            GrandTotal = WorkTotal + MaterialTotal;

            // ============================
            // 2) SLEVY (KOLIK SE UŠETŘILO)
            // ============================
            WorkDiscountTotal = WorkCalcItems
                .Where(x => x.IsDiscountEnabled && x.DiscountPercent.HasValue)
                .Sum(x => BaseTotal(x) - x.Total);

            MaterialDiscountTotal = MaterialItems
                .Where(x => x.IsDiscountEnabled && x.DiscountPercent.HasValue)
                .Sum(x => BaseTotal(x) - x.Total);

            TotalDiscount = WorkDiscountTotal + MaterialDiscountTotal;

            // ============================
            // 3) CENA PŘED SLEVOU
            // ============================
            double workBefore = WorkCalcItems.Sum(x => BaseTotal(x));
            double materialBefore = MaterialItems.Sum(x => BaseTotal(x));
            GrandTotalBeforeDiscount = workBefore + materialBefore;

            // ============================
            // 4) VIDITELNOST SEKCE SLEV
            // ============================
            HasAnyDiscount =
                WorkCalcItems.Any(x => x.IsDiscountEnabled && x.DiscountPercent.HasValue) ||
                MaterialItems.Any(x => x.IsDiscountEnabled && x.DiscountPercent.HasValue);

            // ============================
            // 5) DETAILNÍ ROZPOČET
            // ============================
            BudgetItems.Clear();

            // ------------------------------------------------------------
            // 🔵 PRÁCE – položky práce
            // ------------------------------------------------------------
            // • Všechny hodnoty jsou bezpečně přístupné přes ?.
            // • Description používá ?? "" aby se zabránilo CS8602.
            // ------------------------------------------------------------
            foreach (var x in WorkCalcItems.Where(x => x.Total > 0))
            {
                double basePrice = BaseTotal(x);
                double discountAmount = basePrice - x.Total;

                BudgetItems.Add(new BudgetItem
                {
                    Type = "PRÁCE",

                    // Bezpečné sestavení popisu – žádný null dereference
                    Description =
                        $"{x.SelectedWorkTask?.Name ?? ""} / " +
                        $"{x.SelectedWorkSpecification?.Name ?? ""} / " +
                        $"{x.SelectedBaseMaterial?.Name ?? ""} / " +
                        $"{x.SelectedPosition?.Name ?? ""}",

                    Unit = x.SelectedWorkSpecification?.Unit ?? "",
                    Quantity = x.Quantity,
                    Price = x.Total,

                    DiscountPercent =
                        (x.IsDiscountEnabled && x.DiscountPercent.HasValue)
                            ? x.DiscountPercent
                            : null,

                    DiscountAmount =
                        discountAmount > 0.0001
                            ? discountAmount
                            : null
                });
            }

            // ------------------------------------------------------------
            // 🔵 MATERIÁL – položky materiálu
            // ------------------------------------------------------------
            // • SelectedMaterialPrice je ověřeno v Where() → kompilátor ale stále
            //   nedokáže zaručit, že nebude null (C# warning CS8602).
            // • Proto si hodnotu uložíme do lokální proměnné priceObj, která už
            //   NEMŮŽE být null – tím warning zmizí.
            // • Description používá ?? "" aby se zabránilo null dereference.
            // • Unit používá priceObj.Unit ?? "" – typově bezpečné.
            // ------------------------------------------------------------
            foreach (var x in MaterialItems.Where(x => x.Total > 0 && x.SelectedMaterialPrice != null))
            {
                // ✔ Lokální proměnná – kompilátor ví, že není null
                var priceObj = x.SelectedMaterialPrice!;

                decimal price = priceObj.Price;
                double basePrice = (double)(price * (decimal)x.Quantity);
                double discountAmount = basePrice - x.Total;

                BudgetItems.Add(new BudgetItem
                {
                    Type = "MATERIÁL",

                    // ✔ Bezpečné sestavení popisu – žádný null dereference
                    Description =
                        $"{x.SelectedCategory ?? ""} / " +
                        $"{x.SelectedProductName ?? ""} / " +
                        $"{x.SelectedSupplier ?? ""} / " +
                        $"{x.SelectedOffer ?? ""}",

                    // ✔ priceObj.Unit může být null → proto ?? ""
                    Unit = priceObj.Unit ?? "",

                    Quantity = x.Quantity,
                    Price = x.Total,

                    DiscountPercent =
                        (x.IsDiscountEnabled && x.DiscountPercent.HasValue)
                            ? x.DiscountPercent
                            : null,

                    DiscountAmount =
                        discountAmount > 0.0001
                            ? discountAmount
                            : null
                });
            }
        }

        // =========================================================
        // CHANGE TRACKING
        // =========================================================

        private void MarkAsChanged()
        {
            _hasUnsavedChanges = true;

            if (!string.IsNullOrEmpty(_currentFilePath))
                StatusText = System.IO.Path.GetFileName(_currentFilePath) + " *";
            else
                StatusText = "Nový projekt *";
        }

        // =========================================================
        // SAVE / LOAD
        // =========================================================

        public void Save()
        {
            var data = BuildProjectData();
            var savedPath = _projectService.Save(data, _currentFilePath);

            if (savedPath != null)
            {
                _currentFilePath = savedPath;
                _hasUnsavedChanges = false;
                StatusText = savedPath;
            }
        }

        public void SaveAs()
        {
            var data = BuildProjectData();
            var savedPath = _projectService.SaveAs(data);

            if (savedPath != null)
            {
                _currentFilePath = savedPath;
                _hasUnsavedChanges = false;
                StatusText = savedPath;
            }
        }

        public void Load()
        {
            var (data, path) = _projectService.Load();
            if (data == null) return;

            ApplyProjectData(data, path!);
        }

        // =========================================================
        // NEW PROJECT
        // =========================================================

        public void ResetToNewProject()
        {
            ClearAllItems();

            for (int i = 0; i < 5; i++)
            {
                AddWorkItem();
                AddMaterialItem();
            }

            _currentFilePath = null;
            _hasUnsavedChanges = false;

            StatusText = "Nový projekt";
        }

        // =========================================================
        // CLEAR ITEMS
        // =========================================================

        private void ClearAllItems()
        {
            // 🔒 vypnout reakce na změny
            _isLoading = true;

            foreach (var item in WorkCalcItems)
                item.PropertyChanged -= Item_PropertyChanged;

            foreach (var item in MaterialItems)
                item.PropertyChanged -= Item_PropertyChanged;

            // ❗ Kolekce se musí opravdu vymazat
            WorkCalcItems.Clear();
            MaterialItems.Clear();

            // Rozpočet se může mazat – nemá pozice
            BudgetItems.Clear();

            _isLoading = false;
        }

        // =========================================================
        // BUILD PROJECT DATA – vytvoření kompletního ProjectData objektu
        // =========================================================
        //
        // Tento blok vytváří tři oddělené datové sekce:
        //
        //   1) WorkItems        → metadata řádků PRÁCE (WorkItemData)
        //   2) MaterialItems    → metadata řádků MATERIÁLU (MaterialItemData)
        //   3) CommonItems      → společné hodnoty (CalculationItemData)
        //
        // Architektura:
        //
        //   • WorkItemData / MaterialItemData
        //       – obsahují pouze metadata (Id + Position)
        //       – neobsahují pracovní ani materiálové hodnoty
        //
        //   • CalculationItemData
        //       – obsahuje kompletní pracovní nebo materiálový obsah
        //       – obsahuje společné hodnoty (Quantity, sleva, Total)
        //
        // Výhody:
        //
        //   • PRÁCE a MATERIÁL se nemíchají
        //   • společné hodnoty jsou opravdu společné
        //   • JSON je čistý, přehledný a stabilní
        //   • Load/Save je jednoznačný díky párování přes Id
        //
        // ❗ Prázdné řádky se neukládají (item.IsEmpty)
        // ❗ Každý řádek má vlastní ID → jednoznačné párování PRÁCE/MATERIÁL ↔ SPOLEČNÉ
        // ❗ ID je krátké a čitelné: PRÁCE = W-1, W-2… / MATERIÁL = M-1, M-2…
        //
        // 🔴 ZMĚNA (Id vs. Position):
        // ----------------------------------------------------------------
        // Id a Position mají nyní odlišný účel:
        //
        //   • Id
        //       – sekvenční identifikátor záznamu (W-1, W-2, W-3...)
        //       – generovaný čítačem POUZE pro vyplněné řádky
        //       – slouží výhradně k párování se CalculationItemData
        //
        //   • Position
        //       – skutečná pozice řádku v UI (1-based)
        //       – zjištěná z indexu v kolekci JEŠTĚ PŘED odfiltrováním prázdných řádků
        //
        // Např. pokud je vyplněný jen 1. a 5. řádek:
        //   • 1. řádek → Id = "W-1", Position = 1
        //   • 5. řádek → Id = "W-2", Position = 5
        //
        // Díky tomu:
        //   • Id zůstává sekvenční („druhý vyplněný záznam“)
        //   • Position přesně určuje, kam se má řádek vrátit v UI při načítání projektu
        // =========================================================

        private ProjectData BuildProjectData()
        {
            // ---------------------------------------------------------
            // Interní čítače pro generování krátkých ID
            // ---------------------------------------------------------
            int workCounter = 1;     // W-1, W-2, W-3...
            int materialCounter = 1; // M-1, M-2, M-3...

            // ---------------------------------------------------------
            // 🔧 PRÁCE → WorkItemData + CalculationItemData
            // ---------------------------------------------------------
            //
            // Ukládá se:
            //   • WorkItemData → pouze metadata (Id + Position)
            //   • CalculationItemData → kompletní pracovní obsah
            //
            // Postup:
            //   1) Select((x, i) => ...) – získáme skutečnou pozici řádku (Position = i + 1)
            //   2) Where(!IsEmpty) – odfiltrujeme prázdné řádky
            //   3) Generujeme krátké ID (W-1, W-2…)
            //   4) Ukládáme metadata + pracovní obsah
            //
            var workItemsWithCommon = WorkCalcItems
                .Select((x, i) => new { Item = x, Position = i + 1 }) // 🔴 pozice řádku PŘED filtrováním
                .Where(x => !x.Item.IsEmpty)
                .Select(x =>
                {
                    var id = $"W-{workCounter++}"; // krátké, čitelné, sekvenční ID

                    return new
                    {
                        // 🔵 Metadata řádku PRÁCE
                        Work = new WorkItemData
                        {
                            Id = id,
                            Position = x.Position
                        },

                        // 🔵 Společné + pracovní hodnoty
                        Common = new CalculationItemData
                        {
                            Id = id,

                            // 🔴 Nová typově bezpečná kaskáda PRÁCE
                            WorkTaskName = x.Item.SelectedWorkTask?.Name,
                            WorkSpecificationName = x.Item.SelectedWorkSpecification?.Name,
                            BaseMaterialName = x.Item.SelectedBaseMaterial?.Name,
                            PositionName = x.Item.SelectedPosition?.Name,

                            // 🔴 Jednotka práce + cena práce (z WorkTask.BasePrice)
                            WorkUnit = x.Item.SelectedWorkSpecification?.Unit,
                            WorkPrice = x.Item.SelectedWorkTask?.BasePrice,

                            // 🔴 Společné hodnoty
                            Quantity = x.Item.Quantity,
                            DiscountPercent = x.Item.DiscountPercent,
                            IsDiscountEnabled = x.Item.IsDiscountEnabled,
                            Total = x.Item.Total
                        }
                    };
                })
                .ToList();

            var workItems = workItemsWithCommon.Select(x => x.Work).ToList();
            var commonItems = workItemsWithCommon.Select(x => x.Common).ToList();


            // ---------------------------------------------------------
            // 🔧 MATERIÁL → MaterialItemData + CalculationItemData
            // ---------------------------------------------------------
            //
            // Ukládá se:
            //   • MaterialItemData → metadata (Id + Position)
            //   • CalculationItemData → kompletní materiálový obsah
            //
            var materialItemsWithCommon = MaterialItems
                .Select((x, i) => new { Item = x, Position = i + 1 })
                .Where(x => !x.Item.IsEmpty)
                .Select(x =>
                {
                    var id = $"M-{materialCounter++}";

                    return new
                    {
                        Material = new MaterialItemData
                        {
                            Id = id,
                            Position = x.Position
                        },

                        Common = new CalculationItemData
                        {
                            Id = id,

                            // 🔴 Materiálová kaskáda
                            CategoryName = x.Item.SelectedCategory,
                            ProductName = x.Item.SelectedProductName,
                            SupplierName = x.Item.SelectedSupplier,
                            OfferName = x.Item.SelectedOffer,

                            // 🔴 Cena materiálu (jen hodnoty, ne celý objekt!)
                            MaterialPrice = x.Item.SelectedMaterialPrice?.Price,
                            MaterialUnit = x.Item.SelectedMaterialPrice?.Unit,

                            // 🔴 Společné hodnoty
                            Quantity = x.Item.Quantity,
                            DiscountPercent = x.Item.DiscountPercent,
                            IsDiscountEnabled = x.Item.IsDiscountEnabled,
                            Total = x.Item.Total
                        }
                    };
                })
                .ToList();

            var materialItems = materialItemsWithCommon.Select(x => x.Material).ToList();
            commonItems.AddRange(materialItemsWithCommon.Select(x => x.Common));

            // ---------------------------------------------------------
            // 🔧 FINÁLNÍ PROJECT DATA
            // ---------------------------------------------------------
            return new ProjectData
            {
                WorkItems = workItems,
                MaterialItems = materialItems,
                CommonItems = commonItems
            };
        }

        // ============================================================================
        // 📥 APPLY PROJECT DATA – načtení uloženého projektu do ViewModelů
        // ============================================================================
        //
        // Nový datový model:
        //   • WorkItems        → metadata (Id + Position)
        //   • MaterialItems    → metadata (Id + Position)
        //   • CommonItems      → kompletní obsah PRÁCE i MATERIÁLU
        //
        // ❗ Prázdné řádky se NEukládají do JSONu.
        // ❗ Pozice řádků se obnovují podle pole Position (ne podle Id).
        // ❗ Id slouží výhradně k párování s CommonItems.
        //
        // Logika:
        // -------
        // 1) Vytvoří se dostatečný počet prázdných řádků (WorkCalcItems / MaterialCalcItems)
        //    podle nejvyšší hodnoty Position v uložených datech.
        // 2) Uložené řádky se vloží na index = Position - 1.
        // 3) Společné hodnoty se načtou podle Id.
        // 4) Typově bezpečná kaskáda PRÁCE a MATERIÁLU se obnoví podle názvů uložených v CommonItems.
        // 5) UI se přesně obnoví včetně mezer mezi řádky.
        //
        // ============================================================================

        private void ApplyProjectData(ProjectData data, string path)
        {
            _isLoading = true;
            ClearAllItems();

            var workItems = data.WorkItems;
            var materialItems = data.MaterialItems;
            var commonItems = data.CommonItems;

            // =====================================================================
            // 🔧 1) Vytvoření dostatečného počtu prázdných řádků
            // =====================================================================

            const int minRowCount = 5;

            int maxWorkRow = workItems.Count > 0 ? workItems.Max(w => w.Position) : 0;
            int maxMaterialRow = materialItems.Count > 0 ? materialItems.Max(m => m.Position) : 0;

            int workRowCount = Math.Max(minRowCount, maxWorkRow);
            int materialRowCount = Math.Max(minRowCount, maxMaterialRow);

            for (int i = 0; i < workRowCount; i++)
            {
                var emptyWork = new CalculationItemViewModel(_db);
                emptyWork.PropertyChanged += Item_PropertyChanged;
                WorkCalcItems.Add(emptyWork);
            }

            for (int i = 0; i < materialRowCount; i++)
            {
                var emptyMaterial = new CalculationItemViewModel(_db);
                emptyMaterial.PropertyChanged += Item_PropertyChanged;
                MaterialItems.Add(emptyMaterial);
            }

            // =====================================================================
            // 🔧 2) PRÁCE – načtení WorkItemData + společných hodnot podle ID
            // =====================================================================

            foreach (var savedWork in workItems)
            {
                int index = savedWork.Position - 1;
                var item = WorkCalcItems[index];

                // ---------------------- METADATA ----------------------
                item.Id = savedWork.Id;

                // ---------------------- SPOLEČNÉ ----------------------
                var savedCommon = commonItems.First(c => c.Id == savedWork.Id);

                item.Quantity = savedCommon.Quantity;
                item.DiscountPercent = savedCommon.DiscountPercent;
                item.IsDiscountEnabled = savedCommon.IsDiscountEnabled;

                // ---------------------- PRÁCE – typově bezpečná kaskáda ----------------------
                item.SelectedWorkTask =
                    _db.WorkTasks.FirstOrDefault(t => t.Name == savedCommon.WorkTaskName);

                item.SelectedWorkSpecification =
                    _db.WorkSpecifications.FirstOrDefault(s => s.Name == savedCommon.WorkSpecificationName);

                item.SelectedBaseMaterial =
                    _db.BaseMaterials.FirstOrDefault(m => m.Name == savedCommon.BaseMaterialName);

                item.SelectedPosition =
                    _db.Positions.FirstOrDefault(p => p.Name == savedCommon.PositionName);

                // Jednotka a cena práce (z WorkSpecification)
                item.WorkUnit = savedCommon.WorkUnit;
                item.WorkPrice = savedCommon.WorkPrice;

                item.UpdateTotal();
            }

            // =====================================================================
            // 📦 3) MATERIÁL – načtení MaterialItemData + společných hodnot podle ID
            // =====================================================================

            foreach (var savedMaterial in materialItems)
            {
                int index = savedMaterial.Position - 1;
                var item = MaterialItems[index];

                // ---------------------- METADATA ----------------------
                item.Id = savedMaterial.Id;

                // ---------------------- SPOLEČNÉ ----------------------
                var savedCommon = commonItems.First(c => c.Id == savedMaterial.Id);

                item.Quantity = savedCommon.Quantity;
                item.DiscountPercent = savedCommon.DiscountPercent;
                item.IsDiscountEnabled = savedCommon.IsDiscountEnabled;

                // ---------------------- MATERIÁL – typově bezpečná kaskáda ----------------------
                item.SelectedCategory = savedCommon.CategoryName ?? "";
                item.SelectedProductName = savedCommon.ProductName ?? "";
                item.SelectedSupplier = savedCommon.SupplierName ?? "";
                item.SelectedOffer = savedCommon.OfferName ?? "";

                // ❗ Oprava CS0029 – decimal? → MaterialPrice (model)
                item.SelectedMaterialPrice = savedCommon.MaterialPrice is null
                    ? null
                    : new MaterialPrice
                    {
                        Price = savedCommon.MaterialPrice ?? 0m,
                        Unit = savedCommon.MaterialUnit ?? ""
                    };

                item.UpdateTotal();
            }

            // =====================================================================
            // 🔄 4) Přepočet všech hodnot po načtení
            // =====================================================================
            _isLoading = false;
            Recalculate();
        }

        // =========================================================
        // 🚪 EXIT & CAN CLOSE LOGIKA
        // =========================================================
        //
        // Tento blok řeší bezpečné ukončení aplikace:
        //   • kontrola prázdného projektu
        //   • kontrola neuložených změn
        //   • nabídka uložení před ukončením
        //   • typově bezpečné dialogy přes IMessageService
        //
        // Architektura:
        //   • MainViewModel NEVOLÁ přímo MessageBox ani WPF API
        //   • veškeré dialogy jsou přes IMessageService
        //   • ukončení aplikace je přes IApplicationService
        //   • logika prázdného projektu je typově bezpečná (nové názvy properties)
        // =========================================================

        public void Exit()
        {
            // Jednoduché ukončení aplikace přes službu
            _applicationService.Shutdown();
        }

        public bool CanClose()
        {
            bool isNewProject = _currentFilePath == null;
            bool isEmpty = IsProjectEmpty();

            // ---------------------------------------------------------
            // 🟦 1) Nový + prázdný projekt → není co ukládat
            // ---------------------------------------------------------
            if (isNewProject && isEmpty)
                return _messageService.ShowYesNo("Opravdu ukončit bez uložení?", "Potvrzení");

            // ---------------------------------------------------------
            // 🟦 2) Neuložené změny → nabídka uložení
            // ---------------------------------------------------------
            if (_hasUnsavedChanges)
            {
                var result = _messageService.ShowYesNoCancel(
                    "Opravdu chcete ukončit aplikaci bez uložení?",
                    "Neuložené změny");

                // Uživatel zrušil akci
                if (result == MessageBoxResult.Cancel)
                    return false;

                // Uživatel chce uložit
                if (result == MessageBoxResult.No)
                {
                    var saved = _projectService.Save(BuildProjectData(), _currentFilePath);
                    if (saved == null)
                        return false; // uložení selhalo nebo bylo zrušeno
                }
            }

            // ---------------------------------------------------------
            // 🟦 3) Bez neuložených změn → lze ukončit
            // ---------------------------------------------------------
            return true;
        }

        // =========================================================
        // 🔍 IsProjectEmpty – typově bezpečná kontrola prázdného projektu
        // =========================================================
        //
        // Projekt je prázdný, pokud:
        //   • všechny řádky PRÁCE jsou prázdné (nové názvy properties)
        //   • všechny řádky MATERIÁLU jsou prázdné (nové názvy properties)
        //   • množství je 0
        //
        // Tato metoda je klíčová pro rozhodnutí, zda se má ptát na uložení.
        // =========================================================

        private bool IsProjectEmpty()
        {
            // ---------------------------------------------------------
            // 🟦 PRÁCE – typově bezpečná kontrola
            // ---------------------------------------------------------
            bool workEmpty = WorkCalcItems.All(x =>
                x.SelectedWorkTask == null &&
                x.SelectedWorkSpecification == null &&
                x.SelectedBaseMaterial == null &&
                x.SelectedPosition == null &&
                x.Quantity == 0);

            // ---------------------------------------------------------
            // 🟦 MATERIÁL – typově bezpečná kontrola
            // ---------------------------------------------------------
            bool materialEmpty = MaterialItems.All(x =>
                x.SelectedCategory == null &&
                x.SelectedProductName == null &&
                x.SelectedSupplier == null &&
                x.SelectedOffer == null &&
                x.SelectedMaterialPrice == null &&
                x.Quantity == 0);

            return workEmpty && materialEmpty;
        }

        // =========================================================
        // PRINT
        // =========================================================

        public void Print()
        {
            var text = ExportAsText();
            _printService.Print(text);
        }

        // =========================================================
        // 📤 EXPORT TEXT – jednoduchý textový výpis kalkulace
        // =========================================================
        //
        // Tento export slouží jako rychlý textový přehled kalkulace.
        // Nepoužívá žádné formátování ani tabulky – jen čistý text.
        //
        // Architektura (aktuální):
        //   • PRÁCE:
        //       - používá SelectedWorkTask (string)
        //       - používá SelectedWorkSpecification (string)
        //       - používá SelectedBaseMaterial (string)
        //       - používá SelectedPosition (string)
        //       - používá WorkPrice (decimal?) z CalculationItemData
        //
        //   • MATERIÁL:
        //       - používá SelectedProductName (string)
        //       - používá SelectedMaterialPrice (model: Price + Unit)
        //
        // Staré vlastnosti (WorkItem, SelectedTask, MaterialItem…) byly odstraněny.
        // =========================================================

        public string ExportAsText()
        {
            var sb = new System.Text.StringBuilder();

            sb.AppendLine("=================================");
            sb.AppendLine("ELEKTRO OFFER - KALKULACE");
            sb.AppendLine("=================================");
            sb.AppendLine();

            // ---------------------------------------------------------
            // 🟦 PRÁCE
            // ---------------------------------------------------------
            sb.AppendLine("PRÁCE:");

            foreach (var item in WorkCalcItems)
            {
                // Pokud není vybraná práce → přeskočit
                if (item.SelectedWorkTask == null)
                    continue;

                // Základní cena práce (bez slevy)
                double basePrice =
                    (double)(item.WorkPrice ?? 0m) *
                    item.Quantity;

                if (item.IsDiscountEnabled && item.DiscountPercent.HasValue)
                {
                    double discount = basePrice - item.Total;

                    sb.AppendLine($"  {item.SelectedWorkTask} | {item.Quantity}");
                    sb.AppendLine($"    Cena bez slevy:  {basePrice:N0} Kč");
                    sb.AppendLine($"    Sleva:           -{discount:N0} Kč ({item.DiscountPercent:N0} %)");
                    sb.AppendLine($"    Cena se slevou:  {item.Total:N0} Kč");
                }
                else
                {
                    sb.AppendLine($"  {item.SelectedWorkTask} | {item.Quantity} | {item.Total:N0} Kč");
                }
            }

            sb.AppendLine();
            sb.AppendLine("MATERIÁL:");

            // ---------------------------------------------------------
            // 🟦 MATERIÁL
            // ---------------------------------------------------------
            foreach (var item in MaterialItems)
            {
                // Pokud není vybraná cena materiálu → přeskočit
                if (item.SelectedMaterialPrice == null)
                    continue;

                double basePrice = (double)item.SelectedMaterialPrice.Price * item.Quantity;

                if (item.IsDiscountEnabled && item.DiscountPercent.HasValue)
                {
                    double discount = basePrice - item.Total;

                    sb.AppendLine($"  {item.SelectedProductName} | {item.Quantity}");
                    sb.AppendLine($"    Cena bez slevy:  {basePrice:N0} Kč");
                    sb.AppendLine($"    Sleva:           -{discount:N0} Kč ({item.DiscountPercent:N0} %)");
                    sb.AppendLine($"    Cena se slevou:  {item.Total:N0} Kč");
                }
                else
                {
                    sb.AppendLine($"  {item.SelectedProductName} | {item.Quantity} | {item.Total:N0} Kč");
                }
            }

            return sb.ToString();
        }

        // =========================================================
        // ABOUT
        // =========================================================
        public void ShowAbout()
        {
            // ViewModel nezná AboutWindow, volá jen abstrakci IWindowService.
            _windowService.ShowAbout();
        }

        // =========================================================
        // INotifyPropertyChanged
        // =========================================================
        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
