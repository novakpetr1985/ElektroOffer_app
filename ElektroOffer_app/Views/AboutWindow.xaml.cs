using System.Windows;
using ElektroOffer_app.Services;

namespace ElektroOffer_app.Views
{
    /// <summary>Okno O aplikaci zobrazuje společná metadata aplikace.</summary>
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
            VersionText.Text = $"Verze {ApplicationInfoService.Version}";
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
            => Close();
    }
}
