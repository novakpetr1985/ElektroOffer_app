// PrintService.cs
// -------------------------------------------------------------
// Implementace IPrintService – tisk textu pomocí WPF PrintDialog.
// ViewModel předá text, služba provede tisk.
// -------------------------------------------------------------

using System.Windows;
using System.Windows.Controls;        // ⚠ PrintDialog
using System.Windows.Documents;       // FlowDocument, Paragraph, Run
using System.Windows.Media;           // FontFamily
using ElektroOffer_app.Services;

namespace ElektroOffer_app.Services.Implementations
{
    public class PrintService : IPrintService
    {
        public void Print(string text)
        {
            var dialog = new PrintDialog();
            if (dialog.ShowDialog() != true)
                return;

            var flowDoc = new FlowDocument(
                new Paragraph(new Run(text)))
            {
                FontFamily = new FontFamily("Consolas"),
                FontSize = 12,
                PagePadding = new Thickness(50)
            };

            dialog.PrintDocument(
                ((IDocumentPaginatorSource)flowDoc).DocumentPaginator,
                "ElektroOffer – Kalkulace");
        }
    }
}
