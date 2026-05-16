# Portfolio Summary

## Project

Norvix WorkFlow Hub is a working B2B workflow platform for a fictional Norwegian customer, Agder Drift & Service AS.

It demonstrates how Norvix AS can connect existing systems and reduce manual work between intake, company lookup, case handling, document workflow, AI-assisted review, integrations, secure delivery, audit logs, and analytics.

## Current MVP Status

Implemented phases:

- Phase 0: monorepo, frontend/backend scaffolding, Docker Compose, CI, health endpoint.
- Phase 1: tenant/auth foundation, local dev auth, RBAC, audit writer.
- Phase 2: intake inbox.
- Phase 3: AI review with mock provider and human approval.
- Phase 4: case workspace with tasks, notes, activity.
- Phase 5: Brreg customer enrichment.
- Phase 6: document workflow with upload, versioning, classification, approval, case linking.
- Phase 7: integration dashboard with mock connectors, sync runs, failure and retry.
- Phase 8: delivery package with expiring link, revoke, public page, access log.
- Phase 9: analytics dashboard endpoints and CSV/JSON export.
- Phase 10: portfolio polish, final documentation, architecture diagram, screenshot workflow.

## Demo Value

The demo shows practical automation for organizations that already use Microsoft 365, SharePoint-style document libraries, accounting/project systems, and dashboards, but still move data manually.

The core value is not one narrow vertical feature. It is a credible integration and workflow pattern:

- turn incoming requests into structured cases;
- enrich customer data from Norwegian registers;
- keep AI assistive and reviewable;
- make documents versioned and approved;
- deliver selected documents through controlled links;
- expose audit and operational metrics.

## Published Demo Boundaries

Use only fictional data.

Do not configure production credentials.

External integrations are mocked except Brreg lookup support.

Delivery links are functional but use local document metadata and mock PDF summary behavior in the MVP.

AI is a mock provider and must remain suggestion-only until a real provider, prompt governance, and data processing terms are configured.

## Test Posture

The project has positive and negative automated tests across the main security and workflow boundaries:

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

## Next Production Steps

Before using this with a real customer:

- replace local dev auth with Microsoft Entra ID;
- configure Azure Blob Storage and malware scanning for uploads;
- configure Key Vault and production secrets;
- add real Microsoft Graph, Tripletex/PowerOffice/Fiken, and Fabric/Power BI adapters one at a time;
- complete DPA/subprocessor documentation;
- complete DPIA screening with the customer;
- add production observability, alerting, backup/restore, and incident response runbooks.
