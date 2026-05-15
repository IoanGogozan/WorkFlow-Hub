# Backlog

## MVP Backlog

### Foundation

- Create monorepo structure.
- Add backend solution and projects.
- Add frontend Next.js app.
- Add Docker Compose with PostgreSQL, Azurite, Mailpit.
- Add health endpoints.
- Add CI skeleton.
- Add CI file-size check for hand-written source files.
- Add configuration templates.

### Tenant, Auth, Audit

- Add tenant, user, membership tables.
- Add seed data for Agder Drift & Service AS.
- Add local dev auth stub.
- Add tenant context service.
- Add RBAC service.
- Add audit event service.
- Add tenant isolation tests.

### Intake

- Create intake item.
- List intake items.
- View intake item.
- Add attachments.
- Add mock email intake.
- Add mock form intake.
- Add intake audit events.

### AI Review

- Define AI suggestion schema.
- Add mock AI provider.
- Store AI analysis runs.
- Add review task queue.
- Approve/edit/reject suggestions.
- Audit review decisions.

### Case Workspace

- Convert intake to case.
- Case overview.
- Case tasks.
- Case notes.
- Case documents.
- Customer tab.
- Activity tab.
- Missing information checklist.

### Brreg

- Add Brreg adapter interface.
- Add real Brreg lookup.
- Search by organization number.
- Search by company name.
- Enrich customer data.
- Handle API failures.

### Documents

- Upload document.
- Store blob in Azurite.
- Store metadata in PostgreSQL.
- Add document versions.
- Add classification suggestion.
- Approve classification.
- Link document to case.
- Add document library filters.

### Integrations

- Add integration connection model.
- Add sync run model.
- Add connector status UI.
- Add mock Microsoft Graph adapter.
- Add mock accounting adapter.
- Add mock Power BI/Fabric export adapter.
- Add retry failed sync.

### Delivery

- Create delivery package.
- Select package documents.
- Generate PDF summary.
- Create expiring delivery link.
- Add external delivery page.
- Add delivery access log.
- Add revoke link.

### Analytics

- Add dashboard metrics.
- Add CSV export.
- Add JSON export.
- Add estimated manual work avoided metric.

### Polish

- Add architecture diagram.
- Add screenshots.
- Finalize README.
- Validate demo script.
- Run accessibility pass.

## Post-MVP Backlog

- Microsoft Graph real SharePoint integration.
- Tripletex test API integration.
- Azure AI Document Intelligence provider.
- Azure OpenAI provider.
- Power Platform custom connector.
- Fabric Lakehouse export.
- Production malware scanning.
- Advanced retention/deletion workflows.
- Backup and restore automation.
- Real Entra ID app registration guide.
- Terraform cloud environment.
- Application Insights dashboards.
