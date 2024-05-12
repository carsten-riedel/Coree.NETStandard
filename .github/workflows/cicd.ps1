$ErrorActionPreference = 'Stop'

$version    = $PSVersionTable.PSVersion.ToString()
$datetime   = Get-Date -f 'yyyyMMdd_HHmmss'
$filename   = "Transcript-${version}-${datetime}.log"
$Transcript = Join-Path -Path "$PSScriptRoot" -ChildPath $filename
Start-Transcript -Path "$Transcript"


. "$PSScriptRoot/cicd_helper.ps1"

. "$PSScriptRoot/cicd_setup.ps1"

. "$PSScriptRoot/cicd_preconditions.ps1"

. "$PSScriptRoot/cicd_clean.ps1"

. "$PSScriptRoot/cicd_build_config.ps1"

. "$PSScriptRoot/cicd_build.ps1"

. "$PSScriptRoot/cicd_deploy.ps1"

######################################################################################
Log-Block -Stage "Resolving" -Section "Branch" -Task "Config values for branches"

# Some variables can be $null or unset indicating a skipping step.

if ($branchNameSegment -ieq "feature") {

    $version = "--property:AssemblyVersion=$fullVersion --property:VersionPrefix=$fullVersion --property:VersionSuffix=$branchNameSegment"

    $dotnet_restore_param = "";
    $dotnet_build_param = "--no-restore --configuration Release --property:ContinuousIntegrationBuild=true --property:WarningLevel=3 $version";
    $dotnet_pack_param =  "--force --configuration Release --property:ContinuousIntegrationBuild=true --property:WarningLevel=3 $version";
    $docfx_param = $null

} elseif ($branchNameSegment -ieq "develop") {

    $version = "--property:AssemblyVersion=$fullVersion --property:VersionPrefix=$fullVersion --property:VersionSuffix=$branchNameSegment"

    $dotnet_restore_param = "";
    $dotnet_build_param = "--no-restore --configuration Release --property:ContinuousIntegrationBuild=true --property:WarningLevel=3 $version";
    $dotnet_pack_param =  "--force --configuration Release --property:ContinuousIntegrationBuild=true --property:WarningLevel=3 $version";
    $docfx_param = $null

} elseif ($branchNameSegment -ieq "release") {

    $version = "--property:AssemblyVersion=$fullVersion --property:VersionPrefix=$fullVersion"

    $dotnet_restore_param = "";
    $dotnet_build_param = "--no-restore --configuration Release --property:ContinuousIntegrationBuild=true --property:WarningLevel=3 $version";
    $dotnet_pack_param =  "--force --configuration Release --property:ContinuousIntegrationBuild=true --property:WarningLevel=3 $version";
    $docfx_param = $null

} elseif ($branchNameSegment -ieq "master") {

    $version = "--property:AssemblyVersion=$fullVersion --property:VersionPrefix=$fullVersion"

    $dotnet_restore_param = "";
    $dotnet_build_param = "--no-restore --configuration Release --property:ContinuousIntegrationBuild=true --property:WarningLevel=3 $version";
    $dotnet_pack_param =  "--force --configuration Release --property:ContinuousIntegrationBuild=true --property:WarningLevel=3 $version";
    $docfx_param = "$topLevelPath/docfx/build/docfx_local.json"

} elseif ($branchNameSegment -ieq "hotfix") {

    $version = "--property:AssemblyVersion=$fullVersion --property:VersionPrefix=$fullVersion"

    $dotnet_restore_param = "";
    $dotnet_build_param = "--no-restore --configuration Release --property:ContinuousIntegrationBuild=true --property:WarningLevel=3 $version";
    $dotnet_pack_param =  "--force --configuration Release --property:ContinuousIntegrationBuild=true --property:WarningLevel=3 $version";
    $docfx_param = $null

}

######################################################################################
Log-Block -Stage "Setup" -Section "Tools" -Task "Add addtional nuget source"

