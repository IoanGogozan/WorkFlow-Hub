param(
    [Parameter(Mandatory = $true)]
    [string] $SubscriptionId,

    [Parameter(Mandatory = $true)]
    [string] $TenantId,

    [string] $Location = "norwayeast",
    [string] $ResourceGroup = "rg-norvix-workflow-demo",
    [string] $NamePrefix = "norvixhubdemo",
    [string] $PostgresAdminUser = "norvixhub_admin",
    [string] $PostgresDatabase = "norvixhub",
    [string] $GitHubRepository = "IoanGogozan/WorkFlow-Hub",
    [string] $DemoApiBaseUrl = "",
    [string] $DemoFrontendUrl = "",
    [switch] $SkipGitHubFederatedIdentity
)

$ErrorActionPreference = "Stop"

function Assert-Command {
    param([string] $Name)

    if (-not (Get-Command $Name -ErrorAction SilentlyContinue)) {
        throw "$Name CLI is required. Install it before running this script."
    }
}

function New-RandomName {
    param(
        [string] $Prefix,
        [int] $MaxLength
    )

    $suffix = -join ((48..57) + (97..122) | Get-Random -Count 6 | ForEach-Object { [char] $_ })
    $name = ($Prefix.ToLowerInvariant() -replace "[^a-z0-9]", "") + $suffix
    if ($name.Length -gt $MaxLength) {
        return $name.Substring(0, $MaxLength)
    }

    return $name
}

Assert-Command "az"

$root = Resolve-Path (Join-Path $PSScriptRoot "..")
$envFile = Join-Path $root ".env.demo.local"

az account set --subscription $SubscriptionId

$acrName = New-RandomName -Prefix "${NamePrefix}acr" -MaxLength 50
$storageName = New-RandomName -Prefix "${NamePrefix}st" -MaxLength 24
$postgresName = New-RandomName -Prefix "${NamePrefix}pg" -MaxLength 63
$logAnalyticsName = "$NamePrefix-law"
$containerAppsEnvironment = "$NamePrefix-cae"
$apiApp = "$NamePrefix-api"
$workerApp = "$NamePrefix-worker"
$frontendApp = "$NamePrefix-frontend"
$blobContainer = "documents"
$postgresPassword = [Convert]::ToBase64String([System.Security.Cryptography.RandomNumberGenerator]::GetBytes(24))

Write-Host "Creating resource group $ResourceGroup in $Location..."
az group create --name $ResourceGroup --location $Location | Out-Null

Write-Host "Creating Azure Container Registry $acrName..."
az acr create `
    --resource-group $ResourceGroup `
    --name $acrName `
    --sku Basic `
    --admin-enabled false | Out-Null

$acrLoginServer = az acr show `
    --resource-group $ResourceGroup `
    --name $acrName `
    --query loginServer `
    --output tsv

Write-Host "Creating storage account $storageName..."
az storage account create `
    --resource-group $ResourceGroup `
    --name $storageName `
    --location $Location `
    --sku Standard_LRS `
    --kind StorageV2 `
    --allow-blob-public-access false `
    --min-tls-version TLS1_2 | Out-Null

$blobConnectionString = az storage account show-connection-string `
    --resource-group $ResourceGroup `
    --name $storageName `
    --query connectionString `
    --output tsv

az storage container create `
    --name $blobContainer `
    --connection-string $blobConnectionString `
    --public-access off | Out-Null

Write-Host "Creating PostgreSQL Flexible Server $postgresName..."
az postgres flexible-server create `
    --resource-group $ResourceGroup `
    --name $postgresName `
    --location $Location `
    --admin-user $PostgresAdminUser `
    --admin-password $postgresPassword `
    --database-name $PostgresDatabase `
    --sku-name Standard_B1ms `
    --tier Burstable `
    --storage-size 32 `
    --version 18 `
    --public-access 0.0.0.0 | Out-Null

az postgres flexible-server firewall-rule create `
    --resource-group $ResourceGroup `
    --name $postgresName `
    --rule-name AllowAzureServices `
    --start-ip-address 0.0.0.0 `
    --end-ip-address 0.0.0.0 | Out-Null

$postgresConnectionString = "Host=$postgresName.postgres.database.azure.com;Port=5432;Database=$PostgresDatabase;Username=$PostgresAdminUser;Password=$postgresPassword;Ssl Mode=Require;Trust Server Certificate=true"

Write-Host "Creating Container Apps environment $containerAppsEnvironment..."
az monitor log-analytics workspace create `
    --resource-group $ResourceGroup `
    --workspace-name $logAnalyticsName `
    --location $Location | Out-Null

$workspaceId = az monitor log-analytics workspace show `
    --resource-group $ResourceGroup `
    --workspace-name $logAnalyticsName `
    --query customerId `
    --output tsv

$workspaceKey = az monitor log-analytics workspace get-shared-keys `
    --resource-group $ResourceGroup `
    --workspace-name $logAnalyticsName `
    --query primarySharedKey `
    --output tsv

az containerapp env create `
    --resource-group $ResourceGroup `
    --name $containerAppsEnvironment `
    --location $Location `
    --logs-workspace-id $workspaceId `
    --logs-workspace-key $workspaceKey | Out-Null

