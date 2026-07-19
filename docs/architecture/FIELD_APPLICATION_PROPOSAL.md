# Terénní měření a návazný výpočet ceny

Tento dokument je jediným zdrojem pravdy pro implementovanou podaplikaci měření, její samostatné ukládání a předání dat do kalkulace, nabídky a faktury.

## Cíl a hranice

Terénní aplikace pořizuje data na stavbě i bez internetu. Sama nevystavuje závaznou nabídku ani fakturu. Vytvoří technický návrh, který hlavní aplikace zobrazí v importním náhledu, přepočítá podle aktuálního ceníku a nechá uživatele potvrdit.

`Měření → importní náhled → návrh práce a materiálu → potvrzená kalkulace → nabídka/zakázka → faktura`

Každý krok lze uložit samostatně. Předání do dalšího kroku vytvoří verzovaný snapshot a nesmí tiše přepsat již potvrzená data.

## Doporučené rozdělení projektů

- `ElektroOffer.Contracts` — čisté .NET kontrakty, validace a verze schématu; bez WPF, EF Core a platformního UI.
- `ElektroOffer_app` — import, mapování na katalog, výpočet cen, výběr dodavatele a schválení uživatelem.
- `ElektroOffer.Field` — .NET MAUI offline klient pro Android a Windows s lokálním recovery konceptem a souborovým exportem.
- `ElektroOffer_app.Invoice` — samostatný fakturační modul, který přijímá pouze potvrzený snapshot.

První fáze nepotřebuje cloud ani SOAP službu. Přenos může bezpečně fungovat přes verzovaný souborový balíček. Synchronizační API má smysl až po ověření reálného offline workflow.

Terénní katalog verze 2 je odvozen přímo z hlavní databáze a obsahuje jen dvě úrovně: pracovní úkony a kategorie materiálu. Mobilní klient neudržuje vlastní kopii dodavatelů ani konkrétních výrobků. Po změně hlavního katalogu se vytvoří nový `.eofcatalog`; starší verze katalogu se odmítne a uživatel ji musí nahradit.

## Datový model měření

Základní kontrakty:

- `MeasurementProject` — identita měření, zákazník, místo, technik, časy a stav.
- `MeasurementArea` — objekt, patro, místnost nebo jiný měřený celek.
- `MeasurementCircuit` — okruh, jištění, fáze, rozvaděč a poznámky.
- `MeasurementItem` — naměřená délka, počet, plocha nebo jiná veličina.
- `WorkHint` — nezávazný návrh pracovního úkonu.
- `MaterialRequirement` — technický požadavek bez konkrétního dodavatele.
- `AttachmentReference` — relativní cesta, typ, velikost a kontrolní součet fotografie či přílohy.

Každá entita má stabilní GUID, `createdAt`, `updatedAt`, původ a verzi. Exportní balíček obsahuje `schemaVersion`, `exportId`, kontrolní součty a JSON data. Fotografie nejsou Base64 uvnitř JSON, ale samostatné soubory v balíčku.

## Převod měření na práci

Mapování musí být verzovaná doménová pravidla, ne pevně zapsané podmínky ve ViewModelu. Pravidlo může převést typ měření a jeho parametry na existující kaskádu:

`WorkTask → WorkSpecification → BaseMaterial → WorkPosition`

Výstupem je `WorkDraft` s množstvím, jednotkou, ID použitého pravidla a mírou jistoty. Importní náhled ukáže zdrojovou hodnotu, navržený katalogový záznam, výpočet a případné varování. Nejednoznačný nebo neúplný návrh musí uživatel ručně opravit.

Příklad: kabelová trasa 20 m ve zdivu může navrhnout 20 m odpovídajícího drážkování a 20 m pokládky kabelu. Cena práce se načte až v hlavní aplikaci z aktuálního katalogu a koeficientů, takže terénní balíček nezastará kvůli změně ceníku.

## Převod měření na materiál

Terénní aplikace nevkládá konkrétní nabídku dodavatele. Vytvoří `MaterialRequirement` s kategorií, technickými parametry, jednotkou, základním množstvím, rezervou a případně preferovaným výrobcem.

Příklad: požadavek CYKY 3×2,5, 20 m, rezerva 10 % znamená 22 m. Pokud se prodává balení po 25 m, porovnává se skutečná pořizovací cena 25 m, nikoli pouze katalogová cena za metr.

Hlavní aplikace hledá pouze technicky kompatibilní záznamy `Material` a jejich nabídky `MaterialPrice`. Nad nimi nabídne strategie:

- `Manual` — uživatel zvolí produkt i dodavatele.
- `LowestLineTotal` — nejnižší skutečná cena řádku po zohlednění balení a minimálního množství.
- `PreferredSupplier` — přednost dostane zvolený dodavatel, pokud splní technické podmínky.
- `ShortestLeadTime` — nejkratší známá dostupnost; neznámá dostupnost není automaticky nejlepší.
- `BestScore` — budoucí vážené hodnocení ceny, dostupnosti, počtu dodavatelů a uživatelských preferencí.

Cena bez dopravy, dostupnosti nebo data platnosti musí být viditelně označena jako neúplná. Automatický výběr nikdy nesmí zaměnit technicky nekompatibilní výrobek jen proto, že je levnější.

## Profily dodavatelů a regulární výrazy

Volba „Další“ může být deklarativní importní profil pro dosud nepodporovaný ceník. Profil mapuje sloupce a může pomocí omezených regulárních výrazů rozpoznat kategorii, technický parametr nebo katalogový kód.

- regex má omezenou délku a povinný timeout,
- před uložením se otestuje na vzorových datech,
- nesmí spouštět skript ani uživatelský kód,
- výsledek vždy uvede použité pravidlo a míru jistoty,
- nízká jistota vyžaduje ruční potvrzení.

Pro známé dodavatele je vhodnější explicitní XLSX/CSV adaptér. Regex profil je rozšiřující mapování, ne náhrada validace katalogu.

## Ukládání, import a konflikty

- Podaplikace průběžně ukládá recovery kopii a dovolí explicitní uložení vlastního balíčku.
- Import nejprve ověří schéma, velikosti, cesty, kontrolní součty, rozsahy a duplicity.
- `exportId` zajišťuje idempotenci: opakovaný import stejného balíčku nevytvoří stejné řádky podruhé.
- Konflikt se nikdy automaticky nepřepíše; uživatel zvolí lokální, importovanou nebo sloučenou hodnotu.
- Potvrzená kalkulace ukládá snapshot cen a pravidel. Pozdější změna ceníku nesmí změnit starou nabídku nebo vystavenou fakturu.
- Faktura má vlastní `.eofinvoice`; projekt může uchovat její snapshot. Autosave není náhradou explicitního uložení.

Balíček nesmí obsahovat spustitelný obsah ani uživatelský XAML. Při rozbalení se normalizují cesty, zakáže `..` a kontrolují maximální velikosti. Citlivá data zůstávají v uživatelském úložišti; pozdější export lze doplnit o šifrování.

## Stav a další rozvoj do verze 2.0

1. **1.13.0 — implementováno:** sdílené kontrakty a validace, export katalogu, MAUI klient, recovery koncept, fotografie, `.eofmeasure`, importní náhled, idempotence a persistence příloh.
2. **1.14.x — plánováno:** rozšířené mapovací profily, pravidla balení materiálu a bezpečnější asistovaný výběr kompatibilních položek a dodavatelů.
3. **1.x — plánováno:** řízené daňové nastavení fakturace; sazba nesmí být automaticky odvozena pouze z označení práce nebo materiálu.
4. **2.0.0 — cíl:** stabilní end-to-end tok měření, kalkulace, nabídky/zakázky, profesionálního tisku a faktury se sledovatelnými snapshoty.

Stávající mobilní klient úmyslně nevytváří druhou databázi cen. Katalog nese stabilní identity a popisy pro zadání v terénu; závazná cena se načte až v hlavní aplikaci.

## Stav implementace 1.13.0-feature

End-to-end offline workflow je implementován bez cloudu a serveru:

1. Hlavní aplikace vyexportuje aktuální databázový katalog do `.eofcatalog`.
2. ElektroOffer Terén katalog načte a umožní vybrat pracovní úkon nebo kategorii materiálu, zadat množství, místnost, poznámku a fotografie.
3. Terénní aplikace průběžně ukládá recovery koncept a exportuje `.eofmeasure`.
4. Hlavní aplikace balíček bezpečně ověří, zkontroluje cesty, velikosti a SHA-256 příloh.
5. Importní náhled spáruje stabilní katalogové kódy; u ručních položek použije konzervativní textové mapování.
6. Uživatel vybere práce, materiály a fotografie. Nevyřešené položky se automaticky nevloží.
7. Potvrzený pracovní řádek předvyplní pouze úkon a množství; materiálový řádek pouze kategorii a množství. Zbývající kaskádu uživatel dokončí v hlavní aplikaci a teprve tím zvolí cenu.
8. Projekt uloží historii `exportId`, mapování, zdrojový balíček a fotografie do doprovodné složky `.assets`.
9. Opakovaný import stejného `exportId` je zablokován. „Uložit jako“ kopíruje také doprovodné přílohy.

Praktický postup: `Soubor → Exportovat katalog pro terén` v hlavní aplikaci, `Katalog` v terénní aplikaci, po zaměření `Uložit a exportovat`, a nakonec `Soubor → Importovat terénní měření` v hlavní aplikaci.
