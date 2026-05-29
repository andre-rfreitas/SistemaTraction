# Compila e inicia a API (alternativa ao dotnet run que trava no Windows)

# Matar processos dotnet anteriores para liberar a porta
Write-Host "Encerrando processos dotnet anteriores..." -ForegroundColor Yellow
Get-Process -Name dotnet,SistemaTraction* -ErrorAction SilentlyContinue | Stop-Process -Force
Start-Sleep -Seconds 1

# Garantir que o LocalDB (UNI1500) está rodando
Write-Host "Verificando LocalDB (UNI1500)..." -ForegroundColor Yellow
$state = (sqllocaldb info UNI1500 | Select-String "State:").ToString().Trim()
if ($state -notmatch "Running") {
    Write-Host "Iniciando instancia LocalDB UNI1500..." -ForegroundColor Yellow
    sqllocaldb start UNI1500 | Out-Null
    Start-Sleep -Seconds 3
}
Write-Host "LocalDB OK" -ForegroundColor Green

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
