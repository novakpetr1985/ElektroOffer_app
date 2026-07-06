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
- dvojklik: `scripts\run-publish.bat` — vytvoří publikovatelnou verzi do `./publish` v kořeni repa
- nebo z příkazové řádky:  
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

Projekt prochází postupným přechodem z **code-behind** na **plné MVVM**.

- UI: WPF (XAML)
- logika: `MainViewModel` (náhrada `MainWindow.xaml.cs`) + Services
- příkazy: `ICommand` / `RelayCommand` napojené na XAML bindings (`{Binding SaveCommand}` apod.)
- data: EF Core + SQLite
- Export/Import: JSON serializace (ProjectData, CatalogExportData)
- testování: Unit + Integration
- DI: Konstruktorová injekce ve ViewModelech a Services (bez DI kontejneru, ruční composition v `MainWindow.xaml.cs`)

---

## 🔧 Dependency Injection v hlavním okně aplikace

`MainWindow.xaml.cs` sestavuje celý graf závislostí a předává je do `MainViewModel`, na který je následně nastaven `DataContext`:

- **`AppDbContext`** – jeden sdílený EF Core kontext pro celou aplikaci
- **`ProjectService`** – inicializován s reálnými implementacemi:
  - `RealFileDialogService` – Open/Save dialogy
  - `RealFileSystemService` – čtení a zápis souborů
  - `RealMessageBoxService` – WPF MessageBox
- **`CatalogService`**, **`CalculationCascadeService`**, **`CalculationPriceService`** – aplikační logika
- **`MessageService`**, **`PrintService`**, **`ApplicationService`**, **`WindowService`** – UI abstrakce, aby `MainViewModel` nevolal WPF přímo

Tato změna zajišťuje stabilní běh aplikace a eliminuje chyby typu.
**„IFileDialogService is not configured“** při volání `Load()` a `Save()`

📌 `MainViewModel` obsahuje 13 `ICommand` vlastností (`SaveCommand`, `LoadCommand`, `DeleteWorkItemCommand`, atd.), inicializovaných v konstruktoru pomocí `RelayCommand`.

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

| Service                     | Účel                                                                                           |
| --------------------------- | ---------------------------------------------------------------------------------------------- |
| `ProjectService`            | Ukládání / načítání projektů + import/export                                                   |
| `CatalogService`            | Načítání ceníku z databáze                                                                     |
| `CalculationCascadeService` | Kaskádová logika Task → Specification → Material → Location                                    |
| `CalculationPriceService`   | Výpočet cen a slev                                                                             |
| `PrintService`              | Tisk / export nabídky (FlowDocument / PrintDialog)                                             |
| `MessageService`            | UI abstrakce pro potvrzovací dialogy (`ShowYesNo`, `ShowYesNoCancel`) volaná z `MainViewModel` |
| `ApplicationService`        | UI abstrakce pro `Application.Shutdown()`                                                      |
| `WindowService`             | UI abstrakce pro otevírání oken (např. `AboutWindow`)                                          |
| `ApplicationInfoService`    | Informace o aplikaci (verze, metadata)                                                         |

---

## 🖥️ UI vrstva

