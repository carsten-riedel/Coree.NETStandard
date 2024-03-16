@echo off
for /f "delims=" %%i in ('git rev-parse --show-toplevel') do set "topLevelDirectory=%%i"
echo Top-level directory: %topLevelDirectory%
dotnet tool install --global PowerShell --version 7.4.1
pwsh %topLevelDirectory%/.github/workflows/foo.ps1 -server "sdfgfdg"
pause