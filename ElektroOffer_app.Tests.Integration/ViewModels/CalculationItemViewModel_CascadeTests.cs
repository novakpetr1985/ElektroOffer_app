using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using ElektroOffer_app.Data;
using ElektroOffer_app.Models;
using ElektroOffer_app.ViewModels.Items;

namespace ElektroOffer_app.Tests.Integration.ViewModels
{
    /// =====================================================================
    /// 🔗 INTEGRATION TESTS — CalculationItemViewModel (Cascade Logic)
    /// =====================================================================
    /// Testujeme kompletní kaskádu:
    ///   Task → Specification → Material → Location
    ///
    /// Ověřujeme:
    ///   • reset hodnot při změně vyšší úrovně
    ///   • načítání dostupných hodnot z DB
    ///   • vyvolání PropertyChanged
    ///   • přepočet Total
    /// =====================================================================
    public class CalculationItemViewModel_CascadeTests
    {
        private SqliteConnection _connection = null!;
        private AppDbContext _db = null!;
        private CalculationItemViewModel _vm = null!;

        // =========================================================
        // SETUP
        // =========================================================
        [SetUp]
        public void SetUp()
        {
            _connection = new SqliteConnection("Data Source=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(_connection)
                .Options;

            _db = new AppDbContext(options);
            _db.Database.EnsureCreated();

            SeedTestData();

            _vm = new CalculationItemViewModel(_db);
        }

        // =========================================================
        // TEARDOWN
        // =========================================================
        [TearDown]
        public void TearDown()
        {
            _db.Dispose();
            _connection.Dispose();
        }

        // =========================================================
        // TEST DATA
        // =========================================================
        private void SeedTestData()
        {
            _db.PriceItems.AddRange(new[]
            {
                new PriceItems { Task = "Montáž", Specification = "Kabel", Material = "CYKY 3x2.5", Location = "Strop", BasePrice = 100, MaterialCoef = 1.2, PositionCoef = 1.0 },
                new PriceItems { Task = "Montáž", Specification = "Kabel", Material = "CYKY 3x2.5", Location = "Podlaha", BasePrice = 100, MaterialCoef = 1.2, PositionCoef = 0.8 },
                new PriceItems { Task = "Montáž", Specification = "Trubka", Material = "Kopoflex", Location = "Stěna", BasePrice = 50, MaterialCoef = 1.0, PositionCoef = 1.0 },
                new PriceItems { Task = "Demontáž", Specification = "Demontáž kabelu", Material = "CYKY 3x2.5", Location = "Strop", BasePrice = 30, MaterialCoef = 1.0, PositionCoef = 1.0 }
            });

            _db.SaveChanges();
        }

        // =========================================================
        // 1) Task → reset Specification, Material, Location
        // =========================================================
        [Test]
        public void Changing_Task_Should_Reset_Specification_Material_Location()
        {
            _vm.SelectedTask = "Montáž";
            _vm.SelectedSpecification = "Kabel";
            _vm.SelectedMaterial = "CYKY 3x2.5";
            _vm.SelectedLocation = "Strop";

            _vm.SelectedTask = "Demontáž"; // Tohle je ta změna Task

            Assert.IsNull(_vm.SelectedSpecification);
            Assert.IsNull(_vm.SelectedMaterial);
            Assert.IsNull(_vm.SelectedLocation);
        }


        // =========================================================
        // 2) Specification → reset Material, Location
        // =========================================================
        [Test]
        public void Changing_Specification_Should_Reset_Material_And_Location()
        {
            _vm.SelectedTask = "Montáž";
            _vm.SelectedSpecification = "Kabel";
            _vm.SelectedMaterial = "CYKY 3x2.5";
            _vm.SelectedLocation = "Strop";

            _vm.SelectedSpecification = "Trubka";

            Assert.IsNull(_vm.SelectedMaterial);
            Assert.IsNull(_vm.SelectedLocation);
        }

        // =========================================================
        // 3) Material → reset Location
        // =========================================================
        [Test]
        public void Changing_Material_Should_Reset_Location()
        {
            _vm.SelectedTask = "Montáž";
            _vm.SelectedSpecification = "Kabel";
            _vm.SelectedMaterial = "CYKY 3x2.5";
            _vm.SelectedLocation = "Strop";

            _vm.SelectedMaterial = "Kopoflex"; // změna hodnoty → reset proběhne

            Assert.IsNull(_vm.SelectedLocation);
        }

        // =========================================================
        // 4) Location → přepočet Total
        // =========================================================
        [Test]
        public void Changing_Location_Should_Recalculate_Total()
        {
            _vm.SelectedTask = "Montáž";
            _vm.SelectedSpecification = "Kabel";
            _vm.SelectedMaterial = "CYKY 3x2.5";
            _vm.Quantity = 10;

            _vm.SelectedLocation = "Strop";
            var totalStrop = _vm.Total;

            _vm.SelectedLocation = "Podlaha";
            var totalPodlaha = _vm.Total;

            Assert.AreNotEqual(totalStrop, totalPodlaha);
        }

        // =========================================================
        // 5) Task → načte Specification
        // =========================================================
        [Test]
        public void Changing_Task_Should_Load_New_Specifications()
        {
            _vm.SelectedTask = "Montáž";

            Assert.AreEqual(2, _vm.AvailableSpecifications.Count);
            CollectionAssert.Contains(_vm.AvailableSpecifications, "Kabel");
            CollectionAssert.Contains(_vm.AvailableSpecifications, "Trubka");
        }

        // =========================================================
        // 6) Specification → načte Materials
        // =========================================================
        [Test]
        public void Changing_Specification_Should_Load_New_Materials()
        {
            _vm.SelectedTask = "Montáž";
            _vm.SelectedSpecification = "Kabel";

            Assert.AreEqual(1, _vm.AvailableMaterials.Count);
            Assert.AreEqual("CYKY 3x2.5", _vm.AvailableMaterials.First());
        }

        // =========================================================
        // 7) Material → načte Locations
        // =========================================================
        [Test]
        public void Changing_Material_Should_Load_New_Locations()
        {
            _vm.SelectedTask = "Montáž";
            _vm.SelectedSpecification = "Kabel";
            _vm.SelectedMaterial = "CYKY 3x2.5";

            Assert.AreEqual(2, _vm.AvailableLocations.Count);
            CollectionAssert.Contains(_vm.AvailableLocations, "Strop");
            CollectionAssert.Contains(_vm.AvailableLocations, "Podlaha");
        }
    }
}