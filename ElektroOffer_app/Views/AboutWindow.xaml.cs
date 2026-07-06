using System.Reflection;
using System.Windows;

namespace ElektroOffer_app.Views   // ⚠ MUSÍ být přesně tento namespace
{
    /// <summary>
    /// Okno "O aplikaci" — zobrazí logo, verzi a autora.
    /// Otevírá se z menu Nápověda → O aplikaci.
    /// </summary>
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();   // ⚠ Funguje jen pokud XAML má správný x:Class

            // =========================================================
            // 🔢 NAČTENÍ VERZE APLIKACE (NET 10 – suffixy přes <Version>)
            // =========================================================

            var assembly = Assembly.GetExecutingAssembly();

            // Načteme ProductVersion (obsahuje suffix + hash)
            var info = assembly
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                ?.InformationalVersion;

            // Odstraníme automatický +commit_hash
            var cleanInfo = info?.Split('+')[0];

            if (!string.IsNullOrWhiteSpace(cleanInfo))
            {
                VersionText.Text = $"Verze {cleanInfo}";
                return;
            }

            // Fallback (neměl by nastat)
            VersionText.Text = "Verze neznámá";
        }

        // =========================================================
        // 🔘 ZAVŘENÍ OKNA
        // =========================================================
        private void CloseButton_Click(object sender, RoutedEventArgs e)
            => Close();
    }
}
