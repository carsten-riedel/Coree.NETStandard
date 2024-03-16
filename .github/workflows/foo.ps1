param (
    [Parameter(Mandatory=$true)]
    [string]$server
)

$gitroot = git rev-parse --show-toplevel 2>&1

$secretsPath = "$gitroot/.github/workflows/secrets.ps1"

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
