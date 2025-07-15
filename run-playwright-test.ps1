Write-Host "Starting Playwright test setup..." -ForegroundColor Green

# Check if we're in the right directory
$currentDir = Get-Location
Write-Host "Current directory: $currentDir" -ForegroundColor Yellow

# Check if Node.js is available
try {
    $nodeVersion = node --version
    Write-Host "Node.js version: $nodeVersion" -ForegroundColor Green
} catch {
    Write-Host "Node.js not found!" -ForegroundColor Red
    exit 1
}

# Check if npm is available
try {
    $npmVersion = npm --version
    Write-Host "npm version: $npmVersion" -ForegroundColor Green
} catch {
    Write-Host "npm not found!" -ForegroundColor Red
    exit 1
}

# Install Playwright
Write-Host "Installing Playwright..." -ForegroundColor Yellow
npm install playwright

# Install browser
Write-Host "Installing Playwright browsers..." -ForegroundColor Yellow
npx playwright install chromium

# Run the test
Write-Host "Running Playwright test..." -ForegroundColor Green
node test-browser.js

Write-Host "Script completed!" -ForegroundColor Green
