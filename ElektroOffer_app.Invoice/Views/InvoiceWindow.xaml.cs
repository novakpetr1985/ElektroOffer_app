using System.Windows;
using ElektroOffer_app.Invoice.Models;
using ElektroOffer_app.Invoice.ViewModels;

namespace ElektroOffer_app.Invoice.Views
{
    public partial class InvoiceWindow : Window
    {
        public InvoiceWindow(IEnumerable<InvoiceSourceItem> sourceItems, InvoiceDraft? savedDraft = null)
        {
            InitializeComponent();
            var viewModel = new InvoiceViewModel(sourceItems, savedDraft);
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
