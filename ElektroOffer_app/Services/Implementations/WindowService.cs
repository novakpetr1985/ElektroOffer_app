// WindowService.cs
// -------------------------------------------------------------
// Implementace IWindowService – otevírání AboutWindow.
// ViewModel nezná AboutWindow, volá jen tuto službu.
// -------------------------------------------------------------

using ElektroOffer_app.Services;
using ElektroOffer_app.Views;   // ⚠ Namespace, kde máš AboutWindow

namespace ElektroOffer_app.Services.Implementations
{
    public class WindowService : IWindowService
    {
        public void ShowAbout()
        {
            var about = new AboutWindow();
            about.ShowDialog();
        }
    }
}
