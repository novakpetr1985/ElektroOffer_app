using NUnit.Framework;
using ElektroOffer_app.ViewModels.Items;
using ElektroOffer_app.Models;

namespace ElektroOffer_app.Tests.Unit.ViewModels
{
    // =====================================================================
    // 🧮 UNIT TESTS – CalculationItemViewModel
    // =====================================================================
    // Tyto testy ověřují čistou logiku výpočtů bez databáze a bez WPF UI.
    //
    // Zaměřují se na:
    //   • výpočet Total pro práci (WorkItem)
    //   • výpočet Total pro materiál (MaterialItem)
    //   • aplikaci slevy (IsDiscountEnabled + DiscountPercent)
    //   • reakci na změnu Quantity
    //   • reset slevy při vypnutí IsDiscountEnabled
    //
    // Testy zde pokrývají základní scénáře, které musí fungovat deterministicky
    // bez ohledu na databázi nebo UI. Pokročilé testy (DB, kaskáda, reset logiky)
    // jsou v CalculationItemViewModel_AdvancedTests.cs.
    // =====================================================================
    public class CalculationItemViewModelTests
    {
        // -----------------------------------------------------------------
        // 🧪 TEST 1: Výpočet ceny práce (WorkItem)
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
        [Test]
        public void Total_Should_Calculate_WorkItem_Correctly()
        {
            var vm = new CalculationItemViewModel
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
        // 🧪 TEST 2: Výpočet ceny materiálu (MaterialItem)
        // -----------------------------------------------------------------
        // Ověřuje:
        //   • že pokud WorkItem == null, použije se MaterialItem
        //   • že Total = Material.Price × Quantity
        //   • že WorkItem má prioritu, ale pokud není, logika přepne na materiál
        //
        // Tento test odhalí chyby typu:
        //   • ignorování MaterialItem
        //   • chybné větvení logiky (např. WorkItem preferován i když je null)
        [Test]
        public void Total_Should_Calculate_MaterialItem_When_WorkItem_Is_Null()
        {
            var vm = new CalculationItemViewModel
            {
                Quantity = 5,
                WorkItem = null,
                MaterialItem = new Material { Price = 200 }
            };

            Assert.AreEqual(1000, vm.Total, 0.001);
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 3: Sleva se správně aplikuje
        // -----------------------------------------------------------------
        // Ověřuje:
        //   • že sleva se aplikuje pouze pokud IsDiscountEnabled == true
        //   • že DiscountPercent je správně převeden na faktor (1 - p/100)
        //   • že výsledek je správně přepočítán
        //
        // Tento test odhalí chyby typu:
        //   • sleva se aplikuje i když je vypnutá
        //   • špatný výpočet procent (např. dělení 10 místo 100)
        //   • chybné pořadí operací
        [Test]
        public void Total_Should_Apply_Discount()
        {
            var vm = new CalculationItemViewModel
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
        // 🧪 TEST 4: Sleva se NEaplikuje, pokud IsDiscountEnabled = false
        // -----------------------------------------------------------------
        // Ověřuje:
        //   • že DiscountPercent je ignorováno, pokud je sleva vypnutá
        //   • že Total se počítá bez slevy
        //
        // Tento test odhalí chyby typu:
        //   • sleva se aplikuje i když je vypnutá
        //   • logika ignoruje IsDiscountEnabled
        [Test]
        public void Total_Should_Not_Apply_Discount_When_Disabled()
        {
            var vm = new CalculationItemViewModel
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
        // 🧪 TEST 5: Vypnutí slevy vynuluje DiscountPercent
        // -----------------------------------------------------------------
        // Ověřuje:
        //   • že při vypnutí slevy se DiscountPercent automaticky nastaví na null
        //   • že ViewModel udržuje konzistentní stav
        //
        // Tento test odhalí chyby typu:
        //   • DiscountPercent zůstává nastavené → nekonzistentní stav
        //   • Total se počítá se starou slevou
        [Test]
        public void IsDiscountEnabled_False_Should_Reset_DiscountPercent()
        {
            var vm = new CalculationItemViewModel
            {
                Quantity = 1,
                IsDiscountEnabled = true,
                DiscountPercent = 15
            };

            vm.IsDiscountEnabled = false;

            Assert.IsNull(vm.DiscountPercent);
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 6: Kombinace koeficientů
        // -----------------------------------------------------------------
        // Ověřuje:
        //   • že MaterialCoef a PositionCoef se správně násobí
        //   • že koeficienty mohou být > 1 i < 1
        //
        // Tento test odhalí chyby typu:
        //   • ignorování jednoho z koeficientů
        //   • chybné pořadí násobení
        [Test]
        public void Total_Should_Respect_Material_And_Position_Coefs()
        {
            var vm = new CalculationItemViewModel
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
        // 🧪 TEST 7: Quantity = 0 → Total musí být 0
        // -----------------------------------------------------------------
        // Ověřuje:
        //   • že nulové množství vždy vede k Total = 0
        //   • že koeficienty ani BasePrice nemají vliv
        //
        // Tento test odhalí chyby typu:
        //   • Total se počítá i při Quantity = 0
        //   • chybné větvení logiky
        [Test]
        public void Total_Should_Be_Zero_When_Quantity_Is_Zero()
        {
            var vm = new CalculationItemViewModel
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
        // 🧪 TEST 8: BasePrice = 0 → Total musí být 0
        // -----------------------------------------------------------------
        // Ověřuje:
        //   • že nulová základní cena vede k Total = 0
        //   • že koeficienty ani Quantity nemají vliv
        //
        // Tento test odhalí chyby typu:
        //   • Total se počítá i při BasePrice = 0
        [Test]
        public void Total_Should_Be_Zero_When_BasePrice_Is_Zero()
        {
            var vm = new CalculationItemViewModel
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
        // 🧪 TEST 9: Oba zdroje jsou null → Total = 0
        // -----------------------------------------------------------------
        // Ověřuje:
        //   • že pokud není vybrána práce ani materiál, Total = 0
        //   • že ViewModel nepadá na null referencích
        //
        // Tento test odhalí chyby typu:
        //   • NullReferenceException při přístupu k WorkItem/MaterialItem
        //   • chybné větvení logiky
        [Test]
        public void Total_Should_Be_Zero_When_WorkItem_And_MaterialItem_Are_Null()
        {
            var vm = new CalculationItemViewModel
            {
                Quantity = 10,
                WorkItem = null,
                MaterialItem = null
            };

            Assert.AreEqual(0, vm.Total, 0.001);
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 10: Pokud existuje WorkItem i MaterialItem, použije se WorkItem
        // -----------------------------------------------------------------
        // Ověřuje:
        //   • že WorkItem má vyšší prioritu než MaterialItem
        //   • že logika výpočtu Total správně preferuje práci před materiálem
        //   • že MaterialItem je ignorován, pokud je WorkItem nastaven
        //
        // Tento test odhalí chyby typu:
        //   • Total se počítá z materiálu místo práce
        //   • kombinace obou zdrojů vede k součtu (což je špatně)
        //   • chybné větvení logiky (např. MaterialItem má přednost)
        [Test]
        public void Total_Should_Use_WorkItem_When_Both_WorkItem_And_MaterialItem_Are_Set()
        {
            var vm = new CalculationItemViewModel
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
        //   • že sleva 100 % vynuluje Total
        //   • že logika správně aplikuje maximální možnou slevu
        //   • že nedojde k chybě typu záporná cena
        //
        // Tento test odhalí chyby typu:
        //   • sleva se aplikuje špatně (např. 100 % → 100 % zůstane)
        //   • Total je záporný
        //   • chybné pořadí operací při aplikaci slevy
        [Test]
        public void Total_Should_Be_Zero_When_Discount_Is_100_Percent()
        {
            var vm = new CalculationItemViewModel
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
        // 🧪 TEST 12: Sleva > 100 % → Total nesmí být záporný
        // -----------------------------------------------------------------
        // Ověřuje:
        //   • že sleva nad 100 % je oříznuta (clamp) na 0
        //   • že Total nikdy nemůže být záporný
        //
        // Tento test odhalí chyby typu:
        //   • záporné výsledky (např. -200)
        //   • chybné ošetření extrémních hodnot DiscountPercent
        [Test]
        public void Total_Should_Not_Be_Negative_When_Discount_Above_100()
        {
            var vm = new CalculationItemViewModel
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
        // 🧪 TEST 13: Záporná sleva (< 0) se ignoruje
        // -----------------------------------------------------------------
        // Ověřuje:
        //   • že záporné hodnoty slevy jsou ignorovány
        //   • že Total se počítá bez slevy
        //
        // Tento test odhalí chyby typu:
        //   • záporná sleva se aplikuje jako zvýšení ceny
        //   • logika nevaliduje DiscountPercent
        [Test]
        public void Total_Should_Ignore_Negative_Discount()
        {
            var vm = new CalculationItemViewModel
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
        // 🧪 TEST 14: Změna Quantity vyvolá PropertyChanged pro Total
        // -----------------------------------------------------------------
        // Ověřuje:
        //   • že změna Quantity vyvolá PropertyChanged("Quantity")
        //   • že změna Quantity vyvolá PropertyChanged("Total")
        //   • že MVVM notifikace funguje správně
        //
        // Tento test odhalí chyby typu:
        //   • Total se neaktualizuje při změně Quantity
        //   • PropertyChanged se nevyvolá → UI se neaktualizuje
        [Test]
        public void Quantity_Should_Raise_PropertyChanged_For_Total()
        {
            var vm = new CalculationItemViewModel
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
        // 🧪 TEST 15: Změna WorkItem vyvolá PropertyChanged pro Total
        // -----------------------------------------------------------------
        // Ověřuje:
        //   • že změna WorkItem vyvolá PropertyChanged("WorkItem")
        //   • že změna WorkItem vyvolá PropertyChanged("Total")
        //
        // Tento test odhalí chyby typu:
        //   • Total se neaktualizuje při změně WorkItem
        //   • PropertyChanged se nevyvolá → UI zůstane se starou cenou
        [Test]
        public void WorkItem_Should_Raise_PropertyChanged_For_Total()
        {
            var vm = new CalculationItemViewModel
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
        // 🧪 TEST 16: Změna MaterialItem vyvolá PropertyChanged pro Total
        // -----------------------------------------------------------------
        // Ověřuje:
        //   • že změna MaterialItem vyvolá PropertyChanged("MaterialItem")
        //   • že změna MaterialItem vyvolá PropertyChanged("Total")
        //   • že logika správně přepíná mezi WorkItem a MaterialItem
        //
        // Tento test odhalí chyby typu:
        //   • Total se neaktualizuje při změně MaterialItem
        //   • PropertyChanged se nevyvolá → UI se neaktualizuje
        [Test]
        public void MaterialItem_Should_Raise_PropertyChanged_For_Total()
        {
            var vm = new CalculationItemViewModel
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
        // 🧪 TEST 17: Změna DiscountPercent vyvolá PropertyChanged pro Total
        // -----------------------------------------------------------------
        // Ověřuje:
        //   • že změna DiscountPercent vyvolá PropertyChanged("DiscountPercent")
        //   • že změna DiscountPercent vyvolá PropertyChanged("Total")
        //   • že sleva je správně zahrnuta do výpočtu
        //
        // Tento test odhalí chyby typu:
        //   • Total se neaktualizuje při změně slevy
        //   • PropertyChanged se nevyvolá → UI zůstane se starou cenou
        [Test]
        public void DiscountPercent_Should_Raise_PropertyChanged_For_Total()
        {
            var vm = new CalculationItemViewModel
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
    }
}
