# Tisk, PDF a šablony

## Současný stav 1.12.0

Tisk kalkulace vede cestou `MainViewModel.ExportAsText()` → `IPrintService.Print(string)` → WPF `FlowDocument` → systémový `PrintDialog`. Uživatel může vybrat fyzickou tiskárnu nebo `Microsoft Print to PDF`. Neexistuje náhled, tiskový DTO model, tabulka, opakované záhlaví ani řízené stránkování.

Faktura vede cestou `BudgetItem` → `InvoiceSourceItem` → `InvoiceDraft`/`InvoiceLine` → `InvoiceDocument` → `PdfInvoiceExportService`. QuestPDF šablona `ProfessionalA4` podporuje logo, české znaky, vícestránkovou tabulku, opakované záhlaví, souhrn po sazbách DPH, platební blok, lokální QR, poznámku a podpis. Windows náhled a přímý tisk stejného dokumentu ještě nejsou implementovány.

`ProfessionalA4` je jediná současná fakturační šablona. Používá existující fakturační model a design tokeny s bezpečnými fallbacky pro izolované testy. Řádky se mezi stránky nedělí a záhlaví tabulky se opakuje. Libovolný uživatelský XAML ani spustitelná šablona nejsou povoleny.

QR platba vzniká lokálně přes QRCoder; platební údaje neopouštějí počítač. SPAYD se vytvoří jen pro bankovní převod v CZK, kladnou částku, platný český IBAN (mod-97) a číselný variabilní symbol do 10 znaků. Tuzemský účet se na IBAN automaticky nepřevádí. Při neplatných údajích zůstane faktura tisknutelná bez QR a zobrazí se upozornění. Před produkčním vydáním je nutný ruční test alespoň ve dvou bankovních aplikacích bez potvrzení platby.

QuestPDF používá vlastní licenční model. Před produkční distribucí musí vlastník ověřit podmínky Community licence nebo zajistit odpovídající komerční licenci.

Data faktury se vkládají mapováním v `WindowService` a `InvoiceViewModel`: typ, popis, jednotka, množství, cena před slevou, sleva a konečná cena se převedou do řádků `InvoiceDraft`. Dodavatel, odběratel, číslo, splatnost, DPH a poznámky doplňuje uživatel. Windows tisk kalkulace naproti tomu dostává pouze hotový text, takže layout už nemůže bezpečně měnit.

## Bezpečný cílový návrh dalších kroků

1. `DocumentData`/`InvoicePrintData` — neměnná DTO data bez WPF.
2. `IDocumentDataFactory` — mapování projektu, kalkulace nebo faktury do DTO.
3. `PrintTemplateSettings` — JSON konfigurace loga, firmy, barev, patičky, viditelnosti bloků a varianty layoutu.
4. `IPrintDocumentBuilder` — vytvoření stránkovaného `FixedDocument` nebo řízeného `FlowDocument`.
5. `IWindowsPrintService` — náhled a systémový `PrintDialog` nad stejným dokumentem.
6. `IPdfExportService` — PDF ze stejného dokumentového modelu, aby tisk a PDF neměly rozdílné součty.

Uživatelský libovolný XAML se nesmí načítat. Bezpečné jsou předdefinované layouty a validovaný JSON v `%LocalAppData%\ElektroOffer\Templates`, s náhledem a návratem na výchozí nastavení. Konfigurace může obsahovat logo, název, adresu, IČ/DIČ, banku, IBAN/SWIFT, accent, patičku, podpis/razítko a viditelnost bloků.

## Rozdělení verzí

- 1.11.1: pouze audit, sdílené dokumentové tokeny a opravy stability.
- 1.12.0: implementována QuestPDF šablona `ProfessionalA4`, vícestránkové tabulky, souhrn DPH, lokální QR platba a bezpečné autosave konceptu. Windows tiskový náhled nad stejným dokumentem zůstává další fází.
- 1.12.x: validovaná konfigurace firemního profilu, registr více faktur v projektu a jednotný Windows náhled/tisk nad stejným dokumentovým modelem.
- 1.x: profesionální více­stránková nabídka, rozpočet a faktura včetně českých znaků.
- 2.0: stejné šablony pro tok měření → kalkulace → nabídka → zakázka → faktura.
