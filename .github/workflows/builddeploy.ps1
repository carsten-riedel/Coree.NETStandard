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

if ($isGithubActions -eq $true) { Write-Output "Is github actions." } else { "Is not github actions." }
Write-Output "Branch name is $branchName"
Write-Output "BranchRootName name is $firstBranchSegment"

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

$secretsPath = "$PSScriptRoot/builddeploysecrets.ps1"
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
Ensure-CommandAvailability -CommandName "powershell"

######################################################################################
Log-Block -Stage "Initialization" -Section "Base" -Task "Config values for branches"

if ($firstBranchSegment -ieq "feature") {

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
    Install-Module -Name PSPGP
}

######################################################################################
Log-Block -Stage "Build" -Section "Prepare" -Task ""

Clear-BinObjDirectories -sourceDirectory "src/Projects/Coree.NETStandard"


### continue
