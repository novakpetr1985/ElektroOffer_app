// IApplicationService.cs
// -------------------------------------------------------------
// Abstrakce pro ukončení aplikace.
// ViewModel nesmí volat Application.Current.Shutdown().
// -------------------------------------------------------------

namespace ElektroOffer_app.Services
{
    /// <summary>
    /// Zpřístupňuje životní cyklus desktopové aplikace bez přímé závislosti ViewModelu na WPF.
    /// </summary>
    public interface IApplicationService
    {
        void Shutdown();
    }
}
