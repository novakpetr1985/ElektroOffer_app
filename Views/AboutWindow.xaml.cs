using System.Reflection;
using System.Windows;

namespace ElektroOffer_app
{
    /// <summary>
    /// Okno "O aplikaci" — zobrazí logo, verzi a autora.
    /// Otevírá se z menu Nápověda → O aplikaci.
    /// </summary>
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();

            // =========================================================
            // 🔢 NAČTENÍ VERZE Z ASSEMBLY
            // =========================================================
            // Verze se čte automaticky z .csproj souboru (<Version>x.x.x</Version>).
            // Stačí tedy měnit verzi jen na jednom místě — v .csproj.
            // Formát: "Verze 1.0.0" — major.minor.patch
            // =========================================================

            var version = Assembly
                .GetExecutingAssembly()
                .GetName()
                .Version;

            // Zobrazíme jen major.minor.patch (bez revision čísla za poslední tečkou)
            VersionText.Text = version != null
                ? $"Verze {version.Major}.{version.Minor}.{version.Build}"
                : "Verze neznámá";
        }

        // =========================================================
        // ✅ ZAVŘENÍ OKNA
        // =========================================================

        /// <summary>
        /// Tlačítko Zavřít — zavře okno O aplikaci.
        /// </summary>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
            => Close();
    }
}
