param(
    [string] $Repository = "IoanGogozan/WorkFlow-Hub",
    [string] $Environment = "demo",
    [string] $EnvFile = ".env.demo.local"
)

$ErrorActionPreference = "Stop"

if (-not (Get-Command "gh" -ErrorAction SilentlyContinue)) {
    throw "GitHub CLI is required. Install gh and run 'gh auth login' before this script."
}

$root = Resolve-Path (Join-Path $PSScriptRoot "..")
$envPath = Resolve-Path (Join-Path $root $EnvFile)

$values = @{}
Get-Content $envPath | ForEach-Object {
    if (-not $_ -or $_.TrimStart().StartsWith("#")) {
        return
    }

    $parts = $_ -split "=", 2
    if ($parts.Count -eq 2) {
        $values[$parts[0]] = $parts[1]
    }
}

$variables = @(
    "AZURE_RESOURCE_GROUP",
    "AZURE_CONTAINER_REGISTRY_NAME",
    "AZURE_CONTAINER_REGISTRY_LOGIN_SERVER",
    "AZURE_API_CONTAINER_APP",
    "AZURE_WORKER_CONTAINER_APP",
    "AZURE_FRONTEND_CONTAINER_APP",
    "DEMO_API_BASE_URL",
    "DEMO_FRONTEND_URL",
    "DEMO_BLOB_CONTAINER"
)

$secrets = @(
    "AZURE_CLIENT_ID",
    "AZURE_TENANT_ID",
    "AZURE_SUBSCRIPTION_ID",
    "DEMO_POSTGRES_CONNECTION_STRING",
    "DEMO_BLOB_CONNECTION_STRING"
)

Write-Host "Creating/updating GitHub environment '$Environment' in $Repository..."
gh api `
    --method PUT `
    "repos/$Repository/environments/$Environment" `
    --field wait_timer=0 `
    --field prevent_self_review=false | Out-Null

foreach ($name in $variables) {
    if ($values.ContainsKey($name) -and $values[$name]) {
        Write-Host "Setting variable $name..."
        gh variable set $name --repo $Repository --env $Environment --body $values[$name]
    }
}

foreach ($name in $secrets) {
    if ($values.ContainsKey($name) -and $values[$name]) {
        Write-Host "Setting secret $name..."
        gh secret set $name --repo $Repository --env $Environment --body $values[$name]
    }
}

Write-Host ""
Write-Host "GitHub demo environment updated."
Write-Host "Configure required reviewers in GitHub UI if reviewer protection is needed before first deploy."
