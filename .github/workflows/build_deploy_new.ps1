$ErrorActionPreference = 'Stop'

. "$PSScriptRoot/build_deploy_helper_new.ps1"

######################################################################################
Log-Block -Stage "Resolving" -Section "Preconditions" -Task "Set fixed values"

$sourceCodeFolder = "src"
$versionMajor = "0"
$versionMinor = "1"
$versionBuild = Get-BaseVersionBuild
$versionRevision = Get-BaseVersionRevision
$fullVersion = "$versionMajor.$versionMinor.$versionBuild.$versionRevision"

######################################################################################
Log-Block -Stage "Checking" -Section "Preconditions" -Task "Commands setup."

# Check availability of required commands
$result = Test-CommandsAvailabilities -CommandList @("git", "dotnet", "pwsh")
if (-not $result) {
    Write-Host "One or more required commands are unavailable. Stopping execution."
    exit 1
}

######################################################################################
Log-Block -Stage "Checking" -Section "Preconditions" -Task "Git setup."

# Verify that the current directory is a Git repository
$result = Test-IsGitDirectory
if (-not $result) {
    Write-Host "The directory is not a Git repository."
    exit 1
}

# Check for the existence of a remote named 'origin' in the current Git repository
$result = Test-GitRemoteExistence
if (-not $result) {
    Write-Host "This Git repository does not have a remote named 'origin'."
    exit 1
}

######################################################################################
Log-Block -Stage "Checking" -Section "Preconditions" -Task "Github setup."

$result = Test-GitHubRemoteExistence
if (-not $result) {
    Write-Host "This Git is not a github repository."
    exit 1
}

######################################################################################
Log-Block -Stage "Resolving" -Section "Preconditions" -Task "Branchnames and Paths."

$branchName = Get-GitBranchName
$branchNameSegment = @(Get-NormalizedPathSegments -InputPath $branchName)[0].ToLower()
 
$topLevelPath = Get-GitTopLevelPath
$topLevelDirectory = @(Get-NormalizedPathSegments -InputPath $topLevelPath)[-1]

$gitRemoteOriginUrl = Get-GitRemoteOriginUrl
$gitOwner = @(Get-NormalizedPathSegments -InputPath $gitRemoteOriginUrl)[1]
$gitRepo = @(Get-NormalizedPathSegments -InputPath $gitRemoteOriginUrl)[2]

Write-Host "branchName is         : $branchName"
Write-Host "branchNameSegment is  : $branchNameSegment"
Write-Host "topLevelPath is       : $topLevelPath"
Write-Host "topLevelDirectory is  : $topLevelDirectory"
Write-Host "gitRemoteOriginUrl is : $gitRemoteOriginUrl"
Write-Host "gitOwner is           : $gitOwner"
Write-Host "gitRepo is            : $gitRepo"
Write-Host "fullVersion is        : $fullVersion"

######################################################################################
Log-Block -Stage "Checking" -Section "Preconditions" -Task "Valid branch."

$isValidBranchRootName = @("feature", "develop", "release", "master" , "hotfix" )

if (-not($isValidBranchRootName.ToLower() -contains $branchNameSegment)) {
    Write-Host "No configuration for branches $branchNameSegment. Exiting"
    exit 1
}
else {
    Write-Host "Configuration for branch '$branchNameSegment' will be used."
}

######################################################################################
Log-Block -Stage "Resolving" -Section "Preconditions" -Task "Secrets"

$PAT = $args[0]
$NUGET_PAT = $args[1]
$NUGET_TEST_PAT = $args[2]

$secretsPath = "$PSScriptRoot/build_deploy_new_secrets.ps1"
# Check if the secrets file exists before importing
if (Test-Path $secretsPath) {
    . "$secretsPath"
    Write-Host "Secrets loaded from file."
} else {
    Write-Host "Secrets will be taken from args."
}

######################################################################################
Log-Block -Stage "Resolving" -Section "Branch" -Task "Config values for branches"

