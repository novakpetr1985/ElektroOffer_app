# Integrace ARES

## Rozhodnutí

ElektroOffer používá aktuální veřejné **REST/JSON API ARES** Ministerstva financí. SOAP služba pro tuto integraci není potřeba. Používaný endpoint je:

```text
GET https://ares.gov.cz/ekonomicke-subjekty-v-be/rest/ekonomicke-subjekty/{ico}
```

Oficiální zdroje:

- [ARES API na data.mf.gov.cz](https://data.mf.gov.cz/api/ares.html)
- [Informace Ministerstva financí k ARES](https://mf.gov.cz/cs/ministerstvo/informacni-systemy/ares)
- [Swagger UI ARES](https://ares.gov.cz/swagger-ui/)

## Produkční zapojení

`IAresClient` odděluje fakturační ViewModel od HTTP implementace. `AresLookupService` používá injektovaný `HttpClient`, časový limit 15 sekund, podporu zrušení požadavku a deserializaci JSON. Nenalezené IČO (`404`) vrací prázdný výsledek; jiné HTTP chyby se předají volajícímu a zobrazí se uživateli.

Produkční aplikace nemá přepínač na falešný ARES. Tím se předchází tomu, aby testovací data omylem vstoupila do skutečné faktury.

## Automatické testy a mockování

Testy nepoužívají síť ani skutečný ARES. Do `HttpClient` se vloží lokální `HttpMessageHandler`, který vrací verzované JSON fixtures. Sada ověřuje:

- platnou firmu a mapování adresy,
- neplátce DPH,
- nenalezené a neúplné údaje,
- odpovědi `400`, `429` a `500`,
- neplatný JSON,
- zrušení požadavku a timeout.

Takový mock testuje skutečnou HTTP a JSON hranici klienta a je pro REST API vhodnější než vytváření vlastní SOAP služby.

## Omezení a další krok

ARES může omezit nadměrné, paralelní nebo opakované automatické dotazy. Pro verzi 1.12+ doporučujeme doplnit krátkodobou cache podle normalizovaného IČO a omezený retry s exponenciálním čekáním pouze pro `429` a přechodné `5xx`. `400`, `404` ani chybný obsah se automaticky opakovat nemají.

Volitelný online kontraktní test proti oficiálnímu API má být ruční nebo plánovaný, nikoli součást každého CI běhu. Musí používat stabilní testovací IČO, nízkou frekvenci a nesmí zapisovat získaná data do repozitáře či logovat osobní údaje.
