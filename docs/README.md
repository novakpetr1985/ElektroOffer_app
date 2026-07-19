# ⚡ ElektroOffer

ElektroOffer 1.13.0-feature je desktopová WPF aplikace pro kalkulaci elektro prací, materiálu, nabídek a fakturaci doplněná offline terénním klientem pro Android a Windows.

## Mapa dokumentace

- `docs/README.md` — současné funkce, spuštění a technický přehled.
- `docs/CHANGELOG.md` — změny podle verzí.
- `docs/MANUAL-TESTS.md` — úplný ruční regresní checklist; prioritní release tabulka je v `docs/testing/MANUAL_TEST_SCENARIOS.md`.
- `docs/TEST-SIGNING.md` — vytvoření a instalace lokálního testovacího certifikátu a sestavení samostatného Android APK.
- `docs/architecture/` — vždy jeden dokument pro databázi, ARES, tisk/faktury a terénní workflow.
- `docs/DESIGN-TOKENS.md` — pravidla sdíleného vzhledu WPF a dokumentů.

Podrobnost patří pouze do uvedeného tematického dokumentu; README na ni jen odkazuje. Tím se stejné požadavky neudržují na více místech.

---

## 📦 Požadavky

- instalace hotového EXE: Windows 10/11 x64; .NET ani Visual Studio nejsou potřeba
- vývoj desktopové části ze zdrojového kódu: Visual Studio 2022+, .NET 10 SDK a workload **.NET Desktop Development**
- vývoj `ElektroOffer.Field`: workload **.NET MAUI**; pro Android také Android SDK a Microsoft JDK
- testovací Android zařízení: Android 7.0 / API 24 nebo novější

---

## 🚀 Spuštění aplikace

**Visual Studio**
1. Otevři `ElektroOffer_app.slnx`
2. Sestav projekt (`Ctrl + Shift + B`)
3. Spusť aplikaci (`F5`)

**Lokální instalační balíček pro Windows x64**
- dvojklik: `scripts\run-publish.bat`
- přenositelná self-contained aplikace vznikne v `artifacts/publish/win-x64`
- instalační soubor vznikne jako `artifacts/installer/ElektroOffer-Setup-<verze>-x64.exe`
- cílový počítač nepotřebuje samostatně instalovat .NET; podporované jsou Windows 10 a 11 x64
- instalátor nevyžaduje oprávnění správce, instaluje aplikaci pro aktuálního uživatele a vytvoří zástupce v nabídce Start
- volitelný vlastní publish adresář: `.\scripts\run-publish.bat -OutputDirectory "publish-custom"`

Při vytvoření tagu CI automaticky publikuje dva stažitelné artefakty: `elektrooffer-installer-win-x64` s instalačním EXE a `elektrooffer-portable-win-x64` s přenositelnou aplikací.

Produkční instalátor zatím není podepsaný veřejně důvěryhodným certifikátem. Pro testování na vlastních počítačích lze vytvořit lokální self-signed certifikát a nainstalovat jeho veřejnou část podle `docs/TEST-SIGNING.md`; veřejná distribuce bez upozornění SmartScreen vyžaduje důvěryhodný code-signing certifikát a reputaci vydavatele.

📌 Databáze `%LocalAppData%\ElektroOffer\elektrooffer.db` se vytvoří automaticky při prvním spuštění.

SQLite databáze není verzovaná v Gitu (`*.db` je v `.gitignore`). Uživatelský soubor je `%LocalAppData%\ElektroOffer\elektrooffer.db`. SQL seed je uložený v `ElektroOffer_app/Data/Seed/elektrooffer_1_9_0.sql`; build a publish kopírují pouze seed, nikdy uživatelskou DB. Při prvním spuštění nové verze lze starší DB bezpečně převzít jen tehdy, když cílová AppData DB ještě neexistuje. Podrobnosti jsou v `docs/architecture/DATABASE_LIFECYCLE.md`.

### Offline terénní aplikace

`ElektroOffer.Field` neobsahuje produkční databázi cen a bez internetu pořizuje pouze technické zaměření. Ceny se vždy doplní z aktuální databáze hlavní aplikace až při schváleném importu.

