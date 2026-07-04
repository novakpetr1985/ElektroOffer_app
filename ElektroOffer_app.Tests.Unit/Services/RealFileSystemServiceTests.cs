using System;
using System.IO;
using ElektroOffer_app.Services.Implementations;
using NUnit.Framework;

namespace ElektroOffer_app.Tests.Unit.Services
{
    // ============================================================================
    // 🧪 UNIT TEST – RealFileSystemService
    // ----------------------------------------------------------------------------
    // Testujeme základní funkčnost reálné implementace:
    //  • zápis textu do souboru
    //  • čtení textu ze souboru
    //
    // ❗ Test je izolovaný – nepoužívá ProjectService ani databázi
    // ❗ Cílem je ověřit, že RealFileSystemService funguje korektně
    // ============================================================================

    public class RealFileSystemServiceTests
    {
        private RealFileSystemService _service = null!;
        private string _tempFilePath = null!;

        [SetUp]
        public void Setup()
        {
            _service = new RealFileSystemService();
            _tempFilePath = Path.Combine(Path.GetTempPath(), $"eof_test_{Guid.NewGuid()}.txt");
        }

        [TearDown]
        public void Cleanup()
        {
            if (File.Exists(_tempFilePath))
                File.Delete(_tempFilePath);
        }

        [Test]
        public void WriteRead_Should_Work_Correctly()
        {
            var text = "Hello EOF";

            // 🔵 Zápis
            _service.WriteAllText(_tempFilePath, text);
            Assert.IsTrue(File.Exists(_tempFilePath), "Soubor nebyl vytvořen.");

            // 🟢 Čtení
            var read = _service.ReadAllText(_tempFilePath);
            Assert.AreEqual(text, read);
        }
    }
}
