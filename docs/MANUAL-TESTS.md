# Manuální testy - ElektroOffer 1.13.0-feature

Datum aktualizace: 2026-07-19

Testovaná větev: `feature/1.13.0`
Testovaná verze: `1.13.0-feature`

Strukturované release scénáře s ID, prioritou, daty, očekáváním a polem pro výsledek jsou v `docs/testing/MANUAL_TEST_SCENARIOS.md`.

Tento checklist doplňuje automatické unit a integrační testy. Používej ho hlavně před PR do `dev`, `test` nebo `main`, případně před tagem/release.

---

## 1. Start Aplikace A Databáze

### 1.1 Čistý start po smazání dočasných souborů
1. Zavři aplikaci i Visual Studio.
2. Smaž `bin`, `obj`, `.vs` a lokální `ElektroOffer_app/elektrooffer.db`.
3. Otevři řešení `ElektroOffer_app.slnx` ve Visual Studiu.
4. Spusť build.
5. Spusť aplikaci.
6. Ověř, že aplikace nastartuje bez chyby.
7. Ověř, že vznikne `%LocalAppData%\ElektroOffer\elektrooffer.db`.
8. Otevři tuto DB v SQLite Browseru a ověř výchozí katalog ze seedu.

Očekávání:
- `Tasks`: 5 záznamů
- `Specifications`: 5 záznamů
- `BaseMaterials`: 7 záznamů
- `Positions`: 4 záznamy
- `TaskSpecifications`: 6 vazeb
- `Materials`: 10 záznamů
- `MaterialPrices`: 20 záznamů

### 1.2 SQL Seed
1. Ověř, že seed je uložený v `ElektroOffer_app/Data/Seed/elektrooffer_1_9_0.sql`.
2. Ověř, že v aplikaci nejsou žádná jiná ručně vymyšlená seed data.
3. Ověř, že aplikace používá `%LocalAppData%\ElektroOffer\elektrooffer.db`, ne kopii v projektu, `bin` ani instalační složce.

---

## 2. Kaskáda Práce

### 2.1 Sekvenční odemykání
1. Otevři nový projekt.
2. V sekci PRÁCE přidej řádek.
3. Ověř, že pole `Upřesnění`, `Podklad` a `Umístění` jsou neaktivní, dokud není vybraný `Úkon`.
4. Vyber `Úkon`.
5. Ověř, že se odemkne pouze `Upřesnění`.
6. Vyber `Upřesnění`.
7. Ověř, že se odemkne `Podklad`.
8. Vyber `Podklad`.
9. Ověř, že se odemkne `Umístění`.

### 2.2 Reset nižších polí
1. Vyplň celý řádek PRÁCE.
2. Změň `Úkon`.
3. Ověř, že se vymaže `Upřesnění`, `Podklad` i `Umístění`.
4. Znovu vyplň řádek.
5. Změň `Upřesnění`.
6. Ověř, že se vymaže `Podklad` a `Umístění`.
7. Změň `Podklad`.
8. Ověř, že se vymaže `Umístění`.

### 2.3 Shoda vybraných a zobrazených názvů
1. V jednom řádku postupně vyber konkrétní `Úkon`, `Upřesnění`, `Podklad` a `Umístění`.
2. Po každém výběru ověř, že zavřený ComboBox zobrazuje přesně zvolený název.
3. Vyplň alespoň dva další řádky jinými kombinacemi.
4. Přepínej výběry v jednotlivých řádcích a ověř, že se názvy mezi řádky ani sloupci nezamění.
5. Zkontroluj, že `DETAILNÍ ROZPOČET` skládá popis práce ze stejných čtyř zvolených názvů ve správném pořadí.

### 2.4 Výpočet ceny práce
1. Vyber známou kombinaci `Úkon -> Upřesnění -> Podklad -> Umístění`.
2. Zadej množství.
3. Ručně spočítej `BasePrice × BaseMaterialCoef × PositionCoef × Quantity`.
4. Porovnej výsledek s cenou v řádku.
5. Zapni slevu a ověř, že řádkový sloupec `Po slevě` zobrazí výslednou cenu, ne původní cenu.

