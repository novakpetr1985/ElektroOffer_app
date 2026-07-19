[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string[]] $PublishDirectories,
    [string] $CertificateThumbprint
)

$ErrorActionPreference = "Stop"
$signScript = Join-Path $PSScriptRoot "Sign-TestFile.ps1"
$ownedFiles = @(
    "ElektroOffer_app.exe",
    "ElektroOffer_app.dll",
    "ElektroOffer_app.Invoice.dll",
    "ElektroOffer.Contracts.dll",
    "ElektroOffer.Field.exe",
    "ElektroOffer.Field.dll"
)

$signed = 0
foreach ($directory in $PublishDirectories) {
    $fullDirectory = [System.IO.Path]::GetFullPath($directory)
    foreach ($name in $ownedFiles) {
        $path = Join-Path $fullDirectory $name
        if (Test-Path -LiteralPath $path -PathType Leaf) {
            & $signScript -FilePath $path -CertificateThumbprint $CertificateThumbprint
            $signed++
        }
    }
}
if ($signed -eq 0) {
    throw "V publish adresarich nebyl nalezen zadny vlastni soubor k podpisu."
}
Write-Host "Podepsano vlastnich souboru: $signed" -ForegroundColor Green
