[CmdletBinding(SupportsShouldProcess)]
param(
    [string] $CertificatePath,
    [string] $ExpectedThumbprint
)

$ErrorActionPreference = "Stop"
$scriptDirectory = Split-Path -Parent $MyInvocation.MyCommand.Path
if (-not $CertificatePath) {
    $CertificatePath = Join-Path $scriptDirectory "ElektroOffer-Test-CodeSigning.cer"
}
$certificatePath = [System.IO.Path]::GetFullPath($CertificatePath)
if (-not (Test-Path -LiteralPath $certificatePath -PathType Leaf)) {
    throw "Certifikat nebyl nalezen: $certificatePath"
}

$certificate = [System.Security.Cryptography.X509Certificates.X509Certificate2]::new($certificatePath)
$actualThumbprint = $certificate.Thumbprint.ToUpperInvariant()
if ($ExpectedThumbprint -and $actualThumbprint -ne ($ExpectedThumbprint -replace "\s", "").ToUpperInvariant()) {
    throw "Otisk certifikatu nesouhlasi. Ocekavano: $ExpectedThumbprint, nalezeno: $actualThumbprint"
}
if ($certificate.Subject -ne "CN=ElektroOffer Test") {
    throw "Neocekavany vlastnik certifikatu: $($certificate.Subject)"
}
if ($certificate.NotAfter -le (Get-Date)) {
    throw "Certifikat je prosly."
}
$ekuExtension = $certificate.Extensions | Where-Object { $_ -is [System.Security.Cryptography.X509Certificates.X509EnhancedKeyUsageExtension] } | Select-Object -First 1
if (-not $ekuExtension -or $ekuExtension.EnhancedKeyUsages.Value -notcontains "1.3.6.1.5.5.7.3.3") {
    throw "Certifikat neni urcen pro podepisovani kodu."
}

Write-Host "TESTOVACI CERTIFIKAT - duverujte mu pouze na vlastnim testovacim PC." -ForegroundColor Yellow
Write-Host "Vlastnik: $($certificate.Subject)"
Write-Host "Otisk:    $actualThumbprint"
Write-Host "Platnost: $($certificate.NotAfter)"

if ($PSCmdlet.ShouldProcess("Cert:\CurrentUser\Root a Cert:\CurrentUser\TrustedPublisher", "Nainstalovat testovaci certifikat")) {
    foreach ($storeName in @("Root", "TrustedPublisher")) {
        $store = [System.Security.Cryptography.X509Certificates.X509Store]::new(
            $storeName,
            [System.Security.Cryptography.X509Certificates.StoreLocation]::CurrentUser)
        try {
            $store.Open([System.Security.Cryptography.X509Certificates.OpenFlags]::ReadWrite)
            if (-not ($store.Certificates | Where-Object Thumbprint -eq $actualThumbprint)) {
                $store.Add($certificate)
            }
        } finally {
            $store.Close()
        }
    }
    Write-Host "Certifikat byl nainstalovan pro aktualniho uzivatele." -ForegroundColor Green
}
