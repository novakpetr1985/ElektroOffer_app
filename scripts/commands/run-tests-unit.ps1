# scripts\commands\run-tests.ps1
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# cesta ke složce, kde je tento skript
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path

# přejít do kořene repozitáře (dva levely nahoru: scripts\commands -> root)
$repoRoot = Resolve-Path -LiteralPath (Join-Path -Path $scriptDir -ChildPath '..\..')
Set-Location -LiteralPath $repoRoot

# projekty (relativně k repo root)
$projects = @(
  "ElektroOffer_app.Tests.Unit/ElektroOffer_app.Tests.Unit.csproj"
)

foreach ($p in $projects) {
  Write-Host "=== Running tests: $p ==="
  if (-not (Test-Path $p)) {
    Write-Host "ERROR: Project not found: $p" -ForegroundColor Red
    exit 1
  }
  dotnet test $p --configuration Release --verbosity normal
  if ($LASTEXITCODE -ne 0) {
    Write-Host "Tests failed for $p (exit $LASTEXITCODE)" -ForegroundColor Red
    exit $LASTEXITCODE
  } else {
    Write-Host "OK: $p" -ForegroundColor Green
  }
}

Write-Host "All tests finished successfully." -ForegroundColor Cyan
exit 0
