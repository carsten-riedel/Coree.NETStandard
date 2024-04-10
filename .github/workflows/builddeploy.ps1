$ErrorActionPreference = 'Stop'

Get-ChildItem Env:* | Select-Object -Property Name,Value


$branch = if ($env:GITHUB_HEAD_REF) { $env:GITHUB_HEAD_REF } else { $env:GITHUB_REF -replace 'refs/heads/', '' }

Write-Output "Branchname is $branch"