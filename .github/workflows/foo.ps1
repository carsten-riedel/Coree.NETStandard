param (
    [Parameter(Mandatory=$true)]
    [string]$server
)

$gitroot = git rev-parse --show-toplevel 2>&1
Set-Location -Path $gitroot

$secretsPath = ".github/workflows/secrets.ps1"

# Check if the secrets file exists before importing
if (Test-Path $secretsPath) {
    . $secretsPath
    Write-Output "Imported secrets from: $secretsPath"
} else {
    Write-Output "Secrets file not found at: $secretsPath"
}

# Fetch the current git branch name
$gitBranch = git rev-parse --abbrev-ref HEAD

# Output the server parameter, the git branch name, and if available, the $FOO variable from secrets.ps1
$fooOutput = if ($null -ne $FOO) { $FOO } else { "FOO not set" }
Write-Output "Server: $server, Git Branch: $gitBranch, FOO: $fooOutput, Git root: $gitroot"

dotnet tool install --global docfx --version 2.74.1

dotnet restore ./src
dotnet build ./src --no-restore /p:ContinuousIntegrationBuild=true -c Release
dotnet pack ./src --no-restore /p:ContinuousIntegrationBuild=true -c Release

docfx src/Projects/Coree.NETStandard/Docfx/build/docfx_local.json

