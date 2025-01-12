function Find-File-Up {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$FileName,
        
        [Parameter(Mandatory = $false)]
        [string]$StartPath = (Get-Location).Path
    )

    # Convert the start path to absolute path
    $currentPath = Resolve-Path $StartPath

    while ($true) {
        # Check if the file exists in the current directory
        $filePath = Join-Path $currentPath $FileName
        if (Test-Path $filePath) {
            return $filePath
        }

        # Get the parent directory
        $parentPath = Split-Path $currentPath -Parent

        # If we're at the root directory and haven't found the file, return null
        if ($parentPath -eq $null -or $currentPath -eq $parentPath) {
            return $null
        }

        # Move up to the parent directory
        $currentPath = $parentPath
    }
}

# Example usage:
# Find-Up "package.json"
# Find-Up "web.config" -StartPath "C:\Projects\MyWebApp\src"