1. V hlavní aplikaci zvol `Soubor → Exportovat katalog pro terén` a vytvoř `.eofcatalog`.
2. Přenes soubor do telefonu nebo druhého počítače a v ElektroOffer Terén zvol `Katalog`.
3. Zadej zakázku, místnosti, práce, materiál, poznámky a fotografie; rozpracovaný stav se průběžně obnovuje.
4. Zvol `Uložit a exportovat` a přenes vytvořený `.eofmeasure` zpět do hlavního počítače.
5. V hlavní aplikaci zvol `Soubor → Importovat terénní měření`, zkontroluj mapování a potvrď vybrané řádky a fotografie.
6. Ulož projekt `.eof`; zdrojový balíček a přílohy se uchovají v doprovodné složce `.assets`.

Přenos je záměrně souborový a nevyžaduje cloud, účet ani server. Podrobný datový a bezpečnostní návrh je v `docs/architecture/FIELD_APPLICATION_PROPOSAL.md`; testovací balíčky a podpis popisuje `docs/TEST-SIGNING.md`.

---

## ✨ Hlavní funkce

- 📊 Kalkulace práce podle nové kaskády WorkTask → WorkSpecification → BaseMaterial → WorkPosition
- 📦 Kalkulace materiálu (Materials)
  - Podpora více dodavatelů pro každý materiál
  - Nová kaskáda výběru: Kategorie → Název → Dodavatel → Nabídka
  - Ceny a jednotky se načítají z tabulky MaterialPrices
- 🏷️ Sleva na řádek – detailní rozpočet rozlišuje cenu před slevou, slevu v procentech i Kč a výslednou cenu po slevě
- 💾 Ukládání / načítání projektu (*.eof)
- 📑 Detailní rozpis položek (Budget view)
- 🧾 Tisk (PrintDialog – Windows systémový tisk / PDF přes tiskárnu)
- Fakturace z detailního rozpočtu v odděleném modulu `ElektroOffer_app.Invoice`
  - export Fakturoid JSON
  - profesionální vícestránkový PDF export s DPH a volitelnou QR platbou
  - vyhledání dodavatele/odběratele přes ARES podle IČO
  - samostatné uložení/načtení `*.eofinvoice`
  - volitelné uložení fakturačních údajů přímo do projektu `*.eof`
- Modernější Windows 10/11 vzhled se světlým, tmavým a systémovým režimem
- 💰 Automatický součet práce + materiálu
- 🔄 Plná integrace s EF Core + SQLite
- 🧩 Oddělené modely, služby, viewmodely a testy
- 🧪 Unit + Integration testy
- 📋 Manuální checklist: `docs/MANUAL-TESTS.md`
- 📱 Offline zaměření v `ElektroOffer.Field` s fotografiemi a bezpečným importem do hlavní kalkulace

---

## 🧠 Architektura aplikace

Projekt prochází postupným přechodem z **code-behind** na **plné MVVM**.

- UI: WPF (XAML)
- logika: `MainViewModel` (náhrada `MainWindow.xaml.cs`) + Services
- příkazy: `ICommand` / `RelayCommand` napojené na XAML bindings (`{Binding SaveCommand}` apod.)
- data: EF Core + SQLite
- ukládání projektů: JSON serializace (`ProjectData`)
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
- **`CatalogService`**, **`WorkCascadeService`**, **`MaterialCascadeService`**, **`CalculationPriceService`** – aplikační logika
- **`MessageService`**, **`PrintService`**, **`ApplicationService`**, **`WindowService`**, **`AppThemeService`** – UI abstrakce, aby `MainViewModel` nevolal WPF přímo

Tato změna zajišťuje stabilní běh aplikace a eliminuje chyby typu.
**„IFileDialogService is not configured“** při volání `Load()` a `Save()`

📌 `MainViewModel` obsahuje aplikační `ICommand` vlastnosti (`SaveCommand`, `LoadCommand`, `InvoiceCommand`, `SettingsCommand`, `DeleteWorkItemCommand`, atd.), inicializované v konstruktoru pomocí `RelayCommand`.

---

## 🧩 Modely (Data vrstva)

