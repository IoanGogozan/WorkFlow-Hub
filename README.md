# Norvix WorkFlow Hub

Norvix WorkFlow Hub is a B2B workflow platform for Norwegian organizations that need to connect intake, company lookup, case handling, document control, AI-assisted review, secure delivery, audit logs, and operational reporting.

Norwegian subtitle:

> Fra e-post og skjema til sak, dokumentasjon, fakturagrunnlag og rapportering - uten dobbeltregistrering.

## Product Scenario

The reference customer profile is a Norwegian technical services company that already uses Microsoft 365, SharePoint, Outlook, Excel, an accounting/project system in the Tripletex/PowerOffice/Fiken category, and Power BI.

The problem is not lack of digital tools. The problem is manual work between systems: copying customer data, moving attachments, tracking case status in spreadsheets, preparing delivery packages manually, and producing reports after the fact.

## Product Goal

The application is intended to support one complete operational flow:

1. A request is received from manual entry, email/form adapters, or API.
2. The request appears in the Intake Inbox.
3. AI proposes customer, organization number, category, urgency, tasks, summary, missing information, and document metadata.
4. A user approves, edits, or rejects AI suggestions.
5. The system creates a case/project workspace.
6. Customer data is enriched from Bronnoysundregistrene / Enhetsregisteret.
7. Documents are uploaded, versioned, classified, and approved.
8. Missing information is shown before delivery.
9. A delivery summary is generated.
10. A secure expiring delivery link is created.
11. Audit events are recorded.
12. The dashboard shows operational status, bottlenecks, and exportable metrics.

## Current Product Status

Norvix WorkFlow Hub currently has a working local product flow backed by ASP.NET Core APIs, PostgreSQL persistence, tenant-scoped data access, audit events, a Next.js frontend, document workflow, delivery links, analytics, and automated backend coverage.

The active target is a public interactive demo for Norvix AS. The demo should let each website visitor start an isolated temporary workspace with fictional data, complete the full workflow, and have demo data expire automatically.

Implemented capabilities:

- Tenant-scoped local development auth, RBAC, audit events, and tenant isolation tests.
- Intake inbox with manual/source-based creation and validation.
- AI review workflow with stored analysis runs, review tasks, human approval, and rejection.
- Case workspace with conversion from intake, tasks, notes, linked documents, and activity.
- Brreg organization lookup and customer enrichment APIs.
- Document upload, validation, versioning, classification, human approval, and case linking.
- Integration dashboard with connection state, sync history, failure, and retry flows.
- Delivery packages with selected documents, generated summary record, secure expiring public link, revoke, public page, and access log.
- Analytics endpoints with overview metrics, status groupings, CSV export, and JSON export.
- Frontend pages for dashboard, intakes, cases, documents, delivery packages, public delivery links, and integrations.

Local/development-only components still to replace before public demo or real customer production deployment:

- Public demo session auth is not yet implemented.
- Header-based local dev auth must be replaced with Microsoft Entra ID / OIDC.
- AI provider is currently a mock adapter and must be replaced with a governed real provider before processing real customer data.
- Microsoft Graph/SharePoint, Tripletex/accounting, and Power BI/Fabric adapters are currently mock adapters.
- File storage is local-development oriented and must move to Azure Blob Storage or equivalent durable object storage.
- Delivery summary currently creates a summary document record; production PDF rendering still needs implementation.
- Seed/reference data is fictional and must not be mixed with real customer data.

## Deployment Direction

The target deployment architecture is:

- Frontend: Next.js App Router, TypeScript, Tailwind CSS.
- Backend: ASP.NET Core / .NET 10, C#, Entity Framework Core, PostgreSQL, OpenAPI, xUnit.
- Storage: Azure Blob Storage compatible storage, Azurite locally.
- Local dependencies: Docker Compose with PostgreSQL, Azurite, Mailpit, optional Seq.
- Cloud: Azure App Service or Azure Container Apps, Azure Database for PostgreSQL Flexible Server, Blob Storage, Key Vault, Application Insights, Service Bus or Storage Queue.
- Infrastructure: Terraform and GitHub Actions.

## Verification

Verification targets:

- Backend integration, unit, and contract tests.
- Frontend lint and production build.
- `npm audit`.
- Docker Compose config validation.
- Manual review for file size and architecture boundaries.

Record a command under "Validated locally" only after it has been run in the current environment.

## Local Setup

Prerequisites:

- Node.js 24 or newer;
- npm 11 or newer;
- Docker Desktop with Docker Compose;
- .NET 10 SDK, or Docker for backend validation.

Start local dependencies:

```bash
docker compose up -d
```

PostgreSQL is exposed on host port `55432` to avoid collisions with other local projects.

