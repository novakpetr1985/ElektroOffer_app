# Roadmap automatických testů

## Baseline

Pro 1.13.0-feature prochází 102 unit a 36 automatických integračních testů; tři testy systémových dialogů jsou záměrně `[Explicit]`. Nové testy pokrývají verzované terénní kontrakty, kontrolní součty a cesty v balíčku, mapování pouze na úkon/kategorii, blokování duplicitního `exportId`, ochranu proti dvojímu přidání řádku, kopírování fotografií a zachování historie při `Uložit jako`. Aktuální coverage po rozšíření 1.13.0 zatím nebylo znovu změřeno, proto se starší procenta nepoužívají jako současný baseline.

## Priority

| Priorita | Oblast | Chybějící scénáře | Cílový rozsah |
|---|---|---|---:|
| Hotovo | Databáze | AppData cesta, adopce staré DB, zachování dat, idempotentní seed, nekompatibilní DB | 70,6 % persistence; rozšířit migrace |
| Hotovo | ARES | mapování fixture, 400/404/429/500, timeout, cancellation, neplatný a neúplný JSON | 91,9 % klienta |
| Hotovo | Terénní kontrakty | round-trip, validace, bezpečné cesty, SHA-256 a idempotence importu | kritické scénáře pokryty |
| P0 | Terénní UI | recovery po ukončení, oprávnění fotografií, systémový picker a skutečný Android lifecycle | fyzické zařízení |
| P0 | Projekty | poškozený JSON, starší verze, zrušení dialogu, atomický Save/Save As | 80 % služby |
| P1 | ViewModely | příkazy, chybové stavy, dirty state, fakturační lookup a ukládání | 75 % větví |
| P1 | Tisk | dokumentové DTO, součty, DPH, zalamování a stránkovací model | 80 % builderu |
| P2 | UI | start oken, binding errors, motivy a accessibility smoke test | scénářově |
| P2 | CI | parsování TRX, diagnostický artifact a coverage artifact | scénářově |

Postup: měřit baseline, zabránit poklesu, pokrýt P0, zvýšit kritickou logiku na 70 %, poté 80 %. Triviální DTO, generovaný kód a čistě vizuální XAML se do cíle uměle nezapočítávají.

Manuální musí zůstat fyzický tisk, systémové dialogy, vizuální kontrola A4/PDF, odinstalace, branch protection v GitHub UI a skutečný online ARES smoke test. Online ARES test smí být pouze explicitní a standardně vypnutý.