| Model               | Popis                                                                               |
| ------------------- | ----------------------------------------------------------------------------------- |
| `WorkTask`          | Úkon v ceníku práce a základní cena                                                |
| `WorkSpecification` | Upřesnění úkonu a měrná jednotka                                                   |
| `BaseMaterial`      | Podklad práce a koeficient podkladu                                                |
| `WorkPosition`      | Umístění / poloha práce a koeficient polohy                                        |
| `TaskSpecification` | Vazba povolených kombinací WorkTask ↔ WorkSpecification                            |
| `Material`          | Produktový materiál (název, jednotka, základní cena)                                |
| `Category`          | Kategorie materiálu (Kabely, Jističe, Chrániče…)                                    |
| `Supplier`          | Dodavatel materiálu (ELKOV, EMAS…)                                                  |
| `MaterialPrice`     | Cena materiálu od konkrétního dodavatele (M:N mezi Material a Supplier)             |
| `WorkItemData`      | Uložený řádek kalkulace práce (úkon, upřesnění, podklad, umístění…)                 |
| `MaterialItemData`  | Uložený řádek kalkulace materiálu (kategorie, produkt, dodavatel…)                  |
| `CalculationItemData` | Společné hodnoty (množství, sleva, total)                                         |
| `ProjectData`       | Kompletní uložený projekt (`.eof`) — obsahuje WorkItems, MaterialItems, CommonItems, počty řádků a volitelný návrh faktury |
| `BudgetItem`        | Sloučený řádek rozpočtu včetně ceny před slevou, slevy a výsledné ceny              |


---

## ⚙️ Services (aplikační logika)

| Service                     | Účel                                                                                                                               |
| ---------------------------- | --------------------------------------------------------------------------------------------------------------------------------- |
| `ProjectService`             | Ukládání a načítání projektů `.eof`                                                                                               |
| `MeasurementImportService`   | Ověření `.eofmeasure`, mapování katalogových identit, příprava náhledu a bezpečné uložení příloh                                |
| `CatalogService`             | Načítání ceníku z databáze                                                                                                        |
| `DatabaseBootstrapService`   | Při startu vytvoří chybějící SQLite databázi a naplní ji z SQL seedu 1.9.0                                                        |
| `WorkCascadeService`         | Kaskádová logika PRÁCE: Úkon → Upřesnění → Podklad → Umístění, včetně dohledání entit pro výpočet ceny                           |
| `MaterialCascadeService`     | Kaskádová logika Kategorie → Název → Dodavatel → Materiál pro výběr produktového materiálu s cenou od konkrétního dodavatele      |
| `CalculationPriceService`    | Výpočet cen a slev (práce i materiál, včetně ceny od vybraného dodavatele)                                                        |
| `PrintService`               | Tisk / export nabídky (FlowDocument / PrintDialog)                                                                                |
| `MessageService`             | UI abstrakce pro potvrzovací dialogy (`ShowYesNo`, `ShowYesNoCancel`) volaná z `MainViewModel`                                    |
| `ApplicationService`         | UI abstrakce pro `Application.Shutdown()`                                                                                         |
| `WindowService`              | UI abstrakce pro otevírání oken (např. `AboutWindow`)                                                                             |
| `AppThemeService`            | Načtení, uložení a aplikace světlého/tmavého/systémového režimu vzhledu                                                           |
| `ApplicationInfoService`     | Informace o aplikaci (verze, metadata)                                                                                            |
| `ElektroOffer_app.Invoice`   | Oddělený fakturační modul otevíraný z hlavní aplikace i samostatně jako vlastní Windows aplikace                                  |

### Fakturace

Fakturační modul je oddělený projekt `ElektroOffer_app.Invoice`, který se otevírá z hlavního menu nebo toolbaru hlavní aplikace. Stejný projekt lze spustit i samostatně; v tom případě otevře prázdný návrh faktury bez položek z detailního rozpočtu.