Execute-Command "dotnet nuget add source --username carsten-riedel --password $PAT --store-password-in-clear-text --name github ""https://nuget.pkg.github.com/carsten-riedel/index.json"""

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
    Execute-Command "dotnet nuget push ""$($firstFileMatch.FullName)"" --api-key $PAT --source ""github"""

} elseif ($branchNameSegment -ieq "develop") {

    $basePath = "$topLevelPath/src/Projects/Coree.NETStandard"
    $pattern = "*.nupkg"
    $firstFileMatch = Get-ChildItem -Path $basePath -Filter $pattern -File -Recurse | Select-Object -First 1
    Execute-Command "dotnet nuget push ""$($firstFileMatch.FullName)"" --api-key $PAT --source ""github"""

} elseif ($branchNameSegment -ieq "release") {

    $basePath = "$topLevelPath/src/Projects/Coree.NETStandard"
    $pattern = "*.nupkg"
    $firstFileMatch = Get-ChildItem -Path $basePath -Filter $pattern -File -Recurse | Select-Object -First 1
    Execute-Command "dotnet nuget push ""$($firstFileMatch.FullName)"" --api-key $PAT --source ""github"""

    dotnet nuget push "$($firstFileMatch.FullName)" --api-key $NUGET_TEST_PAT --source https://apiint.nugettest.org/v3/index.json

} elseif ($branchNameSegment -ieq "master") {

    $basePath = "$topLevelPath/src/Projects/Coree.NETStandard"
    $pattern = "*.nupkg"
    $firstFileMatch = Get-ChildItem -Path $basePath -Filter $pattern -File -Recurse | Select-Object -First 1
    Execute-Command "dotnet nuget push ""$($firstFileMatch.FullName)"" --api-key $PAT --source ""github"""

    dotnet nuget push "$($firstFileMatch.FullName)" --api-key $NUGET_PAT --source https://api.nuget.org/v3/index.json

} elseif ($branchNameSegment -ieq "hotfix") {

    $basePath = "$topLevelPath/src/Projects/Coree.NETStandard"
    $pattern = "*.nupkg"
    $firstFileMatch = Get-ChildItem -Path $basePath -Filter $pattern -File -Recurse | Select-Object -First 1
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


$gitUserLocal = git config user.name
$gitMailLocal = git config user.email

# Check if the variables are null or empty (including whitespace)
if ([string]::IsNullOrWhiteSpace($gitUserLocal) -or [string]::IsNullOrWhiteSpace($gitMailLocal)) {
    $gitTempUser= "Workflow"
    $gitTempMail = "carstenriedel@outlook.com"  # Assuming a placeholder email
} else {
    $gitTempUser= $gitUserLocal
    $gitTempMail = $gitMailLocal
}

git config user.name $gitTempUser
git config user.email $gitTempMail

Execute-Command "git add --all"
Execute-Command "git commit -m ""Updated form Workflow [no ci]"""
Execute-Command "git push origin $branchName"
Execute-Command "git tag -a ""$tag"" -m ""[no ci]"""
Execute-Command "git push origin ""$tag"""

#restore
git config user.name $gitUserLocal
git config user.email $gitMailLocal

######################################################################################
Log-Block -Stage "Post Deploy" -Section "Cleanup Packagelist" -Task ""

$headers = @{
    Authorization = "Bearer $PAT"
}

$GitHubNugetPackagelist = Invoke-RestMethod -Uri "https://api.github.com/users/$gitOwner/packages/nuget/$gitRepo/versions" -Headers $headers | Out-Null
$GitHubNugetPackagelistOld = $GitHubNugetPackagelist | Where-Object { $_.name -like "*$branchNameSegment" } | Sort-Object -Property created_at -Descending | Select-Object -Skip 2
foreach ($item in $GitHubNugetPackagelistOld)
{
    $PackageId = $item.id
    Invoke-RestMethod -Method Delete -Uri "https://api.github.com/users/$gitOwner/packages/nuget/$gitRepo/versions/$PackageId" -Headers $headers | Out-Null
    Write-Output "Unlisted package $gitRepo $($item.name)"
}

######################################################################################
Log-Block -Stage "Call" -Section "Dispatch" -Task "dispatching a other job"

if ($branchNameSegment -ieq "master") {
    $worklowFileName = "static.yml"
    $uri = "https://api.github.com/repos/$gitOwner/$gitRepo/actions/workflows/$worklowFileName/dispatches"
    $headers = @{
        "Accept" = "application/vnd.github+json"
        "X-GitHub-Api-Version" = "2022-11-28"
        "Authorization" = "Bearer $PAT"
        "Content-Type" = "application/json"
    }
    $body = @{
        ref = "$branchName"
    } | ConvertTo-Json
    
    Invoke-WebRequest -Uri $uri -Method Post -Headers $headers -Body $body -Verbose | Out-Null
}


#git status --porcelain $sourceCodeFolder

Stop-Transcript