$ErrorActionPreference = 'Stop'

. "$PSScriptRoot/builddeploy_helper.ps1"


$VersionMajor = "0"; $VersionMinor = "1"
$calculatedVersionBuild, $calculatedVersionRevision = Get-AssemblyVersionInfo
$FullVersion = "$VersionMajor.$VersionMinor.$calculatedVersionBuild.$calculatedVersionRevision"


Get-ChildItem Env:* | Select-Object -Property Name,Value

######################################################################################
Log-Block -Stage "Initialization" -Section "Base" -Task "Resolve branchname and action."

$isInGitRepository = IsGit
if (-not $isInGitRepository) {
    Write-Warning "This script is not running in a git environment."
    exit 1 # Non-zero exit code indicates an error condition
}

$initialBranchName = Get-GitBranchName
if (-not $initialBranchName) {
    Write-Warning "Failed to determine the git branch name."
    exit 1 # Non-zero exit code indicates an error condition
}

$isGithubActions = IsGithubActions
$branchName = Get-GitBranchName
$branchNameParts = @(Get-BranchNameParts -branchName $branchName)
$firstBranchSegment = $branchNameParts[0]
$branchSegment = $firstBranchSegment.ToLower()

$gitLocalRootPath = git rev-parse --show-toplevel
$gitLocalRootDir = Get-GitTopleveldir


$gitRemoteOriginUrl = git config --get remote.origin.url
$test = Get-RootDomainName -Url $gitRemoteOriginUrl

if ($isGithubActions -eq $true) { Write-Output "Is github actions." } else { "Is not github actions." }
Write-Output "Branch name is $branchName"
Write-Output "branchSegment is $branchSegment"
Write-Output "gitLocalRootPath is $gitLocalRootPath"
Write-Output "gitLocalRootDir is $gitLocalRootDir"
Write-Output "remote is $gitRemoteOriginUrl"
Write-Output "Calculated VersionBuild: $calculatedVersionBuild"
Write-Output "Calculated VersionRevision: $calculatedVersionRevision"
Write-Output "test : $test"

exit 0

# Define the array of strings
$isValidBranchRootName = @("feature", "develop", "release", "master" , "hotfix" )

if (-not($isValidBranchRootName.ToLower() -contains $firstBranchSegment.ToLower())) {
    Write-Warning "No configuration for branches $firstBranchSegment. Exiting"
    exit 1 # Non-zero exit code indicates an error condition
}

######################################################################################
Log-Block -Stage "Initialization" -Section "Base" -Task "Ensure-VariableSet"

$PAT = $args[0]
$NUGET_PAT = $args[1]
$NUGET_TEST_PAT = $args[2]

$secretsPath = "$PSScriptRoot/builddeploy_secrets.ps1"
# Check if the secrets file exists before importing
if (Test-Path $secretsPath) {
    . "$secretsPath"
}

Ensure-VariableSet -VariableName "`$PAT" -VariableValue "$PAT"
Ensure-VariableSet -VariableName "`$NUGET_PAT" -VariableValue "$NUGET_PAT"
Ensure-VariableSet -VariableName "`$NUGET_TEST_PAT" -VariableValue "$NUGET_TEST_PAT"

######################################################################################
Log-Block -Stage "Initialization" -Section "Base" -Task "Ensure-CommandAvailability"

Ensure-CommandAvailability -CommandName "dotnet"
Ensure-CommandAvailability -CommandName "pwsh"
Ensure-CommandAvailability -CommandName "git"

######################################################################################
Log-Block -Stage "Initialization" -Section "Base" -Task "Config local and remote"

if ($isGithubActions -eq $true) {
    git config --global user.name 'Workflow User'
    git config --global user.email 'carstenriedel@outlook.com'
} else {
    git config --global user.name 'Carsten Riedel'
    git config --global user.email 'carstenriedel@outlook.com'
}

######################################################################################
Log-Block -Stage "Initialization" -Section "Base" -Task "Config values for branches"

