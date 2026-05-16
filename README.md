# Norvix WorkFlow Hub

Norvix WorkFlow Hub is a professional B2B workflow platform that demonstrates how Norvix AS can connect existing systems, automate data and document workflows, support AI-assisted administration with human review, and provide operational visibility for Norwegian organizations.

Norwegian subtitle:

> Fra e-post og skjema til sak, dokumentasjon, fakturagrunnlag og rapportering - uten dobbeltregistrering.

## Demo Scenario

The demo customer is the fictional company **Agder Drift & Service AS**, a Norwegian technical services company with 45 employees. The company already uses Microsoft 365, SharePoint, Outlook, Excel, an accounting/project system in the Tripletex/PowerOffice/Fiken category, and Power BI.

The problem is not lack of digital tools. The problem is manual work between systems: copying customer data, moving attachments, tracking case status in spreadsheets, preparing delivery packages manually, and producing reports after the fact.

## MVP Goal

The MVP supports one complete flow:

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

## Current Status

The backend workflow foundation is implemented with tenant-scoped APIs, PostgreSQL persistence, audit events, local dev auth, mock AI, mock integrations, document workflow, delivery links, analytics, and automated tests.

Frontend integration is in progress. The dashboard, intake inbox, intake creation, AI review actions, case conversion, case list/detail, and integration management pages are connected to existing backend APIs. Document workflow and delivery package UI remain the next implementation phases.

Backend/API capabilities currently implemented:

- Tenant-scoped local dev auth, RBAC, audit events, and tenant isolation tests.
- Intake inbox with manual/mock-source creation and validation.
- AI review workflow with mock provider, stored analysis runs, review tasks, approval and rejection.
- Case workspace with tasks, notes, linked documents, and activity.
- Brreg organization lookup and customer enrichment.
- Document upload, file validation, versioning, AI classification, human approval, and case linking.
- Integration dashboard with Brreg, Microsoft Graph/SharePoint mock, Tripletex-style mock, Power BI/Fabric mock, sync history, failure and retry.
- Delivery packages with selected documents, PDF summary placeholder, secure expiring public link, revoke, and access log.
- Analytics endpoints with overview metrics, status groupings, CSV export, and JSON export.
- Portfolio documentation, architecture diagram, demo script, and screenshot workflow.

Current verification baseline: backend integration/unit/contract tests, frontend lint/build, npm audit, Docker Compose config, and manual file-size checks.

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

- [Product Brief](docs/product-brief.md)
- [Requirements](docs/requirements.md)
- [Architecture](docs/architecture.md)
- [Data Model](docs/data-model.md)
- [API Contract Draft](docs/api-contract.md)
- [Implementation Roadmap](docs/implementation-roadmap.md)
- [Functional Implementation Plan](docs/functional-implementation-plan.md)
- [Security and Privacy](docs/security-and-privacy.md)
- [Norway Legal Checklist](docs/legal-checklist-norway.md)
- [DPIA Screening](docs/dpia-screening.md)
- [Testing Strategy](docs/testing-strategy.md)
- [Coding Standards](docs/coding-standards.md)
- [Demo Script](docs/demo-script.md)
- [Portfolio Summary](docs/portfolio-summary.md)
- [Final Acceptance Status](docs/final-acceptance-status.md)
- [Architecture Diagram](docs/architecture-diagram.md)
- [Screenshots](docs/screenshots.md)
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

## Portfolio Demo

Recommended 5-minute demo path:

1. Show the operational dashboard and integration status.
2. Create/list an intake.
3. Run AI analysis and approve suggestions.
4. Convert intake to case.
5. Enrich customer data through Brreg.
6. Upload and classify a document.
7. Link the document to the case.
8. Create a delivery package and public link.
9. Open the public delivery link and show access logging.
10. Export metrics as CSV/JSON.
