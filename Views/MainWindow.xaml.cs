using ElektroOffer_app.Data;
using ElektroOffer_app.Models;
using ElektroOffer_app.Services;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using ElektroOffer_app.ViewModels.Items;


namespace ElektroOffer_app
{
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
        // 💾 STAV ULOŽENÍ — správa souboru a neuložených změn
        // =========================================================

        /// <summary>
        /// Servisní třída pro Save/Load logiku.
        /// Oddělena od UI — MainWindow jen volá její metody.
        /// </summary>
        private readonly ProjectService _projectService = new();

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
        // 🔄 NAČTENÍ DAT PRO UI (KLÍČOVÁ OPRAVA)
        // =========================================================
        private void LoadCatalogDataFromDb()
        {
            using var db = new AppDbContext();

            Tasks.Clear();
            foreach (var task in db.PriceItems.Select(x => x.Task).Distinct().ToList())
                Tasks.Add(task);

            Materials.Clear();
            foreach (var mat in db.Materials.ToList())
                Materials.Add(mat);
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

        /// <summary>
        /// Menu: Soubor → Nový projekt
        /// Zeptá se na uložení změn, pak resetuje celou kalkulaci.
        /// </summary>
        private void MenuNewProject_Click(object sender, RoutedEventArgs e)
        {
            // Sestaví aktuální data projektu (potřeba pro případné uložení)
            var currentData = BuildProjectData();

            // Zkontroluje neuložené změny — pokud uživatel zruší, nic neděláme
            if (!_projectService.ConfirmNewProject(currentData, _currentFilePath, _hasUnsavedChanges))
                return;

            // Reset aplikace do počátečního stavu
            ResetToNewProject();
        }

        /// <summary>
        /// Menu: Soubor → Otevřít
        /// Načte projekt z .eof souboru a naplní UI daty.
        /// </summary>
        private void MenuLoad_Click(object sender, RoutedEventArgs e)
        {
            // Pokud jsou neuložené změny → zeptáme se před načtením
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
                    if (saved == null) return; // uložení selhalo nebo bylo zrušeno
                }
            }

            // Otevření dialogu a načtení dat
            var (data, path) = _projectService.Load();
            if (data == null) return; // uživatel zrušil nebo chyba

