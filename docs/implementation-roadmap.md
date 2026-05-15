# Implementation Roadmap

## Phase 0 - Setup

Deliverables:

- monorepo structure;
- frontend scaffold;
- backend scaffold;
- Docker Compose;
- PostgreSQL;
- Azurite;
- Mailpit;
- optional Seq;
- CI GitHub Actions;
- health endpoint;
- README.

Acceptance criteria:

- `docker compose up` starts local dependencies.
- API health endpoint works.
- Frontend starts.
- CI runs lint, tests, and build skeleton.

## Phase 1 - Auth, Tenants, Demo Data

Deliverables:

- local dev auth stub;
- Entra ID architecture prepared;
- tenants;
- users;
- tenant memberships;
- RBAC;
- seed tenant: Agder Drift & Service AS.

Acceptance criteria:

- user can access app through local dev auth;
- user sees only own tenant data;
- tenant isolation tests pass;
- audit event service exists.

## Phase 2 - Intake Inbox

Deliverables:

- create/list/view intake items;
- attachments;
- manual source;
- mock email source;
- mock form source;
- audit events.

Acceptance criteria:

- demo intake can be created and viewed;
- file attachment metadata is stored;
- intake state changes are audited.

## Phase 3 - AI Review

Deliverables:

- mock AI provider;
- AI suggestion schema;
- AI analysis runs;
- review queue;
- approve/edit/reject actions.

Acceptance criteria:

- AI suggestions do not change final data without approval;
- provider/model/prompt version/output/confidence are logged;
- review decisions are audited.

## Phase 4 - Case Workspace

Deliverables:

- convert intake to case;
- case overview;
- tasks;
- notes;
- documents tab;
- customer tab;
- activity log;
- missing information checklist.

Acceptance criteria:

- one intake can be converted into a case;
- user can add tasks, notes, and documents;
- activity log shows relevant events.

## Phase 5 - Brreg Integration

Deliverables:

- search by organization number;
- search by company name;
- customer enrichment;
- source timestamp;
- failure handling.

Acceptance criteria:

- user can connect case/customer to Brreg organization data;
- API failures are visible and do not break the case workflow.

## Phase 6 - Document Workflow

Deliverables:

- upload;
- blob storage/Azurite;
- document metadata;
- AI classification;
- approval;
- versioning;
- document library.

Acceptance criteria:

- document can be uploaded, classified, approved, versioned, and linked to a case.

## Phase 7 - Integration Dashboard

Deliverables:

- connector statuses;
- mock Microsoft Graph/SharePoint adapter;
- mock Tripletex/accounting adapter;
- mock Power BI/Fabric export adapter;
- sync runs;
- retry failed sync.

Acceptance criteria:

- connector state is visible;
- failed sync can be retried;
- case can be linked to mock external customer/project.

## Phase 8 - Delivery Package

Deliverables:

- PDF generation;
- selected delivery documents;
- secure delivery link;
- external delivery page;
- access logs;
- revoke link.

Acceptance criteria:

- demo case can be delivered to an external recipient;
- delivery access is logged;
- expired/revoked links fail.

## Phase 9 - Analytics

Deliverables:

- dashboard metrics;
- CSV export;
- JSON export;
- optional Fabric/Power BI notes.

Acceptance criteria:

- dashboard shows operational metrics;
- export contains relevant event/case data.

## Phase 10 - Polish and Portfolio

Deliverables:

- UI polish;
- demo script;
- screenshots;
- architecture diagram;
- security/privacy docs;
- README final.

Acceptance criteria:

- 5-minute demo can be performed end-to-end;
- project can be published online as a portfolio-quality working app.

## Recommended First Implementation Slice

Start with:

1. monorepo;
2. backend .NET API;
3. frontend Next.js shell;
4. Docker Compose dependencies;
5. tenant/auth/audit foundation;
6. Intake Inbox;
7. Case Workspace;
8. Brreg lookup;
9. Document Workflow with mock AI review;
10. Delivery Package.

Do not start with real Fabric, Tripletex, Graph, or real AI in the first week. Build adapters and mocks first, then activate real integrations one at a time.
