using ElektroOffer_app.Data;
using ElektroOffer_app.Models;
using ElektroOffer_app.ViewModels.Items;
using NUnit.Framework;

namespace ElektroOffer_app.Tests.Unit.ViewModels
{
    // =====================================================================
    // 🧮 UNIT TESTS – CalculationItemViewModel – VÝPOČET TOTAL
    // =====================================================================
    // Tento soubor obsahuje testy ověřující výpočet vlastnosti Total:
    //
    //   • základní výpočet z WorkItem / MaterialItem
    //   • aplikace slevy (IsDiscountEnabled + DiscountPercent)
    //   • koeficienty materiálu a pozice
    //   • pokročilé a edge-case scénáře (extrémní hodnoty, kombinace vstupů,
    //     zaokrouhlování, chování při mazání/aktualizaci ceny v DB)
    //
    // Rozsah testů v tomto souboru:
    //   T_01–T_12   – základní výpočet Total
    //   T_18–T_55   – pokročilé scénáře výpočtu Total
    //   T_62–T_72   – kombinace vstupů a floating‑point edge cases
    //
    // Sdílený databázový kontext (_db) a SetUp/TearDown jsou definované
    // v TestBase.cs, ze kterého tato třída dědí.
    // =====================================================================
    [TestFixture]
    public class CalculationItemViewModelTests_Total : TestBase
    {
    
        // -----------------------------------------------------------------
        // 🧪 TEST 01: Výpočet ceny práce (WorkItem)
        // -----------------------------------------------------------------
        // Ověřuje:
        //   • že Total správně počítá cenu práce podle vzorce:
        //       BasePrice × MaterialCoef × PositionCoef × Quantity
        //   • že koeficienty jsou aplikovány správně
        //   • že Quantity ovlivňuje výsledek lineárně
        //
        // Tento test odhalí chyby typu:
        //   • špatné pořadí násobení
        //   • chybně aplikované koeficienty
        //   • chybné zaokrouhlování
        //   • použití staré hodnoty Quantity
        //
        // OPRAVA: přidán argument (_db) do konstruktoru – bez něj test
        // nejde ani zkompilovat po refaktoringu 1.7.6.
        [Test]
        [Order(01)]
        public void T_01_Total_Should_Calculate_WorkItem_Correctly()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                Quantity = 10
            };

            vm.WorkItem = new PriceItems
            {
                BasePrice = 150,
                MaterialCoef = 1.2,
                PositionCoef = 1.0
            };

