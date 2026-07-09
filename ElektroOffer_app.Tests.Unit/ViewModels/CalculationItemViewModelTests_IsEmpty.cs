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
    //   T_015–T_017 – základní IsEmpty scénáře
    //   T_018       – kombinace hodnot
    //   T_019–T_021 – IsEmpty false při Quantity/Location/Task
    //   T_022–T_027 – IsEmpty false při Discount/Material/Location
    //
    // Sdílený databázový kontext (_db) a SetUp/TearDown jsou definované
    // v TestBase.cs, ze kterého tato třída dědí.
    // =====================================================================
    [TestFixture]
    public class CalculationItemViewModelTests_IsEmpty : TestBase
    {
        
        // -----------------------------------------------------------------
        // 🧪 TEST 015: IsEmpty vrací TRUE pro zcela prázdný řádek
        // -----------------------------------------------------------------
        // Co testujeme:
        //  • že prázdný řádek (bez práce, materiálu, lokace, specifikace)
        //    je správně označen jako IsEmpty == true
        //  • že Quantity == 0 a sleva vypnutá znamenají prázdný řádek
        //  • že ProjectService může bezpečně filtrovat placeholder řádky
        // -----------------------------------------------------------------
        [Test]
        [Order(015)]
        public void T_015_IsEmpty_Should_Return_True_For_Completely_Empty_Row()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                Quantity = 0,
                IsDiscountEnabled = false
            };

            Assert.IsTrue(vm.IsEmpty, "Prázdný řádek musí být označen jako IsEmpty == true.");
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 016: IsEmpty vrací FALSE, pokud je vybrána práce (WorkItem)
        // -----------------------------------------------------------------
        // Co testujeme:
        //  • že řádek s prací není prázdný
        //  • že IsEmpty správně reflektuje vyplněná data
        // -----------------------------------------------------------------
        [Test]
        [Order(016)]
        public void T_016_IsEmpty_Should_Return_False_When_WorkItem_Is_Selected()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                Quantity = 1,
                WorkItem = new PriceItems { BasePrice = 100.0 }
            };

            Assert.IsFalse(vm.IsEmpty, "Řádek s WorkItem nesmí být označen jako prázdný.");
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 017: IsEmpty vrací FALSE, pokud je vybrán materiál
        // -----------------------------------------------------------------
        // Co testujeme:
        //  • že řádek s materiálem není prázdný
        //  • že materiálová větev správně ovlivňuje IsEmpty
        // -----------------------------------------------------------------
        [Test]
        [Order(017)]
        public void T_017_IsEmpty_Should_Return_False_When_MaterialItem_Is_Selected()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                Quantity = 1,
                SelectedMaterialPrice = new MaterialPrice { Price = 50.0m }
            };

            Assert.IsFalse(vm.IsEmpty, "Řádek s materiálem nesmí být prázdný.");
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 018: IsEmpty při kombinaci hodnot
        // -----------------------------------------------------------------
        // Co testujeme:
        //  • že IsEmpty vrací TRUE, pokud řádek nemá žádná relevantní data
        //  • že sleva sama o sobě neznamená „vyplněný řádek“
        // -----------------------------------------------------------------
        [Test]
        [Order(018)]
        public void T_018_IsEmpty_Should_Handle_Combination_Of_Values()
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
        // 🧪 TEST 019: IsEmpty – Quantity > 0 znamená ne-prázdný řádek
        // -----------------------------------------------------------------
        // Co testujeme:
        //  • že samotné množství stačí k tomu, aby řádek nebyl prázdný
        //  • že IsEmpty vrací FALSE, pokud Quantity > 0
        // -----------------------------------------------------------------
        [Test]
        [Order(019)]
        public void T_019_IsEmpty_Should_Return_False_When_Quantity_Is_Greater_Than_Zero()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                Quantity = 1
            };

            Assert.IsFalse(vm.IsEmpty);
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 020: IsEmpty – vybraná Location znamená ne-prázdný řádek
        // -----------------------------------------------------------------
        // Co testujeme:
        //  • že výběr Location je relevantní údaj
        //  • že IsEmpty vrací FALSE, pokud je Location vyplněná
        // -----------------------------------------------------------------
        [Test]
        [Order(020)]
        public void T_020_IsEmpty_Should_Return_False_When_Location_Is_Selected()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                SelectedLocation = "Byt 3+1"
            };

            Assert.IsFalse(vm.IsEmpty);
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 021: IsEmpty – vybraný Task znamená ne-prázdný řádek
        // -----------------------------------------------------------------
        // Co testujeme:
        //  • že Task je relevantní údaj
        //  • že IsEmpty vrací FALSE, pokud je Task vyplněný
        // -----------------------------------------------------------------
        [Test]
        [Order(021)]
        public void T_021_IsEmpty_Should_Return_False_When_Task_Is_Selected()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                SelectedTask = "Montáž zásuvky"
            };

            Assert.IsFalse(vm.IsEmpty);
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 022: IsEmpty – true při úplně prázdném řádku
        // -----------------------------------------------------------------
        // Co testujeme:
        //  • že řádek bez jakýchkoli vyplněných hodnot je považován za prázdný
        //  • že defaultní stav ViewModelu vede k IsEmpty == true
        //  • že ProjectService může bezpečně filtrovat placeholder řádky
        // -----------------------------------------------------------------
        [Test]
        [Order(022)]
        public void T_022_IsEmpty_Should_Be_True_When_All_Fields_Are_Default()
        {
            var vm = new CalculationItemViewModel(_db);

            Assert.IsTrue(vm.IsEmpty);
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 023: IsEmpty – false když je vybrán Task
        // -----------------------------------------------------------------
        // Co testujeme:
        //  • že vyplněný Task je relevantní údaj, který činí řádek ne-prázdným
        //  • že IsEmpty správně reaguje na vyplněnou položku Task
        // -----------------------------------------------------------------
        [Test]
        [Order(023)]
        public void T_023_IsEmpty_Should_Be_False_When_Task_Is_Selected()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                SelectedTask = "Montáž"
            };

            Assert.IsFalse(vm.IsEmpty);
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 024: IsEmpty – false když je Quantity > 0
        // -----------------------------------------------------------------
        // Co testujeme:
        //  • že samotné množství (Quantity) stačí k tomu, aby řádek nebyl prázdný
        //  • že IsEmpty vrací FALSE, pokud Quantity > 0
        // -----------------------------------------------------------------
        [Test]
        [Order(024)]
        public void T_024_IsEmpty_Should_Be_False_When_Quantity_Is_Positive()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                Quantity = 5
            };

            Assert.IsFalse(vm.IsEmpty);
        }

        // -----------------------------------------------------------------------------
        // 🧪 TEST 025: IsEmpty – false při aktivní slevě
        // -----------------------------------------------------------------------------
        // Co ověřujeme:
        //   • že řádek není prázdný, pokud má nějakou hodnotu (Quantity > 0)
        //   • že sleva sama o sobě IsEmpty neovlivňuje (IsDiscountEnabled neznamená vyplněný řádek)
        //   • že kombinace Quantity + sleva vede správně k IsEmpty == false
        // -----------------------------------------------------------------------------
        [Test]
        [Order(025)]
        public void T_025_IsEmpty_Should_Be_False_When_Discount_Is_Enabled()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                Quantity = 1,              // musí být > 0, jinak je řádek prázdný
                IsDiscountEnabled = true,
                DiscountPercent = 10
            };

            Assert.IsFalse(vm.IsEmpty);
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 026: IsEmpty – false když je vybrán materiál
        // -----------------------------------------------------------------
        // Co testujeme:
        //  • že vyplněná položka SelectedMaterial činí řádek ne-prázdným
        //  • že materiálová větev správně ovlivňuje IsEmpty
        // -----------------------------------------------------------------
        [Test]
        [Order(026)]
        public void T_026_IsEmpty_Should_Be_False_When_Material_Is_Selected()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                SelectedMaterial = "CYKY 3x2.5"
            };

            Assert.IsFalse(vm.IsEmpty);
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 027: IsEmpty – false když je vybrána lokace
        // -----------------------------------------------------------------
        // Co testujeme:
        //  • že vyplněná Location je relevantní údaj
        //  • že IsEmpty vrací FALSE, pokud je Location vyplněná
        // -----------------------------------------------------------------
        [Test]
        [Order(027)]
        public void T_027_IsEmpty_Should_Be_False_When_Location_Is_Selected()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                SelectedLocation = "Obývák"
            };

            Assert.IsFalse(vm.IsEmpty);
        }
    }
}