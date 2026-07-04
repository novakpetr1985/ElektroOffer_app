using ElektroOffer_app.Services.Abstractions;
using System.IO;

namespace ElektroOffer_app.Services.Implementations
{
    // ========================================================================
    // 🧩 RealFileSystemService
    // Skutečná implementace práce se souborovým systémem.
    //
    // ÚČEL:
    // - Zajišťuje reálné čtení a zápis souborů na disk
    // - Používá se v INTEGRATION TESTECH
    // - Unit testy používají Mock<IFileSystemService>
    //
    // DŮLEŽITÉ:
    // - Třída neobsahuje žádnou logiku ProjectService
    // - Pouze volá File.ReadAllText / File.WriteAllText
    // ========================================================================
    public class RealFileSystemService : IFileSystemService
    {
        // --------------------------------------------------------------------
        // 📖 Čtení souboru
        // --------------------------------------------------------------------
        public string ReadAllText(string path)
        {
            return File.ReadAllText(path);
        }

        // --------------------------------------------------------------------
        // ✏️ Zápis souboru
        // --------------------------------------------------------------------
        public void WriteAllText(string path, string content)
        {
            File.WriteAllText(path, content);
        }
    }
}
