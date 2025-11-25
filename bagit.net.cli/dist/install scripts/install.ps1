# install.ps1 - Install BagIt.NET CLI to %LOCALAPPDATA%\bagit.net

$TargetDir = "$env:LOCALAPPDATA\bagit.net"
$Exe = "bagit.net.exe"

if (-Not (Test-Path $Exe)) {
    Write-Error "$Exe not found in current directory."
    exit 1
}

# Create the target directory if it doesn't exist
if (-Not (Test-Path $TargetDir)) {
    New-Item -ItemType Directory -Force -Path $TargetDir | Out-Null
}

# Copy the executable
Copy-Item -Path $Exe -Destination $TargetDir -Force

Write-Host "Installation complete."
Write-Host "You can now run BagIt.NET CLI using:"
Write-Host "$TargetDir\$Exe"
Write-Host ""
Write-Host "Optional: add it to your PATH via System Properties in the Control Panel."
