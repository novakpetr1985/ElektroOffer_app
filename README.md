# ElektroOffer

Desktopová WPF aplikace pro kalkulaci elektro prací a materiálu.

## Požadavky

- Windows 10/11
- Visual Studio 2022+ s workload **.NET Desktop Development**
- .NET 10 SDK

## Jak spustit

1. Otevři `ElektroOffer_app.slnx` ve Visual Studio
2. Sestav projekt — `Ctrl+Shift+B`
3. Spusť — `F5`

Databáze `elektrooffer.db` se vytvoří automaticky při prvním spuštění.  
Tabulky jsou při prvním spuštění prázdné — ceník importuj přes  
**Soubor → Import ceníku** (soubor `.eofcat`).

## Hlavní funkce

- Evidence a kalkulace práce (ceník práce – tabulka `PriceItems`)
- Evidence a kalkulace materiálu (tabulka `Materials`)
- Uložení a načtení projektu do/z souboru (`*.eof`)
// - Export a import ceníku (`*.eofcat`) - nezařazeno do v1.4.1
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

- Aktuální verze aplikace: **1.5.0**
//- Formát exportu ceníku (`CatalogExportData.FormatVersion`): **1.0** - nezařazeno do v1.4.1

> Poznámka: Verzi aplikace je vhodné držet v AssemblyInfo / csproj a číst ji v okně „O aplikaci“.

## Architektura rozhodnutí (ADR)

- **Proč SQLite?** — Desktopová aplikace bez serveru, jednoduchý deployment
- **Proč code-behind místo čistého MVVM?** — Projekt vzniká pro učení, code-behind je srozumitelnější
- **Proč JSON pro .eof soubory?** — Čitelné pro debugging, snadná migrace
