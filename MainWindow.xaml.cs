using ElektroOffer_app.Data;
using ElektroOffer_app.Models;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;

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
        public ObservableCollection<CalculationItems> WorkCalcItems { get; set; } = new();
        public ObservableCollection<CalculationItems> MaterialItems { get; set; } = new();

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
        // START
        // =========================================================
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            using (var db = new AppDbContext())
            {
                db.Database.EnsureCreated();

                Materials = new ObservableCollection<Material>(db.Materials.ToList());

                Tasks = new ObservableCollection<string>(
                    db.PriceItems.Select(x => x.Task).Distinct().ToList()
                );
            }

            for (int i = 0; i < 5; i++)
            {
                AddWorkItemInternal();
                AddMaterialItemInternal();
            }

            WorkCalcItems.CollectionChanged += WorkCalcItems_CollectionChanged;
            MaterialItems.CollectionChanged += MaterialItems_CollectionChanged;
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
            var item = new CalculationItems();
            item.PropertyChanged += Item_PropertyChanged;
            WorkCalcItems.Add(item);
        }

        private void AddMaterialItemInternal()
        {
            var item = new CalculationItems();
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
                fe.DataContext is CalculationItems item)
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
                fe.DataContext is CalculationItems item)
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
                fe.DataContext is CalculationItems item)
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
                fe.DataContext is CalculationItems item)
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
            => Recalculate();

        private void MaterialItems_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
            => Recalculate();

        private void Item_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CalculationItems.Total) ||
                e.PropertyName == nameof(CalculationItems.Quantity) ||
                e.PropertyName == nameof(CalculationItems.WorkItem) ||
                e.PropertyName == nameof(CalculationItems.MaterialItem))
            {
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
                    Unit = x.WorkUnit,

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
}