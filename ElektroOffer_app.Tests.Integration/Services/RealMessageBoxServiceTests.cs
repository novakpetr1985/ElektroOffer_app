using System;
using System.Runtime.InteropServices;
using System.Threading;
using NUnit.Framework;
using System.Windows;
using System.Windows.Interop;
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
    // Proč se okno schovávalo / minimalizovalo?
    //  • Test runner NEspouští WPF message loop (Dispatcher.Run)
    //  • Window.Show() vytvoří okno, ale NEaktivuje ho
    //  • Windows ho považuje za „background window“
    //  • Proto se minimalizuje nebo schová pod ostatní aplikace
    //
    // Řešení:
    //  • Vytvořit vlastní Application
    //  • Zobrazit okno
    //  • Aktivovat okno přes Win32 API (SetForegroundWindow)
    //  • Teprve potom volat MessageBox.Show
    //
    // Poznámka:
    //  • Dialog se v testovacím runneru NEotevře (nemá message loop)
    //  • Ale MessageBox.Show stále vrátí validní MessageBoxResult
    // ============================================================================

    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class RealMessageBoxServiceTests
    {
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [Test, Explicit("UI dialog – spouštět pouze lokálně, CI nemá UI prostředí")]
        public void Show_Should_Not_Throw_And_Return_Valid_Result()
        {
            // 🔧 1) Vytvoříme WPF Application, pokud neexistuje
            var app = Application.Current ?? new Application();

            // 🔧 2) Vytvoříme vlastní okno
            var window = new Window
            {
                Title = "Test Owner Window",
                Width = 300,
                Height = 200,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };

            // 🔧 3) Zobrazíme okno
            window.Show();

            // 🔧 4) Aktivujeme okno přes Win32 API → jinak bude minimalizované
            var hwnd = new WindowInteropHelper(window).Handle;
            SetForegroundWindow(hwnd);

            // 🔧 5) Nastavíme MainWindow → MessageBox dostane OWNER
            app.MainWindow = window;

            var service = new RealMessageBoxService();

            Assert.DoesNotThrow(() =>
            {
                // 🔥 6) Zavoláme dialog
                var result = service.Show(
                    "Test message",
                    "Test title",
                    MessageBoxButton.OKCancel,
                    MessageBoxImage.Information
                );

                // 🔍 7) Ověříme validní návratovou hodnotu
                Assert.IsTrue(
                    result == MessageBoxResult.OK ||
                    result == MessageBoxResult.Cancel ||
                    result == MessageBoxResult.Yes ||
                    result == MessageBoxResult.No,
                    "RealMessageBoxService vrátil nevalidní MessageBoxResult."
                );
            });

            // 🔧 8) Zavřeme okno po testu
            window.Close();
        }
    }
}