if ($branchNameSegment -ieq "feature") {

    $version = "--property:AssemblyVersion=$fullVersion --property:VersionPrefix=$fullVersion --property:VersionSuffix=$branchNameSegment"

    $dotnet_restore_param = "";
    $dotnet_build_param = "--no-restore --configuration Release --property:ContinuousIntegrationBuild=true --property:WarningLevel=3 $version";
    $dotnet_pack_param =  "--force --configuration Release --property:ContinuousIntegrationBuild=true --property:WarningLevel=3 $version";
    $docfx_param = "$topLevelPath/src/Projects/Coree.NETStandard/Docfx/build/docfx_local.json"

} elseif ($branchNameSegment -ieq "develop") {

    $version = "--property:AssemblyVersion=$fullVersion --property:VersionPrefix=$fullVersion --property:VersionSuffix=$branchNameSegment"

    $dotnet_restore_param = "";
    $dotnet_build_param = "--no-restore --configuration Release --property:ContinuousIntegrationBuild=true --property:WarningLevel=3 $version";
    $dotnet_pack_param =  "--force --configuration Release --property:ContinuousIntegrationBuild=true --property:WarningLevel=3 $version";
    $docfx_param = "$topLevelPath/src/Projects/Coree.NETStandard/Docfx/build/docfx_local.json"

} elseif ($branchNameSegment -ieq "release") {

    $version = "--property:AssemblyVersion=$fullVersion --property:VersionPrefix=$fullVersion"

    $dotnet_restore_param = "";
    $dotnet_build_param = "--no-restore --configuration Release --property:ContinuousIntegrationBuild=true --property:WarningLevel=3 $version";
    $dotnet_pack_param =  "--force --configuration Release --property:ContinuousIntegrationBuild=true --property:WarningLevel=3 $version";
    $docfx_param = "$topLevelPath/src/Projects/Coree.NETStandard/Docfx/build/docfx_local.json"

} elseif ($branchNameSegment -ieq "master") {

    $version = "--property:AssemblyVersion=$fullVersion --property:VersionPrefix=$fullVersion"

    $dotnet_restore_param = "";
    $dotnet_build_param = "--no-restore --configuration Release --property:ContinuousIntegrationBuild=true --property:WarningLevel=3 $version";
    $dotnet_pack_param =  "--force --configuration Release --property:ContinuousIntegrationBuild=true --property:WarningLevel=3 $version";
    $docfx_param = "$topLevelPath/src/Projects/Coree.NETStandard/Docfx/build/docfx_local.json"

} elseif ($branchNameSegment -ieq "hotfix") {

    $version = "--property:AssemblyVersion=$fullVersion --property:VersionPrefix=$fullVersion"

    $dotnet_restore_param = "";
    $dotnet_build_param = "--no-restore --configuration Release --property:ContinuousIntegrationBuild=true --property:WarningLevel=3 $version";
    $dotnet_pack_param =  "--force --configuration Release --property:ContinuousIntegrationBuild=true --property:WarningLevel=3 $version";
    $docfx_param = "$topLevelPath/src/Projects/Coree.NETStandard/Docfx/build/docfx_local.json"

}

######################################################################################
Log-Block -Stage "Checking" -Section "Preconditions" -Task "Variables set."

Ensure-VariableSet -VariableName "`$branchName" -VariableValue "$branchName"
Ensure-VariableSet -VariableName "`$branchNameSegment" -VariableValue "$branchNameSegment"
Ensure-VariableSet -VariableName "`$topLevelPath" -VariableValue "$topLevelPath"
Ensure-VariableSet -VariableName "`$topLevelDirectory" -VariableValue "$topLevelDirectory"
Ensure-VariableSet -VariableName "`$gitRemoteOriginUrl" -VariableValue "$gitRemoteOriginUrl"
Ensure-VariableSet -VariableName "`$gitOwner" -VariableValue "$gitOwner"
Ensure-VariableSet -VariableName "`$gitRepo" -VariableValue "$gitRepo"
Ensure-VariableSet -VariableName "`$sourceCodeFolder" -VariableValue "$sourceCodeFolder"
Ensure-VariableSet -VariableName "`$versionMajor" -VariableValue "$versionMajor"
Ensure-VariableSet -VariableName "`$versionMinor" -VariableValue "$versionMinor"
Ensure-VariableSet -VariableName "`$versionBuild" -VariableValue "$versionBuild"
Ensure-VariableSet -VariableName "`$versionRevision" -VariableValue "$versionRevision"
Ensure-VariableSet -VariableName "`$fullVersion" -VariableValue "$fullVersion"
Ensure-VariableSet -VariableName "`$PAT" -VariableValue "$PAT"
Ensure-VariableSet -VariableName "`$NUGET_PAT" -VariableValue "$NUGET_PAT"
Ensure-VariableSet -VariableName "`$NUGET_TEST_PAT" -VariableValue "$NUGET_TEST_PAT"

######################################################################################
Log-Block -Stage "Resolving" -Section "Branch" -Task "Config values for branches"

# Some variables can be $null or unset indicating a skipping step.

