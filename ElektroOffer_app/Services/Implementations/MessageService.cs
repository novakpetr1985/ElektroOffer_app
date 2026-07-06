// MessageService.cs
// -------------------------------------------------------------
// Implementace IMessageService pomocí WPF MessageBox.
// ViewModel volá jen tuto službu, ne MessageBox přímo.
// -------------------------------------------------------------

using System.Windows;
using ElektroOffer_app.Services;

namespace ElektroOffer_app.Services.Implementations
{
    public class MessageService : IMessageService
    {
        public bool ShowYesNo(string message, string title)
            => MessageBox.Show(message, title, MessageBoxButton.YesNo) == MessageBoxResult.Yes;

        public MessageBoxResult ShowYesNoCancel(string message, string title)
            => MessageBox.Show(message, title, MessageBoxButton.YesNoCancel);
    }
}
