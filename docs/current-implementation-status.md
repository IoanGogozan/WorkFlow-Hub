# Current Implementation Status

This file records the current technical status. It is not a final acceptance statement for public deployment.

## Implemented Foundation

- Monorepo, local dependencies, health endpoints, CI skeleton, and README setup.
- Tenant/auth/audit foundation with RBAC and tenant isolation tests.
- Intake inbox API with validation and tenant isolation tests.
- AI review queue using provider abstraction, stored AI runs, human approval/rejection, and negative tests.
- Case workspace API with conversion, tasks, notes, aggregated workflow activity, and cross-tenant tests.
- Brreg lookup and customer enrichment APIs.
- Document workflow with centralized upload size/type validation, metadata, versioning, classification, approval, case linking, and negative tests.
- Integration dashboard with adapters, sync runs, failed sync retry, RBAC, and failure-mode tests.
- Delivery package with selected documents, simple generated PDF summary, expiring token link, public delivery page, access logging, revoke, and negative tests.
- Analytics with dashboard metrics, CSV export, JSON export, and tenant-scoped tests.
- Demo session model, migration, creation endpoint, bearer-token auth, and non-Development rejection of local dev headers.
- Rich fictional per-session demo seeding now includes intakes, a customer, case workspace, task, note, approved document, delivery package, generated summary document, integrations, and audit trail.
- Frontend `/demo` start page, demo token storage, API client bearer-token support, and public demo banner.
- Read-only, tenant-scoped `/api/demo-story` projection for the simplified
  client experience, with cross-tenant, missing-story, authentication, and
  response-header coverage.
- Client-facing `/` experience with incoming request, manual-process comparison,
  replayable evidence timeline, real demo outcome, editable calculator,
  integration boundaries, technical evidence, and CTA.
- `/automation` compatibility redirect, `/technical` technical overview, and
  `/summary` consolidation redirect to `/#resultat`.
- Separate client-facing and technical Playwright paths, responsive checks at
  375/768/1280 px, keyboard focus checks, and reduced-motion support.
- Worker-backed expired demo session cleanup for demo tenants, tenant-scoped records, and stored local files.
- Explicit demo session isolation tests and public demo upload blocking.
- Demo-safe sample document endpoint and frontend action for public demo document workflow.
- Case activity now aggregates related intake, AI review, document, delivery package, and public delivery access events for the visible demo audit trail.
- Frontend demo labels identify Mock AI, mock accounting, mock Microsoft/SharePoint,
  mock Fabric/Power BI, and Brreg public-data-capable status backed by stored demo evidence.
- Public `/privacy` and `/terms` pages exist and are linked from the demo start page, app shell, and public delivery page.
- Rate limiting is enabled for `POST /api/demo-sessions` and public delivery endpoints.
- Global request body size limits and upload file size/type limits are configured and tested.
- Frontend document upload copy is aligned with backend limits: PDF/PNG/JPG/JPEG and 5 MB.
- Correlation IDs are emitted on responses, accepted from `X-Correlation-ID`, included in clean error responses, added to logging scope, and written to audit events.
- Security headers and non-Development exception handling are enabled and tested so public errors do not expose stack traces.
- Forwarded headers, optional HTTPS enforcement, and HSTS are configured for reverse-proxy deployment readiness and tested.
- GitHub Actions CI validates backend tests, EF migration drift, frontend dependency audit/lint/build, and Docker Compose configuration.
- Demo deploy workflow has a main/tag gate, fictional-data confirmation, validation jobs, a `demo` environment gate, ACR image publishing, and Azure Container Apps update steps.
- Azure Blob Storage adapter is implemented for shared deployed document storage and idempotent cleanup, with Azurite integration coverage for expired demo cleanup.
- Bootstrap scripts exist for first-pass Azure demo resource provisioning and GitHub `demo` environment configuration.
- Consolidated product documentation, client and technical walkthroughs,
  architecture diagrams, and screenshot instructions.

## Current Public Direction

