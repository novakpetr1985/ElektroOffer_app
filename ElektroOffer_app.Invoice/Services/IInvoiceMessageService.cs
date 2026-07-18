namespace ElektroOffer_app.Invoice.Services;

/// <summary>
/// Odděluje potvrzovací dialogy fakturačního ViewModelu od WPF MessageBoxu.
/// </summary>
public interface IInvoiceMessageService
{
    bool ShowYesNo(string message, string title);
    System.Windows.MessageBoxResult ShowYesNoCancel(string message, string title);
}
