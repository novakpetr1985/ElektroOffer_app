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
                item.MaterialItem = null;
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
                if (x.MaterialItem != null)
                    return x.MaterialItem.Price * x.Quantity;
                return 0;
            }

            WorkTotal = WorkCalcItems.Sum(x => x.Total);
            MaterialTotal = MaterialItems.Sum(x => x.Total);
            GrandTotal = WorkTotal + MaterialTotal;

            WorkDiscountTotal = WorkCalcItems
                .Where(x => x.IsDiscountEnabled && x.DiscountPercent.HasValue)
                .Sum(x => BaseTotal(x) - x.Total);

            MaterialDiscountTotal = MaterialItems
                .Where(x => x.IsDiscountEnabled && x.DiscountPercent.HasValue)
                .Sum(x => BaseTotal(x) - x.Total);

            TotalDiscount = WorkDiscountTotal + MaterialDiscountTotal;
            GrandTotalBeforeDiscount = GrandTotal + TotalDiscount;

            HasAnyDiscount = TotalDiscount > 0.0001;

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

            foreach (var x in MaterialItems.Where(x => x.Total > 0 && x.MaterialItem != null))
            {
                double basePrice = BaseTotal(x);
                double discountAmount = basePrice - x.Total;

                BudgetItems.Add(new BudgetItem
                {
                    Type = "MATERIÁL",
                    Description = x.MaterialItem!.Name,
                    Unit = x.MaterialItem?.Unit ?? "",
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
                OnProjectSaved(savedPath);
        }

        public void SaveAs()
        {
            var data = BuildProjectData();
            var savedPath = _projectService.SaveAs(data);

            if (savedPath != null)
                OnProjectSaved(savedPath);
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
            foreach (var item in WorkCalcItems)
                item.PropertyChanged -= Item_PropertyChanged;

            foreach (var item in MaterialItems)
                item.PropertyChanged -= Item_PropertyChanged;

            WorkCalcItems.Clear();
            MaterialItems.Clear();
            BudgetItems.Clear();
        }

        // =========================================================
        // BUILD / APPLY PROJECT DATA
        // =========================================================

        private ProjectData BuildProjectData()
        {
            return new ProjectData
            {
                ProjectName = _currentFilePath != null
                    ? System.IO.Path.GetFileNameWithoutExtension(_currentFilePath)
                    : "Nový projekt",
                SavedAt = DateTime.Now,

                WorkItems = WorkCalcItems.Select(x => new WorkItemData
                {
                    SelectedTask = x.SelectedTask,
                    SelectedSpecification = x.SelectedSpecification,
                    SelectedMaterial = x.SelectedMaterial,
                    SelectedLocation = x.SelectedLocation,
                    Quantity = x.Quantity,
                    IsDiscountEnabled = x.IsDiscountEnabled,
                    DiscountPercent = x.DiscountPercent
                }).ToList(),

                MaterialItems = MaterialItems.Select(x => new MaterialItemData
                {
                    MaterialName = x.MaterialItem?.Name,
                    Quantity = x.Quantity,
                    IsDiscountEnabled = x.IsDiscountEnabled,
                    DiscountPercent = x.DiscountPercent
                }).ToList()
            };
        }

        private void ApplyProjectData(ProjectData data, string path)
        {
            ClearAllItems();

            foreach (var saved in data.WorkItems)
            {
                var item = new CalculationItemViewModel(_db);
                item.PropertyChanged += Item_PropertyChanged;

                item.SelectedTask = saved.SelectedTask;
                item.SelectedSpecification = saved.SelectedSpecification;
                item.SelectedMaterial = saved.SelectedMaterial;
                item.SelectedLocation = saved.SelectedLocation;
                item.Quantity = saved.Quantity;
                item.DiscountPercent = saved.DiscountPercent;
                item.IsDiscountEnabled = saved.IsDiscountEnabled;

                WorkCalcItems.Add(item);
            }

            foreach (var saved in data.MaterialItems)
            {
                var item = new CalculationItemViewModel(_db);
                item.PropertyChanged += Item_PropertyChanged;

                item.MaterialItem = Materials.FirstOrDefault(m => m.Name == saved.MaterialName);
                item.Quantity = saved.Quantity;
                item.DiscountPercent = saved.DiscountPercent;
                item.IsDiscountEnabled = saved.IsDiscountEnabled;

                MaterialItems.Add(item);
            }

            _currentFilePath = path;
            _hasUnsavedChanges = false;

            StatusText = path;
            Recalculate();
        }

        private void OnProjectSaved(string path)
        {
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
