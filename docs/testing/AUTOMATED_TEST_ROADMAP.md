# Roadmap automatických testů

## Baseline

Před hotfixem 1.11.1 procházelo 63 unit a 30 integračních testů a kombinované řádkové pokrytí bylo přibližně 56,2 %. Po stabilizaci prošlo 76 unit a 33 integračních testů; kombinované pokrytí dosáhlo 57,4 %. ARES klient má 91,9 %, modely 88,8 %, persistence 70,6 %, tisk/PDF 75,9 %, služby 69,5 % a ViewModely 40,5 %. Největší mezera proto zůstává v hlavním a fakturačním ViewModelu, startu aplikace, motivu, stránkování a code-behind.

## Priority

| Priorita | Oblast | Chybějící scénáře | Cílový rozsah |
|---|---|---|---:|
| Hotovo | Databáze | AppData cesta, adopce staré DB, zachování dat, idempotentní seed, nekompatibilní DB | 70,6 % persistence; rozšířit migrace |
| Hotovo | ARES | mapování fixture, 400/404/429/500, timeout, cancellation, neplatný a neúplný JSON | 91,9 % klienta |
| P0 | Projekty | poškozený JSON, starší verze, zrušení dialogu, atomický Save/Save As | 80 % služby |
| P1 | ViewModely | příkazy, chybové stavy, dirty state, fakturační lookup a ukládání | 75 % větví |
| P1 | Tisk | dokumentové DTO, součty, DPH, zalamování a stránkovací model | 80 % builderu |
| P2 | UI | start oken, binding errors, motivy a accessibility smoke test | scénářově |
| P2 | CI | parsování TRX, diagnostický artifact a coverage artifact | scénářově |

Postup: měřit baseline, zabránit poklesu, pokrýt P0, zvýšit kritickou logiku na 70 %, poté 80 %. Triviální DTO, generovaný kód a čistě vizuální XAML se do cíle uměle nezapočítávají.

Manuální musí zůstat fyzický tisk, systémové dialogy, vizuální kontrola A4/PDF, odinstalace, branch protection v GitHub UI a skutečný online ARES smoke test. Online ARES test smí být pouze explicitní a standardně vypnutý.