$placeholderImage = "mcr.microsoft.com/azuredocs/containerapps-helloworld:latest"

Write-Host "Creating API Container App $apiApp..."
az containerapp create `
    --resource-group $ResourceGroup `
    --name $apiApp `
    --environment $containerAppsEnvironment `
    --image $placeholderImage `
    --target-port 8080 `
    --ingress external `
    --min-replicas 1 `
    --max-replicas 2 `
    --system-assigned | Out-Null

Write-Host "Creating worker Container App $workerApp..."
az containerapp create `
    --resource-group $ResourceGroup `
    --name $workerApp `
    --environment $containerAppsEnvironment `
    --image $placeholderImage `
    --ingress disabled `
    --min-replicas 1 `
    --max-replicas 1 `
    --system-assigned | Out-Null

Write-Host "Creating frontend Container App $frontendApp..."
az containerapp create `
    --resource-group $ResourceGroup `
    --name $frontendApp `
    --environment $containerAppsEnvironment `
    --image $placeholderImage `
    --target-port 3000 `
    --ingress external `
    --min-replicas 1 `
    --max-replicas 2 `
    --system-assigned | Out-Null

$acrId = az acr show --resource-group $ResourceGroup --name $acrName --query id --output tsv
foreach ($app in @($apiApp, $workerApp, $frontendApp)) {
    $principalId = az containerapp show `
        --resource-group $ResourceGroup `
        --name $app `
        --query identity.principalId `
        --output tsv

    az role assignment create `
        --assignee $principalId `
        --role AcrPull `
        --scope $acrId | Out-Null

    az containerapp registry set `
        --resource-group $ResourceGroup `
        --name $app `
        --server $acrLoginServer `
        --identity system | Out-Null
}

if (-not $DemoApiBaseUrl) {
    $apiFqdn = az containerapp show --resource-group $ResourceGroup --name $apiApp --query properties.configuration.ingress.fqdn --output tsv
    $DemoApiBaseUrl = "https://$apiFqdn"
}

if (-not $DemoFrontendUrl) {
    $frontendFqdn = az containerapp show --resource-group $ResourceGroup --name $frontendApp --query properties.configuration.ingress.fqdn --output tsv
    $DemoFrontendUrl = "https://$frontendFqdn"
}

$githubClientId = ""
if (-not $SkipGitHubFederatedIdentity) {
    Write-Host "Creating GitHub OIDC app registration..."
    $appRegistration = az ad app create --display-name "$NamePrefix-github-deploy" | ConvertFrom-Json
    $githubClientId = $appRegistration.appId
    az ad sp create --id $githubClientId | Out-Null

    az role assignment create `
        --assignee $githubClientId `
        --role Contributor `
        --scope "/subscriptions/$SubscriptionId/resourceGroups/$ResourceGroup" | Out-Null

    $federatedCredential = @{
        name = "github-main-demo"
        issuer = "https://token.actions.githubusercontent.com"
        subject = "repo:$GitHubRepository`:environment:demo"
        audiences = @("api://AzureADTokenExchange")
    } | ConvertTo-Json -Depth 5

    $tempCredentialPath = Join-Path ([System.IO.Path]::GetTempPath()) "norvixhub-github-federated-credential.json"
    Set-Content -Path $tempCredentialPath -Value $federatedCredential -Encoding utf8
    az ad app federated-credential create --id $githubClientId --parameters $tempCredentialPath | Out-Null
    Remove-Item -LiteralPath $tempCredentialPath -Force
}

@"
AZURE_RESOURCE_GROUP=$ResourceGroup
AZURE_CONTAINER_REGISTRY_NAME=$acrName
AZURE_CONTAINER_REGISTRY_LOGIN_SERVER=$acrLoginServer
AZURE_API_CONTAINER_APP=$apiApp
AZURE_WORKER_CONTAINER_APP=$workerApp
AZURE_FRONTEND_CONTAINER_APP=$frontendApp
DEMO_API_BASE_URL=$DemoApiBaseUrl
DEMO_FRONTEND_URL=$DemoFrontendUrl
DEMO_BLOB_CONTAINER=$blobContainer
AZURE_CLIENT_ID=$githubClientId
AZURE_TENANT_ID=$TenantId
AZURE_SUBSCRIPTION_ID=$SubscriptionId
DEMO_POSTGRES_CONNECTION_STRING=$postgresConnectionString
DEMO_BLOB_CONNECTION_STRING=$blobConnectionString
"@ | Set-Content -Path $envFile -Encoding utf8

Write-Host ""
Write-Host "Provisioning complete."
Write-Host "Configuration was written to $envFile. This file is ignored by git."
Write-Host "Next: run scripts/configure-github-demo-environment.ps1 to populate the GitHub demo environment."