- zdrojem řádků faktury je aktuální detailní rozpočet (`BudgetItems`)
- řádek faktury přebírá původní jednotkovou cenu, cenu před slevou, slevu v procentech i Kč a výslednou fakturovanou cenu
- žádné pole dodavatele ani odběratele není povinné
- tlačítko `Vyhledat` u IČO doplní údaje z veřejného ARES REST API
- fakturaci lze uložit nezávisle do `*.eofinvoice`
- fakturaci lze uložit do hlavního projektu, aby se načetla spolu s `*.eof`
- rozpracovaná faktura se automaticky zálohuje a při samostatném spuštění modulu ji lze obnovit
- tlačítky lze vymazat celý formulář nebo samostatně údaje dodavatele či odběratele
- ceny kalkulace jsou bez DPH; fakturační řádek má uživatelem nastavitelnou sazbu DPH a importované řádky začínají na 0 %
- aplikace automaticky neurčuje daňový režim podle označení práce nebo materiálu
- exporty: Fakturoid JSON a profesionální vícestránkové PDF s DPH a volitelnou QR platbou
- testy ověřují také přenos ceny před slevou, slevy a výsledné ceny do fakturace a zachování konečné částky ve Fakturoid JSON

ARES používá oficiální REST/JSON službu Ministerstva financí, nikoli SOAP. Produkční klient je oddělen přes `IAresClient`; automatické testy používají lokální HTTP handler a JSON fixtures, takže produkční ARES nekontaktují. Podrobnosti jsou v `docs/architecture/ARES_INTEGRATION.md`. Fakturační PDF používá od 1.12.0 profesionální vícestránkovou QuestPDF šablonu `ProfessionalA4` s DPH a lokální QR platbou. Architektura tisku je v `docs/architecture/PRINT_AND_TEMPLATE_ARCHITECTURE.md`; návazné ukládání měření, kalkulace a faktury sjednocuje `docs/architecture/FIELD_APPLICATION_PROPOSAL.md`.

### Nastavení vzhledu

Menu `Možnosti → Nastavení...` otevírá první stránku nastavení aplikace.

- levý panel obsahuje základní skupinu `Vzhled`
- pravá část obsahuje detailní volbu režimu
- dostupné režimy: `Dle nastavení systému`, `Světlý režim`, `Tmavý režim`
- volba se ukládá do uživatelských dat aplikace a použije se při dalším spuštění
- motiv se aplikuje na hlavní okno, fakturaci, nastavení i okno O aplikaci
- aplikace nastavuje i standardní WPF systémové barvy, aby v tmavém režimu nezůstávaly světlé vstupy, menu a tabulky
- vybrané položky v ComboBoxu, ListBoxu a DataGridu používají samostatné kontrastní barvy pro výběr a text výběru

Vizuální hodnoty nejsou zapisované přímo do jednotlivých oken. Sdílený slovník `ElektroOffer_app.Invoice/Resources/DesignTokens.xaml` definuje barvy, typografii, rozměry, odsazení, rádiusy, stíny a animace pro hlavní aplikaci, fakturaci, tisk i PDF. Konvence a příklady jsou v `docs/DESIGN-TOKENS.md`.

### GitHub Actions

Workflow `.github/workflows/elektrooffer-ci-pipeline.yml` je nastavený tak, aby se testovalo jen tam, kde to dává smysl:

- `push` do `feature/**`, `hotfix/**`, `release/**` spouští restore, Debug build, samostatný krok unit testů a samostatný krok integračních testů
- `pull_request` do `dev`, `test`, `main` spouští stejný CI běh pro kontrolu chráněných větví
- `workflow_dispatch` umožňuje ruční spuštění
- tag spouští CI a po něm samostatný Release publish
- krátký CI souhrn `elektrooffer-ci-summary` se ukládá jako artifact po každém běhu a obsahuje počty unit/integračních testů
- detailní diagnostický log se vytváří jen při chybě, aby se běžné úspěšné běhy zbytečně neduplikovaly
- všechny joby používají environment `manual-approval`; pro skutečné pozastavení běhu nastav v GitHubu `Settings -> Environments -> manual-approval -> Required reviewers`

### Import materiálů

Katalog se doplňuje přímo z jednoho XLSX přes `Soubor -> Importovat katalog z Excelu...`.

