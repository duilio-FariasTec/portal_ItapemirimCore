param(
    [string]$DbPath = "",
    [switch]$ResetDatabase,
    [switch]$Rebuild
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
if ([string]::IsNullOrWhiteSpace($DbPath)) {
    $DbPath = Join-Path $repoRoot "src\Itapemirim.Ani.Api\ani.db"
}
else {
    $DbPath = [System.IO.Path]::GetFullPath($DbPath)
}

$apiProj = Join-Path $repoRoot "src\Itapemirim.Ani.Api\Itapemirim.Ani.Api.csproj"
$seedOutDir = Join-Path $repoRoot "src\Itapemirim.Ani.Api\bin\Seeder\net10.0"
$seedDll = Join-Path $seedOutDir "Itapemirim.Ani.Api.dll"

if ($Rebuild -or -not (Test-Path $seedDll)) {
    Write-Host "[INFO] Compilando binários do Seeder Offline..." -ForegroundColor Gray
    & dotnet build $apiProj --no-restore --disable-build-servers -p:UseSharedCompilation=false -m:1 -o $seedOutDir -v quiet /nologo
    if ($LASTEXITCODE -ne 0) {
        throw "Falha ao preparar os binários do seeder."
    }
}

$seedArgs = @("--seed-only")
if ($ResetDatabase) {
    $seedArgs += "--reset-db"
}

$env:ConnectionStrings__DefaultConnection = "Data Source=$DbPath"
${env:Logging__LogLevel__Microsoft.EntityFrameworkCore} = "Warning"

Push-Location (Join-Path $repoRoot "src\Itapemirim.Ani.Api")
try {
    & dotnet exec $seedDll @seedArgs
}
finally {
    Pop-Location
}