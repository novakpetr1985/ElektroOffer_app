using NUnit.Framework;
using ElektroOffer_app.Services;
using ElektroOffer_app.Models;
using ElektroOffer_app.Services.Abstractions;
using ElektroOffer_app.Services.Implementations;
using System.IO;

namespace ElektroOffer_app.Tests.Integration.Services   // ← OPRAVENO
{
    /// =====================================================================
    /// 📁 INTEGRATION TESTS — ProjectService
    /// Testujeme reálné ukládání a načítání projektů do souboru.
    /// =====================================================================

    /// <summary>Ověřuje chyby, zrušené dialogy a nestandardní souborové scénáře projektu.</summary>
    public class ProjectServiceTests_Advanced
    {
        private ProjectService? _service;
        private string? _tempFile;

        [SetUp]
        public void Setup()
        {
            // 🔧 INTEGRATION TEST = používáme skutečné implementace
            var dialogs = new RealFileDialogService();      // skutečný dialog (nepoužije se)
            var fs = new RealFileSystemService();           // skutečný File.Read/Write
            var msg = new RealMessageBoxService();          // skutečný MessageBox

            // ✔ DI konstruktor — už žádné null služby
            _service = new ProjectService(dialogs, fs, msg);

            // 🧪 Temporary file pro test
            _tempFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.eof");
        }

        [TearDown]
        public void Cleanup()
        {
            if (_tempFile != null && File.Exists(_tempFile))
                File.Delete(_tempFile);
        }

        [Test]
        public void Save_Should_Write_File_And_Update_SavedAt()
        {
            // ARRANGE
            var project = new ProjectData
            {
                ProjectName = "TestProjekt"
            };

            // ACT
            var path = _service!.Save(project, _tempFile);

            // ASSERT
            Assert.IsNotNull(path, "Save() vrátil null — ukládání selhalo.");
            Assert.IsTrue(File.Exists(path!), "Soubor nebyl vytvořen.");
            Assert.IsNotNull(project.SavedAt, "SavedAt nebyl nastaven.");
        }
    }
}
