# Current Implementation Status

This file records the current technical status. It is not a final acceptance statement for public deployment.

## Implemented Foundation

- Monorepo, local dependencies, health endpoints, CI skeleton, and README setup.
- Tenant/auth/audit foundation with RBAC and tenant isolation tests.
- Intake inbox API with validation and tenant isolation tests.
- AI review queue using provider abstraction, stored AI runs, human approval/rejection, and negative tests.
- Case workspace API with conversion, tasks, notes, activity, and cross-tenant tests.
- Brreg lookup and customer enrichment APIs.
- Document workflow with upload validation, metadata, versioning, classification, approval, case linking, and negative tests.
- Integration dashboard with adapters, sync runs, failed sync retry, RBAC, and failure-mode tests.
- Delivery package with selected documents, summary document record, expiring token link, public delivery page, access logging, revoke, and negative tests.
- Analytics with dashboard metrics, CSV export, JSON export, and tenant-scoped tests.
- Product documentation, walkthrough, architecture diagram, and screenshot instructions.

## Current Target

The next target is a public interactive demo for Norvix AS.

The demo should let a website visitor start an isolated temporary workspace with fictional data, complete the main workflow, and have all demo data expire automatically.

The active implementation plan is:

- [Public Demo Implementation Plan - Draft](public-demo-implementation-plan-draft.md)

## Current Gaps Before Public Demo

- Public demo session model is not yet implemented.
- `POST /api/demo-sessions` is not yet implemented.
- Demo bearer-token auth is not yet implemented.
- Local development auth headers must be rejected outside Development.
- Demo tenant cloning or per-session seeding is not yet implemented.
- Expired demo session cleanup is not yet implemented.
- Frontend `/demo` start page is not yet implemented.
- Global public demo banner is not yet implemented.
- Public demo privacy and terms pages are not yet implemented.
- Production-grade PDF rendering is not yet implemented.
- Public deployment hardening still needs to be completed.

## Local-Only Or Mock-Backed Components

- Local development auth is used instead of public demo session auth or Microsoft Entra ID.
- Microsoft Graph / SharePoint integration is mocked.
- Tripletex/accounting integration is mocked.
- Power BI/Fabric export status is mocked while CSV/JSON export is functional.
- AI provider is mocked and suggestion-only.
- PDF summary is represented as a generated summary document record, not a production PDF rendering engine.
- Local file storage must be replaced or configured as durable object storage before real production use.

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
- Docker Compose config validation.
- Manual file-size check for hand-written files.
- Full public demo smoke test once demo session mode exists.

## Validated Locally

Record exact date and command output summaries here only after validation commands have been run in the current environment.
