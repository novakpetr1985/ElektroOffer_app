# Changelog

Všechny důležité změny projektu jsou dokumentovány v tomto souboru.  
Formát vychází z [Keep a Changelog](https://keepachangelog.com/cs/1.0.0/).

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