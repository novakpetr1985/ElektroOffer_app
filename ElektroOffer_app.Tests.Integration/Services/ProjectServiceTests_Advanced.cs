using NUnit.Framework;
using ElektroOffer_app.Services;
using ElektroOffer_app.Models;
using System.IO;

namespace ElektroOffer_app.Tests.Integration.Services
{
    /// =====================================================================
    /// 📁 INTEGRATION TESTS — ProjectService
    /// Testujeme reálné ukládání a načítání projektů do souboru.
    /// =====================================================================

    public class ProjectServiceTests_Advanced
    {
        private ProjectService? _service;
        private string? _tempFile;

        [SetUp]
        public void Setup()
        {
            _service = new ProjectService();
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
            var project = new ProjectData
            {
                ProjectName = "TestProjekt"
            };

            var path = _service!.Save(project, _tempFile);

            Assert.IsNotNull(path);
            Assert.IsTrue(File.Exists(path!));
            Assert.IsNotNull(project.SavedAt);
        }
    }
}
