# ElektroOffer

Desktopová WPF aplikace pro kalkulaci elektro prací a materiálu.

---

## 📦 Požadavky

- Windows 10/11
- Visual Studio 2022+ s workload **.NET Desktop Development**
- .NET 10 SDK

---

## 🚀 Jak spustit

1. Otevři `ElektroOffer_app.slnx` ve Visual Studio
2. Sestav projekt — `Ctrl+Shift+B`
3. Spusť — `F5`

Databáze `elektrooffer.db` se vytvoří automaticky při prvním spuštění.  
Tabulky jsou při prvním spuštění prázdné — ceník importuj přes:

**Soubor → Import ceníku** (`.eofcat`)

---

## ✨ Hlavní funkce

- Kalkulace práce podle ceníku (`PriceItems`)
- Kalkulace materiálu (`Materials`)
- Uložení a načtení projektu (`*.eof`)
- Export a import ceníku (`*.eofcat`)
- Přehledný rozpis položek s celkovou cenou

---

## 🧠 Technický přehled

- **Platforma:** .NET 10, WPF
- **UI:** XAML (`MainWindow`, `AboutWindow`, `Resources/Styles`)
- **Databáze:** SQLite + Entity Framework Core (`AppDbContext`)
- **Architektura:** Code-behind + částečné oddělení service vrstvy

---

## 🧩 Modely

| Třída | Účel |
|------|------|
| `PriceItems` | Ceník práce |
| `Material` | Ceník materiálu |
| `ProjectData` | Serializovaný projekt (`.eof`) |
| `CatalogExportData` | Export/import ceníku (`.eofcat`) |
| `WorkItemData` | Řádek kalkulace práce |
| `MaterialItemData` | Řádek kalkulace materiálu |

---

## ⚙️ Služby

| Třída | Účel |
|------|------|
| `ProjectService` | Ukládání/načítání projektů + import/export ceníku |
| `CatalogService` | Načítání ceníku z databáze (testovatelné bez UI) |
| `DialogService` | Zobrazení MessageBox dialogů |

---

## 📁 Struktura projektu
ElektroOffer_app.slnx
├── ElektroOffer_app/
│ ├── App.xaml
│ ├── Views/ – MainWindow, AboutWindow
│ ├── Models/ – datové modely
│ ├── Data/ – AppDbContext (EF Core)
│ ├── Services/ – business logika
│ ├── ViewModels/ – MVVM částečná vrstva
│ ├── Commands/ – RelayCommand
│ └── Resources/ – styly, barvy
│
├── ElektroOffer_app.Tests.Unit/
│ ├── LogicTests
│ ├── DatabaseTests
│ └── RepositoryTests
│
├── ElektroOffer_app.Tests.Integration/
│ ├── Database/
│ │ ├── DatabaseConnectionTests
│ │ ├── DatabaseSchemaTests
│ │ └── DatabaseCrudTests
│ │
│ └── Services/
│ ├── CatalogServiceTests
│ └── ProjectServiceTests
│
└── docs/
├── README.md
└── CHANGELOG.md

---

## 🧪 Testování

Projekt obsahuje dvě úrovně testů:

---

### 🔬 Unit testy

Umístění: `ElektroOffer_app.Tests.Unit`

Testují izolovanou logiku bez závislosti na databázi nebo UI.

Kategorie:
- `LogicTests` – výpočty a ViewModel logika
- `RepositoryTests` – práce s EF Core v izolaci
- `DatabaseTests` – základní DB operace

---

### 🧪 Integrační testy

Umístění: `ElektroOffer_app.Tests.Integration`

Testují spolupráci více částí systému.

Kategorie:

- `DatabaseConnectionTests` – připojení k SQLite
- `DatabaseSchemaTests` – vytvoření tabulek (EF Core)
- `DatabaseCrudTests` – CRUD operace nad databází
- `CatalogServiceTests` – logika načítání ceníku
- `ProjectServiceTests` – ukládání a načítání projektů (.eof)

---

## 🧠 Testovací architektura

- SQLite InMemory databáze
- EF Core `DbContext` izolovaný pro každý test
- oddělení Unit vs Integration testů
- testování service vrstvy bez UI

---

## 📌 Verze

Aktuální verze: **1.6.0 (Integrační testovací základ)**

- databázová vrstva otestována
- service vrstva otestována
- připraveno pro PDF export (1.7.0)

---

## 🏗️ Architektura — rozhodnutí (ADR)

### Proč SQLite?
Desktopová aplikace bez serveru → jednoduché nasazení.

### Proč code-behind + částečné MVVM?
Výukový projekt → jednoduchost > enterprise složitost.

### Proč JSON (.eof)?
Snadná čitelnost a ladění dat.

### Proč CatalogService?
Oddělení DB logiky od UI → umožňuje testování bez WPF.