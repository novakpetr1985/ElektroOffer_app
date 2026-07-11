using System;
using System.IO;
using System.Text.Json;
using ElektroOffer_app.Models;

namespace ElektroOffer_app.Services
{
    // ============================================================================
    // 🏢 SupplierSettingsService – dočasná persistence údajů dodavatele
    // ----------------------------------------------------------------------------
    // Účel:
    //   • Ukládá/načítá SupplierSettings (trvalé údaje o firmě) do jednoho
    //     pevného JSON souboru v uživatelském adresáři appky.
    //   • Na rozdíl od projektových souborů (FileProjectStorage) si uživatel
    //     cestu nevybírá – je vždy stejná, appka si ji spravuje sama.
    //
    // 🔴 DOČASNÉ ŘEŠENÍ:
    //   • Tahle služba vznikla, aby fakturace mohla fungovat ještě před
    //     dokončením SettingsWindow (kde bude nastavení dodavatele součástí
    //     širší obrazovky Možností).
    //   • Až bude SettingsWindow hotové, tahle třída se buď stane jeho
    //     interní persistenční vrstvou (beze změny), nebo se přejmenuje/
    //     rozšíří o další sekce nastavení (např. barevné téma) – rozhraní
    //     Load/Save zůstane stejné.
    //
    // Umístění souboru:
    //   • %AppData%\ElektroOffer\supplier-settings.json
    //   • %AppData% je standardní uživatelský adresář pro app data na Windows
    //     (např. C:\Users\Petr\AppData\Roaming), nezávisí na tom, odkud appka
    //     běží, a přežije i přeinstalaci appky.
    // ============================================================================
    public class SupplierSettingsService
    {
        private static readonly string FolderPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ElektroOffer");

        private static readonly string FilePath = Path.Combine(FolderPath, "supplier-settings.json");

        // ------------------------------------------------------------------
        // 📥 Load – načte uložená data, nebo vrátí prázdný výchozí objekt
        // ------------------------------------------------------------------
        // Pokud soubor ještě neexistuje (první spuštění appky), vrací nový
        // SupplierSettings s výchozími hodnotami – volající kód se tedy
        // nemusí starat o null a UI zobrazí prázdný formulář k vyplnění.
        // ------------------------------------------------------------------
        public SupplierSettings Load()
        {
            if (!File.Exists(FilePath))
                return new SupplierSettings();

            var json = File.ReadAllText(FilePath);
            return JsonSerializer.Deserialize<SupplierSettings>(json) ?? new SupplierSettings();
        }

        // ------------------------------------------------------------------
        // 💾 Save – uloží data, vytvoří adresář, pokud neexistuje
        // ------------------------------------------------------------------
        public void Save(SupplierSettings settings)
        {
            Directory.CreateDirectory(FolderPath);

            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(FilePath, json);
        }
    }
}