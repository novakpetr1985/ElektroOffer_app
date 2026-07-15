# Manuální testy - ElektroOffer 1.9.0

Datum aktualizace: 2026-07-15  
Testovaná větev: `feature/1.9.0`  
Testovaná verze: `1.9.0`

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
7. Ověř, že vznikne `ElektroOffer_app/elektrooffer.db`.
8. Otevři `ElektroOffer_app/elektrooffer.sqbpro` a ověř, že DB obsahuje testovací data ze seedu.

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
3. Ověř, že aplikace při běhu z Visual Studia používá `ElektroOffer_app/elektrooffer.db`, ne kopii v `bin`.

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

### 2.3 Výpočet ceny práce
1. Vyber známou kombinaci `Úkon -> Upřesnění -> Podklad -> Umístění`.
2. Zadej množství.
3. Ručně spočítej `BasePrice × BaseMaterialCoef × PositionCoef × Quantity`.
4. Porovnej výsledek s cenou v řádku.
5. Zapni slevu a ověř přepočet.

### 2.4 Barvy a čitelnost v tmavém režimu
1. Přepni aplikaci do tmavého režimu.
2. Ověř, že neaktivní kaskádová pole jsou jasně rozeznatelná, ale čitelná.
3. Ověř, že cena práce v řádku má stejný neutrální styl jako cena materiálu.
4. Ověř, že `Součet práce` je zelený.

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
3. Ověř, že `Součet materiálu` je modrý.
4. Zapni slevu na řádku a ověř přepočet.

---

## 4. Uložení A Načtení Projektu

### 4.1 Vyplněná data
1. Vyplň několik řádků PRÁCE i MATERIÁL.
2. Ulož projekt jako `*.eof`.
3. Zavři aplikaci.
4. Znovu otevři aplikaci a načti soubor.
5. Ověř, že se správně načetly výběry, množství, slevy a součty.

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
3. Ověř sloupce `Typ`, `Popis`, `Množství (MJ)`, `Cena`, `Sleva %`, `Sleva Kč`.
4. V tmavém režimu ověř čitelnost textu, hlaviček, řádků a vybraného řádku.

### 5.2 Celková nabídka
1. Ověř `Cena před slevou`, pokud existuje sleva.
2. Ověř `Celková sleva`.
3. Ověř `Celková cena nabídky`.
4. Ověř, že celková cena je zelená a dobře čitelná ve světlém i tmavém režimu.

---

## 6. Fakturace

### 6.1 Otevření z hlavní aplikace
1. Otevři fakturaci z hlavního menu.
2. Otevři fakturaci z toolbaru.
3. Ověř, že se položky přebírají z detailního rozpočtu.

### 6.2 Volitelná pole
1. Vyplň dodavatele, odběratele, IČO, DIČ, adresy, číslo faktury, variabilní symbol, datumy a poznámku.
2. Ověř, že žádné pole není povinné.
3. Ověř, že lze fakturu uložit i s částečně vyplněnými údaji.

### 6.3 ARES
1. Zadej platné IČO dodavatele a klikni `Vyhledat`.
2. Ověř doplnění údajů.
3. Zopakuj pro odběratele.
4. Zadej neplatné IČO.
5. Ověř, že aplikace nespadne a zobrazí rozumnou informaci.

### 6.4 Exporty
1. Exportuj Fakturoid JSON.
2. Ověř strukturu `client_*`, `lines`, `quantity`, `unit_name`, `unit_price`, `vat_rate`.
3. Exportuj PDF.
4. Otevři PDF a ověř hlavičku, položky a součty.

### 6.5 Samostatné uložení faktury
1. Ulož fakturu jako `*.eofinvoice`.
2. Zavři fakturační okno.
3. Načti fakturu znovu.
4. Ověř shodu dat.

### 6.6 Uložení faktury do projektu
1. Vyplň fakturační údaje.
2. Ulož hlavní projekt.
3. Zavři aplikaci.
4. Načti projekt znovu.
5. Otevři fakturaci a ověř, že se údaje načetly z projektu.

### 6.7 Neuložené změny ve fakturaci
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

## 8. Tisk A Výstupy

### 8.1 Tisk nabídky
1. Vyplň položky.
2. Spusť tisk.
3. Ověř náhled/dialog.
4. Ověř, že se v tiskovém výstupu zobrazí položky a součty.

### 8.2 Export / import katalogu
1. Exportuj katalog do `*.eofcat`.
2. Načti katalog zpět.
3. Ověř, že materiály a práce zůstávají konzistentní.

---

## 9. GitHub Actions A PR Proces

### 9.1 CI na feature větvi
1. Pushni změnu do `feature/**`.
2. Ověř, že se spustí GitHub Actions workflow `ElektroOffer CI Pipeline`.
3. Ověř, že projde `Restore`, `Build solution` a `Run tests`.

Poznámka: `Run tests` spouští celé řešení, tedy unit i integrační testy.

### 9.2 PR do dev/test/main
1. Vytvoř PR do `dev`, `test` nebo `main`.
2. Ověř, že se spustí stejné CI.
3. Merge povol až po zeleném CI a ruční kontrole.

### 9.3 Release
1. Release publish se spouští pouze při tagu.
2. Ověř, že běžný push do feature větve release build nespouští.

### 9.4 Diagnostický log
1. Běžný krátký log je dostupný vždy v krocích workflow.
2. Dlouhý diagnostický log se vytvoří pouze při chybě CI nebo release jobu.

---

## 10. Regresní Kontrola Starého Modelu

1. Ověř, že aplikace nestartuje se starou chybou konstruktoru `MainViewModel`.
2. Ověř, že v UI nejsou rozbité bindingy na staré `SelectedTask`, `Specification`, `Material`, `Location`.
3. Ověř, že kaskáda PRÁCE nepoužívá starý `CalculationCascadeService`.
4. Ověř, že uložení projektu používá nová pole `SelectedWorkTask`, `SelectedWorkSpecification`, `SelectedBaseMaterial`, `SelectedWorkPosition`.

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
| GitHub Actions | | |
| Regrese starého modelu | | |