Doporučený postup:
- zkopírovat šablonu `docs/templates/ElektroOffer_Catalog_Import_Template_1.0.xlsx`
- vyplnit jednotlivé listy; názvy listů a hlaviček neměnit
- nejdříve vyplnit hlavní číselníky a potom vazební listy `TaskSpecifications` a `MaterialPrices`
- spustit import a případné chyby opravit podle názvu listu, řádku a sloupce
- po úspěchu zkontrolovat načtené volby v sekcích PRÁCE a MATERIÁL

Import kontroluje povinné hodnoty, čísla, datum, duplicity a odkazy mezi listy. Existující položky aktualizuje bez rozlišování velikosti písmen; chybějící přidá a žádné záznamy automaticky nemaže. Neplatný soubor se do databáze nezapíše. Přímý XLSX import nevyžaduje export jednotlivých listů do CSV.

Ruční SQL je vhodné jen pro jednorázovou servisní úpravu databáze, protože snadno obejde validaci a může vytvořit duplicity.

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

Řešení obsahuje tyto hlavní projekty:

| Projekt | Účel |
|---|---|
| `ElektroOffer_app` | Hlavní WPF kalkulace, katalog, export katalogu a schválený import měření |
| `ElektroOffer.Contracts` | Platformně nezávislé kontrakty, validace a serializace `.eofcatalog`/`.eofmeasure` |
| `ElektroOffer.Field` | .NET MAUI offline klient pro Android a Windows |
| `ElektroOffer_app.Invoice` | Samostatný WPF fakturační modul |
| `ElektroOffer_app.Tests.Unit` | Jednotkové a regresní testy |
| `ElektroOffer_app.Tests.Integration` | Integrační testy databáze, dokumentů a WPF zdrojů |

```
📁 Core Application
├── 📁 .github
│   └── 📁 workflows
│       └── 📄 elektrooffer-ci-pipeline.yml
│
├── 📁 docs
│   ├── 📄 CHANGELOG.md
│   ├── 📄 MANUAL-TESTS.md
│   ├── 📄 README.md
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
    │   ├── 📄 CalculationItemData.cs
    │   ├── 📄 Category.cs
    │   ├── 📄 Material.cs
    │   ├── 📄 Materials.cs
    │   ├── 📄 MaterialItemData.cs
    │   ├── 📄 MaterialPrice.cs
    │   ├── 📄 BaseMaterial.cs
    │   ├── 📄 TaskSpecification.cs
    │   ├── 📄 WorkPosition.cs
    │   ├── 📄 WorkSpecification.cs
    │   ├── 📄 WorkTask.cs
    │   ├── 📄 ProjectData.cs
    │   ├── 📄 Supplier.cs
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
    │   ├── 📄 WorkCascadeService.cs
    │   ├── 📄 CalculationPriceService.cs
    │   ├── 📄 MaterialCascadeService.cs
    │   ├── 📄 PrintService.cs
    │   ├── 📄 ProjectService.cs
    │
    ├── 📁 ViewModels
    │   ├── 📁 Items
    │   │    └── 📄 CalculationItemViewModel.cs
    │   │
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
    ├── 📄 elektrooffer.sqbpro
    ├── 📄 ElektroOffer_app.csproj
    └── 📄 ElektroOffer_app.slnx

🧪 Testing
│
├── 📁 ElektroOffer_app.Tests.Unit
│   ├── 📁 CommandTests
│   │   └── 📄 RelayCommandTests.cs
│   │
│   ├── 📁 LogicTests
│   │   ├── 📄 DiscountCalculationTests.cs
│   │   ├── 📄 PriceCalculationTests.cs
│   │   └── 📄 ApplicationInfoServiceTests.cs
│   │
│   ├── 📁 RepositoryTests
│   │   ├── 📄 MaterialRepositoryTests.cs
│   │   ├── 📄 WorkCatalogRepositoryTests.cs
│   │   └── 📄 RepositoryEdgeCaseTests.cs
│   │
│   ├── 📁 Services
│   │   ├── 📄 CatalogServiceTests.cs
│   │   └── 📄 RealFileSystemServiceTests.cs
│   │
│   ├── 📁 ViewModels
│   │   ├── 📄 CalculationItemViewModelTests_Total.cs
│   │   ├── 📄CalculationItemViewModelTests_Validation.cs
│   │   ├── 📄 CalculationItemViewModelTests_PropertyChanged.cs
│   │   ├── 📄 CalculationItemViewModelTests_IsEmpty.cs
│   │   ├── 📄 CalculationItemViewModelTests_CascadeWork.cs
│   │   └── 📄 CalculationItemViewModelTests_CascadeMaterial.cs
│   │
│   ├── 📄 ElektroOffer_app.Tests.Unit.csproj
│   └── 📄 TestBase.cs
│
└── 📁 ElektroOffer_app.Tests.Integration
    ├── 📁 Database
    │   ├── 📄 DatabaseConnectionTests.cs
    │   ├── 📄 DatabaseCrudTests.cs
    │   └── 📄 DatabaseSchemaTests.cs
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
    │   ├── 📄 CalculationItemViewModel_CascadeTests.cs
    │
    ├── 📁 UI
    │   └── 📄 DesignTokenResourceTests.cs
    │
    └── 📄 ElektroOffer_app.Tests.Integration.csproj
```

