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
        // 🔧 GLOBÁLNÍ SEZNAM ÚKONŮ (Task combobox)
        // =========================================================
        // 👉 Specifications/Material/Locations jsou per-řádek v CalculationItems
        public ObservableCollection<string> Tasks { get; set; } = new();

        // =========================================================
        // 🧮 KALKULACE
        // =========================================================
        public ObservableCollection<CalculationItems> WorkCalcItems { get; set; } = new();
        public ObservableCollection<CalculationItems> MaterialItems { get; set; } = new();

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
        // START
        // =========================================================
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            using (var db = new AppDbContext())
            {
                db.Database.EnsureCreated();

                Materials = new ObservableCollection<Material>(
                    db.Materials.ToList()
                );

                Tasks = new ObservableCollection<string>(
                    db.PriceItems.Select(x => x.Task).Distinct().ToList()
                );
            }

            // =====================================================
            // INIT ŘÁDKŮ (start UI)
            // =====================================================
            for (int i = 0; i < 5; i++)
            {
                AddWorkItemInternal();
                AddMaterialItemInternal();
            }

            // =====================================================
            // TRACKING KOLEKCÍ
            // =====================================================
            WorkCalcItems.CollectionChanged += WorkCalcItems_CollectionChanged;
            MaterialItems.CollectionChanged += MaterialItems_CollectionChanged;
        }

        // =========================================================
        // ➕ ADD BUTTONY
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
        // ➖ DELETE ROW
        // =========================================================
        // 👉 Handler se odpojuje zde — CollectionChanged ho již NEodpojuje
        private void DeleteWorkItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe &&
                fe.DataContext is CalculationItems item)
            {
                item.PropertyChanged -= Item_PropertyChanged;
                WorkCalcItems.Remove(item);
            }
        }

        private void DeleteMaterialItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe &&
                fe.DataContext is CalculationItems item)
            {
                item.PropertyChanged -= Item_PropertyChanged;
                MaterialItems.Remove(item);
            }
        }

        // =========================================================
        // 🧹 RESET ROW
        // =========================================================
        // 👉 Vymaže obsah řádku, řádek zůstává v tabulce
        private void ResetWorkItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe &&
                fe.DataContext is CalculationItems item)
            {
                item.SelectedTask = null;
                item.Quantity = 0;
                Recalculate();
            }
        }

        private void ResetMaterialItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe &&
                fe.DataContext is CalculationItems item)
            {
                item.MaterialItem = null;
                item.Quantity = 0;
                Recalculate();
            }
        }

        // =========================================================
        // 📌 COLLECTION CHANGE HANDLERS
        // =========================================================
        // 👉 Odpojení handleru řeší Delete metody — zde jen rekalkulace
        private void WorkCalcItems_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
            => Recalculate();

        private void MaterialItems_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
            => Recalculate();

        // =========================================================
        // 🔥 LIVE UPDATE
        // =========================================================
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
        // 💰 REKALKULACE
        // =========================================================
        private void Recalculate()
        {
            GrandTotal =
                WorkCalcItems.Sum(x => x.Total) +
                MaterialItems.Sum(x => x.Total);
        }

        // =========================================================
        // PROPERTY CHANGED
        // =========================================================
        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
