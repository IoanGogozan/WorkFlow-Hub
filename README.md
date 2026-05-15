# Norvix WorkFlow Hub

Norvix WorkFlow Hub is a professional B2B workflow platform that demonstrates how Norvix AS can connect existing systems, automate data and document workflows, support AI-assisted administration with human review, and provide operational visibility for Norwegian organizations.

Norwegian subtitle:

> Fra e-post og skjema til sak, dokumentasjon, fakturagrunnlag og rapportering - uten dobbeltregistrering.

## Demo Scenario

The demo customer is the fictional company **Agder Drift & Service AS**, a Norwegian technical services company with 45 employees. The company already uses Microsoft 365, SharePoint, Outlook, Excel, an accounting/project system in the Tripletex/PowerOffice/Fiken category, and Power BI.

The problem is not lack of digital tools. The problem is manual work between systems: copying customer data, moving attachments, tracking case status in spreadsheets, preparing delivery packages manually, and producing reports after the fact.

## MVP Goal

The MVP must support one complete flow:

1. A request is received from manual entry, mock email, mock form, or API.
2. The request appears in the Intake Inbox.
3. AI proposes customer, organization number, category, urgency, tasks, summary, missing information, and document metadata.
4. A user approves, edits, or rejects AI suggestions.
5. The system creates a case/project workspace.
6. Customer data is enriched from Bronnoysundregistrene / Enhetsregisteret.
7. Documents are uploaded, versioned, classified, and approved.
8. Missing information is shown.
9. A PDF delivery summary is generated.
10. A secure expiring delivery link is created.
11. Audit events are recorded.
12. The dashboard shows operational status, bottlenecks, and exportable metrics.

## Technology Direction

- Frontend: Next.js App Router, TypeScript, Tailwind CSS, shadcn/ui or Radix UI, React Hook Form, Zod, MSAL.js, Playwright.
- Backend: ASP.NET Core / .NET 10, C#, Entity Framework Core, PostgreSQL, OpenAPI, xUnit, Testcontainers, Serilog, OpenTelemetry.
- Storage: Azure Blob Storage compatible storage, Azurite locally.
- Local dependencies: Docker Compose with PostgreSQL, Azurite, Mailpit, optional Seq.
- Cloud: Azure App Service or Azure Container Apps, Azure Database for PostgreSQL Flexible Server, Blob Storage, Key Vault, Application Insights, Service Bus or Storage Queue.
- Infrastructure: Terraform and GitHub Actions.

## Documentation Index

- [Product Brief](docs/product-brief.md)
- [Requirements](docs/requirements.md)
- [Architecture](docs/architecture.md)
- [Data Model](docs/data-model.md)
- [API Contract Draft](docs/api-contract.md)
- [Implementation Roadmap](docs/implementation-roadmap.md)
- [Security and Privacy](docs/security-and-privacy.md)
- [Norway Legal Checklist](docs/legal-checklist-norway.md)
- [DPIA Screening](docs/dpia-screening.md)
- [Testing Strategy](docs/testing-strategy.md)
- [Coding Standards](docs/coding-standards.md)
- [Demo Script](docs/demo-script.md)
- [Backlog](docs/backlog.md)
- [References](docs/references.md)

## Non-Goals for MVP

- Full CRM or ERP.
- Real invoice issuing.
- Full SharePoint migration.
- Autonomous AI decisions.
- Public SaaS billing.
- Real production credentials.
- Complex multi-region deployment.

## Implementation Rule

Build the system as if it may later be used by real Norwegian customers:

- Use fake demo data only.
- Keep all business records tenant-scoped.
- Add tenant isolation tests early.
- Mock integrations first.
- Treat AI output as suggestions only.
- Require human approval before final case, document, delivery, or external action changes.
- Keep audit logs for important actions.
- Add serious automated tests for every module, including negative tests for invalid input, forbidden actions, cross-tenant access, expired/revoked links, integration failures, AI failures, and unsafe uploads.
- Keep code files small and modular. Follow the file size limits in [Coding Standards](docs/coding-standards.md).

## GitHub Repository

Target repository:

https://github.com/IoanGogozan/WorkFlow-Hub
