# PowerShell script to convert all Mermaid diagrams to SVG
# Requires: Node.js and npm installed
# Install Mermaid CLI: npm install -g @mermaid-js/mermaid-cli

param(
    [switch]$InstallCLI = $false,
    [switch]$Help = $false
)

$ErrorActionPreference = "Stop"

function Show-Help {
    Write-Host @"
PoTicTac Diagram Converter
==========================

Converts all Mermaid (.mmd) diagrams to SVG format.

Usage:
    .\convert-diagrams.ps1              # Convert all diagrams
    .\convert-diagrams.ps1 -InstallCLI  # Install Mermaid CLI first, then convert
    .\convert-diagrams.ps1 -Help        # Show this help message

Prerequisites:
    - Node.js and npm must be installed
    - Mermaid CLI (@mermaid-js/mermaid-cli)

To install Node.js:
    Download from: https://nodejs.org/

To install Mermaid CLI:
    npm install -g @mermaid-js/mermaid-cli

"@
    exit 0
}

if ($Help) {
    Show-Help
}

Write-Host "PoTicTac Diagram Converter" -ForegroundColor Cyan
Write-Host "=========================" -ForegroundColor Cyan
Write-Host ""

# Check if npm is available
try {
    $npmVersion = npm --version 2>$null
    if ($LASTEXITCODE -ne 0) { throw }
    Write-Host "✓ npm found (version: $npmVersion)" -ForegroundColor Green
}
catch {
    Write-Host "✗ npm not found" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please install Node.js from: https://nodejs.org/" -ForegroundColor Yellow
    Write-Host "After installation, restart your terminal and run this script again." -ForegroundColor Yellow
    exit 1
}

# Install Mermaid CLI if requested
if ($InstallCLI) {
    Write-Host ""
    Write-Host "Installing Mermaid CLI..." -ForegroundColor Yellow
    npm install -g @mermaid-js/mermaid-cli
    if ($LASTEXITCODE -ne 0) {
        Write-Host "✗ Failed to install Mermaid CLI" -ForegroundColor Red
        exit 1
    }
    Write-Host "✓ Mermaid CLI installed successfully" -ForegroundColor Green
}

# Check if mmdc is available
try {
    $mmdcVersion = mmdc --version 2>$null
    if ($LASTEXITCODE -ne 0) { throw }
    Write-Host "✓ Mermaid CLI found (version: $mmdcVersion)" -ForegroundColor Green
}
catch {
    Write-Host "✗ Mermaid CLI not found" -ForegroundColor Red
    Write-Host ""
    Write-Host "Install it by running:" -ForegroundColor Yellow
    Write-Host "  npm install -g @mermaid-js/mermaid-cli" -ForegroundColor White
    Write-Host ""
    Write-Host "Or run this script with -InstallCLI flag:" -ForegroundColor Yellow
    Write-Host "  .\convert-diagrams.ps1 -InstallCLI" -ForegroundColor White
    exit 1
}

Write-Host ""
Write-Host "Converting Mermaid diagrams to SVG..." -ForegroundColor Yellow
Write-Host ""

$diagramsPath = Join-Path $PSScriptRoot "."
$mmdFiles = Get-ChildItem -Path $diagramsPath -Filter "*.mmd"

if ($mmdFiles.Count -eq 0) {
    Write-Host "✗ No .mmd files found in $diagramsPath" -ForegroundColor Red
    exit 1
}

$successCount = 0
$failCount = 0

foreach ($file in $mmdFiles) {
    $inputFile = $file.FullName
    $outputFile = $inputFile -replace '\.mmd$', '.svg'
    $fileName = $file.Name
    
    Write-Host "Converting: $fileName" -ForegroundColor Cyan
    
    try {
        mmdc -i $inputFile -o $outputFile -b transparent
        
        if ($LASTEXITCODE -eq 0 -and (Test-Path $outputFile)) {
            Write-Host "  ✓ Created: $($file.BaseName).svg" -ForegroundColor Green
            $successCount++
        }
        else {
            throw "mmdc returned error code $LASTEXITCODE"
        }
    }
    catch {
        Write-Host "  ✗ Failed to convert $fileName" -ForegroundColor Red
        Write-Host "  Error: $_" -ForegroundColor Red
        $failCount++
    }
}

Write-Host ""
Write-Host "Conversion Summary:" -ForegroundColor Cyan
Write-Host "  Total files: $($mmdFiles.Count)" -ForegroundColor White
Write-Host "  Successful: $successCount" -ForegroundColor Green
Write-Host "  Failed: $failCount" -ForegroundColor $(if ($failCount -gt 0) { "Red" } else { "White" })

if ($successCount -gt 0) {
    Write-Host ""
    Write-Host "SVG files created in: $diagramsPath" -ForegroundColor Green
}

if ($failCount -gt 0) {
    exit 1
}

exit 0
