using ElektroOffer_app.Services.Abstractions;
using Microsoft.Win32;

namespace ElektroOffer_app.Services.Implementations
{
    // ========================================================================
    // 🧩 RealFileDialogService
    // Skutečná implementace dialogů pro ukládání a načítání souborů.
    //
    // ÚČEL:
    // - Poskytuje reálné WPF dialogy (OpenFileDialog / SaveFileDialog)
    // - Používá se v INTEGRATION TESTECH (testujeme skutečné chování)
    // - Unit testy používají Mock<IFileDialogService>
    //
    // DŮLEŽITÉ:
    // - Třída neobsahuje žádnou logiku ProjectService
    // - Pouze zobrazuje dialogy a vrací cestu k souboru
    // ========================================================================
    public class RealFileDialogService : IFileDialogService
    {
        // --------------------------------------------------------------------
        // 📂 Otevření souboru
        // --------------------------------------------------------------------
        public string? ShowOpenFileDialog(string filter, string title)
        {
            var dialog = new OpenFileDialog
            {
                Filter = filter,
                Title = title
            };

            // Vrací cestu nebo null (uživatel zrušil)
            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }

        // --------------------------------------------------------------------
        // 💾 Uložení souboru
        // --------------------------------------------------------------------
        public string? ShowSaveFileDialog(string filter, string title, string defaultExt, string fileName)
        {
            var dialog = new SaveFileDialog
            {
                Filter = filter,
                Title = title,
                DefaultExt = defaultExt,
                FileName = fileName
            };

            // Vrací cestu nebo null (uživatel zrušil)
            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }
    }
}
