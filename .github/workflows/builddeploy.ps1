$ErrorActionPreference = 'Stop'

. "builddeploy_helper.ps1"

Get-ChildItem Env:* | Select-Object -Property Name,Value


$branch = if ($env:GITHUB_HEAD_REF) { $env:GITHUB_HEAD_REF } else { $env:GITHUB_REF -replace 'refs/heads/', '' }

$gitBranch = git rev-parse --abbrev-ref HEAD

Write-Output "branch is $branch"
Write-Output "gitBranch is $gitBranch"