The implemented public direction is a client-facing integration case study for Norvix AS. It
shows one fictional service request moving from email through validation, case
creation, document handling, reporting/delivery preparation, and audit evidence.
The existing detailed application remains available as technical evidence.

The approved direction and staged implementation plan are:

- [Client-Facing Integration Demo](client-facing-integration-demo.md)

## Current Gaps Before Public Deployment

- Production-grade PDF rendering is not yet implemented; the current demo generates a simple PDF summary.
- Azure resources have not been created because public demo deployment is intentionally deferred until an Azure subscription is available and costs are approved.
- Terraform provisioning is not yet implemented in the repository; the current provisioning path is a bootstrap PowerShell script.

## Local-Only Or Mock-Backed Components

- Local development uses header auth for technical API work. The public demo
  path uses isolated bearer-token demo sessions; Microsoft Entra ID is not implemented.
- Microsoft Graph / SharePoint integration is mocked.
- Tripletex/accounting integration is mocked.
- Power BI/Fabric export status is mocked while CSV/JSON export is functional.
- AI provider is mocked and suggestion-only.
- PDF summary is generated as a simple demo PDF and stored as a document record, not by a production PDF rendering engine.
- Local file storage must be replaced or configured as durable object storage before real production use.
- Azure Blob Storage can be selected with `Storage:Provider=AzureBlob` for the public demo.
- Arbitrary public upload is disabled in Demo/Public; the public demo uses generated sample documents.

## Required Before Real Customer Production

The public demo is not the same as a real customer SaaS deployment. Before processing real customer data, the product still needs:

- Microsoft Entra ID / OIDC authentication and MSAL frontend integration;
- Azure Blob Storage or equivalent durable object storage;
- malware scanning for real uploads;
- Key Vault and production secret management;
- real integration adapters introduced one at a time;
- governed real AI provider integration and documented prompt/data handling;
- production PDF rendering;
- DPA, DPIA screening, and subprocessor documentation;
- production observability, alerting, backup/restore, and incident response runbooks;
- structured security and accessibility review.

## Verification Targets

- Backend integration, unit, and contract tests.
- Frontend `npm audit`, `npm run lint`, and `npm run build`.
- EF migration drift check.
- Docker Compose config validation.
- Manual file-size check for hand-written files.
- Full client-facing and technical demo smoke tests.

## Validated Locally

Record exact date and command output summaries here only after validation commands have been run in the current environment.

2026-07-11 — real live integration demo V2 pre-change baseline:

- `npm --prefix frontend ci` — passed; 361 packages installed. npm reported
  1 low and 1 moderate advisory for the complete dependency tree.
- `npm --prefix frontend run lint` — passed with no ESLint errors.
- `npm --prefix frontend run build` — passed with Next.js 16.2.6; all 15
  routes compiled successfully. Node emitted a `DEP0205` deprecation warning
  during the build, but it did not affect the result.
- `npm --prefix frontend audit --omit=dev --audit-level=high` — passed with
  0 production vulnerabilities at the requested threshold.
- `dotnet test backend/NorvixHub.sln --configuration Release -nr:false` —
  passed: 103 tests total (1 contract, 2 unit, 100 integration), 0 failed and
  0 skipped.
- `dotnet tool restore --tool-manifest dotnet-tools.json` — passed; restored
  `dotnet-ef` 10.0.0.
- `dotnet tool run dotnet-ef -- migrations has-pending-model-changes ...` —
  passed; no model changes since the last migration.
- `docker compose config --quiet` — passed with no output.

2026-07-10 — client-facing integration demo documentation gate:

- `git diff --check` — passed; Git emitted only informational LF/CRLF warnings.
- `npm --prefix frontend run lint` — passed with no ESLint errors.
- `npm --prefix frontend run build` — passed with Next.js 16.2.6; all public,
  compatibility, technical, and detailed routes compiled successfully.
- `dotnet test backend/NorvixHub.sln --configuration Release -nr:false` — passed
  103 tests: 1 contract, 2 unit, and 100 integration; 0 failed and 0 skipped.

