# Kritické manuální scénáře ElektroOffer 1.13.0-feature

Skutečný výsledek, stav a poznámku doplní tester. `P0` blokuje vydání, `P1` blokuje dotčenou funkci, `P2` je následná kontrola.

| ID | Oblast a název | Priorita | Předpoklady / data | Kroky | Očekávaný výsledek | Skutečnost / stav / poznámka | Automatizace |
|---|---|---:|---|---|---|---|---|
| DB-01 | První start bez DB | P0 | Záloha; chybí `%LocalAppData%\ElektroOffer` | Spustit aplikaci | Vznikne složka, schéma a výchozí katalog; aplikace startuje | — / Neprovedeno / — | Ano |
| DB-02 | Opakovaný start | P0 | DB z DB-01 | Přidat vlastní záznam; restartovat | Data zůstanou, seed se neduplikuje | — / Neprovedeno / — | Ano |
| DB-03 | Adopce staré DB | P0 | DB vedle staré aplikace; nový cíl chybí | Spustit novou verzi | DB se jednou zkopíruje do AppData, zdroj zůstane | — / Neprovedeno / — | Ano |
| DB-04 | Existující cílová DB | P0 | DB v AppData i stará DB | Spustit aplikaci | Cílová DB se nikdy nepřepíše | — / Neprovedeno / — | Ano |
| DB-05 | Starší schéma | P0 | Záloha DB s chybějící tabulkou | Spustit | Idempotentní schéma doplní bezpečné chybějící tabulky; data zůstanou | — / Neprovedeno / — | Částečně |
| DB-06 | Poškozená DB | P0 | Neplatný soubor DB v AppData | Spustit | Viditelná chyba; soubor se nesmaže ani nepřepíše | — / Neprovedeno / — | Ano |
| DB-07 | Reinstalace | P0 | Existující AppData DB | Odinstalovat aplikační soubory; znovu instalovat | Uživatelská DB zůstane, pokud ji odinstalátor explicitně nemaže | — / Neprovedeno / dle instalátoru | Ne |
| DB-08 | Jiný počítač | P1 | Čistý Windows účet | Instalovat a spustit | Vznikne vlastní nová DB; cizí uživatelská data nejsou v balíčku | — / Neprovedeno / — | Ne |
| DB-09 | Záloha a obnova | P1 | Zavřená aplikace | Zkopírovat DB; upravit data; vrátit zálohu | Po startu jsou obnovena zálohovaná data | — / Neprovedeno / — | Částečně |
| DB-10 | Demo vs. produkční data | P1 | Čistý profil | Prověřit první katalog | Výchozí katalog je jasně označen; nejsou přítomna cizí uživatelská data | — / Neprovedeno / seed vyžaduje další oddělení | Ne |
| PRJ-01 | Nový projekt | P0 | Spuštěná aplikace | Vyplnit práci a materiál | Součty a dirty state odpovídají vstupu | — / Neprovedeno / — | Ano |
| PRJ-02 | Save a Save As | P0 | Vyplněný projekt | Uložit; změnit; Uložit jako | Správné soubory, žádná ztráta dat | — / Neprovedeno / — | Ano |
| PRJ-03 | Přepsání souboru | P1 | Existující `.eof` | Uložit na stejnou cestu a potvrdit | Soubor je platný a znovu načitelný | — / Neprovedeno / — | Ano |
| PRJ-04 | Zrušení dialogu | P1 | Dirty projekt | Zrušit Save/Open dialog | Stav ani soubor se nezmění | — / Neprovedeno / — | Ano |
| PRJ-05 | Poškozený JSON | P0 | Neplatný `.eof` | Otevřít | Řízená chyba, současný projekt zůstane | — / Neprovedeno / — | Ano |
| PRJ-06 | Starší formát | P0 | Archivní `.eof` | Otevřít a znovu uložit kopii | Kompatibilní pole se zachovají | — / Neprovedeno / chybí verzované schema | Ano |
| PRJ-07 | Zachování položek | P0 | Práce, materiál, množství, sleva | Save → zavřít → Load | Všechny hodnoty a součty jsou shodné | — / Neprovedeno / — | Ano |
| CALC-01 | Práce a materiál | P0 | Známé katalogové ceny | Přidat oba typy položek | Řádkové a celkové ceny odpovídají ručnímu výpočtu | — / Neprovedeno / — | Ano |
| CALC-02 | Množství a sleva | P0 | Položka 960 Kč | Nastavit množství; slevu 10 % | Před slevou 960, sleva 96, po slevě 864 | — / Neprovedeno / — | Ano |
| CALC-03 | Nula a záporná hodnota | P0 | Nový řádek | Zadat 0 a záporné hodnoty | Hodnoty jsou validovány/clampovány, bez záporného součtu | — / Neprovedeno / — | Ano |
| CALC-04 | Maximální hodnoty | P1 | Velké množství/cena | Zadat hraniční hodnoty | Bez overflow, čitelné formátování | — / Neprovedeno / — | Ano |
| CALC-05 | Odstranění/reset | P1 | Více řádků | Odstranit a resetovat | Součty a návazné výběry se okamžitě opraví | — / Neprovedeno / — | Ano |
| PRT-01 | Windows PrintDialog | P0 | Vyplněná kalkulace; tiskárna | Tisk → vybrat tiskárnu | Dialog a tisk dokončeny bez pádu | — / Neprovedeno / — | Ne |
| PRT-02 | Microsoft Print to PDF | P0 | Jako PRT-01 | Vybrat virtuální PDF tiskárnu | Vznikne čitelné PDF se správnými daty | — / Neprovedeno / — | Ne |
| PRT-03 | Zrušení/bez tiskárny | P1 | Různé Windows profily | Zrušit dialog; test bez tiskárny | Bez změny projektu a bez pádu | — / Neprovedeno / — | Ne |
| PRT-04 | Mnoho položek | P0 | Více než jedna A4 | Tisk/PDF | Žádná data se neztratí; zalomení je čitelné | — / Neprovedeno / Windows tisk kalkulace vyžaduje ruční kontrolu | Částečně |
| PRT-05 | Čeština a kontrast | P0 | Text s diakritikou; grayscale | Exportovat oba druhy PDF | Znaky, součty a text jsou čitelné | — / Neprovedeno / ProfessionalA4 automatizováno, tisk ručně | Částečně |
| INV-01 | Plátce/neplátce DPH | P0 | Dvě faktury | Nastavit 21 % a 0 % | Řádky a celky odpovídají režimu | — / Neprovedeno / — | Ano |
| INV-02 | Splatnost a VS | P0 | Datum, 14 dní, VS | Exportovat a znovu načíst | Hodnoty zůstávají a jsou v PDF/JSON | — / Neprovedeno / — | Ano |
| INV-03 | Sleva a zaokrouhlení | P0 | Více řádků | Nastavit slevy a desetinné ceny | Součet faktury odpovídá rozpočtu | — / Neprovedeno / — | Ano |
| INV-04 | Dlouhé/zahraniční údaje | P1 | Dlouhá adresa; měna EUR | Exportovat | Bez ořezu dat a chyb formátu | — / Neprovedeno / jednoduché PDF omezené | Částečně |
| INV-05 | QR a více sazeb | P1 | Platný IBAN; 0/12/21 % | Vytvořit fakturu | ProfessionalA4 zobrazí rekapitulaci DPH a čitelný QR kód | — / Neprovedeno / QR ověřit dvěma bankovními aplikacemi | Částečně |
| ARES-01 | Platné IČO | P0 | Internet; známé IČO | Vyhledat jednou | Doplní veřejné údaje z oficiálního REST API | — / Neprovedeno / explicitní online smoke | Ne |
| ARES-02 | Neplatné/neexistující IČO | P0 | `123`, validní nenalezené IČO | Vyhledat | Validace nebo „nenalezeno“, bez pádu | — / Neprovedeno / — | Ano |
| ARES-03 | Timeout/offline/HTTP chyba | P0 | Odpojit síť nebo stub | Vyhledat | Čitelný stav chyby, UI zůstane použitelné | — / Neprovedeno / — | Ano |
| ARES-04 | Neúplná/poškozená odpověď | P0 | Lokální fixture | Spustit unit testy | Bez produkčního requestu; řízený výsledek | — / Neprovedeno / — | Ano |
| ARES-05 | Mock vs. produkce | P0 | Testovací sestavení | Spustit automatické testy a aplikaci | Testy používají handler; aplikace oficiální klient | — / Neprovedeno / — | Ano |
| FLD-01 | Export a načtení katalogu | P0 | Hlavní DB s prací a materiálem | Exportovat katalog verze 2; přenést a načíst v Terénu | Zobrazí se jen pracovní úkony a kategorie materiálu se stabilními identitami; ceny se v Terénu nepočítají | — / Neprovedeno / — | Částečně |
| FLD-02 | Offline zaměření | P0 | Telefon bez internetu | Zadat zakázku, místnosti, práci, materiál a poznámky | Data lze zadat, průběžně obnovit a exportovat bez sítě | — / Neprovedeno / — | Částečně |
| FLD-03 | Fotografie | P0 | Telefon s fotoaparátem | Přidat fotografie k místnosti i položce; exportovat | Balíček obsahuje fotografie, relativní cesty, velikosti a správné SHA-256 | — / Neprovedeno / — | Částečně |
| FLD-04 | Importní náhled | P0 | Platný `.eofmeasure` | Importovat; zkontrolovat mapování; vybrat řádky a přílohy | Práce vyplní jen úkon, materiál jen kategorii; množství se neduplikuje a cenu určí až dokončená hlavní kaskáda | — / Neprovedeno / — | Ano |
| FLD-05 | Opakovaný import | P0 | Již potvrzený balíček | Importovat stejný `.eofmeasure` znovu | Stejné `exportId` je odmítnuto a řádky se neduplikují | — / Neprovedeno / — | Ano |
| FLD-06 | Uložení projektu a příloh | P0 | Import s fotografiemi | Uložit `.eof`; zavřít; načíst; poté `Uložit jako` | Historie importu a přílohy zůstanou dostupné, kopie má vlastní `.assets` | — / Neprovedeno / — | Ano |
| FLD-07 | Android 9 smoke test | P0 | Samsung Galaxy S8 / Android 9 | Nainstalovat samostatné APK; spustit; vytvořit demo a export | Aplikace se spustí bez Fast Deployment závislostí a export dokončí | — / Částečně / spuštění ověřeno, export zbývá | Ne |
