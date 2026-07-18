# Design tokeny ElektroOffer

Jediným zdrojem vizuálních hodnot je `ElektroOffer_app.Invoice/Resources/DesignTokens.xaml`. Hlavní aplikace načítá tentýž WPF `ResourceDictionary` z fakturačního sestavení, takže se hodnoty nekopírují.

## Konvence klíčů

- `Color.*` — světlá a tmavá paleta; kompatibilní `App*Brush` zdroje jsou pouze sémantické aliasy.
- `Typography.*` — rodiny, velikosti a váhy písma pro UI i dokumenty.
- `Space.*`, `Spacing.*` a `Size.*` — základní 4px mřížka, složená odsazení a rozměry prvků.
- `Radius.*` a `Border.*` — zaoblení a šířky ohraničení.
- `Shadow.*` — sdílené úrovně stínu.
- `Motion.*` — délky a easing animací.

Ve WPF se token používá přes `{DynamicResource Token.Key}`. Dynamický odkaz zajišťuje, že změna zdroje za běhu aktualizuje všechny spotřebitele. Tisková a PDF služba čtou stejné klíče přes `Application.Current.TryFindResource`; fallback slouží pouze pro spuštění služby bez inicializované WPF aplikace (například v izolovaném testu).

## Příklad změny značky

Změna `Color.Accent` upraví světlý akcent, výběr a všechny prvky používající `AppAccentBrush`. Tmavý motiv má samostatný token `Color.Dark.Accent`. Změna `Typography.FontSize.Document` a `Spacing.DocumentPagePadding` se současně projeví v tisku i generovaném PDF.
