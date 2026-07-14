// IWindowService.cs
// -------------------------------------------------------------
// Abstrakce pro otevírání oken (např. AboutWindow).
// ViewModel nesmí znát konkrétní WPF okna.
// -------------------------------------------------------------

using ElektroOffer_app.Models;
using ElektroOffer_app.Invoice.Models;

namespace ElektroOffer_app.Services
{
    public interface IWindowService
    {
        void ShowAbout();
        InvoiceDraft? ShowInvoice(IEnumerable<BudgetItem> budgetItems, InvoiceDraft? savedDraft);
    }
}
