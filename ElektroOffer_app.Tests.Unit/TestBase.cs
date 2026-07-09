using ElektroOffer_app.Data;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace ElektroOffer_app.Tests.Unit
{
    // =====================================================================
    // 🧱 TEST BASE – společná základna pro všechny UNIT testy
    // =====================================================================
    // Tato třída poskytuje sdílené prostředky pro všechny testovací třídy:
    //
    //   • databázový kontext (_db)
    //   • SetUp – vytvoření nové testovací DB před každým testem
    //   • TearDown – zrušení DB po každém testu
    //
    // ---------------------------------------------------------------------
    // 🔍 Proč tato základna existuje
    // ---------------------------------------------------------------------
    // CalculationItemViewModel i ProjectService pracují s databází (SQLite).
    // Aby byly testy spolehlivé, musí každá testovací metoda běžet
    // v naprosto čistém prostředí:
    //
    //   • žádná data z předchozích testů nesmí zůstat v DB
    //   • žádné zamčené SQLite handle
    //   • žádné duplicitní ID (UNIQUE constraint failed)
    //   • žádné „kontaminované“ ceny materiálu z jiných testů
    //
    // Proto se před každým testem databáze kompletně smaže a znovu vytvoří.
    // Tím se simuluje reálné chování aplikace, která pracuje s čistou DB.
    //
    // ---------------------------------------------------------------------
    // 🧪 Výhody tohoto přístupu
    // ---------------------------------------------------------------------
    //   • testy jsou deterministické (stejný vstup → stejný výstup)
    //   • testy se navzájem neovlivňují
    //   • testy odhalí reálné chyby v DB (FK, UNIQUE, CASCADE)
    //   • testy odhalí reálné chování EF Core při Include(), vztazích,
    //     lookupu cen, kaskádách práce i materiálu
    //   • žádné falešné chyby způsobené zbytky dat
    //
    // ---------------------------------------------------------------------
    // 🧪 Jak se TestBase používá
    // ---------------------------------------------------------------------
    // Každá testovací třída dědí z TestBase:
    //
    //     public class CalculationItemViewModelTests_Total : TestBase
    //
    // Díky tomu má automaticky:
    //   • přístup k _db
    //   • čistou DB před každým testem
    //   • správně uvolněné prostředky po testu
    //
    // TestBase není označen jako [TestFixture], protože sám neobsahuje testy.
    // Testovací třídy mají vlastní [TestFixture].
    // =====================================================================
    public abstract class TestBase
    {
        // -----------------------------------------------------------------
        // 🔧 Sdílený databázový kontext
        // -----------------------------------------------------------------
        // Null-forgiving operator (!) je bezpečný, protože SetUp garantuje,
        // že _db bude vždy inicializováno před každým testem.
        protected AppDbContext _db = null!;

        // -----------------------------------------------------------------
        // 🚀 SETUP – spustí se před KAŽDÝM testem
        // -----------------------------------------------------------------
        // Vytvoří nový AppDbContext, kompletně smaže databázi a znovu ji
        // vytvoří. Tím se zajistí, že každý test běží v izolovaném prostředí.
        //
        // Toto je nejbližší simulace reálného chování aplikace:
        //   • SQLite je skutečný provider
        //   • EF Core používá reálné constrainty
        //   • Include(), vztahy, lookupy a kaskády fungují stejně jako v produkci
        //
        // Tím se odstraní:
        //   • UNIQUE constraint chyby
        //   • zamčené SQLite handle
        //   • kontaminace dat mezi testy
        //   • nesprávné ceny materiálu z jiných testů
        // -----------------------------------------------------------------
        [SetUp]
        public void SetUp()
        {
            _db = new AppDbContext();

            // kompletní reset databáze
            _db.Database.EnsureDeleted();
            _db.Database.EnsureCreated();
        }

        // -----------------------------------------------------------------
        // 🧹 TEARDOWN – spustí se po KAŽDÉM testu
        // -----------------------------------------------------------------
        // Uvolní databázový kontext a zavře připojení.
        // Zabraňuje únikům paměti a zamčeným SQLite handle.
        // -----------------------------------------------------------------
        [TearDown]
        public void TearDown()
        {
            _db.Dispose();
        }
    }
}
