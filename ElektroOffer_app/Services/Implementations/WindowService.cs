// WindowService.cs
// -------------------------------------------------------------
// Implementace IWindowService – otevírání AboutWindow.
// ViewModel nezná AboutWindow, volá jen tuto službu.
// -------------------------------------------------------------

using ElektroOffer_app.Services;
using ElektroOffer_app.Invoice.Models;
using ElektroOffer_app.Invoice.Views;
using ElektroOffer_app.Views;   // ⚠ Namespace, kde máš AboutWindow

namespace ElektroOffer_app.Services.Implementations
{
    /// <summary>
    /// Otevírá nastavení, informace a fakturační modul a mapuje rozpočet do fakturačních položek.
    /// </summary>
    public class WindowService : IWindowService
    {
        public void ShowAbout()
        {
            var about = new AboutWindow();
            about.ShowDialog();
        }

        public void ShowSettings()
        {
            var settings = new SettingsWindow(App.ThemeService);
            settings.ShowDialog();
        }

        public InvoiceDraft? ShowInvoice(IEnumerable<Models.BudgetItem> budgetItems, InvoiceDraft? savedDraft)
        {
            var sourceItems = budgetItems.Select(item => new InvoiceSourceItem
            {
                Type = item.Type,
                Description = item.Description,
                Unit = item.Unit,
                Quantity = item.Quantity,
                PriceBeforeDiscount = item.PriceBeforeDiscount,
                Price = item.Price,
                DiscountPercent = item.DiscountPercent,
                DiscountAmount = item.DiscountAmount
            }).ToList();

            var invoice = new InvoiceWindow(sourceItems, savedDraft);
            return invoice.ShowDialog() == true
                ? invoice.SavedDraft
                : null;
        }
    }
}
