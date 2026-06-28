# ⚡ ElektroOffer

Desktopová WPF aplikace pro kalkulaci elektro prací a materiálu.
Cílem projektu je vytvořit přehledný nástroj pro tvorbu nabídek (rozpočtů) s možností ukládání, načítání, exportu a budoucím PDF / tisku.

---

## 📦 Požadavky

- Windows 10 / 11
- Visual Studio 2022+
- .NET 10 SDK
- workload: **.NET Desktop Development**

---

## 🚀 Spuštění aplikace

1. Otevři `ElektroOffer_app.slnx`
2. Sestav projekt (`Ctrl + Shift + B`)
3. Spusť aplikaci (`F5`)

📌 Databáze `elektrooffer.db` se vytvoří automaticky při prvním spuštění.

---

## ✨ Hlavní funkce

- 📊 Kalkulace práce podle ceníku (PriceItems)
- 📦 Kalkulace materiálu (Materials)
- 💾 Ukládání / načítání projektu (`*.eof`)
- 📤 Export / import ceníku (`*.eofcat`)
- 📑 Detailní rozpis položek (Budget view)
- 🧾 Tisk (PrintDialog – Windows systémový tisk / PDF přes tiskárnu)
- 💰 Automatický součet práce + materiálu

---

## 🧠 Architektura aplikace

Projekt je navržen jako **výuková WPF aplikace s postupným přechodem k oddělené architektuře**.

- UI: WPF (XAML)
- Logika: Code-behind + Services
- Data: EF Core + SQLite
- Export/Import: JSON serializace
- Testování: Unit + Integration

---

## 🧩 Modely (Data vrstva)

| Model | Popis |
|------|------|
| `PriceItems` | Ceník práce |
| `Material` | Ceník materiálu |
| `WorkItemData` | Řádek kalkulace práce |
| `MaterialItemData` | Řádek kalkulace materiálu |
| `ProjectData` | Celý uložený projekt (`.eof`) |
| `CatalogExportData` | Export/import ceníku |
| `BudgetItem` | Sloučený řádek rozpočtu (UI výstup) |

---

## ⚙️ Services (aplikační logika)

| Service | Účel |
|--------|------|
| `ProjectService` | Ukládání / načítání projektů + import/export |
| `CatalogService` | Načítání ceníku z databáze |
| `DialogService` | Abstrakce MessageBox UI |
| `PrintService` | Tisk / export nabídky (FlowDocument / PrintDialog) |

---

## 🖥️ UI vrstva

- `MainWindow.xaml` – hlavní kalkulační rozhraní
- `AboutWindow.xaml` – informace o aplikaci
- Toolbar + Menu + StatusBar
- Dynamické ItemsControl pro práce a materiál
- Rozpis (BudgetItems)
- Celková cena (GrandTotal)

---

## 📁 Struktura projektu

```
📁 Core Application
├── Views
│   ├── MainWindow (hlavní kalkulace)
│   └── AboutWindow (info o aplikaci)
│
├── ViewModels
│   ├── BaseViewModel
│   ├── CalculationItemViewModel (logika řádku kalkulace)
│   └── AboutWindowViewModel
│
├── Models
│   ├── ProjectData (uložený projekt)
│   ├── WorkItemData (práce)
│   ├── MaterialItemData (materiál)
│   ├── BudgetItem (výstupní rozpis)
│   └── CatalogExportData (import/export)
│
├── Services
│   ├── ProjectService (save/load/export/import)
│   ├── CatalogService (DB ceník)
│   ├── DialogService (UI dialogy)
│   └── PrintService (tisk / export)
│
├── Data
│   └── AppDbContext (EF Core + SQLite)
│
├── Commands
│   └── RelayCommand (MVVM commandy)
│
└── Resources
    ├── Styles.xaml
    ├── Colors.xaml
    └── Icons

🧪 Testing
├── Unit Tests (ElektroOffer_app.Tests.Unit)
│   ├── LogicTests
│   ├── RepositoryTests
│   └── DatabaseTests
│
└── Integration Tests (ElektroOffer_app.Tests.Integration)
    ├── Database Tests
    ├── Service Tests
    └── CRUD Scenarios
```

---

## 🧪 Testování

