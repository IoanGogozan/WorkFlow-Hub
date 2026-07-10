# Product Overview

## Product Direction

Norvix WorkFlow Hub is a technical integration platform presented publicly as
**Automatisert serviceflyt**, a focused client-facing integration case study.
It demonstrates how Norvix can connect systems a company already uses and reduce
manual administration between email, company data, case handling, documents,
delivery preparation, reporting, and audit history.

The public experience is not positioned as a generic workflow product or a
commercial multi-company SaaS. The broader application remains available as
technical evidence behind one simple service-request story.

The approved direction and staged implementation plan are maintained in
[Client-Facing Integration Demo](client-facing-integration-demo.md).

## Target Visitor

The primary visitor is a decision-maker in a Norwegian technical or service
company with approximately 20–250 employees. Typical organizations already use
Outlook, Microsoft 365, SharePoint, an accounting/project/service system, and
Excel or Power BI, but employees still copy information between them manually.

Reference data is fictional and represents a Norwegian technical service
company such as Agder Drift & Service AS.

## Public Scenario

The demo tells one story:

1. A service request arrives by email with attachments.
2. Customer, reference, category, and missing information are identified.
3. Company data is checked.
4. A case is created.
5. Documents are stored, classified, and linked to the case.
6. Reporting and delivery preparation are updated.
7. Important actions are traceable.

The visitor should conclude that Norvix can integrate existing systems without
requiring a company to replace its entire software landscape.

## Current Technical Foundation

The repository contains:

- an ASP.NET Core backend and Next.js/TypeScript frontend;
- PostgreSQL persistence and tenant-scoped data access;
- isolated temporary public-demo sessions with fictional seed data;
- intake, AI-assisted review, case, document, integration, delivery, analytics,
  and audit capabilities;
- Brreg-capable organization handling;
- document versioning, classification, approval, and case linking;
- delivery packages, generated demo PDF summaries, secure expiring links, and
  access history;
- Azure Blob Storage support and local storage adapters;
- automated backend and Playwright coverage;
- CI, container, and Azure deployment preparation.

Detailed and dated implementation facts belong in
[Current Implementation Status](current-implementation-status.md).

## Integration Boundaries

Public integration claims use three explicit modes:

- **Implemented** — performed and stored by the current application.
- **Public-data capable** — a public-source capability exists, while the demo
  uses a deterministic stored demo snapshot for reliability.
- **Demo adapter** — the contract, status, retry, and payload flow are shown
  without sending data to a real customer system.

Current mock or demo-backed areas include AI, Microsoft Graph/SharePoint,
accounting/project integration, and Power BI/Fabric status. Brreg is
public-data capable, while the public story uses stored fictional evidence.
CSV/JSON export and internal document, delivery, and audit behavior are
implemented. The UI and documentation must not present a demo adapter as a live
customer integration.

## Safety and Deployment Boundaries

- Use fictional data only in the public demo.
- Do not upload personal, confidential, or real customer material.
- Public demo sessions expire and are cleaned up.
- Arbitrary public upload remains disabled; generated sample documents are used.
- Local development authentication is not production authentication.
- Real customer use requires identity, secrets, storage, malware scanning,
  governed integrations and AI, legal agreements, observability, backup, and
  operational runbooks appropriate to the deployment.

See [Security and Privacy](security-and-privacy.md),
[Demo Azure Deployment](deployment-demo-azure.md), and
[DPIA Screening](dpia-screening.md) for the durable details.

## Evidence and Claims

AI remains optional and suggestion-only, with human approval before state-changing
actions. Example time-savings calculations must use editable assumptions and
must always explain that actual impact needs to be measured in a limited pilot.
No fixed estimate may be presented as a measured customer result without evidence
and permission.

## Later Production Work

Before processing real customer data:

- replace local development auth with Microsoft Entra ID/OIDC;
- configure production secrets and durable object storage;
- introduce real integration adapters one at a time with least privilege;
- add malware scanning for real uploads;
- govern any real AI provider and its data handling;
- complete customer-specific DPA/DPIA and subprocessor work;
- implement production observability, alerting, backup/restore, and incident
  response procedures.