### 2.5 Barvy a čitelnost v tmavém režimu
1. Přepni aplikaci do tmavého režimu.
2. Ověř, že neaktivní kaskádová pole jsou jasně rozeznatelná, ale čitelná.
3. Ověř, že cena práce v řádku má stejný neutrální styl jako cena materiálu.
4. Ověř, že `Součet práce` i `Součet materiálu` používají stejnou zelenou barvu písma.

---

## 3. Materiál

### 3.1 Kaskáda materiálu
1. Přidej řádek v sekci MATERIÁL.
2. Vyber `Kategorie`.
3. Ověř, že se odemkne `Název`.
4. Vyber `Název`.
5. Ověř, že se odemkne `Dodavatel`.
6. Vyber `Dodavatel`.
7. Ověř, že se odemkne `Materiál`.
8. Vyber konkrétní nabídku a množství.
9. Ověř cenu a jednotku.

### 3.2 Součty materiálu
1. Vyplň více materiálových řádků.
2. Ověř výpočet jednotlivých cen.
3. Ověř, že `Součet materiálu` je zelený a barevně shodný se `Součtem práce`.
4. Zapni slevu na řádku a ověř přepočet.
5. Ověř, že řádkový sloupec `Po slevě` a `Součet materiálu` používají výsledné ceny po slevě.

---

## 4. Uložení A Načtení Projektu

### 4.1 Vyplněná data
1. Vyplň několik řádků PRÁCE i MATERIÁL.
2. Ulož projekt jako `*.eof`.
3. Zavři aplikaci.
4. Znovu otevři aplikaci a načti soubor.
5. Ověř, že se správně načetly výběry, množství, slevy a součty.
6. U každého řádku PRÁCE porovnej názvy `Úkon`, `Upřesnění`, `Podklad` a `Umístění` s hodnotami vybranými před uložením.
7. Ověř, že načtené názvy zůstaly ve správných sloupcích a odpovídají popisu v `DETAILNÍM ROZPOČTU`.

### 4.2 Prázdné řádky
1. Přidej několik prázdných řádků do PRÁCE i MATERIÁL.
2. Ulož projekt.
3. Načti projekt znovu.
4. Ověř, že počet prázdných řádků zůstal zachovaný.

### 4.3 Neuložené změny
1. Proveď změnu v projektu.
2. Zkus vytvořit nový projekt, otevřít jiný projekt nebo zavřít aplikaci.
3. Ověř, že se zobrazí potvrzovací dialog.
4. Ověř volby uložit, neukládat a zrušit.

---

## 5. Detailní Rozpočet A Souhrny

### 5.1 Detailní rozpočet
1. Vyplň položky PRÁCE i MATERIÁL.
2. Ověř, že `DETAILNÍ ROZPOČET` obsahuje oba typy položek.
3. Ověř sloupce `Typ`, `Popis`, `Množství (MJ)`, `Před slevou`, `Sleva %`, `Sleva Kč`, `Po slevě`.
4. Vytvoř pracovní položku `Osazení / El. krabice / Beton / Stěna`, nastav množství 5; cena před slevou musí být přesně 960 Kč. Potom nastav slevu 10 %.
5. Ověř hodnoty: `Před slevou` 960 Kč, `Sleva %` 10 %, `Sleva Kč` 96 Kč a `Po slevě` 864 Kč.
6. Zopakuj stejnou kontrolu pro materiálovou položku.
7. Vypni slevu a ověř, že `Před slevou` a `Po slevě` jsou shodné a slevové buňky jsou prázdné.
8. Ověř slevy 0 %, 100 % a desetinnou hodnotu; výsledná cena nesmí být záporná.
9. V tmavém režimu ověř čitelnost textu, hlaviček, řádků a vybraného řádku.

### 5.2 Celková nabídka
1. Ověř `Cena před slevou`, pokud existuje sleva.
2. Ověř `Celková sleva`.
3. Ověř `Celková cena nabídky`.
4. Ověř, že celková cena je zelená a dobře čitelná ve světlém i tmavém režimu.
5. Ověř vztah `Cena před slevou − Celková sleva = Celková cena nabídky`.

