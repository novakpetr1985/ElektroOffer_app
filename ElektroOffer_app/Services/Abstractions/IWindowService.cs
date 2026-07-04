// IWindowService.cs
// -------------------------------------------------------------
// Abstrakce pro otevírání oken (např. AboutWindow).
// ViewModel nesmí znát konkrétní WPF okna.
// -------------------------------------------------------------

namespace ElektroOffer_app.Services
{
    public interface IWindowService
    {
        void ShowAbout();
    }
}