if ($branchNameSegment -ieq "feature") {

    $version = "--property:AssemblyVersion=$fullVersion --property:VersionPrefix=$fullVersion --property:VersionSuffix=$branchNameSegment"

    $dotnet_restore_param = "";
    $dotnet_build_param = "--no-restore --configuration Release --property:ContinuousIntegrationBuild=true --property:WarningLevel=3 $version";
    $dotnet_pack_param =  "--force --configuration Release --property:ContinuousIntegrationBuild=true --property:WarningLevel=3 $version";
    $docfx_param = "$topLevelPath/src/Projects/Coree.NETStandard/Docfx/build/docfx_local.json"

} elseif ($branchNameSegment -ieq "develop") {

    $version = "--property:AssemblyVersion=$fullVersion --property:VersionPrefix=$fullVersion --property:VersionSuffix=$branchNameSegment"

    $dotnet_restore_param = "";
    $dotnet_build_param = "--no-restore --configuration Release --property:ContinuousIntegrationBuild=true --property:WarningLevel=3 $version";
    $dotnet_pack_param =  "--force --configuration Release --property:ContinuousIntegrationBuild=true --property:WarningLevel=3 $version";
    $docfx_param = "$topLevelPath/src/Projects/Coree.NETStandard/Docfx/build/docfx_local.json"

} elseif ($branchNameSegment -ieq "release") {

    $version = "--property:AssemblyVersion=$fullVersion --property:VersionPrefix=$fullVersion"

    $dotnet_restore_param = "";
    $dotnet_build_param = "--no-restore --configuration Release --property:ContinuousIntegrationBuild=true --property:WarningLevel=3 $version";
    $dotnet_pack_param =  "--force --configuration Release --property:ContinuousIntegrationBuild=true --property:WarningLevel=3 $version";
    $docfx_param = "$topLevelPath/src/Projects/Coree.NETStandard/Docfx/build/docfx_local.json"

} elseif ($branchNameSegment -ieq "master") {

    $version = "--property:AssemblyVersion=$fullVersion --property:VersionPrefix=$fullVersion"

    $dotnet_restore_param = "";
    $dotnet_build_param = "--no-restore --configuration Release --property:ContinuousIntegrationBuild=true --property:WarningLevel=3 $version";
    $dotnet_pack_param =  "--force --configuration Release --property:ContinuousIntegrationBuild=true --property:WarningLevel=3 $version";
    $docfx_param = "$topLevelPath/src/Projects/Coree.NETStandard/Docfx/build/docfx_local.json"

} elseif ($branchNameSegment -ieq "hotfix") {

    $version = "--property:AssemblyVersion=$fullVersion --property:VersionPrefix=$fullVersion"

    $dotnet_restore_param = "";
    $dotnet_build_param = "--no-restore --configuration Release --property:ContinuousIntegrationBuild=true --property:WarningLevel=3 $version";
    $dotnet_pack_param =  "--force --configuration Release --property:ContinuousIntegrationBuild=true --property:WarningLevel=3 $version";
    $docfx_param = "$topLevelPath/src/Projects/Coree.NETStandard/Docfx/build/docfx_local.json"

}

######################################################################################
Log-Block -Stage "Setup" -Section "Tools" -Task "Install dotnet tools"

if (-not (Test-CommandAvailability -CommandName "docfx"))
{
    Execute-Command "dotnet tool install --global docfx --version 2.74.1"
}

######################################################################################
Log-Block -Stage "Setup" -Section "Tools" -Task "Install powershell modules"

if (-not (Test-CommandAvailability -CommandName "New-PGPKey"))
{
    #Install-Module -Name PSPGP -AcceptLicense -AllowClobber -AllowPrerelease -Force
}

######################################################################################
Log-Block -Stage "Setup" -Section "Clean" -Task "Clean local binaries"

# Example of how to call the function and capture the results
$results = Find-SpecialSubfolders -Path "$topLevelPath/$sourceCodeFolder"
foreach($item in $results)
{
    Remove-FilesAndDirectories -FolderPath $item
}

######################################################################################
Log-Block -Stage "Build" -Section "Restore" -Task "Restoreing nuget packages."

if ($null -ne $dotnet_restore_param)
{
    Execute-Command "dotnet restore $topLevelPath/$sourceCodeFolder $dotnet_restore_param"
}

######################################################################################
Log-Block -Stage "Build" -Section "Build" -Task "Building the solution."


if ($null -ne $dotnet_build_param)
{
    Execute-Command "dotnet build $topLevelPath/$sourceCodeFolder $dotnet_build_param"
}

######################################################################################
Log-Block -Stage "Build" -Section "Pack" -Task "Creating a nuget package."