---

## 6. Fakturace

### ProfessionalA4, QR a automaticky uložený koncept

- [ ] Vyplnit fakturu s českými znaky, dvěma sazbami DPH a více než 25 položkami; ověřit opakované záhlaví a nedělené řádky.
- [ ] Exportovat s platným IBANem a načíst QR nejméně dvěma bankovními aplikacemi; ověřit účet, částku, CZK, splatnost, VS a zprávu.
- [ ] Odstranit IBAN; ověřit varování a PDF bez QR, bez pádu aplikace.
- [ ] Zavřít neuloženou samostatnou fakturu, aplikaci znovu spustit a potvrdit nabídku obnovy autosave konceptu.
- [ ] Uložit `.eofinvoice`; ověřit opětovné načtení bankovních údajů, DUZP a identifikátorů zakázky.
- [ ] Použít `Uložit do projektu`, uložit hlavní `.eof`, aplikaci restartovat a ověřit vloženou fakturu.
- [ ] Vytisknout barevně i černobíle a zkontrolovat kontrast, okraje A4, podpis a čitelnost tabulky.
- [ ] Před produkčním použitím nechat daňové náležitosti potvrdit účetním nebo daňovým specialistou.

### 6.1 Otevření z hlavní aplikace
1. Otevři fakturaci z hlavního menu.
2. Otevři fakturaci z toolbaru.
3. Ověř, že se položky přebírají z detailního rozpočtu.
4. U položky 960 Kč se slevou 10 % ověř ve fakturaci: jednotkovou cenu před slevou, `Před slevou` 960 Kč, `Sleva %` 10, `Sleva Kč` 96 Kč a `Po slevě` 864 Kč.
5. Ověř, že celková částka faktury obsahuje 864 Kč pouze jednou a sleva se znovu neodečte.

### 6.2 Samostatné spuštění fakturace
1. Ve Visual Studiu nastav `ElektroOffer_app.Invoice` jako startup projekt.
2. Spusť aplikaci.
3. Ověř, že se otevře samostatné okno fakturace bez hlavního ElektroOffer okna.
4. Ověř, že návrh faktury je prázdný a neobsahuje položky z detailního rozpočtu.
5. Vyplň libovolná fakturační pole a ověř, že funguje samostatné uložení do `*.eofinvoice`.
6. Zavři samostatnou fakturaci a ověř upozornění na neuložené změny, pokud data nebyla uložena.

### 6.3 Volitelná pole
1. Vyplň dodavatele, odběratele, IČO, DIČ, adresy, číslo faktury, variabilní symbol, datumy a poznámku.
2. Ověř, že žádné pole není povinné.
3. Ověř, že lze fakturu uložit i s částečně vyplněnými údaji.

### 6.4 ARES
1. Zadej platné IČO dodavatele a klikni `Vyhledat`.
2. Ověř doplnění údajů.
3. Zopakuj pro odběratele.
4. Zadej neplatné IČO.
5. Ověř, že aplikace nespadne a zobrazí rozumnou informaci.

### 6.5 Exporty
1. Exportuj Fakturoid JSON.
2. Ověř strukturu `client_*`, `lines`, `quantity`, `unit_name`, `unit_price`, `vat_rate`.
3. U zlevněné položky ověř, že `unit_price × quantity` odpovídá výsledné ceně po slevě.
4. Exportuj PDF.
5. Otevři PDF a ověř hlavičku, položky a výsledné součty po slevě.

### 6.6 Samostatné uložení faktury
1. Ulož fakturu jako `*.eofinvoice`.
2. Zavři fakturační okno.
3. Načti fakturu znovu.
4. Ověř shodu dat.
5. Ověř, že se zachovaly také ceny před slevou, procenta, částky slev a ceny po slevě.

### 6.7 Uložení faktury do projektu
1. Vyplň fakturační údaje.
2. Ulož hlavní projekt.
3. Zavři aplikaci.
4. Načti projekt znovu.
5. Otevři fakturaci a ověř, že se údaje načetly z projektu.

