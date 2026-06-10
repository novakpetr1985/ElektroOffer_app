# ElektroOffer

Desktopová WPF aplikace pro kalkulaci elektro prací a materiálu.

## Hlavní funkce

- Evidence a kalkulace práce (ceník práce – tabulka `PriceItems`)
- Evidence a kalkulace materiálu (tabulka `Materials`)
- Uložení a načtení projektu do/z souboru (`*.eof`)
- Export a import ceníku (`*.eofcat`)
- Přehledný rozpis (práce + materiál) s celkovou cenou

## Technický přehled

- **Platforma:** .NET (WPF)
- **UI:** XAML (MainWindow, AboutWindow, Resources/Styles)
- **Databáze:** SQLite přes Entity Framework Core (`AppDbContext`)
- **Modely:**
  - `PriceItems` – ceník práce
  - `Material` – ceník materiálu
  - `ProjectData` – uložený projekt (práce + materiál)
  - `CatalogExportData` – export/import ceníku
- **Služby:**
  - `ProjectService` – Save/Load projektů, export/import ceníku
  - `DialogService` – zobrazení MessageBox dialogů

## Struktura projektu

- `App.xaml`, `App.xaml.cs` – start aplikace
- `Views/` – okna (např. `MainWindow`, `AboutWindow`)
- `Models/` – datové třídy (projekty, položky, ceníky)
- `Data/` – EF Core kontext (`AppDbContext`)
- `Services/` – logika pro práci se soubory a dialogy
- `Resources/` – barvy, styly

## Verze

- Aktuální verze aplikace: **1.4.0**
- Formát exportu ceníku (`CatalogExportData.FormatVersion`): **1.0**

> Poznámka: Verzi aplikace je vhodné držet v AssemblyInfo / csproj a číst ji v okně „O aplikaci“.
