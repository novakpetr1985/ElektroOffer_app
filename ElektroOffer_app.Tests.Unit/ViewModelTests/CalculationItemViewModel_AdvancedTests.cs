using NUnit.Framework;
using ElektroOffer_app.ViewModels.Items;
using ElektroOffer_app.Models;
using ElektroOffer_app.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace ElektroOffer_app.Tests.Unit.ViewModels
{
    /// =====================================================================
    /// 🧪 ADVANCED UNIT TESTS — CalculationItemViewModel
    /// =====================================================================
    /// Tyto testy ověřují pokročilé chování ViewModelu, které:
    ///   • vyžaduje databázi (SQLite InMemory)
    ///   • zahrnuje reset logiky při změně Task/Specification/Material
    ///   • ověřuje PropertyChanged události související s databází
    ///   • testuje edge-case scénáře, které nejsou pokryty základními testy
    ///
    /// Základní testy (výpočty Total, slevy, koeficienty, základní PropertyChanged)
    /// jsou umístěny v CalculationItemViewModelTests.cs.
    ///
    /// Tento soubor obsahuje pouze testy, které:
    ///   • pracují s databází
    ///   • testují reset kaskády výběru
    ///   • testují načítání dostupných hodnot z DB
    ///   • testují načítání WorkUnit a ukládání WorkItem
    ///
    /// =====================================================================
    public class CalculationItemViewModel_AdvancedTests
    {
        private SqliteConnection? _connection;
        private AppDbContext? _db;

        // ------------------------------------------------------------------
        // 🧰 SETUP — vytvoření izolované SQLite InMemory databáze
        // ------------------------------------------------------------------
        /// Používáme SQLite InMemory, protože:
        ///   • je extrémně rychlá
        ///   • chová se stejně jako skutečná SQLite DB
        ///   • EF Core nad ní funguje plnohodnotně (migrace, LINQ, tracking)
        ///   • testy jsou izolované — po každém testu se DB zahodí
        [SetUp]
        public void Setup()
        {
            _connection = new SqliteConnection("Data Source=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(_connection)
                .Options;

            _db = new AppDbContext(options);
            _db.Database.EnsureCreated();

            // Seed — vložíme jeden záznam PriceItems, aby testy měly data
            _db.PriceItems.Add(new PriceItems
            {
                Task = "Montáž",
                Specification = "CYKY 3x2,5",
                Material = "CYKY",
                Location = "Stěna",
                Unit = "m",
                BasePrice = 100,
                MaterialCoef = 1.1,
                PositionCoef = 1.0
            });

            _db.SaveChanges();
        }

        // ------------------------------------------------------------------
        // 🧹 TEARDOWN — uzavření DB po testu
        // ------------------------------------------------------------------
        /// Uzavření připojení je důležité:
        ///   • uvolní paměť
        ///   • zabrání únikům handle
        ///   • zajistí čisté prostředí pro další test
        [TearDown]
        public void Cleanup()
        {
            _connection?.Close();
        }

        // ------------------------------------------------------------------
        // 🧪 TEST 1: PropertyChanged při změně Quantity
        // ------------------------------------------------------------------
        /// Ověřuje:
        ///   • že změna Quantity vyvolá PropertyChanged("Total")
        ///   • že ViewModel správně reaguje na změnu vstupních dat
        ///   • že MVVM notifikace funguje i při použití DB
        [Test]
        public void Quantity_Should_Raise_PropertyChanged_For_Total()
        {
            var vm = new CalculationItemViewModel(_db!);

            vm.WorkItem = new PriceItems
            {
                BasePrice = 100,
                MaterialCoef = 1,
                PositionCoef = 1
            };

            var changed = new List<string>();

            vm.PropertyChanged += (_, e) => changed.Add(e.PropertyName!);

            vm.Quantity = 5;

            Assert.Contains(nameof(CalculationItemViewModel.Total), changed);
        }

        // ------------------------------------------------------------------
        // 🧪 TEST 2: PropertyChanged při změně slevy
        // ------------------------------------------------------------------
        /// Ověřuje:
        ///   • že změna DiscountPercent vyvolá PropertyChanged("Total")
        ///   • že sleva je součástí výpočtu Total
        ///   • že MVVM notifikace funguje správně
        [Test]
        public void DiscountPercent_Should_Raise_PropertyChanged_For_Total()
        {
            var vm = new CalculationItemViewModel(_db!);

            vm.WorkItem = new PriceItems
            {
                BasePrice = 100,
                MaterialCoef = 1,
                PositionCoef = 1
            };

            var changed = new List<string>();

            vm.PropertyChanged += (_, e) => changed.Add(e.PropertyName!);

            vm.DiscountPercent = 10;

            Assert.Contains(nameof(CalculationItemViewModel.Total), changed);
        }

        // ------------------------------------------------------------------
        // 🧪 TEST 3: PropertyChanged při změně WorkItem
        // ------------------------------------------------------------------
        /// Ověřuje:
        ///   • že změna WorkItem ovlivní Total
        ///   • že ViewModel správně reaguje na změnu cenové položky
        [Test]
        public void WorkItem_Should_Raise_PropertyChanged_For_Total()
        {
            var vm = new CalculationItemViewModel(_db!);

            var changed = new List<string>();

            vm.PropertyChanged += (_, e) => changed.Add(e.PropertyName!);

            vm.WorkItem = new PriceItems
            {
                BasePrice = 200,
                MaterialCoef = 1,
                PositionCoef = 1
            };

            Assert.Contains(nameof(CalculationItemViewModel.Total), changed);
        }

        // ------------------------------------------------------------------
        // 🧪 TEST 4: PropertyChanged při změně MaterialItem
        // ------------------------------------------------------------------
        /// Ověřuje:
        ///   • že změna MaterialItem ovlivní Total
        ///   • že ViewModel správně přepíná mezi WorkItem a MaterialItem
        [Test]
        public void MaterialItem_Should_Raise_PropertyChanged_For_Total()
        {
            var vm = new CalculationItemViewModel(_db!);

            var changed = new List<string>();

            vm.PropertyChanged += (_, e) => changed.Add(e.PropertyName!);

            vm.MaterialItem = new Material
            {
                Name = "CYKY",
                Price = 50
            };

            Assert.Contains(nameof(CalculationItemViewModel.Total), changed);
        }

        // ------------------------------------------------------------------
        // 🧪 TEST 5: ResetBelowTask()
        // ------------------------------------------------------------------
        /// Ověřuje:
        ///   • že změna Task resetuje celou kaskádu výběru
        ///   • že ViewModel se vrátí do konzistentního stavu
        ///   • že se vymažou dostupné hodnoty (Specifications, Materials, Locations)
        ///   • že se vymaže WorkItem i WorkUnit
        [Test]
        public void ResetBelowTask_Should_Clear_All_Selections()
        {
            var vm = new CalculationItemViewModel(_db!);

            vm.SelectedTask = "Montáž";
            vm.SelectedSpecification = "CYKY 3x2,5";
            vm.SelectedMaterial = "CYKY";
            vm.SelectedLocation = "Stěna";
            vm.Quantity = 10;

            vm.SelectedTask = null;

            Assert.IsNull(vm.SelectedSpecification);
            Assert.IsNull(vm.SelectedMaterial);
            Assert.IsNull(vm.SelectedLocation);
            Assert.IsNull(vm.WorkItem);
            Assert.IsNull(vm.WorkUnit);
            Assert.IsEmpty(vm.AvailableSpecifications);
            Assert.IsEmpty(vm.AvailableMaterials);
            Assert.IsEmpty(vm.AvailableLocations);
        }

        // ------------------------------------------------------------------
        // 🧪 TEST 6: ResetBelowSpecification()
        // ------------------------------------------------------------------
        /// Ověřuje:
        ///   • že změna Specification resetuje Material a Location
        ///   • že ViewModel neumožní nekonzistentní stav
        [Test]
        public void ResetBelowSpecification_Should_Clear_Material_And_Location()
        {
            var vm = new CalculationItemViewModel(_db!);

            vm.SelectedTask = "Montáž";
            vm.SelectedSpecification = "CYKY 3x2,5";
            vm.SelectedMaterial = "CYKY";
            vm.SelectedLocation = "Stěna";

            vm.SelectedSpecification = null;

            Assert.IsNull(vm.SelectedMaterial);
            Assert.IsNull(vm.SelectedLocation);
            Assert.IsEmpty(vm.AvailableMaterials);
            Assert.IsEmpty(vm.AvailableLocations);
        }

        // ------------------------------------------------------------------
        // 🧪 TEST 7: ResetBelowMaterial()
        // ------------------------------------------------------------------
        /// Ověřuje:
        ///   • že změna Material resetuje Location
        ///   • že ViewModel udržuje konzistentní výběr
        [Test]
        public void ResetBelowMaterial_Should_Clear_Location()
        {
            var vm = new CalculationItemViewModel(_db!);

            vm.SelectedTask = "Montáž";
            vm.SelectedSpecification = "CYKY 3x2,5";
            vm.SelectedMaterial = "CYKY";
            vm.SelectedLocation = "Stěna";

            vm.SelectedMaterial = null;

            Assert.IsNull(vm.SelectedLocation);
            Assert.IsEmpty(vm.AvailableLocations);
        }

        // ------------------------------------------------------------------
        // 🧪 TEST 8: Edge-case — Quantity = 0
        // ------------------------------------------------------------------
        /// Ověřuje:
        ///   • že Total je 0, pokud Quantity = 0
        ///   • že ViewModel správně ošetřuje nulové množství
        [Test]
        public void Total_Should_Be_Zero_When_Quantity_Is_Zero()
        {
            var vm = new CalculationItemViewModel(_db!);

            vm.WorkItem = new PriceItems
            {
                BasePrice = 100,
                MaterialCoef = 1,
                PositionCoef = 1
            };

            vm.Quantity = 0;

            Assert.AreEqual(0, vm.Total);
        }

        // ------------------------------------------------------------------
        // 🧪 TEST 9: Edge-case — sleva 100 %
        // ------------------------------------------------------------------
        /// Ověřuje:
        ///   • že sleva 100 % vynuluje Total
        ///   • že ViewModel správně aplikuje maximální slevu
        [Test]
        public void Total_Should_Be_Zero_When_Discount_Is_100_Percent()
        {
            var vm = new CalculationItemViewModel(_db!);

            vm.WorkItem = new PriceItems
            {
                BasePrice = 100,
                MaterialCoef = 1,
                PositionCoef = 1
            };

            vm.Quantity = 5;
            vm.IsDiscountEnabled = true;
            vm.DiscountPercent = 100;

            Assert.AreEqual(0, vm.Total);
        }

        // ------------------------------------------------------------------
        // 🧪 TEST 10: Edge-case — sleva > 100 %
        // ------------------------------------------------------------------
        /// Ověřuje:
        ///   • že sleva > 100 % nevede k záporné ceně
        ///   • že Total je správně oříznut na 0
        [Test]
        public void Total_Should_Not_Be_Negative_When_Discount_Above_100()
        {
            var vm = new CalculationItemViewModel(_db!);

            vm.WorkItem = new PriceItems
            {
                BasePrice = 100,
                MaterialCoef = 1,
                PositionCoef = 1
            };

            vm.Quantity = 5;
            vm.IsDiscountEnabled = true;
            vm.DiscountPercent = 150;

            Assert.AreEqual(0, vm.Total);
        }
    }
}
