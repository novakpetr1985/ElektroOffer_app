[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string] $FilePath,
    [string] $CertificateThumbprint
)

$ErrorActionPreference = "Stop"
$filePath = [System.IO.Path]::GetFullPath($FilePath)
if (-not (Test-Path -LiteralPath $filePath -PathType Leaf)) {
    throw "Soubor k podpisu nebyl nalezen: $filePath"
}

$candidates = Get-ChildItem Cert:\CurrentUser\My |
    Where-Object {
        $_.HasPrivateKey -and
        $_.Subject -eq "CN=ElektroOffer Test" -and
        $_.NotAfter -gt (Get-Date) -and
        $_.EnhancedKeyUsageList.ObjectId -contains "1.3.6.1.5.5.7.3.3"
    }
if ($CertificateThumbprint) {
    $normalizedThumbprint = ($CertificateThumbprint -replace "\s", "").ToUpperInvariant()
    $candidates = $candidates | Where-Object Thumbprint -eq $normalizedThumbprint
}
$certificate = $candidates | Sort-Object NotAfter -Descending | Select-Object -First 1
if (-not $certificate) {
    throw "Testovaci certifikat s privatnim klicem nebyl nalezen. Spustte New-TestCodeSigningCertificate.ps1."
}

$signature = Set-AuthenticodeSignature -LiteralPath $filePath -Certificate $certificate -HashAlgorithm SHA256 -IncludeChain All
if ($signature.Status -notin @("Valid", "UnknownError")) {
    throw "Podepsani selhalo: $($signature.Status) - $($signature.StatusMessage)"
}

$verification = Get-AuthenticodeSignature -LiteralPath $filePath
if (-not $verification.SignerCertificate -or $verification.SignerCertificate.Thumbprint -ne $certificate.Thumbprint) {
    throw "Podpis souboru nelze overit spravnym testovacim certifikatem: $filePath"
}
Write-Host "Podepsano: $filePath"
