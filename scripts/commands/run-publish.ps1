# ==========================================
# ElektroOffer - Publish Release Build
# ==========================================

param(
    [string] $OutputDirectory,
    [switch] $SkipInstaller,
    [switch] $TestSign,
    [string] $CertificateThumbprint
)

$ErrorActionPreference = "Stop"
$Root = Resolve-Path "$PSScriptRoot\..\.."

$Project = Join-Path $Root "ElektroOffer_app\ElektroOffer_app.csproj"
$FieldProject = Join-Path $Root "ElektroOffer.Field\ElektroOffer.Field.csproj"
$PublishOutput = if ($OutputDirectory) {
    [System.IO.Path]::GetFullPath((Join-Path $Root $OutputDirectory))
} else {
    Join-Path $Root "artifacts\publish\win-x64"
}
$FieldPublishOutput = Join-Path $Root "artifacts\publish\field-win-x64"
$InstallerOutput = Join-Path $Root "artifacts\installer"
$InstallerScript = Join-Path $Root "scripts\installer\ElektroOffer.iss"
$TestSignFileScript = Join-Path $Root "scripts\signing\Sign-TestFile.ps1"
$TestSignPublishScript = Join-Path $Root "scripts\signing\Sign-TestPublish.ps1"

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

Write-Host ""
Write-Host "Publikuji ElektroOffer Teren:" -ForegroundColor Cyan
Write-Host "  $FieldPublishOutput"

dotnet publish $FieldProject `
    --configuration Release `
    --framework net10.0-windows10.0.19041.0 `
    --runtime win-x64 `
    --self-contained true `
    -p:UseMonoRuntime=false `
    -p:WindowsPackageType=None `
    -p:WindowsAppSDKSelfContained=true `
    -p:DebugType=None `
    -p:DebugSymbols=false `
    --output $FieldPublishOutput

if ($LASTEXITCODE -ne 0) {
    throw "Publish ElektroOffer Teren selhal s kodem $LASTEXITCODE."
}

if ($TestSign) {
    Write-Host ""
    Write-Host "Podepisuji testovacim Windows certifikatem:" -ForegroundColor Cyan
    & $TestSignPublishScript -PublishDirectories @($PublishOutput, $FieldPublishOutput) -CertificateThumbprint $CertificateThumbprint
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
    $isccArguments = @(
        "/DSourceDir=$PublishOutput",
        "/DFieldSourceDir=$FieldPublishOutput",
        "/DOutputDir=$InstallerOutput",
        "/DAppVersion=$ReleaseVersion"
    )
    if ($TestSign) {
        $thumbprintArgument = if ($CertificateThumbprint) { " -CertificateThumbprint $CertificateThumbprint" } else { "" }
        $signCommand = "powershell.exe -NoProfile -ExecutionPolicy Bypass -File `"$TestSignFileScript`" -FilePath `$f$thumbprintArgument"
        $isccArguments += "/DTestSigning=1"
        $isccArguments += "/STestSign=$signCommand"
    }
    & $Iscc @isccArguments $InstallerScript
    if ($LASTEXITCODE -ne 0) {
        throw "Vytvoreni instalatoru selhalo s kodem $LASTEXITCODE."
    }
}

Write-Host ""
Write-Host "==========================================" -ForegroundColor Green
Write-Host " Release balicek byl uspesne vytvoren." -ForegroundColor Green
Write-Host "==========================================" -ForegroundColor Green
Write-Host "Aplikace:  $PublishOutput"
Write-Host "Teren:     $FieldPublishOutput"
if (-not $SkipInstaller) {
    Write-Host "Instalator: $InstallerOutput\ElektroOffer-Setup-$ReleaseVersion-x64.exe"
}
if ($TestSign) {
    Write-Host "Podpis:    TESTOVACI (neni verejne duveryhodny)"
}
