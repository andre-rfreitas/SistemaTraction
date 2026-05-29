# Compila e inicia a API (alternativa ao dotnet run que trava no Windows)

# Matar processos dotnet anteriores para liberar a porta
Write-Host "Encerrando processos dotnet anteriores..." -ForegroundColor Yellow
Get-Process -Name dotnet,SistemaTraction* -ErrorAction SilentlyContinue | Stop-Process -Force
Start-Sleep -Seconds 1

Set-Location "$PSScriptRoot\backend"

Write-Host "Compilando API..." -ForegroundColor Cyan
dotnet build src/API -v q
if ($LASTEXITCODE -ne 0) { Write-Host "Build falhou." -ForegroundColor Red; exit 1 }

$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:ASPNETCORE_URLS = "http://localhost:5000"

Write-Host ""
Write-Host "Iniciando API em http://localhost:5000 ..." -ForegroundColor Green
Write-Host "Scalar UI: http://localhost:5000/scalar/v1" -ForegroundColor Green
Write-Host ""

# ContentRoot deve apontar para src/API onde está o appsettings.json
Set-Location "$PSScriptRoot\backend\src\API"
dotnet "$PSScriptRoot\backend\src\API\bin\Debug\net9.0\SistemaTraction.API.dll"
