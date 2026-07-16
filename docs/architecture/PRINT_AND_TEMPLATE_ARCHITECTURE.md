# Tisk, PDF a šablony

## Současný stav 1.11.1

Tisk kalkulace vede cestou `MainViewModel.ExportAsText()` → `IPrintService.Print(string)` → WPF `FlowDocument` → systémový `PrintDialog`. Uživatel může vybrat fyzickou tiskárnu nebo `Microsoft Print to PDF`. Neexistuje náhled, tiskový DTO model, tabulka, opakované záhlaví ani řízené stránkování.

Faktura vede cestou `BudgetItem` → `InvoiceSourceItem` → `InvoiceDraft`/`InvoiceLine` → `PdfInvoiceExportService`. PDF je nyní skládáno ručně jako jednostránkový PDF 1.4 text s maximálně 48 řádky. Nepoužívá XAML šablonu, neumí vložit logo, QR platbu, podpis, tabulkový layout ani více stran a převádí text na ASCII. Jde o technický export, nikoli finální profesionální fakturační šablonu.

Data faktury se vkládají mapováním v `WindowService` a `InvoiceViewModel`: typ, popis, jednotka, množství, cena před slevou, sleva a konečná cena se převedou do řádků `InvoiceDraft`. Dodavatel, odběratel, číslo, splatnost, DPH a poznámky doplňuje uživatel. Windows tisk kalkulace naproti tomu dostává pouze hotový text, takže layout už nemůže bezpečně měnit.

## Bezpečný cílový návrh

1. `DocumentData`/`InvoicePrintData` — neměnná DTO data bez WPF.
2. `IDocumentDataFactory` — mapování projektu, kalkulace nebo faktury do DTO.
3. `PrintTemplateSettings` — JSON konfigurace loga, firmy, barev, patičky, viditelnosti bloků a varianty layoutu.
4. `IPrintDocumentBuilder` — vytvoření stránkovaného `FixedDocument` nebo řízeného `FlowDocument`.
5. `IWindowsPrintService` — náhled a systémový `PrintDialog` nad stejným dokumentem.
6. `IPdfExportService` — PDF ze stejného dokumentového modelu, aby tisk a PDF neměly rozdílné součty.

Uživatelský libovolný XAML se nesmí načítat. Bezpečné jsou předdefinované layouty a validovaný JSON v `%LocalAppData%\ElektroOffer\Templates`, s náhledem a návratem na výchozí nastavení. Konfigurace může obsahovat logo, název, adresu, IČ/DIČ, banku, IBAN/SWIFT, accent, patičku, podpis/razítko a viditelnost bloků.

## Rozdělení verzí

- 1.11.1: pouze audit, sdílené dokumentové tokeny a opravy stability.
- 1.12.x: DTO, validovaná konfigurace a jednotný builder s náhledem.
- 1.x: profesionální více­stránková nabídka, rozpočet a faktura včetně českých znaků.
- 2.0: stejné šablony pro tok měření → kalkulace → nabídka → zakázka → faktura.
