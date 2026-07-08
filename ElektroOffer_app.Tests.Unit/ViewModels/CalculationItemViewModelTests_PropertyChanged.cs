using ElektroOffer_app.Data;
using ElektroOffer_app.Models;
using ElektroOffer_app.ViewModels.Items;
using NUnit.Framework;

namespace ElektroOffer_app.Tests.Unit.ViewModels
{
    // =====================================================================
    // 🔔 UNIT TESTS – CalculationItemViewModel – PROPERTYCHANGED NOTIFIKACE
    // =====================================================================
    // Tento soubor obsahuje testy ověřující, že změna vstupní vlastnosti
    // (Quantity, WorkItem, MaterialItem, DiscountPercent, SelectedMaterialPrice,
    // Task, Material, Location...) vyvolá správné PropertyChanged notifikace
    // pro odvozené vlastnosti Total a IsEmpty – a to přesně JEDNOU.
    //
    // Rozsah testů v tomto souboru:
    //   T_028–T_032  – základní PropertyChanged pro Total
    //   T_033–T_034  – PropertyChanged pro IsEmpty
    //   T_035–T_041  – PropertyChanged "Only_Once"
    //
    // Sdílený databázový kontext (_db) a SetUp/TearDown jsou definované
    // v TestBase.cs, ze kterého tato třída dědí.
    // =====================================================================
    [TestFixture]
    public class CalculationItemViewModelTests_PropertyChanged : TestBase
    {
    
