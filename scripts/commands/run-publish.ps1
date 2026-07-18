# ==========================================
# ElektroOffer - Publish Release Build
# ==========================================

param(
    [string] $OutputDirectory,
    [switch] $SkipInstaller
)

$ErrorActionPreference = "Stop"
$Root = Resolve-Path "$PSScriptRoot\..\.."

$Project = Join-Path $Root "ElektroOffer_app\ElektroOffer_app.csproj"
$PublishOutput = if ($OutputDirectory) {
    [System.IO.Path]::GetFullPath((Join-Path $Root $OutputDirectory))
} else {
    Join-Path $Root "artifacts\publish\win-x64"
}
$InstallerOutput = Join-Path $Root "artifacts\installer"
$InstallerScript = Join-Path $Root "scripts\installer\ElektroOffer.iss"

[xml] $ProjectXml = Get-Content $Project
$Version = $ProjectXml.Project.PropertyGroup.Version | Where-Object { $_ } | Select-Object -First 1
$ReleaseVersion = $Version -replace '-.*$', ''

Write-Host ""
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host " ElektroOffer - Publish Release Build"
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "Projekt:"
Write-Host "  $Project"

Write-Host ""
Write-Host "Výstup:"
Write-Host "  $PublishOutput"
Write-Host ""

dotnet publish $Project `
    --configuration Release `
    --runtime win-x64 `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:DebugType=None `
    -p:DebugSymbols=false `
    --output $PublishOutput

if ($LASTEXITCODE -ne 0) {
    throw "Publish selhal s kodem $LASTEXITCODE."
}

if (-not $SkipInstaller) {
    $IsccCandidates = @(
        (Get-Command ISCC.exe -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Source -First 1),
        "C:\Program Files (x86)\Inno Setup 6\ISCC.exe",
        "C:\Program Files\Inno Setup 6\ISCC.exe"
    ) | Where-Object { $_ -and (Test-Path $_) }
    $Iscc = $IsccCandidates | Select-Object -First 1

    if (-not $Iscc) {
        throw "Inno Setup 6 nebyl nalezen. Nainstalujte jej, nebo pouzijte -SkipInstaller."
    }

    New-Item -ItemType Directory -Force -Path $InstallerOutput | Out-Null
    & $Iscc "/DSourceDir=$PublishOutput" "/DOutputDir=$InstallerOutput" "/DAppVersion=$ReleaseVersion" $InstallerScript
    if ($LASTEXITCODE -ne 0) {
        throw "Vytvoreni instalatoru selhalo s kodem $LASTEXITCODE."
    }
}

Write-Host ""
Write-Host "==========================================" -ForegroundColor Green
Write-Host " Release balicek byl uspesne vytvoren." -ForegroundColor Green
Write-Host "==========================================" -ForegroundColor Green
Write-Host "Aplikace:  $PublishOutput"
if (-not $SkipInstaller) {
    Write-Host "Instalator: $InstallerOutput\ElektroOffer-Setup-$ReleaseVersion-x64.exe"
}
