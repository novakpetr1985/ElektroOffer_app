using ElektroOffer_app.Data;
using ElektroOffer_app.Models;
using ElektroOffer_app.ViewModels.Items;
using NUnit.Framework;

namespace ElektroOffer_app.Tests.Unit.ViewModels
{
    // =====================================================================
    // 🧩 UNIT TESTS – CalculationItemViewModel – IsEmpty
    // =====================================================================
    // Tento soubor obsahuje testy ověřující vlastnost IsEmpty, která slouží
    // ProjectService k odfiltrování prázdných řádků před serializací.
    //
    // Testuje všechny kombinace vstupů, které mají/nemají vliv na to,
    // zda je řádek považován za "prázdný":
    //   • Task, Specification, Material, Location
    //   • Quantity
    //   • IsDiscountEnabled
    //
    // Rozsah testů v tomto souboru:
    //   T_01–T_03   – základní IsEmpty scénáře
    //   T_04        – kombinace hodnot
    //   T_05–T_07   – IsEmpty false při Quantity/Location/Task
    //   T_08–T_12   – IsEmpty false při Discount/Material/Location
    //
    // Sdílený databázový kontext (_db) a SetUp/TearDown jsou definované
    // v TestBase.cs, ze kterého tato třída dědí.
    // =====================================================================
    [TestFixture]
    public class CalculationItemViewModelTests_IsEmpty : TestBase
    {
        
        // -----------------------------------------------------------------
        // 🧪 TEST 01: IsEmpty vrací TRUE pro zcela prázdný řádek
        // -----------------------------------------------------------------
        // Co testujeme:
        //  • že prázdný řádek (bez práce, materiálu, lokace, specifikace)
        //    je správně označen jako IsEmpty == true
        //  • že Quantity == 0 a sleva vypnutá znamenají prázdný řádek
        //  • že ProjectService může bezpečně filtrovat placeholder řádky
        // -----------------------------------------------------------------
        [Test]
        [Order(01)]
        public void T_01_IsEmpty_Should_Return_True_For_Completely_Empty_Row()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                Quantity = 0,
                IsDiscountEnabled = false
            };

            Assert.IsTrue(vm.IsEmpty, "Prázdný řádek musí být označen jako IsEmpty == true.");
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 02: IsEmpty vrací FALSE, pokud je vybrána práce (WorkItem)
        // -----------------------------------------------------------------
        // Co testujeme:
        //  • že řádek s prací není prázdný
        //  • že IsEmpty správně reflektuje vyplněná data
        // -----------------------------------------------------------------
        [Test]
        [Order(02)]
        public void T_02_IsEmpty_Should_Return_False_When_WorkItem_Is_Selected()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                Quantity = 1,
                WorkItem = new PriceItems { BasePrice = 100.0 }
            };

            Assert.IsFalse(vm.IsEmpty, "Řádek s WorkItem nesmí být označen jako prázdný.");
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 03: IsEmpty vrací FALSE, pokud je vybrán materiál
        // -----------------------------------------------------------------
        // Co testujeme:
        //  • že řádek s materiálem není prázdný
        //  • že materiálová větev správně ovlivňuje IsEmpty
        // -----------------------------------------------------------------
        [Test]
        [Order(03)]
        public void T_03_IsEmpty_Should_Return_False_When_MaterialItem_Is_Selected()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                Quantity = 1,
                SelectedMaterialPrice = new MaterialPrice { Price = 50.0m }
            };

            Assert.IsFalse(vm.IsEmpty, "Řádek s materiálem nesmí být prázdný.");
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 04: IsEmpty při kombinaci hodnot
        // -----------------------------------------------------------------
        // Co testujeme:
        //  • že IsEmpty vrací TRUE, pokud řádek nemá žádná relevantní data
        //  • že sleva sama o sobě neznamená „vyplněný řádek“
        // -----------------------------------------------------------------
        [Test]
        [Order(04)]
        public void T_04_IsEmpty_Should_Handle_Combination_Of_Values()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                Quantity = 0,
                WorkItem = null,
                SelectedMaterialPrice = null,

                // Sleva je aktivní, ale řádek je jinak prázdný → IsEmpty musí být TRUE
                IsDiscountEnabled = true,
                DiscountPercent = 10
            };

            Assert.IsTrue(vm.IsEmpty);
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 05: IsEmpty – Quantity > 0 znamená ne-prázdný řádek
        // -----------------------------------------------------------------
        // Co testujeme:
        //  • že samotné množství stačí k tomu, aby řádek nebyl prázdný
        //  • že IsEmpty vrací FALSE, pokud Quantity > 0
        // -----------------------------------------------------------------
        [Test]
        [Order(05)]
        public void T_05_IsEmpty_Should_Return_False_When_Quantity_Is_Greater_Than_Zero()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                Quantity = 1
            };

            Assert.IsFalse(vm.IsEmpty);
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 06: IsEmpty – vybraná Location znamená ne-prázdný řádek
        // -----------------------------------------------------------------
        // Co testujeme:
        //  • že výběr Location je relevantní údaj
        //  • že IsEmpty vrací FALSE, pokud je Location vyplněná
        // -----------------------------------------------------------------
        [Test]
        [Order(06)]
        public void T_06_IsEmpty_Should_Return_False_When_Location_Is_Selected()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                SelectedLocation = "Byt 3+1"
            };

            Assert.IsFalse(vm.IsEmpty);
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 07: IsEmpty – vybraný Task znamená ne-prázdný řádek
        // -----------------------------------------------------------------
        // Co testujeme:
        //  • že Task je relevantní údaj
        //  • že IsEmpty vrací FALSE, pokud je Task vyplněný
        // -----------------------------------------------------------------
        [Test]
        [Order(07)]
        public void T_07_IsEmpty_Should_Return_False_When_Task_Is_Selected()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                SelectedTask = "Montáž zásuvky"
            };

            Assert.IsFalse(vm.IsEmpty);
        }
