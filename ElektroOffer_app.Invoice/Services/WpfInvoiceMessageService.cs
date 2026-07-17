using System.Windows;

namespace ElektroOffer_app.Invoice.Services;

/// <summary>
/// Zobrazuje fakturační potvrzení pomocí standardního WPF MessageBoxu.
/// </summary>
public sealed class WpfInvoiceMessageService : IInvoiceMessageService
{
    public bool ShowYesNo(string message, string title)
        => MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes;

    public MessageBoxResult ShowYesNoCancel(string message, string title)
        => MessageBox.Show(message, title, MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
}
