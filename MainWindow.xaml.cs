using ElektroOffer_app.Models;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;

namespace ElektroOffer_app
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        // =========================
        // 📦 DATA Z DB
        // =========================
        public ObservableCollection<PriceItems> WorkItemsSource { get; set; } = new();
        public ObservableCollection<Material> Materials { get; set; } = new();

        // =========================
        // 📦 KALKULAČNÍ ŘÁDKY
        // =========================
        public ObservableCollection<CalculationItems> WorkCalcItems { get; set; } = new();
        public ObservableCollection<CalculationItems> MaterialItems { get; set; } = new();

        // =========================
        // 💰 CELKEM
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
        // START
        // =========================
        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;

            LoadWorkItems();
            LoadMaterials();

            // startovní řádky
            for (int i = 0; i < 5; i++)
            {
                WorkCalcItems.Add(new CalculationItems());
                MaterialItems.Add(new CalculationItems());
            }

            // sledování změn kolekcí
            WorkCalcItems.CollectionChanged += (_, __) => Recalculate();
            MaterialItems.CollectionChanged += (_, __) => Recalculate();
        }

        // =========================
        // ➕ WORK ITEM
        // =========================
        private void AddWorkItem_Click(object sender, RoutedEventArgs e)
        {
            WorkCalcItems.Add(new CalculationItems());
        }

        // =========================
        // ➕ MATERIAL ITEM
        // =========================
        private void AddMaterialsItem_Click(object sender, RoutedEventArgs e)
        {
            MaterialItems.Add(new CalculationItems());
        }

        // =========================
        // 💰 PŘEPočet
        // =========================
        private void Recalculate()
        {
            GrandTotal =
                WorkCalcItems.Sum(x => x.Total) +
                MaterialItems.Sum(x => x.Total);
        }

        // =========================
        // 📥 LOAD WORK ITEMS
        // =========================
        private void LoadWorkItems()
        {
            string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "elektrooffer.db");

            using var connection = new SqliteConnection($"Data Source={dbPath}");
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = @"SELECT Id, BasePrice, Unit, Task, MaterialCoef, PositionCoef FROM PriceItems";

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                WorkItemsSource.Add(new PriceItems
                {
                    Id = reader.GetInt32(0),
                    BasePrice = reader.GetDouble(1),
                    Unit = reader.GetString(2),
                    Task = reader.GetString(3),
                    MaterialCoef = reader.GetDouble(4),
                    PositionCoef = reader.GetDouble(5)
                });
            }
        }

        // =========================
        // 📥 LOAD MATERIALS
        // =========================
        private void LoadMaterials()
        {
            string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "elektrooffer.db");

            using var connection = new SqliteConnection($"Data Source={dbPath}");
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = @"SELECT Id, Name, Price, Unit FROM Materials";

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                Materials.Add(new Material
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Price = reader.GetDouble(2),
                    Unit = reader.GetString(3)
                });
            }
        }

        // =========================
        // PROPERTY CHANGED
        // =========================
        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}