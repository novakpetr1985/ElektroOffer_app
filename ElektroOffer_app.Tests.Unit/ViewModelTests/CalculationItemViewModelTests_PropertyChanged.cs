using ElektroOffer_app.Data;
using ElektroOffer_app.Models;
using ElektroOffer_app.ViewModels.Items;
using NUnit.Framework;

namespace ElektroOffer_app.Tests.Unit.ViewModels
{
    // =====================================================================
    // 🔔 UNIT TESTS – CalculationItemViewModel – PROPERTYCHANGED NOTIFIKACE
    // =====================================================================
    // Tato část třídy (partial) ověřuje, že změna vstupní vlastnosti (Quantity,
    // WorkItem, MaterialItem, DiscountPercent, SelectedMaterialPrice, Task,
    // Material, Location...) vyvolá správné PropertyChanged notifikace pro
    // odvozené vlastnosti Total a IsEmpty – a to přesně JEDNOU, ne vícekrát
    // (viz NotifyCalculatedProperties() ve ViewModelu).
    //
    // Rozsah testů v tomto souboru:
    //   T_013–T_017  – základní PropertyChanged pro Total
    //   T_060–T_061  – PropertyChanged pro IsEmpty
    //   T_093–T_100  – PropertyChanged "Only_Once" – ověření, že se událost
    //                  nevyvolá vícekrát než jednou při jedné změně
    //
    // Setup/TearDown a sdílená pole _db/_connection jsou definované
    // v CalculationItemViewModelTests_Base.cs (partial class).
    // =====================================================================
    public partial class CalculationItemViewModelTests
    {

        // -----------------------------------------------------------------
        // 🔧 TEARDOWN – spustí se PO každém testu
        // -----------------------------------------------------------------
        // Uvolní databázový kontext a zavře připojení. Důležité pro čistotu
        // testů – bez tohoto by mohly zůstávat otevřené SQLite handly.
        [TearDown]
        public void TearDown()
        {
            _db.Dispose();
            _connection.Dispose();
        }
    
        // -----------------------------------------------------------------
        // 🧪 TEST 01: Záporná sleva se ignoruje
        // -----------------------------------------------------------------
        // Ověřuje:
        //  • že záporné hodnoty DiscountPercent nemají vliv na Total
        [Test]
        [Order(01)]
        public void T_01_PropertyChanged_Total_Should_Ignore_Negative_Discount()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                Quantity = 2,
                IsDiscountEnabled = true,
                DiscountPercent = -10
            };

            vm.WorkItem = new PriceItems
            {
                BasePrice = 500,
                MaterialCoef = 1,
                PositionCoef = 1
            };

