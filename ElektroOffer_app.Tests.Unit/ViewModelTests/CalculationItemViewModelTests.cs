using NUnit.Framework;
using Microsoft.EntityFrameworkCore;     // OPRAVA: chybělo → potřebné pro DbContextOptionsBuilder<>
using Microsoft.Data.Sqlite;             // OPRAVA: chybělo → potřebné pro SqliteConnection (SQLite InMemory)
using ElektroOffer_app.Data;
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
    // DŮLEŽITÁ ZMĚNA (oprava po refaktoringu 1.7.6):
    // -----------------------------------------------------------------------
    // CalculationItemViewModel už NEMÁ bezparametrický konstruktor.
    // Po opravě bugu s izolací databázového kontextu (_db vs. new AppDbContext())
    // vyžaduje konstruktor povinný parametr AppDbContext db.
    //
    // Proto zde v [SetUp] vytváříme jednu sdílenou SQLite InMemory databázi
    // pro každý jednotlivý test (ne pro celou třídu!) a předáváme ji do
    // konstruktoru ViewModelu. Používáme Microsoft.Data.Sqlite InMemory,
    // NE EF InMemory provider (UseInMemoryDatabase) – to je záměrné a
    // odpovídá principu zavedenému napříč projektem: reálné SQLite chování
    // (typy sloupců, omezení, chování transakcí) se chová jinak než
    // zjednodušený EF InMemory provider, a testy tak lépe odpovídají realitě.
    //
    // Testy zde pokrývají základní scénáře, které musí fungovat deterministicky
    // bez ohledu na databázi nebo UI. Pokročilé testy (DB, kaskáda, reset logiky)
    // jsou v CalculationItemViewModel_AdvancedTests.cs.
    // =====================================================================
    public class CalculationItemViewModelTests
    {
        // Pole na úrovni třídy – drží otevřené připojení a kontext po dobu
        // běhu JEDNOHO testu. NUnit vytvoří novou instanci třídy pro každý
        // test, takže tahle pole nejsou sdílená mezi testy (žádné riziko
        // "prosakování" dat mezi testy).
        private SqliteConnection _connection = null!;
        private AppDbContext _db = null!;

        // -----------------------------------------------------------------
        // 🔧 SETUP – spustí se PŘED každým testem
        // -----------------------------------------------------------------
        // Vytvoří čerstvou SQLite databázi v paměti (":memory:") a nový
        // AppDbContext nad ní. Připojení musí zůstat OTEVŘENÉ po celou dobu
        // testu – jakmile se SqliteConnection zavře, in-memory databáze
        // zanikne i se všemi daty.
        [SetUp]
        public void Setup()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(_connection)
                .Options;

            _db = new AppDbContext(options);

            // Vytvoří schéma (tabulky) podle DbSetů v AppDbContext.
            // Bez tohoto volání by databáze byla prázdná i strukturně (bez tabulek).
            _db.Database.EnsureCreated();
        }

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
        //
        // OPRAVA: přidán argument (_db) do konstruktoru – bez něj test
        // nejde ani zkompilovat po refaktoringu 1.7.6.
        [Test]
        public void Total_Should_Calculate_WorkItem_Correctly()
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
        //
        // OPRAVA: Původně si tento test vytvářel VLASTNÍ databázi přes
        // EF InMemory provider (UseInMemoryDatabase). To je nekonzistentní
        // s principem projektu (SQLite InMemory) a navíc balíček
        // Microsoft.EntityFrameworkCore.InMemory není ani přidaný v .csproj
        // – proto by to samo o sobě padalo na chybu chybějícího typu/metody.
        // Nahrazeno sdíleným _db ze [SetUp], stejně jako ostatní testy.
        [Test]
        public void Total_Should_Calculate_MaterialItem_When_WorkItem_Is_Null()
        {
            var vm = new CalculationItemViewModel(_db)
            {
                // Množství (Quantity)
                Quantity = 5,

                // Práce (WorkItem) je NULL → musí se použít materiál
                WorkItem = null,

                // Materiál s cenou 200 Kč / MJ
                MaterialItem = new Material
                {
                    Price = 200
                }
            };

            // Očekáváme: Total = 5 × 200 = 1000
            Assert.AreEqual(1000, vm.Total, 0.001,
                "Pokud WorkItem == null, musí se cena počítat z MaterialItem.Price × Quantity.");
        }

        // -----------------------------------------------------------------
        // 🧪 TEST 3: Sleva se správně aplikuje
        // -----------------------------------------------------------------
        // OPRAVA: přidán argument (_db).
        [Test]
        public void Total_Should_Apply_Discount()
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
        // 🧪 TEST 4: Sleva se NEaplikuje, pokud IsDiscountEnabled = false
        // -----------------------------------------------------------------
        // OPRAVA: přidán argument (_db).
        [Test]
        public void Total_Should_Not_Apply_Discount_When_Disabled()
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
        // 🧪 TEST 5: Vypnutí slevy vynuluje DiscountPercent
        // -----------------------------------------------------------------
        // OPRAVA: přidán argument (_db).
        [Test]
        public void IsDiscountEnabled_False_Should_Reset_DiscountPercent()
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
        // 🧪 TEST 6: Kombinace koeficientů
        // -----------------------------------------------------------------
        // OPRAVA: přidán argument (_db).
        [Test]
        public void Total_Should_Respect_Material_And_Position_Coefs()
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
        // 🧪 TEST 7: Quantity = 0 → Total musí být 0
        // -----------------------------------------------------------------
        // OPRAVA: přidán argument (_db).
        [Test]
        public void Total_Should_Be_Zero_When_Quantity_Is_Zero()
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
        // 🧪 TEST 8: BasePrice = 0 → Total musí být 0
        // -----------------------------------------------------------------
        // OPRAVA: přidán argument (_db).
        [Test]
        public void Total_Should_Be_Zero_When_BasePrice_Is_Zero()
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
        // 🧪 TEST 9: Oba zdroje jsou null → Total = 0
        // -----------------------------------------------------------------
        // OPRAVA: přidán argument (_db).
        [Test]
        public void Total_Should_Be_Zero_When_WorkItem_And_MaterialItem_Are_Null()
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
        // 🧪 TEST 10: Pokud existuje WorkItem i MaterialItem, použije se WorkItem
        // -----------------------------------------------------------------
        // OPRAVA: přidán argument (_db).
        [Test]
        public void Total_Should_Use_WorkItem_When_Both_WorkItem_And_MaterialItem_Are_Set()
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
        // OPRAVA: přidán argument (_db).
        [Test]
        public void Total_Should_Be_Zero_When_Discount_Is_100_Percent()
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
        // 🧪 TEST 12: Sleva > 100 % → Total nesmí být záporný
        // -----------------------------------------------------------------
        // OPRAVA: přidán argument (_db).
        [Test]
        public void Total_Should_Not_Be_Negative_When_Discount_Above_100()
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
        // 🧪 TEST 13: Záporná sleva (< 0) se ignoruje
        // -----------------------------------------------------------------
        // OPRAVA: přidán argument (_db).
        [Test]
        public void Total_Should_Ignore_Negative_Discount()
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
        // 🧪 TEST 14: Změna Quantity vyvolá PropertyChanged pro Total
        // -----------------------------------------------------------------
        // OPRAVA: přidán argument (_db).
        [Test]
        public void Quantity_Should_Raise_PropertyChanged_For_Total()
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
        // 🧪 TEST 15: Změna WorkItem vyvolá PropertyChanged pro Total
        // -----------------------------------------------------------------
        // OPRAVA: přidán argument (_db).
        [Test]
        public void WorkItem_Should_Raise_PropertyChanged_For_Total()
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
        // 🧪 TEST 16: Změna MaterialItem vyvolá PropertyChanged pro Total
        // -----------------------------------------------------------------
        // OPRAVA: přidán argument (_db).
        [Test]
        public void MaterialItem_Should_Raise_PropertyChanged_For_Total()
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
        // 🧪 TEST 17: Změna DiscountPercent vyvolá PropertyChanged pro Total
        // -----------------------------------------------------------------
        // OPRAVA: přidán argument (_db).
        [Test]
        public void DiscountPercent_Should_Raise_PropertyChanged_For_Total()
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
    }
}