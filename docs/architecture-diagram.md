# Architecture Diagram

```mermaid
flowchart LR
    User[Internal user] --> Frontend[Next.js frontend]
    External[External recipient] --> DeliveryPublic[Public delivery link]

    Frontend --> Api[ASP.NET Core API]
    DeliveryPublic --> Api

    Api --> Auth[Local dev auth now\nMicrosoft Entra ID target]
    Api --> Tenant[Tenant context + RBAC]
    Api --> Db[(PostgreSQL)]
    Api --> Storage[Blob-compatible storage\nLocal storage/Azurite now]
    Api --> Audit[Audit events]

    Api --> Intake[Intake Inbox]
    Api --> Ai[AI review queue\nMock provider now]
    Api --> Cases[Case Workspace]
    Api --> Documents[Document Workflow]
    Api --> Integrations[Integration Dashboard]
    Api --> Delivery[Delivery Package]
    Api --> Metrics[Analytics + exports]

    Integrations --> Brreg[Brreg API]
    Integrations --> Graph[Microsoft Graph mock]
    Integrations --> Accounting[Tripletex-style mock]
    Integrations --> Fabric[Power BI/Fabric mock]

    Worker[Worker service] -. future async jobs .-> Ai
    Worker -. future async jobs .-> Documents
    Worker -. future async jobs .-> Integrations
    Worker -. future async jobs .-> Delivery
```

## Request Flow

1. Requests enter through manual entry, mock email/form, or API.
2. The API resolves tenant context from the authenticated user membership.
3. Every business query is scoped to the current tenant.
4. AI and integration providers operate behind adapter interfaces.
5. User approvals convert AI suggestions into final data.
6. Delivery links use random tokens; only token hashes are stored.
7. Public delivery access is logged and exposes only selected package items.
8. Analytics endpoints aggregate tenant-scoped operational data and export CSV/JSON.

## Deployment Target

```mermaid
flowchart TB
    Github[GitHub Actions] --> Azure[Azure deployment]
    Azure --> AppService[Azure App Service\nor Container Apps]
    Azure --> Pg[Azure Database for PostgreSQL]
    Azure --> Blob[Azure Blob Storage]
    Azure --> KeyVault[Azure Key Vault]
    Azure --> Insights[Application Insights]
```
