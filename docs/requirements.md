# Requirements

## Scope

The MVP covers one strong operational workflow from request intake to reviewed case, document package, secure delivery, audit log, and dashboard.

## Functional Modules

### Tenant and Users

- Multi-tenant by design.
- Every business record must include `tenant_id`.
- Users can belong to one or more tenants.
- Roles:
  - `TenantOwner`
  - `Admin`
  - `OperationsUser`
  - `Reviewer`
  - `Viewer`
  - `ExternalRecipient`
- Microsoft Entra ID architecture prepared.
- Local development auth stub allowed.
- Tenant isolation tests are mandatory.

### Intake Inbox

Sources:

- manual;
- mock email;
- mock form;
- API.

Statuses:

- `New`
- `AIAnalyzed`
- `NeedsReview`
- `Approved`
- `ConvertedToCase`
- `Rejected`

Features:

- create/list/view intake items;
- attach files;
- show source and status;
- show AI suggestions;
- approve, edit, or reject suggestions;
- convert approved intake to case;
- record audit events.

### AI Review Queue

AI can suggest:

- customer;
- organization number;
- category;
- urgency;
- tasks;
- document type;
- expiry date;
- summary;
- missing information.

Rules:

- AI suggestions are never final data until approved by a user.
- AI can be disabled per tenant.
- Provider, model, prompt version, output, confidence, and review status must be logged.
- Mock AI provider is default for local development.

### Case Workspace

Tabs:

- Overview;
- Tasks;
- Documents;
- Customer;
- Integrations;
- Activity.

Statuses:

- `Draft`
- `Open`
- `WaitingForCustomer`
- `WaitingForInternalReview`
- `ReadyForDelivery`
- `Delivered`
- `Closed`

Features:

- convert intake to case;
- add/edit tasks;
- add notes;
- link documents;
- show missing information;
- show integration state;
- show activity and audit events.

### Customer and Company Enrichment

Use Bronnoysundregistrene / Enhetsregisteret API.

Features:

- search by organization number;
- search by company name;
- store selected company data;
- show source and timestamp;
- refresh company data.

### Document Workflow

Features:

- upload file;
- import mocked SharePoint file;
- store binary file in Azure Blob compatible storage;
- store metadata in PostgreSQL;
- random blob names;
- display-safe original file names;
- link document to case;
- versioning;
- AI classification;
- human approval;
- expiry metadata;
- document library filters;
- audit log.

Allowed upload types:

- PDF;
- DOCX;
- XLSX;
- PNG;
- JPG/JPEG.

Document statuses:

- `Uploaded`
- `Processing`
- `AIClassified`
- `NeedsReview`
- `Approved`
- `Rejected`
- `Archived`

### Integrations Dashboard

Show:

- Brreg status;
- Microsoft Graph/SharePoint status;
- Tripletex/accounting status;
- Power BI/Fabric export status;
- last sync;
- failed syncs;
- retry button;
- connection mode: mocked or real.

### Accounting/Project Adapter

Start with a mock adapter.

Features:

- link or create mock customer;
- create mock project/order basis;
- store external IDs;
- show "fakturagrunnlag ready";
- never issue real invoices in MVP.

### Delivery Package

Features:

- generate PDF summary;
- select documents;
- create secure external link;
- expiring link;
- revoke link;
- external delivery page;
- access log.

### Analytics

MVP:

- internal dashboard;
- CSV export;
- JSON export.

Metrics:

- new intakes per week;
- time to first review;
- cases by status;
- missing information count;
- documents waiting for review;
- integration failures;
- cases ready for delivery;
- estimated manual work avoided.

## Cross-Cutting Requirements

- All important actions produce audit events.
- All tenant-scoped queries include tenant filtering.
- External integrations use adapter interfaces.
- Real integrations are activated after mocks are stable.
- No production credentials in source code.
- No real customer data in demo.
- Accessibility follows WCAG 2.1 A/AA principles.
- Serious automated tests are required for every module.
- Negative tests are required for invalid input, forbidden actions, expired/revoked links, cross-tenant access, integration failures, AI failures, and unsafe file uploads.
- A feature is not accepted until relevant tests are implemented and passing.
