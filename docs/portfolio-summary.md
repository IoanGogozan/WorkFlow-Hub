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
- Case workspace with tasks, notes, and activity.
- Brreg customer enrichment.
- Document workflow with upload, versioning, classification, approval, and case linking.
- Integration dashboard with connector state, sync runs, failure, and retry.
- Delivery package with expiring link, revoke, public page, and access log.
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

## Current Deployment Boundaries

Use only fictional data until production hardening and customer legal work are complete.

Do not configure production credentials in the repository.

Current local-only or mock-backed areas:

- Local development auth is active instead of public demo session auth or Microsoft Entra ID.
- Public demo session sandboxing is not yet implemented.
- Microsoft Graph / SharePoint integration is mocked.
- Tripletex/accounting integration is mocked.
- Power BI/Fabric export status is mocked while CSV/JSON export is functional.
- AI provider is mocked and suggestion-only.
- Delivery summary uses a generated summary record; production PDF rendering is still required.
- File storage must be moved to durable object storage before real production use.

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

- add demo session model and `POST /api/demo-sessions`;
- seed or clone a fictional tenant per visitor;
- add bearer-token demo auth;
- reject local dev auth headers outside Development;
- add session expiry and cleanup;
- add `/demo` start page and global demo banner;
- add privacy and terms pages for the demo;
- add rate limiting and public endpoint hardening;
- complete the end-to-end browser flow;
- add real simple PDF generation for delivery packages.

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
