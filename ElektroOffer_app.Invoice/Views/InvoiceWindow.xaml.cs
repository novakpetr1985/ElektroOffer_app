using System.Windows;
using ElektroOffer_app.Invoice.Models;
using ElektroOffer_app.Invoice.Services;
using ElektroOffer_app.Invoice.ViewModels;

namespace ElektroOffer_app.Invoice.Views
{
    /// <summary>Hostuje fakturační ViewModel a řeší pouze životní cyklus WPF okna.</summary>
    public partial class InvoiceWindow : Window
    {
        public InvoiceWindow(IEnumerable<InvoiceSourceItem> sourceItems, InvoiceDraft? savedDraft = null)
        {
            InitializeComponent();
            var sourceList = sourceItems.ToList();
            if (savedDraft == null && sourceList.Count == 0)
            {
                var recovered = new InvoiceAutosaveService().LoadLatest();
                if (recovered != null && new WpfInvoiceMessageService().ShowYesNo(
                        "Byl nalezen automaticky uložený koncept faktury. Obnovit jej?",
                        "Obnova faktury"))
                    savedDraft = recovered;
            }

            var viewModel = new InvoiceViewModel(sourceList, savedDraft);
            viewModel.SaveToProjectRequested += (_, draft) =>
            {
                SavedDraft = draft;
                DialogResult = true;
            };

            DataContext = viewModel;
        }

        public InvoiceDraft? SavedDraft { get; private set; }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (DataContext is InvoiceViewModel viewModel && !viewModel.ConfirmDiscardChanges())
                e.Cancel = true;

            base.OnClosing(e);
        }
    }
}
