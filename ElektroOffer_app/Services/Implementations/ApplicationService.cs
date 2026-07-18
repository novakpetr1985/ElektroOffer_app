// ApplicationService.cs
// -------------------------------------------------------------
// Implementace IApplicationService – ukončení aplikace.
// ViewModel nevolá Application.Current.Shutdown().
// -------------------------------------------------------------

using System.Windows;
using ElektroOffer_app.Services;

namespace ElektroOffer_app.Services.Implementations
{
    /// <summary>
    /// Produkční WPF implementace ukončení aplikace.
    /// </summary>
    public class ApplicationService : IApplicationService
    {
        public void Shutdown()
        {
            Application.Current.Shutdown();
        }
    }
}
