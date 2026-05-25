using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Data.Sqlite;
using ElektroOffer_app.Models;
using System.IO;

namespace ElektroOffer_app
{
    public partial class MainWindow : Window
    {
        private ObservableCollection<PriceItems> _priceItems = new();
        private ObservableCollection<CalculationItems> _calculationItems = new();

        public MainWindow()
        {
            InitializeComponent();

            LoadPriceItems();

            // 10 prázdných řádků (jak jsi chtěl)
            for (int i = 0; i < 10; i++)
            {
                _calculationItems.Add(new CalculationItems());
            }

            SetupUI();
        }

        // ---------------------------
        // NAČTENÍ CENÍKU Z DB
        // ---------------------------
        private void LoadPriceItems()
        {
            _priceItems.Clear();

            string dbPath =
                 System.IO.Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "elektrooffer.db");

            using var connection =
                new SqliteConnection($"Data Source={dbPath}");
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
        // NASTAVENÍ COMBOBOXŮ
        // ---------------------------
        private void SetupUI()
        {
            // napojení ComboBoxu (v XAML musí být ItemsControl / ListBox / StackPanel)
            PriceItemCombo.ItemsSource = _priceItems;
            PriceItemCombo.DisplayMemberPath = "FullName";
        }

        // ---------------------------
        // VÝPOČET 1 ŘÁDKU
        // ---------------------------
        private void OnSelectionChanged(object sender, EventArgs e)
        {
            try
            {
                if (PriceItemCombo.SelectedItem is not PriceItems item)
                    return;

                double quantity = 0;
                double.TryParse(QuantityTextBox.Text, out quantity);

                double total =
                    item.BasePrice *
                    item.MaterialCoef *
                    item.PositionCoef *
                    quantity;

                ResultText.Text = $"{total:N2} Kč";
            }
            catch
            {
                ResultText.Text = "";
            }
        }

        // ---------------------------
        // CELKOVÝ SOUČET (pro budoucí UI)
        // ---------------------------
        private double GetGrandTotal()
        {
            return _calculationItems
                .Where(x => x.Item != null)
                .Sum(x => x.Total);
        }
    }
}