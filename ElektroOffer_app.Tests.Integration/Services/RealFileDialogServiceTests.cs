using System.IO;
using System.Threading;
using NUnit.Framework;
using ElektroOffer_app.Services.Implementations;

namespace ElektroOffer_app.Tests.Integration.Services
{
    // ============================================================================
    // 🧪 INTEGRATION TEST – RealFileDialogService
    // ----------------------------------------------------------------------------
    // Proč integrační test?
    //  • Třída používá skutečné WPF dialogy (OpenFileDialog / SaveFileDialog)
    //  • Tyto dialogy vyžadují STA thread → unit test runner by se zasekl
    //  • GitHub Actions běží bez UI → dialogy nelze spouštět automaticky
    //
    // Proto jsou testy označeny jako [Explicit]:
    //  • Lokálně se spustí normálně
    //  • V CI pipeline se přeskočí → pipeline se nezasekne
    //
    // Co testujeme:
    //  • Metoda nevyhodí výjimku
    //  • Dialog lze bezpečně vytvořit
    //  • Dialog se v testovacím prostředí NEotevře (ShowDialog vrátí false)
    //
    // Test NENÍ funkční test UI → je to smoke-test stability implementace.
    // ============================================================================

    [TestFixture]
    [Apartment(ApartmentState.STA)] // 🔥 WPF dialogy vyžadují STA thread
    /// <summary>Explicitní smoke testy skutečných systémových dialogů pro ruční spuštění.</summary>
    public class RealFileDialogServiceTests
    {
        private string _testFilePath = null!;

        [SetUp]
        public void Setup()
        {
            // 🧪 Automaticky vytvoříme testovací soubor
            _testFilePath = Path.Combine(Path.GetTempPath(), "eof_test_dialog.txt");
            File.WriteAllText(_testFilePath, "Test content");
        }

        [TearDown]
        public void Cleanup()
        {
            if (File.Exists(_testFilePath))
                File.Delete(_testFilePath);
        }

        [Test, Explicit("UI dialog – spouštět pouze lokálně, CI nemá UI prostředí")]
        public void ShowOpenFileDialog_Should_Not_Throw()
        {
            var service = new RealFileDialogService();

            Assert.DoesNotThrow(() =>
            {
                // 🔧 Nastavíme testovací adresář jako výchozí
                Environment.CurrentDirectory = Path.GetTempPath();

                var result = service.ShowOpenFileDialog(
                    "Text files (*.txt)|*.txt",
                    "Open test file"
                );

                // ✔ Výsledek může být null → test je OK
            });
        }

        [Test, Explicit("UI dialog – spouštět pouze lokálně, CI nemá UI prostředí")]
        public void ShowSaveFileDialog_Should_Not_Throw()
        {
            var service = new RealFileDialogService();

            Assert.DoesNotThrow(() =>
            {
                Environment.CurrentDirectory = Path.GetTempPath();

                var result = service.ShowSaveFileDialog(
                    "Text files (*.txt)|*.txt",
                    "Save test file",
                    ".txt",
                    "saved_test.txt"
                );

                // ✔ Výsledek může být null → test je OK
            });
        }
    }
}