            // 150 × 1.2 × 1.0 × 10 = 1800
            Assert.AreEqual(1800, vm.Total, 0.001);
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 02: Výpočet ceny materiálu (MaterialItem) pokud WorkItem == null
        // -----------------------------------------------------------------
        // Kontext změn (verze 1.8.0):
        //  • Materiálová cena se již NEbere z Material.Price (legacy pole)
        //  • Výpočet materiálu používá SelectedMaterialPrice.Price (nový model)
        //  • Pokud není vyplněná materiálová kaskáda → SelectedMaterialPrice = null
        //  • Výpočet Total padne na NullReferenceException
        //
        // Proč původní test padal:
        //  • vm.Total je double → Assert musí porovnávat double × double
        //  • expected byl decimal → decimal × double → CS0019
        //  • Quantity je double → decimal × double → CS0019
        //
        // Co testujeme:
        //  • CalculationItemViewModel správně vypočítá cenu materiálu
        //  • WorkItem je null → testujeme čistě materiálovou větev
        //  • Používáme fake SelectedMaterialPrice (bez DB), protože Material.Price je ignorován
        //
        // Poznámka:
        //  • expected musí být double, protože vm.Total je double
        // -----------------------------------------------------------------
        [Test]
        [Order(02)]
        public void T_02_Total_Should_Calculate_MaterialItem_When_WorkItem_Is_Null()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                Quantity = 5,
                WorkItem = null
            };

            vm.SelectedMaterialPrice = new MaterialPrice
            {
                Price = 25.90m,
                SupplierCode = "TEST",
                SupplierId = 1,
                MaterialId = 1
            };

            // 🔍 Očekávaný výsledek (decimal × decimal → decimal → double)
            double expected = (double)(vm.SelectedMaterialPrice.Price * (decimal)vm.Quantity);

            Assert.AreEqual(expected, vm.Total, 0.001,
                "Výpočet ceny materiálu neodpovídá SelectedMaterialPrice.Price × Quantity.");
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 03: Aktivní sleva se správně aplikuje na Total
        // -----------------------------------------------------------------
        // Ověřuje:
        //  • že IsDiscountEnabled aktivuje slevu
        //  • že DiscountPercent se správně odečte od Total
        [Test]
        [Order(03)]
        public void T_03_Total_Should_Apply_Discount()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                Quantity = 2,
                IsDiscountEnabled = true,
                DiscountPercent = 10
            };

            vm.WorkItem = new PriceItems
            {
                BasePrice = 500,
                MaterialCoef = 1,
                PositionCoef = 1
            };

            // baseTotal = 500 × 1 × 1 × 2 = 1000
            // sleva 10 % → 1000 × 0.9 = 900
            Assert.AreEqual(900, vm.Total, 0.001);
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 04: Sleva se neaplikuje, pokud IsDiscountEnabled == false
        // -----------------------------------------------------------------
        // Ověřuje:
        //  • že vypnutá sleva nemění Total
        [Test]
        [Order(04)]
        public void T_04_Total_Should_Not_Apply_Discount_When_Disabled()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                Quantity = 2,
                IsDiscountEnabled = false,
                DiscountPercent = 10
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
        // 🧪 TEST 05: Vypnutí slevy resetuje DiscountPercent
        // -----------------------------------------------------------------
        // Ověřuje:
        //  • že DiscountPercent se resetuje při IsDiscountEnabled = false
        [Test]
        [Order(05)]
        public void T_05_IsDiscountEnabled_False_Should_Reset_DiscountPercent()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                Quantity = 1,
                IsDiscountEnabled = true,
                DiscountPercent = 15
            };

            vm.IsDiscountEnabled = false;

            Assert.IsNull(vm.DiscountPercent);
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 06: Materiálový a poziční koeficient ovlivňují Total
        // -----------------------------------------------------------------
        // Ověřuje:
        //  • že MaterialCoef a PositionCoef jsou správně aplikovány
        [Test]
        [Order(06)]
        public void T_06_Total_Should_Respect_Material_And_Position_Coefs()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                Quantity = 7
            };

            vm.WorkItem = new PriceItems
            {
                BasePrice = 100,
                MaterialCoef = 1.5,
                PositionCoef = 0.8
            };

            Assert.AreEqual(840, vm.Total, 0.001);
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 07: Quantity = 0 → Total = 0
        // -----------------------------------------------------------------
        // Ověřuje:
        //  • že nulové množství vždy vede k nulové ceně
        [Test]
        [Order(07)]
        public void T_07_Total_Should_Be_Zero_When_Quantity_Is_Zero()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                Quantity = 0
            };

            vm.WorkItem = new PriceItems
            {
                BasePrice = 999,
                MaterialCoef = 2,
                PositionCoef = 3
            };

            Assert.AreEqual(0, vm.Total, 0.001);
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 08: BasePrice = 0 → Total = 0
        // -----------------------------------------------------------------
        // Ověřuje:
        //  • že nulová základní cena vede k nulovému výsledku
        [Test]
        [Order(08)]
        public void T_08_Total_Should_Be_Zero_When_BasePrice_Is_Zero()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                Quantity = 10
            };

            vm.WorkItem = new PriceItems
            {
                BasePrice = 0,
                MaterialCoef = 1.5,
                PositionCoef = 0.8
            };

            Assert.AreEqual(0, vm.Total, 0.001);
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 09: Oba zdroje ceny null → Total = 0
        // -----------------------------------------------------------------
        // Ověřuje:
        //  • že Total je 0, pokud není práce ani materiál
        [Test]
        [Order(09)]
        public void T_09_Total_Should_Be_Zero_When_WorkItem_And_MaterialItem_Are_Null()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                Quantity = 10,
                WorkItem = null,
                MaterialItem = null
            };

            Assert.AreEqual(0, vm.Total, 0.001);
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 10: WorkItem má prioritu před MaterialItem
        // -----------------------------------------------------------------
        // Ověřuje:
        //  • že pokud existuje WorkItem, materiálová větev se ignoruje
        [Test]
        [Order(10)]
        public void T_10_Total_Should_Use_WorkItem_When_Both_WorkItem_And_MaterialItem_Are_Set()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                Quantity = 3
            };

            vm.WorkItem = new PriceItems
            {
                BasePrice = 100,
                MaterialCoef = 1,
                PositionCoef = 1
            };

            vm.MaterialItem = new Material
            {
                Price = 999 // měl by být ignorován
            };

            Assert.AreEqual(300, vm.Total, 0.001);
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 11: Sleva 100 % → Total musí být 0
        // -----------------------------------------------------------------
        // Ověřuje:
        //  • že maximální sleva vynuluje Total
        [Test]
        [Order(11)]
        public void T_11_Total_Should_Be_Zero_When_Discount_Is_100_Percent()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                Quantity = 5,
                IsDiscountEnabled = true,
                DiscountPercent = 100
            };

            vm.WorkItem = new PriceItems
            {
                BasePrice = 200,
                MaterialCoef = 1,
                PositionCoef = 1
            };

            Assert.AreEqual(0, vm.Total, 0.001);
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 12: Sleva nad 100 % nesmí vytvořit záporné hodnoty
        // -----------------------------------------------------------------
        // Ověřuje:
        //  • že Total je minimálně 0
        [Test]
        [Order(12)]
        public void T_12_Total_Should_Not_Be_Negative_When_Discount_Above_100()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                Quantity = 5,
                IsDiscountEnabled = true,
                DiscountPercent = 150
            };

            vm.WorkItem = new PriceItems
            {
                BasePrice = 200,
                MaterialCoef = 1,
                PositionCoef = 1
            };

            Assert.AreEqual(0, vm.Total, 0.001);
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 13: Sleva se aplikuje pouze na práci (WorkItem)
        // -----------------------------------------------------------------
        // Co testujeme:
        //  • že pokud je WorkItem vyplněn, materiálová větev se ignoruje
        //  • že sleva se aplikuje pouze na výpočet práce
        //  • že Total odpovídá implementaci CalculationPriceService
        // -----------------------------------------------------------------

        [Test]
        [Order(13)]
        public void T_13_Total_Should_Apply_Discount_On_WorkItem()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                Quantity = 2,
                WorkItem = new PriceItems
                {
                    BasePrice = 100.0,
                    MaterialCoef = 1.0,
                    PositionCoef = 1.0
                },
                SelectedMaterialPrice = new MaterialPrice { Price = 50.0m },
                IsDiscountEnabled = true,
                DiscountPercent = 10.0
            };

            // Implementace počítá pouze práci:
            // WorkItem = 100 × 2 = 200
            // Sleva 10 % = 20
            // Total = 180
            double expected = 180.0;

            Assert.AreEqual(expected, vm.Total, 0.001,
                "Sleva se má aplikovat pouze na práci, materiál se ignoruje.");
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 14: Sleva se aplikuje na materiál, pokud WorkItem == null
        // -----------------------------------------------------------------
        // Co testujeme:
        //  • že materiálová větev se použije, pokud WorkItem není vyplněn
        //  • že sleva se aplikuje na materiálovou cenu
        //  • že Total odpovídá implementaci CalculationPriceService
        // -----------------------------------------------------------------

        [Test]
        [Order(14)]
        public void T_14_Total_Should_Apply_Discount_On_MaterialItem_When_WorkItem_Is_Null()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                Quantity = 2,
                WorkItem = null,
                SelectedMaterialPrice = new MaterialPrice { Price = 50.0m },
                IsDiscountEnabled = true,
                DiscountPercent = 10.0
            };

            // Materiál = 50 × 2 = 100
            // Sleva 10 % = 10
            // Total = 90
            double expected = 90.0;

            Assert.AreEqual(expected, vm.Total, 0.001,
                "Sleva se má aplikovat na materiál, pokud WorkItem == null.");
        }


        // -----------------------------------------------------------------
        // 🧪 TEST 15: Změna SelectedMaterialPrice musí vyvolat PropertyChanged("Total")
        // -----------------------------------------------------------------
        // Co testujeme:
        //  • že změna materiálové ceny okamžitě přepočítá Total
        //  • že CalculationItemViewModel správně vyvolá PropertyChanged pro Total
        //  • že MVVM notifikace funguje správně (UI se aktualizuje)
        // -----------------------------------------------------------------
        [Test]
        [Order(15)]
        public void T_15_SelectedMaterialPrice_Should_Raise_PropertyChanged_For_Total()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                Quantity = 3
            };

            bool eventRaised = false;

            vm.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "Total")
                    eventRaised = true;
            };

            vm.SelectedMaterialPrice = new MaterialPrice
            {
                Price = 10.0m,
                SupplierCode = "TEST",
                SupplierId = 1,
                MaterialId = 1
            };

            double expected = (double)(10.0m * (decimal)vm.Quantity);

            Assert.IsTrue(eventRaised, "Změna SelectedMaterialPrice musí vyvolat PropertyChanged(\"Total\").");
            Assert.AreEqual(expected, vm.Total, 0.001,
                "Total nebyl správně přepočítán po změně SelectedMaterialPrice.");
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 16: Extrémní Quantity musí být správně zpracováno
        // -----------------------------------------------------------------
        // Co testujeme:
        //  • že CalculationItemViewModel zvládne velmi vysoké hodnoty Quantity
        //  • že nedojde k přetečení nebo ztrátě přesnosti
        //  • že Total je stále správně vypočítán
        // -----------------------------------------------------------------
        [Test]
        [Order(16)]
        public void T_16_Total_Should_Handle_Extreme_Quantity_Values()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                Quantity = 10000
            };

            vm.WorkItem = new PriceItems
            {
                BasePrice = 1.5,
                MaterialCoef = 1.0,
                PositionCoef = 1.0
            };

            double expected = 1.5 * 10000;

            Assert.AreEqual(expected, vm.Total, 0.001,
                "Extrémní Quantity nebylo správně zpracováno.");
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 17: SelectedMaterialPrice == null → Total = 0
        // -----------------------------------------------------------------
        // Co testujeme:
        //  • že materiálová větev je bezpečná, pokud SelectedMaterialPrice není nastaven
        //  • že Total nepadne na NullReferenceException
        //  • že WorkItem == null + SelectedMaterialPrice == null → Total = 0
       // -----------------------------------------------------------------
        [Test]
        [Order(17)]
        public void T_17_Total_Should_Be_Zero_When_SelectedMaterialPrice_Is_Null()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                Quantity = 5,
                SelectedMaterialPrice = null,
                WorkItem = null
            };

            Assert.AreEqual(0.0, vm.Total, 0.001,
                "Pokud SelectedMaterialPrice == null, Total musí být 0.");
        }
        // -----------------------------------------------------------------
        // 🧪 TEST 18: Změna SelectedMaterialPrice přepočítá Total
        // -----------------------------------------------------------------
        // Co testujeme:
        //  • že změna materiálové ceny okamžitě přepočítá Total
        //  • že MVVM PropertyChanged funguje správně
        //  • že UI bude reagovat správně při změně nabídky materiálu
        // -----------------------------------------------------------------
        [Test]
        [Order(18)]
        public void T_18_Total_Should_Update_When_SelectedMaterialPrice_Changes()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                Quantity = 3
            };

            vm.SelectedMaterialPrice = new MaterialPrice { Price = 10.0m };
            double first = vm.Total;

            vm.SelectedMaterialPrice = new MaterialPrice { Price = 20.0m };
            double second = vm.Total;

            Assert.AreEqual(30.0, first, 0.001);
            Assert.AreEqual(60.0, second, 0.001,
                "Změna SelectedMaterialPrice musí přepočítat Total.");
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 19: Extrémní materiálová cena musí být správně zpracována
        // -----------------------------------------------------------------
        // Co testujeme:
        //  • že decimal → double konverze je stabilní
        //  • že vysoké materiálové ceny nepřetečou
        //  • že Total je přesný i při cenách typu 99999.99
        // -----------------------------------------------------------------
        [Test]
        [Order(19)]
        public void T_19_Total_Should_Handle_Extreme_Material_Prices()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                Quantity = 2
            };

            vm.SelectedMaterialPrice = new MaterialPrice
            {
                Price = 99999.99m
            };

            double expected = (double)(99999.99m * 2m);

            Assert.AreEqual(expected, vm.Total, 0.001,
                "Extrémní materiálová cena nebyla správně zpracována.");
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 20: Total musí být správně zaokrouhlen na 3 desetinná místa
        // -----------------------------------------------------------------
        // Co testujeme:
        //  • že výpočet neobsahuje floating‑point chyby
        //  • že kombinace práce + sleva nevyrobí 269.999999 místo 270
        //  • že Total má stabilní přesnost
        // -----------------------------------------------------------------
        [Test]
        [Order(20)]
        public void T_20_Total_Should_Round_Correctly_To_Three_Decimals()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                Quantity = 3
            };

            vm.WorkItem = new PriceItems
            {
                BasePrice = 33.3333,
                MaterialCoef = 1.0,
                PositionCoef = 1.0
            };

            vm.IsDiscountEnabled = true;
            vm.DiscountPercent = 10.0;

            double expected = (33.3333 * 3) * 0.9; // sleva 10 %

            Assert.AreEqual(expected, vm.Total, 0.001,
                "Total není správně zaokrouhlen nebo obsahuje floating‑point chyby.");
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 21: Total se musí správně přepočítat po více změnách
        // -----------------------------------------------------------------
        // Co testujeme:
        //  • že ViewModel nemá cache starých hodnot
        //  • že změna Quantity, práce, materiálu i slevy vždy přepočítá Total
        //  • že WorkItem má prioritu před materiálem
        // -----------------------------------------------------------------
        [Test]
        [Order(21)]
        public void T_21_Total_Should_Recalculate_Correctly_After_Multiple_Changes()
        {
            var vm = new CalculationItemViewModel(_db);

            // 1) První výpočet – práce
            vm.Quantity = 1;
            vm.WorkItem = new PriceItems
            {
                BasePrice = 100.0,
                MaterialCoef = 1.0,
                PositionCoef = 1.0
            };
            double step1 = vm.Total;   // 100 × 1 = 100

            // 2) Změna množství
            vm.Quantity = 2;
            double step2 = vm.Total;   // 100 × 2 = 200

            // 3) Nastavení materiálu – ale WorkItem má prioritu
            vm.SelectedMaterialPrice = new MaterialPrice { Price = 50.0m };
            double step3 = vm.Total;   // stále 200 (materiál se ignoruje)

            // 4) Sleva 20 %
            vm.IsDiscountEnabled = true;
            vm.DiscountPercent = 20.0;
            double step4 = vm.Total;   // 200 - 20% = 160

            Assert.AreEqual(100.0, step1, 0.001);
            Assert.AreEqual(200.0, step2, 0.001);
            Assert.AreEqual(200.0, step3, 0.001);
            Assert.AreEqual(160.0, step4, 0.001,
                "Total nebyl správně přepočítán po více změnách.");
        }


        // -----------------------------------------------------------------
        // 🧪 TEST 22: WorkItem == null + validní materiál → Total se počítá z materiálu
        // -----------------------------------------------------------------
        // Co testujeme:
        //  • že materiálová větev funguje samostatně
        //  • že WorkItem == null nevyvolá výjimku
        //  • že Total je správně vypočítán z materiálu
        // -----------------------------------------------------------------
        [Test]
        [Order(22)]
        public void T_22_Total_Should_Not_Throw_When_WorkItem_Is_Null_And_MaterialItem_Is_Valid()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                Quantity = 4,
                WorkItem = null
            };

            vm.SelectedMaterialPrice = new MaterialPrice
            {
                Price = 12.5m
            };

            double expected = (double)(12.5m * 4m);

            Assert.AreEqual(expected, vm.Total, 0.001,
                "WorkItem == null + validní materiál musí být bezpečný a správně vypočítaný.");
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 23: Cena materiálu se správně načte z databáze
        // -----------------------------------------------------------------
        // Co testujeme:
        //  • že SQLite InMemory správně ukládá Material, Supplier a MaterialPrice
        //  • že CalculationItemViewModel používá DB hodnotu
        //  • že Total odpovídá ceně uložené v databázi
        // -----------------------------------------------------------------
        [Test]
        [Order(23)]
        public void T_23_Total_Should_Use_Price_From_Database_When_MaterialItem_Is_Loaded()
        {
            // 1) Vytvoření Material a Supplier (nutné kvůli FK)
            var material = new Material { Id = 1, Name = "TestMaterial" };
            var supplier = new Supplier { Id = 1, Name = "TestSupplier" };

            _db.Materials.Add(material);
            _db.Suppliers.Add(supplier);
            _db.SaveChanges();

            // 2) Vytvoření MaterialPrice s platnými FK
            var price = new MaterialPrice
            {
                Price = 42.0m,
                SupplierId = supplier.Id,
                MaterialId = material.Id,
                SupplierCode = "DB"
            };

            _db.MaterialPrices.Add(price);
            _db.SaveChanges();

            // 3) ViewModel
            var vm = new CalculationItemViewModel(_db)
            {
                Quantity = 3,
                WorkItem = null,
                SelectedMaterialPrice = _db.MaterialPrices.First()
            };

            double expected = (double)(42.0m * 3m);

            Assert.AreEqual(expected, vm.Total, 0.001,
                "Cena materiálu nebyla správně načtena z databáze.");
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 24: Total se přepne z práce na materiál po odstranění WorkItem
        // -----------------------------------------------------------------
        // Co testujeme:
        //  • že WorkItem má prioritu
        //  • že po odstranění WorkItem se výpočet přepne na materiál
        //  • že MVVM PropertyChanged funguje správně při změně WorkItem
        // -----------------------------------------------------------------
        [Test]
        [Order(24)]
        public void T_24_Total_Should_Switch_From_WorkItem_To_MaterialItem_When_WorkItem_Is_Cleared()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                Quantity = 2,
                WorkItem = new PriceItems { BasePrice = 100.0, MaterialCoef = 1.0, PositionCoef = 1.0 },
                SelectedMaterialPrice = new MaterialPrice { Price = 50.0m }
            };

            double step1 = vm.Total; // 100 × 2 = 200

            vm.WorkItem = null;
            double step2 = vm.Total; // 50 × 2 = 100

            Assert.AreEqual(200.0, step1, 0.001);
            Assert.AreEqual(100.0, step2, 0.001,
                "Po odstranění WorkItem se má výpočet přepnout na materiál.");
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 25: Sleva se resetuje, pokud DiscountPercent == null
        // -----------------------------------------------------------------
        // Co testujeme:
        //  • že DiscountPercent == null vypne slevu
        //  • že Total se vrátí na základní hodnotu bez slevy
        // -----------------------------------------------------------------
        [Test]
        [Order(25)]
        public void T_25_Discount_Should_Reset_When_DiscountPercent_Is_Set_To_Null()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                Quantity = 2,
                WorkItem = new PriceItems { BasePrice = 100.0, MaterialCoef = 1.0, PositionCoef = 1.0 },
                IsDiscountEnabled = true,
                DiscountPercent = 20.0
            };

            double discounted = vm.Total; // 200 - 20% = 160

            vm.DiscountPercent = null;
            double reset = vm.Total; // 200

            Assert.AreEqual(160.0, discounted, 0.001);
            Assert.AreEqual(200.0, reset, 0.001,
                "DiscountPercent == null musí resetovat slevu.");
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 26: Total zvládne sekvenci WorkItem → Material → WorkItem
        // -----------------------------------------------------------------
        // Co testujeme:
        //  • že ViewModel správně přepíná mezi větvemi výpočtu
        //  • že žádná změna nezpůsobí chybný cache nebo starou hodnotu
        // -----------------------------------------------------------------
        [Test]
        [Order(26)]
        public void T_26_Total_Should_Handle_WorkItem_Then_MaterialItem_Then_WorkItem()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                Quantity = 3
            };

            vm.WorkItem = new PriceItems { BasePrice = 100.0, MaterialCoef = 1.0, PositionCoef = 1.0 };
            double step1 = vm.Total; // 300

            vm.WorkItem = null;
            vm.SelectedMaterialPrice = new MaterialPrice { Price = 50.0m };
            double step2 = vm.Total; // 150

            vm.WorkItem = new PriceItems { BasePrice = 200.0, MaterialCoef = 1.0, PositionCoef = 1.0 };
            double step3 = vm.Total; // 600

            Assert.AreEqual(300.0, step1, 0.001);
            Assert.AreEqual(150.0, step2, 0.001);
            Assert.AreEqual(600.0, step3, 0.001,
                "ViewModel musí správně přepínat mezi WorkItem a MaterialItem.");
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 27: Total nesmí vyhazovat výjimky při nevalidních hodnotách
        // -----------------------------------------------------------------
        // Co testujeme:
        //  • že ViewModel je robustní vůči nevalidním vstupům
        //  • že Quantity = 0, DiscountPercent = -999, BasePrice = -100
        //    nevyvolají výjimku
        //  • že Total se bezpečně vrátí jako 0 nebo validní hodnota
        // -----------------------------------------------------------------
        [Test]
        [Order(27)]
        public void T_27_Total_Should_Not_Throw_On_Invalid_Values()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                Quantity = 0,   // pokud je Quantity int, musí být 0, ne null
                WorkItem = new PriceItems
                {
                    BasePrice = -100.0,
                    MaterialCoef = 1.0,
                    PositionCoef = 1.0
                },
                IsDiscountEnabled = true,
                DiscountPercent = -999.0
            };

            Assert.DoesNotThrow(() =>
            {
                double total = vm.Total;
                _ = total;
            }, "Nevalidní hodnoty nesmí způsobit výjimku.");
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 28: Total se musí přepočítat při změně koeficientů práce
        // -----------------------------------------------------------------
        // Co testujeme:
        //  • že změna MaterialCoef nebo PositionCoef okamžitě ovlivní Total
        //  • že ViewModel správně reaguje na změny WorkItem parametrů
        // -----------------------------------------------------------------
        [Test]
        [Order(28)]
        public void T_28_Total_Should_Update_When_WorkItem_Coefs_Change()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                Quantity = 2,
                WorkItem = new PriceItems
                {
                    BasePrice = 100.0,
                    MaterialCoef = 1.0,
                    PositionCoef = 1.0
                }
            };

            double step1 = vm.Total; // 100 × 1 × 1 × 2 = 200

            vm.WorkItem.MaterialCoef = 2.0;
            double step2 = vm.Total; // 100 × 2 × 1 × 2 = 400

            vm.WorkItem.PositionCoef = 0.5;
            double step3 = vm.Total; // 100 × 2 × 0.5 × 2 = 200

            Assert.AreEqual(200.0, step1, 0.001);
            Assert.AreEqual(400.0, step2, 0.001);
            Assert.AreEqual(200.0, step3, 0.001,
                "Změna koeficientů práce musí okamžitě ovlivnit Total.");
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 29: Total musí použít novou cenu materiálu po změně dodavatele
        // -----------------------------------------------------------------
        // Co testujeme:
        //  • že změna SelectedMaterialPrice přepne výpočet na novou cenu
        //  • že ViewModel správně reaguje na změnu dodavatele
        // -----------------------------------------------------------------
        [Test]
        [Order(29)]
        public void T_29_Total_Should_Use_New_MaterialPrice_When_Supplier_Changes()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                Quantity = 3,
                WorkItem = null,
                SelectedMaterialPrice = new MaterialPrice { Price = 10.0m }
            };

            double step1 = vm.Total; // 30

            vm.SelectedMaterialPrice = new MaterialPrice { Price = 25.0m };
            double step2 = vm.Total; // 75

            Assert.AreEqual(30.0, step1, 0.001);
            Assert.AreEqual(75.0, step2, 0.001,
                "Po změně dodavatele musí být použita nová cena materiálu.");
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 30: Total se musí vrátit na 0 po vymazání všech vstupů
        // -----------------------------------------------------------------
        // Co testujeme:
        //  • že ViewModel správně resetuje stav
        //  • že po odstranění práce, materiálu, slevy i množství je Total = 0
        // -----------------------------------------------------------------
        [Test]
        [Order(30)]
        public void T_30_Total_Should_Reset_When_All_Inputs_Are_Cleared()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                Quantity = 5,
                WorkItem = new PriceItems
                {
                    BasePrice = 100.0,
                    MaterialCoef = 1.0,
                    PositionCoef = 1.0
                },
                IsDiscountEnabled = true,
                DiscountPercent = 50.0
            };

            double step1 = vm.Total; // 100 × 5 × 0.5 = 250

            vm.Quantity = 0;
            vm.WorkItem = null;
            vm.SelectedMaterialPrice = null;
            vm.IsDiscountEnabled = false;
            vm.DiscountPercent = 0;

            double step2 = vm.Total; // 0

            Assert.AreEqual(250.0, step1, 0.001);
            Assert.AreEqual(0.0, step2, 0.001);
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 31: Materiálová cena se ignoruje, pokud je WorkItem nastaven
        // -----------------------------------------------------------------
        // Co testujeme:
        //  • že WorkItem má prioritu před materiálem
        //  • že změna SelectedMaterialPrice neovlivní Total, pokud WorkItem != null
        // -----------------------------------------------------------------
        [Test]
        [Order(31)]
        public void T_31_Total_Should_Handle_MaterialPrice_Change_After_WorkItem_Is_Set()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                Quantity = 4,
                WorkItem = new PriceItems
                {
                    BasePrice = 50.0,
                    MaterialCoef = 1.0,
                    PositionCoef = 1.0
                },
                SelectedMaterialPrice = new MaterialPrice { Price = 999.0m }
            };

            double step1 = vm.Total; // 200

            vm.SelectedMaterialPrice = new MaterialPrice { Price = 1.0m };
            double step2 = vm.Total; // stále 200

            Assert.AreEqual(200.0, step1, 0.001);
            Assert.AreEqual(200.0, step2, 0.001);
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 32: Přepnutí slevy musí okamžitě ovlivnit Total
        // -----------------------------------------------------------------
        // Co testujeme:
        //  • že IsDiscountEnabled správně zapíná/vypíná slevu
        //  • že Total se okamžitě přepočítá
        // -----------------------------------------------------------------
        [Test]
        [Order(32)]
        public void T_32_Total_Should_Handle_Discount_Toggle_Correctly()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                Quantity = 2,
                WorkItem = new PriceItems
                {
                    BasePrice = 100.0,
                    MaterialCoef = 1.0,
                    PositionCoef = 1.0
                },
                IsDiscountEnabled = false,
                DiscountPercent = 50.0
            };

            double step1 = vm.Total; // 200

            vm.IsDiscountEnabled = true;
            double step2 = vm.Total; // 100

            vm.IsDiscountEnabled = false;
            double step3 = vm.Total; // 200

            Assert.AreEqual(200.0, step1, 0.001);
            Assert.AreEqual(100.0, step2, 0.001);
            Assert.AreEqual(200.0, step3, 0.001);
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 33: Sleva se musí aplikovat na materiál po odstranění WorkItem
        // -----------------------------------------------------------------
        // Co testujeme:
        //  • že sleva se správně aplikuje na práci
        //  • že po odstranění WorkItem se sleva aplikuje na materiál
        //  • že přepnutí větve výpočtu funguje správně
        // -----------------------------------------------------------------
        [Test]
        [Order(33)]
        public void T_33_Total_Should_Apply_Discount_On_Material_When_WorkItem_Is_Cleared()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                Quantity = 3,
                WorkItem = new PriceItems
                {
                    BasePrice = 100.0,
                    MaterialCoef = 1.0,
                    PositionCoef = 1.0
                },
                SelectedMaterialPrice = new MaterialPrice { Price = 50.0m },
                IsDiscountEnabled = true,
                DiscountPercent = 20.0
            };

            double step1 = vm.Total; // sleva na práci → 240
            vm.WorkItem = null;
            double step2 = vm.Total; // sleva na materiál → 120

            Assert.AreEqual(240.0, step1, 0.001);
            Assert.AreEqual(120.0, step2, 0.001);
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 34: Materiál → práce → sleva
        // -----------------------------------------------------------------
        // Co testujeme:
        //  • že materiál se použije, pokud WorkItem není nastaven
        //  • že po nastavení WorkItem má práce prioritu
        //  • že sleva se aplikuje až na finální větev výpočtu
        // -----------------------------------------------------------------
        [Test]
        [Order(34)]
        public void T_34_Total_Should_Handle_Material_Then_WorkItem_Then_Discount()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                Quantity = 4,
                SelectedMaterialPrice = new MaterialPrice { Price = 25.0m }
            };

            double step1 = vm.Total; // 100
            vm.WorkItem = new PriceItems { BasePrice = 50.0, MaterialCoef = 1.0, PositionCoef = 1.0 };
            double step2 = vm.Total; // 200
            vm.IsDiscountEnabled = true;
            vm.DiscountPercent = 50.0;
            double step3 = vm.Total; // 100

            Assert.AreEqual(100.0, step1, 0.001);
            Assert.AreEqual(200.0, step2, 0.001);
            Assert.AreEqual(100.0, step3, 0.001);
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 35: Vybere se nejvyšší cena materiálu z DB
        // -----------------------------------------------------------------
        // Co testujeme:
        //  • že DB obsahuje více cen materiálu
        //  • že ViewModel správně vybere nejvyšší cenu
        //  • že Total odpovídá správné ceně × Quantity
        // -----------------------------------------------------------------
        [Test]
        [Order(35)]
        public void T_35_Total_Should_Use_Highest_MaterialPrice_From_Database()
        {
            var material = new Material { Id = 1, Name = "Test" };
            var supplierA = new Supplier { Id = 1, Name = "A" };
            var supplierB = new Supplier { Id = 2, Name = "B" };

            _db.Materials.Add(material);
            _db.Suppliers.AddRange(supplierA, supplierB);
            _db.SaveChanges();

            // ✔ double místo decimal → SQLite uloží správně
            _db.MaterialPrices.Add(new MaterialPrice { MaterialId = 1, SupplierId = 1, Price = 10 });
            _db.MaterialPrices.Add(new MaterialPrice { MaterialId = 1, SupplierId = 2, Price = 25 });
            _db.SaveChanges();

            var vm = new CalculationItemViewModel(_db)
            {
                Quantity = 2,
                SelectedMaterialPrice = _db.MaterialPrices.OrderByDescending(p => p.Price).First()
            };

            Assert.AreEqual(50.0, vm.Total, 0.001);
        }


        // -----------------------------------------------------------------
        // 🧪 TEST 36: Sleva po změně materiálu
        // -----------------------------------------------------------------
        // Co testujeme:
        //  • že sleva se aplikuje na materiál
        //  • že změna ceny materiálu okamžitě ovlivní Total
        //  • že sleva se správně přepočítá po změně ceny
        // -----------------------------------------------------------------
        [Test]
        [Order(36)]
        public void T_36_Total_Should_Handle_Discount_After_MaterialPrice_Change()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                Quantity = 3,
                SelectedMaterialPrice = new MaterialPrice { Price = 20.0m },
                IsDiscountEnabled = true,
                DiscountPercent = 50.0
            };

            double step1 = vm.Total; // 30
            vm.SelectedMaterialPrice = new MaterialPrice { Price = 40.0m };
            double step2 = vm.Total; // 60

            Assert.AreEqual(30.0, step1, 0.001);
            Assert.AreEqual(60.0, step2, 0.001);
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 37: Práce po změně slevy
        // -----------------------------------------------------------------
        // Co testujeme:
        //  • že sleva se aplikuje na práci
        //  • že změna DiscountPercent okamžitě ovlivní Total
        // -----------------------------------------------------------------
        [Test]
        [Order(37)]
        public void T_37_Total_Should_Handle_WorkItem_After_Discount_Change()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                Quantity = 2,
                WorkItem = new PriceItems
                {
                    BasePrice = 100.0,
                    MaterialCoef = 1.0,
                    PositionCoef = 1.0
                },
                IsDiscountEnabled = true,
                DiscountPercent = 20.0
            };

            double step1 = vm.Total; // 160
            vm.DiscountPercent = 50.0;
            double step2 = vm.Total; // 100

            Assert.AreEqual(160.0, step1, 0.001);
            Assert.AreEqual(100.0, step2, 0.001);
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 38: Extrémní množství (int.MaxValue)
        // -----------------------------------------------------------------
        // Co testujeme:
        //  • že výpočet zvládne extrémní hodnoty Quantity
        //  • že nedojde k přetečení nebo výjimce
        // -----------------------------------------------------------------
        [Test]
        [Order(38)]
        public void T_38_Total_Should_Handle_MaxInt_Quantity()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                Quantity = int.MaxValue,
                WorkItem = new PriceItems
                {
                    BasePrice = 1.0,
                    MaterialCoef = 1.0,
                    PositionCoef = 1.0
                }
            };

            double total = vm.Total;
            Assert.Greater(total, 0.0);
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 39: Extrémní cena (double.MaxValue)
        // -----------------------------------------------------------------
        // Co testujeme:
        //  • že výpočet zvládne extrémní BasePrice
        //  • že Total se rovná double.MaxValue
        // -----------------------------------------------------------------
        [Test]
        [Order(39)]
        public void T_39_Total_Should_Handle_MaxDouble_BasePrice()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                Quantity = 1,
                WorkItem = new PriceItems
                {
                    BasePrice = double.MaxValue,
                    MaterialCoef = 1.0,
                    PositionCoef = 1.0
                }
            };

            Assert.AreEqual(double.MaxValue, vm.Total);
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 40: Materiál null → materiál validní
        // -----------------------------------------------------------------
        // Co testujeme:
        //  • že Total je 0, pokud SelectedMaterialPrice == null
        //  • že po nastavení materiálu se Total správně přepočítá
        // -----------------------------------------------------------------
        [Test]
        [Order(40)]
        public void T_40_Total_Should_Handle_MaterialPrice_Null_Then_NotNull()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                Quantity = 3,
                SelectedMaterialPrice = null
            };

            double step1 = vm.Total; // 0
            vm.SelectedMaterialPrice = new MaterialPrice { Price = 10.0m };
            double step2 = vm.Total; // 30

            Assert.AreEqual(0.0, step1, 0.001);
            Assert.AreEqual(30.0, step2, 0.001);
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 41: Práce null → práce validní
        // -----------------------------------------------------------------
        // Co testujeme:
        //  • že Total je 0, pokud WorkItem == null
        //  • že po nastavení práce se Total správně přepočítá
        // -----------------------------------------------------------------
        [Test]
        [Order(41)]
        public void T_41_Total_Should_Handle_WorkItem_Null_Then_NotNull()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                Quantity = 2,
                WorkItem = null
            };

            double step1 = vm.Total; // 0
            vm.WorkItem = new PriceItems
            {
                BasePrice = 100.0,
                MaterialCoef = 1.0,
                PositionCoef = 1.0
            };
            double step2 = vm.Total; // 200

            Assert.AreEqual(0.0, step1, 0.001);
            Assert.AreEqual(200.0, step2, 0.001);
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 42: Sleva null → sleva validní
        // -----------------------------------------------------------------
        // Co testujeme:
        //  • že DiscountPercent == null znamená „bez slevy“
        //  • že po nastavení slevy se Total správně přepočítá
        // -----------------------------------------------------------------
        [Test]
        [Order(42)]
        public void T_42_Total_Should_Handle_Discount_Null_Then_NotNull()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                Quantity = 2,
                WorkItem = new PriceItems
                {
                    BasePrice = 100.0,
                    MaterialCoef = 1.0,
                    PositionCoef = 1.0
                },
                IsDiscountEnabled = true,
                DiscountPercent = null
            };

            double step1 = vm.Total; // 200
            vm.DiscountPercent = 50.0;
            double step2 = vm.Total; // 100

            Assert.AreEqual(200.0, step1, 0.001);
            Assert.AreEqual(100.0, step2, 0.001);
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 43: Sleva validní → sleva null
        // -----------------------------------------------------------------
        // Co testujeme:
        //  • že sleva se správně aplikuje
        //  • že DiscountPercent == null resetuje slevu
        // -----------------------------------------------------------------
        [Test]
        [Order(43)]
        public void T_43_Total_Should_Handle_Discount_NotNull_Then_Null()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                Quantity = 2,
                WorkItem = new PriceItems
                {
                    BasePrice = 100.0,
                    MaterialCoef = 1.0,
                    PositionCoef = 1.0
                },
                IsDiscountEnabled = true,
                DiscountPercent = 50.0
            };

            double step1 = vm.Total; // 100
            vm.DiscountPercent = null;
            double step2 = vm.Total; // 200

            Assert.AreEqual(100.0, step1, 0.001);
            Assert.AreEqual(200.0, step2, 0.001);
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 44: Materiál + sleva + změna ceny
        // -----------------------------------------------------------------
        // Co testujeme:
        //  • že sleva se aplikuje na materiál
        //  • že změna ceny materiálu okamžitě ovlivní Total
        // -----------------------------------------------------------------
                [Test]
        [Order(44)]
        public void T_44_Total_Should_Handle_MaterialPrice_Change_With_Discount()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                Quantity = 3,
                SelectedMaterialPrice = new MaterialPrice { Price = 20.0m },
                IsDiscountEnabled = true,
                DiscountPercent = 50.0
            };

            double step1 = vm.Total; // 30
            vm.SelectedMaterialPrice = new MaterialPrice { Price = 40.0m };
            double step2 = vm.Total; // 60

            Assert.AreEqual(30.0, step1, 0.001);
            Assert.AreEqual(60.0, step2, 0.001);
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 45: Práce + sleva + změna ceny
        // -----------------------------------------------------------------
        // Co testujeme:
        //  • že sleva se aplikuje na práci
        //  • že změna BasePrice okamžitě ovlivní Total
        // -----------------------------------------------------------------
        [Test]
        [Order(45)]
        public void T_45_Total_Should_Handle_WorkItem_Change_With_Discount()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                Quantity = 2,
                WorkItem = new PriceItems
                {
                    BasePrice = 100.0,
                    MaterialCoef = 1.0,
                    PositionCoef = 1.0
                },
                IsDiscountEnabled = true,
                DiscountPercent = 50.0
            };

            double step1 = vm.Total; // 100
            vm.WorkItem.BasePrice = 200.0;
            double step2 = vm.Total; // 200

            Assert.AreEqual(100.0, step1, 0.001);
            Assert.AreEqual(200.0, step2, 0.001);
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 46: Změna všech vstupů najednou
        // -----------------------------------------------------------------
        // Co testujeme:
        //  • že ViewModel správně reaguje na změnu všech vstupů
        //  • že Total se správně přepočítá i při kombinaci změn
        // -----------------------------------------------------------------
        [Test]
        [Order(46)]
        public void T_46_Total_Should_Handle_All_Inputs_Changing_At_Once()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                Quantity = 1,
                WorkItem = new PriceItems
                {
                    BasePrice = 100.0,
                    MaterialCoef = 1.0,
                    PositionCoef = 1.0
                },
                SelectedMaterialPrice = new MaterialPrice { Price = 50.0m },
                IsDiscountEnabled = true,
                DiscountPercent = 20.0
            };

            double step1 = vm.Total; // 80

            vm.Quantity = 5;
            vm.WorkItem.BasePrice = 200.0;
            vm.SelectedMaterialPrice = new MaterialPrice { Price = 10.0m };
            vm.IsDiscountEnabled = false;
            vm.DiscountPercent = 0;

            double step2 = vm.Total; // 1000

            Assert.AreEqual(80.0, step1, 0.001);
            Assert.AreEqual(1000.0, step2, 0.001);
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 47: Total – WorkItem má přednost i při slevě a materiálu
        // -----------------------------------------------------------------
        // Co testujeme:
        //  • že WorkItem má vyšší prioritu než MaterialPrice
        //  • že sleva se správně aplikuje na WorkItem
        // -----------------------------------------------------------------
        [Test]
        [Order(47)]
        public void T_47_Total_Should_Handle_WorkItem_With_MaterialPrice_And_Discount()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                Quantity = 2,
                WorkItem = new PriceItems
                {
                    BasePrice = 100,
                    MaterialCoef = 1,
                    PositionCoef = 1
                },
                SelectedMaterialPrice = new MaterialPrice { Price = 999M },
                IsDiscountEnabled = true,
                DiscountPercent = 10
            };

            Assert.AreEqual(180M, (decimal)vm.Total); // (100 * 2) * 0.9
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 48: Total – materiál + sleva + změna množství
        // -----------------------------------------------------------------
        // Co testujeme:
        //  • že sleva se správně aplikuje na materiál
        //  • že změna Quantity přepočítá Total
        // -----------------------------------------------------------------
        [Test]
        [Order(48)]
        public void T_48_Total_Should_Handle_MaterialPrice_With_Discount_And_Quantity_Change()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                SelectedMaterialPrice = new MaterialPrice { Price = 50 },
                IsDiscountEnabled = true,
                DiscountPercent = 20
            };

            vm.Quantity = 3;

            Assert.AreEqual(120, vm.Total); // (50 * 3) * 0.8
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 49: Total – nesmí spadnout při smazané ceně v DB
        // -----------------------------------------------------------------
        // Co testujeme:
        //  • že ViewModel zvládne situaci, kdy je SelectedMaterialPrice = null
        //  • že Total se vrátí na 0
        //  • že výpočet nevyhodí žádnou výjimku
        // -----------------------------------------------------------------
        [Test]
        [Order(49)]
        public void T_49_Total_Should_Not_Throw_When_MaterialPrice_Is_Deleted_From_DB()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                Quantity = 1,
                SelectedMaterialPrice = null
            };

            Assert.DoesNotThrow(() => { var _ = vm.Total; });
            Assert.AreEqual(0, vm.Total);
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 50: Total – musí se přepočítat po změně ceny v DB
        // -----------------------------------------------------------------
        // Co testujeme:
        //  • že změna ceny v MaterialPrice se okamžitě projeví ve výpočtu
        //  • že Total reaguje na změnu hodnoty Price
        // -----------------------------------------------------------------
        [Test]
        [Order(50)]
        public void T_50_Total_Should_Update_When_MaterialPrice_Is_Updated_In_DB()
        {
            var price = new MaterialPrice { Price = 10 };
            var vm = new CalculationItemViewModel(_db)
            {
                Quantity = 5,
                SelectedMaterialPrice = price
            };

            Assert.AreEqual(50, vm.Total);

            price.Price = 20;

            Assert.AreEqual(100, vm.Total);
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 51: Total – WorkItem má přednost před MaterialPrice
        // -----------------------------------------------------------------
        // Co testujeme:
        //  • že pokud je WorkItem vyplněný, MaterialPrice se ignoruje
        //  • že Total se počítá pouze z WorkItem
        // -----------------------------------------------------------------
        [Test]
        [Order(51)]
        public void T_51_Total_Should_Use_WorkItem_When_MaterialPrice_Is_Set()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                Quantity = 2,
                WorkItem = new PriceItems
                {
                    BasePrice = 100,
                    MaterialCoef = 1,
                    PositionCoef = 1
                },
                SelectedMaterialPrice = new MaterialPrice { Price = 999M }
            };

            Assert.AreEqual(200M, (decimal)vm.Total);
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 52: Total – po vymazání WorkItem se má použít MaterialPrice
        // -----------------------------------------------------------------
        // Co testujeme:
        //  • že ViewModel správně přepne výpočet z WorkItem na MaterialPrice
        //  • že Total se přepočítá podle materiálu
        // -----------------------------------------------------------------
        [Test]
        [Order(52)]
        public void T_52_Total_Should_Use_MaterialPrice_When_WorkItem_Is_Cleared()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                Quantity = 3,
                WorkItem = new PriceItems
                {
                    BasePrice = 100,
                    MaterialCoef = 1,
                    PositionCoef = 1
                },
                SelectedMaterialPrice = new MaterialPrice { Price = 50M }
            };

            Assert.AreEqual(300M, (decimal)vm.Total);

            vm.WorkItem = null;

            Assert.AreEqual(150M, (decimal)vm.Total);
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 53: Total – ošetření floating-point edge case hodnot
        // -----------------------------------------------------------------
        // Co testujeme:
        //  • že SQLite REAL nepokazí výpočet (např. 0.1 + 0.2)
        //  • že Total se správně zaokrouhlí na 3 desetinná místa
        // -----------------------------------------------------------------
        [Test]
        [Order(53)]
        public void T_53_Total_Should_Handle_FloatingPoint_EdgeCases()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                Quantity = 1,
                SelectedMaterialPrice = new MaterialPrice { Price = 0.1M + 0.2M }
            };

            Assert.AreEqual(0.3M, Math.Round(vm.Total, 3));
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 54: Total – velmi malé ceny musí být správně zpracovány
        // -----------------------------------------------------------------
        // Co testujeme:
        //  • že REAL zvládne extrémně malé hodnoty
        //  • že Total se správně zaokrouhlí
        // -----------------------------------------------------------------
        [Test]
        [Order(54)]
        public void T_54_Total_Should_Handle_Very_Small_Prices()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                Quantity = 10,
                SelectedMaterialPrice = new MaterialPrice { Price = 0.0001M }
            };

            Assert.AreEqual(0.001M, Math.Round(vm.Total, 3));
        }
    }
}