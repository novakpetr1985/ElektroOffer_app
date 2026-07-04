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
    /// Testy PropertyChanged, resetů a edge-case scénářů.
    /// =====================================================================
    public class CalculationItemViewModel_AdvancedTests
    {
        private SqliteConnection? _connection;
        private AppDbContext? _db;

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

            // Seed – jeden záznam PriceItems
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

        [TearDown]
        public void Cleanup()
        {
            _connection?.Close();
        }

        // ------------------------------------------------------------------
        // 🧪 TEST: PropertyChanged při změně Quantity
        // ------------------------------------------------------------------
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
        // 🧪 TEST: PropertyChanged při změně slevy
        // ------------------------------------------------------------------
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
        // 🧪 TEST: PropertyChanged při změně WorkItem
        // ------------------------------------------------------------------
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
        // 🧪 TEST: PropertyChanged při změně MaterialItem
        // ------------------------------------------------------------------
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
        // 🧪 TEST: ResetBelowTask()
        // ------------------------------------------------------------------
        [Test]
        public void ResetBelowTask_Should_Clear_All_Selections()
        {
            var vm = new CalculationItemViewModel(_db!);

            vm.SelectedTask = "Montáž";
            vm.SelectedSpecification = "CYKY 3x2,5";
            vm.SelectedMaterial = "CYKY";
            vm.SelectedLocation = "Stěna";
            vm.Quantity = 10;

            vm.SelectedTask = null; // vyvolá ResetBelowTask()

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
        // 🧪 TEST: ResetBelowSpecification()
        // ------------------------------------------------------------------
        [Test]
        public void ResetBelowSpecification_Should_Clear_Material_And_Location()
        {
            var vm = new CalculationItemViewModel(_db!);

            vm.SelectedTask = "Montáž";
            vm.SelectedSpecification = "CYKY 3x2,5";
            vm.SelectedMaterial = "CYKY";
            vm.SelectedLocation = "Stěna";

            vm.SelectedSpecification = null; // vyvolá ResetBelowSpecification()

            Assert.IsNull(vm.SelectedMaterial);
            Assert.IsNull(vm.SelectedLocation);
            Assert.IsEmpty(vm.AvailableMaterials);
            Assert.IsEmpty(vm.AvailableLocations);
        }

        // ------------------------------------------------------------------
        // 🧪 TEST: ResetBelowMaterial()
        // ------------------------------------------------------------------
        [Test]
        public void ResetBelowMaterial_Should_Clear_Location()
        {
            var vm = new CalculationItemViewModel(_db!);

            vm.SelectedTask = "Montáž";
            vm.SelectedSpecification = "CYKY 3x2,5";
            vm.SelectedMaterial = "CYKY";
            vm.SelectedLocation = "Stěna";

            vm.SelectedMaterial = null; // vyvolá ResetBelowMaterial()

            Assert.IsNull(vm.SelectedLocation);
            Assert.IsEmpty(vm.AvailableLocations);
        }

        // ------------------------------------------------------------------
        // 🧪 TEST: Edge-case — Quantity = 0
        // ------------------------------------------------------------------
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
        // 🧪 TEST: Edge-case — sleva 100 %
        // ------------------------------------------------------------------
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
        // 🧪 TEST: Edge-case — sleva > 100 %
        // ------------------------------------------------------------------
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
            vm.DiscountPercent = 150; // 150 % sleva

            Assert.AreEqual(0, vm.Total); // logicky nesmí být záporné
        }
    }
}
