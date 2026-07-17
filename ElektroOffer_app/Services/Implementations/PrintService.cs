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
    /// <summary>
    /// Vytvoří jednoduchý FlowDocument a odešle jej do systémového PrintDialogu.
    /// </summary>
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
                FontFamily = Token<FontFamily>("Typography.FontFamily.Document", new FontFamily("Arial")),
                FontSize = Token("Typography.FontSize.Document", 10d),
                PagePadding = Token("Spacing.DocumentPagePadding", new Thickness(50))
            };

            dialog.PrintDocument(
                ((IDocumentPaginatorSource)flowDoc).DocumentPaginator,
                "ElektroOffer – Kalkulace");
        }

        private static T Token<T>(string key, T fallback)
            => Application.Current?.TryFindResource(key) is T value ? value : fallback;
    }
}
