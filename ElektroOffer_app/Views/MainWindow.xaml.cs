using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

using ElektroOffer_app.Commands;
using ElektroOffer_app.Data;
using ElektroOffer_app.Models;
using ElektroOffer_app.Services;
using ElektroOffer_app.ViewModels.Items;

namespace ElektroOffer_app
{
    // RELEASE MAIN 1.1.1 – původní stabilní verze před refaktorem do MVVM
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        // =========================================================
        // 📦 DATA Z DB
        // =========================================================
        public ObservableCollection<Material> Materials { get; set; } = new();

        // =========================================================
        // 🔧 GLOBÁLNÍ SEZNAM ÚKONŮ
        // =========================================================
        public ObservableCollection<string> Tasks { get; set; } = new();

        // =========================================================
        // 🧮 KALKULACE
        // =========================================================
        public ObservableCollection<CalculationItemViewModel> WorkCalcItems { get; set; } = new();
        public ObservableCollection<CalculationItemViewModel> MaterialItems { get; set; } = new();

        // =========================================================
        // 📊 DETAILNÍ ROZPIS
        // =========================================================
        public ObservableCollection<BudgetItem> BudgetItems { get; set; } = new();

        // =========================================================
        // 💰 CELKOVÁ CENA
        // =========================================================
        private double _grandTotal;
        public double GrandTotal
        {
            get => _grandTotal;
            set
            {
                if (Math.Abs(_grandTotal - value) < 0.0001) return;
                _grandTotal = value;
                OnPropertyChanged();
            }
        }

        // =========================================================
        // 💰 DÍLČÍ SOUČTY
        // =========================================================
        private double _workTotal;
        public double WorkTotal
        {
            get => _workTotal;
            set
            {
                if (Math.Abs(_workTotal - value) < 0.0001) return;
                _workTotal = value;
                OnPropertyChanged();
            }
        }

        private double _materialTotal;
        public double MaterialTotal
        {
            get => _materialTotal;
            set
            {
                if (Math.Abs(_materialTotal - value) < 0.0001) return;
                _materialTotal = value;
                OnPropertyChanged();
            }
        }

        // =========================================================
        // 🏷️ SLEVY — celková výše slev (práce + materiál zvlášť)
        // =========================================================

        private double _workDiscountTotal;
        /// <summary>
        /// Celková sleva na práci v Kč (součet všech řádkových slev).
        /// = cena bez slevy MINUS cena se slevou.
        /// </summary>
        public double WorkDiscountTotal
        {
            get => _workDiscountTotal;
            set
            {
                if (Math.Abs(_workDiscountTotal - value) < 0.0001) return;
                _workDiscountTotal = value;
                OnPropertyChanged();
            }
        }

        private double _materialDiscountTotal;
        /// <summary>
        /// Celková sleva na materiál v Kč (součet všech řádkových slev).
        /// </summary>
        public double MaterialDiscountTotal
        {
            get => _materialDiscountTotal;
            set
            {
                if (Math.Abs(_materialDiscountTotal - value) < 0.0001) return;
                _materialDiscountTotal = value;
                OnPropertyChanged();
            }
        }

        private double _totalDiscount;
        /// <summary>
        /// Celková sleva v Kč — práce + materiál dohromady.
        /// Zobrazuje se v celkovém součtu jako "Sleva celkem".
        /// </summary>
        public double TotalDiscount
        {
            get => _totalDiscount;
            set
            {
                if (Math.Abs(_totalDiscount - value) < 0.0001) return;
                _totalDiscount = value;
                OnPropertyChanged();
            }
        }

        private bool _hasAnyDiscount;
        /// <summary>
        /// True pokud existuje alespoň jeden řádek se slevou.
        /// Používá se v XAML pro Visibility — sekce slevy se zobrazí jen když je relevantní.
        /// </summary>
        public bool HasAnyDiscount
        {
            get => _hasAnyDiscount;
            set
            {
                if (_hasAnyDiscount == value) return;
                _hasAnyDiscount = value;
                OnPropertyChanged();
            }
        }

        private double _grandTotalBeforeDiscount;
        /// <summary>
        /// Celková cena nabídky BEZ slevy = GrandTotal + TotalDiscount.
        /// Zobrazuje se v sekci celkového součtu nad řádkem slevy.
        /// Skrytá (přes HasAnyDiscount) pokud žádná sleva neexistuje.
        /// </summary>
        public double GrandTotalBeforeDiscount
        {
            get => _grandTotalBeforeDiscount;
            set
            {
                if (Math.Abs(_grandTotalBeforeDiscount - value) < 0.0001) return;
                _grandTotalBeforeDiscount = value;
                OnPropertyChanged();
            }
        }