if ($branchSegment -ieq "feature") {

    $version = "--property:AssemblyVersion=$FullVersion --property:VersionPrefix=$FullVersion --property:VersionSuffix=$branchSegment"

    $dotnet_restore_param = "";
    $dotnet_build_param = "--no-restore --configuration Release --property:ContinuousIntegrationBuild=true --property:WarningLevel=3 $version";
    $dotnet_pack_param =  "--force --configuration Release --property:ContinuousIntegrationBuild=true --property:WarningLevel=3 $version";
    $docfx_param = "$gitLocalRootPath/src/Projects/Coree.NETStandard/Docfx/build/docfx_local.json"

} elseif ($branchSegment -ieq "develop") {

    $version = "--property:AssemblyVersion=$FullVersion --property:VersionPrefix=$FullVersion --property:VersionSuffix=$branchSegment"

    $dotnet_restore_param = "";
    $dotnet_build_param = "--no-restore --configuration Release --property:ContinuousIntegrationBuild=true --property:WarningLevel=3 $version";
    $dotnet_pack_param =  "--force --configuration Release --property:ContinuousIntegrationBuild=true --property:WarningLevel=3 $version";
    $docfx_param = "$gitLocalRootPath/src/Projects/Coree.NETStandard/Docfx/build/docfx_local.json"

} elseif ($branchSegment -ieq "release") {

    $version = "--property:AssemblyVersion=$FullVersion --property:VersionPrefix=$FullVersion"

    $dotnet_restore_param = "";
    $dotnet_build_param = "--no-restore --configuration Release --property:ContinuousIntegrationBuild=true --property:WarningLevel=3 $version";
    $dotnet_pack_param =  "--force --configuration Release --property:ContinuousIntegrationBuild=true --property:WarningLevel=3 $version";
    $docfx_param = "$gitLocalRootPath/src/Projects/Coree.NETStandard/Docfx/build/docfx_local.json"

} elseif ($branchSegment -ieq "master") {

    $version = "--property:AssemblyVersion=$FullVersion --property:VersionPrefix=$FullVersion"

    $dotnet_restore_param = "";
    $dotnet_build_param = "--no-restore --configuration Release --property:ContinuousIntegrationBuild=true --property:WarningLevel=3 $version";
    $dotnet_pack_param =  "--force --configuration Release --property:ContinuousIntegrationBuild=true --property:WarningLevel=3 $version";
    $docfx_param = "$gitLocalRootPath/src/Projects/Coree.NETStandard/Docfx/build/docfx_local.json"

} elseif ($branchSegment -ieq "hotfix") {

    $version = "--property:AssemblyVersion=$FullVersion --property:VersionPrefix=$FullVersion"

    $dotnet_restore_param = "";
    $dotnet_build_param = "--no-restore --configuration Release --property:ContinuousIntegrationBuild=true --property:WarningLevel=3 $version";
    $dotnet_pack_param =  "--force --configuration Release --property:ContinuousIntegrationBuild=true --property:WarningLevel=3 $version";
    $docfx_param = "$gitLocalRootPath/src/Projects/Coree.NETStandard/Docfx/build/docfx_local.json"

}

######################################################################################
Log-Block -Stage "Setup" -Section "Base" -Task "Install dotnet tools"

if (-not (Test-CommandAvailability -CommandName "docfx"))
{
    Execute-Command "dotnet tool install --global docfx --version 2.74.1"
}

######################################################################################
Log-Block -Stage "Setup" -Section "Base" -Task "Install powershell modules"

if (-not (Test-CommandAvailability -CommandName "New-PGPKey"))
{
    #Install-Module -Name PSPGP -AcceptLicense -AllowClobber -AllowPrerelease -Force
}

######################################################################################
Log-Block -Stage "Build" -Section "Clean" -Task "Clear projects bin obj"

Clear-BinObjDirectories -sourceDirectory "$gitLocalRootPath/src/Projects/Coree.NETStandard"

######################################################################################
Log-Block -Stage "Build" -Section "Restore" -Task "Restoreing nuget packages."

if ($null -ne $dotnet_restore_param)
{
    Execute-Command "dotnet restore $gitLocalRootPath/src $dotnet_restore_param"
}

######################################################################################
Log-Block -Stage "Build" -Section "Build" -Task "Building the solution."


if ($null -ne $dotnet_build_param)
{
    Execute-Command "dotnet build $gitLocalRootPath/src $dotnet_build_param"
}

######################################################################################
Log-Block -Stage "Build" -Section "Pack" -Task "Creating a nuget package."

if ($null -ne $dotnet_pack_param)
{
    Execute-Command "dotnet pack $gitLocalRootPath/src $dotnet_pack_param"
}

######################################################################################
Log-Block -Stage "Build" -Section "Docfx" -Task "Creating the docs."

if ($null -ne $docfx_param)
{
    Execute-Command "docfx $docfx_param"
}

######################################################################################
Log-Block -Stage "Build" -Section "Docfx" -Task "Copying the docs."

if ($null -ne $docfx_param)
{
    Copy-Directory -sourceDir "$gitLocalRootPath/src/Projects/Coree.NETStandard/Docfx/result/local/" -destinationDir "$gitLocalRootPath/docs/docfx" -exclusions @('.git', '.github')
}

######################################################################################
Log-Block -Stage "Deploy" -Section "Nuget" -Task "Nuget"

