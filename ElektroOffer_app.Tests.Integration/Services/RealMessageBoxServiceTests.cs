using System.Threading;
using NUnit.Framework;
using System.Windows;
using ElektroOffer_app.Services.Implementations;

namespace ElektroOffer_app.Tests.Integration.Services
{
    // ============================================================================
    // 🧪 INTEGRATION TEST – RealMessageBoxService
    // ----------------------------------------------------------------------------
    // MessageBox.Show:
    //  • vyžaduje STA thread
    //  • v testovacím prostředí se NEotevře
    //  • vrátí defaultní hodnotu (většinou OK)
    // ============================================================================

    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class RealMessageBoxServiceTests
    {
        [Test]
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
