# Testovací podepisování ElektroOffer

Tento postup je určen výhradně pro vlastní testovací počítače a telefon. Testovací certifikát není veřejně důvěryhodný a nesmí být použit jako produkční identita.

## Windows – vytvoření a build

V PowerShellu v kořeni repozitáře:

```powershell
.\scripts\signing\New-TestCodeSigningCertificate.ps1 -TrustCurrentUser
.\scripts\commands\run-publish.ps1 -TestSign
```

Soukromý klíč zůstane jako neexportovatelný klíč v `Cert:\CurrentUser\My`. Do `artifacts\test-signing` se exportuje pouze veřejný certifikát, jeho metadata a instalační/odinstalační skripty.

## Druhý testovací počítač

1. Bezpečně přeneste celý adresář `artifacts\test-signing` a podepsaný instalátor.
2. Porovnejte otisk v `certificate.json` s otiskem zobrazeným na sestavovacím PC.
3. Spusťte PowerShell bez administrátorských práv v přeneseném adresáři.
4. Nainstalujte důvěru pouze pro aktuálního uživatele:

```powershell
.\Install-TestCertificate.ps1 -ExpectedThumbprint "OTISK_Z_CERTIFICATE_JSON"
```

5. U instalátoru otevřete Vlastnosti → Digitální podpisy a ověřte vydavatele `ElektroOffer Test`.
6. Spusťte instalátor.

Po ukončení testování lze důvěru odebrat:

```powershell
.\Remove-TestCertificate.ps1
```

## Android telefon

Debug APK používá standardní Android debug podpis vytvořený vývojovým prostředím. V telefonu povolte instalaci neznámých aplikací pouze pro aplikaci, přes kterou APK otevřete. Po testu lze toto oprávnění opět vypnout.

Build:

```powershell
.\scripts\commands\run-android-test-build.ps1
```

Při úplně první konfiguraci počítače lze oficiální Android SDK a Microsoft JDK doplnit příkazem:

```powershell
.\scripts\commands\run-android-test-build.ps1 -InstallDependencies
```

Ověřené APK se zkopíruje do `artifacts\android-test`. Stejné instalované testovací aplikaci lze posílat aktualizace pouze APK podepsaným stejným Android klíčem.

Debug konfigurace má zapnuté `EmbedAssembliesIntoApk`, protože balíček je určen pro samostatnou instalaci přes USB, OneDrive nebo správce souborů. Výchozí Fast Deployment APK z Visual Studia není samostatně přenosné.

## Bezpečnost

- Nikdy necommitujte `.pfx`, `.p12`, `.jks` ani `.keystore`.
- Veřejný `.cer` neobsahuje soukromý klíč.
- Testovacímu certifikátu důvěřujte pouze na vlastních testovacích zařízeních.
- Podpis nenahrazuje antivirovou kontrolu a nevytváří firewallovou výjimku.
