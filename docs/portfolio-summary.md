# Portfolio Summary

## Project

Norvix WorkFlow Hub is a B2B workflow platform for Norwegian organizations that need structured intake, company lookup, case handling, document workflow, AI-assisted review, integrations, secure delivery, audit logs, and analytics.

The current product direction is a public interactive demo for Norvix AS. The demo should be available from the Norvix website, create an isolated temporary workspace for each visitor, use fictional data, and expire automatically.

## Current Product Status

Implemented foundation:

- Monorepo, frontend/backend scaffolding, Docker Compose, CI, and health endpoint.
- Tenant/auth foundation, local dev auth, RBAC, audit writer, and tenant isolation tests.
- Intake inbox.
- AI review with provider abstraction and human approval.
- Case workspace with tasks, notes, and aggregated workflow activity.
- Brreg customer enrichment.
- Document workflow with centralized upload size/type validation, versioning, classification, approval, and case linking.
- Integration dashboard with connector state, sync runs, failure, and retry.
- Delivery package with simple generated PDF summary, expiring link, revoke, public page, and access log.
- Analytics dashboard endpoints and CSV/JSON export.
- Product documentation, architecture diagram, and screenshot workflow.

Active plan:

- [Public Demo Implementation Plan - Draft](public-demo-implementation-plan-draft.md)

## Product Value

The product targets organizations that already use Microsoft 365, SharePoint-style document libraries, accounting/project systems, and dashboards, but still move data manually.

The core value is a credible integration and workflow pattern:

- turn incoming requests into structured cases;
- enrich customer data from Norwegian registers;
- keep AI assistive and reviewable;
- make documents versioned and approved;
- deliver selected documents through controlled links;
- expose audit and operational metrics.
- show the end-to-end audit trail for a case, from intake through public delivery access.

## Current Deployment Boundaries

Use only fictional data until production hardening and customer legal work are complete.

Do not configure production credentials in the repository.

Current local-only or mock-backed areas:

- Public demo session sandboxing is implemented for temporary tenant/user creation and bearer-token auth.
- Local development auth remains available only for Development; Microsoft Entra ID is still planned for real production.
- Expired demo session cleanup removes database records and stored local files.
- Frontend demo labels identify Mock AI, mock Microsoft/accounting/Fabric integrations, and Brreg real-capable behavior.
- Public privacy and terms pages are implemented and linked from the demo.
- Rate limiting is enabled for demo session creation and public delivery endpoints.
- Global request body size limits and upload file size/type limits are configured and tested.
- Security headers and clean non-Development error responses are enabled and tested.
- Reverse-proxy readiness is configured with forwarded headers, optional HTTPS enforcement, and HSTS.
- GitHub Actions CI includes backend tests, EF migration drift check, frontend dependency audit/lint/build, and Docker Compose validation.
- Demo deploy workflow is gated by branch/tag rules, fictional-data confirmation, validation jobs, and the `demo` environment, then pushes ACR images and updates Azure Container Apps.
- Azure Blob Storage adapter is available for shared public demo document storage.
- Microsoft Graph / SharePoint integration is mocked.
- Tripletex/accounting integration is mocked.
- Power BI/Fabric export status is mocked while CSV/JSON export is functional.
- AI provider is mocked and suggestion-only.
- Delivery summary uses a simple generated demo PDF; production PDF rendering is still required.
- File storage must use Azure Blob Storage or equivalent durable object storage before public demo deployment.

## Test Posture

The project has automated tests across the main security and workflow boundaries:

- tenant isolation;
- auth/RBAC;
- intake validation;
- AI review gating;
- case access;
- Brreg/customer enrichment;
- document upload validation;
- cross-tenant document/case blocking;
- integration sync failure and retry;
- delivery token invalid/expired/revoked behavior;
- public delivery access logging;
- analytics exports.

## Public Demo Work

Before linking the demo from the Norvix website:

- keep session expiry and cleanup enabled in the deployed worker;
- continue public endpoint hardening beyond the initial rate limits;
- keep the end-to-end browser flow passing after each Phase B change;
- keep arbitrary upload disabled in public demo and use generated sample documents;
- wire the deploy workflow to Azure infrastructure once the target environment is defined.
- add Terraform or another repeatable provisioning path for the Azure resources.

## Later Production Release Work

Before using this with real customer data:

- replace local dev auth with Microsoft Entra ID;
- configure Azure Blob Storage and malware scanning for uploads;
- configure Key Vault and production secrets;
- add real Microsoft Graph, Tripletex/PowerOffice/Fiken, and Fabric/Power BI adapters one at a time;
- replace mock AI with a governed provider and documented prompt/data handling;
- add production PDF rendering;
- complete DPA/subprocessor documentation;
- complete DPIA screening with the customer;
- add production observability, alerting, backup/restore, and incident response runbooks.
