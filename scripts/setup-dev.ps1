# Setup script for Tonome Desktop development on Windows
param(
    [switch]$InstallDotnet,
    [switch]$InstallWsl
)

Write-Host "=== Tonome Desktop Development Setup ===" -ForegroundColor Cyan

# Check .NET SDK
try {
    $dotnetVer = dotnet --version
    Write-Host "[OK] .NET SDK $dotnetVer" -ForegroundColor Green
} catch {
    if ($InstallDotnet) {
        Write-Host "Installing .NET SDK..." -ForegroundColor Yellow
        # Download and install .NET SDK
        Invoke-WebRequest -Uri "https://dot.net/v1/dotnet-install.ps1" -OutFile "$env:TEMP\dotnet-install.ps1"
        & "$env:TEMP\dotnet-install.ps1" -Channel 9.0
        Remove-Item "$env:TEMP\dotnet-install.ps1"
    } else {
        Write-Host "[WARN] .NET SDK not found. Install from: https://dotnet.microsoft.com/download" -ForegroundColor Yellow
    }
}

# Check Git
try {
    $gitVer = git --version
    Write-Host "[OK] $gitVer" -ForegroundColor Green
} catch {
    Write-Host "[WARN] Git not found. Install from: https://git-scm.com" -ForegroundColor Yellow
}

# Check for WSL2 (for Linux builds)
if ($InstallWsl) {
    try {
        $wslStatus = wsl -l -v 2>&1
        Write-Host "[OK] WSL detected" -ForegroundColor Green

        # Check if Arch is installed in WSL
        $archWsl = wsl -l -v | Select-String -Pattern "Arch"
        if (-not $archWsl) {
            Write-Host "Installing Arch WSL..." -ForegroundColor Yellow
            # Instructions for Arch WSL setup
            Write-Host "  Install Arch WSL from: https://github.com/yuk7/ArchWSL" -ForegroundColor Cyan
        }
    } catch {
        Write-Host "Installing WSL..." -ForegroundColor Yellow
        wsl --install
    }
}

# Restore NuGet packages
Write-Host "Restoring packages..." -ForegroundColor Cyan
dotnet restore Tonome.sln

Write-Host "=== Setup Complete ===" -ForegroundColor Green
Write-Host "Run 'dotnet build' to build all projects" -ForegroundColor Cyan
Write-Host "Run 'src/Tonome.Demo' to see the desktop demo" -ForegroundColor Cyan
