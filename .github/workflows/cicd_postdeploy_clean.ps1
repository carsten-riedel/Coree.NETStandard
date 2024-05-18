
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