Projekt obsahuje dvě úrovně testů:

### 🔬 Unit testy

- izolovaná logika bez DB a UI
- rychlé testování výpočtů a repository vrstvy

📌 zaměření:
- kalkulace cen (`PriceCalculationTests`)
- repository logika (`MaterialRepositoryTests`, `PriceItemsRepositoryTests`)
- ViewModel logika

### 🧪 Integrační testy

Testují spolupráci:

- EF Core + SQLite
- Services + DB
- kompletní scénáře aplikace

📌 zahrnují:
- DB schema testy
- CRUD operace
- ProjectService testy
- CatalogService testy

### 🧠 Testovací architektura

- SQLite InMemory / test DB (izolovaná DB per test, mazaná v `[TearDown]`)
- EF Core izolované DbContexty
- oddělení Unit vs Integration projektů
- testování service vrstvy bez závislosti na UI
- `Microsoft.Data.Sqlite` API (ne starý `System.Data.SQLite`)

---

## 📦 NuGet závislosti

### Hlavní projekt (`ElektroOffer_app`)

| Balíček | Verze | Účel |
|--------|-------|------|
| `Microsoft.Data.Sqlite` | 10.0.9 | SQLite driver pro EF Core |
| `Microsoft.EntityFrameworkCore` | 10.0.9 | ORM vrstva |
| `Microsoft.EntityFrameworkCore.Sqlite` | 10.0.9 | SQLite provider pro EF Core |
| `Microsoft.EntityFrameworkCore.Design` | 10.0.9 | Migrace (pouze dev) |
| `Microsoft.EntityFrameworkCore.Tools` | 10.0.9 | CLI nástroje (pouze dev) |
| `SQLitePCLRaw.lib.e_sqlite3` | 3.50.3 | Nativní SQLite knihovna (pin kvůli bezpečnosti) |
| `SQLitePCLRaw.bundle_e_sqlite3` | 3.0.3 | Tranzitivní pin (bezpečnostní oprava) |
| `SQLitePCLRaw.core` | 3.0.3 | Tranzitivní pin (bezpečnostní oprava) |
| `SQLitePCLRaw.provider.e_sqlite3` | 3.0.3 | Tranzitivní pin (bezpečnostní oprava) |

### Testovací projekty (Unit + Integration)

| Balíček | Verze | Účel |
|--------|-------|------|
| `NUnit` | 3.14.0 | Testovací framework |
| `NUnit3TestAdapter` | 6.2.0 | Integrace s VS Test Explorer |
| `Microsoft.NET.Test.Sdk` | 18.7.0 | Test runner |
| `coverlet.collector` | 10.0.1 | Code coverage |

> ⚠️ **Poznámka k SQLitePCLRaw:** Balíčky `bundle`, `core` a `provider` jsou explicitně pinovány na verzi `3.0.3` aby přebily tranzitivní požadavek na zranitelnou verzi `2.1.11` ([GHSA-2m69-gcr7-jv3q](https://github.com/advisories/GHSA-2m69-gcr7-jv3q)), která přichází přes strom závislostí EF Core.

---

## 🏗️ Architektonická rozhodnutí (ADR)

### SQLite
Lokální desktop aplikace → jednoduché nasazení bez serveru.

### Code-behind + Services
Výukový projekt → jednoduchost + postupný refactoring.

### JSON (.eof)
Snadná čitelnost, debug a kompatibilita.

### PrintService
Oddělení exportu/tisku od UI logiky.

### Microsoft.Data.Sqlite (ne System.Data.SQLite)
Projekt používá výhradně `Microsoft.Data.Sqlite` jako SQLite driver – je to moderní, aktivně vyvíjený balíček integrovaný přímo do ekosystému EF Core a .NET. Starý `System.Data.SQLite` byl odebrán – konfliktoval s EF Core a nepatří do moderního .NET 10 projektu.

---

## 🚧 Roadmap

- [x] Kalkulace práce a materiálu
- [x] SQLite databáze
- [x] Ukládání projektu
- [x] Integrační testy
- [x] Tisk / PrintDialog
- [x] NuGet závislosti stabilizovány a zabezpečeny
- [ ] PDF export (QuestPDF nebo podobné)
- [ ] MVVM refactor (částečný → plný)