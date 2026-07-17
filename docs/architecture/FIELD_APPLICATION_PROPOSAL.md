# Terénní měření a návazný výpočet ceny

Tento dokument je jediným zdrojem pravdy pro budoucí podaplikaci měření, její samostatné ukládání a předání dat do kalkulace, nabídky a faktury.

## Cíl a hranice

Terénní aplikace pořizuje data na stavbě i bez internetu. Sama nevystavuje závaznou nabídku ani fakturu. Vytvoří technický návrh, který hlavní aplikace zobrazí v importním náhledu, přepočítá podle aktuálního ceníku a nechá uživatele potvrdit.

`Měření → importní náhled → návrh práce a materiálu → potvrzená kalkulace → nabídka/zakázka → faktura`

Každý krok lze uložit samostatně. Předání do dalšího kroku vytvoří verzovaný snapshot a nesmí tiše přepsat již potvrzená data.

## Doporučené rozdělení projektů

- `ElektroOffer.Contracts` — čisté .NET kontrakty, validace a verze schématu; bez WPF, EF Core a platformního UI.
- `ElektroOffer_app` — import, mapování na katalog, výpočet cen, výběr dodavatele a schválení uživatelem.
- `ElektroOffer.Field` — budoucí offline klient pro tablet/telefon; vhodný kandidát je .NET MAUI se SQLite.
- `ElektroOffer_app.Invoice` — samostatný fakturační modul, který přijímá pouze potvrzený snapshot.

První fáze nepotřebuje cloud ani SOAP službu. Přenos může bezpečně fungovat přes verzovaný souborový balíček. Synchronizační API má smysl až po ověření reálného offline workflow.

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

## Doporučená realizace do verze 2.0

1. **1.13.0 — kontrakty a validátor:** `ElektroOffer.Contracts`, JSON schema, ukázkový balíček a unit testy verzování, validace a idempotence.
2. **1.13.x — importní náhled:** načtení balíčku v hlavní aplikaci, diff, mapovací pravidla práce a ruční potvrzení.
3. **1.14.x — materiál a dodavatelé:** `MaterialRequirement`, kompatibilní kandidáti, balení, strategie ceny a bezpečné importní profily.
4. **1.x — samostatný offline prototyp:** lokální SQLite, autosave, fotografie a export/import bez serveru.
5. **2.0.0 — stabilní end-to-end tok:** měření, kalkulace, nabídka/zakázka, profesionální tisk a faktura se sledovatelnými snapshoty.

Toto pořadí nejprve stabilizuje kontrakt a cenovou logiku. Mobilní UI pak nevytváří druhý, nekompatibilní datový model.
