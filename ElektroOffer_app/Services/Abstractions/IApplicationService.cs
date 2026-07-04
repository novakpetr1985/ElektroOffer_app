// IApplicationService.cs
// -------------------------------------------------------------
// Abstrakce pro ukončení aplikace.
// ViewModel nesmí volat Application.Current.Shutdown().
// -------------------------------------------------------------

namespace ElektroOffer_app.Services
{
    public interface IApplicationService
    {
        void Shutdown();
    }
}
