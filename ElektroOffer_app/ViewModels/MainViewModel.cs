using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ElektroOffer_app.Commands;
using ElektroOffer_app.Data;
using ElektroOffer_app.Invoice.Models;
using ElektroOffer_app.Invoice.Services;
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

        // 🔴 ZMĚNA (1.9.0): CalculationCascadeService (nad starou PriceItems)
        // odstraněna. Kaskádu PRÁCE teď řeší WorkCascadeService, ale ta se
        // instancuje přímo uvnitř CalculationItemViewModel (per řádek) –
        // MainViewModel žádnou vlastní instanci kaskádové služby nepotřebuje,
        // jen CatalogService pro naplnění sdílených seznamů níže.
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

        // 🔴 ZMĚNA (1.9.0): Tasks (List<string>) nahrazeno WorkTasks (celé
        // entity WorkTask, kvůli Id i Name). Přibyly sdílené seznamy
        // BaseMaterialsList a WorkPositionsList – v nové kaskádě se nabízí
        // vždy celé, nezávisle na řádku, takže žijí tady, ne per-řádek.
        public ObservableCollection<WorkTask> WorkTasks { get; } = new();
        public ObservableCollection<BaseMaterial> BaseMaterialsList { get; } = new();
        public ObservableCollection<WorkPosition> WorkPositionsList { get; } = new();

        public ObservableCollection<Material> Materials { get; } = new();
        public ObservableCollection<CalculationItemViewModel> WorkCalcItems { get; } = new();
        public ObservableCollection<CalculationItemViewModel> MaterialItems { get; } = new();
        public ObservableCollection<BudgetItem> BudgetItems { get; } = new();

        // =========================================================
        // PROJECT STATE
        // =========================================================

        private string? _currentFilePath = null;
        private bool _hasUnsavedChanges = false;
        private InvoiceDraft? _invoiceDraft;

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
        public ICommand InvoiceCommand { get; }
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
            // DI – všechny služby + jeden sdílený AppDbContext
            _projectService = projectService;
            _catalogService = catalogService;
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
            InvoiceCommand = new RelayCommand(_ => ShowInvoice());
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

        // 🔴 ZMĚNA (1.9.0): Vedle materiálu se teď plní i 3 sdílené seznamy
        // PRÁCE (WorkTasks / BaseMaterialsList / WorkPositionsList) přímo
        // z CatalogService – ta jim odpovídá metodami GetWorkTasks(),
        // GetBaseMaterials(), GetWorkPositions() (viz krok 4).
        private void LoadCatalogDataFromDb()
        {
            var materials = _catalogService.LoadMaterials(_db);
            Materials.Clear();
            foreach (var m in materials) Materials.Add(m);

            var tasks = _catalogService.GetWorkTasks(_db);
            WorkTasks.Clear();
            foreach (var t in tasks) WorkTasks.Add(t);

            var baseMaterials = _catalogService.GetBaseMaterials(_db);
            BaseMaterialsList.Clear();
            foreach (var b in baseMaterials) BaseMaterialsList.Add(b);

            var positions = _catalogService.GetWorkPositions(_db);
            WorkPositionsList.Clear();
            foreach (var p in positions) WorkPositionsList.Add(p);
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

        // 🔴 ZMĚNA (1.9.0): staré SelectedTask/Specification/Material/Location
        // nahrazeny novou kaskádou SelectedWorkTask/WorkSpecification/
        // BaseMaterial/WorkPosition.
        public void ResetWorkItem(object? obj)
        {
            if (obj is CalculationItemViewModel item)
            {
                item.SelectedWorkTask = null;
                item.SelectedWorkSpecification = null;
                item.SelectedBaseMaterial = null;
                item.SelectedWorkPosition = null;
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
                // Kategorie + Název
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

        // 🔴 ZMĚNA (1.9.0): nasloucháme nově SelectedWorkTaskEntity/
        // SelectedBaseMaterialEntity/SelectedWorkPositionEntity místo
        // starého WorkItem – to jsou entity, jejichž změna teď ovlivňuje Total.
        private void Item_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // 🔒 Během načítání projektu se nesmí nic přepočítávat ani spouštět
            if (_isLoading)
                return;

            if (e.PropertyName == nameof(CalculationItemViewModel.Total) ||
                e.PropertyName == nameof(CalculationItemViewModel.Quantity) ||
                e.PropertyName == nameof(CalculationItemViewModel.SelectedWorkTaskEntity) ||
                e.PropertyName == nameof(CalculationItemViewModel.SelectedBaseMaterialEntity) ||
                e.PropertyName == nameof(CalculationItemViewModel.SelectedWorkPositionEntity) ||
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
            // 🔴 ZMĚNA (1.9.0): BaseTotal pro práci teď počítá ze 3 nezávislých
            // entit (SelectedWorkTaskEntity/SelectedBaseMaterialEntity/
            // SelectedWorkPositionEntity) místo staré x.WorkItem.
            static double BaseTotal(CalculationItemViewModel x)
            {
                if (x.SelectedWorkTaskEntity != null &&
                    x.SelectedBaseMaterialEntity != null &&
                    x.SelectedWorkPositionEntity != null)
                {
                    return (double)x.SelectedWorkTaskEntity.BasePrice
                         * (double)x.SelectedBaseMaterialEntity.BaseMaterialCoef
                         * (double)x.SelectedWorkPositionEntity.PositionCoef
                         * x.Quantity;
                }

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
            // 3) CENA PŘED SLEVOU
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

            // 🔴 ZMĚNA (1.9.0): popis řádku PRÁCE teď skládá nové názvy
            foreach (var x in WorkCalcItems.Where(x => x.Total > 0))
            {
                double basePrice = BaseTotal(x);
                double discountAmount = basePrice - x.Total;

                BudgetItems.Add(new BudgetItem
                {
                    Type = "PRÁCE",
                    Description = $"{x.SelectedWorkTask} / {x.SelectedWorkSpecification} / {x.SelectedBaseMaterial} / {x.SelectedWorkPosition}",
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
            _invoiceDraft = null;

            StatusText = "Nový projekt";
        }

        // =========================================================
        // CLEAR ITEMS
        // =========================================================

        private void ClearAllItems()
        {
            _isLoading = true;

            foreach (var item in WorkCalcItems)
                item.PropertyChanged -= Item_PropertyChanged;

            foreach (var item in MaterialItems)
                item.PropertyChanged -= Item_PropertyChanged;

            WorkCalcItems.Clear();
            MaterialItems.Clear();

            BudgetItems.Clear();

            _isLoading = false;
        }

        // =========================================================
        // BUILD PROJECT DATA
        // =========================================================
        //
        // 🔴 ZMĚNA (1.9.0): WorkItemData teď ukládá nové názvy polí
        // (SelectedWorkTask/WorkSpecification/BaseMaterial/WorkPosition
        // místo Task/Specification/Material/Location). Viz krok s
        // aktualizací WorkItemData.cs (další soubor).
        //
        private ProjectData BuildProjectData()
        {
            int workCounter = 1;
            int materialCounter = 1;

            var workItemsWithCommon = WorkCalcItems
                .Select((x, i) => new { Item = x, Position = i + 1 })
                .Where(x => !x.Item.IsEmpty)
                .Select(x =>
                {
                    var id = $"W-{workCounter++}";

                    return new
                    {
                        Work = new WorkItemData
                        {
                            Id = id,
                            Position = x.Position,
                            SelectedWorkTask = x.Item.SelectedWorkTask,
                            SelectedWorkSpecification = x.Item.SelectedWorkSpecification,
                            SelectedBaseMaterial = x.Item.SelectedBaseMaterial,
                            SelectedWorkPosition = x.Item.SelectedWorkPosition,
                            SelectedWorkPrice = x.Item.SelectedWorkPrice,
                            SelectedWorkUnit = x.Item.SelectedWorkUnit
                        },
                        Common = new CalculationItemData
                        {
                            Id = id,
                            Quantity = x.Item.Quantity,
                            DiscountPercent = x.Item.DiscountPercent,
                            IsDiscountEnabled = x.Item.IsDiscountEnabled,
                            Total = x.Item.Total
                        }
                    };
                })
                .ToList();

            var workItems = workItemsWithCommon.Select(x => x.Work).ToList();

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
                            Position = x.Position,
                            SelectedCategory = x.Item.SelectedCategory,
                            SelectedProductName = x.Item.SelectedProductName,
                            SelectedSupplier = x.Item.SelectedSupplier,
                            SelectedOffer = x.Item.SelectedOffer,
                            SelectedMaterialPrice = x.Item.SelectedMaterialPriceValue,
                            SelectedMaterialUnit = x.Item.SelectedMaterialUnit
                        },
                        Common = new CalculationItemData
                        {
                            Id = id,
                            Quantity = x.Item.Quantity,
                            DiscountPercent = x.Item.DiscountPercent,
                            IsDiscountEnabled = x.Item.IsDiscountEnabled,
                            Total = x.Item.Total
                        }
                    };
                })
                .ToList();

            var materialItems = materialItemsWithCommon.Select(x => x.Material).ToList();

            var commonItems =
                workItemsWithCommon.Select(x => x.Common)
                .Concat(materialItemsWithCommon.Select(x => x.Common))
                .ToList();

            return new ProjectData
            {
                ProjectName = _currentFilePath != null
                    ? System.IO.Path.GetFileNameWithoutExtension(_currentFilePath)
                    : "Nový projekt",

                SavedAt = DateTime.Now,

                WorkItems = workItems,
                MaterialItems = materialItems,
                CommonItems = commonItems,
                InvoiceDraft = _invoiceDraft != null
                    ? InvoiceDraftCloneService.Clone(_invoiceDraft)
                    : null,
                WorkRowCount = WorkCalcItems.Count,
                MaterialRowCount = MaterialItems.Count
            };
        }

        // ============================================================================
        // 📥 APPLY PROJECT DATA
        // ============================================================================
        //
        // 🔴 ZMĚNA (1.9.0): načítání PRÁCE teď nastavuje nové property
        // (SelectedWorkTask/WorkSpecification/BaseMaterial/WorkPosition).
        //
        private void ApplyProjectData(ProjectData data, string path)
        {
            _isLoading = true;

            ClearAllItems();

            var workItems = data.WorkItems;
            var materialItems = data.MaterialItems;
            var commonItems = data.CommonItems;
            _invoiceDraft = data.InvoiceDraft != null
                ? InvoiceDraftCloneService.Clone(data.InvoiceDraft)
                : null;

            const int minRowCount = 5;

            int maxWorkRow = workItems.Count > 0
                ? workItems.Max(w => w.Position)
                : 0;

            int maxMaterialRow = materialItems.Count > 0
                ? materialItems.Max(m => m.Position)
                : 0;

            int workRowCount = Math.Max(Math.Max(minRowCount, maxWorkRow), data.WorkRowCount);
            int materialRowCount = Math.Max(Math.Max(minRowCount, maxMaterialRow), data.MaterialRowCount);

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

            foreach (var savedWork in workItems)
            {
                int index = savedWork.Position - 1;
                var item = WorkCalcItems[index];

                item.Id = savedWork.Id;

                // 🔴 Nastavením SelectedWorkTask/WorkSpecification/BaseMaterial/
                // WorkPosition se automaticky spustí WorkCascadeService (LoadWorkSpecifications,
                // UpdateSelectedWorkTask/BaseMaterial/WorkPosition) a dohledají se
                // odpovídající EF entity potřebné pro výpočet Total.
                item.SelectedWorkTask = savedWork.SelectedWorkTask;
                item.SelectedWorkSpecification = savedWork.SelectedWorkSpecification;
                item.SelectedBaseMaterial = savedWork.SelectedBaseMaterial;
                item.SelectedWorkPosition = savedWork.SelectedWorkPosition;

                item.SelectedWorkPrice = savedWork.SelectedWorkPrice;
                item.SelectedWorkUnit = savedWork.SelectedWorkUnit;

                var savedCommon = commonItems.First(c => c.Id == savedWork.Id);

                item.Quantity = savedCommon.Quantity;
                item.DiscountPercent = savedCommon.DiscountPercent;
                item.IsDiscountEnabled = savedCommon.IsDiscountEnabled;
            }

            foreach (var savedMaterial in materialItems)
            {
                int index = savedMaterial.Position - 1;
                var item = MaterialItems[index];

                item.Id = savedMaterial.Id;
                item.SelectedCategory = savedMaterial.SelectedCategory;
                item.SelectedProductName = savedMaterial.SelectedProductName;
                item.SelectedSupplier = savedMaterial.SelectedSupplier;
                item.SelectedOffer = savedMaterial.SelectedOffer;

                item.SelectedMaterialPriceValue = savedMaterial.SelectedMaterialPrice;
                item.SelectedMaterialUnit = savedMaterial.SelectedMaterialUnit;

                var savedCommon = commonItems.First(c => c.Id == savedMaterial.Id);

                item.Quantity = savedCommon.Quantity;
                item.DiscountPercent = savedCommon.DiscountPercent;
                item.IsDiscountEnabled = savedCommon.IsDiscountEnabled;
            }

            _isLoading = false;
            Recalculate();
            _currentFilePath = path;
            _hasUnsavedChanges = false;
            StatusText = path;
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

        // 🔴 ZMĚNA (1.9.0): kontrola prázdného projektu používá nové property PRÁCE.
        private bool IsProjectEmpty()
        {
            bool workEmpty = WorkCalcItems.All(x =>
                x.SelectedWorkTask == null &&
                x.SelectedWorkSpecification == null &&
                x.SelectedBaseMaterial == null &&
                x.SelectedWorkPosition == null &&
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

        public void ShowInvoice()
        {
            Recalculate();
            var savedDraft = _windowService.ShowInvoice(BudgetItems.ToList(), _invoiceDraft);
            if (savedDraft != null)
            {
                _invoiceDraft = savedDraft;
                MarkAsChanged();
            }
        }

        // =========================================================
        // EXPORT TEXT
        // =========================================================

        // 🔴 ZMĚNA (1.9.0): blok PRÁCE počítá basePrice ze 3 entit místo
        // starého item.WorkItem, a vypisuje SelectedWorkTask místo SelectedTask.
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
                if (item.SelectedWorkTaskEntity == null ||
                    item.SelectedBaseMaterialEntity == null ||
                    item.SelectedWorkPositionEntity == null)
                    continue;

                double basePrice = (double)item.SelectedWorkTaskEntity.BasePrice
                                  * (double)item.SelectedBaseMaterialEntity.BaseMaterialCoef
                                  * (double)item.SelectedWorkPositionEntity.PositionCoef
                                  * item.Quantity;

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
