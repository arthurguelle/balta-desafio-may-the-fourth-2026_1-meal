param(
    [string]$ApiProject = "api/Meal.Api",
    [string]$FrontendProject = "frontend/Meal.Frontend"
)

$root = Split-Path -Parent $PSScriptRoot
Set-Location $root

Write-Host "[meal] Validando Ollama..." -ForegroundColor Cyan
$ollamaInstalled = $null -ne (Get-Command ollama -ErrorAction SilentlyContinue)
if (-not $ollamaInstalled) {
    Write-Warning "Ollama nao encontrado. Instale em https://ollama.com/download"
}
else {
    try {
        $null = ollama list
        Write-Host "[meal] Ollama disponivel." -ForegroundColor Green
    }
    catch {
        Write-Warning "Ollama instalado, mas nao respondeu. Inicie o servico e tente novamente."
    }
}

Write-Host "[meal] Iniciando API em nova janela..." -ForegroundColor Cyan
Start-Process powershell -ArgumentList @(
    "-NoExit",
    "-Command",
    "Set-Location '$root'; dotnet run --project '$ApiProject'"
)

Start-Sleep -Seconds 1

Write-Host "[meal] Iniciando Frontend em nova janela..." -ForegroundColor Cyan
Start-Process powershell -ArgumentList @(
    "-NoExit",
    "-Command",
    "Set-Location '$root'; dotnet run --project '$FrontendProject'"
)

Write-Host "[meal] Ambiente iniciado." -ForegroundColor Green
Write-Host "API: http://localhost:5230" -ForegroundColor Yellow
Write-Host "Frontend: http://localhost:5282" -ForegroundColor Yellow
