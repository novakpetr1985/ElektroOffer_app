@echo off
setlocal

REM Spustí PowerShell skript
powershell.exe -ExecutionPolicy Bypass -NoProfile -File "%~dp0commands\run-publish.ps1" %*

pause
