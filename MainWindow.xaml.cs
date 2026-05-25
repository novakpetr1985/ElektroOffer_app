using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Microsoft.Data.Sqlite;
using ElektroOffer_app.Models;
using System.IO;

namespace ElektroOffer_app
{
    public partial class MainWindow : Window
    {
        // ---------------------------
        // 📦 DATOVÉ KOLEKCE
        // ---------------------------

        // Ceník načtený z databáze (Práce + Materiál)
        private ObservableCollection<PriceItems> _priceItems = new();

        // Řádky pro PRÁCI
        private ObservableCollection<CalculationItems> _workItems = new();

        // Řádky pro MATERIÁL
        private ObservableCollection<CalculationItems> _materialItems = new();


        // ---------------------------
        // 🏗️ KONSTRUKTOR - START APPKY
        // ---------------------------
        public MainWindow()
        {
            InitializeComponent();

            // 1) načti ceník z DB
            LoadPriceItems();

            // 2) inicializuj UI vazby (ItemsControl)
            SetupUI();

            // 3) vytvoř startovní prázdné řádky
            for (int i = 0; i < 5; i++)
            {
                _workItems.Add(new CalculationItems());
                _materialItems.Add(new CalculationItems());
            }
        }


        // ---------------------------
        // 🔗 PROPOJENÍ UI ↔ DATA
        // ---------------------------
        private void SetupUI()
        {
            // ItemsControl v XAML (PRÁCE)
            WorkItemsControl.ItemsSource = _workItems;

            // ItemsControl v XAML (MATERIÁL)
            MaterialItemsControl.ItemsSource = _materialItems;
        }


        // ---------------------------
        // 📥 NAČTENÍ CENÍKU Z DB
        // ---------------------------
        private void LoadPriceItems()
        {
            _priceItems.Clear();

            // cesta k SQLite databázi
            string dbPath =
                System.IO.Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "elektrooffer.db");

            using var connection =
                new SqliteConnection($"Data Source={dbPath}");

            connection.Open();

            var cmd = connection.CreateCommand();

            // načítáme všechny položky ceníku
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
        // ➕ PŘIDÁNÍ PRÁCE (BUTTON +)
        // ---------------------------
        private void AddWorkItem_Click(object sender, RoutedEventArgs e)
        {
            _workItems.Add(new CalculationItems());
        }


        // ---------------------------
        // ➕ PŘIDÁNÍ MATERIÁLU (BUTTON +)
        // ---------------------------
        private void AddMaterialItem_Click(object sender, RoutedEventArgs e)
        {
            _materialItems.Add(new CalculationItems());
        }


        // ---------------------------
        // 💰 VÝPOČET CELKOVÉ CENY
        // ---------------------------
        private double GetGrandTotal()
        {
            // součet práce
            double workTotal = _workItems.Sum(x => x.Total);

            // součet materiálu
            double materialTotal = _materialItems.Sum(x => x.Total);

            return workTotal + materialTotal;
        }


        // ---------------------------
        // 🧠 STARÁ LOGIKA (LEGACY - NEPOUŽÍVÁ SE VE VARIANTĚ 2)
        // ---------------------------
        /*
        private void OnSelectionChanged(...)
        {
            // toto je původní single-item logika
            // ve variantě 2 už se NEPOUŽÍVÁ
        }
        */
    }
}