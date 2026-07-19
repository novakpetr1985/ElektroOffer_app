[CmdletBinding(SupportsShouldProcess)]
param(
    [string] $Thumbprint,
    [string] $CertificatePath
)

$ErrorActionPreference = "Stop"
$scriptDirectory = Split-Path -Parent $MyInvocation.MyCommand.Path
if (-not $CertificatePath) {
    $CertificatePath = Join-Path $scriptDirectory "ElektroOffer-Test-CodeSigning.cer"
}
if (-not $Thumbprint) {
    if (-not (Test-Path -LiteralPath $CertificatePath -PathType Leaf)) {
        throw "Zadejte -Thumbprint nebo ponechte verejny certifikat vedle skriptu."
    }
    $Thumbprint = ([System.Security.Cryptography.X509Certificates.X509Certificate2]::new($CertificatePath)).Thumbprint
}
$Thumbprint = ($Thumbprint -replace "\s", "").ToUpperInvariant()

foreach ($store in @("Root", "TrustedPublisher")) {
    $path = "Cert:\CurrentUser\$store\$Thumbprint"
    if (Test-Path -LiteralPath $path) {
        if ($PSCmdlet.ShouldProcess($path, "Odebrat testovaci certifikat")) {
            Remove-Item -LiteralPath $path
        }
    }
}
Write-Host "Testovaci duvera pro $Thumbprint byla odebrana."
