using System.Threading;
using NUnit.Framework;
using System.Windows;
using ElektroOffer_app.Services.Implementations;

namespace ElektroOffer_app.Tests.Integration.Services
{
    // ============================================================================
    // 🧪 INTEGRATION TEST – RealMessageBoxService
    // ----------------------------------------------------------------------------
    // Proč integrační test?
    //  • MessageBox.Show je WPF UI dialog → vyžaduje STA thread
    //  • GitHub Actions běží bez UI → dialog nelze otevřít
    //
    // Proto je test označen jako [Explicit]:
    //  • Lokálně se spustí
    //  • V CI se přeskočí → pipeline se nezasekne
    //
    // Co testujeme:
    //  • Metoda nevyhodí výjimku
    //  • Vrátí validní MessageBoxResult
    //
    // Dialog se NEotevře → testovací runner nemá UI okno.
    // ============================================================================

    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class RealMessageBoxServiceTests
    {
        [Test, Explicit("UI dialog – spouštět pouze lokálně, CI nemá UI prostředí")]
        public void Show_Should_Not_Throw_And_Return_Valid_Result()
        {
            var service = new RealMessageBoxService();

            Assert.DoesNotThrow(() =>
            {
                var result = service.Show(
                    "Test message",
                    "Test title",
                    MessageBoxButton.OKCancel,
                    MessageBoxImage.Information
                );

                Assert.IsTrue(
                    result == MessageBoxResult.OK ||
                    result == MessageBoxResult.Cancel ||
                    result == MessageBoxResult.Yes ||
                    result == MessageBoxResult.No,
                    "RealMessageBoxService vrátil nevalidní MessageBoxResult."
                );
            });
        }
    }
}
