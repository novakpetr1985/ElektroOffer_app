using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ElektroOffer_app.Commands;
using ElektroOffer_app.Data;
using ElektroOffer_app.Models;
using ElektroOffer_app.Services;
using ElektroOffer_app.ViewModels.Items;

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
        private readonly CalculationCascadeService _cascade;
        private readonly CalculationPriceService _price;
        private readonly AppDbContext _db;

        // UI služby – ViewModel nevolá WPF přímo
        private readonly IMessageService _messageService;
        private readonly IPrintService _printService;
        private readonly IApplicationService _applicationService;
        private readonly IWindowService _windowService;

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
            CalculationCascadeService cascade,
            CalculationPriceService price,
            AppDbContext db,
            IMessageService messageService,
            IPrintService printService,
            IApplicationService applicationService,
            IWindowService windowService)
        {
            // DI – všechny služby + jeden sdílený AppDbContext
            _projectService = projectService;
            _catalogService = catalogService;
            _cascade = cascade;
            _price = price;
            _db = db;

            _messageService = messageService;
            _printService = printService;
            _applicationService = applicationService;
            _windowService = windowService;

            LoadCatalogDataFromDb();

            // Výchozí položky (stejně jako původní MainWindow)
            for (int i = 0; i < 5; i++)
            {
                AddWorkItem();
                AddMaterialItem();
            }

            // Reakce na změny kolekcí
            WorkCalcItems.CollectionChanged += (_, __) => { MarkAsChanged(); Recalculate(); };
            MaterialItems.CollectionChanged += (_, __) => { MarkAsChanged(); Recalculate(); };

            // Inicializace příkazů
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
        // RESET ITEMS
        // =========================================================

        public void ResetWorkItem(object? obj)
        {
            if (obj is CalculationItemViewModel item)
            {
                item.SelectedTask = null;
                item.SelectedSpecification = null;
                item.SelectedMaterial = null;
                item.SelectedLocation = null;
                item.Quantity = 0;
                item.IsDiscountEnabled = false;
                item.DiscountPercent = null;

                Recalculate();
            }
        }

        public void ResetMaterialItem(object? obj)
        {
            if (obj is CalculationItemViewModel item)
            {
                // Kategorie + Název (to ti chybělo)
                item.SelectedCategory = null;
                item.SelectedProductName = null;

                // Dodavatel + nabídka
                item.SelectedSupplier = null;
                item.SelectedOffer = null;

                // Cena od dodavatele
                item.SelectedMaterialPrice = null;

                // Starý model
                item.MaterialItem = null;

                // Množství + sleva
                item.Quantity = 0;
                item.IsDiscountEnabled = false;
                item.DiscountPercent = null;

                Recalculate();
            }
        }

        // =========================================================
        // PROPERTY CHANGED
        // =========================================================

        private void Item_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // 🔒 Během načítání projektu se nesmí nic přepočítávat ani spouštět
            if (_isLoading)
                return;

            // 🔧 Reagujeme jen na změny, které ovlivňují cenu
            if (e.PropertyName == nameof(CalculationItemViewModel.Total) ||
                e.PropertyName == nameof(CalculationItemViewModel.Quantity) ||
                e.PropertyName == nameof(CalculationItemViewModel.WorkItem) ||
                e.PropertyName == nameof(CalculationItemViewModel.MaterialItem))
            {
                MarkAsChanged();
                Recalculate();
            }
        }

        // =========================================================
        // RECALCULATE
        // =========================================================
        public void Recalculate()
        {
            static double BaseTotal(CalculationItemViewModel x)
            {
                if (x.WorkItem != null)
                    return x.WorkItem.BasePrice * x.WorkItem.MaterialCoef * x.WorkItem.PositionCoef * x.Quantity;

                // ✔ Tady je oprava – materiál počítat ze SelectedMaterialPrice
                if (x.SelectedMaterialPrice != null)
                    return (double)x.SelectedMaterialPrice.Price * x.Quantity;

                if (x.MaterialItem != null)
                    return x.MaterialItem.Price * x.Quantity;

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
            // 3) CENA PŘED SLEVOU (SPRÁVNĚ)
            // ============================
            double workBefore = WorkCalcItems.Sum(x => BaseTotal(x));
            double materialBefore = MaterialItems.Sum(x => BaseTotal(x));
            GrandTotalBeforeDiscount = workBefore + materialBefore;

            // ============================
            // 4) VIDITELNOST SEKCE SLEV
            // ============================
            HasAnyDiscount =
                WorkCalcItems.Any(x => x.IsDiscountEnabled && x.DiscountPercent.HasValue)
                || MaterialItems.Any(x => x.IsDiscountEnabled && x.DiscountPercent.HasValue);


            // ============================
            // 5) DETAILNÍ ROZPOČET
            // ============================
            BudgetItems.Clear();

            foreach (var x in WorkCalcItems.Where(x => x.Total > 0))
            {
                double basePrice = BaseTotal(x);
                double discountAmount = basePrice - x.Total;

                BudgetItems.Add(new BudgetItem
                {
                    Type = "PRÁCE",
                    Description = $"{x.SelectedTask} / {x.SelectedSpecification} / {x.SelectedMaterial} / {x.SelectedLocation}",
                    Unit = x.WorkUnit ?? "",
                    Quantity = x.Quantity,
                    Price = x.Total,
                    DiscountPercent = (x.IsDiscountEnabled && x.DiscountPercent.HasValue) ? x.DiscountPercent : null,
                    DiscountAmount = discountAmount > 0.0001 ? discountAmount : null
                });
            }

            foreach (var x in MaterialItems.Where(x => x.SelectedMaterialPrice != null))
            {
                double basePrice = (double)x.SelectedMaterialPrice!.Price * x.Quantity;
                double discountAmount = basePrice - x.Total;

                BudgetItems.Add(new BudgetItem
                {
                    Type = "MATERIÁL",
                    Description = x.SelectedOffer ?? "",
                    Unit = x.SelectedMaterialPrice?.Unit ?? "",
                    Quantity = x.Quantity,
                    Price = x.Total,
                    DiscountPercent = (x.IsDiscountEnabled && x.DiscountPercent.HasValue) ? x.DiscountPercent : null,
                    DiscountAmount = discountAmount > 0.0001 ? discountAmount : null
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
        //   1) WorkItems        → pracovní hodnoty (WorkItemData)
        //   2) MaterialItems    → materiálové hodnoty (MaterialItemData)
        //   3) CommonItems      → společné hodnoty (CalculationItemData)
        //
        // Díky tomu:
        // - PRÁCE a MATERIÁL se nemíchají
        // - společné hodnoty jsou opravdu společné
        // - JSON je čistý a přehledný
        // - Load/Save je stabilní
        //
        // ❗ Prázdné řádky se neukládají (item.IsEmpty)
        // ❗ Každý řádek má vlastní ID → jednoznačné párování PRÁCE/MATERIÁL ↔ SPOLEČNÉ
        // ❗ ID je krátké a čitelné: PRÁCE = W-1, W-2… / MATERIÁL = M-1, M-2…
        // =========================================================

        private ProjectData BuildProjectData()
        {
            // ---------------------------------------------------------
            // Interní čítače pro generování krátkých ID
            // ---------------------------------------------------------
            //
            // PRÁCE → W-1, W-2, W-3...
            // MATERIÁL → M-1, M-2, M-3...
            //
            int workCounter = 1;
            int materialCounter = 1;

            // ---------------------------------------------------------
            // 🔧 PRÁCE → WorkItemData
            // ---------------------------------------------------------
            //
            // Ukládají se pouze pracovní hodnoty:
            // - SelectedTask
            // - SelectedSpecification
            // - SelectedMaterial
            // - SelectedLocation
            // - SelectedWorkPrice (volitelné)
            // - SelectedWorkUnit  (volitelné)
            //
            // Každý řádek dostane vlastní ID (W-1, W-2…), které se použije i ve společné sekci.
            // Prázdné řádky se odfiltrují pomocí item.IsEmpty.
            //
            var workItemsWithCommon = WorkCalcItems
                .Where(x => !x.IsEmpty)
                .Select(x =>
                {
                    var id = $"W-{workCounter++}"; // krátké, čitelné ID

                    return new
                    {
                        Work = new WorkItemData
                        {
                            Id = id,
                            SelectedTask = x.SelectedTask,
                            SelectedSpecification = x.SelectedSpecification,
                            SelectedMaterial = x.SelectedMaterial,
                            SelectedLocation = x.SelectedLocation,
                            SelectedWorkPrice = x.SelectedWorkPrice,
                            SelectedWorkUnit = x.SelectedWorkUnit
                        },
                        Common = new CalculationItemData
                        {
                            Id = id,
                            Quantity = x.Quantity,
                            DiscountPercent = x.DiscountPercent,
                            IsDiscountEnabled = x.IsDiscountEnabled,
                            Total = x.Total
                        }
                    };
                })
                .ToList();

            var workItems = workItemsWithCommon.Select(x => x.Work).ToList();


            // ---------------------------------------------------------
            // 📦 MATERIÁL → MaterialItemData
            // ---------------------------------------------------------
            //
            // Ukládají se pouze materiálové hodnoty:
            // - SelectedCategory
            // - SelectedProductName
            // - SelectedSupplier
            // - SelectedOffer
            // - SelectedMaterialPrice (volitelné)
            // - SelectedMaterialUnit  (volitelné)
            //
            // Každý řádek dostane vlastní ID (M-1, M-2…), které se použije i ve společné sekci.
            // Prázdné řádky se odfiltrují pomocí item.IsEmpty.
            //
            var materialItemsWithCommon = MaterialItems
                .Where(x => !x.IsEmpty)
                .Select(x =>
                {
                    var id = $"M-{materialCounter++}"; // krátké, čitelné ID

                    return new
                    {
                        Material = new MaterialItemData
                        {
                            Id = id,
                            SelectedCategory = x.SelectedCategory,
                            SelectedProductName = x.SelectedProductName,
                            SelectedSupplier = x.SelectedSupplier,
                            SelectedOffer = x.SelectedOffer,
                            SelectedMaterialPrice = x.SelectedMaterialPriceValue,
                            SelectedMaterialUnit = x.SelectedMaterialUnit
                        },
                        Common = new CalculationItemData
                        {
                            Id = id,
                            Quantity = x.Quantity,
                            DiscountPercent = x.DiscountPercent,
                            IsDiscountEnabled = x.IsDiscountEnabled,
                            Total = x.Total
                        }
                    };
                })
                .ToList();

            var materialItems = materialItemsWithCommon.Select(x => x.Material).ToList();


            // ---------------------------------------------------------
            // 🧮 SPOLEČNÉ → CalculationItemData
            // ---------------------------------------------------------
            //
            // Společné hodnoty jsou stejné pro PRÁCI i MATERIÁL:
            // - Quantity
            // - DiscountPercent
            // - IsDiscountEnabled
            // - Total
            //
            // Každý řádek má ID shodné s WorkItemData nebo MaterialItemData.
            // Díky tomu lze jednoznačně spárovat položky při načítání.
            //
            var commonItems =
                workItemsWithCommon.Select(x => x.Common)
                .Concat(materialItemsWithCommon.Select(x => x.Common))
                .ToList();


            // ---------------------------------------------------------
            // Sestavení ProjectData
            // ---------------------------------------------------------
            return new ProjectData
            {
                ProjectName = _currentFilePath != null
                    ? System.IO.Path.GetFileNameWithoutExtension(_currentFilePath)
                    : "Nový projekt",

                SavedAt = DateTime.Now,

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
        //   • WorkItems        → pouze pracovní hodnoty
        //   • MaterialItems    → pouze materiálové hodnoty
        //   • CommonItems      → Quantity, sleva, Total
        //
        // ❗ Prázdné řádky se NEukládají do JSONu.
        // ❗ Pozice řádků se obnovují podle ID (W-1 → řádek 1, W-5 → řádek 5).
        //
        // Logika:
        // -------
        // 1) Vytvoří se pevný počet prázdných řádků (např. 5).
        // 2) Načtou se uložené položky podle ID → vloží se na správné pozice.
        // 3) Společné hodnoty (CommonItems) se načítají podle stejného ID.
        // 4) UI tak přesně obnoví původní strukturu řádků.
        //
        // ============================================================================
        private void ApplyProjectData(ProjectData data, string path)
        {
            // 🔒 vypnout reakce na změny během načítání
            _isLoading = true;

            ClearAllItems();

            var workItems = data.WorkItems;
            var materialItems = data.MaterialItems;
            var commonItems = data.CommonItems;

            // =====================================================================
            // 🔧 1) Vytvoření pevného počtu prázdných řádků
            // =====================================================================
            //
            // Prázdné řádky se NEukládají do JSONu, ale UI je potřebuje.
            // Proto se zde vytvoří základní struktura (např. 5 řádků).
            //
            const int defaultRowCount = 5;

            for (int i = 0; i < defaultRowCount; i++)
            {
                var emptyWork = new CalculationItemViewModel(_db);
                emptyWork.PropertyChanged += Item_PropertyChanged;
                WorkCalcItems.Add(emptyWork);

                var emptyMaterial = new CalculationItemViewModel(_db);
                emptyMaterial.PropertyChanged += Item_PropertyChanged;
                MaterialItems.Add(emptyMaterial);
            }

            // =====================================================================
            // 🔧 PRÁCE – načtení WorkItemData + společných hodnot podle ID
            // =====================================================================
            //
            // ID má formát W-1, W-2, W-5...
            // → číslo za pomlčkou určuje pozici řádku.
            //
            // Prázdné řádky se NEukládají do JSONu,
            // proto se zde načítají jen vyplněné řádky a vkládají se na správné pozice.
            //
            foreach (var savedWork in workItems)
            {
                // W-5 → 5 → index 4
                int index = int.Parse(savedWork.Id.Split('-')[1]) - 1;

                var item = WorkCalcItems[index];

                // ---------------------- PRÁCE ----------------------
                item.Id = savedWork.Id;
                item.SelectedTask = savedWork.SelectedTask;
                item.SelectedSpecification = savedWork.SelectedSpecification;
                item.SelectedMaterial = savedWork.SelectedMaterial;
                item.SelectedLocation = savedWork.SelectedLocation;

                item.SelectedWorkPrice = savedWork.SelectedWorkPrice;
                item.SelectedWorkUnit = savedWork.SelectedWorkUnit;

                // ---------------------- SPOLEČNÉ ----------------------
                var savedCommon = commonItems.First(c => c.Id == savedWork.Id);

                item.Quantity = savedCommon.Quantity;
                item.DiscountPercent = savedCommon.DiscountPercent;
                item.IsDiscountEnabled = savedCommon.IsDiscountEnabled;
            }

            // =====================================================================
            // 📦 3) MATERIÁL – načtení MaterialItemData + společných hodnot podle ID
            // =====================================================================
            //
            // ID má formát M-1, M-2, M-5...
            // → číslo za pomlčkou určuje pozici řádku.
            //
            // Prázdné řádky se NEukládají do JSONu,
            // proto se zde načítají jen vyplněné řádky a vkládají se na správné pozice.
            //
            foreach (var savedMaterial in materialItems)
            {
                int index = int.Parse(savedMaterial.Id.Split('-')[1]) - 1;

                var item = MaterialItems[index];

                // ---------------------- MATERIÁL ----------------------
                item.Id = savedMaterial.Id;
                item.SelectedCategory = savedMaterial.SelectedCategory;
                item.SelectedProductName = savedMaterial.SelectedProductName;
                item.SelectedSupplier = savedMaterial.SelectedSupplier;
                item.SelectedOffer = savedMaterial.SelectedOffer;

                item.SelectedMaterialPriceValue = savedMaterial.SelectedMaterialPrice;
                item.SelectedMaterialUnit = savedMaterial.SelectedMaterialUnit;

                // ---------------------- SPOLEČNÉ ----------------------
                var savedCommon = commonItems.First(c => c.Id == savedMaterial.Id);

                item.Quantity = savedCommon.Quantity;
                item.DiscountPercent = savedCommon.DiscountPercent;
                item.IsDiscountEnabled = savedCommon.IsDiscountEnabled;
            }

            // =====================================================================
            // 🔄 4) Přepočet všech hodnot po načtení
            // =====================================================================
            _isLoading = false;
            Recalculate();
        }

        // =========================================================
        // EXIT
        // =========================================================

        public void Exit()
        {
            _applicationService.Shutdown();
        }

        public bool CanClose()
        {
            bool isNewProject = _currentFilePath == null;
            bool isEmpty = IsProjectEmpty();

            if (isNewProject && isEmpty)
                return _messageService.ShowYesNo("Opravdu ukončit bez uložení?", "Potvrzení");

            if (_hasUnsavedChanges)
            {
                var result = _messageService.ShowYesNoCancel(
                    "Opravdu chcete ukončit aplikaci bez uložení?",
                    "Neuložené změny");

                if (result == System.Windows.MessageBoxResult.Cancel)
                    return false;

                if (result == System.Windows.MessageBoxResult.No)
                {
                    var saved = _projectService.Save(BuildProjectData(), _currentFilePath);
                    if (saved == null)
                        return false;
                }
            }

            return true;
        }

        private bool IsProjectEmpty()
        {
            bool workEmpty = WorkCalcItems.All(x =>
                x.SelectedTask == null &&
                x.SelectedSpecification == null &&
                x.SelectedMaterial == null &&
                x.SelectedLocation == null &&
                x.Quantity == 0);

            bool materialEmpty = MaterialItems.All(x =>
                x.MaterialItem == null &&
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
        // EXPORT TEXT
        // =========================================================

        public string ExportAsText()
        {
            var sb = new System.Text.StringBuilder();

            sb.AppendLine("=================================");
            sb.AppendLine("ELEKTRO OFFER - KALKULACE");
            sb.AppendLine("=================================");
            sb.AppendLine();

            sb.AppendLine("PRÁCE:");

            foreach (var item in WorkCalcItems)
            {
                if (item.WorkItem == null) continue;

                double basePrice = item.WorkItem.BasePrice
                                 * item.WorkItem.MaterialCoef
                                 * item.WorkItem.PositionCoef
                                 * item.Quantity;

                if (item.IsDiscountEnabled && item.DiscountPercent.HasValue)
                {
                    double discount = basePrice - item.Total;

                    sb.AppendLine($"  {item.SelectedTask} | {item.Quantity}");
                    sb.AppendLine($"    Cena bez slevy:  {basePrice:N0} Kč");
                    sb.AppendLine($"    Sleva:           -{discount:N0} Kč ({item.DiscountPercent:N0} %)");
                    sb.AppendLine($"    Cena se slevou:  {item.Total:N0} Kč");
                }
                else
                {
                    sb.AppendLine($"  {item.SelectedTask} | {item.Quantity} | {item.Total:N0} Kč");
                }
            }

            sb.AppendLine();
            sb.AppendLine("MATERIÁL:");

            foreach (var item in MaterialItems)
            {
                if (item.MaterialItem == null) continue;

                double basePrice = item.MaterialItem.Price * item.Quantity;

                if (item.IsDiscountEnabled && item.DiscountPercent.HasValue)
                {
                    double discount = basePrice - item.Total;

                    sb.AppendLine($"  {item.MaterialItem.Name} | {item.Quantity}");
                    sb.AppendLine($"    Cena bez slevy:  {basePrice:N0} Kč");
                    sb.AppendLine($"    Sleva:           -{discount:N0} Kč ({item.DiscountPercent:N0} %)");
                    sb.AppendLine($"    Cena se slevou:  {item.Total:N0} Kč");
                }
                else
                {
                    sb.AppendLine($"  {item.MaterialItem.Name} | {item.Quantity} | {item.Total:N0} Kč");
                }
            }

            sb.AppendLine();
            sb.AppendLine("---------------------------------");
            sb.AppendLine($"PRÁCE CELKEM:    {WorkTotal:N0} Kč");
            sb.AppendLine($"MATERIÁL CELKEM: {MaterialTotal:N0} Kč");

            if (HasAnyDiscount)
            {
                sb.AppendLine("---------------------------------");
                sb.AppendLine($"CENA PŘED SLEVOU: {GrandTotalBeforeDiscount:N0} Kč");
                sb.AppendLine($"CELKOVÁ SLEVA:    -{TotalDiscount:N0} Kč");
            }

            sb.AppendLine("---------------------------------");
            sb.AppendLine($"CELKEM:          {GrandTotal:N0} Kč");

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
