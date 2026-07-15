using System.Windows;
using ElektroOffer_app.Services;
using ElektroOffer_app.ViewModels;

namespace ElektroOffer_app.Views
{
    public partial class SettingsWindow : Window
    {
        public SettingsWindow(AppThemeService themeService)
        {
            InitializeComponent();
            DataContext = new SettingsViewModel(themeService);
        }
    }
}