---

## 🧪 Testování

- dvojklik: `scripts\run-tests.bat`, spouští příkaz - `scripts\commands\run-tests.ps1`
- nebo z příkazové řádky: `.\scripts\run-tests.bat` , `.\scripts\run-tests.ps1`

Projekt obsahuje dvě úrovně testů:

### 🔬 Unit testy

- dvojklik: `scripts\run-tests-unit.bat`, spouští příkaz `scripts\commands\run-tests-unit.ps1`
- nebo z příkazové řádky: `.\scripts\run-tests-unit.bat`

- izolovaná logika bez skutečného UI a externích služeb; repository testy používají lokálně izolovaný SQLite
- rychlé testování výpočtů, ViewModelů a repository vrstvy

📌 zaměření:
- společná základní třída `TestBase`
- kalkulace cen a slev (`PriceCalculationTests`, `DiscountCalculationTests`)
- repository logika + edge-case scénáře (`MaterialRepositoryTests`, `WorkCatalogRepositoryTests`, `RepositoryEdgeCaseTests`)
- ViewModel logika (`CalculationItemViewModelTests_CascadeMaterial.cs`, `CalculationItemViewModelTests_CascadeWork.cs`, `CalculationItemViewModelTests_IsEmpty.cs`, `CalculationItemViewModelTests_PropertyChanged.cs`, `CalculationItemViewModelTests_Total.cs`, `CalculationItemViewModelTests_Validation.cs`)
- MVVM command logika (`RelayCommandTests`)
- kontrola verze aplikace (`ApplicationInfoServiceTests`)
- reálná implementace file systému (`RealFileSystemServiceTests`)

#### 🧱 Stabilní testovací databáze (SQLite)

- unit testy používají reálný SQLite provider, aby chování odpovídalo skutečné aplikaci.
- každý test běží v izolovaném prostředí díky
  - nové základní třídě TestBase
  - automatickému resetu databáze před každým testem ()`EnsureDeleted()` + `EnsureCreated()`)
  - jednotné struktuře testů rozdělené podle oblastí (`Total`, `Validation`, `CascadeWork`, `CascadeMaterial`, `PropertyChanged`, `IsEmpty`)
- tím je zajištěno
  - stabilní a deterministické chování testů
  - žádné kontaminované záznamy mezi testy
  - žádné chyby typu UNIQUE constraint failed
  - správné chování EF Core (Include, relace, lookupy, kaskády)
  - žádné zamčené SQLite handle
- testy tak přesně simulují reálné chování aplikace.

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
- kompletní kaskáda ViewModelu Úkon → Upřesnění → Podklad → Umístění (`CalculationItemViewModel_CascadeTests`)
- UI dialogové služby (`RealFileDialogServiceTests`, `RealMessageBoxServiceTests`) – `[Explicit]`, spouští se pouze ručně
- runtime načtení design tokenů a aplikace implicitních WPF stylů (`DesignTokenResourceTests`), včetně kontroly typů jako `Double` vs. `Thickness`

### Reálné pokrytí a omezení

