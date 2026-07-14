using System.Windows;
using ElektroOffer_app.Services;

namespace ElektroOffer_app
{
    /// <summary>
    /// Globální vstup aplikace. Při startu načítá uložený motiv a aplikuje sdílené styly.
    /// </summary>
    public partial class App : Application
    {
        public static AppThemeService ThemeService { get; } = new();

        protected override void OnStartup(StartupEventArgs e)
        {
            ThemeService.LoadAndApply();
            base.OnStartup(e);
        }
    }
}
