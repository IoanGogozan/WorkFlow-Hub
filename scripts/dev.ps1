$ErrorActionPreference = "Stop"

$root = Resolve-Path (Join-Path $PSScriptRoot "..")
Set-Location $root

function Assert-PortAvailable {
    param([int[]] $Ports)

    $listeners = Get-NetTCPConnection -LocalPort $Ports -State Listen -ErrorAction SilentlyContinue
    if ($listeners) {
        $details = $listeners |
            Select-Object LocalPort, OwningProcess |
            Sort-Object LocalPort |
            Format-Table -AutoSize |
            Out-String
        throw "Required dev port is already in use:`n$details"
    }
}

Assert-PortAvailable -Ports @(3000, 5000)

Write-Host "Starting local dependencies with Docker Compose..."
docker compose up -d

if (-not (Test-Path (Join-Path $root "frontend/node_modules"))) {
    Write-Host "Installing frontend dependencies..."
    npm --prefix frontend install
}

$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:ASPNETCORE_URLS = "http://localhost:5000"
$env:MSBUILDDISABLENODEREUSE = "1"

Write-Host ""
Write-Host "Starting Norvix WorkFlow Hub..."
Write-Host "Backend:  http://localhost:5000"
Write-Host "Frontend: http://localhost:3000"
Write-Host "Mailpit:  http://localhost:8025"
Write-Host ""
Write-Host "Press Ctrl+C to stop backend and frontend. Docker services stay running."
Write-Host ""

$jobs = @()

try {
    $jobs += Start-Job -Name "NorvixHub.Api" -ScriptBlock {
        param([string] $Root)

        Set-Location $Root
        $env:ASPNETCORE_ENVIRONMENT = "Development"
        $env:ASPNETCORE_URLS = "http://localhost:5000"
        $env:MSBUILDDISABLENODEREUSE = "1"
        dotnet run --project backend/src/NorvixHub.Api -nr:false
    } -ArgumentList $root.Path

    $jobs += Start-Job -Name "NorvixHub.Frontend" -ScriptBlock {
        param([string] $FrontendRoot)

        Set-Location $FrontendRoot
        npm run dev -- -p 3000
    } -ArgumentList (Join-Path $root "frontend")

    while (($jobs | Where-Object { $_.State -eq "Running" }).Count -eq $jobs.Count) {
        foreach ($job in $jobs) {
            Receive-Job -Job $job
        }
        Start-Sleep -Seconds 1
    }

    foreach ($job in $jobs) {
        Receive-Job -Job $job
    }
}
finally {
    Write-Host ""
    Write-Host "Stopping dev processes..."
    foreach ($job in $jobs) {
        Stop-Job -Job $job -ErrorAction SilentlyContinue
        Remove-Job -Job $job -Force -ErrorAction SilentlyContinue
    }
}
