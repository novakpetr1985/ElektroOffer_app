using ElektroOffer_app.Models;
using Microsoft.Win32;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace ElektroOffer_app.Services
{
    /// <summary>
    /// Servisní třída zodpovědná za:
    /// - Uložení projektu do JSON souboru (Save / Save As)
    /// - Načtení projektu z JSON souboru (Load)
    /// - Reset projektu (Nový projekt)
    ///
    /// Tato třída NEVÍ nic o UI — pouze pracuje s daty (ProjectData).
    /// Veškeré propojení s UI (ObservableCollection atd.) se děje v MainWindow.
    /// </summary>
    public class ProjectService
    {
        // =========================================================
        // ⚙️ NASTAVENÍ SERIALIZACE
        // =========================================================

        /// <summary>
        /// Možnosti pro System.Text.Json serializátor.
        /// WriteIndented = true → JSON je čitelný pro lidi (pěkně odsazený).
        /// </summary>
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true
        };

        // =========================================================
        // 💾 SAVE — uložení na aktuální cestu (Ctrl+S)
        // =========================================================

        /// <summary>
        /// Uloží projekt na aktuální cestu.
        /// Pokud cesta ještě neexistuje (první uložení), zavolá SaveAs.
        /// </summary>
        /// <param name="data">Data projektu k uložení</param>
        /// <param name="currentPath">Aktuální cesta souboru (null = ještě neuloženo)</param>
        /// <returns>Cesta, kam se uložilo (nebo null při zrušení dialogu)</returns>
        public string? Save(ProjectData data, string? currentPath)
        {
            // Pokud ještě nemáme cestu → chová se jako Save As
            if (string.IsNullOrEmpty(currentPath))
                return SaveAs(data);

            return SaveToPath(data, currentPath);
        }

        // =========================================================
        // 💾 SAVE AS — uložení s výběrem cesty (Ctrl+Shift+S)
        // =========================================================

        /// <summary>
        /// Otevře dialog pro výběr cesty a uloží projekt do nového souboru.
        /// </summary>
        /// <param name="data">Data projektu k uložení</param>
        /// <returns>Cesta, kam se uložilo (nebo null při zrušení dialogu)</returns>
        public string? SaveAs(ProjectData data)
        {
            // Otevření dialogu pro uložení souboru
            var dialog = new SaveFileDialog
            {
                Title = "Uložit projekt",
                Filter = "Projekt ElektroOffer (*.eof)|*.eof|Všechny soubory (*.*)|*.*",
                DefaultExt = ".eof",
                FileName = data.ProjectName
            };

            // Pokud uživatel zruší dialog → vrátíme null
            if (dialog.ShowDialog() != true)
                return null;

            return SaveToPath(data, dialog.FileName);
        }

        // =========================================================
        // 📂 LOAD — načtení projektu ze souboru
        // =========================================================

        /// <summary>
        /// Otevře dialog pro výběr souboru a načte projekt z JSON.
        /// </summary>
        /// <returns>
        /// Tuple: (načtená data, cesta k souboru).
        /// Vrátí (null, null) pokud uživatel zruší dialog nebo nastane chyba.
        /// </returns>
        public (ProjectData? data, string? path) Load()
        {
            // Otevření dialogu pro výběr souboru
            var dialog = new OpenFileDialog
            {
                Title = "Načíst projekt",
                Filter = "Projekt ElektroOffer (*.eof)|*.eof|Všechny soubory (*.*)|*.*"
            };

            // Pokud uživatel zruší dialog → vrátíme null
            if (dialog.ShowDialog() != true)
                return (null, null);

            try
            {
                // Přečtení JSON souboru a deserializace
                var json = File.ReadAllText(dialog.FileName);
                var data = JsonSerializer.Deserialize<ProjectData>(json, _jsonOptions);

                if (data == null)
                {
                    MessageBox.Show("Soubor nelze načíst — pravděpodobně poškozený formát.",
                        "Chyba načítání", MessageBoxButton.OK, MessageBoxImage.Error);
                    return (null, null);
                }

                return (data, dialog.FileName);
            }
            catch (Exception ex)
            {
                // Chyba při čtení nebo parsování JSON
                MessageBox.Show($"Chyba při načítání souboru:\n{ex.Message}",
                    "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
                return (null, null);
            }
        }

        // =========================================================
        // 🆕 NEW PROJECT — kontrola neuložených změn
        // =========================================================

        /// <summary>
        /// Zeptá se uživatele, zda chce uložit změny před resetem.
        /// </summary>
        /// <returns>
        /// True  = pokračovat (uloženo nebo uživatel zvolil "Ne")
        /// False = uživatel zrušil akci (kliknul "Storno")
        /// </returns>
        public bool ConfirmNewProject(ProjectData data, string? currentPath, bool hasUnsavedChanges)
        {
            // Pokud nejsou neuložené změny → rovnou pokračujeme
            if (!hasUnsavedChanges)
                return true;

            var result = MessageBox.Show(
                "Projekt obsahuje neuložené změny.\nChcete je uložit před vytvořením nového projektu?",
                "Neuložené změny",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Warning);

            return result switch
            {
                // Uložit a pokračovat
                MessageBoxResult.Yes => Save(data, currentPath) != null,

                // Bez uložení pokračovat
                MessageBoxResult.No => true,

                // Zrušit akci
                _ => false
            };
        }

        // =========================================================
        // 🔒 PRIVÁTNÍ: Zápis JSON na disk
        // =========================================================

        /// <summary>
        /// Interní metoda — serializuje data do JSON a zapíše na disk.
        /// </summary>
        private string? SaveToPath(ProjectData data, string path)
        {
            try
            {
                // Aktualizace data uložení
                data.SavedAt = DateTime.Now;

                // Serializace do JSON stringu
                var json = JsonSerializer.Serialize(data, _jsonOptions);

                // Zápis na disk
                File.WriteAllText(path, json);

                return path;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Chyba při ukládání souboru:\n{ex.Message}",
                    "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }
    }
}
