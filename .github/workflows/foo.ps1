param (
    [Parameter(Mandatory=$true)]
    [string]$server
)

. "$PSScriptRoot/secrets.ps1"

# Fetch the current git branch name
$gitBranch = git rev-parse --abbrev-ref HEAD

$gittld = git rev-parse --show-toplevel 2>&1

# Output the server parameter and the git branch name
Write-Output "Server: $server, Git Branch: $gitBranch , $FOO $gittld"