// ============================================================================
// Rozhraní pro práci se souborovými dialogy (OpenFileDialog / SaveFileDialog)
// ============================================================================

namespace ElektroOffer_app.Services.Abstractions
{
    /// <summary>
    /// Abstrakce pro otevírací a ukládací dialogy.
    /// Umožňuje testovat ProjectService bez skutečných dialogů.
    /// </summary>
    public interface IFileDialogService
    {
        /// <summary>
        /// Zobrazí dialog pro výběr souboru k otevření.
        /// Vrací cestu k souboru nebo null, pokud uživatel dialog zavře.
        /// </summary>
        string? ShowOpenFileDialog(string filter, string title);

        /// <summary>
        /// Zobrazí dialog pro uložení souboru.
        /// Vrací cestu k souboru nebo null, pokud uživatel dialog zavře.
        /// </summary>
        string? ShowSaveFileDialog(string filter, string title, string defaultExt, string fileName);
    }
}
