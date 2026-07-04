
// ============================================================================
// Rozhraní pro zobrazování MessageBoxů (Yes/No/Cancel dialog)
// ============================================================================

using System.Windows;

namespace ElektroOffer_app.Services.Abstractions
{
    /// <summary>
    /// Abstrakce pro MessageBox.
    /// Umožňuje testovat ProjectService bez skutečných oken.
    /// </summary>
    public interface IMessageBoxService
    {
        /// <summary>
        /// Zobrazí MessageBox s textem, titulkem, tlačítky a ikonou.
        /// Vrací výsledek kliknutí uživatele (Yes / No / Cancel).
        /// </summary>
        MessageBoxResult Show(string text, string caption, MessageBoxButton buttons, MessageBoxImage icon);
    }
}