        // =========================================================
        // 💾 STAV ULOŽENÍ — správa souboru a neuložených změn
        // =========================================================

        /// <summary>
        /// Servisní třída pro Save/Load logiku.
        /// Oddělena od UI — MainWindow jen volá její metody.
        /// </summary>
        private readonly ProjectService _projectService = new();

        /// <summary>
        /// Servisní třída pro načítání ceníku z databáze.
        /// Oddělena od UI — umožňuje testování bez WPF.
        /// </summary>
        private readonly CatalogService _catalogService = new();

        /// <summary>
        /// Aktuální cesta k otevřenému souboru.
        /// Null = projekt ještě nebyl uložen (nový projekt).
        /// </summary>
        private string? _currentFilePath = null;

        /// <summary>
        /// Příznak neuložených změn.
        /// True = data byla změněna od posledního uložení.
        /// Používá se při zavírání okna nebo "Nový projekt".
        /// </summary>
        private bool _hasUnsavedChanges = false;

        // =========================================================
        // 📌 STAVOVÝ ŘÁDEK — text zobrazený dole v okně
        // =========================================================

        private string _statusText = "Nový projekt";
        /// <summary>
        /// Text zobrazený ve StatusBaru dole.
        /// Ukazuje název souboru nebo stav projektu.
        /// </summary>
        public string StatusText
        {
            get => _statusText;
            set
            {
                _statusText = value;
                OnPropertyChanged();
            }
        }

        // =========================================================
        // START
        // =========================================================
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            using (var db = new AppDbContext())
            {
                db.Database.EnsureCreated();
            }

            // 🔄 SPRÁVNÉ NAČTENÍ DAT PRO WPF
            LoadCatalogDataFromDb();

            for (int i = 0; i < 5; i++)
            {
                AddWorkItemInternal();
                AddMaterialItemInternal();
            }

            WorkCalcItems.CollectionChanged += WorkCalcItems_CollectionChanged;
            MaterialItems.CollectionChanged += MaterialItems_CollectionChanged;