Start the full local app with one command from the repository root:

```powershell
npm run dev
```

This starts Docker Compose dependencies, the ASP.NET Core API on `http://localhost:5000`, and the Next.js frontend on `http://localhost:3000`.

Run frontend checks:

```bash
cd frontend
npm ci
npm run lint
npm run build
```

Run backend tests with local .NET SDK:

```bash
dotnet test backend/NorvixHub.sln --configuration Release -nr:false
```

The `-nr:false` flag disables MSBuild node reuse. This avoids intermittent `Child node exited prematurely` failures on Windows when IDE build hosts or stale MSBuild nodes are active, while still allowing normal project-level parallelism.

Restore local .NET tools before creating EF migrations:

```bash
dotnet tool restore
dotnet tool run dotnet-ef migrations add MigrationName --project backend/src/NorvixHub.Infrastructure --startup-project backend/src/NorvixHub.Api --output-dir Persistence/Migrations
```

Run backend tests through Docker when .NET is not installed locally:

```bash
docker run --rm -v "${PWD}:/src" -w /src mcr.microsoft.com/dotnet/sdk:10.0 dotnet test backend/NorvixHub.sln --configuration Release
```

On Windows, when running integration tests through the .NET SDK container, use the compose network and the already running Postgres container:

```powershell
$env:NORVIXHUB_TEST_POSTGRES="Host=norvixhub-postgres;Port=5432;Database=norvixhub_tests;Username=norvixhub;Password=norvixhub_dev_password"
docker run --rm --network workflow-hub_default -e NORVIXHUB_TEST_POSTGRES=$env:NORVIXHUB_TEST_POSTGRES -v "${PWD}:/src" -w /src mcr.microsoft.com/dotnet/sdk:10.0 dotnet test backend/NorvixHub.sln --configuration Release
```

API health endpoint:

```http
GET /health
GET /health/ready
```

Local dev auth for `/api/*` endpoints uses headers:

```http
X-Norvix-Tenant-Id: 11111111-1111-4111-8111-111111111111
X-Norvix-User-Id: 22222222-2222-4222-8222-222222222222
```

## Documentation Index

- [Public Demo Implementation Plan - Draft](docs/public-demo-implementation-plan-draft.md)
- [Current Implementation Status](docs/current-implementation-status.md)
- [Product Brief](docs/product-brief.md)
- [Requirements](docs/requirements.md)
- [Architecture](docs/architecture.md)
- [Data Model](docs/data-model.md)
- [API Contract Draft](docs/api-contract.md)
- [Security and Privacy](docs/security-and-privacy.md)
- [Norway Legal Checklist](docs/legal-checklist-norway.md)
- [DPIA Screening](docs/dpia-screening.md)
- [Testing Strategy](docs/testing-strategy.md)
- [Coding Standards](docs/coding-standards.md)
- [Product Walkthrough](docs/product-walkthrough.md)
- [Portfolio Summary](docs/portfolio-summary.md)
- [Architecture Diagram](docs/architecture-diagram.md)
- [Screenshots](docs/screenshots.md)
- [References](docs/references.md)

## Non-Goals for First Production Release

- Full CRM or ERP replacement.
- Real invoice issuing.
- Full SharePoint migration.
- Autonomous AI decisions.
- Public SaaS billing.
- Unreviewed AI writes to external systems.
- Multi-region enterprise deployment.

## Implementation Rule

Build the system as a deployable product for Norwegian customers:

- Keep all business records tenant-scoped.
- For public demo mode, derive tenant context from the demo session token, not from client-provided tenant headers.
- Replace local-only adapters before real customer production use.
- Use fictional seed data only in development and staging.
- Treat AI output as suggestions only.
- Require human approval before final case, document, delivery, or external action changes.
- Keep audit logs for important actions.
- Add automated tests for every module, including negative tests for invalid input, forbidden actions, cross-tenant access, expired/revoked links, integration failures, AI failures, and unsafe uploads.
- Keep code files small and modular. Follow the file size limits in [Coding Standards](docs/coding-standards.md).

## GitHub Repository

Target repository:

https://github.com/IoanGogozan/WorkFlow-Hub

## Product Walkthrough

Recommended 5-minute product walkthrough path:

1. Start a demo workspace from `/demo`.
2. Show the operational dashboard and integration status.
3. Create/list an intake.
4. Run AI analysis and approve suggestions.
5. Convert intake to case.
6. Enrich customer data through Brreg.
7. Select or attach a demo-safe document and classify it.
8. Link the document to the case.
9. Create a delivery package and public link.
10. Open the public delivery link and show access logging.
11. Export metrics as CSV/JSON.
