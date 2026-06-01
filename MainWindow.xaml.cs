using ElektroOffer_app.Data;
using ElektroOffer_app.Models;
using System;
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
        public ObservableCollection<PriceItems> WorkItemsSource { get; set; } = new();
        public ObservableCollection<Material> Materials { get; set; } = new();

        // =========================================================
        // 🔧 FILTRY
        // =========================================================
        public ObservableCollection<string> Tasks { get; set; } = new();
        public ObservableCollection<string> Specifications { get; set; } = new();
        public ObservableCollection<string> Material { get; set; } = new();
        public ObservableCollection<string> Locations { get; set; } = new();

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

                WorkItemsSource = new ObservableCollection<PriceItems>(
                    db.PriceItems.ToList()
                );

                Materials = new ObservableCollection<Material>(
                    db.Materials.ToList()
                );

                Tasks = new ObservableCollection<string>(
                    WorkItemsSource.Select(x => x.Task).Distinct()
                );

                Specifications = new ObservableCollection<string>(
                    WorkItemsSource.Select(x => x.Specification).Distinct()
                );

                Material = new ObservableCollection<string>(
                    WorkItemsSource.Select(x => x.Material).Distinct()
                );

                Locations = new ObservableCollection<string>(
                    WorkItemsSource.Select(x => x.Location).Distinct()
                );
            }

            // =====================================================
            // INIT ŘÁDKŮ
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
        // 📌 COLLECTION CHANGE HANDLERS
        // =========================================================
        private void WorkCalcItems_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
                foreach (CalculationItems item in e.OldItems)
                    item.PropertyChanged -= Item_PropertyChanged;

            Recalculate();
        }

        private void MaterialItems_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
                foreach (CalculationItems item in e.OldItems)
                    item.PropertyChanged -= Item_PropertyChanged;

            Recalculate();
        }

        // =========================================================
        // 🔥 KLÍČOVÝ LIVE UPDATE
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