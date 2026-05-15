# Acceptance Checklist

Use this checklist during implementation. A phase is not complete until its acceptance items are done and tested.

## Global Rules

- [ ] Fake/demo data only.
- [ ] No production credentials.
- [ ] Every business record is tenant-scoped.
- [ ] Every tenant-scoped query includes tenant context.
- [ ] Important actions create audit events.
- [ ] External integrations have mock adapters first.
- [ ] AI output is suggestion-only until human approval.
- [ ] Delivery links have token, expiry, revocation, and access logging.
- [ ] File uploads use allowlisted types and size limits.
- [ ] Accessibility basics are checked for each user-facing flow.
- [ ] Every feature has happy path tests.
- [ ] Every feature has negative tests for invalid input and forbidden actions.
- [ ] Every tenant-scoped feature has cross-tenant isolation tests.
- [ ] Every integration adapter has failure-mode tests.
- [ ] Code files follow `docs/coding-standards.md` size limits or have documented exceptions.

## Phase 0 - Setup

- [ ] Monorepo structure exists.
- [ ] Frontend app starts locally.
- [ ] Backend API starts locally.
- [ ] Docker Compose starts PostgreSQL, Azurite, and Mailpit.
- [ ] Health endpoint returns success.
- [ ] CI workflow skeleton runs build/test commands.
- [ ] README contains setup instructions.
- [ ] Backlog item exists for automated file-size checks in CI.

## Phase 1 - Auth, Tenants, Demo Data

- [ ] Local dev auth stub works.
- [ ] Entra ID production architecture is documented.
- [ ] Tenants, users, and memberships exist.
- [ ] Roles are enforced server-side.
- [ ] Demo tenant Agder Drift & Service AS is seeded.
- [ ] Tenant isolation tests pass.
- [ ] Negative auth/RBAC tests pass.
- [ ] Audit event service is implemented.

## Phase 2 - Intake Inbox

- [ ] User can create manual intake.
- [ ] User can list and view intake items.
- [ ] Mock email intake exists.
- [ ] Mock form intake exists.
- [ ] Attachments can be added.
- [ ] Intake status changes are audited.
- [ ] Invalid intake payloads return validation errors.
- [ ] Unauthorized users cannot create or edit intake items.
- [ ] Cross-tenant direct ID access is rejected.

## Phase 3 - AI Review

- [ ] Mock AI provider returns structured suggestions.
- [ ] AI analysis run is stored.
- [ ] Review queue shows pending suggestions.
- [ ] User can approve suggestions.
- [ ] User can edit suggestions before approval.
- [ ] User can reject suggestions.
- [ ] AI approval/rejection is audited.
- [ ] Invalid AI output schema is rejected.
- [ ] AI disabled tenant does not call AI provider.
- [ ] Unauthorized roles cannot approve suggestions.

## Phase 4 - Case Workspace

- [ ] Approved intake can be converted to case.
- [ ] Case overview displays core metadata.
- [ ] Tasks can be added and updated.
- [ ] Notes can be added.
- [ ] Documents can be linked.
- [ ] Missing information checklist is visible.
- [ ] Activity tab shows audit/activity events.
- [ ] Invalid case status transitions are rejected.
- [ ] Cross-tenant case access is rejected.

## Phase 5 - Brreg Integration

- [ ] Organization can be searched by organization number.
- [ ] Organization can be searched by name.
- [ ] Customer data can be enriched from selected result.
- [ ] Source timestamp is stored.
- [ ] API failure is handled gracefully.
- [ ] Invalid organization number input is rejected.
- [ ] Brreg timeout/failure path is tested.

## Phase 6 - Document Workflow

- [ ] Document upload stores metadata.
- [ ] Document binary is stored in blob-compatible storage.
- [ ] New versions can be uploaded.
- [ ] AI classification can be requested.
- [ ] Classification requires human approval.
- [ ] Document can be linked to case.
- [ ] Document library filters work.
- [ ] Unsupported file type is rejected.
- [ ] Oversized file is rejected.
- [ ] Tenant A document cannot be linked to tenant B case.

## Phase 7 - Integration Dashboard

- [ ] Connector status page exists.
- [ ] Brreg status is shown.
- [ ] Microsoft Graph/SharePoint mock status is shown.
- [ ] Accounting/project mock status is shown.
- [ ] Power BI/Fabric export mock status is shown.
- [ ] Sync run history is shown.
- [ ] Failed sync can be retried.
- [ ] User without admin role cannot edit integrations.
- [ ] Failed sync path is tested.

## Phase 8 - Delivery Package

- [ ] Delivery package can be created from case.
- [ ] Documents can be selected for package.
- [ ] PDF summary can be generated.
- [ ] Secure expiring link can be created.
- [ ] External delivery page works.
- [ ] Delivery access is logged.
- [ ] Link can be revoked.
- [ ] Expired/revoked links fail.
- [ ] Invalid/random delivery token fails.
- [ ] Delivery link exposes only selected package items.

## Phase 9 - Analytics

- [ ] Dashboard metrics are shown.
- [ ] CSV export works.
- [ ] JSON export works.
- [ ] Integration failure metrics are visible.
- [ ] Cases ready for delivery are visible.
- [ ] User cannot export another tenant's metrics.

## Phase 10 - Polish

- [ ] Demo script can be completed in 5 minutes.
- [ ] README explains business problem and architecture.
- [ ] Screenshots are captured.
- [ ] Architecture diagram exists.
- [ ] Security/privacy docs are reviewed.
- [ ] Basic accessibility pass is complete.
