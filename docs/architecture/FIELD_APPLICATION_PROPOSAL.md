# Návrh terénní podaplikace

## Cíl a hranice

Terénní aplikace pořizuje data na stavbě offline. Nevytváří závaznou cenu ani fakturu. Výstupem je upravitelný návrh, který hlavní aplikace validuje a uživatel ručně schválí:

`Measurement → ProjectDraft → CalculationDraft → Offer/Budget → Order → Invoice`

## Projekty a kontrakty

Pro verzi 1.12 je vhodná knihovna `ElektroOffer.Contracts` bez závislosti na WPF, EF Core a UI. Kontrakty: `SiteMeasurement`, `MeasurementRoom`, `MeasurementCircuit`, `MeasurementItem`, `MeasurementPhotoReference`, `MeasurementNote`, `CustomerDraft`, `ProjectDraft`, `CalculationDraft` a `MeasurementExportPackage`.

Balíček JSON obsahuje `schemaVersion`, ID měření, čas, zákazníka, adresu, místnosti, okruhy, množství zásuvek/vypínačů/světel, kabelové trasy, materiály, rozvaděče, jištění, poznámky a relativní odkazy na fotografie. Import nejprve ověří verzi, povinná pole, rozsahy, duplicity a kontrolní součet příloh; až poté zobrazí náhled rozdílů.

## Offline a konflikty

První etapa používá lokální SQLite a export/import souboru, bez cloudového backendu. Každá entita má stabilní GUID a `updatedAt`. Fotografie jsou samostatné soubory v adresáři balíčku, ne Base64 v JSON. Při opakovaném importu se konflikt nikdy automaticky nepřepíše; uživatel volí lokální, importovanou nebo sloučenou hodnotu.

## Bezpečnost

- JSON schema a maximální velikosti polí/souborů.
- Normalizace cest a zákaz `..` při rozbalení příloh.
- Žádný spustitelný obsah ani uživatelský XAML.
- Citlivá data pouze v uživatelském úložišti; export lze později šifrovat heslem.
- Audit původu, verze a času importu.

## Technologie a verze

- 1.12.0: kontrakty, JSON schema, validátor a desktopový importní prototyp.
- 1.x: WPF/tablet prototyp, mapování měření do kalkulačních návrhů.
- 2.0.0: stabilní end-to-end MVP v hlavní aplikaci.
- Po 2.0: mobilní klient (například .NET MAUI) sdílející pouze kontrakty a validační logiku; synchronizace až po ověření offline workflow.