            // Klávesové zkratky pro Save/Load
            // Ctrl+S, Ctrl+Shift+S, Ctrl+O, Ctrl+N
            RegisterKeyboardShortcuts();
        }

        // =========================================================
        // 🔄 NAČTENÍ DAT PRO UI
        // =========================================================

        /// <summary>
        /// Načte ceník z databáze přes CatalogService a naplní kolekce pro UI.
        /// Volá se při startu aplikace a po importu ceníku.
        /// Samotná DB logika je v CatalogService — zde jen přijímáme výsledek.
        /// </summary>
        private void LoadCatalogDataFromDb()
        {
            // CatalogService vrátí dvojici (tasks, materials) načtenou z DB
            // new AppDbContext() → použije výchozí připojení k elektrooffer.db
            var (tasks, materials) = _catalogService.LoadCatalog(new AppDbContext());

            // Naplnění kolekce úkonů — Clear() zachovává referenci (WPF binding zůstane funkční)
            Tasks.Clear();
            foreach (var t in tasks) Tasks.Add(t);

            // Naplnění kolekce materiálů — stejný princip
            Materials.Clear();
            foreach (var m in materials) Materials.Add(m);
        }

        // =========================================================
        // ⌨️ KLÁVESOVÉ ZKRATKY
        // =========================================================

        /// <summary>
        /// Registrace klávesových zkratek přes CommandBindings.
        /// Tímto způsobem zkratky fungují i bez zaměření na konkrétní prvek UI.
        /// </summary>
        private void RegisterKeyboardShortcuts()
        {
            // Ctrl+S → Uložit
            var saveBinding = new System.Windows.Input.KeyBinding(
                new RelayCommand(_ => MenuSave_Click(this, new RoutedEventArgs())),
                System.Windows.Input.Key.S,
                System.Windows.Input.ModifierKeys.Control);

            // Ctrl+Shift+S → Uložit jako
            var saveAsBinding = new System.Windows.Input.KeyBinding(
                new RelayCommand(_ => MenuSaveAs_Click(this, new RoutedEventArgs())),
                System.Windows.Input.Key.S,
                System.Windows.Input.ModifierKeys.Control | System.Windows.Input.ModifierKeys.Shift);

            // Ctrl+O → Otevřít
            var openBinding = new System.Windows.Input.KeyBinding(
                new RelayCommand(_ => MenuLoad_Click(this, new RoutedEventArgs())),
                System.Windows.Input.Key.O,
                System.Windows.Input.ModifierKeys.Control);

            // Ctrl+N → Nový projekt
            var newBinding = new System.Windows.Input.KeyBinding(
                new RelayCommand(_ => MenuNewProject_Click(this, new RoutedEventArgs())),
                System.Windows.Input.Key.N,
                System.Windows.Input.ModifierKeys.Control);

            InputBindings.Add(saveBinding);
            InputBindings.Add(saveAsBinding);
            InputBindings.Add(openBinding);
            InputBindings.Add(newBinding);
        }

        // =========================================================
        // 📋 MENU — AKCE
        // =========================================================

        private void MenuNewProject_Click(object sender, RoutedEventArgs e)
        {
            var currentData = BuildProjectData();

            if (!_projectService.ConfirmNewProject(currentData, _currentFilePath, _hasUnsavedChanges))
                return;

            ResetToNewProject();
        }

        private void MenuLoad_Click(object sender, RoutedEventArgs e)
        {
            if (_hasUnsavedChanges)
            {
                var result = MessageBox.Show(
                    "Máte neuložené změny. Chcete je uložit před načtením jiného projektu?",
                    "Neuložené změny",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Cancel) return;
                if (result == MessageBoxResult.Yes)
                {
                    var saved = _projectService.Save(BuildProjectData(), _currentFilePath);
                    if (saved == null) return;
                }
            }

            var (data, path) = _projectService.Load();
            if (data == null) return;

            ApplyProjectData(data, path!);
        }

        private void MenuSave_Click(object sender, RoutedEventArgs e)
        {
            var data = BuildProjectData();
            var savedPath = _projectService.Save(data, _currentFilePath);

            if (savedPath != null)
                OnProjectSaved(savedPath);
        }

        private void MenuSaveAs_Click(object sender, RoutedEventArgs e)
        {
            var data = BuildProjectData();
            var savedPath = _projectService.SaveAs(data);

            if (savedPath != null)
                OnProjectSaved(savedPath);
        }

        // =========================================================
        // ❓ NÁPOVĚDA
        // =========================================================

        private void MenuAbout_Click(object sender, RoutedEventArgs e)
        {
            var aboutWindow = new AboutWindow
            {
                Owner = this
            };

            aboutWindow.ShowDialog();
        }

        // =========================================================
        // 🛠️ POMOCNÉ METODY — Save/Load logika
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
                    Quantity = x.Quantity
                }).ToList(),

                MaterialItems = MaterialItems.Select(x => new MaterialItemData
                {
                    MaterialName = x.MaterialItem?.Name,
                    Quantity = x.Quantity
                }).ToList()
            };
        }

        private void ApplyProjectData(ProjectData data, string path)
        {
            ClearAllItems();

            foreach (var saved in data.WorkItems)
            {
                var item = new CalculationItemViewModel();
                item.PropertyChanged += Item_PropertyChanged;

                item.SelectedTask = saved.SelectedTask;
                item.SelectedSpecification = saved.SelectedSpecification;
                item.SelectedMaterial = saved.SelectedMaterial;
                item.SelectedLocation = saved.SelectedLocation;
                item.Quantity = saved.Quantity;

                WorkCalcItems.Add(item);
            }

            foreach (var saved in data.MaterialItems)
            {
                var item = new CalculationItemViewModel();
                item.PropertyChanged += Item_PropertyChanged;

                item.MaterialItem = Materials.FirstOrDefault(m => m.Name == saved.MaterialName);
                item.Quantity = saved.Quantity;

                MaterialItems.Add(item);
            }

            _currentFilePath = path;
            _hasUnsavedChanges = false;

            UpdateWindowTitle(path);
            Recalculate();
        }

        private void OnProjectSaved(string path)
        {
            _currentFilePath = path;
            _hasUnsavedChanges = false;
            UpdateWindowTitle(path);
        }

        private void ResetToNewProject()
        {
            ClearAllItems();

            for (int i = 0; i < 5; i++)
            {
                AddWorkItemInternal();
                AddMaterialItemInternal();
            }

            _currentFilePath = null;
            _hasUnsavedChanges = false;

            Title = "Elektro Offer - Kalkulace";
            StatusText = "Nový projekt";
        }

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

        private void UpdateWindowTitle(string path)
        {
            var fileName = System.IO.Path.GetFileNameWithoutExtension(path);
            Title = $"{fileName} - Elektro Offer";
            StatusText = path;
        }

        private void MarkAsChanged()
        {
            if (!_hasUnsavedChanges)
            {
                _hasUnsavedChanges = true;

                if (!Title.StartsWith("*"))
                    Title = "* " + Title;
            }
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
        // 🖨️ PRINT / EXPORT (tisk kalkulace do PDF / tiskárny)
        // =========================================================
        //
        // ÚČEL:
        // - Vytvoří textový výstup kalkulace (PRÁCE + MATERIÁL)
        // - Převede ho do FlowDocument
        // - Odešle na tiskárnu přes PrintDialog
        //
        // POZNÁMKA:
        // - V této verzi jde o jednoduchý tisk (textový layout)
        // - Později lze nahradit PDF generátorem (např. QuestPDF)
        // - ExportAsText() sestavuje data z aktuální kalkulace
        // =========================================================

        private void MenuPrint_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Controls.PrintDialog();

            // pokud uživatel zruší dialog, nic netiskneme
            if (dialog.ShowDialog() != true)
                return;

            // převod kalkulace do textové podoby
            var text = ExportAsText();

            // vytvoření dokumentu pro tisk
            var flowDoc = new FlowDocument(new Paragraph(new Run(text)))
            {
                FontFamily = new FontFamily("Consolas"), // monospaced font pro zarovnání
                FontSize = 12,
                PagePadding = new Thickness(50)
            };

            // odeslání na tiskárnu
            dialog.PrintDocument(
                ((IDocumentPaginatorSource)flowDoc).DocumentPaginator,
                "ElektroOffer – Kalkulace"
            );
        }

        /// <summary>
        /// Vytvoří textovou reprezentaci celé kalkulace.
        /// Používá se pro tisk (FlowDocument).
        /// Obsahuje sekci slev pokud jsou zadány — stejná logika jako UI.
        /// </summary>
        private string ExportAsText()
        {
            var sb = new System.Text.StringBuilder();

            sb.AppendLine("=================================");
            sb.AppendLine("ELEKTRO OFFER - KALKULACE");
            sb.AppendLine("=================================");
            sb.AppendLine();

            // ── PRÁCE ─────────────────────────────────────────────────────
            sb.AppendLine("PRÁCE:");

            foreach (var item in WorkCalcItems)
            {
                if (item.WorkItem == null) continue;

                // Pokud má řádek slevu → zobrazíme původní cenu, slevu i výsledek
                if (item.IsDiscountEnabled && item.DiscountPercent.HasValue)
                {
                    double basePrice = item.WorkItem.BasePrice
                                     * item.WorkItem.MaterialCoef
                                     * item.WorkItem.PositionCoef
                                     * item.Quantity;
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

            // ── MATERIÁL ──────────────────────────────────────────────────
            sb.AppendLine("MATERIÁL:");

            foreach (var item in MaterialItems)
            {
                if (item.MaterialItem == null) continue;

                // Pokud má řádek slevu → zobrazíme původní cenu, slevu i výsledek
                if (item.IsDiscountEnabled && item.DiscountPercent.HasValue)
                {
                    double basePrice = item.MaterialItem.Price * item.Quantity;
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

            // ── SOUČTY ────────────────────────────────────────────────────
            sb.AppendLine($"PRÁCE CELKEM:    {WorkTotal:N0} Kč");
            sb.AppendLine($"MATERIÁL CELKEM: {MaterialTotal:N0} Kč");

            // Sekce slev — zobrazí se pouze pokud existuje alespoň jedna sleva
            // Stejná podmínka jako HasAnyDiscount v UI
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
        // ➕ ADD
        // =========================================================
        private void AddWorkItem_Click(object sender, RoutedEventArgs e)
            => AddWorkItemInternal();

        private void AddMaterialsItem_Click(object sender, RoutedEventArgs e)
            => AddMaterialItemInternal();

        private void AddWorkItemInternal()
        {
            var item = new CalculationItemViewModel();
            item.PropertyChanged += Item_PropertyChanged;
            WorkCalcItems.Add(item);
        }

        private void AddMaterialItemInternal()
        {
            var item = new CalculationItemViewModel();
            item.PropertyChanged += Item_PropertyChanged;
            MaterialItems.Add(item);
        }

        // =========================================================
        // ➖ DELETE
        // =========================================================
        private void DeleteWorkItem_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Opravdu chcete položku odebrat?", "Potvrzení",
                MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
                return;

            if (sender is FrameworkElement fe &&
                fe.DataContext is CalculationItemViewModel item)
            {
                item.PropertyChanged -= Item_PropertyChanged;
                WorkCalcItems.Remove(item);
                Recalculate();
            }
        }

        private void DeleteMaterialItem_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Opravdu chcete položku odebrat?", "Potvrzení",
                MessageBoxButton.OKCancel, MessageBoxImage.Question) != MessageBoxResult.OK)
                return;

            if (sender is FrameworkElement fe &&
                fe.DataContext is CalculationItemViewModel item)
            {
                item.PropertyChanged -= Item_PropertyChanged;
                MaterialItems.Remove(item);
                Recalculate();
            }
        }

        // =========================================================
        // 🧹 RESET WORK ITEM
        // =========================================================
        private void ResetWorkItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe &&
                fe.DataContext is CalculationItemViewModel item)
            {
                bool isFilled =
                    item.SelectedTask != null ||
                    item.SelectedSpecification != null ||
                    item.SelectedMaterial != null ||
                    item.SelectedLocation != null ||
                    item.Quantity > 0;

                if (isFilled)
                {
                    if (MessageBox.Show(
                        "Opravdu chcete vymazat vyplněný řádek práce?",
                        "Potvrzení",
                        MessageBoxButton.OKCancel,
                        MessageBoxImage.Question) != MessageBoxResult.OK)
                        return;
                }

                item.SelectedTask = null;
                item.SelectedSpecification = null;
                item.SelectedMaterial = null;
                item.SelectedLocation = null;
                item.Quantity = 0;

                Recalculate();
            }
        }

        // =========================================================
        // 🧹 RESET MATERIAL ITEM
        // =========================================================
        private void ResetMaterialItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe &&
                fe.DataContext is CalculationItemViewModel item)
            {
                bool isFilled =
                    item.MaterialItem != null ||
                    item.Quantity > 0;

                if (isFilled)
                {
                    if (MessageBox.Show(
                        "Opravdu chcete vymazat vyplněný řádek materiálu?",
                        "Potvrzení",
                        MessageBoxButton.OKCancel,
                        MessageBoxImage.Question) != MessageBoxResult.OK)
                        return;
                }

                item.MaterialItem = null;
                item.Quantity = 0;

                Recalculate();
            }
        }

        // =========================================================
        // 📌 EVENTS
        // =========================================================
        private void WorkCalcItems_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            MarkAsChanged();
            Recalculate();
        }

        private void MaterialItems_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            MarkAsChanged();
            Recalculate();
        }

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
        // 💰 REKALKULACE CEN A ROZPISU
        // =========================================================
        private void Recalculate()
        {
            // ── Pomocná lokální funkce ─────────────────────────────────────
            // Spočítá cenu řádku BEZ slevy.
            // Potřebujeme ji proto, že x.Total už slevu obsahuje —
            // abychom zjistili kolik Kč sleva odebrala, musíme
            // základ dopočítat znovu ze vstupních hodnot.
            static double BaseTotal(CalculationItemViewModel x)
            {
                if (x.WorkItem != null)
                    return x.WorkItem.BasePrice * x.WorkItem.MaterialCoef * x.WorkItem.PositionCoef * x.Quantity;
                if (x.MaterialItem != null)
                    return x.MaterialItem.Price * x.Quantity;
                return 0;
            }

            // ── Dílčí součty ──────────────────────────────────────────────
            // x.Total = cena SE slevou (díky úpravě v CalculationItemViewModel)
            WorkTotal = WorkCalcItems.Sum(x => x.Total);
            MaterialTotal = MaterialItems.Sum(x => x.Total);
            GrandTotal = WorkTotal + MaterialTotal;

            // ── Výpočet slev v Kč ─────────────────────────────────────────
            // Logika: základ (bez slevy) − výsledek (se slevou) = ušetřeno
            // Řádky bez aktivní slevy přispívají nulou → součet je bezpečný
            WorkDiscountTotal = WorkCalcItems
                .Where(x => x.IsDiscountEnabled && x.DiscountPercent.HasValue)
                .Sum(x => BaseTotal(x) - x.Total);

            MaterialDiscountTotal = MaterialItems
                .Where(x => x.IsDiscountEnabled && x.DiscountPercent.HasValue)
                .Sum(x => BaseTotal(x) - x.Total);

            TotalDiscount = WorkDiscountTotal + MaterialDiscountTotal;
            GrandTotalBeforeDiscount = GrandTotal + TotalDiscount; // ← NOVĚ: cena před slevou
                                                                   // GrandTotal už slevu obsahuje,
                                                                   // proto zpětně přičteme co bylo odečteno

            // ── Příznak pro XAML Visibility ───────────────────────────────
            // Sekce slevy v celkovém součtu se zobrazí jen když skutečně
            // existuje nějaká nenulová sleva — jinak zůstane skrytá.
            HasAnyDiscount = TotalDiscount > 0.0001;

            // ── Detailní rozpis (BudgetItems) ─────────────────────────────
            BudgetItems.Clear();

            foreach (var x in WorkCalcItems.Where(x => x.Total > 0))
            {
                // Základ bez slevy potřebujeme pro výpočet DiscountAmount
                double basePrice = BaseTotal(x);
                // Kolik Kč sleva odebrala = základ − cena se slevou
                double discountAmount = basePrice - x.Total;

                BudgetItems.Add(new BudgetItem
                {
                    Type = "PRÁCE",
                    Description = $"{x.SelectedTask} / {x.SelectedSpecification} / {x.SelectedMaterial} / {x.SelectedLocation}",
                    Unit = x.WorkUnit ?? "",
                    Quantity = x.Quantity,
                    Price = x.Total,

                    // null → XAML zobrazí prázdnou buňku (TargetNullValue="")
                    // hodnota → zobrazí se procento / částka slevy
                    DiscountPercent = (x.IsDiscountEnabled && x.DiscountPercent.HasValue)
                        ? x.DiscountPercent
                        : null,
                    DiscountAmount = discountAmount > 0.0001
                        ? discountAmount
                        : null
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

                    // null → prázdná buňka, hodnota → zobrazí slevu
                    DiscountPercent = (x.IsDiscountEnabled && x.DiscountPercent.HasValue)
                        ? x.DiscountPercent
                        : null,
                    DiscountAmount = discountAmount > 0.0001
                        ? discountAmount
                        : null
                });
            }
        }

        // =========================================================
        // PROPERTY CHANGED
        // =========================================================
        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        // =========================================================
        // 🚪 MENU: SOUBOR → KONEC
        // =========================================================
        private void MenuExit_Click(object sender, RoutedEventArgs e)
        {
            bool isNewProject = _currentFilePath == null;
            bool isEmpty = IsProjectEmpty();

            if (isNewProject && isEmpty)
            {
                var result = MessageBox.Show(
                    "Opravdu ukončit bez uložení?",
                    "Ukončit aplikaci",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes)
                    return;

                Application.Current.Shutdown();
                return;
            }

            if (_hasUnsavedChanges)
            {
                var result = MessageBox.Show(
                    "Opravdu chcete ukončit aplikaci bez uložení?",
                    "Neuložené změny",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Cancel)
                    return;

                if (result == MessageBoxResult.No)
                {
                    var saved = _projectService.Save(BuildProjectData(), _currentFilePath);
                    if (saved == null)
                        return;

                    Application.Current.Shutdown();
                    return;
                }

                Application.Current.Shutdown();
                return;
            }

            Application.Current.Shutdown();
        }

        // =========================================================
        // ❌ KŘÍŽEK (X) – UKONČENÍ OKNA
        // =========================================================
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            bool isNewProject = _currentFilePath == null;
            bool isEmpty = IsProjectEmpty();

            if (isNewProject && isEmpty)
            {
                var result = MessageBox.Show(
                    "Opravdu ukončit bez uložení?",
                    "Ukončit aplikaci",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes)
                    e.Cancel = true;

                return;
            }

            if (_hasUnsavedChanges)
            {
                var result = MessageBox.Show(
                    "Opravdu chcete ukončit aplikaci bez uložení?",
                    "Neuložené změny",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }

                if (result == MessageBoxResult.No)
                {
                    var saved = _projectService.Save(BuildProjectData(), _currentFilePath);
                    if (saved == null)
                    {
                        e.Cancel = true;
                        return;
                    }
                }

                return;
            }
        }
    }
}