Automatické testy nejsou end-to-end test celé desktopové ani mobilní aplikace. Pro 1.13.0-feature prošlo **100 unit a 36 automatických integračních testů**. Tři dialogové smoke scénáře jsou `[Explicit]` a v běžném CI se nespouštějí. Coverage po rozšíření 1.13.0 nebylo znovu změřeno, proto dokumentace neuvádí starší procenta jako současný stav. Start a oprávnění na fyzickém Android zařízení, systémové pickery, fotoaparát, tisk a instalace zůstávají součástí manuálního checklistu.

WPF XAML se může úspěšně zkompilovat a přesto selhat až při aplikaci šablony za běhu. Proto integrační sada načítá oba `ResourceDictionary`, vytváří prvky s implicitními styly, volá `ApplyTemplate` a `Measure`. Tato kontrola zachytí například předání tokenu typu `Double` vlastnosti `Padding`, která vyžaduje `Thickness`. Před vydáním je stále nutné projít `docs/MANUAL-TESTS.md`.

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
| `feature/*`	      | Pracovní větve pro konkrétní úkoly. Bez PR.        |
| `feature/<verze>` | Hlavní větev dané verze. PR z pracovních větví.    |
| `release/<verze>` | Release větev dané verze. Pro dev, test a main     |
| `dev`	            | Vývojová větev. PR z feature.                      |
| `test`	          | Staging větev. PR z dev.                           |
| `main`	          | Produkční větev. PR z test.                        |
| `tag`	            | Spouští release pipeline (publish + detailní log). |


### 🧮 Verzování aplikace


| Číslo    |	Příklad   |	Popis                                          |
| -------- | ---------- | ---------------------------------------------- |
| `MAJOR`  |	`1.x.x.`  |	Největší změny, zásadní úpravy, nové generace. |
| `MINOR`  |	`x.7.x.`  |	Větší balík změn, nové funkce.                 |
| `PATCH`  |	`x.x.5.`  |	Menší změny, běžný vývoj verze.                |
| `HOTFIX` |	`x.x.x.1` |	Fixy, drobné opravy, interní buildy.           |


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

### Multi-dodavatelské ceny materiálu (Category / Supplier / MaterialPrice)
Původní model `Material` měl jedinou cenu na položku, což neumožňovalo porovnání nabídek od více dodavatelů. Řešením je normalizovaná struktura `Category` + `Supplier` + `MaterialPrice` (spojovací tabulka nesoucí cenu, kód a název položky konkrétního dodavatele). Staré pole `Material.Price` je zatím ponecháno pro zpětnou kompatibilitu a bude odstraněno po úplném přechodu na `MaterialPrice` napříč aplikací.

---

## 🚧 Roadmap

- [x] Kalkulace práce a materiálu
- [x] SQLite databáze
- [x] Ukládání projektu
- [x] Integrační testy
- [x] NuGet závislosti stabilizovány a zabezpečeny
- [x] PDF export (aktuálně přes Windows PrintDialog → „Microsoft Print to PDF“)
- [x] GitHub tests - UNIT + integration + build + minimální CI log při každé akci; detailní log + publish jen při tagu
- [x] MVVM refactor – `MainViewModel` jako náhrada `MainWindow.xaml.cs`, `ICommand` bindings, UI služby oddělené od ViewModelu
- [x] Multi-dodavatelské ceny materiálu (`Category`, `Supplier`, `MaterialPrice`) + kaskádový výběr Kategorie → Název → Dodavatel → Materiál
- [x] Nová kaskáda PRÁCE bez `PriceItems` (`WorkTask`, `WorkSpecification`, `BaseMaterial`, `WorkPosition`)
- [x] Oddělený fakturační modul s ARES, Fakturoid JSON, PDF exportem a vlastním uložením
- [x] Modernější vzhled aplikace a základní nastavení motivu
- [ ] MVVM refactor – `AboutWindow` (zatím code-behind: `VersionText`, `CloseButton_Click`)
- [ ] Doplnit UI import materiálů z CSV s mapováním sloupců, validací a upsert logikou
- [ ] Zobrazení kódu a ceny konkrétní nabídky (`SupplierCode`, `Price`) v detailním rozpočtu
- [ ] Odstranění staršího pole `Material.Price` po úplném přechodu na `MaterialPrice`
