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

---

## Hlavní funkce

- Kalkulace práce podle ceníku (tabulka `PriceItems`)
- Kalkulace materiálu (tabulka `Materials`)
- Uložení a načtení projektu (`*.eof`)
- Export a import ceníku (`*.eofcat`)
- Přehledný rozpis položek s celkovou cenou

---

## Technický přehled

- **Platforma:** .NET 10, WPF
- **UI:** XAML (`MainWindow`, `AboutWindow`, `Resources/Styles`)
- **Databáze:** SQLite přes Entity Framework Core (`AppDbContext`)
- **Architektura:** Code-behind s částečným MVVM (viz ADR níže)

### Modely
| Třída | Účel |
|---|---|
| `PriceItems` | Ceník práce |
| `Material` | Ceník materiálu |
| `ProjectData` | Serializovaný projekt (`.eof`) |
| `CatalogExportData` | Export/import ceníku (`.eofcat`) |
| `BudgetItem` | Řádek rozpisu kalkulace |

### Služby
| Třída | Účel |
|---|---|
| `ProjectService` | Ukládání a načítání projektů, export/import ceníku |
| `CatalogService` | Načítání ceníku z DB (testovatelné bez WPF) |
| `DialogService` | Zobrazení MessageBox dialogů |

---

## Struktura projektu

```
ElektroOffer_app.slnx
├── ElektroOffer_app/         – hlavní WPF projekt
│   ├── App.xaml              – vstupní bod aplikace
│   ├── Views/                – okna (MainWindow, AboutWindow)
│   ├── Models/               – datové třídy
│   ├── Data/                 – EF Core kontext (AppDbContext)
│   ├── Services/             – business logika (ProjectService, CatalogService)
│   ├── ViewModels/           – ViewModely a CalculationItemViewModel
│   ├── Commands/             – RelayCommand
│   └── Resources/            – barvy, styly (XAML)
├── ElektroOffer_app.Tests/   – unit a integrační testy
└── docs/                     – dokumentace (README, CHANGELOG)
```

---

## Testování

Solution obsahuje samostatný testovací projekt `ElektroOffer_app.Tests`.

| Kategorie | Popis |
|---|---|
| `DatabaseTests` | Integrační testy DB vrstvy (SQLite in-memory) |
| `RepositoryTests` | Testy operací s `PriceItems` a `Materials` |
| `LogicTests` | Unit testy logiky `CalculationItemViewModel` |

`AppDbContext` podporuje předání `DbContextOptions` zvenčí — testy proto používají izolovanou in-memory databázi a nezasahují do produkčního `elektrooffer.db`.

---

## Verze

Aktuální verze: **1.5.2**

Verze je definována v `ElektroOffer_app.csproj` a zobrazena v dialogu „O aplikaci".

---

## Architektura — rozhodnutí (ADR)

**Proč SQLite?**  
Desktopová aplikace bez serveru. Jednoduchý deployment — jeden soubor `.db`.

**Proč code-behind místo čistého MVVM?**  
Projekt vzniká jako výukový. Code-behind je pro začínajícího programátora srozumitelnější než plné MVVM. Přechod je plánován postupně.

**Proč JSON pro `.eof` soubory?**  
Čitelné pro ruční debugging, snadná migrace formátu do budoucna.

**Proč `CatalogService` místo přímého volání DB z `MainWindow`?**  
Oddělení DB logiky od UI umožňuje integrační testování bez závislosti na WPF.