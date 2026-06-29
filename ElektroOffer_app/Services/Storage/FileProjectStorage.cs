using ElektroOffer_app.Models;
using System;
using System.IO;
using System.Text.Json;

namespace ElektroOffer_app.Services.Storage
{
    /// <summary>
    /// KONKRÉTNÍ implementace ukládání na disk (.eof).
    /// 
    /// Hlavní zásady:
    /// - Žádné UI volání (MessageBox) v této vrstvě.
    /// - V případě chyby vyhazujeme výjimky, aby nadřazená vrstva (ProjectService / DialogService) rozhodla, jak uživatele informovat.
    /// - Atomický zápis: nejprve do temp souboru, poté přesun/přepsání cílového souboru.
    /// - Jednoduché, testovatelné API: Save, Load, Exists, Delete.
    /// </summary>
    public class FileProjectStorage : IProjectStorage
    {
        // ------------------------------------------------------------
        // JSON nastavení (sdílené pro Save i Load)
        // ------------------------------------------------------------
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true
        };

        // ------------------------------------------------------------
        // SAVE
        // ------------------------------------------------------------
        /// <summary>
        /// Uloží projekt na zadanou cestu. Vrací absolutní cestu k uloženému souboru.
        /// 
        /// Chování:
        /// - Validuje vstupy.
        /// - Aktualizuje metadata (SavedAt).
        /// - Serializuje do JSON.
        /// - Zapíše atomicky: do temp souboru ve stejném adresáři a poté přejmenuje/přepíše cílový soubor.
        /// - V případě chyby propouští výjimky (IOException, UnauthorizedAccessException, ArgumentException apod.).
        /// 
        /// Poznámka pro testy:
        /// - Testy by měly ověřit, že soubor vznikne, obsahuje očekávaný JSON a že Save vrací absolutní cestu.
        /// - Testy pro přepis: uložit, změnit data, uložit znovu a ověřit obsah.
        /// </summary>
        public string Save(ProjectData data, string path)
        {
            // Validace vstupu
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Path must be provided", nameof(path));

            // Aktualizace metadat projektu (můžeme později injektovat IDateTimeProvider pro testovatelnost)
            data.SavedAt = DateTime.Now;

            // Serializace do JSON (může vyhodit JsonException)
            var json = JsonSerializer.Serialize(data, _jsonOptions);

            // Zajistíme existenci adresáře cílové cesty
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Vytvoříme temp soubor ve stejném adresáři, aby přesun/replace byl atomický na stejné FS
            var tempFileName = $"{Path.GetFileName(path)}.{Guid.NewGuid():N}.tmp";
            var tempFilePath = Path.Combine(string.IsNullOrEmpty(directory) ? Path.GetTempPath() : directory, tempFileName);

            // Zápis do temp souboru (může vyhodit IOException / UnauthorizedAccessException)
            File.WriteAllText(tempFilePath, json);

            // Pokud cílový soubor existuje, pokusíme se použít File.Replace pro atomické přepsání.
            // Pokud Replace není podporováno nebo selže, File.Move s přepsáním je fallback.
            try
            {
                if (File.Exists(path))
                {
                    // File.Replace může vyhodit IOException; necháme výjimku propadnout, aby volající věděl, co se stalo.
                    File.Replace(tempFilePath, path, null);
                }
                else
                {
                    // Pokud cílový soubor neexistuje, přesuneme temp soubor na cílovou cestu.
                    File.Move(tempFilePath, path);
                }
            }
            catch
            {
                // Pokud dojde k chybě při přesunu/přepsání, pokusíme se temp soubor odstranit (cleanup).
                // Nechceme potlačit původní výjimku, proto ji znovu vyhodíme po cleanup.
                try
                {
                    if (File.Exists(tempFilePath))
                    {
                        File.Delete(tempFilePath);
                    }
                }
                catch
                {
                    // Ignorujeme chybu cleanupu; původní výjimka je důležitější.
                }

                // Znovu vyhodíme původní výjimku (propagujeme ji volajícímu).
                throw;
            }

            // Vracíme absolutní cestu pro konzistenci
            return Path.GetFullPath(path);
        }

        // ------------------------------------------------------------
        // LOAD
        // ------------------------------------------------------------
        /// <summary>
        /// Načte projekt ze zadané cesty. Vrací tuple (data, absolutní cesta).
        /// 
        /// Chování:
        /// - Validuje vstup.
        /// - Pokud soubor neexistuje, vyhodí FileNotFoundException.
        /// - Načte text, deserializuje JSON do ProjectData.
        /// - Pokud deserializace vrátí null, vyhodí InvalidDataException.
        /// - V případě jiných chyb (I/O, JSON) propouští příslušné výjimky.
        /// </summary>
        public (ProjectData data, string path) Load(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Path must be provided", nameof(path));
            if (!File.Exists(path)) throw new FileNotFoundException("Project file not found", path);

            // Načtení obsahu souboru (může vyhodit IOException)
            var json = File.ReadAllText(path);

            // Deserializace (může vyhodit JsonException)
            var data = JsonSerializer.Deserialize<ProjectData>(json, _jsonOptions);

            if (data == null)
            {
                // Pokud deserializace vrátí null, považujeme soubor za poškozený/nesprávného formátu
                throw new InvalidDataException("Project file contains invalid data.");
            }

            return (data, Path.GetFullPath(path));
        }

        // ------------------------------------------------------------
        // EXISTS
        // ------------------------------------------------------------
        /// <summary>
        /// Vrátí true, pokud soubor existuje na disku.
        /// </summary>
        public bool Exists(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return false;
            return File.Exists(path);
        }

        // ------------------------------------------------------------
        // DELETE
        // ------------------------------------------------------------
        /// <summary>
        /// Smaže soubor, pokud existuje. V případě chyby (např. přístup odepřen) vyhodí výjimku.
        /// </summary>
        public void Delete(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Path must be provided", nameof(path));
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}
