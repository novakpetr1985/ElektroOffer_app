# Větve a vydávání ElektroOffer

## Chráněné větve

`dev`, `test` a `main` jsou cílové větve Pull Requestů. Přímé změny, commity, push, rebase, cherry-pick a merge jsou zakázané. Skutečné nastavení GitHub Rulesets nebylo možné ověřit: dostupný GitHub konektor neposkytuje Branch Protection/Rulesets a GitHub CLI není v prostředí nainstalované. Samotná konfigurace workflow ochranu větví nedokládá; vlastník ji musí potvrdit v `Settings → Rules → Rulesets` nebo `Branches`.

## Směr změn

- Feature vzniká z nejnovější schválené vývojové větve a míří přes PR do `dev`.
- Stabilizační hotfix vzniká z nejnovější aktivní verze, nikoli z historické větve, a cíl PR určí vlastník podle toho, kde je daná verze nasazena.
- Release větev vzniká až po uzavření rozsahu verze. Verze se mění v obou aplikačních `.csproj`.
- Hotová release větev je zdrojem povinných Pull Requestů. Pro 1.12.0 se používá `release/1.12.0`; vlastník ji postupně ověří přes PR do `dev`, následně `test` a nakonec `main`. Do chráněných větví se nepushuje ani nemerguje přímo.
- Tag `vX.Y.Z` se vytváří až nad schváleným release commitem; tag spouští publish workflow.
- Oprava přijatá do produkční linie se vrací do aktivního vývoje samostatnou synchronizační větví a PR. Automatický merge mezi `main`, `test` a `dev` se neprovádí.

Konflikty se řeší pouze v pracovní větvi, následuje kompletní build a testy a teprve potom nový review. Historie chráněných větví se nepřepisuje.
