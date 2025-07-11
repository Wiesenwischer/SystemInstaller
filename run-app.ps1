cd d:\proj\SystemInstaller
Write-Host "Building application..."
dotnet build
if ($LASTEXITCODE -eq 0) {
    Write-Host "Build successful. Starting application..."
    dotnet run --urls="http://localhost:5000"
} else {
    Write-Host "Build failed!"
}
