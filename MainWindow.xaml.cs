using ElektroOffer_app.Models;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;

namespace ElektroOffer_app
{
    public partial class MainWindow : Window
    {
        // ---------------------------
        // 📦 CENÍKY
        // ---------------------------

        // ceník PRÁCE
        private ObservableCollection<PriceItems> _priceItems = new();

        // ceník MATERIÁLU (tabulka Materials)
        public ObservableCollection<Models.Material> Materials { get; set; } = new();
        public ObservableCollection<Models.WorkItem> WorkItems { get; set; } = new();


        // ---------------------------
        // 📦 KALKULAČNÍ ŘÁDKY
        // ---------------------------

        private ObservableCollection<CalculationItems> _workItems = new();
        private ObservableCollection<CalculationItems> _materialItems = new();


        // ---------------------------
        // 🏗️ START APLIKACE
        // ---------------------------
        public MainWindow()
        {
            InitializeComponent();

            // ⭐ DŮLEŽITÉ PRO BINDING
            DataContext = this;

            // načtení dat z DB
            LoadPriceItems();
            LoadMaterials();

            // napojení UI
            SetupUI();

            // startovní řádky
            for (int i = 0; i < 5; i++)
            {
                _workItems.Add(new CalculationItems());
                _materialItems.Add(new CalculationItems());
            }
        }


        // ---------------------------
        // 🔗 PROPOJENÍ UI
        // ---------------------------
        private void SetupUI()
        {
            WorkItemsControl.ItemsSource = _workItems;
            MaterialItemsControl.ItemsSource = _materialItems;
        }


        // ---------------------------
        // 📥 NAČTENÍ PRÁCE (PriceItems)
        // ---------------------------
        private void LoadPriceItems()
        {
            _priceItems.Clear();

            string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "elektrooffer.db");

            using var connection = new SqliteConnection($"Data Source={dbPath}");
            connection.Open();

            var cmd = connection.CreateCommand();

            cmd.CommandText =
                @"SELECT Id, BasePrice, Unit, Task, Specification, Material, Location, MaterialCoef, PositionCoef
                  FROM PriceItems";

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                _priceItems.Add(new PriceItems
                {
                    Id = reader.GetInt32(0),
                    BasePrice = reader.GetDouble(1),
                    Unit = reader.GetString(2),
                    Task = reader.GetString(3),
                    Specification = reader.GetString(4),
                    Material = reader.GetString(5),
                    Location = reader.GetString(6),
                    MaterialCoef = reader.GetDouble(7),
                    PositionCoef = reader.GetDouble(8)
                });
            }
        }


        // ---------------------------
        // 📥 NAČTENÍ MATERIÁLU (Materials tabulka)
        // ---------------------------
        private void LoadMaterials()
        {
            Materials.Clear();

            string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "elektrooffer.db");

            using var connection = new SqliteConnection($"Data Source={dbPath}");
            connection.Open();

            var cmd = connection.CreateCommand();

            cmd.CommandText =
                @"SELECT Id, Name, Price, Unit
                  FROM Materials";

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                Materials.Add(new Models.Material
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Price = reader.GetDouble(2),
                    Unit = reader.GetString(3)
                });
            }
        }


        // ---------------------------
        // ➕ PŘIDÁNÍ PRÁCE
        // ---------------------------
        private void AddWorkItem_Click(object sender, RoutedEventArgs e)
        {
            _workItems.Add(new CalculationItems());
        }


        // ---------------------------
        // ➕ PŘIDÁNÍ MATERIÁLU
        // ---------------------------
        private void AddMaterialsItem_Click(object sender, RoutedEventArgs e)
        {
            _materialItems.Add(new CalculationItems());
        }


        // ---------------------------
        // 💰 CELKOVÁ CENA
        // ---------------------------
        private double GetGrandTotal()
        {
            return _workItems.Sum(x => x.Total)
                 + _materialItems.Sum(x => x.Total);
        }
    }
}