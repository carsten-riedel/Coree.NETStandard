param (
    [Parameter(Mandatory=$true)]
    [string]$server
)


$gitroot = git rev-parse --show-toplevel 2>&1
Set-Location -Path $gitroot

# Custom function to copy files, excluding specified directories
function Copy-Directory {
    param (
        [string]$sourceDir,
        [string]$destinationDir,
        [string[]]$exclusions
    )

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

            if ($item.PSIsContainer) {
                # Create directory if it doesn't exist
                if (-not (Test-Path -Path $targetPath)) {
                    New-Item -ItemType Directory -Path $targetPath
                }
            } else {
                # Copy file
                Copy-Item -Path $item.FullName -Destination $targetPath -Force
                Write-Output "Copyied: $($item.FullName) to $($targetPath)"
            }
        }
    }
}

$source = "src/Projects/Coree.NETStandard/Docfx/result/local/"
$destination = "docs/docfx"
$exclusions = @('.git', '.github')

# Copy items from source to destination, excluding specified directories
Copy-Directory -sourceDir $source -destinationDir $destination -exclusions $exclusions

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


Write-Output "Docfx install"
dotnet tool install --global docfx --version 2.74.1

Write-Output "Dotnet restore"
dotnet restore ./src
Write-Output "Dotnet build"
dotnet build ./src --no-restore /p:ContinuousIntegrationBuild=true -c Release
Write-Output "Dotnet pack"
dotnet pack ./src --no-restore /p:ContinuousIntegrationBuild=true -c Release

docfx src/Projects/Coree.NETStandard/Docfx/build/docfx_local.json


