﻿using ElektroOffer_app.Models;
using ElektroOffer_app.Services.Storage;
using Microsoft.Win32;
using System.Windows;

namespace ElektroOffer_app.Services
{
    // ========================================================================
    // ProjectService – HLAVNÍ ORCHESTRÁTOR APLIKACE
    // ========================================================================
    //
    // 🧠 CO TATO TŘÍDA DĚLÁ:
    // - Řídí ukládání a načítání projektu
    // - Řeší UI dialogy (zatím zde, později DialogService)
    // - Koordinuje storage vrstvu
    //
    // ❌ CO TATO TŘÍDA NESMÍ DĚLAT:
    // - File I/O (File.Read/Write)
    // - JSON serializaci
    // - přímou práci se soubory
    //
    // 👉 Tohle vše je přesunuto do IProjectStorage / FileProjectStorage
    //
    // 🔵 KROK 4 — CO SE ZMĚNILO:
    // - konstruktor už NEVYTVÁŘÍ FileProjectStorage sám (new FileProjectStorage())
    // - místo toho PŘIJÍMÁ hotovou instanci zvenku (Dependency Injection)
    // - výhoda: v testech lze předat mock, v aplikaci FileProjectStorage
    // - MainWindow nyní rozhoduje CO se předá (viz MainWindow.xaml.cs)
    //
    // ========================================================================
    public class ProjectService
    {
        // =========================================================
        // STORAGE VRSTVA (ABSTRAKCE)
        // =========================================================
        //
        // 🧠 PROČ TU JE:
        // - ProjectService už neřeší "JAK" se ukládá
        // - jen říká "ULOŽ / NAČTI"
        // - umožňuje budoucí výměnu (DB, API, cloud)
        //
        // 🔵 KROK 4 — ZMĚNA:
        // - pole zůstává stejné (IProjectStorage _storage)
        // - ale hodnota se teď PŘIŘAZUJE z konstruktoru (ne new FileProjectStorage())
        // - díky tomu je závislost "injektovaná" = Dependency Injection
        private readonly IProjectStorage _storage;

        // =========================================================
        // KONSTRUKTOR — DEPENDENCY INJECTION
        // =========================================================
        //
        // 🧠 CO SE DĚJE:
        // - konstruktor přijme HOTOVOU instanci IProjectStorage
        // - ProjectService si ji uloží a používá
        // - ProjectService NEVÍ co konkrétně dostane (FileProjectStorage? Mock? DB?)
        // - to je záměr — třída závisí jen na rozhraní, ne na implementaci
        //
        // 🔵 KROK 4 — ZMĚNA OPROTI PŘEDCHOZÍMU STAVU:
        // ❌ BYLO:
        //    public ProjectService()
        //    {
        //        _storage = new FileProjectStorage();   ← hardcoded závislost
        //    }
        //
        // ✔ JE:
        //    public ProjectService(IProjectStorage storage)
        //    {
        //        _storage = storage;                    ← přichází zvenku
        //    }
        //
        // 👉 KDO PŘEDÁVÁ STORAGE?
        // - MainWindow.xaml.cs → new ProjectService(new FileProjectStorage())
        // - testy → new ProjectService(new MockStorage())   ← budoucí krok 5
        public ProjectService(IProjectStorage storage)
        {
            // ✔ uložíme si co nám přišlo — budeme to používat v Save/Load
            _storage = storage;
        }

        // =========================================================
        // SAVE (Ctrl+S)
        // =========================================================
        //
        // 🧠 LOGIKA:
        // - pokud projekt ještě nemá cestu → SaveAs
        // - jinak uloží přímo na existující cestu
        public string? Save(ProjectData data, string? currentPath)
        {
            // ❗ nový projekt bez cesty → musíme vyvolat Save As
            if (string.IsNullOrEmpty(currentPath))
                return SaveAs(data);

            // ✔ uloží přes storage (už ne File.WriteAllText!)
            return _storage.Save(data, currentPath);
        }

        // =========================================================
        // SAVE AS (Ctrl+Shift+S)
        // =========================================================
        //
        // 🧠 LOGIKA:
        // - vždy se zobrazí dialog pro výběr souboru
        // - uživatel určí umístění a jméno
        public string? SaveAs(ProjectData data)
        {
            var dialog = new SaveFileDialog
            {
                Title = "Uložit projekt",

                // 🧠 formát projektu (.eof = vlastní formát aplikace)
                Filter = "Projekt ElektroOffer (*.eof)|*.eof",
                DefaultExt = ".eof",

                // 🧠 předvyplněný název podle projektu
                FileName = data.ProjectName
            };

            // ❗ pokud uživatel zruší dialog nebo FileName není platná cesta → vrátit null
            if (dialog.ShowDialog() != true || string.IsNullOrWhiteSpace(dialog.FileName))
                return null;


            // ✔ delegace na storage vrstvu
            return _storage.Save(data, dialog.FileName);
        }

