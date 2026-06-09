using System.Windows;

namespace ElektroOffer_app.Services
{
    // =========================================================
    // 💬 DIALOG SERVICE
    // =========================================================
    // 👉 Jednoduchý servis pro zobrazování MessageBox dialogů
    // 👉 Umožňuje ViewModelům volat dialogy bez přímé závislosti na UI
    // =========================================================
    public class DialogService
    {
        // =========================================================
        // ℹ INFO DIALOG
        // =========================================================
        /// <summary>
        /// Zobrazí informační dialog s tlačítkem OK.
        /// </summary>
        /// <param name="message">Text zprávy.</param>
        /// <param name="title">Titulek okna.</param>
        public void ShowInfo(string message, string title = "Informace")
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // =========================================================
        // ⚠ WARNING DIALOG
        // =========================================================
        /// <summary>
        /// Zobrazí varovný dialog s tlačítkem OK.
        /// </summary>
        /// <param name="message">Text zprávy.</param>
        /// <param name="title">Titulek okna.</param>
        public void ShowWarning(string message, string title = "Upozornění")
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        // =========================================================
        // ❌ ERROR DIALOG
        // =========================================================
        /// <summary>
        /// Zobrazí chybový dialog s tlačítkem OK.
        /// </summary>
        /// <param name="message">Text zprávy.</param>
        /// <param name="title">Titulek okna.</param>
        public void ShowError(string message, string title = "Chyba")
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        // =========================================================
        // ❓ CONFIRM DIALOG (ANO/NE)
        // =========================================================
        /// <summary>
        /// Zobrazí potvrzovací dialog s tlačítky Ano/Ne.
        /// </summary>
        /// <param name="message">Text zprávy.</param>
        /// <param name="title">Titulek okna.</param>
        /// <returns>True = uživatel zvolil Ano, False = Ne.</returns>
        public bool Confirm(string message, string title = "Potvrzení")
        {
            var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
            return result == MessageBoxResult.Yes;
        }
    }
}
