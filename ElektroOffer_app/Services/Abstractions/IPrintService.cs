// IPrintService.cs
// -------------------------------------------------------------
// Abstrakce pro tisk.
// ViewModel předá text → služba provede tisk.
// -------------------------------------------------------------

namespace ElektroOffer_app.Services
{
    /// <summary>
    /// Předá textový dokument systémovému dialogu Windows pro tisk nebo Print to PDF.
    /// </summary>
    public interface IPrintService
    {
        void Print(string text);
    }
}
