@echo off
REM nastaví pracovní adresář na umístění .bat (scripts)
cd /d "%~dp0"

REM spustí PowerShell skript v podsložce "příkazy" a nechá okno otevřené
powershell -NoProfile -ExecutionPolicy Bypass -Command "& '%~dp0commands\run-tests-integration.ps1'"
pause