### 6.8 Neuložené změny ve fakturaci
1. Proveď změnu ve fakturačním okně.
2. Zkus okno zavřít.
3. Ověř upozornění na neuložené změny.

---

## 7. Vzhled

### 7.1 Přepínání režimů
1. Otevři `Možnosti -> Nastavení -> Vzhled`.
2. Vyzkoušej `Dle systému`, `Světlý režim`, `Tmavý režim`.
3. Ověř, že změna platí pro hlavní okno, fakturaci, nastavení i okno `O aplikaci`.

### 7.2 Tmavý režim
1. Ověř hlavní menu.
2. Ověř toolbar.
3. Ověř ComboBoxy po otevření i po zavření.
4. Ověř disabled ComboBoxy v kaskádách.
5. Ověř detailní rozpočet.
6. Ověř barvy součtů.

### 7.3 Windows 10 / Windows 11
1. Na Windows 10 ověř kompatibilní tmavý režim.
2. Na Windows 11 ověř jemně upravenou tmavou paletu.
3. Ověř, že UI zůstává čitelné při systémovém světlém i tmavém nastavení.

---

## 8. Tisk

### 8.1 Tisk nabídky
1. Vyplň položky.
2. Spusť tisk.
3. Ověř náhled/dialog.
4. Ověř, že se v tiskovém výstupu zobrazí položky a součty.

## 9. Import katalogu z XLSX

### 9.1 Platná šablona
1. Zkopíruj `docs/templates/ElektroOffer_Catalog_Import_Template_1.0.xlsx` mimo repozitář.
2. Změň cenu existující práce a materiálu, přidej novou práci se specifikací a nový materiál s dodavatelem a cenou.
3. V aplikaci zvol `Soubor -> Importovat katalog z Excelu...` a vyber soubor.
4. Ověř hlášení o počtu nových a aktualizovaných záznamů.
5. Ověř nové i změněné hodnoty v kaskádách PRÁCE a MATERIÁL bez restartu aplikace.

### 9.2 Opakovaný import
1. Importuj stejný soubor podruhé.
2. Ověř, že nevzniknou duplicitní úkony, materiály, dodavatelé ani dodavatelské ceny.
3. Ověř, že import žádný existující záznam automaticky nesmazal.

### 9.3 Chybné vazby a hodnoty
1. Do kopie šablony zadej u materiálu kategorii, která není v listu `Categories`.
2. Spusť import a ověř chybu s listem, řádkem a sloupcem.
3. Ověř, že se z chybného sešitu neuložila ani jeho jinak platná data.
4. Zopakuj kontrolu s nečíselnou cenou, duplicitním názvem a odstraněným povinným listem.

### 9.4 Zrušení výběru
1. Otevři import a dialog zavři bez výběru souboru.
2. Ověř, že aplikace nezobrazí chybu a databázi nezmění.

## 10. Offline terénní workflow

### 10.1 Export katalogu
1. V hlavní aplikaci zvol `Soubor → Exportovat katalog pro terén`.
2. Ulož `.eofcatalog` mimo repozitář.
3. Přenes soubor do testovacího telefonu nebo druhého počítače.
4. V ElektroOffer Terén zvol `Načíst katalog` a soubor načti.
5. Ověř, že jsou dostupné pracovní úkony a kategorie materiálu, nikoli dodavatelé a konkrétní výrobky, a že terénní aplikace nezobrazuje ani nepočítá ceny.
6. Po změně hlavního katalogu vytvoř nový export a ověř, že mobilní aplikace zobrazí nové katalogové číslo verze.

### 10.2 Zaměření bez internetu
1. Vypni na telefonu Wi-Fi i mobilní data.
2. Založ zakázku, vyplň zákazníka, adresu, technika a poznámku.
3. Přidej nejméně dvě místnosti a do každé práci i materiál.
4. Jednu položku vyber z katalogu a jednu zadej ručně.
5. Vyplň množství, jednotku, rezervu a poznámku.
6. Ukonči aplikaci a znovu ji spusť; ověř obnovení rozpracovaného měření.