if ($branchSegment -ieq "feature") {

    $basePath = "$gitLocalRootPath/src/Projects/Coree.NETStandard"
    $pattern = "*.nupkg"
    $firstFileMatch = Get-ChildItem -Path $basePath -Filter $pattern -File -Recurse | Select-Object -First 1
    Execute-Command "dotnet nuget add source --username carsten-riedel --password $PAT --store-password-in-clear-text --name github ""https://nuget.pkg.github.com/carsten-riedel/index.json"""
    Execute-Command "dotnet nuget push ""$($firstFileMatch.FullName)"" --api-key $PAT --source ""github"""

} elseif ($branchSegment -ieq "develop") {

    $basePath = "$gitLocalRootPath/src/Projects/Coree.NETStandard"
    $pattern = "*.nupkg"
    $firstFileMatch = Get-ChildItem -Path $basePath -Filter $pattern -File -Recurse | Select-Object -First 1
    Execute-Command "dotnet nuget add source --username carsten-riedel --password $PAT --store-password-in-clear-text --name github ""https://nuget.pkg.github.com/carsten-riedel/index.json"""
    Execute-Command "dotnet nuget push ""$($firstFileMatch.FullName)"" --api-key $PAT --source ""github"""

} elseif ($branchSegment -ieq "release") {

    $basePath = "$gitLocalRootPath/src/Projects/Coree.NETStandard"
    $pattern = "*.nupkg"
    $firstFileMatch = Get-ChildItem -Path $basePath -Filter $pattern -File -Recurse | Select-Object -First 1
    Execute-Command "dotnet nuget add source --username carsten-riedel --password $PAT --store-password-in-clear-text --name github ""https://nuget.pkg.github.com/carsten-riedel/index.json"""
    Execute-Command "dotnet nuget push ""$($firstFileMatch.FullName)"" --api-key $PAT --source ""github"""

    dotnet nuget push "$($firstFileMatch.FullName)" --api-key $NUGET_TEST_PAT --source https://apiint.nugettest.org/v3/index.json

} elseif ($branchSegment -ieq "master") {

    $basePath = "$gitLocalRootPath/src/Projects/Coree.NETStandard"
    $pattern = "*.nupkg"
    $firstFileMatch = Get-ChildItem -Path $basePath -Filter $pattern -File -Recurse | Select-Object -First 1
    Execute-Command "dotnet nuget add source --username carsten-riedel --password $PAT --store-password-in-clear-text --name github ""https://nuget.pkg.github.com/carsten-riedel/index.json"""
    Execute-Command "dotnet nuget push ""$($firstFileMatch.FullName)"" --api-key $PAT --source ""github"""

    dotnet nuget push "$($firstFileMatch.FullName)" --api-key $NUGET_PAT --source https://api.nuget.org/v3/index.json

} elseif ($branchSegment -ieq "hotfix") {

    $basePath = "$gitLocalRootPath/src/Projects/Coree.NETStandard"
    $pattern = "*.nupkg"
    $firstFileMatch = Get-ChildItem -Path $basePath -Filter $pattern -File -Recurse | Select-Object -First 1
    Execute-Command "dotnet nuget add source --username carsten-riedel --password $PAT --store-password-in-clear-text --name github ""https://nuget.pkg.github.com/carsten-riedel/index.json"""
    Execute-Command "dotnet nuget push ""$($firstFileMatch.FullName)"" --api-key $PAT --source ""github"""

    dotnet nuget push "$($firstFileMatch.FullName)" --api-key $NUGET_PAT --source https://api.nuget.org/v3/index.json
}

######################################################################################
Log-Block -Stage "Post Deploy" -Section "Tag and Push" -Task ""

if ($branchSegment -eq "master" -OR $branchSegment -eq "release" -OR $branchSegment -eq "hostfix")
{
    $tag = "v$FullVersion"
}
else {
    $tag = "v$FullVersion-$branchSegment"
}

Execute-Command "git add $gitLocalRootPath/docs/docfx"
Execute-Command "git commit -m ""Updated form Workflow"""
Execute-Command "git tag -a ""$tag"" -m ""[no ci]"""
Execute-Command "git push origin ""$tag"""

######################################################################################
Log-Block -Stage "Post Deploy" -Section "Cleanup Packagelist" -Task ""

$headers = @{
    Authorization = "Bearer $PAT"
}

$GitHubNugetPackagelist = Invoke-RestMethod -Uri "https://api.github.com/users/carsten-riedel/packages/nuget/$gitLocalRootDir/versions" -Headers $headers

$GitHubNugetPackagelistOld = $GitHubNugetPackagelist | Where-Object { $_.name -like "*$branchSegment" } | Sort-Object -Property created_at -Descending | Select-Object -Skip 2

foreach ($item in $GitHubNugetPackagelistOld)
{
    $PackageId = $item.id
    Invoke-RestMethod -Method Delete -Uri "https://api.github.com/users/carsten-riedel/packages/nuget/$gitLocalRootDir/versions/$PackageId" -Headers $headers | Out-Null
    Write-Output "Unlisted package $gitLocalRootDir $($item.name)"
}

#if (-not $GitHubNugetPackagelistOld) {
#    $GitHubNugetPackagelistOld = $GitHubNugetPackagelist | Where-Object { $_.name -like "*$branchSegment" } | Sort-Object -Property created_at -Descending | Select-Object -Skip 2
#}



#$env:GITHUB_REPOSITORY_OWNER
#$env:GITHUB_REPOSITORY
#$env:GITHUB_WORKSPACE