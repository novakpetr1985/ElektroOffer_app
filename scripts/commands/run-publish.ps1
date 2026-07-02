# ==========================================
# ElektroOffer - Publish Release Build
# ==========================================

$Root = Resolve-Path "$PSScriptRoot\..\.."

$Project = Join-Path $Root "ElektroOffer_app\ElektroOffer_app.csproj"
$Output  = Join-Path $Root "public"

Write-Host ""
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host " ElektroOffer - Publish Release Build"
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "Projekt:"
Write-Host "  $Project"

Write-Host ""
Write-Host "Výstup:"
Write-Host "  $Output"
Write-Host ""

dotnet publish $Project `
    --configuration Release `
    --output $Output

if ($LASTEXITCODE -eq 0)
{
    Write-Host ""
    Write-Host "==========================================" -ForegroundColor Green
    Write-Host " Publish finished successfully." -ForegroundColor Green
    Write-Host "==========================================" -ForegroundColor Green
}
else
{
    Write-Host ""
    Write-Host "==========================================" -ForegroundColor Red
    Write-Host " Publish selhal!" -ForegroundColor Red
    Write-Host "==========================================" -ForegroundColor Red
    exit $LASTEXITCODE
}