// IWindowService.cs
// -------------------------------------------------------------
// Abstrakce pro otevírání oken (např. AboutWindow).
// ViewModel nesmí znát konkrétní WPF okna.
// -------------------------------------------------------------

using ElektroOffer_app.Models;
using ElektroOffer_app.Invoice.Models;

namespace ElektroOffer_app.Services
{
    /// <summary>
    /// Otevírá aplikační okna a převádí jejich výsledky zpět do ViewModelu.
    /// </summary>
    public interface IWindowService
    {
        void ShowAbout();
        void ShowSettings();
        InvoiceDraft? ShowInvoice(IEnumerable<BudgetItem> budgetItems, InvoiceDraft? savedDraft);
    }
}
