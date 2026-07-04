
// ============================================================================
// Rozhraní pro práci se souborovým systémem (File.ReadAllText / WriteAllText)
// ============================================================================

namespace ElektroOffer_app.Services.Abstractions
{
    /// <summary>
    /// Abstrakce pro čtení a zápis souborů.
    /// Umožňuje testovat ProjectService bez skutečného přístupu na disk.
    /// </summary>
    public interface IFileSystemService
    {
        /// <summary>
        /// Zapíše text do souboru na dané cestě.
        /// </summary>
        void WriteAllText(string path, string content);

        /// <summary>
        /// Načte text ze souboru na dané cestě.
        /// </summary>
        string ReadAllText(string path);
    }
}