        // =========================================================
        // LOAD (Ctrl+O)
        // =========================================================
        //
        // 🧠 LOGIKA:
        // - otevře dialog pro výběr souboru
        // - načtení probíhá ve storage vrstvě
        public (ProjectData? data, string? path) Load()
        {
            var dialog = new OpenFileDialog
            {
                Title = "Načíst projekt",
                Filter = "Projekt ElektroOffer (*.eof)|*.eof"
            };

            // ❗ uživatel zrušil výběr souboru
            if (dialog.ShowDialog() != true)
                return (null, null);

            // ✔ veškeré file + JSON zpracování je ve storage
            return _storage.Load(dialog.FileName);
        }

        // =========================================================
        // NEULOŽENÉ ZMĚNY – ochrana proti ztrátě dat
        // =========================================================
        //
        // 🧠 ÚČEL:
        // - pokud má projekt změny → upozornit uživatele
        // - rozhodnutí je na uživateli (Yes / No / Cancel)
        public bool ConfirmNewProject(ProjectData data, string? currentPath, bool hasUnsavedChanges)
        {
            // ✔ pokud nic není změněno → pokračuj bez dotazu
            if (!hasUnsavedChanges)
                return true;

            var result = MessageBox.Show(
                "Projekt obsahuje neuložené změny.\nUložit před vytvořením nového projektu?",
                "Neuložené změny",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Warning);

            return result switch
            {
                // ✔ uloží projekt a pokračuje
                MessageBoxResult.Yes => Save(data, currentPath) != null,

                // ✔ zahodí změny a pokračuje
                MessageBoxResult.No => true,

                // ❌ zruší akci
                _ => false
            };
        }

        // =========================================================
        // EXPORT CENÍKU (.eofcat)
        // =========================================================
        //
        // 🧠 ÚČEL:
        // - export dat do souboru
        // - zatím stále obsahuje JSON + File (KROK 6–7 refactor)
        public bool ExportCatalog(CatalogExportData data)
        {
            var dialog = new SaveFileDialog
            {
                Title = "Exportovat ceník",

                // 🧠 vlastní formát exportu
                Filter = "Ceník ElektroOffer (*.eofcat)|*.eofcat",

                DefaultExt = ".eofcat",

                // 🧠 defaultní název s datem
                FileName = $"cenik_export_{DateTime.Now:yyyy-MM-dd}"
            };

            if (dialog.ShowDialog() != true)
                return false;

            try
            {
                // 🧠 metadata exportu
                data.ExportedAt = DateTime.Now;

                // ⚠️ zatím ponecháno (refactor přijde v KROK 6–7)
                var json = System.Text.Json.JsonSerializer.Serialize(
                    data,
                    new System.Text.Json.JsonSerializerOptions
                    {
                        WriteIndented = true
                    });

                System.IO.File.WriteAllText(dialog.FileName, json);

                MessageBox.Show(
                    $"Export dokončen.\n\nPráce: {data.PriceItems.Count}\nMateriál: {data.Materials.Count}",
                    "Hotovo",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Chyba exportu:\n{ex.Message}",
                    "Chyba",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                return false;
            }
        }

        // =========================================================
        // IMPORT CENÍKU (.eofcat)
        // =========================================================
        //
        // 🧠 ÚČEL:
        // - načtení exportovaného ceníku
        public CatalogExportData? ImportCatalog()
        {
            var dialog = new OpenFileDialog
            {
                Title = "Importovat ceník",
                Filter = "Ceník ElektroOffer (*.eofcat)|*.eofcat"
            };

            // ❗ dialog zrušen nebo neobsahuje platnou cestu → nic nenačítáme
            if (dialog.ShowDialog() != true || string.IsNullOrWhiteSpace(dialog.FileName))
                return null;

            try
            {
                var json = System.IO.File.ReadAllText(dialog.FileName);

                return System.Text.Json.JsonSerializer.Deserialize<CatalogExportData>(
                    json,
                    new System.Text.Json.JsonSerializerOptions
                    {
                        WriteIndented = true
                    });
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Chyba importu:\n{ex.Message}",
                    "Chyba",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                return null;
            }
        }
    }
}