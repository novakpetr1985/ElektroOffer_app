[CmdletBinding()]
param(
    [string] $AndroidSdkDirectory = (Join-Path $env:LOCALAPPDATA "Android\Sdk"),
    [string] $JavaSdkDirectory = (Join-Path $env:LOCALAPPDATA "Android\Jdk"),
    [switch] $InstallDependencies,
    [switch] $NoBuild
)

$ErrorActionPreference = "Stop"
$root = Resolve-Path "$PSScriptRoot\..\.."
$project = Join-Path $root "ElektroOffer.Field\ElektroOffer.Field.csproj"
$outputDirectory = Join-Path $root "artifacts\android-test"

if ($InstallDependencies) {
    dotnet build $project `
        -t:InstallAndroidDependencies `
        --framework net10.0-android `
        "-p:AndroidSdkDirectory=$AndroidSdkDirectory" `
        "-p:JavaSdkDirectory=$JavaSdkDirectory" `
        -p:AcceptAndroidSdkLicenses=True
    if ($LASTEXITCODE -ne 0) {
        throw "Instalace Android zavislosti selhala s kodem $LASTEXITCODE."
    }
}

if (-not (Test-Path -LiteralPath $AndroidSdkDirectory -PathType Container)) {
    throw "Android SDK nebylo nalezeno. Spustte skript s -InstallDependencies."
}
if (-not (Test-Path -LiteralPath (Join-Path $JavaSdkDirectory "bin\java.exe") -PathType Leaf)) {
    throw "Java SDK nebylo nalezeno. Spustte skript s -InstallDependencies."
}

if (-not $NoBuild) {
    dotnet build $project `
        --framework net10.0-android `
        --configuration Debug `
        -p:EmbedAssembliesIntoApk=true `
        "-p:AndroidSdkDirectory=$AndroidSdkDirectory" `
        "-p:JavaSdkDirectory=$JavaSdkDirectory"
    if ($LASTEXITCODE -ne 0) {
        throw "Android testovaci build selhal s kodem $LASTEXITCODE."
    }
}

$apk = Get-ChildItem -LiteralPath (Join-Path $root "ElektroOffer.Field\bin\Debug\net10.0-android") -Filter "*-Signed.apk" |
    Sort-Object LastWriteTime -Descending |
    Select-Object -First 1
if (-not $apk) {
    throw "Podepsane APK nebylo po buildu nalezeno."
}

$apkSigner = Get-ChildItem -LiteralPath (Join-Path $AndroidSdkDirectory "build-tools") -Recurse -Filter "apksigner.bat" |
    Sort-Object FullName -Descending |
    Select-Object -First 1
if (-not $apkSigner) {
    throw "apksigner nebyl v Android SDK nalezen."
}

$env:JAVA_HOME = $JavaSdkDirectory
& $apkSigner.FullName verify --verbose --print-certs $apk.FullName
if ($LASTEXITCODE -ne 0) {
    throw "Overeni Android podpisu selhalo s kodem $LASTEXITCODE."
}

[xml] $projectXml = Get-Content -LiteralPath $project
$displayVersion = $projectXml.Project.PropertyGroup.ApplicationDisplayVersion | Where-Object { $_ } | Select-Object -First 1
New-Item -ItemType Directory -Force -Path $outputDirectory | Out-Null
$destination = Join-Path $outputDirectory "ElektroOffer.Field-$displayVersion-debug.apk"
Copy-Item -LiteralPath $apk.FullName -Destination $destination -Force
$hash = Get-FileHash -Algorithm SHA256 -LiteralPath $destination

Write-Host ""
Write-Host "Android testovaci APK bylo vytvoreno a podpis overen." -ForegroundColor Green
Write-Host "APK:     $destination"
Write-Host "SHA-256: $($hash.Hash)"