if ($null -ne $dotnet_pack_param)
{
    Execute-Command "dotnet pack $topLevelPath/$sourceCodeFolder $dotnet_pack_param"
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
    Copy-Directory -sourceDir "$topLevelPath/src/Projects/Coree.NETStandard/Docfx/result/local/" -destinationDir "$topLevelPath/docs/docfx" -exclusions @('.git', '.github')
}

######################################################################################
Log-Block -Stage "Deploy" -Section "Nuget" -Task "Nuget"

if ($branchNameSegment -ieq "feature") {

    $basePath = "$topLevelPath/src/Projects/Coree.NETStandard"
    $pattern = "*.nupkg"
    $firstFileMatch = Get-ChildItem -Path $basePath -Filter $pattern -File -Recurse | Select-Object -First 1
    Execute-Command "dotnet nuget add source --username carsten-riedel --password $PAT --store-password-in-clear-text --name github ""https://nuget.pkg.github.com/carsten-riedel/index.json"""
    Execute-Command "dotnet nuget push ""$($firstFileMatch.FullName)"" --api-key $PAT --source ""github"""

} elseif ($branchNameSegment -ieq "develop") {

    $basePath = "$topLevelPath/src/Projects/Coree.NETStandard"
    $pattern = "*.nupkg"
    $firstFileMatch = Get-ChildItem -Path $basePath -Filter $pattern -File -Recurse | Select-Object -First 1
    Execute-Command "dotnet nuget add source --username carsten-riedel --password $PAT --store-password-in-clear-text --name github ""https://nuget.pkg.github.com/carsten-riedel/index.json"""
    Execute-Command "dotnet nuget push ""$($firstFileMatch.FullName)"" --api-key $PAT --source ""github"""

} elseif ($branchNameSegment -ieq "release") {

    $basePath = "$topLevelPath/src/Projects/Coree.NETStandard"
    $pattern = "*.nupkg"
    $firstFileMatch = Get-ChildItem -Path $basePath -Filter $pattern -File -Recurse | Select-Object -First 1
    Execute-Command "dotnet nuget add source --username carsten-riedel --password $PAT --store-password-in-clear-text --name github ""https://nuget.pkg.github.com/carsten-riedel/index.json"""
    Execute-Command "dotnet nuget push ""$($firstFileMatch.FullName)"" --api-key $PAT --source ""github"""

    dotnet nuget push "$($firstFileMatch.FullName)" --api-key $NUGET_TEST_PAT --source https://apiint.nugettest.org/v3/index.json

} elseif ($branchNameSegment -ieq "master") {

    $basePath = "$topLevelPath/src/Projects/Coree.NETStandard"
    $pattern = "*.nupkg"
    $firstFileMatch = Get-ChildItem -Path $basePath -Filter $pattern -File -Recurse | Select-Object -First 1
    Execute-Command "dotnet nuget add source --username carsten-riedel --password $PAT --store-password-in-clear-text --name github ""https://nuget.pkg.github.com/carsten-riedel/index.json"""
    Execute-Command "dotnet nuget push ""$($firstFileMatch.FullName)"" --api-key $PAT --source ""github"""

    dotnet nuget push "$($firstFileMatch.FullName)" --api-key $NUGET_PAT --source https://api.nuget.org/v3/index.json

} elseif ($branchNameSegment -ieq "hotfix") {

    $basePath = "$topLevelPath/src/Projects/Coree.NETStandard"
    $pattern = "*.nupkg"
    $firstFileMatch = Get-ChildItem -Path $basePath -Filter $pattern -File -Recurse | Select-Object -First 1
    Execute-Command "dotnet nuget add source --username carsten-riedel --password $PAT --store-password-in-clear-text --name github ""https://nuget.pkg.github.com/carsten-riedel/index.json"""
    Execute-Command "dotnet nuget push ""$($firstFileMatch.FullName)"" --api-key $PAT --source ""github"""

    dotnet nuget push "$($firstFileMatch.FullName)" --api-key $NUGET_PAT --source https://api.nuget.org/v3/index.json
}

######################################################################################
Log-Block -Stage "Post Deploy" -Section "Tag and Push" -Task ""

if ($branchNameSegment -eq "master" -OR $branchNameSegment -eq "release" -OR $branchNameSegment -eq "hostfix")
{
    $tag = "v$fullVersion"
}
else {
    $tag = "v$fullVersion-$branchNameSegment"
}

Execute-Command "git add $topLevelPath/docs/docfx"
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

git status --porcelain $sourceCodeFolder

#Execute-Command "git add --all"
#Execute-Command "git commit -m ""Updated form Workflow [skip ci]"""
#Execute-Command "git tag -a ""$tag"" -m ""[skip ci]"""
#Execute-Command "git push origin ""$tag"""

$x=1