//
        // -----------------------------------------------------------------
        // 🧪 TEST 08: IsEmpty – true při úplně prázdném řádku
        // -----------------------------------------------------------------
        [Test]
        [Order(08)]
        public void T_08_IsEmpty_Should_Be_True_When_All_Fields_Are_Default()
        {
            var vm = new CalculationItemViewModel(_db);

            Assert.IsTrue(vm.IsEmpty);
        }
//
        // -----------------------------------------------------------------
        // 🧪 TEST 09: IsEmpty – false když je vybrán Task
        // -----------------------------------------------------------------
        [Test]
        [Order(09)]
        public void T_09_IsEmpty_IsEmpty_Should_Be_False_When_Task_Is_Selected()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                SelectedTask = "Montáž"
            };

            Assert.IsFalse(vm.IsEmpty);
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 010: IsEmpty – false když je Quantity > 0
        // -----------------------------------------------------------------
        [Test]
        [Order(010)]
        public void T_010_IsEmpty_Should_Be_False_When_Quantity_Is_Positive()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                Quantity = 5
            };

            Assert.IsFalse(vm.IsEmpty);
        }

        // -----------------------------------------------------------------------------
        // 🧪 TEST 011: IsEmpty – false při aktivní slevě
        // Co ověřuje:
        //   • že řádek není prázdný, pokud má nějakou hodnotu (Quantity)
        //   • sleva sama o sobě IsEmpty neovlivňuje (podle ViewModelu)
        // -----------------------------------------------------------------------------
        [Test]
        [Order(011)]
        public void T_011_IsEmpty_IsEmpty_Should_Be_False_When_Discount_Is_Enabled()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                Quantity = 1,              // 🔧 musí být > 0, jinak je řádek prázdný
                IsDiscountEnabled = true,
                DiscountPercent = 10
            };

            Assert.IsFalse(vm.IsEmpty);
        }


        // -----------------------------------------------------------------
        // 🧪 TEST 012: IsEmpty – false když je vybrán materiál
        // -----------------------------------------------------------------
        [Test]
        [Order(012)]
        public void T_012_IsEmpty_IsEmpty_Should_Be_False_When_Material_Is_Selected()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                SelectedMaterial = "CYKY 3x2.5"
            };

            Assert.IsFalse(vm.IsEmpty);
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 013: IsEmpty – false když je vybrána lokace
        // -----------------------------------------------------------------
        [Test]
        [Order(013)]
        public void T_013_IsEmpty_Should_Be_False_When_Location_Is_Selected()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                SelectedLocation = "Obývák"
            };

            Assert.IsFalse(vm.IsEmpty);
        }
    }
}