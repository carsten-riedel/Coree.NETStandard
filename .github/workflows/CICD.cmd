@echo off
for /f "delims=" %%i in ('git rev-parse --show-toplevel') do set "topLevelDirectory=%%i"
cd %topLevelDirectory%
echo Change to git toplevel directory: %topLevelDirectory%
echo install powershell as dotnet tool
dotnet tool install --global PowerShell --version 7.4.1
echo starting the
pwsh .github/workflows/workflow.ps1 -server "sdfgfdg"
pause