function Get-GithubActionsBranchName {
    if ($env:GITHUB_HEAD_REF) {
        return $env:GITHUB_HEAD_REF
    } elseif ($env:GITHUB_REF) {
        return $env:GITHUB_REF -replace 'refs/heads/', ''
    }
    return $null
}

function Get-GitBranchName {
    try {
        $branch = git rev-parse --abbrev-ref HEAD
        return $branch
    } catch {
        return $null
    }
}

function IsGithubActions {
    $githubActionsBranchName = Get-GithubActionsBranchName
    return -not [string]::IsNullOrWhiteSpace($githubActionsBranchName)
}

function IsGit {
    # Attempt to get the GitHub Actions branch name
    $githubBranchName = Get-GithubActionsBranchName
    # Attempt to get the local git branch name
    $gitBranchName = Get-GitBranchName

    # Check if either GitHub Actions branch name or local git branch name is available
    if (-not [string]::IsNullOrWhiteSpace($githubBranchName) -or -not [string]::IsNullOrWhiteSpace($gitBranchName)) {
        return $true
    } else {
        return $false
    }
}

function Get-BranchNameParts {
    param (
        [string]$branchName
    )

    # Split the branch name by the '/' delimiter
    $parts = $branchName -split '/'

    # Return the array of parts
    return $parts
}

function Log-Block {
    [Diagnostics.CodeAnalysis.SuppressMessageAttribute("PSUseApprovedVerbs", "")]
    param (
        [string]$Stage,
        [string]$Section,
        [string]$Task
    )
    Write-Output "_"
    Write-Output "==============================================================================================================="
    if (-not [string]::IsNullOrEmpty($Stage)) {
        $output =  "Stage: {0} Section: {1} Task: {2} " -f $Stage.PadRight(15), $Section.PadRight(20), $Task.PadRight(35)
        Write-Output $output
    }
    Write-Output "==============================================================================================================="
}

function Test-CommandAvailability {
    param (
        [string]$CommandName
    )

    try {
        $null = Get-Command $CommandName -ErrorAction Stop
        return $true
    } catch {
        return $false
    }
}

function Ensure-CommandAvailability {
    [Diagnostics.CodeAnalysis.SuppressMessageAttribute("PSUseApprovedVerbs", "")]
    param (
        [Parameter(Mandatory = $true)]
        [string]$CommandName
    )
    process {
        $isAvailible = $false

        try {
            [Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSUserDeclaredVarsMoreThanAssignments','')]
            $commandInfo = Get-Command $CommandName -ErrorAction Stop
            $isAvailible = $true
            $output = "Command is available   : {0}" -f $CommandName
            Write-Output $output
        }
        catch {}        
        if ($isAvailible -eq $false)
        {
            $output = "Command is not available : {0}" -f $CommandName
            throw "$output";
        }
    }
}

function Ensure-VariableSet {
    [Diagnostics.CodeAnalysis.SuppressMessageAttribute("PSUseApprovedVerbs", "")]
    param (
        [Parameter(Mandatory = $true)]
        [string]$VariableName,
        
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [AllowNull()]
        [string]$VariableValue
    )
    process {
        if ([string]::IsNullOrEmpty($VariableValue)) {
            $output = "VariableName: {0} is not set." -f $VariableName.PadRight(30)
            Write-Output $output
            throw "$output";
        }
        else {
            Write-Output ("VariableName: {0} is set." -f $VariableName.PadRight(30))
        }
    }
}

