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
            // 🔢 NAČTENÍ VERZE APLIKACE (NET 10 – suffixy přes <Version>)
            // =========================================================
            // .NET 10 preview ignoruje AssemblyInformationalVersion,
            // ale respektuje hodnotu <Version> včetně suffixů:
            //
            //     <Version>1.7.5-dev</Version>
            //
            // ProductVersion pak vypadá takto:
            //
            //     1.7.5-dev+b5f108402e3003471b7eda7a3a626b7edc0cd879
            //
            // Proto se zobrazuje pouze část před znakem '+', tedy:
            //
            //     1.7.5-dev
            //
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
        // ✅ ZAVŘENÍ OKNA
        // =========================================================

        /// <summary>
        /// Tlačítko Zavřít — zavře okno O aplikaci.
        /// </summary>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
            => Close();
    }
}
