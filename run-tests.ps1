# Run All Tests with Coverage Report
# This script runs unit tests, integration tests, generates coverage reports, and optionally runs E2E tests

param(
    [switch]$SkipE2E,
    [switch]$OpenReport
)

Write-Host "================================" -ForegroundColor Cyan
Write-Host "PoTicTac Test Suite Runner" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan
Write-Host ""

# Step 1: Run Unit & Integration Tests with Coverage
Write-Host "Step 1: Running Unit & Integration Tests with Coverage..." -ForegroundColor Yellow
dotnet test PoTicTac.sln `
    /p:CollectCoverage=true `
    /p:CoverletOutputFormat=cobertura `
    /p:CoverletOutput=./coverage/ `
    --nologo `
    --verbosity quiet

if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ Tests failed!" -ForegroundColor Red
    exit 1
}

Write-Host "✓ All unit & integration tests passed!" -ForegroundColor Green
Write-Host ""

# Step 2: Generate Coverage Report
Write-Host "Step 2: Generating HTML Coverage Report..." -ForegroundColor Yellow
reportgenerator `
    -reports:"**\coverage.cobertura.xml" `
    -targetdir:"coverage\report" `
    -reporttypes:"Html;Badges;TextSummary"

if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ Coverage report generation failed!" -ForegroundColor Red
    exit 1
}

Write-Host "✓ Coverage report generated at: coverage\report\index.html" -ForegroundColor Green
Write-Host ""

# Display coverage summary
if (Test-Path "coverage\report\Summary.txt") {
    Write-Host "Coverage Summary:" -ForegroundColor Cyan
    Get-Content "coverage\report\Summary.txt" | Write-Host
    Write-Host ""
}

# Step 3: Run E2E Tests (optional)
if (-not $SkipE2E) {
    Write-Host "Step 3: Running E2E Tests with Playwright..." -ForegroundColor Yellow
    Push-Location "tests\PoTicTac.E2ETests"
    
    # Check if node_modules exists
    if (-not (Test-Path "node_modules")) {
        Write-Host "Installing E2E test dependencies..." -ForegroundColor Yellow
        npm install
    }
    
    npm test
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "✗ E2E tests failed!" -ForegroundColor Red
        Pop-Location
        exit 1
    }
    
    Write-Host "✓ All E2E tests passed!" -ForegroundColor Green
    Pop-Location
} else {
    Write-Host "Step 3: Skipping E2E tests (use without -SkipE2E to run)" -ForegroundColor Gray
}

Write-Host ""
Write-Host "================================" -ForegroundColor Cyan
Write-Host "✓ All Tests Complete!" -ForegroundColor Green
Write-Host "================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Reports:" -ForegroundColor Cyan
Write-Host "  - Coverage: coverage\report\index.html" -ForegroundColor White
if (-not $SkipE2E) {
    Write-Host "  - E2E: tests\PoTicTac.E2ETests\playwright-report\index.html" -ForegroundColor White
}
Write-Host ""

# Open coverage report if requested
if ($OpenReport) {
    Write-Host "Opening coverage report..." -ForegroundColor Yellow
    Start-Process "coverage\report\index.html"
}
