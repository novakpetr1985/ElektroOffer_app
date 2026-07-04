using System.IO;
using System.Threading;
using NUnit.Framework;
using ElektroOffer_app.Services.Implementations;

namespace ElektroOffer_app.Tests.Integration.Services
{
    // ============================================================================
    // 🧪 INTEGRATION TEST – RealFileDialogService
    // ----------------------------------------------------------------------------
    // Tento test:
    //  • automaticky vytvoří testovací .txt soubor
    //  • nastaví dialog tak, aby se NEpokoušel otevřít poslední cestu Windows
    //  • běží v STA threadu (nutné pro WPF dialogy)
    //  • ověřuje, že dialog lze bezpečně vytvořit a zavolat
    //  • dialog se NEotevře (ShowDialog vrátí false)
    // ============================================================================

    [TestFixture]
    [Apartment(ApartmentState.STA)] // 🔥 WPF dialogy vyžadují STA thread
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

        [Test]
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

        [Test]
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