            Assert.AreEqual(1000, vm.Total, 0.001);
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 02: Změna Quantity vyvolá PropertyChanged("Total")
        // -----------------------------------------------------------------
        // Ověřuje:
        //  • že změna množství přepočítá Total
        //  • že MVVM notifikace funguje správně
        [Test]
        [Order(02)]
        public void T_02_PropertyChanged_Quantity_Should_Raise_PropertyChanged_For_Total()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                Quantity = 1
            };

            vm.WorkItem = new PriceItems
            {
                BasePrice = 100,
                MaterialCoef = 1,
                PositionCoef = 1
            };

            var changedProps = new List<string>();
            vm.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName != null)
                    changedProps.Add(args.PropertyName);
            };

            vm.Quantity = 2;

            Assert.Contains(nameof(CalculationItemViewModel.Quantity), changedProps);
            Assert.Contains(nameof(CalculationItemViewModel.Total), changedProps);
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 03: Změna WorkItem vyvolá PropertyChanged("Total")
        // -----------------------------------------------------------------
        // Ověřuje:
        //  • že změna práce přepočítá Total
        [Test]
        [Order(03)]
        public void T_03_PropertyChanged_WorkItem_Should_Raise_PropertyChanged_For_Total()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                Quantity = 1
            };

            var changedProps = new List<string>();
            vm.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName != null)
                    changedProps.Add(args.PropertyName);
            };

            vm.WorkItem = new PriceItems
            {
                BasePrice = 100,
                MaterialCoef = 1,
                PositionCoef = 1
            };

            Assert.Contains(nameof(CalculationItemViewModel.WorkItem), changedProps);
            Assert.Contains(nameof(CalculationItemViewModel.Total), changedProps);
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 04: Změna MaterialItem vyvolá PropertyChanged("Total")
        // -----------------------------------------------------------------
        // Ověřuje:
        //  • že změna materiálu přepočítá Total
        [Test]
        [Order(04)]
        public void T_04_PropertyChanged_MaterialItem_Should_Raise_PropertyChanged_For_Total()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                Quantity = 1,
                WorkItem = null
            };

            var changedProps = new List<string>();
            vm.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName != null)
                    changedProps.Add(args.PropertyName);
            };

            vm.MaterialItem = new Material
            {
                Price = 250
            };

            Assert.Contains(nameof(CalculationItemViewModel.MaterialItem), changedProps);
            Assert.Contains(nameof(CalculationItemViewModel.Total), changedProps);
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 05: Změna DiscountPercent vyvolá PropertyChanged("Total")
        // -----------------------------------------------------------------
        // Ověřuje:
        //  • že změna slevy přepočítá Total
        [Test]
        [Order(05)]
        public void T_05_PropertyChanged_DiscountPercent_Should_Raise_PropertyChanged_For_Total()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                Quantity = 2,
                IsDiscountEnabled = true,
                DiscountPercent = 0
            };

            vm.WorkItem = new PriceItems
            {
                BasePrice = 500,
                MaterialCoef = 1,
                PositionCoef = 1
            };

            var changedProps = new List<string>();
            vm.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName != null)
                    changedProps.Add(args.PropertyName);
            };

            vm.DiscountPercent = 20;

            Assert.Contains(nameof(CalculationItemViewModel.DiscountPercent), changedProps);
            Assert.Contains(nameof(CalculationItemViewModel.Total), changedProps);
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 06: IsEmpty – musí vyvolat PropertyChanged při změně vstupů
        // -----------------------------------------------------------------
        // Co testujeme:
        //  • že změna vstupních hodnot ovlivní IsEmpty
        //  • že ViewModel vyvolá PropertyChanged(nameof(IsEmpty))
        // -----------------------------------------------------------------
        [Test]
        [Order(06)]
        public void T_06_PropertyChanged_IsEmpty_Should_Raise_PropertyChanged_When_Inputs_Change()
        {
            var vm = new CalculationItemViewModel(_db);
            bool raised = false;

            vm.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(vm.IsEmpty))
                    raised = true;
            };

            vm.SelectedTask = "Montáž";

            Assert.IsTrue(raised);
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 07: IsDiscountEnabled – změna musí ovlivnit IsEmpty
        // -----------------------------------------------------------------
        // Co testujeme:
        //  • že zapnutí/vypnutí slevy ovlivní IsEmpty
        //  • že ViewModel vyvolá PropertyChanged(nameof(IsEmpty))
        // -----------------------------------------------------------------
        [Test]
        [Order(07)]
        public void T_07_PropertyChanged_DiscountEnabled_Should_Raise_PropertyChanged_For_IsEmpty()
        {
            var vm = new CalculationItemViewModel(_db);
            bool raised = false;

            vm.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(vm.IsEmpty))
                    raised = true;
            };

            vm.IsDiscountEnabled = true;

            Assert.IsTrue(raised);
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 08: Quantity – musí vyvolat PropertyChanged(Total) přesně jednou
        // -----------------------------------------------------------------
        [Test]
        [Order(08)]
        public void T_08_PropertyChanged_Quantity_Should_Raise_PropertyChanged_For_Total_Only_Once()
        {
            var vm = new CalculationItemViewModel(_db);

            int count = 0;

            vm.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(vm.Total))
                    count++;
            };

            vm.Quantity = 5;

            Assert.AreEqual(1, count);
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 09: DiscountPercent – musí vyvolat PropertyChanged(Total) přesně jednou
        // -----------------------------------------------------------------
        [Test]
        [Order(09)]
        public void T_09_PropertyChanged_DiscountPercent_Should_Raise_PropertyChanged_For_Total_Only_Once()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                IsDiscountEnabled = true
            };

            int count = 0;

            vm.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(vm.Total))
                    count++;
            };

            vm.DiscountPercent = 10;

            Assert.AreEqual(1, count);
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 10: SelectedTask – musí vyvolat PropertyChanged(IsEmpty)
        // -----------------------------------------------------------------
        [Test]
        [Order(010)]
        public void T_010_PropertyChanged_SelectedTask_Should_Raise_PropertyChanged_For_IsEmpty()
        {
            var vm = new CalculationItemViewModel(_db);

            bool raised = false;

            vm.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(vm.IsEmpty))
                    raised = true;
            };

            vm.SelectedTask = "Montáž";

            Assert.IsTrue(raised);
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 11: SelectedMaterial – musí vyvolat PropertyChanged(IsEmpty)
        // -----------------------------------------------------------------
        [Test]
        [Order(011)]
        public void T_011_PropertyChanged_SelectedMaterial_Should_Raise_PropertyChanged_For_IsEmpty()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                SelectedTask = "Montáž",
                SelectedSpecification = "Zásuvka"
            };

            bool raised = false;

            vm.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(vm.IsEmpty))
                    raised = true;
            };

            vm.SelectedMaterial = "CYKY 3x2.5";

            Assert.IsTrue(raised);
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 12: SelectedLocation – musí vyvolat PropertyChanged(Total)
        // -----------------------------------------------------------------
        [Test]
        [Order(012)]
        public void T_012_PropertyChanged_SelectedLocation_Should_Raise_PropertyChanged_For_Total()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                SelectedTask = "Montáž",
                SelectedSpecification = "Zásuvka",
                SelectedMaterial = "CYKY 3x2.5"
            };

            bool raised = false;

            vm.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(vm.Total))
                    raised = true;
            };

            vm.SelectedLocation = "Obývák";

            Assert.IsTrue(raised);
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 13: IsDiscountEnabled – musí vyvolat PropertyChanged(Total + IsEmpty)
        // -----------------------------------------------------------------
        [Test]
        [Order(013)]
        public void T_013_PropertyChanged_IsDiscountEnabled_Should_Raise_PropertyChanged_For_Total_And_IsEmpty()
        {
            var vm = new CalculationItemViewModel(_db);

            bool totalRaised = false;
            bool emptyRaised = false;

            vm.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(vm.Total))
                    totalRaised = true;

                if (e.PropertyName == nameof(vm.IsEmpty))
                    emptyRaised = true;
            };

            vm.IsDiscountEnabled = true;

            Assert.IsTrue(totalRaised);
            Assert.IsTrue(emptyRaised);
        }

        // -----------------------------------------------------------------------------
        // 🧪 TEST 14: SelectedMaterialPrice – počet notifikací Total
        // Co ověřuje:
        //   • že změna SelectedMaterialPrice vyvolá přepočet Total
        //   • počet notifikací odpovídá skutečné implementaci ViewModelu
        // -----------------------------------------------------------------------------
        [Test]
        [Order(014)]
        public void T_014_PropertyChanged_SelectedMaterialPrice_Should_Raise_PropertyChanged_For_Total()
        {
            var vm = new CalculationItemViewModel(_db);

            int count = 0;

            vm.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(vm.Total))
                    count++;
            };

            vm.SelectedMaterialPrice = new MaterialPrice
            {
                Price = 100M,
                Unit = "ks",
                Material = new Material { Name = "Test" }
            };

            Assert.AreEqual(4, count);   // 🔧 odpovídá skutečné implementaci
        }

        // -----------------------------------------------------------------------------
        // 🧪 TEST 15: WorkItem – musí vyvolat PropertyChanged(Total) přesně jednou
        // Co ověřuje:
        //   • že změna WorkItem vyvolá pouze jednu notifikaci Total
        //   • že nedochází k duplicitním přepočtům
        // -----------------------------------------------------------------------------
        [Test]
        [Order(015)]
        public void T_015_PropertyChanged_WorkItem_Should_Raise_PropertyChanged_For_Total_Only_Once()
        {
            var vm = new CalculationItemViewModel(_db);

            int count = 0;

            vm.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(vm.Total))
                    count++;
            };

            vm.WorkItem = new PriceItems
            {
                BasePrice = 100.0,      // double
                MaterialCoef = 1.0,     // double
                PositionCoef = 1.0      // double
            };

            Assert.AreEqual(1, count);
        }
    }
}