            // Naplnění UI načtenými daty
            ApplyProjectData(data, path!);
        }

        /// <summary>
        /// Menu: Soubor → Uložit (Ctrl+S)
        /// Uloží na aktuální cestu, nebo vyvolá Save As dialog.
        /// </summary>
        private void MenuSave_Click(object sender, RoutedEventArgs e)
        {
            var data = BuildProjectData();
            var savedPath = _projectService.Save(data, _currentFilePath);

            if (savedPath != null)
                OnProjectSaved(savedPath);
        }

        /// <summary>
        /// Menu: Soubor → Uložit jako (Ctrl+Shift+S)
        /// Vždy otevře dialog pro výběr nového umístění.
        /// </summary>
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

        /// <summary>
        /// Menu: Nápověda → O aplikaci
        /// Otevře modální okno s logem, verzí a autorem aplikace.
        /// Owner = this zajistí centrování okna vůči MainWindow.
        /// </summary>
        private void MenuAbout_Click(object sender, RoutedEventArgs e)
        {
            var aboutWindow = new AboutWindow
            {
                // Owner zajistí správné centrování a chování modálního okna
                Owner = this
            };

            // ShowDialog = modální okno (nelze klikat na MainWindow dokud je otevřené)
            aboutWindow.ShowDialog();
        }

        // =========================================================
        // 🚪 KONEC
        // =========================================================

        /// <summary>
        /// Menu: Soubor → Konec
        /// Zeptá se na uložení neuložených změn, pak ukončí aplikaci.
        /// </summary>
        private void MenuExit_Click(object sender, RoutedEventArgs e)
        {
            // Pokud jsou neuložené změny → zeptáme se před ukončením
            if (_hasUnsavedChanges)
            {
                var result = MessageBox.Show(
                    "Máte neuložené změny. Chcete je uložit před ukončením?",
                    "Neuložené změny",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Cancel) return;

                if (result == MessageBoxResult.Yes)
                {
                    var saved = _projectService.Save(BuildProjectData(), _currentFilePath);
                    if (saved == null) return; // uložení selhalo nebo bylo zrušeno
                }
            }

            // Ukončení aplikace
            Application.Current.Shutdown();
        }

        // ADD-KOMENT: PDF export metody — odkomentovat až bude QuestPDF nainstalován a struktura dokumentu finální
        // ADD-KOMENT: Instalace: NuGet → Install-Package QuestPDF
        // ADD-KOMENT: Docs: https://www.questpdf.com/documentation/getting-started.html
        /*
        private void MenuExportPdf_Click(object sender, RoutedEventArgs e)
        {
            // 🚧 PLACEHOLDER — implementace přijde v další fázi
            MessageBox.Show(
                "Export PDF kalkulace bude implementován v další fázi projektu.\n\nPlánovaná knihovna: QuestPDF",
                "Připravuje se",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void MenuExportPricePdf_Click(object sender, RoutedEventArgs e)
        {
            // 🚧 PLACEHOLDER — implementace přijde v další fázi
            MessageBox.Show(
                "Export PDF ceníku bude implementován v další fázi projektu.\n\nPlánovaná knihovna: QuestPDF",
                "Připravuje se",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        */

        // =========================================================
        // 🛠️ POMOCNÉ METODY — Save/Load logika
        // =========================================================

        /// <summary>
        /// Sestaví objekt ProjectData z aktuálního stavu UI.
        /// Volá se před každým uložením.
        /// </summary>
        private ProjectData BuildProjectData()
        {
            return new ProjectData
            {
                // Metadata
                ProjectName = _currentFilePath != null
                    ? System.IO.Path.GetFileNameWithoutExtension(_currentFilePath)
                    : "Nový projekt",
                SavedAt = DateTime.Now,

                // Sekce PRÁCE — uložíme vybrané hodnoty každého řádku
                WorkItems = WorkCalcItems.Select(x => new WorkItemData
                {
                    SelectedTask = x.SelectedTask,
                    SelectedSpecification = x.SelectedSpecification,
                    SelectedMaterial = x.SelectedMaterial,
                    SelectedLocation = x.SelectedLocation,
                    Quantity = x.Quantity
                }).ToList(),

                // Sekce MATERIÁL — uložíme název materiálu a množství
                MaterialItems = MaterialItems.Select(x => new MaterialItemData
                {
                    MaterialName = x.MaterialItem?.Name,
                    Quantity = x.Quantity
                }).ToList()
            };
        }

        /// <summary>
        /// Aplikuje načtená data (ProjectData) do UI.
        /// Zavolá se po úspěšném Load.
        /// </summary>
        private void ApplyProjectData(ProjectData data, string path)
        {
            // Nejdříve vyprázdníme stávající UI
            ClearAllItems();

            // ========================= //
            // 🔧 OBNOVENÍ SEKCE: PRÁCE  //
            // ========================= //
            foreach (var saved in data.WorkItems)
            {
                var item = new CalculationItemViewModel();
                item.PropertyChanged += Item_PropertyChanged;

                // Obnovení vybraných hodnot
                item.SelectedTask = saved.SelectedTask;
                item.SelectedSpecification = saved.SelectedSpecification;
                item.SelectedMaterial = saved.SelectedMaterial;
                item.SelectedLocation = saved.SelectedLocation;
                item.Quantity = saved.Quantity;

                WorkCalcItems.Add(item);
            }

            // =========================== //
            // 📦 OBNOVENÍ SEKCE: MATERIÁL //
            // =========================== //
            foreach (var saved in data.MaterialItems)
            {
                var item = new CalculationItemViewModel();
                item.PropertyChanged += Item_PropertyChanged;

                // Dohledání objektu Material z DB podle názvu
                // (ukládáme název, ne ID → nezávislé na DB)
                item.MaterialItem = Materials.FirstOrDefault(m => m.Name == saved.MaterialName);
                item.Quantity = saved.Quantity;

                MaterialItems.Add(item);
            }

            // Uložíme cestu a resetujeme příznak změn
            _currentFilePath = path;
            _hasUnsavedChanges = false;

            // Aktualizace titulku a stavového řádku
            UpdateWindowTitle(path);
            Recalculate();
        }

        /// <summary>
        /// Zavolá se po úspěšném uložení.
        /// Aktualizuje cestu, příznak změn, titulek a status.
        /// </summary>
        private void OnProjectSaved(string path)
        {
            _currentFilePath = path;
            _hasUnsavedChanges = false;
            UpdateWindowTitle(path);
        }

        /// <summary>
        /// Resetuje aplikaci do stavu "nový projekt".
        /// Vymaže všechny řádky a přidá výchozí prázdné řádky.
        /// </summary>
        private void ResetToNewProject()
        {
            ClearAllItems();

            // Přidání výchozích prázdných řádků jako při startu
            for (int i = 0; i < 5; i++)
            {
                AddWorkItemInternal();
                AddMaterialItemInternal();
            }

            _currentFilePath = null;
            _hasUnsavedChanges = false;

            // Reset titulku a stavového řádku
            Title = "Elektro Offer - Kalkulace";
            StatusText = "Nový projekt";
        }

        /// <summary>
        /// Vymaže všechny řádky z obou sekcí (PRÁCE + MATERIÁL).
        /// Odhlásí event handlery před odebráním, aby nedošlo k memory leaku.
        /// </summary>
        private void ClearAllItems()
        {
            // Odhlášení event handlerů — důležité pro správnou správu paměti
            foreach (var item in WorkCalcItems)
                item.PropertyChanged -= Item_PropertyChanged;

            foreach (var item in MaterialItems)
                item.PropertyChanged -= Item_PropertyChanged;

            WorkCalcItems.Clear();
            MaterialItems.Clear();
            BudgetItems.Clear();
        }

        /// <summary>
        /// Aktualizuje titulek okna a text ve StatusBaru.
        /// Konvence v IT: "*" v titulku = neuložené změny.
        /// </summary>
        private void UpdateWindowTitle(string path)
        {
            var fileName = System.IO.Path.GetFileNameWithoutExtension(path);

            // Titulek okna: "NázevSouboru - Elektro Offer"
            Title = $"{fileName} - Elektro Offer";

            // StatusBar: celá cesta k souboru
            StatusText = path;
        }

        /// <summary>
        /// Označí projekt jako "má neuložené změny".
        /// Volá se při každé změně dat v UI.
        /// </summary>
        private void MarkAsChanged()
        {
            if (!_hasUnsavedChanges)
            {
                _hasUnsavedChanges = true;

                // Přidá hvězdičku do titulku (konvence: * = neuložené změny)
                if (!Title.StartsWith("*"))
                    Title = "* " + Title;
            }
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
            // potvrzovací dialog
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
            // potvrzovací dialog
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
                // zjištění, zda má řádek nějaká data
                bool isFilled =
                    item.SelectedTask != null ||
                    item.SelectedSpecification != null ||
                    item.SelectedMaterial != null ||
                    item.SelectedLocation != null ||
                    item.Quantity > 0;

                // potvrzení jen pokud je řádek vyplněný
                if (isFilled)
                {
                    if (MessageBox.Show(
                        "Opravdu chcete vymazat vyplněný řádek práce?",
                        "Potvrzení",
                        MessageBoxButton.OKCancel,
                        MessageBoxImage.Question) != MessageBoxResult.OK)
                        return;
                }

                // reset hodnot
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
                // zjištění, zda je řádek vyplněný
                bool isFilled =
                    item.MaterialItem != null ||
                    item.Quantity > 0;

                // potvrzení jen pokud má data
                if (isFilled)
                {
                    if (MessageBox.Show(
                        "Opravdu chcete vymazat vyplněný řádek materiálu?",
                        "Potvrzení",
                        MessageBoxButton.OKCancel,
                        MessageBoxImage.Question) != MessageBoxResult.OK)
                        return;
                }

                // reset hodnot
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
            MarkAsChanged(); // každá změna kolekce = neuložená změna
            Recalculate();
        }

        private void MaterialItems_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            MarkAsChanged(); // každá změna kolekce = neuložená změna
            Recalculate();
        }

        private void Item_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CalculationItemViewModel.Total) ||
                e.PropertyName == nameof(CalculationItemViewModel.Quantity) ||
                e.PropertyName == nameof(CalculationItemViewModel.WorkItem) ||
                e.PropertyName == nameof(CalculationItemViewModel.MaterialItem))
            {
                MarkAsChanged(); // každá změna hodnoty = neuložená změna
                Recalculate();
            }
        }

        // =========================================================
        // 💰 REKALKULACE CEN A ROZPISU
        // =========================================================
        private void Recalculate()
        {
            // =========================
            // 💰 CELKOVÉ SOUČTY
            // =========================
            WorkTotal = WorkCalcItems.Sum(x => x.Total);
            MaterialTotal = MaterialItems.Sum(x => x.Total);
            GrandTotal = WorkTotal + MaterialTotal;

            // =========================
            // 📊 VYČIŠTĚNÍ ROZPISU
            // =========================
            BudgetItems.Clear();

            // =========================
            // 🔧 PRÁCE
            // =========================
            foreach (var x in WorkCalcItems.Where(x => x.Total > 0))
            {
                BudgetItems.Add(new BudgetItem
                {
                    Type = "PRÁCE",

                    // popis sestavený z vybraných hodnot
                    Description = $"{x.SelectedTask} / {x.SelectedSpecification} / {x.SelectedMaterial} / {x.SelectedLocation}",

                    // měrná jednotka práce (hod, ks, apod.)
                    // ?? "" = fallback na prázdný string pokud WorkUnit vrátí null
                    Unit = x.WorkUnit ?? "",

                    // množství
                    Quantity = x.Quantity,

                    // výsledná cena řádku
                    Price = x.Total
                });
            }

            // =========================
            // 📦 MATERIÁL
            // =========================
            foreach (var x in MaterialItems.Where(x => x.Total > 0 && x.MaterialItem != null))
            {
                BudgetItems.Add(new BudgetItem
                {
                    Type = "MATERIÁL",

                    // název materiálu
                    Description = x.MaterialItem!.Name,

                    // měrná jednotka materiálu (ks, m, balení, ...)
                    Unit = x.MaterialItem?.Unit ?? "",

                    // množství
                    Quantity = x.Quantity,

                    // výsledná cena řádku
                    Price = x.Total
                });
            }
        }

        // =========================================================
        // PROPERTY CHANGED
        // =========================================================
        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    // =============================================================
    // ⌨️ RELAY COMMAND — pomocná třída pro klávesové zkratky
    // =============================================================

    /// <summary>
    /// Jednoduchá implementace ICommand pro použití v KeyBinding.
    /// Umožňuje předat lambda funkci jako příkaz bez nutnosti
    /// vytvářet celou třídu Command pro každou akci zvlášť.
    /// </summary>
    public class RelayCommand : System.Windows.Input.ICommand
    {
        private readonly Action<object?> _execute;

        public RelayCommand(Action<object?> execute) => _execute = execute;

        // CanExecute = vždy true (příkaz je vždy dostupný)
        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter) => _execute(parameter);

        // Napojení na WPF CommandManager — standardní pattern pro RelayCommand.
        // CommandManager.RequerySuggested se spustí, když WPF zjistí změnu stavu UI.
        // add/remove přes CommandManager eliminuje warning CS0067 (event se "používá").
        public event EventHandler? CanExecuteChanged
        {
            add => System.Windows.Input.CommandManager.RequerySuggested += value;
            remove => System.Windows.Input.CommandManager.RequerySuggested -= value;
        }
    }
}
