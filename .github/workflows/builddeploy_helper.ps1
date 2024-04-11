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

function Clear-BinObjDirectories {
    param(
        [string]$sourceDirectory = "source"
    )

    # Define bin and obj folder paths
    $binFolderPath = Join-Path -Path $sourceDirectory -ChildPath "bin"
    $objFolderPath = Join-Path -Path $sourceDirectory -ChildPath "obj"

    # Ensure that $sourceDir and $destinationDir are absolute paths
    if (-not [System.IO.Path]::IsPathRooted($binFolderPath)) {
        $binFolderPath = Join-Path (Get-Location) $binFolderPath
    }

    if (-not [System.IO.Path]::IsPathRooted($objFolderPath)) {
        $objFolderPath = Join-Path (Get-Location) $objFolderPath
    }

    # Function to delete files and directory
    function Delete-DirectoryContents {
        param(
            [System.IO.DirectoryInfo]$directory
        )

        if ($directory.Exists) {
            $files = Get-ChildItem -Path $directory.FullName -Recurse -File
            foreach ($file in $files) {
                try {
                    Remove-Item $file.FullName -Force
                    Write-Output "Deleted file: $($file.FullName)."
                } catch {
                    Write-Output "Could not delete file: $($file.FullName)."
                }
            }

            try {
                Remove-Item $directory.FullName -Recurse -Force
                Write-Output "Deleted directory: $($directory.FullName)."
            } catch {
                Write-Output"Could not delete directory: $($directory.FullName)."
            }
        }
    }

    # Create DirectoryInfo objects
    $binFolder = [System.IO.DirectoryInfo]::new($binFolderPath)
    $objFolder = [System.IO.DirectoryInfo]::new($objFolderPath)

    # Delete contents of bin and obj directories
    Delete-DirectoryContents -directory $binFolder
    Delete-DirectoryContents -directory $objFolder
}

function Invoke-Process {
    param (
        [string]$ProcessName,
        [string[]]$ArgumentList
    )

    # Construct the argument list string for display
    $arguments = $ArgumentList -join ' '
    
    # Display the command being invoked
    Write-Output "Invoking command: $ProcessName $arguments"
    
    # Execute the command
    Start-Process -FilePath $ProcessName -NoNewWindow -Wait -ArgumentList $ArgumentList
}