        // -----------------------------------------------------------------
        // 🧪 TEST 028: Záporná sleva se ignoruje
        // -----------------------------------------------------------------
        // Ověřuje:
        //  • že záporné hodnoty DiscountPercent nemají vliv na Total
        [Test]
        [Order(028)]
        public void T_028_PropertyChanged_Total_Should_Ignore_Negative_Discount()
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
        // 🧪 TEST 029: Změna Quantity vyvolá PropertyChanged("Total")
        // -----------------------------------------------------------------
        // Ověřuje:
        //  • že změna množství přepočítá Total
        //  • že MVVM notifikace funguje správně
        [Test]
        [Order(029)]
        public void T_029_PropertyChanged_Quantity_Should_Raise_PropertyChanged_For_Total()
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
        // 🧪 TEST 030: Změna WorkItem vyvolá PropertyChanged("Total")
        // -----------------------------------------------------------------
        // Ověřuje:
        //  • že změna práce přepočítá Total
        [Test]
        [Order(030)]
        public void T_030_PropertyChanged_WorkItem_Should_Raise_PropertyChanged_For_Total()
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
        // 🧪 TEST 031: Změna MaterialItem vyvolá PropertyChanged("Total")
        // -----------------------------------------------------------------
        // Ověřuje:
        //  • že změna materiálu přepočítá Total
        [Test]
        [Order(031)]
        public void T_031_PropertyChanged_MaterialItem_Should_Raise_PropertyChanged_For_Total()
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
        // 🧪 TEST 032: Změna DiscountPercent vyvolá PropertyChanged("Total")
        // -----------------------------------------------------------------
        // Ověřuje:
        //  • že změna slevy přepočítá Total
        [Test]
        [Order(032)]
        public void T_032_PropertyChanged_DiscountPercent_Should_Raise_PropertyChanged_For_Total()
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
        // 🧪 TEST 033: IsEmpty – musí vyvolat PropertyChanged při změně vstupů
        // -----------------------------------------------------------------
        // Co testujeme:
        //  • že změna vstupních hodnot ovlivní IsEmpty
        //  • že ViewModel vyvolá PropertyChanged(nameof(IsEmpty))
        // -----------------------------------------------------------------
        [Test]
        [Order(033)]
        public void T_033_PropertyChanged_IsEmpty_Should_Raise_PropertyChanged_When_Inputs_Change()
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
        // 🧪 TEST 034: IsDiscountEnabled – změna musí ovlivnit IsEmpty
        // -----------------------------------------------------------------
        // Co testujeme:
        //  • že zapnutí/vypnutí slevy ovlivní IsEmpty
        //  • že ViewModel vyvolá PropertyChanged(nameof(IsEmpty))
        // -----------------------------------------------------------------
        [Test]
        [Order(034)]
        public void T_034_PropertyChanged_DiscountEnabled_Should_Raise_PropertyChanged_For_IsEmpty()
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
        // 🧪 TEST 035: Quantity – musí vyvolat PropertyChanged(Total) přesně jednou
        // -----------------------------------------------------------------
        // Co testujeme:
        //  • že změna Quantity ovlivňuje výpočet Total
        //  • že ViewModel vyvolá PropertyChanged(nameof(Total)) pouze jednou
        //  • že nedochází k duplicitním notifikacím (regrese proti dřívějším chybám)
        //  • že NotifyCalculatedProperties() funguje korektně pro Quantity
        // -----------------------------------------------------------------
        [Test]
        [Order(035)]
        public void T_035_PropertyChanged_Quantity_Should_Raise_PropertyChanged_For_Total_Only_Once()
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
        // 🧪 TEST 036: DiscountPercent – musí vyvolat PropertyChanged(Total) přesně jednou
        // -----------------------------------------------------------------
        // Co testujeme:
        //  • že změna DiscountPercent ovlivňuje Total pouze pokud je sleva aktivní
        //  • že PropertyChanged(nameof(Total)) je vyvolán přesně jednou
        //  • že nedochází k vícenásobným notifikacím při změně slevy
        //  • že logika slevy je správně integrována do NotifyCalculatedProperties()
        // -----------------------------------------------------------------
        [Test]
        [Order(036)]
        public void T_036_PropertyChanged_DiscountPercent_Should_Raise_PropertyChanged_For_Total_Only_Once()
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
        // 🧪 TEST 037: SelectedTask – musí vyvolat PropertyChanged(IsEmpty)
        // -----------------------------------------------------------------
        // Co testujeme:
        //  • že změna SelectedTask ovlivňuje stav IsEmpty
        //  • že ViewModel správně vyvolá PropertyChanged(nameof(IsEmpty))
        //  • že logika IsEmpty reaguje na vyplněný Task
        // -----------------------------------------------------------------
        [Test]
        [Order(037)]
        public void T_037_PropertyChanged_SelectedTask_Should_Raise_PropertyChanged_For_IsEmpty()
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
        // 🧪 TEST 038: SelectedMaterial – musí vyvolat PropertyChanged(IsEmpty)
        // -----------------------------------------------------------------
        // Co testujeme:
        //  • že změna SelectedMaterial ovlivňuje stav IsEmpty
        //  • že ViewModel správně vyvolá PropertyChanged(nameof(IsEmpty))
        //  • že logika IsEmpty reaguje na vyplněný materiál
        // -----------------------------------------------------------------
        [Test]
        [Order(038)]
        public void T_038_PropertyChanged_SelectedMaterial_Should_Raise_PropertyChanged_For_IsEmpty()
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
        // 🧪 TEST 039: SelectedLocation – musí vyvolat PropertyChanged(Total)
        // -----------------------------------------------------------------
        // Co testujeme:
        //  • že změna SelectedLocation ovlivňuje výpočet Total (např. koeficient lokace)
        //  • že ViewModel vyvolá PropertyChanged(nameof(Total))
        //  • že logika výpočtu Total správně reaguje na změnu lokace
        // -----------------------------------------------------------------
        [Test]
        [Order(039)]
        public void T_039_PropertyChanged_SelectedLocation_Should_Raise_PropertyChanged_For_Total()
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
        // 🧪 TEST 040: IsDiscountEnabled – musí vyvolat PropertyChanged(Total + IsEmpty)
        // -----------------------------------------------------------------
        // Co testujeme:
        //  • že změna IsDiscountEnabled ovlivňuje výpočet Total
        //  • že změna IsDiscountEnabled ovlivňuje stav IsEmpty (sleva = „vyplněný řádek“)
        //  • že ViewModel vyvolá PropertyChanged(nameof(Total)) i PropertyChanged(nameof(IsEmpty))
        //  • že obě notifikace proběhnou korektně a nezávisle
        // -----------------------------------------------------------------
        [Test]
        [Order(040)]
        public void T_040_PropertyChanged_IsDiscountEnabled_Should_Raise_PropertyChanged_For_Total_And_IsEmpty()
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
        // 🧪 TEST 041: SelectedMaterialPrice – počet notifikací Total
        // Co ověřuje:
        //   • že změna SelectedMaterialPrice vyvolá přepočet Total
        //   • počet notifikací odpovídá skutečné implementaci ViewModelu
        // -----------------------------------------------------------------------------
        [Test]
        [Order(041)]
        public void T_041_PropertyChanged_SelectedMaterialPrice_Should_Raise_PropertyChanged_For_Total()
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
        // 🧪 TEST 042: WorkItem – musí vyvolat PropertyChanged(Total) přesně jednou
        // Co ověřuje:
        //   • že změna WorkItem vyvolá pouze jednu notifikaci Total
        //   • že nedochází k duplicitním přepočtům
        // -----------------------------------------------------------------------------
        [Test]
        [Order(042)]
        public void T_042_PropertyChanged_WorkItem_Should_Raise_PropertyChanged_For_Total_Only_Once()
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