- `MainWindow.xaml` – hlavní kalkulační rozhraní, plně databound přes `MainViewModel` (`ICommand` bindings místo code-behind event handlerů)
- `AboutWindow.xaml` – informace o aplikaci *(zatím code-behind, viz Roadmapa)*
- toolbar + Menu + StatusBar
- dynamické ItemsControl pro práce a materiál
- rozpis (BudgetItems)
- celková cena (GrandTotal)

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
    │   ├── 📁 Abstractions
    │   │   ├── 📄 IFileDialogService.cs
    │   │   ├── 📄 IFileSystemService.cs
    │   │   ├── 📄 IMessageBoxService.cs
    │   │   ├── 📄 IMessageService.cs
    │   │   ├── 📄 IPrintService.cs
    │   │   ├── 📄 IApplicationService.cs
    │   │   └── 📄 IWindowService.cs
    │   │
    │   ├── 📁 Implementations
    │   │   ├── 📄 RealFileDialogService.cs
    │   │   ├── 📄 RealFileSystemService.cs
    │   │   ├── 📄 RealMessageBoxService.cs
    │   │   ├── 📄 MessageService.cs
    │   │   ├── 📄 ApplicationService.cs
    │   │   └── 📄 WindowService.cs
    │   │
    │   ├── 📄 ApplicationInfoService.cs
    │   ├── 📄 CatalogService.cs
    │   ├── 📄 PrintService.cs
    │   ├── 📄 ProjectService.cs
    │   └── 📄 VersionService.cs
    │
    ├── 📁 ViewModels
    │   ├── 📁 Base
    │   │    └── 📄 BaseViewModel.cs
    │   │ 
    │   ├── 📁 Items
    │   │    └── 📄 CalculationItemViewModel.cs
    │   │
    │   ├── 📄 AboutWindowViewModel.cs
    │   └── 📄 MainViewModel.cs
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
│   │   ├── 📄 WorkItemCalculationTests.cs -> doplnit
│   │   └── 📄 VersionTests.cs
│   │
│   ├── 📁 RepositoryTests
│   │   ├── 📄 MaterialRepositoryTests.cs
│   │   ├── 📄 PriceItemsRepositoryTests.cs
│   │   └── 📄 RepositoryEdgeCaseTests.cs
│   │
│   ├── 📁 CommandTests
│   │   └── 📄 RelayCommandTests.cs
│   │
│   ├── 📁 Services
│   │   ├── 📄 CatalogServiceTests.cs
│   │   ├── 📄 ProjectServiceTests.cs
│   │   └── 📄 RealFileSystemServiceTests.cs
│   │
│   ├── 📁 ViewModelTests
│   │   └── 📄 CalculationItemViewModelTests.cs
│   │
│   ├── 📁 ViewModels
│   │   └── 📄 CalculationItemViewModel_AdvancedTests.cs
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
    │   ├── 📄 ProjectServiceTests.cs
    │   ├── 📄 ProjectServiceTests_Advanced.cs
    │   ├── 📄 CatalogServiceTests_Advanced.cs
    │   ├── 📄 RealFileDialogServiceTests.cs [Explicit]
    │   └── 📄 RealMessageBoxServiceTests.cs [Explicit]
    │
    ├── 📁 ViewModels
    │   ├── 📄 CalculationItemViewModelIntegrationTests.cs
    │   ├── 📄 CalculationItemViewModel_CascadeTests.cs
    │   └── 📄 CalculationItemViewModelStub.cs (přejmenováno z CalculationItemViewModel.cs)
    │
    └── 📄 ElektroOffer_app.Tests.Integration.csproj
