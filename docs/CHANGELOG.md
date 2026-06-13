# Changelog

Všechny důležité změny projektu jsou dokumentovány v tomto souboru.  
Formát vychází z [Keep a Changelog](https://keepachangelog.com/cs/1.0.0/).

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

## [1.5.2] - GIT - ingorování dočasných souborů
### Přidáno
- `.gitignore` do složky projektu

### Změněno
- ignorace souborů -> bin/, obj/, .vs/, TestResults/, *.user, *.suo, *.cache, *.log, *.db-shm, *.db-wal
  - vždy se generují automaticky,
  - nikdy se necommitují,
  - nikdy nejsou potřeba pro build,
  - nikdy nejsou potřeba pro běh aplikace,
  - nikdy nejsou potřeba pro testy,
  - a jejich ignorování je 100% bezpečné.


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
