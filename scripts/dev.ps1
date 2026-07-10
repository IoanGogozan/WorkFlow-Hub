$ErrorActionPreference = "Stop"

$root = Resolve-Path (Join-Path $PSScriptRoot "..")
Set-Location $root

function Use-Utf8Console {
    try {
        $utf8 = [System.Text.UTF8Encoding]::new($false)
        [Console]::InputEncoding = $utf8
        [Console]::OutputEncoding = $utf8
        $global:OutputEncoding = $utf8

        if ($env:OS -eq "Windows_NT") {
            chcp.com 65001 | Out-Null
        }
    }
    catch {
        # Console encoding is best-effort; startup should continue if the host does not allow it.
    }
}

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

function Receive-DevJobOutput {
    param([System.Management.Automation.Job] $Job)

    Receive-Job -Job $Job -ErrorAction Continue
}

function Wait-BackendReady {
    param(
        [string] $Url,
        [System.Management.Automation.Job] $Job,
        [int] $TimeoutSeconds = 60
    )

    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    while ((Get-Date) -lt $deadline) {
        Receive-DevJobOutput -Job $Job

        if ($Job.State -ne "Running") {
            throw "Backend process stopped before it became ready."
        }

        try {
            $response = Invoke-WebRequest -Uri $Url -UseBasicParsing -TimeoutSec 2
            if ($response.StatusCode -ge 200 -and $response.StatusCode -lt 500) {
                return
            }
        }
        catch {
            Start-Sleep -Seconds 1
        }
    }

    throw "Backend did not become ready at $Url within $TimeoutSeconds seconds."
}

Use-Utf8Console
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

        $utf8 = [System.Text.UTF8Encoding]::new($false)
        [Console]::InputEncoding = $utf8
        [Console]::OutputEncoding = $utf8
        $global:OutputEncoding = $utf8
        Set-Location $Root
        $env:ASPNETCORE_ENVIRONMENT = "Development"
        $env:ASPNETCORE_URLS = "http://localhost:5000"
        $env:MSBUILDDISABLENODEREUSE = "1"
        dotnet run --no-restore --project backend/src/NorvixHub.Api -nr:false
    } -ArgumentList $root.Path

    $jobs += Start-Job -Name "NorvixHub.Frontend" -ScriptBlock {
        param([string] $FrontendRoot)

        $utf8 = [System.Text.UTF8Encoding]::new($false)
        [Console]::InputEncoding = $utf8
        [Console]::OutputEncoding = $utf8
        $global:OutputEncoding = $utf8
        Set-Location $FrontendRoot
        $env:PYTHONIOENCODING = "utf-8"
        $env:NODE_DISABLE_COLORS = "0"
        npm run dev -- -p 3000
    } -ArgumentList (Join-Path $root "frontend")

    Write-Host "Waiting for backend to become ready..."
    Wait-BackendReady -Url "http://localhost:5000/health" -Job $jobs[0]

    while (($jobs | Where-Object { $_.State -eq "Running" }).Count -eq $jobs.Count) {
        foreach ($job in $jobs) {
            Receive-DevJobOutput -Job $job
        }
        Start-Sleep -Seconds 1
    }

    foreach ($job in $jobs) {
        Receive-DevJobOutput -Job $job
    }

    $failedJobs = $jobs | Where-Object { $_.State -eq "Failed" }
    if ($failedJobs) {
        $details = $failedJobs |
            Select-Object Name, State |
            Format-Table -AutoSize |
            Out-String
        throw "One or more dev processes failed:`n$details"
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
