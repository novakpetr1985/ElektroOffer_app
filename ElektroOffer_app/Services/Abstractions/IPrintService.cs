// IPrintService.cs
// -------------------------------------------------------------
// Abstrakce pro tisk.
// ViewModel předá text → služba provede tisk.
// -------------------------------------------------------------

namespace ElektroOffer_app.Services
{
    public interface IPrintService
    {
        void Print(string text);
    }
}
