$ErrorActionPreference = 'Stop'

Get-ChildItem Env:* | Select-Object -Property Name,Value