using ElektroOffer_app.Data;
using ElektroOffer_app.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;

namespace ElektroOffer_app
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        // =========================
        // 📦 DATA Z DATABÁZE
        // =========================
        public ObservableCollection<PriceItems> WorkItemsSource { get; set; } = new();
        public ObservableCollection<Material> Materials { get; set; } = new();

        // =========================
        // 🧮 KALKULAČNÍ ŘÁDKY
        // =========================
        public ObservableCollection<CalculationItems> WorkCalcItems { get; set; } = new();
        public ObservableCollection<CalculationItems> MaterialItems { get; set; } = new();

        // =========================
        // 💰 CELKOVÁ CENA
        // =========================
        private double _grandTotal;

        public double GrandTotal
        {
            get => _grandTotal;
            set
            {
                _grandTotal = value;
                OnPropertyChanged();
            }
        }

        // =========================
        // 🚀 START APLIKACE
        // =========================
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            // =========================
            // 📥 NAČTENÍ DAT Z DB (EF CORE)
            // =========================
            using (var db = new AppDbContext())
            {
                db.Database.EnsureCreated();

                WorkItemsSource = new ObservableCollection<PriceItems>(
                    db.PriceItems.ToList()
                );

                Materials = new ObservableCollection<Material>(
                    db.Materials.ToList()
                );
            }

            // =========================
            // 🧮 STARTOVNÍ ŘÁDKY
            // =========================
            for (int i = 0; i < 5; i++)
            {
                WorkCalcItems.Add(new CalculationItems());
                MaterialItems.Add(new CalculationItems());
            }

            // =========================
            // 🔔 SLEDOVÁNÍ ZMĚN
            // =========================
            WorkCalcItems.CollectionChanged += (_, __) => Recalculate();
            MaterialItems.CollectionChanged += (_, __) => Recalculate();
        }

        // =========================
        // ➕ PŘIDAT PRÁCI
        // =========================
        private void AddWorkItem_Click(object sender, RoutedEventArgs e)
        {
            WorkCalcItems.Add(new CalculationItems());
        }

        // =========================
        // ➕ PŘIDAT MATERIÁL
        // =========================
        private void AddMaterialsItem_Click(object sender, RoutedEventArgs e)
        {
            MaterialItems.Add(new CalculationItems());
        }

        // =========================
        // 💰 PŘEPočet CELKU
        // =========================
        private void Recalculate()
        {
            GrandTotal =
                WorkCalcItems.Sum(x => x.Total) +
                MaterialItems.Sum(x => x.Total);
        }

        // =========================
        // 🔔 NOTIFIKACE UI
        // =========================
        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}