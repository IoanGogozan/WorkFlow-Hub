# Testing Strategy

## Test Goals

- Prove tenant isolation.
- Prove authorization and role checks.
- Prove the complete MVP flow.
- Prevent regressions in document, delivery, and AI review security.
- Cover negative, abuse, and failure scenarios, not only happy paths.
- Keep tests useful and focused, not decorative.

## Testing Standard

Tests are a first-class delivery requirement. A feature is not complete when it only works manually; it is complete when positive, negative, authorization, tenant isolation, validation, and relevant failure-mode tests are in place.

Every feature should include:

- happy path tests;
- negative input tests;
- unauthorized and forbidden access tests;
- cross-tenant access tests where tenant-scoped data is involved;
- validation error tests;
- external dependency failure tests for adapters/integrations;
- audit-event tests for important state changes;
- regression tests for every fixed bug.

Do not add shallow tests that only assert framework wiring. Prefer tests that prove business rules, security boundaries, and user-visible behavior.

## Backend Tests

Use:

- xUnit;
- FluentAssertions;
- Testcontainers for PostgreSQL;
- integration tests through the API where security behavior matters.

### Unit Tests

Cover:

- domain status transitions;
- AI suggestion approval rules;
- delivery link expiry/revocation logic;
- file type validation;
- file size validation;
- MIME/extension mismatch rejection;
- role permission helpers;
- missing information checklist logic;
- invalid state transitions;
- token hash comparison logic;
- data retention policy calculations.

### Integration Tests

Cover:

- database migrations;
- tenant query isolation;
- API authorization;
- audit event writes;
- document upload metadata;
- delivery link access;
- Brreg adapter mock failure handling.

Negative integration tests:

- invalid JSON returns validation problem details;
- invalid UUIDs return controlled errors;
- missing required fields return validation errors;
- duplicate tenant-safe business keys are rejected;
- stale or missing tenant context is rejected;
- direct object access by ID across tenants is rejected;
- transaction rollback prevents partial state after failed workflow actions.

### Mandatory Tenant Isolation Tests

- user from tenant A cannot list cases from tenant B;
- user from tenant A cannot fetch an intake from tenant B by direct ID;
- user from tenant A cannot download a document from tenant B;
- user from tenant A cannot access tenant B audit events;
- API key from tenant A cannot write webhook events for tenant B.
- tenant A delivery package cannot include tenant B document;
- tenant A review task cannot approve tenant B AI run;
- tenant A integration sync cannot update tenant B connection.

### Mandatory Authorization Tests

- Viewer cannot create intake.
- Viewer cannot approve AI suggestion.
- OperationsUser cannot edit integrations.
- Reviewer can approve review tasks.
- Admin can manage integrations.
- TenantOwner can manage tenant settings.
- Unauthenticated API requests are rejected.
- ExternalRecipient cannot access internal API endpoints.
- Admin in tenant A has no admin rights in tenant B unless explicitly a member there.
- Disabled user cannot access any tenant data.

### Delivery Security Tests

- expired link fails;
- revoked link fails;
- random invalid token fails;
- delivery link only exposes selected package items;
- access is logged.
- raw delivery token is never stored;
- token guessing is rate-limited;
- download is blocked after package revocation;
- external recipient cannot enumerate package IDs or document IDs.

### AI Safety Tests

- AI output does not directly update final case fields without approval;
- invalid AI output schema is rejected;
- prompt/model/provider metadata is stored;
- AI disabled tenant does not call AI provider.
- low-confidence AI output is routed to review;
- prompt-injection-like document text cannot override system behavior;
- malformed provider response is stored as failed run without mutating business data;
- approving AI suggestions writes audit events;
- rejecting AI suggestions leaves original business data unchanged.

### File Upload Negative Tests

- executable extension is rejected;
- double extension like `invoice.pdf.exe` is rejected;
- unsupported MIME type is rejected;
- extension/MIME mismatch is rejected;
- oversized file is rejected;
- empty file is rejected where not allowed;
- original filename with path traversal characters is sanitized;
- document from tenant B cannot be linked to tenant A case.

### Integration Failure Tests

- Brreg timeout returns controlled error and does not block case viewing;
- Brreg invalid organization number returns validation error;
- accounting mock failure creates failed sync run;
- failed sync retry creates new sync run and preserves original failure;
- disconnected integration cannot be synced;
- external ID from another tenant cannot be reused.

### Audit Tests

Important actions must produce audit events:

- intake created;
- AI analysis requested;
- AI suggestion approved/rejected;
- intake converted to case;
- document uploaded;
- document classification approved/rejected;
- delivery package created;
- delivery link created/revoked;
- integration sync failed/retried;
- authorization failure for cross-tenant access.

## Frontend Tests

Use:

- component tests where useful;
- Playwright for E2E workflows.

E2E flows:

- dashboard loads;
- create intake;
- review AI suggestion;
- convert intake to case;
- upload/classify/approve document;
- generate delivery package;
- open delivery link;
- revoke delivery link;
- verify integrations page status.

Negative E2E flows:

- try to open a case from another tenant and verify access denied;
- try to approve AI suggestion as Viewer and verify action is blocked;
- try to open revoked delivery link and verify it fails cleanly;
- upload unsupported file type and verify accessible validation message;
- submit invalid intake form and verify field-level errors.

Accessibility checks:

- keyboard navigation for main flows;
- visible focus states;
- labels for form fields;
- clear validation messages;
- no obvious color contrast failures.

## CI Checks

Minimum CI:

- backend restore/build/test;
- frontend install/lint/typecheck/test/build;
- Docker Compose config validation;
- dependency scanning later;
- Playwright E2E later when app shell is stable.

CI quality gates:

- backend tests must pass;
- frontend typecheck must pass;
- frontend lint must pass;
- E2E smoke tests must pass before demo deployment;
- security-critical tests must not be skipped;
- tests should run with deterministic fake data.

## Coverage Expectations

Coverage percentage is not the main goal, but minimum expectations should still be tracked:

- domain/application services: high coverage for business rules;
- API authorization and tenant isolation: all security-critical paths covered;
- frontend: core workflows covered by E2E tests;
- integrations: mock adapters and failure modes covered;
- AI: schema validation, review gating, disabled mode, and failure handling covered.

Do not block progress on arbitrary global coverage numbers early in the MVP. Block progress on missing tests for security boundaries, tenant isolation, delivery links, file upload, and AI review gating.

## Test Data

Use fictional demo data only:

- Agder Drift & Service AS;
- fake users;
- fake customers;
- fake documents;
- mock intake messages;
- mock integration responses.
