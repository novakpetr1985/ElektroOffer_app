[CmdletBinding()]
param(
    [string] $Subject = "CN=ElektroOffer Test",
    [int] $ValidYears = 3,
    [string] $ExportDirectory,
    [switch] $TrustCurrentUser
)

$ErrorActionPreference = "Stop"
$root = Resolve-Path "$PSScriptRoot\..\.."
$exportDirectory = if ($ExportDirectory) {
    [System.IO.Path]::GetFullPath((Join-Path $root $ExportDirectory))
} else {
    Join-Path $root "artifacts\test-signing"
}

$existing = Get-ChildItem Cert:\CurrentUser\My |
    Where-Object {
        $_.Subject -eq $Subject -and
        $_.HasPrivateKey -and
        $_.NotAfter -gt (Get-Date).AddDays(30) -and
        $_.EnhancedKeyUsageList.ObjectId -contains "1.3.6.1.5.5.7.3.3"
    } |
    Sort-Object NotAfter -Descending |
    Select-Object -First 1

if ($existing) {
    $certificate = $existing
    Write-Host "Pouzivam existujici testovaci certifikat $($certificate.Thumbprint)."
} else {
    $certificate = New-SelfSignedCertificate `
        -Type CodeSigningCert `
        -Subject $Subject `
        -FriendlyName "ElektroOffer testovaci podpis" `
        -CertStoreLocation Cert:\CurrentUser\My `
        -KeyAlgorithm RSA `
        -KeyLength 3072 `
        -HashAlgorithm SHA256 `
        -KeyExportPolicy NonExportable `
        -NotAfter (Get-Date).AddYears($ValidYears)

    Write-Host "Vytvoren testovaci certifikat $($certificate.Thumbprint)."
}

New-Item -ItemType Directory -Force -Path $exportDirectory | Out-Null
$cerPath = Join-Path $exportDirectory "ElektroOffer-Test-CodeSigning.cer"
Export-Certificate -Cert $certificate -FilePath $cerPath -Force | Out-Null

$metadata = [ordered]@{
    purpose = "TEST ONLY - not publicly trusted"
    subject = $certificate.Subject
    thumbprint = $certificate.Thumbprint
    notBefore = $certificate.NotBefore.ToUniversalTime().ToString("O")
    notAfter = $certificate.NotAfter.ToUniversalTime().ToString("O")
    publicCertificate = [System.IO.Path]::GetFileName($cerPath)
}
$metadata | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $exportDirectory "certificate.json") -Encoding UTF8

Copy-Item -LiteralPath (Join-Path $PSScriptRoot "Install-TestCertificate.ps1") -Destination $exportDirectory -Force
Copy-Item -LiteralPath (Join-Path $PSScriptRoot "Remove-TestCertificate.ps1") -Destination $exportDirectory -Force

if ($TrustCurrentUser) {
    & (Join-Path $PSScriptRoot "Install-TestCertificate.ps1") -CertificatePath $cerPath -ExpectedThumbprint $certificate.Thumbprint -Confirm:$false
}

Write-Host "Verejny certifikat: $cerPath"
Write-Host "Otisk: $($certificate.Thumbprint)"
Write-Output $certificate
