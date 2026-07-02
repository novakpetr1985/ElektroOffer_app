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

**Visual Studio**
1. Otevři `ElektroOffer_app.slnx`
2. Sestav projekt (`Ctrl + Shift + B`)
3. Spusť aplikaci (`F5`)

**Lokální publish (výstup do kořenové složky `publish`)**
- Dvojklik: `scripts\run-publish.bat` — vytvoří publikovatelnou verzi do `./publish` v kořeni repa.  
- Nebo z příkazové řádky:  
  `.\scripts\run-publish.bat "ElektroOffer_app/publish-custom"`

📌 Databáze `elektrooffer.db` se vytvoří automaticky při prvním spuštění.

---

## ✨ Hlavní funkce

- 📊 Kalkulace práce podle ceníku (PriceItems)
- 📦 Kalkulace materiálu (Materials)
- 🏷️ Sleva na řádek – volitelná procentuální sleva na každou položku práce i materiálu
- 💾 Ukládání / načítání projektu (*.eof)
- 📤 Export / import ceníku (*.eofcat)
- 📑 Detailní rozpis položek (Budget view)
- 🧾 Tisk (PrintDialog – Windows systémový tisk / PDF přes tiskárnu)
- 💰 Automatický součet práce + materiálu
- 🔄 Plná integrace s EF Core + SQLite
- 🧩 Oddělené modely, služby, viewmodely a testy
- 🧪 Unit + Integration testy

---

## 🧠 Architektura aplikace

Projekt je navržen jako **výuková WPF aplikace s postupným přechodem k oddělené architektuře**.

- UI: WPF (XAML)
- Logika: Code-behind + Services
- Data: EF Core + SQLite
- Export/Import: JSON serializace (ProjectData, CatalogExportData)
- Testování: Unit + Integration
- DI: Konstruktorová injekce ve ViewModelech a Services

---

## 🧩 Modely (Data vrstva)

| Model               | Popis                               |
| ------------------- | ----------------------------------- |
| `PriceItems`        | Ceník práce                         |
| `Material`          | Ceník materiálu                     |
| `WorkItemData`      | Řádek kalkulace práce               |
| `MaterialItemData`  | Řádek kalkulace materiálu           |
| `ProjectData`       | Celý uložený projekt (``.eof``)     |
| `CatalogExportData` | Export/import ceníku                |
| `BudgetItem`        | Sloučený řádek rozpočtu (UI výstup) |

---

## ⚙️ Services (aplikační logika)

| Service                  | Účel                                               |
| ------------------------ | -------------------------------------------------  |
| `ProjectService`         | Ukládání / načítání projektů + import/export       |
| `CatalogService`         | Načítání ceníku z databáze                         |
| `DialogService`          | Abstrakce MessageBox UI                            |
| `PrintService`           | Tisk / export nabídky (FlowDocument / PrintDialog) |
| `ApplicationInfoService` | Informace o aplikaci (verze, metadata)             |

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
├── 📁 .github
│   └── 📁 workflows
│       └── 📄 elektrooffer-ci-pipeline.yml
│
├── 📁 docs
│   ├── 📄 CHANGELOG.md
│   └── 📄 README.md
│
├── 📁 scripts
│   ├── 📄 AllMainFile.py
│   ├── 📄 run-publish.bat
│   ├── 📄 run-tests.bat
│   ├── 📄 run-tests-integration.bat
│   ├── 📄 run-tests-unit.bat
│   │ 
│   ├── 📁 commands
│   │   ├── 📄 run-publish.bat 
│   │   ├── 📄 run-tests.bat
│   │   ├── 📄 run-tests-integration.bat
│   │   └── 📄 run-tests-unit.bat
│   │
│   └── 📁 scripts-output
│       └── 📄 AllMainFile.txt



