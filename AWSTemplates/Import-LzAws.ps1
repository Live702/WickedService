# This script will search for the LzAws module in the current directory 
# and all parent directories. It installs the first one it finds
# and installs it into the current session.

# First remove existing module
Remove-Module LzAws -Force -ErrorAction SilentlyContinue

# Get current directory's full path
$currentPath = (Get-Location).Path

# Keep searching until we reach the root
while ($currentPath -ne "") {
    # Check for direct LzAws folder
    $directPath = Join-Path $currentPath "LzAws\lzaws.psm1"
    # Check for LazyMagic/LzAws path
    $nestedPath = Join-Path $currentPath "LazyMagic\LzAws\LzAws.psm1"

    if (Test-Path $directPath) {
        Write-Host "Importing LzAws from $directPath"
        Import-Module $directPath -Force
        break;
    }
    elseif (Test-Path $nestedPath) {
        Write-Host "Importing LzAws from $nestedPath"
        Import-Module $nestedPath -Force
        break;
    }
    
    # Move up one directory
    $currentPath = Split-Path $currentPath -Parent
}

if (-not (Get-Module LzAws)) {
    Write-Error "Could not find LzAws module in parent directories"
}