# Životní cyklus SQLite databáze

## Umístění a distribuce

Produkční databáze je `%LocalAppData%\ElektroOffer\elektrooffer.db`. Instalační a publish složka obsahuje pouze aplikaci a výchozí SQL skript `Data/Seed/elektrooffer_1_9_0.sql`; databázový soubor není `Content` a nekopíruje se při buildu ani publishi.

Při prvním startu `AppDataPathProvider` vytvoří uživatelskou složku. Pokud cílová DB chybí a nalezne se starší `elektrooffer.db` vedle aplikace, pracovního adresáře nebo projektu, jednou ji zkopíruje. Existující cílový soubor nikdy nepřepisuje a starý zdroj nemaže.

## Inicializace

`DatabaseBootstrapService` při každém startu idempotentně provede část `CREATE TABLE/INDEX IF NOT EXISTS`. Pokud katalog obsahuje jakákoli data, seed se nepoužije. Jen zcela prázdný katalog dostane výchozí záznamy. Poškozená databáze se automaticky nemaže ani nenahrazuje.

Projekt zatím nemá EF Core migrace ani tabulku historie schématu. Hotfix bezpečně doplní chybějící tabulky/indexy, ale neumí obecnou migraci změn sloupců. Verzované migrace a oddělení systémových, demo a testovacích dat jsou úkol pro další feature.

## Scénáře

- První instalace: vznikne AppData DB, schéma a výchozí katalog.
- Existující DB: zůstane zachována; doplní se pouze idempotentní schéma a seed se neduplikuje.
- Reinstalace: běžné odstranění aplikačních souborů AppData nemaže; přesné chování musí potvrdit budoucí instalátor.
- Jiný počítač/účet: vznikne samostatná nová DB bez uživatelských dat jiného zařízení.
- Odstranění uživatelských dat: musí být explicitní samostatná volba se zálohou/potvrzením; aplikace ji automaticky neprovádí.