🖥️ Aplikace
│
└── 📁 ElektroOffer_app
    ├── 📁 Commands
    │   └── 📄 RelayCommand.cs
    │
    ├── 📁 Data
    │   └── 📄 AppDbContext.cs
    │
    ├── 📁 Models
    │   ├── 📄 BudgetItem.cs
    │   ├── 📄 CatalogExportData.cs
    │   ├── 📄 Material.cs
    │   ├── 📄 MaterialItemData.cs
    │   ├── 📄 PriceItems.cs
    │   ├── 📄 ProjectData.cs
    │   └── 📄 WorkItemData.cs
    │
    ├── 📁 Resources
    │   ├── 📁 Icons
    │   │   └── 📄 LOGO.jpg
    │   │
    │   ├── 📄 Colors.xaml
    │   └── 📄 Styles.xaml
    │
    ├── 📁 Services
    │   ├── 📄 ApplicationInfoService.cs
    │   ├── 📄 CatalogService.cs
    │   ├── 📄 DialogService.cs
    │   └── 📄 ProjectService.cs
    │
    ├── 📁 ViewModels
    │   ├── 📁 Base
    │   │    └── 📄 BaseViewModel.cs
    │   │ 
    │   ├── 📁 Items
    │   │    └── 📄 CalculationItemViewModel.cs
    │   │
    │   └── 📄 AboutWindowViewModel.cs
    │
    ├── 📁 Views
    │   ├── 📄 AboutWindow.xaml
    │   ├── 📄 AboutWindow.xaml.cs
    │   ├── 📄 MainWindow.xaml
    │   └── 📄 MainWindow.xaml.cs
    │
    ├── 📄 App.xaml
    ├── 📄 App.xaml.cs
    ├── 📄 elektrooffer.db
    ├── 📄 elektrooffer.sqbpro
    ├── 📄 ElektroOffer_app.csproj
    └── 📄 ElektroOffer_app.slnx

🧪 Testing
│
├── 📁 ElektroOffer_app.Tests.Unit
│   ├── 📁 LogicTests
│   │   ├── 📄 DiscountCalculationTests.cs
│   │   ├── 📄 PriceCalculationTests.cs
│   │   └── 📄 WorkItemCalculationTests.cs -> doplnit
│   │
│   ├── 📁 RepositoryTests
│   │   ├── 📄 MaterialRepositoryTests.cs
│   │   └── 📄 PriceItemsRepositoryTests.cs
│   │
│   └── 📄 ElektroOffer_app.Tests.Unit.csproj
│
└── 📁 ElektroOffer_app.Tests.Integration
    ├── 📁 Database
    │   ├── 📄 DatabaseConnectionTests.cs
    │   ├── 📄 DatabaseCrudTests.cs
    │   └── 📄 DatabaseSchemaTests.cs
    │
    ├── 📁 Infrastructure
    │   ├── 📄 TestDatabaseFactory.cs -> doplnit
    │   ├── 📄 TestDbContextOptions.cs -> doplnit
    │   └── 📄 TestFileSystem.cs -> doplnit
    │
    ├── 📁 Services
    │   ├── 📄 CatalogServiceTests.cs
    │   └── 📄 ProjectServiceTests.cs
    │
    └── 📄 ElektroOffer_app.Tests.Integration.csproj