2026-07-10 — final regression gate:

- `npm --prefix frontend ci` — passed; 361 packages installed. The complete
  dependency tree reported 1 low and 1 moderate advisory.
- `npm --prefix frontend run lint` and `npm --prefix frontend run build` — passed.
- `npm --prefix frontend audit --omit=dev --audit-level=high` — passed with
  0 production vulnerabilities.
- `dotnet test backend/NorvixHub.sln --configuration Release -nr:false` — passed
  103 tests with 0 failed and 0 skipped.
- `dotnet tool restore --tool-manifest dotnet-tools.json` — passed.
- EF `migrations has-pending-model-changes` — passed; no model drift.
- `docker compose config --quiet` — passed.
- `npm run test:e2e:public-demo` — passed both client-facing and technical
  Chromium scenarios.
- Manual release checklist was cross-checked against the E2E and integration
  coverage for session creation/expiry, routing, replay, calculator, technical
  evidence, legal routes, clean errors, blocked public upload, fictional data,
  honest integration labels, and mobile overflow.

2026-07-10 — pre-redesign baseline on the existing working tree:

- `npm --prefix frontend ci` — passed; installed 361 packages and audited 362.
  npm reported 2 known dependency vulnerabilities: 1 low and 1 moderate. No
  dependency fix or upgrade was performed during baseline capture.
- `npm --prefix frontend run lint` — passed with no ESLint errors.
- `npm --prefix frontend run build` — passed with Next.js 16.2.6; TypeScript,
  static generation, and production optimization completed. Node emitted a
  non-blocking `DEP0205` deprecation warning for `module.register()`.
- `dotnet test backend/NorvixHub.sln --configuration Release -nr:false` — passed
  99 tests: 1 contract, 2 unit, and 96 integration; 0 failed and 0 skipped.
- `docker compose config --quiet` — passed.

2026-05-17:

- `dotnet test backend\NorvixHub.sln --configuration Release -nr:false` - passed 95 tests.
- `dotnet tool run dotnet-ef -- migrations has-pending-model-changes --project backend/src/NorvixHub.Infrastructure/NorvixHub.Infrastructure.csproj --startup-project backend/src/NorvixHub.Api/NorvixHub.Api.csproj --configuration Release --verbose` - build succeeded; no pending model changes.
- `npm --prefix frontend run lint` - passed.
- `npm --prefix frontend run build` - passed with Next.js standalone output enabled.
- `npm --prefix frontend audit --omit=dev --audit-level=high` - found 0 vulnerabilities.
- `docker compose config --quiet` - passed.
- `docker build -f backend/src/NorvixHub.Api/Dockerfile -t norvixhub-api:local .` - passed.
- `docker build -f backend/src/NorvixHub.Worker/Dockerfile -t norvixhub-worker:local .` - passed.
- `docker build -f frontend/Dockerfile --build-arg NEXT_PUBLIC_API_BASE_URL=http://localhost:5000 -t norvixhub-frontend:local .` - passed.
- `az account show` - not run successfully; Azure CLI is not installed on this workstation.
- `gh auth status` - not run successfully; GitHub CLI is not installed on this workstation.

2026-05-22:

- `dotnet test backend\NorvixHub.sln --configuration Release -nr:false` - passed 99 tests.
- `dotnet tool run dotnet-ef -- migrations has-pending-model-changes --project backend/src/NorvixHub.Infrastructure/NorvixHub.Infrastructure.csproj --startup-project backend/src/NorvixHub.Api/NorvixHub.Api.csproj --configuration Release` - build succeeded; no pending model changes.
- `npm --prefix frontend run lint` - passed.
- `npm --prefix frontend run build` - passed.
- `npm --prefix frontend audit --omit=dev --audit-level=high` - found 0 vulnerabilities.
- `docker compose config --quiet` - passed.
- `npm run test:e2e:public-demo` - Playwright public demo smoke test passed.
- Rich per-session demo seed data for case/document/delivery/audit story was added and covered by demo session integration tests.
