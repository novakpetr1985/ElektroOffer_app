using System.Windows;

// -----------------------------------------------------------------------------
// AssemblyInfo.cs
// -----------------------------------------------------------------------------
// Tento soubor obsahuje atribut ThemeInfo, který říká WPF, kde hledat zdroje
// (ResourceDictionary) pro styly a šablony ovládacích prvků.
//
// - ResourceDictionaryLocation.None
//   → Specifické slovníky pro jednotlivá témata (např. Classic, Luna, Aero)
//     se nepoužívají.
//
// - ResourceDictionaryLocation.SourceAssembly
//   → Hlavní (generic) ResourceDictionary je součástí tohoto assembly.
//     Typicky se používá pro styly ovládacích prvků.
// -----------------------------------------------------------------------------

[assembly: ThemeInfo(
    ResourceDictionaryLocation.None,            // kde hledat theme-specific resource dictionaries
                                                // (pokud se zdroj nenajde na stránce
                                                // nebo v aplikačních zdrojích)

    ResourceDictionaryLocation.SourceAssembly   // kde hledat generic resource dictionary
                                                // (pokud se zdroj nenajde na stránce,
                                                // v aplikaci nebo v theme-specific slovnících)
)]