```

---

## 🧪 Testování

- Dvojklik: `scripts\run-tests.bat`, spouští příkaz - `scripts\commands\run-tests.ps1`
- Nebo z příkazové řádky: `.\scripts\run-tests.bat` , `.\scripts\run-tests.ps1`

Projekt obsahuje dvě úrovně testů:

### 🔬 Unit testy

- Dvojklik: `scripts\run-tests-units.bat`, spouští příkaz - `scripts\commands\run-tests-units.ps1`
- Nebo z příkazové řádky: `.\scripts\run-tests-units.bat` , `.\scripts\run-tests-units.ps1`

- izolovaná logika bez DB a UI
- rychlé testování výpočtů a repository vrstvy

📌 zaměření:
- kalkulace cen (`PriceCalculationTests`)
- repository logika (`MaterialRepositoryTests`, `PriceItemsRepositoryTests`)
- ViewModel logika

### 🧪 Integrační testy

- Dvojklik: `scripts\run-tests-integration.bat`, spouští příkaz - `scripts\commands\run-tests-integration.ps1`
- Nebo z příkazové řádky: `.\scripts\run-tests-integration.bat` , `.\scripts\run-tests-integration.ps1`

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
- `Microsoft.Data.Sqlite` API (moderní, bezpečné)

---

## 🧩 CI / GitHub Actions

Projekt využívá Continuous Integration (CI) přes GitHub Actions.

### 🔄 Co se spouští automaticky

| Akce                       | Push | Pull | Tag |
| -------------------------- | ---- | ---- | ----|
| `**Restore NuGet balíčků**`|  ✔  |   ✔  |  ✔  |
| `**Build solution**`       |  ✔  |   ✔  |  ✔  |
| `**Unit testy**`           |  ✔  |   ✔  |  ✔  |
| `**Integration testy**`    |  ✔  |   ✔  |  ✔  |
| `**Publish aplikace**`     |  ❌ |  ❌  |  ✔  |
| `**Upload artefaktu**`     |  ❌ |  ❌  |  ✔  |
| `**CI log**`               |  ✔  |   ✔  |  ✔  |

### 📝 Popis workflow

- Build + testy probíhají při každém pushi a pull requestu.  
- Publish a upload artefaktu probíhají pouze při vytvoření tagu (např. `v1.7.5`).  
- Díky tomu je CI rychlé při vývoji a plně automatické při vydání nové verze.

### Workflow: 

- `.github/workflows/elektrooffer-ci-pipeline` 
📁 Core Application
│
├── 📁 .github
    └── 📁 workflows
        └── 📄 elektrooffer-ci-pipeline.yml

---

## 📦 NuGet závislosti

### Hlavní projekt (`ElektroOffer_app`)

| Balíček                               | Verze  | Účel                                            |
| ------------------------------------- | ------ | ----------------------------------------------- |
| `Microsoft.Data.Sqlite`               | 10.0.9 | SQLite driver pro EF Core                       |
| `Microsoft.EntityFrameworkCore`       | 10.0.9 | ORM vrstva                                      |
| `Microsoft.EntityFrameworkCore.Sqlite`| 10.0.9 | SQLite provider pro EF Core                     |
| `Microsoft.EntityFrameworkCore.Design`| 10.0.9 | Migrace (pouze dev)                             |
| `Microsoft.EntityFrameworkCore.Tools` | 10.0.9 | CLI nástroje (pouze dev)                        |
| `SQLitePCLRaw.lib.e_sqlite3`          | 3.50.3 | Nativní SQLite knihovna (pin kvůli bezpečnosti) |
| `SQLitePCLRaw.bundle_e_sqlite3`       | 3.0.3  | Tranzitivní pin (bezpečnostní oprava)           |
| `SQLitePCLRaw.core`                   | 3.0.3  | Tranzitivní pin (bezpečnostní oprava)           |
| `SQLitePCLRaw.provider.e_sqlite3`     | 3.0.3  | Tranzitivní pin (bezpečnostní oprava)           |

### Testovací projekty (Unit + Integration)

| Balíček                  | Verze  | Účel                         |
| ------------------------ | ------ | ---------------------------- |
| `NUnit`                  | 3.14.0 | Testovací framework          |
| `NUnit3TestAdapter`      | 6.2.0  | Integrace s VS Test Explorer |
| `Microsoft.NET.Test.Sdk` | 18.7.0 | Test runner                  |
| `coverlet.collector`     | 10.0.1 | Code coverage                |

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
- [x] PDF export (aktuálně přes Windows PrintDialog → „Microsoft Print to PDF“)
- [x] GitHub tests - UNIT + integration + build + CI log při každé akci, publish jen při tagu
- [ ] MVVM refactor (částečný → plný)
- [ ] Přidání hodnoty navíc ve verzování v okně o aplikaci