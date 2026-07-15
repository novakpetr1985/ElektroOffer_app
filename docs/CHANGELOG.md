# Changelog

Všechny důležité změny projektu jsou dokumentovány v tomto souboru.  
Formát vychází z [Keep a Changelog](https://keepachangelog.com/cs/1.0.0/).

---

## [1.9.0] - 2026-07-15

### Přidáno
- Nová kaskáda sekce PRÁCE postavená na samostatných entitách `WorkTask`, `WorkSpecification`, `BaseMaterial`, `WorkPosition` a vazební tabulce `TaskSpecification`.
- Doplněn manuální testovací checklist `docs/MANUAL-TESTS.md` pro celkovou regresi aplikace, DB, fakturaci, vzhled, CI a PR proces.
- Nová služba `WorkCascadeService`, která nahrazuje původní práci nad `PriceItems`.
- Nový samostatný WPF modul `ElektroOffer_app.Invoice` pro přípravu faktury z detailního rozpočtu.
- Nové menu a toolbar akce `Fakturace`, které otevřou fakturační okno z hlavní aplikace.
- Fakturační okno obsahuje volitelná pole pro dodavatele, odběratele, číslo faktury, variabilní symbol, splatnost, poznámku a položky převzaté z detailního rozpočtu.
- Export návrhu faktury do JSON payloadu kompatibilního se strukturou Fakturoid API (`client_*`, `lines`, `quantity`, `unit_name`, `unit_price`, `vat_rate`).
- Fakturace umí vyhledat dodavatele i odběratele přes veřejné ARES REST API podle platného IČO po kliknutí na `Vyhledat`.
- Fakturace má samostatné uložení/načtení do souboru `*.eofinvoice`.
- Fakturační data lze uložit také přímo do hlavního projektu `*.eof`; při dalším otevření projektu se znovu předají do fakturačního okna.
- Přidán export faktury do jednoduchého PDF souboru.
- Přidána ochrana proti zavření fakturačního okna s neuloženými změnami.
- Přidáno okno `Nastavení` v menu `Možnosti` s první stránkou `Vzhled`.
- Přidána volba motivu `Dle systému`, `Světlý režim`, `Tmavý režim` přes `AppThemeService`.
- Přidány unit testy pro Fakturoid JSON, PDF export, validaci IČO a klonování fakturačního návrhu.
- Přidány integrační testy pro samostatné uložení/načtení fakturace a serializaci faktury do `ProjectData`.

### Změněno
- Výpočet ceny práce už nečte jednu spojenou položku `PriceItems`; cena se skládá z `WorkTask.BasePrice × BaseMaterial.BaseMaterialCoef × WorkPosition.PositionCoef × Quantity`.
- `WorkItemData` ukládá nové názvy polí `SelectedWorkTask`, `SelectedWorkSpecification`, `SelectedBaseMaterial`, `SelectedWorkPosition`.
- `MainViewModel` už nepřijímá ani neinstancuje zrušený `CalculationCascadeService`.
- XAML sekce PRÁCE je přepojená na nové bindingy `WorkTasks`, `AvailableWorkSpecifications`, `BaseMaterialsList`, `WorkPositionsList`.
- Kaskáda PRÁCE nyní v UI postupuje sekvenčně: `Úkon → Upřesnění → Podklad → Umístění`. Nižší ComboBoxy jsou zakázané, dokud není vybraná předchozí položka.
- Při změně vyšší položky v kaskádě PRÁCE se nižší výběry resetují, aby v řádku nezůstala neplatná kombinace.
- Unit a integrační testy byly přepsány ze starého modelu `PriceItems` / `WorkItem` na nový pracovní model.
- Databázové a katalogové testy ověřují nové tabulky práce (`Tasks`, `Specifications`, `BaseMaterials`, `Positions`, `TaskSpecifications`).
- `ProjectData` nově ukládá počet řádků v sekcích PRÁCE a MATERIÁL (`WorkRowCount`, `MaterialRowCount`), aby se zachoval i stav přidaných nebo odebraných prázdných řádků.
- Doporučený směr importu materiálů je CSV import přes UI s validací a mapováním sloupců, ne ruční SQL zásahy do databáze.
- Globální WPF styly a barvy jsou nově zapojené přes `App.xaml` a používají dynamické resources pro světlý/tmavý režim.
- Vzhled byl srovnán blíže ke standardnímu Windows chování; zůstávají jen ty vlastní styly a šablony, které jsou potřeba pro čitelnost světlého/tmavého režimu.
- Tmavý režim se nově aplikuje na hlavní okno, fakturaci, nastavení i okno O aplikaci včetně kořenových panelů, menu, toolbarů, vstupů, tabulek a systémových WPF barev.
- Výběr položek v tmavém režimu má nově vlastní kontrastní barvy pro ComboBox, ListBox a DataGrid, aby byl text vybrané položky čitelný.
- Opraveno přebíjení barvy textu u vybraných položek v tmavém režimu; text nyní dědí kontrastní barvu výběru i uvnitř ComboBoxů, ListBoxů a DataGrid buněk.
- Zavřený stav ComboBoxů má vlastní šablonu, aby po výběru položky zůstal text čitelný i v tmavém režimu.
- Menu a detailní rozpočet (`ListView`/`GridView`) mají doplněné tmavé styly pro čitelný text, hlavičky tabulky, řádky a výběr.
- Řádkové ceny práce a materiálu jsou barevně sjednocené, součty mají jasné odlišení podle sekce a deaktivované kaskádové výběry jsou v tmavém režimu čitelnější.
- Aplikace už nevyžaduje zdrojový soubor `ElektroOffer_app/elektrooffer.db` při buildu; při startu chybějící SQLite databázi založí a naplní testovacími daty ze SQL seedu `Data/Seed/elektrooffer_1_9_0.sql`.
- Vývojová DB cesta je sjednocená se SQLite Browser projektem: při běhu z Visual Studia aplikace používá `ElektroOffer_app/elektrooffer.db` vedle `elektrooffer.sqbpro`, ne samostatnou DB v `bin`.
- GitHub Actions workflow bylo zjednodušeno: běžné CI spouští restore/build/test, detailní diagnostický log se generuje jen při chybě a publish Release běží pouze při tagu.
- Unit a integrační testy jsou v GitHub Actions oddělené do samostatných kroků, aby bylo v logu hned vidět, která sada případně selhala.
- GitHub Actions joby jsou připravené na ruční schválení přes environment `manual-approval`.
- GitHub Actions nově ukládají krátký CI souhrn `elektrooffer-ci-summary` při každém běhu včetně úspěšných běhů.
- Komentáře v upravovaných třídách a datových modelech byly srovnány na aktuální stav 1.9.0; zavádějící migrační poznámky a zastaralé odkazy na `Guid` identifikátory byly odstraněny.

### Odstraněno
- `CalculationCascadeService` a model `PriceItems` jako stará spojená kaskáda PRÁCE.
- Testovací stub a testy navázané výhradně na zaniklou tabulku `PriceItems`.

### Opraveno
- Chyba konstruktoru `MainViewModel` po odebrání `CalculationCascadeService`.
- Neplatné bindingy v XAML na staré `Tasks`, `SelectedTask`, `AvailableSpecifications`, `SelectedMaterial`, `SelectedLocation`.
- Nefunkční uložení/načtení pracovních polí po přejmenování modelu `WorkItemData`.
- Uložení projektu po přidání nebo odebrání prázdného řádku nyní zachová počet řádků i tehdy, když řádek neobsahuje žádná kalkulační data.
- Opraveno nullable warning v integračním testu `RealMessageBoxServiceTests`.

### Ověřeno
- `dotnet build ElektroOffer_app.slnx`
- `dotnet build ElektroOffer_app.slnx -p:OutputPath=...\artifacts\verify-build\` kvůli běžící aplikaci zamykající standardní `bin` výstup
- `dotnet build ElektroOffer_app.slnx -p:OutputPath=...\artifacts\verify-theme\`
- `dotnet build ElektroOffer_app.slnx -p:OutputPath=...\artifacts\verify-selection\`
- `dotnet build ElektroOffer_app.slnx --configuration Debug`
- `dotnet test ElektroOffer_app.slnx --no-build`
- `dotnet test ElektroOffer_app.slnx --configuration Debug --no-build`

---

## [1.8.0.4] - 2026-07-10
### Opraveno
- Materiálové řádky (`CalculationItemViewModel.IsEmpty`) se nyní správně ukládají do JSON i při
  částečně vyplněné produktové kaskádě (Kategorie → Název → Dodavatel → Nabídka → Cena), stejně
  jako řádky Práce. Dříve kontrolovala `IsEmpty` pouze pracovní pole (Task/Specification/Material/
  Location) a Quantity, takže se materiálový řádek uložil až po vyplnění množství bez ohledu na to,
  kolik z kaskády už bylo vybráno.

---

## [1.8.0.3] - Unit Test Architecture Stabilization

### Přidáno

- Nová základní třída TestBase
  - obsahuje sdílený _db kontext
  - SetUp() nyní provádí EnsureDeleted + EnsureCreated
  - každý test běží v naprosto čisté databázi
  - odstraněny chyby typu UNIQUE constraint failed
  - odstraněny chyby způsobené kontaminací dat mezi testy
  - odstraněny zamčené SQLite handle
  - zajištěno reálné chování EF Core (Include, vztahy, lookupy, kaskády)
  - přidán dokument `UNIT-ViewModels.md` do složky `docs/` s kompletní tabulkou UNIT testů pro ViewModels

### Změněno
- kompletní refaktoring architektury UNIT testů
  - odstraněny partial testovací třídy
  - každá oblast testů má vlastní třídu (Total, Validation, CascadeWork, CascadeMaterial, PropertyChanged, IsEmpty)
  - všechny testy nyní dědí z nové základní třídy TestBase
  - sjednocený styl komentářů a struktury testů
- sjednoceno číslování UNIT testů pro ViewModels (T_001–T_100)
- opraveny názvy testů a doplněny detailní komentáře k jednotlivým testům
- aktualizována struktura testovacích souborů pro konzistentní pořadí a logické řazení

### Opraveno

- Chyby v testech
- T_23_Total_Should_Use_Price_From_Database_When_MaterialItem_Is_Loaded
- T_35_Total_Should_Use_Highest_MaterialPrice_From_Database
- T_07_CascadeMaterial_SelectedOffer_Should_Update_SelectedMaterialPrice
- příčina
- testy sdílely zbytky dat v SQLite → nyní odstraněno díky resetu DB před každým testem
- ppraveno nesprávné načítání cen materiálu (23.22 místo 100) způsobené kontaminovanou DB

### Odstraněno
- starý soubor CalculationItemViewModelTests_Base.cs
- všechny partial class definice testů
- zbytky systémového bordelu z chatu, které se zobrazovaly v komentářích

---

## [1.8.0.2] - Oprava pozicování řádků při Save/Load

### Přidáno
- Nové pole `Position` v `WorkItemData` a `MaterialItemData` – nese skutečnou pozici řádku v UI (1-based), nezávisle na `Id`

### Opraveno
- Vyplněné řádky, mezi kterými byly prázdné mezery (např. vyplněný 1. a 5. řádek), se po uložení a znovunačtení projektu chybně „srovnaly pod sebe“ (5. řádek se posunul na 2. pozici)
  - příčina: pozice řádku se odvozovala parsováním čísla z `Id` (např. „W-5“ → 5), zatímco `Id` se ve skutečnosti generovalo sekvenčně jen pro vyplněné řádky
- Duplicitní definice `BuildProjectData()` v `MainViewModel.cs` (CS0121/CS0111) vzniklá při vkládání opravené verze metody

### Změněno
- `BuildProjectData()` – `Position` se nyní zaznamenává podle skutečného indexu řádku v kolekci (`WorkCalcItems`/`MaterialItems`) ještě před odfiltrováním prázdných řádků; `Id` zůstává čistě sekvenční identifikátor (W-1, W-2… / M-1, M-2…) pro párování s `CalculationItemData`
- `ApplyProjectData()` – počet vytvářených prázdných řádků se nyní počítá dynamicky, zvlášť pro PRÁCI a MATERIÁL, jako maximum z minimálního počtu řádků (5) a nejvyšší hodnoty `Position` nalezené v uložených datech dané sekce
- `ApplyProjectData()` – vyplněné řádky se vkládají na index podle `Position` (`Position - 1`) místo dřívějšího parsování čísla z `Id`

### Poznámky k migraci dat
- Starší `.eof` soubory bez pole `Position` nejsou touto opravou zpětně kompatibilní (`Position` se deserializuje jako `0`, což by vedlo k chybnému indexu) – pokud je to relevantní, řešit v samostatném patchi

---

## [1.8.0.1] - UI vylepšení PRÁCE a MATERIÁL

### Přidáno
- Ikona WORK v sekci PRÁCE (zobrazení před nadpisem)

### Opraveno
- tlačítka u MATERIÁLU (přidání/mazání řádků)
- zarovnání a layout prvků v sekci PRÁCE a MATERIÁL

### Změněno
- odstraněny pokusy o Geometry ikonky (nekompatibilní s WPF)
- ikony nyní vloženy přímo jako Image (stabilní řešení)

---

## [1.8.0] - Více dodavatelů materiálu - DB + UI

### Přidáno
- Podpora více dodavatelů u materiálu – nové entity `Category`, `Supplier`, `MaterialPrice`
- Kaskádový výběr materiálu v kalkulaci: Kategorie → Název → Dodavatel → Materiál
  (obdobně jako existující kaskáda Task → Specification → Material → Location u práce)
- `MaterialCascadeService` – řízení kaskády výběru materiálu a dotažení ceny
- Naplánován import ceníku materiálu z CSV exportu Excel listu Import_Master s upsert logikou podle jednoznačného klíče dodavatele a položky.
- Unikátní databázový index na `MaterialPrices (SupplierId, SupplierCode)`
- Testovací sada 10 materiálů (kabely, chráničky, spínače, zásuvky, rozvaděče,  jističe, chrániče) s cenami od dvou dodavatelů (ELKOV, EMAS)
- uložené hodnoty pro práci `SelectedWorkPrice`, `SelectedWorkUnit`
- uložené hodnoty pro materiál `SelectedMaterialPriceValue`, `SelectedMaterialUnit`
- krátké lidsky čitelné identifikátory položek (W-1, W-2… pro práci, M-1, M-2… pro materiál)
- oddělené číselné řady pro PRÁCI a MATERIÁL.
- ID se ukládá do všech tří sekcí (`WorkItems`, `MaterialItems`, `CommonItems`)
- ID slouží jako jednoznačný klíč pro párování položek při načítání projektu
- podpora ukládání ID do JSONu (součást `ProjectData`)
- podpora načítání ID z JSONu `ApplyProjectData`
  - korektní spárování PRÁCE ↔ SPOLEČNÉ,
  - korektní spárování MATERIÁL ↔ SPOLEČNÉ

### Změněno
- `Material` rozšířen o vazbu na `Category` (partial třída, `Materials.cs`)
- `CalculationItemViewModel` doplněn o novou kaskádu produktového materiálu (`SelectedCategory`, `SelectedProductName`, `SelectedSupplier`, `SelectedOffer`, `SelectedMaterialPrice`) vedle stávající kaskády práce
- `CalculationPriceService.CalculateBaseTotal` – výpočet ceny materiálu nyní čte `SelectedMaterialPrice.Price` (cena od konkrétního vybraného dodavatele) místo dřívějšího jednotného `Material.Price`
- `MainWindow.xaml` – tabulka materiálu rozšířena o sloupce Kategorie, Dodavatel a Materiál (název položky od dodavatele)
- Opraveno chování tlačítka „SMAZAT“ / „RESET“
  - Reset nyní správně maže:
    - Kategorie `SelectedCategory`
    - Název `SelectedProductName`
    - Dodavatele `SelectedSupplier`
    - Nabídku `SelectedOffer`
    - Cenu `SelectedMaterialPrice`
  - Starý model `MaterialItem`
        - Množství, slevu, cenu řádku
- Oddělení datového modelu do tří sekcí:
  - `WorkItems` – pracovní hodnoty
  - `MaterialItems` – produktový materiál
  - `CommonItems` – společné hodnoty (množství, sleva, total)
- upraveno formátování slevy — vizuálně „–“, interně kladná hodnota
- výpočet ceny před slevou (`GrandTotalBeforeDiscount`)  
  - nyní z původních cen položek
- výpočet slevy u materiálu
  - používá `SelectedMaterialPrice.Price`
- výpočet MaterialDiscountTotal
  - sleva se zobrazuje jako kladná hodnota.
- buildProjectData() nyní generuje krátké ID místo GUID
  - PRÁCE: W-<číslo>
  - MATERIÁL: M-<číslo>
- `CalculationItemData.Id`, `WorkItemData.Id`, `MaterialItemData.Id` změněny na typ string
- odstraněny původní Guid Id.
- JSON je díky tomu čitelnější a stabilnější
- Parsování JSONu `ApplyProjectData` upraveno pro práci s ID typu string
  - odstraněny konverze GUID → string
  - odstraněny kolize mezi typy
  - načítání je nyní plně deterministické

### Poznámky k migraci dat
- Staré pole `Material.Price` ponecháno pro zpětnou kompatibilitu, plánováno k odstranění v pozdějším úklidovém patchi až po úplném přechodu na `MaterialPrice`
- Databázové tabulky (`Categories`, `Suppliers`, `MaterialPrices`, rozšíření `Materials` o `CategoryId`) vytvořeny ručně přes SQL (testovací data, bez EF Core migrace)

### Stabilizováno
- `ApplyProjectData()` korektně načítá tři oddělené datové sekce
- `BuildProjectData()` generuje čistý, přehledný JSON bez míchání dat
- výpočet Total je plně delegován do `CalculationPriceService`

### Na obzoru
- Doplnění a reálné vyzkoušení UI importu ceníku materiálu z CSV; zatím jsou testovací data vložená ručně přes SQL.
- Zobrazení kódu a ceny konkrétní nabídky (`SupplierCode`, `Price`) v detailním rozpočtu
  - aktuálně se v kalkulaci zobrazuje jen název položky od dodavatele
- Odstranění staršího pole `Material.Price` po úplném přechodu na `MaterialPrice`

---

## [1.7.7] – Stabilizace MVVM refaktoringu

### Opraveno
- **XAML parsing crash v `MainWindow.xaml`**
  Odstraněny cizí texty vložené do `MainWindow.xaml`, které způsobovaly `XamlParseException` a znemožňovaly načtení okna.
- **Chybějící `ICommand` vlastnosti v `MainViewModel`**
  - po přechodu na plné MVVM (`MainViewModel` jako náhrada `MainWindow.xaml.cs`) obsahoval ViewModel pouze obyčejné metody (`Save()`, `Load()`, `Print()`, `DeleteWorkItem(object?)`, atd.), ale žádné `ICommand` vlastnosti, na které se XAML bindoval (`{Binding SaveCommand}` apod.). To způsobovalo binding chyby typu *"Vlastnost XCommand se v objektu typu MainViewModel nenašla"* a nefunkční tlačítka/menu za běhu
  - přidány a inicializovány veřejné `ICommand` vlastnosti používané v XAML: `NewProjectCommand`, `LoadCommand`, `SaveCommand`, `SaveAsCommand`, `PrintCommand`, `ExitCommand`, `AboutCommand`, `AddWorkItemCommand`, `DeleteWorkItemCommand`, `ResetWorkItemCommand`, DeleteMaterialItemCommand`, `ResetMaterialItemCommand` – napojených na existující metody přes `RelayCommand`
- **`IFileDialogService is not configured` v `MainWindow.xaml.cs`**
  po refaktoringu na `MainViewModel` se `ProjectService` v konstruktoru `MainWindow` opět vytvářel přes bezparametrový konstruktor (`new ProjectService()`), takže interní `IFileDialogService`, `IFileSystemService` a `IMessageBoxService` zůstaly `null` a `Save()`/`Load()` vyhazovaly `InvalidOperationException`
  opraveno předáním reálných implementací z 1.7.6 (`RealFileDialogService`, `RealFileSystemService`, `RealMessageBoxService`) do DI konstruktoru `ProjectService`
- **Pořadí inicializace `DataContext` v `MainWindow`**
  - `MainWindow` nyní nejprve vytváří sdílený `AppDbContext` a všechny služby (`ProjectService`, `CatalogService`, `CalculationCascadeService`, `CalculationPriceService`, `MessageService`, `PrintService`, `ApplicationService`, `WindowService`), a teprve poté vytváří `MainViewModel` a nastavuje ho jako `DataContext`
    - zajišťuje, že ViewModel má při inicializaci k dispozici všechny závislosti
  - odstraněno duplicitní nastavování `DataContext` v `MainWindow.xaml.cs`.
- **Chování při zavírání okna (`CanClose()`)**
  - opraveno chování `MainWindow` při zavírání – `OnClosing` nyní korektně volá `MainViewModel.CanClose()` a v případě neuložených změn zabrání zavření okna (`e.Cancel = true`)
- **`WindowService.ShowAbout()`**
  - zajištěno správné vytvoření a zobrazení `AboutWindow` s nastaveným `Owner = Application.Current.MainWindow` a použitím `ShowDialog()` pro stabilní chování
- **Bindingy v XAML**
  - opraveny bindingy v `ItemsControl`/`DataTemplate` u položek práce a materiálu, v sekci rozpisu rozpočtu (práce + materiál), u slev (`DiscountPercent`, `DiscountAmount`) a u celkových součtů (`GrandTotalBeforeDiscount`, `GrandTotal`) – nově navázané na `MainViewModel` místo bývalého code-behind
- **`CalculationItemViewModelTests.cs`**
  - testy nekompilovaly po zavedení povinného DI konstruktoru `CalculationItemViewModel(AppDbContext db)` v 1.7.6
  - doplněn chybějící `using Microsoft.EntityFrameworkCore;` (chyba *"Typ nebo název oboru názvů DbContextOptionsBuilder<> se nenašel"*)
  - doplněn chybějící `using Microsoft.Data.Sqlite;`
  - všechna volání `new CalculationItemViewModel { ... }` upravena na `new CalculationItemViewModel(_db) { ... }` (16 výskytů)
  - zaveden sdílený `[SetUp]`/`[TearDown]` vytvářející SQLite InMemory databázi (`SqliteConnection` + `AppDbContext`) pro každý test
  - test `Total_Should_Calculate_MaterialItem_When_WorkItem_Is_Null` přepsán z EF InMemory provideru (`UseInMemoryDatabase`) na SQLite InMemory
    - sjednoceno s principem projektu a opraven chybějící balíček `Microsoft.EntityFrameworkCore.InMemory`, který ani nebyl v `.csproj`

### Přidáno
- **Spolehlivá implementace `RelayCommand`**
  - jednoduchá, otestovaná implementace `ICommand` pro MVVM (`Action<object?> execute`, volitelný `Func<object?, bool>? canExecute`), použitá pro všechny nové Command vlastnosti v `MainViewModel`
- **Službová vrstva pro `MainViewModel`**
  - zavedeny UI abstrakce `MessageService`, `PrintService`, `ApplicationService` a `WindowService`, díky nimž `MainViewModel` nevolá WPF přímo (viz `IMessageService`, `IPrintService`, `IApplicationService`, `IWindowService`)

### Odstraněno
- **`DialogService.cs`** – nepoužívaná služba (Info/Warning/Error/Confirm dialogy). Ověřeno přes "Find All References" – žádné odkazy v celém řešení. `MainViewModel` používá pro potvrzovací dialogy `IMessageService`/`MessageService` (`ShowYesNo`, `ShowYesNoCancel`)

### Poznámka k návrhu
- **Bindingy v `DataTemplate`**
  - pokud tlačítko uvnitř `DataTemplate` (např. u položek `WorkCalcItems`/`MaterialItems`) nemá `DataContext` nastavený přímo na `Window`, doporučuje se u Command bindingu použít `ElementName` nebo ověřit `AncestorType` v `RelativeSource`, aby binding správně našel `MainViewModel`
- Potvrzeno, že `CalculationItemViewModelTests.cs` odpovídá zavedenému
  vzoru testovací DB izolace (viz `CalculationItemViewModel_AdvancedTests.cs`
  a integrační testy) – SQLite InMemory přes otevřené připojení, ne EF
  InMemory provider

---

## [1.7.6] – DI refaktoring, testovací pokrytí a stabilizace ProjectService / CalculationItemViewModel (UNIT + INTEGRATION)

### Přidáno
- **DI konstruktor pro `CalculationItemViewModel`**
  Umožňuje předat `AppDbContext` zvenčí (aplikace → `elektrooffer.db`, testy → SQLite InMemory).
- **Rozhraní pro abstrakce služeb** (`Services/Abstractions/`):
  - `IFileDialogService.cs` – práce se souborovými dialogy (Open/SaveFileDialog)
  - `IFileSystemService.cs` – práce se souborovým systémem (`File.ReadAllText` / `WriteAllText`)
  - `IMessageBoxService.cs` – zobrazování MessageBoxů (Yes/No/Cancel) 
  *(Původní sloučený soubor `Abstractions.cs` byl rozdělen do samostatných souborů výše a smazán.)*
- **Reálné implementace služeb** (`Services/Implementations/`) pro integrační testy:
  - `RealFileDialogService.cs` – skutečné WPF dialogy pro ukládání/načítání
  - `RealFileSystemService.cs` – skutečné čtení/zápis souborů na disk
  - `RealMessageBoxService.cs` – skutečné WPF MessageBox dialogy 
  Tyto třídy neobsahují žádnou logiku `ProjectService` – pouze zprostředkovávají reálné chování. Unit testy nadále používají mocky (`Mock<IFileDialogService>` atd.).
- **Ochrana proti záporné ceně při slevě:**
  - Sleva ≥ 100 % → `Total` = 0
  - Sleva < 0 % → sleva se ignoruje
- **Nové UNIT testy:**
  - `VersionTests` – ověřuje správné nastavení verze aplikace (assembly metadata): že verze není prázdná, má validní formát a není defaultní `1.0.0.0`
  - `CalculationItemViewModelTests` (základní logika: Total, sleva, Quantity)
  - `CalculationItemViewModel_AdvancedTests` (PropertyChanged, edge-case scénáře, reset kaskády)
  - `RelayCommandTests` (MVVM command logika)
  - `RepositoryEdgeCaseTests` (CRUD chybové stavy — null objekt, update/delete neexistujícího záznamu)
  - `CatalogServiceTests` – načítání ceníku práce a materiálu, `Distinct()` u Tasks, `IsCatalogEmpty()`, práce se SQLite InMemory databází
  - `ProjectServiceTests` – `Save`, `SaveAs`, `Load`, `ConfirmNewProject`, `ExportCatalog`, `ImportCatalog` (s mockováním dialogů, MessageBoxů a souborových operací)
  - `RealFileSystemServiceTests` – ověřuje základní funkčnost reálné implementace: zápis textu do souboru, čtení textu ze souboru, smazání souboru. Test je izolovaný, nepoužívá `ProjectService` ani databázi
- **Nové INTEGRATION testy:**
  - `ProjectServiceTests_Advanced` (reálné ukládání/načítání projektů)
  - `CatalogServiceTests_Advanced` (reálné načítání ceníku ze SQLite InMemory DB)
  - `CalculationItemViewModelIntegrationTests` (ViewModel + reálná DB)
  - `CalculationItemViewModel_CascadeTests` (kompletní kaskáda Task → Specification → Material → Location: resety, načítání dostupných hodnot, PropertyChanged, přepočet Total)
  - `RealFileDialogServiceTests` (`[Explicit]`) – ověřuje, že metody služby nevyhodí výjimku a lze je bezpečně volat v testovacím prostředí. Reálné dialogy se v testu neotevírají – cílem je ověřit stabilitu implementace, ne UI chování
  - `RealMessageBoxServiceTests` (`[Explicit]`) – ověřuje, že metoda nevyhodí výjimku a vrátí validní `MessageBoxResult`. Reálné UI dialogy se v testovacím prostředí neotevírají – jde o smoke-test, ne funkční test UI

### Změněno
- `CalculationItemViewModel` už nevytváří vlastní `AppDbContext`. Všechny metody (`LoadSpecifications`, `LoadMaterials`, `LoadLocations`, `LoadWorkUnit`, `UpdateWorkItem`) nyní jednotně používají injektovaný `_db`
- refactoring výpočtu `Total`: sjednocený výpočet pro práci i materiál, bezpečná aplikace slevy, přehlednější komentáře
- testy upraveny tak, aby používaly SQLite InMemory databázi místo EF InMemory provideru (odpovídá reálnému chování `AppDbContext`)
- testovací dvojník přejmenován na `CalculationItemViewModelStub.cs` pro jasné odlišení od produkčního ViewModelu
- integrace `ProjectService` nyní používá DI konstruktor se skutečnými implementacemi služeb → integrační testy korektně ověřují reálné chování aplikace (File I/O, MessageBox, dialogy)
- přejmenováno: `versionTests.cs` (soubor) → `VersionService.cs`
- **UI integrační testy `RealFileDialogServiceTests` a `RealMessageBoxServiceTests`** byly přesunuty z unit testů do integračních testů, protože využívají WPF dialogy (`OpenFileDialog`, `SaveFileDialog`, `MessageBox`), které vyžadují STA thread a nejsou kompatibilní s běžným unit test runnerem
- oba testy byly označeny jako `[Explicit]`, aby se nespouštěly automaticky v CI pipeline, která běží v prostředí bez UI (GitHub Actions). Lokálně je lze spouštět ručně přes Test Explorer
- testy běží v STA threadu (`[Apartment(ApartmentState.STA)]`), což je nutné pro WPF dialogy
- testy nyní automaticky vytvářejí testovací `.txt` soubor v `TempPath`, aby měly stabilní výchozí cestu a nevyužívaly poslední uloženou cestu Windows

### Opraveno
- **Duplicitní property `SelectedMaterial`** — druhá kopie (ve skutečnosti určená jako `SelectedLocation`) vznikla copy-paste chybou a způsobovala chyby kompilace (nejednoznačnost `CS0102` + „SelectedLocation neexistuje")
- **Testovací DB izolace** — metody `LoadWorkUnit()`, `LoadMaterials()` a `UpdateWorkItem()` si vytvářely vlastní `new AppDbContext()` místo použití injektovaného `_db`, takže v testech ignorovaly testovací in-memory databázi a dotazovaly se prázdné produkční DB → test `Changing_Specification_Should_Load_New_Materials` padal s `Expected: 1, But was: 0`
- sleva ≥ 100 % už nezpůsobuje záporné `Total`
- záporná sleva (< 0 %) se nyní správně ignoruje místo nesprávného chování
- **UNIT testy ProjectService** nyní používají DI konstruktor místo defaultního
  Dříve testy vytvářely `new ProjectService()` → interní služby (`IFileSystemService`, `IFileDialogService`, `IMessageBoxService`) byly `null` → reflexní volání `SaveToPath()` vyhazovalo výjimku *"IFileSystemService is not configured"*.
  Testy byly upraveny tak, aby používaly mockované služby
- **Inicializace ProjectService v MainWindow**
  Dříve se `ProjectService` vytvářel defaultním konstruktorem, což vedlo k runtime chybám typu *"IFileDialogService is not configured"* při volání `Load()` a `Save()`
  `MainWindow` nyní inicializuje `ProjectService` přes DI (`RealFileDialogService`, `RealFileSystemService`, `RealMessageBoxService`)

### Odstraněno
- odstraněny původní unit testy pro UI služby (`RealFileDialogServiceTests`, `RealMessageBoxServiceTests` v Unit projektu), které způsobovaly zasekávání test runneru a CI pipeline

---

## 1.7.5 - skripty + Git úpravy

### Přidáno
- **scripts/** folder with spouštěcí skripty:
  - `scripts/run-tests.bat` — dvojklikem spustí testy (Windows).
  - `scripts/commands/run-tests.ps1` — PowerShell skript spouštějící všechny unit a integration testy
  - `scripts/run-tests-unit.bat` — dvojklikem spustí UNIT testy (Windows).
  - `scripts/commands/run-tests-unit.ps1` — PowerShell skript spouštějící unit testy
  - `scripts/run-testsintegration.bat` — dvojklikem spustí integrační testy (Windows).
  - `scripts/commands/run-tests-integration.ps1` — PowerShell skript spouštějící integrační testy
  - `scripts/run-publish.bat` — wrapper pro publikaci.
  - `scripts/commands/run-publish.ps1` — PowerShell skript pro `dotnet publish`
- `CI workflow`: `.github/workflows/elektrooffer-ci-pipeline.yml` 
  - nyní spouští Publish a upload artefaktu pouze při vytvoření tagu
  - build, Unit testy a Integration testy se nyní spouští při každém push/pull requestu
  - `logování stavu`
    - minimální - vždy --> `elektrooffer_ci_pipeline`
    - detailní - pouze při tagu --> `elektrooffer_ci_pipeline_full` 

### Změněno
- přidána základní `.gitignore` pravidla pro `bin/`, `obj/` a `publish/`
- aktualizováno zobrazování verze aplikace (kompatibilita s .NET 10)
- upraven a zpřesněn vývojový workflow: feature → dev → test → main → tag
- aktivovány GitHub Rulesety pro povinné PR a CI na chráněných větvích

---

## [1.7.5.1] - BUGFIX: Ukládání a reset slevy (DiscountPercent, IsDiscountEnabled)

## Opraveno
- stav slevy se nyní správně persistuje do modelu před uložením/serializací
- `ClearAllItems` doplněno o reset slevy: `IsDiscountEnabled = false`, `DiscountPercent = null`
- `ResetWorkItem_Click` doplněno o reset slevy: `IsDiscountEnabled = false`, `DiscountPercent = null`
- `ResetMaterialItem_Click` doplněno o reset slevy: `IsDiscountEnabled = false`, `DiscountPercent = null`
- přidána synchronizace ViewModel → model (flush/snapshot) před exportem, aby JSON vždy odrážel stav v UI

---

## [1.7.4] - Integrační testy ProjectService (Save / Load + stabilní test framework)

### Přidáno
- `Tests/Integration/Services/ProjectServiceTests.cs` – nová integrační testovací třída pro `ProjectService`
  - testuje reálné chování service vrstvy v izolovaném prostředí
  - pokrývá scénáře ukládání a načítání projektů přes soubory (.eof)
  - využívá SQLite InMemory (SHARED mode) pro stabilní testovací prostředí
  - obsahuje kompletní setup a teardown pro každý test (čistá instance DB pro každý běh)
- Testovací infrastruktura (rozšíření v rámci testovací třídy)
  - `SqliteConnection _connection` – udržuje životnost InMemory SQLite databáze
  - `AppDbContext _db` – EF Core kontext pro případnou kontrolu datové vrstvy
  - `ProjectService _service` – System Under Test (SUT), testovaná business logika
  - důraz na izolaci testů a odstranění sdíleného stavu mezi testy
- Helper metody pro testování interní logiky service vrstvy
  - `InvokeSaveToPath(ProjectData data, string path)`
    - zpřístupňuje privátní metodu `SaveToPath` pomocí reflection
    - umožňuje testovat ukládání bez UI vrstvy (SaveFileDialog)
    - ověřuje serializaci dat do JSON a zápis na disk
  - `InvokeLoadFromFile(string path)`
    - simuluje načtení projektu z externího souboru
    - provádí deserializaci `ProjectData` z JSON
    - používá stejné `JsonSerializerOptions` jako produkční kód
- `Should_Save_Project_To_File_Correctly`
  testuje ukládání projektu do .eof souboru
  validuje:
    vytvoření souboru na disku
    správnost JSON serializace
    obsah uložených dat (`ProjectName`)
  - využívá `Path.GetTempPath()` pro izolaci testů
  - obsahuje dodatečnou kontrolu deserializace JSON (integritní validace dat)
  - provádí automatický cleanup vytvořeného souboru
- `Should_Load_Project_From_File_Correctly`
  - testuje načítání projektu ze souboru `.eof`
  - validuje:
    - správnost deserializace JSON → ProjectData
    - zachování hodnoty ProjectName
    - korektní návratová cesta souboru
  - obsahuje kontrolu integrity načtených dat (neprázdné hodnoty)
  - využívá reálný soubor vytvořený během testu
  - provádí cleanup testovacího souboru po dokončení
- Smoke test `Should_Be_Able_To_Initialize_ProjectService`
  - ověřuje inicializaci ProjectService
  - validuje správné sestavení testovacího prostředí
  - slouží jako základní health-check celé integrační vrstvy
  - netestuje business logiku, pouze inicializaci SUT

### Změněno
- `Tests/Integration/Services/ProjectServiceTests.cs`
  - rozšířena testovací infrastruktura o helper metody pro přístup k privátním metodám
  - doplněna detailní validace JSON dat po serializaci (ochrana proti regresím modelu)
  - rozšířené assertiony pro lepší diagnostiku chyb:
    - kontrola `null` návratových hodnot
    - kontrola existence souboru před čtením
  - sjednocen přístup k temp souborům (`Guid.NewGuid()` pro unikátnost)
  - doplněny dodatečné integrity checky u načtených dat
  - zpřesněna struktura Arrange / Act / Assert bloků pro lepší čitelnost testů

### Poznámka k návrhu
- Testy `ProjectService` jsou záměrně vedené jako black-box integrační testy
  - nezasahují do interní implementace service logiky
  - ověřují pouze vstupy a výstupy (souborový systém + JSON)
- Použití reflection (`InvokeSaveToPath`) je dočasné řešení pro testovatelnost
  - dlouhodobě vhodné řešení: přesun metody do `internal` + `InternalsVisibleTo`
- Testy jsou navrženy tak, aby byly rozšiřitelné o:
  - `ExportCatalog`
  - `ImportCatalog`
  - případně další file-based operace v `ProjectService`

---

## [1.7.3] - Sleva na řádek kalkulace

### Přidáno
- `ViewModels/Items/CalculationItemViewModel.cs` – podpora slevy na jednotlivý řádek kalkulace
  - Nová privátní pole `_isDiscountEnabled` (bool) a `_discountPercent` (double?)
  - Property `IsDiscountEnabled` – přepíná aktivaci slevy; při deaktivaci automaticky
    vynuluje `DiscountPercent` a vyvolá přepočet `Total`
  - Property `DiscountPercent` – procentuální hodnota slevy (null = sleva nezadána)
  - Obě properties volají `OnPropertyChanged(nameof(Total))` → UI se automaticky přepočítá
- `Models/BudgetItem.cs` – dvě nová pole pro zobrazení slevy v detailním rozpisu
  - `DiscountPercent` (double?) – procentuální výše slevy na řádku (null = bez slevy)
  - `DiscountAmount` (double?) – výše slevy v Kč = cena bez slevy minus cena se slevou (null = bez slevy)
- `Models/WorkItemData.cs` – dvě nová pole pro perzistenci slevy při ukládání projektu
  - `IsDiscountEnabled` (bool) – výchozí false → staré .eof soubory se načtou správně
  - `DiscountPercent` (double?) – výše slevy v procentech (null = nezadána)
- `Models/MaterialItemData.cs` – dvě nová pole pro perzistenci slevy při ukládání projektu
  - `IsDiscountEnabled` (bool) – výchozí false → staré .eof soubory se načtou správně
  - `DiscountPercent` (double?) – výše slevy v procentech (null = nezadána)
- `MainWindow.xaml.cs` – pět nových properties pro agregaci slev
  - `WorkDiscountTotal` – celková sleva na práci v Kč
  - `MaterialDiscountTotal` – celková sleva na materiál v Kč
  - `TotalDiscount` – celková sleva v Kč (práce + materiál)
  - `GrandTotalBeforeDiscount` – celková cena nabídky před odečtením slev
  - `HasAnyDiscount` – příznak pro XAML Visibility; true pokud existuje alespoň jedna nenulová sleva
- `Tests/Unit/LogicTests/PriceCalculationTests.cs`
  - přidán soubor s testy základní cenové logiky
  - ověřuje výpočet ceny z `BasePrice × MaterialCoef × PositionCoef`
  - pokrývá:
    - základní výpočet
    - koeficienty = 1 (neutralita)
    - koeficienty < 1 (snížení ceny)
    - koeficienty > 1 (navýšení ceny)
    - nulové hodnoty
    - záporné hodnoty
    - zaokrouhlení výsledku
- `Tests/Unit/LogicTests/DiscountCalculationTests.cs`
  - přidán soubor pro testování slevové logiky
  - ověřuje výpočet slevy `basePrice × (1 - percent / 100)`
  - pokrývá:
    - 10 % sleva (běžný scénář)
    - 0 % sleva (bez změny ceny)
    - 100 % sleva (nulová cena)
    - edge case nad 100 % (záporný výsledek)
    - správnost výpočtu slevy v izolované logice

### Změněno
- `ViewModels/Items/CalculationItemViewModel.cs` – přepočet `Total` rozšířen o aplikaci slevy
  - Původní přímý `return` nahrazen mezivýsledkem `baseTotal`
  - Sleva se aplikuje pouze pokud `IsDiscountEnabled == true` a `DiscountPercent` má hodnotu
  - Vzorec: `baseTotal * (1 - DiscountPercent.Value / 100.0)`
  - Bez aktivní slevy je chování identické s předchozí verzí
- `MainWindow.xaml.cs` – metoda `Recalculate()` rozšířena o výpočet slev
  - Přibyla lokální pomocná funkce `BaseTotal()` – počítá cenu řádku bez slevy
  - Výpočet `WorkDiscountTotal`, `MaterialDiscountTotal`, `TotalDiscount`,
    `GrandTotalBeforeDiscount` a `HasAnyDiscount` probíhá při každé rekalkulaci
  - `BudgetItem` se nově plní hodnotami `DiscountPercent` a `DiscountAmount`
    (null pokud řádek nemá aktivní slevu → XAML zobrazí prázdnou buňku)
- `MainWindow.xaml.cs` – metody `BuildProjectData()` a `ApplyProjectData()` rozšířeny
  - `BuildProjectData()` ukládá `IsDiscountEnabled` a `DiscountPercent` do `WorkItemData`
    a `MaterialItemData`
  - `ApplyProjectData()` obnovuje `IsDiscountEnabled` a `DiscountPercent` na každém
    řádku ViewModelu při načtení projektu
- `MainWindow.xaml.cs` – metoda `ExportAsText()` rozšířena o podporu slev
  - Řádky se slevou zobrazí tři dílčí řádky: cena bez slevy, výše slevy, cena se slevou
  - Řádky bez slevy zůstávají v původním jednořádkovém formátu
  - Sekce slev v součtu (cena před slevou + celková sleva) se tiskne pouze pokud
    `HasAnyDiscount` je true — stejná podmínka jako v UI
- `MainWindow.xaml` – detailní rozpočet rozšířen o dva nové sloupce za sloupcem Cena
  - `Sleva %` – procentuální výše slevy na řádku; prázdná buňka pokud sleva není zadána
  - `Sleva Kč` – výše slevy v Kč zvýrazněná červeně; prázdná buňka pokud sleva není zadána
- `MainWindow.xaml` – sekce celkové ceny rozšířena o tři řádky
  - `Cena před slevou` – zobrazí se pouze pokud `HasAnyDiscount == true`
  - `Celková sleva` – zobrazí se pouze pokud `HasAnyDiscount == true`
  - `Celková cena nabídky` – vždy viditelná; zobrazuje cenu po odečtení všech slev
- UNIT testy byly rozděleny do dvou samostatných testovacích tříd:
  - `PriceCalculationTests` – testy cenové logiky
  - `DiscountCalculationTests` – testy logiky slev
- sjednocen styl pojmenování testovacích metod (`Method_Should_ExpectedBehavior`)
- doplněny XML komentáře (`<summary>`) a podrobné komentáře pro lepší čitelnost a údržbu testů

### Poznámka k návrhu
Sleva je záměrně vedena na úrovni jednotlivého řádku (ne celého úseku). Důvod: různé
položky mohou mít různou výši slevy nebo být bez slevy zcela. Případná hromadná sleva
na celý úsek je připravena jako budoucí rozšíření (UI akce, bez zásahu do datového modelu).

---

## [1.7.2] - Verzování na jednom místě + oprava komentářů

### Přidáno
- `Services/ApplicationInfoService.cs` – nová service pro čtení metadat aplikace
  - Čte verzi z `AssemblyInformationalVersionAttribute` (generováno z `.csproj` při buildu)
  - Ořezává automaticky přidávaný `+commit_hash` suffix (např. `1.7.2+a3f2c8b` → `1.7.2`)
  - Záložní čtení přes `AssemblyName.Version` pokud `InformationalVersion` není dostupná
  - Vrací `"neznámá"` pokud nelze verzi určit

### Změněno
- `ElektroOffer_app.csproj` – verzování rozšířeno o všechny tři tagy:
  - `<Version>` – čte `ApplicationInfoService`, zobrazuje se uživateli v okně „O aplikaci"
  - `<AssemblyVersion>` – identifikace sestavení pro .NET runtime
  - `<FileVersion>` – zobrazuje se ve Vlastnostech `.exe` v Průzkumníku Windows
- `AboutWindow` (ViewModel) – `private string _version = "1.5.1"` nahrazeno voláním `ApplicationInfoService.Version`
  - Verze se napříště načítá automaticky z buildu, není třeba ji měnit ručně v kódu

### Opraveno
- Poškozené české znaky ve všech třech `.csproj` souborech
  - Příčina: soubory uložené jako Windows-1250, čtené jako UTF-8
  - Opraveny znaky jako `bal\xed\xe8ky` → `balíčky`, `ZM\xccN\xccNO` → `ZMĚNĚNO` apod.
  - Sjednoceno odsazení komentářů v `Tests.Unit.csproj` a `Tests.Integration.csproj`

### Technická poznámka
Od této verze stačí při vydání nové verze změnit číslo pouze na jednom místě – v `<Version>` tagu v `ElektroOffer_app.csproj`. Okno „O aplikaci" i případné další části aplikace si verzi načtou automaticky přes `ApplicationInfoService.Version`.

---

## [1.7.1] - NuGet úklid a oprava závislostí

### Opraveno

- **`Microsoft.Data.Sqlite`** downgradeován z `11.0.0-preview.5.26302.115` na `10.0.9`
  - Preview verze `11.x` patří do .NET 11 ekosystému a není kompatibilní s projektem cílícím na `net10.0`
  - Způsobovala chybu: *„Balíček není kompatibilní s net10.0-windows7.0"*
  - Opraveno ve všech třech projektech (App, Tests.Unit, Tests.Integration)

- **`SQLitePCLRaw.lib.e_sqlite3`** aktualizován z `2.1.11` na `3.50.3`
  - Verze `2.1.11` obsahuje známou **vysokou bezpečnostní zranitelnost** [GHSA-2m69-gcr7-jv3q](https://github.com/advisories/GHSA-2m69-gcr7-jv3q)
  - Verze `2.1.12` (nejbližší patch) není na NuGet publikována – resolved na `3.50.3`

- **Tranzitivní SQLitePCLRaw balíčky** explicitně pinovány na `3.0.3`
  - `SQLitePCLRaw.bundle_e_sqlite3`, `SQLitePCLRaw.core`, `SQLitePCLRaw.provider.e_sqlite3`
  - Tyto balíčky přicházely jako tranzitivní závislosti přes EF Core a držely zranitelnou verzi `2.1.11`
  - Explicitní pin v `.csproj` přebíjí tranzitivní požadavek

- **`NUnit`** v projektu `Tests.Integration` sjednocen z `4.6.1` na `3.14.0`
  - Nekonzistentní verze způsobovala potenciální konflikty při sdílení testovací infrastruktury

- **`NUnit3TestAdapter`** v projektu `Tests.Integration` sjednocen z `4.5.0` na `6.2.0`
  - Stejný důvod jako u NUnit – sjednocení s ostatními projekty

- **`Microsoft.Extensions.Hosting`** odebrán ze všech projektů
  - Balíček nebyl v žádném `.cs` souboru fakticky použit (žádné `IHost`, `HostBuilder`, `IHostedService`)
  - Zbytečná závislost táhla celý strom `Microsoft.Extensions.*` balíčků

- **`System.Data.SQLite.Core`** odebrán ze všech projektů
  - Starý SQLite wrapper pro `System.Data` namespace – **konfliktuje** s `Microsoft.Data.Sqlite` + EF Core
  - Projekt používá výhradně `Microsoft.Data.Sqlite` přes EF Core, tento balíček byl nadbytečný

- **Testovací balíčky** odebrány z hlavního WPF projektu (`ElektroOffer_app.csproj`)
  - `coverlet.collector`, `Microsoft.NET.Test.Sdk`, `NUnit`, `NUnit3TestAdapter`
  - Tyto balíčky patří výhradně do testovacích projektů, ne do produkční WPF aplikace

- **`DatabaseTests.cs`** přepsán z `System.Data.SQLite` API na `Microsoft.Data.Sqlite` API
  - Původní kód používal `SQLiteConnection` (starý namespace), `SQLiteConnection.CreateFile()` a `ClearAllPools()`
  - `Microsoft.Data.Sqlite` tyto metody nemá – soubor se vůbec nekompiloval
  - Nový kód: `SqliteConnection`, bez `CreateFile()` (soubor vznikne při `Open()`), bez `ClearAllPools()` (uvolnění je automatické)
  - Connection string změněn z `"Data Source=x;Version=3;"` na `"Data Source=x"` (Microsoft verze `Version=3` nepodporuje)

- **`EF Core Design` a `Tools`** verze sjednoceny z `10.0.8` na `10.0.9`
  - Drobná nekonzistence která mohla způsobovat downgrade warningy

### Technická poznámka

Během oprav docházelo k opakovanému přepisování `.csproj` souborů Visual Studiem zpět na staré verze.
Příčina: VS měl otevřené projekty a při detekci změn souborů na disku je přepisoval hodnotami ze své vnitřní cache (NuGet Package Manager v pozadí).
Řešení: Změny prováděny přes PowerShell při **zavřeném** Visual Studiu, následované `dotnet nuget locals all --clear` pro vyčištění NuGet cache.

---

## [1.7.0] - Print / Export

### Přidáno
- `ElektroOffer_app/ElektroOffer_app/Services/PrintService.cs` – základní servisní vrstva pro budoucí exporty (PDF / tisková logika)
- `MainWindow.xaml.cs`
  - implementace tisku pomocí `PrintDialog`
  - metoda `MenuPrint_Click` pro spuštění tisku kalkulace
  - metoda `ExportAsText()` pro generování textové reprezentace rozpočtu (PRÁCE + MATERIÁL + celkové součty)
- UI rozšířeno o možnost tisku:
  - menu položka „Tisk"
  - toolbar tlačítko „🖨 Tisk"
- Tiskový výstup zahrnuje:
  - detailní rozpis práce
  - detailní rozpis materiálu
  - součty jednotlivých sekcí
  - celkovou cenu nabídky

### Změněno
- `ProjectData.cs`
  - zpřesnění datového modelu (DTO pro serializaci)
  - odstranění/zamezení neexistujících odvozených property z modelu (např. `TotalPrice` mimo model)
- `MainWindow.xaml.cs`
  - sjednocení `using System.Windows.*` a odstranění duplicit
  - doplnění chybějících namespace pro tisk (`FlowDocument`, `Run`, `Paragraph`, `PrintDialog`)
- `BudgetItem`
  - zpřesnění modelu pro rozpis kalkulace (práce/materiál)
  - sjednocení datového toku pro export i UI
- Architektura aplikace:
  - jasné oddělení:
    - výpočty (`Recalculate`)
    - UI (MainWindow)
    - export/tisk (ExportAsText + PrintDialog)
  - příprava na budoucí PDF export (QuestPDF nebo Print-to-PDF pipeline)

### Opraveno
- odstraněny build chyby:
  - chybějící `ExportAsText`
  - chybějící WPF tiskové namespace
  - duplicity `using System.Windows`
- stabilizace kompilace solution (Unit + Integration testy + UI projekt)

### Poznámka
Tato verze zavádí první jednoduchý reporting/export vrstvu nad kalkulací bez externích knihoven. Slouží jako základ pro budoucí PDF export a pokročilé tiskové šablony (fakturační styl).

---

## [1.6.0] - Integrační testování
### Přidáno
- `ElektroOffer_app.Tests.Integration` – nový projekt pro integrační testy
- `DatabaseConnectionTests` – ověření připojení k SQLite databázi
- `DatabaseSchemaTests` – ověření vytváření databázového schématu
- `DatabaseCrudTests` – CRUD operace nad SQLite databází
- `CatalogServiceTests` – testování business logiky katalogu (PriceItems, Materials)
- `ProjectServiceTests` – testování ukládání a načítání projektů (.eof)

### Změněno
- Oddělení testů na:
  - Unit testy (`ElektroOffer_app.Tests.Unit`)
  - Integrační testy (`ElektroOffer_app.Tests.Integration`)
- Stabilizace SQLite InMemory testovací databáze (přechod na shared memory mode)
- Úprava testovací architektury pro podporu EF Core integračních testů

---

## [1.5.2] - GIT - ignorování dočasných souborů
### Přidáno
- `.gitignore` do složky projektu

### Změněno
- ignorace souborů → bin/, obj/, .vs/, TestResults/, *.user, *.suo, *.cache, *.log, *.db-shm, *.db-wal
  - vždy se generují automaticky
  - nikdy se necommitují
  - nikdy nejsou potřeba pro build, běh aplikace ani testy
  - jejich ignorování je 100% bezpečné

---

## [1.5.1] - Refaktoring pro testovatelnost
### Přidáno
- `CatalogService` – nová service pro načítání dat ceníku z databáze
  - Metoda `LoadCatalog(AppDbContext db)` vrací seznam úkonů a materiálů
  - Metoda `IsCatalogEmpty(AppDbContext db)` pro detekci prázdné DB (připraveno pro seed data)

### Změněno
- `MainWindow.LoadCatalogDataFromDb()` – logika DB dotazů přesunuta do `CatalogService`
  - MainWindow nadále volá tuto metodu beze změny, jen deleguje na service
  - Umožňuje integrační testování načítání ceníku bez závislosti na WPF

---

## [1.5.0]
### Přidáno
- Integrační testy databázové vrstvy (`DatabaseTests/`)
- Unit testy logiky kalkulací (`CalculationItemViewModelTests`)
- Repository testy (`RepositoryTests/PriceItemsRepositoryTests`, `MaterialRepositoryTests`)
- Skripty pro generování stromové struktury projektu (s výjimkami `bin`, `obj`, `.git`, `.vs`)

### Změněno
- Rozdělení solution na dva projekty: `ElektroOffer_app` a `ElektroOffer_app.Tests`
- Přesun `CHANGELOG.md` a `README.md` do složky `docs/` v root solution
- Přesun databáze `elektrooffer.db` do složky `ElektroOffer_app/Data/`

---

## [1.4.1]
### Přidáno
- Rozšířené XML doc komentáře (`/// <summary>`) napříč kódem

### Opraveno
- Správné zobrazení verze aplikace v dialogu „O aplikaci"
- Doplněny záznamy do `CHANGELOG.md`

---

## [1.4.0]
### Přidáno
- `README.md` s popisem projektu, struktury a ADR
- Zarovnání číselných sloupců doprava v tabulkách kalkulace

### Opraveno
- Zobrazení verze aplikace v dialogu „O aplikaci"

---

## [1.3.0] - Clean Code
### Změněno
- Refaktoring stromové struktury projektu (`Services/`, `ViewModels/`, `Commands/`)
- Kompletní revize kódu: doplnění komentářů, odstranění mrtvého kódu
