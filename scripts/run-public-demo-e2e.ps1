param(
    [int] $BackendPort = 5100,
    [int] $FrontendPort = 3100
)

$ErrorActionPreference = "Stop"

$root = Resolve-Path (Join-Path $PSScriptRoot "..")
Set-Location $root

function Wait-HttpOk {
    param(
        [string] $Url,
        [int] $TimeoutSeconds = 90
    )

    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    do {
        try {
            $response = Invoke-WebRequest -Uri $Url -UseBasicParsing -TimeoutSec 5
            if ($response.StatusCode -ge 200 -and $response.StatusCode -lt 400) {
                return
            }
        }
        catch {
            Start-Sleep -Seconds 2
        }
    } while ((Get-Date) -lt $deadline)

    throw "Timed out waiting for $Url"
}

function Stop-ListenersOnPort {
    param([int] $Port)

    $processIds = Get-NetTCPConnection -LocalPort $Port -State Listen -ErrorAction SilentlyContinue |
        Select-Object -ExpandProperty OwningProcess -Unique

    foreach ($processId in $processIds) {
        if ($processId -gt 0) {
            Stop-Process -Id $processId -Force -ErrorAction SilentlyContinue
        }
    }
}

$backendUrl = "http://localhost:$BackendPort"
$frontendUrl = "http://localhost:$FrontendPort"

Stop-ListenersOnPort -Port $FrontendPort
Stop-ListenersOnPort -Port $BackendPort

Write-Host "Starting local dependencies..."
docker compose up -d

Write-Host "Installing Playwright Chromium if needed..."
npm --prefix frontend exec playwright install chromium

$jobs = @()
$succeeded = $false

try {
    $jobs += Start-Job -Name "NorvixHub.Api.Demo" -ScriptBlock {
        param([string] $Root, [int] $Port)

        Set-Location $Root
        $env:ASPNETCORE_ENVIRONMENT = "Demo"
        $env:ASPNETCORE_URLS = "http://localhost:$Port"
        $env:ConnectionStrings__Postgres = "Host=localhost;Port=55432;Database=norvixhub;Username=norvixhub;Password=norvixhub_dev_password"
        $env:Database__ApplyMigrationsOnStartup = "true"
        $env:Deployment__EnforceHttps = "false"
        $env:LiveDemo__Enabled = "true"
        $env:LiveDemo__OrganizationNumber = "999888777"
        $env:LiveDemo__WorkerPollMilliseconds = "100"
        $env:LiveDemo__RunRecoveryMinutes = "5"
        $env:Storage__Local__RootPath = Join-Path $Root "storage/e2e-documents"
        # Keep the public E2E deterministic: no external Brreg call is allowed in CI.
        $env:Brreg__BaseUrl = "http://127.0.0.1:1/"
        $env:LiveDemo__BrregFallbackEnabled = "true"
        $env:MSBUILDDISABLENODEREUSE = "1"
        dotnet run --project backend/src/NorvixHub.Api -nr:false
    } -ArgumentList $root.Path, $BackendPort

    $jobs += Start-Job -Name "NorvixHub.Frontend.E2E" -ScriptBlock {
        param([string] $FrontendRoot, [string] $ApiBaseUrl, [int] $Port)

        Set-Location $FrontendRoot
        $env:NEXT_PUBLIC_API_BASE_URL = $ApiBaseUrl
        npm run dev -- -p $Port
    } -ArgumentList (Join-Path $root "frontend"), $backendUrl, $FrontendPort

    Wait-HttpOk -Url "$backendUrl/health/ready" -TimeoutSeconds 120

    $jobs += Start-Job -Name "NorvixHub.Worker.LiveDemo" -ScriptBlock {
        param([string] $Root)

        Set-Location $Root
        $env:DOTNET_ENVIRONMENT = "Demo"
        $env:ConnectionStrings__Postgres = "Host=localhost;Port=55432;Database=norvixhub;Username=norvixhub;Password=norvixhub_dev_password"
        $env:LiveDemo__Enabled = "true"
        $env:LiveDemo__OrganizationNumber = "999888777"
        $env:LiveDemo__WorkerPollMilliseconds = "100"
        $env:LiveDemo__RunRecoveryMinutes = "5"
        $env:Storage__Local__RootPath = Join-Path $Root "storage/e2e-documents"
        # Match the API process so the worker uses the labelled local fallback.
        $env:Brreg__BaseUrl = "http://127.0.0.1:1/"
        $env:LiveDemo__BrregFallbackEnabled = "true"
        $env:MSBUILDDISABLENODEREUSE = "1"
        dotnet run --project backend/src/NorvixHub.Worker -nr:false
    } -ArgumentList $root.Path

    Wait-HttpOk -Url "$frontendUrl/demo" -TimeoutSeconds 120
    Wait-HttpOk -Url "$frontendUrl/intakes/new" -TimeoutSeconds 120

    Write-Host "Running public demo E2E smoke test..."
    $env:E2E_BASE_URL = $frontendUrl
    npm --prefix frontend run test:e2e -- --project=chromium
    if ($LASTEXITCODE -ne 0) {
        throw "Public demo E2E smoke test failed with exit code $LASTEXITCODE."
    }

    $succeeded = $true
}
finally {
    Write-Host "Stopping E2E app processes..."
    foreach ($job in $jobs) {
        if (-not $succeeded) {
            Receive-Job -Job $job -ErrorAction SilentlyContinue
        }

        Stop-Job -Job $job -ErrorAction SilentlyContinue
        Remove-Job -Job $job -Force -ErrorAction SilentlyContinue
    }

    Stop-ListenersOnPort -Port $FrontendPort
    Stop-ListenersOnPort -Port $BackendPort
}