### 10.3 Fotografie a export
1. Přidej fotografii k místnosti a další ke konkrétní položce.
2. Zkontroluj počet fotografií v aplikaci.
3. Zvol `Uložit a exportovat` a vytvoř `.eofmeasure`.
4. Přenes balíček zpět do hlavního počítače bez změny jeho obsahu.

### 10.4 Importní náhled a ceny
1. V hlavní aplikaci zvol `Soubor → Importovat terénní měření`.
2. Ověř údaje zakázky, místnosti, mapování položek a seznam fotografií.
3. Ověř, že katalogové položky jsou spárované přes stabilní kód a položka zadaná bez katalogu zůstane jako jediný nevyřešený řádek.
4. Vyber řádky a fotografie a import potvrď.
5. Ověř, že práce předvyplnila jen úkon a množství a materiál jen kategorii a množství; zbývající kaskádová pole jsou prázdná.
6. Ručně dokonči kaskádu a ověř, že cena se načetla z aktuální hlavní databáze, nikoli z telefonu.
7. Ověř, že nevyřešená položka nebyla automaticky vložena a žádný řádek se v seznamu neobjevil dvakrát.

### 10.5 Idempotence, projekt a přílohy
1. Importuj stejný `.eofmeasure` znovu a ověř odmítnutí stejného `exportId`.
2. Ulož projekt jako `.eof` a ověř vznik doprovodné složky `.assets` se zdrojovým balíčkem a vybranými fotografiemi.
3. Projekt zavři, znovu načti a otevři historii terénních importů.
4. Použij `Uložit jako` a ověř, že nová kopie projektu dostala vlastní úplnou složku `.assets`.

### 10.6 Android instalační smoke test
1. Spusť `scripts\commands\run-android-test-build.ps1`.
2. Ověř podpis APK a poznamenej jeho SHA-256.
3. Nainstaluj APK na podporovaný testovací telefon; pro Samsung Galaxy S8 s Androidem 9 bylo spuštění ověřeno.
4. Spusť aplikaci bez připojeného Visual Studia a ověř, že se ihned neukončí.
5. Načti testovací data, přidej fotografii a dokonči export.

## 11. GitHub Actions A PR Proces

### 11.1 CI na feature větvi
1. Pushni změnu do `feature/**`.
2. Ověř, že se spustí GitHub Actions workflow `ElektroOffer CI Pipeline`.
3. Ověř, že projde `Restore`, `Build solution` a právě jeden běh `Run unit tests`.
4. Ověř, že se při běžném pushi nespustí integrační sada ani release publish.
5. Ověř, že se uloží artifact `elektrooffer-ci-summary`.

Poznámka: Unit a integrační testy jsou v CI oddělené, aby bylo hned vidět, která sada případně selhala.

### 11.2 PR do dev/test/main
1. Vytvoř PR do `dev`, `test` nebo `main`.
2. Ověř, že se spustí build a právě jeden běh integračních testů; unit test se po stejném commitu z PR znovu nespouští.
3. Merge povol až po zeleném CI a ruční kontrole.

### 11.3 Release
1. Release publish se spouští pouze při tagu.
2. Ověř, že tag spustí build, unit i integrační testy a vytvoření Windows instalačních artefaktů.
3. Ověř, že běžný push ani PR release build nespouští.

### 11.4 Diagnostický log
1. Běžný krátký souhrn `elektrooffer-ci-summary` se vytvoří po každém běhu, včetně úspěšného běhu.
2. Dlouhý diagnostický log se vytvoří pouze při chybě CI nebo release jobu.

---

## Výsledky Testování

| Oblast | OK / Chyba | Poznámka |
|---|---|---|
| Start a DB | | |
| Kaskáda PRÁCE | | |
| Materiál | | |
| Save/Load | | |
| Detailní rozpočet | | |
| Fakturace | | |
| Vzhled | | |
| Tisk/export | | |
| Import katalogu XLSX | | |
| Offline terénní workflow | | |
| GitHub Actions | | |
