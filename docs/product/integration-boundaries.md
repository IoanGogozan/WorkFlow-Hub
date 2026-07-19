# Integration Boundaries

This document is the source of truth for public integration claims. UI copy, the README, screenshots, and presentations must remain consistent with these classifications.

## Classification Matrix

| Boundary | Implemented behavior | Public wording | Must not be claimed |
| --- | --- | --- | --- |
| WorkFlow Hub core | API and worker persist the run, case, document, PDF metadata, step state, and audit events in the tenant-scoped application model | Implemented by WorkFlow Hub | Production-ready customer deployment |
| Brreg | The worker attempts a lookup against the public Brreg service; a deterministic snapshot is used and labelled when live resolution is unavailable | Live when available; labelled fallback otherwise | Guaranteed live response |
| SharePoint | A local adapter persists simulated folders, document items, idempotency data, and operation history | Functional local SharePoint simulator | Microsoft Graph or Microsoft 365 connection |
| ERP | A separate ASP.NET Core receiver validates signed requests, persists receipts, supports idempotency, and can demonstrate a controlled first-attempt failure | Self-hosted ERP demo receiver | Tripletex, PowerOffice, Fiken, or customer ERP integration |
| Email/Outlook | The public scenario starts from a seeded fictional request with representative metadata and attachments | Fictional request source | Connected mailbox or Microsoft Graph email ingestion |
| AI | The broader technical application stores suggestions and review tasks and supports approve/reject flows | Human-controlled suggestion workflow | Autonomous classification or production model accuracy |
| Documents | The application creates document records and generates a demo PDF using project-owned storage abstractions | Implemented demo document workflow | Production document archive or malware-scanning service |
| Audit | Tenant-scoped audit records and run evidence are persisted and exposed through technical views | Implemented audit evidence | Certified compliance logging or immutable external archive |

## Capability-Driven Copy

The live environment exposes capability flags for Brreg, SharePoint simulator, ERP receiver, and controlled failure behavior. Public UI must derive claims from those flags where availability can differ by environment.

If a capability is disabled, the UI should omit or clearly mark the corresponding step. It must not imply that an unavailable receiver or external service completed successfully.

## Evidence Rules

Every completed run should make the following inspectable where applicable:

- run identifier and duration;
- step status, attempt count, and evidence mode;
- whether Brreg used live data or fallback;
- exact case and document references;
- SharePoint simulator folder, item, and operation history;
- ERP receipt and attempt count;
- chronological audit events.

## Production Boundary

A customer deployment would additionally require production identity, customer-owned credentials, provider-specific authorization, data-processing agreements, retention decisions, monitored queues, backup and restore validation, incident handling, and acceptance testing against each real target system.
