# Demo Environment

The current target is documented in:

- [Demo Azure Deployment](../../../../docs/deployment-demo-azure.md)

Terraform still needs to be added for:

- Azure Container Registry;
- Azure Container Apps environment;
- API, worker, and frontend Container Apps;
- Azure Database for PostgreSQL Flexible Server;
- Azure Blob Storage;
- Application Insights;
- role assignments and managed identities.

Until Terraform is added, use the bootstrap scripts:

```powershell
.\scripts\provision-demo-azure.ps1 -SubscriptionId "<subscription-id>" -TenantId "<tenant-id>"
.\scripts\configure-github-demo-environment.ps1
```