```

---

## 🧪 Testování

- dvojklik: `scripts\run-tests.bat`, spouští příkaz - `scripts\commands\run-tests.ps1`
- nebo z příkazové řádky: `.\scripts\run-tests.bat` , `.\scripts\run-tests.ps1`

Projekt obsahuje dvě úrovně testů:

### 🔬 Unit testy

- dvojklik: `scripts\run-tests-units.bat`, spouští příkaz - `scripts\commands\run-tests-units.ps1`
- nebo z příkazové řádky: `.\scripts\run-tests-units.bat` , `.\scripts\run-tests-units.ps1`

- izolovaná logika bez DB a UI
- rychlé testování výpočtů a repository vrstvy

📌 zaměření:
- kalkulace cen a slev (`PriceCalculationTests`, `DiscountCalculationTests`)
- repository logika + edge-case scénáře (`MaterialRepositoryTests`, `PriceItemsRepositoryTests`, `RepositoryEdgeCaseTests`)
- ViewModel logika (`CalculationItemViewModelTests`, `CalculationItemViewModel_AdvancedTests`)
- MVVM command logika (`RelayCommandTests`)
- kontrola verze aplikace (`VersionTests`)
- reálná implementace file systému (`RealFileSystemServiceTests`)

### 🧪 Integrační testy

- dvojklik: `scripts\run-tests-integration.bat`, spouští příkaz - `scripts\commands\run-tests-integration.ps1`
- nebo z příkazové řádky: `.\scripts\run-tests-integration.bat` , `.\scripts\run-tests-integration.ps1`

Testují spolupráci:

- EF Core + SQLite
- Services + DB
- kompletní scénáře aplikace

📌 zahrnují:
- DB schema testy
- CRUD operace
- ProjectService testy (`ProjectServiceTests`, `ProjectServiceTests_Advanced`)
- CatalogService testy (`CatalogServiceTests`, `CatalogServiceTests_Advanced`)
- kompletní kaskáda ViewModelu Task → Specification → Material → Location (`CalculationItemViewModelIntegrationTests`, `CalculationItemViewModel_CascadeTests`)
- UI dialogové služby (`RealFileDialogServiceTests`, `RealMessageBoxServiceTests`) – `[Explicit]`, spouští se pouze ručně

### 🧠 Testovací architektura

- SQLite InMemory / test DB (izolovaná DB per test, mazaná v `[TearDown]`)
- EF Core izolované DbContexty
- oddělení Unit vs Integration projektů
- testování service vrstvy bez závislosti na UI
- `Microsoft.Data.Sqlite` API (moderní, bezpečné)
- sjednoceno napříč všemi ViewModel testy (`CalculationItemViewModelTests`, `CalculationItemViewModel_AdvancedTests`)
  - žádný test nepoužívá EF InMemory provider

---

## 🧩 CI / GitHub Actions

### 🔄 Development workflow

Vývoj probíhá přes více větví, aby byla zajištěna stabilita a kontrola nad změnami:

1. hotfix/* – větve pro hotfixy bugů
2. feature/* – pracovní větve pro nové funkce a úpravy
3. release/* – větev pro vydání verzí do dev, test a main větví
4. dev – integrační větev, kde se slučují dokončené feature větve
5. test – staging větev pro ověření před nasazením
6. main – produkční větev
7. tag – finální verze aplikace (spouští release pipeline)

Každá změna prochází přes Pull Request, CI kontrolu a pravidla z GitHub Rulesetu.


### 🧩 Typy větví

| Větev   	        | Účel                                               |
| ----------------- | -------------------------------------------------- |
| `hotfix/*`  	    | Větev pro hotfixy                                  |
| `feature/*`	    | Pracovní větve pro konkrétní úkoly. Bez PR.        |
| `feature/<verze>` | Hlavní větev dané verze. PR z pracovních větví.    |
| `release/<verze>` | Release větev dané verze. Pro dev, test a main     |
| `dev`	            | Vývojová větev. PR z feature.                      |
| `test`	        | Staging větev. PR z dev.                           |
| `main`	        | Produkční větev. PR z test.                        |
| `tag`	            | Spouští release pipeline (publish + detailní log). |


### 🧮 Verzování aplikace


| Číslo   |	Příklad   |	Popis                                          |
| ------- | --------- | ---------------------------------------------- |
| `MAJOR` |	`1.x.x.`  |	Největší změny, zásadní úpravy, nové generace. |
| `MINOR` |	`x.7.x.`  |	Větší balík změn, nové funkce.                 |
| `PATCH` |	`x.x.5.`  |	Menší změny, běžný vývoj verze.                |
| `FIX`   |	`x.x.x.1` |	Fixy, drobné opravy, interní buildy.           |


### Projekt využívá Continuous Integration (CI) přes GitHub Actions.

#### 📊 CI Pipeline – Matice běhu akcí

| Akce                                    | Push | Pull Request | Tag | Error |
| --------------------------------------- | ---- | ------------ | --- | ----- |
| `Restore NuGet Packages`                |  ✔   |      ✔      |  ✔  |  ❌  |
| `Build Solution`                        |  ✔   |      ✔      |  ✔  |  ❌  |
| `Run Unit Tests`                        |  ✔   |      ✔      |  ✔  |  ❌  |
| `Run Integration Tests`                 |  ✔   |      ✔      |  ✔  |  ❌  |
| `Generate Minimal CI Log`               |  ✔   |      ✔      |  ✔  |  ❌  |
| `Upload CI log artifact`                |  ✔   |      ✔      |  ✔  |  ❌  |
| `Publish Application`                   |  ❌  |      ❌     |  ✔  |  ❌  |
| `Upload Publish Artifact`               |  ❌  |      ❌     |  ✔  |  ❌  |
| `Generate Detailed CI Log (release)`    |  ❌  |      ❌     |  ✔  |  ❌  |
| `Upload CI log full artifact (release)` |  ❌  |      ❌     |  ✔  |  ❌  |
| `Generate Detailed CI Log (error)`      |  ❌  |      ❌     |  ❌ |   ✔  |
| `Upload CI log error artifact`          |  ❌  |      ❌     |  ❌ |   ✔  |


#### 📝 Popis workflow

- `.github/workflows/elektrooffer-ci-pipeline`
- build + testy + minimální log probíhají při každém pushi a pull requestu
- publish + upload artefaktů + detailní logy probíhají pouze při vytvoření tagu (např. v1.7.6)
- CI je díky tomu rychlé při vývoji a plně automatické při vydání nové verze

```
📁 Core Application
└── 📁 .github
    └── 📁 workflows
        └── 📄 elektrooffer-ci-pipeline.yml
```

---

## 📦 NuGet závislosti

### Hlavní projekt (`ElektroOffer_app`)

| Balíček                                | Verze  | Účel                                            |
| -------------------------------------- | ------ | ----------------------------------------------- |
| `Microsoft.Data.Sqlite`                | 10.0.9 | SQLite driver pro EF Core                       |
| `Microsoft.EntityFrameworkCore`        | 10.0.9 | ORM vrstva                                      |
| `Microsoft.EntityFrameworkCore.Sqlite` | 10.0.9 | SQLite provider pro EF Core                     |
| `Microsoft.EntityFrameworkCore.Design` | 10.0.9 | Migrace (pouze dev)                             |
| `Microsoft.EntityFrameworkCore.Tools`  | 10.0.9 | CLI nástroje (pouze dev)                        |
| `SQLitePCLRaw.lib.e_sqlite3`           | 3.50.3 | Nativní SQLite knihovna (pin kvůli bezpečnosti) |
| `SQLitePCLRaw.bundle_e_sqlite3`        | 3.0.3  | Tranzitivní pin (bezpečnostní oprava)           |
| `SQLitePCLRaw.core`                    | 3.0.3  | Tranzitivní pin (bezpečnostní oprava)           |
| `SQLitePCLRaw.provider.e_sqlite3`      | 3.0.3  | Tranzitivní pin (bezpečnostní oprava)           |

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

### MVVM (MainViewModel)
Postupný přechod z code-behind na MVVM zlepšuje testovatelnost (`ICommand` lze testovat bez UI) a připravuje aplikaci na budoucí škálování. UI abstrakce (`IMessageService`, `IWindowService`, `IPrintService`, `IApplicationService`) drží `MainViewModel` nezávislý na WPF – ViewModel nikdy nevolá `MessageBox.Show`, `Application.Current.Shutdown()` ani WPF okna přímo.

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
- [x] GitHub tests - UNIT + integration + build + minimální CI log při každé akci; detailní log + publish jen při tagu
- [x] MVVM refactor – `MainViewModel` jako náhrada `MainWindow.xaml.cs`, `ICommand` bindings, UI služby oddělené od ViewModelu
- [ ] MVVM refactor – `AboutWindow` (zatím code-behind: `VersionText`, `CloseButton_Click`)