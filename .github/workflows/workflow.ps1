$ErrorActionPreference = 'Stop'

# Custom function to copy files, excluding specified directories
function Copy-Directory {
    param (
        [string]$sourceDir,
        [string]$destinationDir,
        [string[]]$exclusions
    )

    $sourceDirParam = $sourceDir
    $destinationDirParam = $destinationDir

    # Ensure that $sourceDir and $destinationDir are absolute paths
    if (-not [System.IO.Path]::IsPathRooted($sourceDir)) {
        $sourceDir = Join-Path (Get-Location) $sourceDir
    }

    if (-not [System.IO.Path]::IsPathRooted($destinationDir)) {
        $destinationDir = Join-Path (Get-Location) $destinationDir
    }

    # Ensure paths end with a directory separator for consistent behavior
    $sourceDir = [System.IO.Path]::GetFullPath($sourceDir)
    $destinationDir = [System.IO.Path]::GetFullPath($destinationDir)

    # Get all items in the source directory
    $items = Get-ChildItem -Path $sourceDir -Recurse

    foreach ($item in $items) {
        # Check if the item is in an excluded directory
        $excluded = $false
        foreach ($exclusion in $exclusions) {
            if ($item.FullName -like "*\$exclusion*") {
                $excluded = $true
                break
            }
        }

        if (-not $excluded) {
            $relativePath = [System.IO.Path]::GetRelativePath($sourceDir, $item.FullName)
            $targetPath = Join-Path -Path $destinationDir -ChildPath $relativePath


            $relativeSource = Join-Path -Path $sourceDirParam -ChildPath $relativePath
            $relativeDestination = Join-Path -Path $destinationDirParam -ChildPath $relativePath

            if ($item.PSIsContainer) {
                # Create directory if it doesn't exist
                if (-not (Test-Path -Path $targetPath)) {
                    New-Item -ItemType Directory -Path $targetPath
                }
            } else {
                # Copy file
                Copy-Item -Path $item.FullName -Destination $targetPath -Force
                Write-Output "Copyied: $($relativeSource) --> $($relativeDestination)"
            }
        }
    }
}

function Log-Block {
    param (
        [string]$Title,
        [string]$Content = $null
    )
    Write-Output "=================================================================================="
    if (-not [string]::IsNullOrEmpty($Title)) {
        Write-Output "$Title"
        Write-Output "----------------------------------------------------------------------------------"
    }
    if (-not [string]::IsNullOrEmpty($Content)) {
        Write-Output $Content
    }
    Write-Output "=================================================================================="
    Write-Output ""
}

Log-Block -Title "Stage: Prepare environment" -Content "Setting currentdir to git root directory"
$gitroot = git rev-parse --show-toplevel 2>&1
Set-Location -Path $gitroot
$gitBranch = git rev-parse --abbrev-ref HEAD

Log-Block -Title "Stage: Prepare secrets environment"
$SECRETS_PAT = $args[0]
$SECRETS_NUGET_PAT = $args[1]
$SECRETS_NUGET_TEST_PAT = $args[2]

$secretsPath = ".github/workflows/secrets.ps1"

# Check if the secrets file exists before importing
if (Test-Path $secretsPath) {
    . $secretsPath
    Write-Output "Imported secrets from: $secretsPath"
} else {
    Write-Output "Secrets file not found at: $secretsPath"
}

throw "This is an error message."

# Output the server parameter, the git branch name, and if available, the $FOO variable from secrets.ps1
$fooOutput = if ($null -ne $FOO) { $FOO } else { "FOO not set" }
Write-Output "Server: $server, Git Branch: $gitBranch, FOO: $fooOutput, Git root: $gitroot"


Log-Block -Title "Stage: Tool" -Content "Install docfx"
dotnet tool install --global docfx --version 2.74.1

Log-Block -Title "Stage: Dotnet" -Content "Building the application..."
Log-Block -Title "Restore"
dotnet restore ./src
Log-Block -Title "Build"
dotnet build ./src --no-restore /p:ContinuousIntegrationBuild=true -c Release
Log-Block -Title "Pack"
dotnet pack ./src --no-restore /p:ContinuousIntegrationBuild=true -c Release
Log-Block -Title "Docs"
docfx src/Projects/Coree.NETStandard/Docfx/build/docfx_local.json


Log-Block -Title "Copy docs"
Copy-Directory -sourceDir "src/Projects/Coree.NETStandard/Docfx/result/local/" -destinationDir "docs/docfx" -exclusions @('.git', '.github')

Log-Block -Title "Commit and push"

git config --global user.name 'Updated form Workflow'
git config --global user.email 'carstenriedel@outlook.com'
git add docs/docfx
git commit -m "Updated form Workflow"
git push origin master

<#
   # Push packagaes
    - name: Add github nuget source
      run: dotnet nuget add source --username carsten-riedel --password ${{ secrets.PAT }} --store-password-in-clear-text --name github "https://nuget.pkg.github.com/carsten-riedel/index.json"
      
    - name: Push nupkg to github repository
      run: dotnet nuget push "src/Projects/Coree.NETStandard/bin/Pack/Coree.NETStandard.*.nupkg" --api-key ${{ secrets.PAT }} --source "github"
      
    - name: Push nupkg to nupkg repository
      run: dotnet nuget push "src/Projects/Coree.NETStandard/bin/Pack/Coree.NETStandard.*.nupkg" --api-key ${{ secrets.NUGET_PAT }} --source https://api.nuget.org/v3/index.json

    
    # Dispatch other workflows
    - name: Dispatch a other workflow (Deploy static content to Pages)
      run: |
        curl -X POST -H "Authorization: token ${{ secrets.PAT }}" -H "Accept: application/vnd.github.v3+json" \
        https://api.github.com/repos/carsten-riedel/Coree.NETStandard/dispatches \
        -d '{"event_type": "trigger-other-workflow"}'
#>