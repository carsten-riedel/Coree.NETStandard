$ErrorActionPreference = 'Stop'

. "$PSScriptRoot/builddeploy_helper.ps1"

Get-ChildItem Env:* | Select-Object -Property Name,Value

######################################################################################
Log-Block -Stage "Initialization" -Section "Base" -Task "Resolve branchname and action."

$isInGitRepository = IsGit
if (-not $isInGitRepository) {
    Write-Error "This script is not running in a git environment."
    exit 1 # Non-zero exit code indicates an error condition
}

$initialBranchName = Get-GitBranchName
if (-not $initialBranchName) {
    Write-Error "Failed to determine the git branch name."
    exit 1 # Non-zero exit code indicates an error condition
}

$isGithubActions = IsGithubActions
$branchName = Get-GitBranchName
$branchNameParts = @(Get-BranchNameParts -branchName $branchName)
$firstBranchSegment = $branchNameParts[0]
$gitroot = git rev-parse --show-toplevel

if ($isGithubActions -eq $true) { Write-Output "Is github actions." } else { "Is not github actions." }
Write-Output "Branch name is $branchName"
Write-Output "BranchRootName name is $firstBranchSegment"
Write-Output "gitroot is $gitroot"

# Define the array of strings
$isValidBranchRootName = @("feature", "develop", "release", "master" , "hotfix" )

if (-not($isValidBranchRootName.ToLower() -contains $firstBranchSegment.ToLower())) {
    Write-Error "No configuration for branches $firstBranchSegment"
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

if ($firstBranchSegment -ieq "feature") {

    
    $dotnet_restore_param = "";
    $dotnet_build_param = "--no-restore --configuration Release --property:ContinuousIntegrationBuild=true --property:WarningLevel=3 --property:VersionSuffix=feature";
    $dotnet_pack_param =  "--force --configuration Release --property:ContinuousIntegrationBuild=true --property:WarningLevel=3 --property:VersionSuffix=feature";
    $docfx_param = "$gitroot/src/Projects/Coree.NETStandard/Docfx/build/docfx_local.json"

} elseif ($firstBranchSegment -ieq "develop") {

} elseif ($firstBranchSegment -ieq "release") {

} elseif ($firstBranchSegment -ieq "master") {

} elseif ($firstBranchSegment -ieq "hotfix") {

}

######################################################################################
Log-Block -Stage "Setup" -Section "Base" -Task "Install dotnet tools"

if (-not (Test-CommandAvailability -CommandName "docfx"))
{
    dotnet tool install --global docfx --version 2.74.1
}

######################################################################################
Log-Block -Stage "Setup" -Section "Base" -Task "Install powershell modules"

if (-not (Test-CommandAvailability -CommandName "New-PGPKey"))
{
    #Install-Module -Name PSPGP -AcceptLicense -AllowClobber -AllowPrerelease -Force
}

######################################################################################
Log-Block -Stage "Build" -Section "Clean" -Task "Clear projects bin obj"

Clear-BinObjDirectories -sourceDirectory "src/Projects/Coree.NETStandard"

######################################################################################
Log-Block -Stage "Build" -Section "Restore" -Task "Restoreing nuget packages."

if ($null -ne $dotnet_restore_param)
{
    Invoke-Process -ProcessName "dotnet" -ArgumentList @("restore", "$gitRoot/src", $dotnet_restore_param)
}

######################################################################################
Log-Block -Stage "Build" -Section "Build" -Task "Building the solution."


if ($null -ne $dotnet_build_param)
{
    Invoke-Process -ProcessName "dotnet" -ArgumentList @("build", "$gitRoot/src", $dotnet_build_param)
    #start-process dotnet -NoNewWindow -wait -ArgumentList @("build", "$gitroot/src","$dotnet_build_param")
}

######################################################################################
Log-Block -Stage "Build" -Section "Pack" -Task "Creating a nuget package."

if ($null -ne $dotnet_pack_param)
{
    Invoke-Process -ProcessName "dotnet" -ArgumentList @("pack", "$gitRoot/src", $dotnet_pack_param)
}

######################################################################################
Log-Block -Stage "Build" -Section "Tag" -Task ""

if ($null -ne $dotnet_pack_param)
{
    Invoke-Process -ProcessName "git" -ArgumentList @("tag -a ""v0000"" -m ""Tag version"" ")
    Invoke-Process -ProcessName "git" -ArgumentList @("push origin ""v0000""")
}

######################################################################################
Log-Block -Stage "Build" -Section "Docfx" -Task "Creating the docs."

if ($null -ne $docfx_param)
{
    Invoke-Process -ProcessName "docfx" -ArgumentList @($docfx_param)
}

#$env:GITHUB_REPOSITORY_OWNER
#$env:GITHUB_REPOSITORY
#$env:GITHUB_WORKSPACE