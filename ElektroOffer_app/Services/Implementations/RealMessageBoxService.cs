using ElektroOffer_app.Services.Abstractions;
using System.Windows;

namespace ElektroOffer_app.Services.Implementations
{
    // ========================================================================
    // 🧩 RealMessageBoxService
    // Skutečná implementace MessageBox dialogů.
    //
    // ÚČEL:
    // - Zobrazuje reálné WPF MessageBox dialogy
    // - Používá se v INTEGRATION TESTECH
    // - Unit testy používají Mock<IMessageBoxService>
    //
    // DŮLEŽITÉ:
    // - Třída neobsahuje žádnou logiku ProjectService
    // - Pouze zobrazuje dialog a vrací MessageBoxResult
    // ========================================================================
    public class RealMessageBoxService : IMessageBoxService
    {
        // --------------------------------------------------------------------
        // ⚠️ Zobrazení MessageBox dialogu
        // --------------------------------------------------------------------
        public MessageBoxResult Show(string text, string caption, MessageBoxButton buttons, MessageBoxImage icon)
        {
            return MessageBox.Show(text, caption, buttons, icon);
        }
    }
}
