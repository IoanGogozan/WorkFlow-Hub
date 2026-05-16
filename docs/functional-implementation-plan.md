# Functional Implementation Plan

This document converts the external evaluation into a concrete implementation plan for making Norvix WorkFlow Hub a usable, deployable product, not a static demo.

The priority is a complete visible workflow in the browser:

Dashboard -> Create Intake -> Analyze with AI -> Approve -> Convert to Case -> Upload/Classify Documents -> Create Delivery Package -> Open Public Link -> Review Audit/Activity

## Current Product Gap

The backend already contains many workflow APIs and domain modules, but the frontend is still mostly a static dashboard shell. This means the project can look broad in code while still feeling inactive to a real user.

The immediate goal is not to add more modules. The immediate goal is to connect the existing backend to the frontend and complete one vertical product flow that a potential customer can test.

## Product Definition of Done

The application is considered functionally ready for external demo/testing only when all of these are true:

- A user can complete the full intake-to-delivery workflow from the browser.
- Dashboard metrics, inbox items, integrations, cases, documents, and delivery links come from the backend API.
- No main UI page depends on hardcoded fake arrays unless explicitly marked as seeded demo data from the backend.
- Local development auth is clearly labelled as local-only and is not presented as production authentication.
- API contract, README, and visible product status match what is actually implemented.
- Frontend build does not depend on external Google Fonts or other unnecessary remote build-time assets.
- Delivery links can return useful downloadable content or a clearly named placeholder PDF.
- Public endpoints have minimum hardening before public deployment.
- Demo data can be reset, deleted, or anonymized.
- Automated checks cover the critical flow.

## Implementation Principles

- Finish vertical slices before expanding scope.
- Do not add real Graph, Tripletex, Azure AI, Azure Blob, Fabric, or Entra ID until the mock-backed product flow works end to end.
- Keep AI as suggestion-only. Human approval remains required before case, document, delivery, or external action changes.
- Keep all business data tenant-scoped.
- Treat seeded demo data as demo data, not as static UI content.
- Keep documentation honest: implemented means usable through the product or API, not only planned.

## Phase 11 - Make the Frontend Real

Objective: the user can complete this browser flow:

Dashboard -> Create Intake -> Analyze with AI -> Approve -> Convert to Case -> View Case

### 11.1 Frontend API Proxy

Implement `frontend/next.config.ts` rewrites:

- `/api/:path*` -> `http://localhost:5000/api/:path*`
- `/delivery/:path*` -> `http://localhost:5000/delivery/:path*`

Acceptance criteria:

- Frontend can call `fetch("/api/intakes")` without browser CORS errors.
- Public delivery pages can call `/delivery/{token}` from the frontend.
- Backend URL is configurable for deployment, not hardcoded forever.

### 11.2 Remove External Font Build Dependency

Replace `next/font/google` usage with system fonts in `frontend/src/app/layout.tsx` and `globals.css`.

Acceptance criteria:

- `npm --prefix frontend run build` does not need Google Fonts network access.
- Visual quality remains professional with system font stack.

### 11.3 Frontend API Client

Create:

- `frontend/src/lib/api.ts`
- `frontend/src/lib/dev-auth.ts`
- `frontend/src/lib/types.ts`

The local API client should include the existing local development headers:

- `X-Norvix-Tenant-Id: 11111111-1111-4111-8111-111111111111`
- `X-Norvix-User-Id: 22222222-2222-4222-8222-222222222222`

Acceptance criteria:

- All frontend API calls go through one helper.
- Errors are surfaced as readable UI messages.
- The helper supports JSON requests, JSON responses, `204 No Content`, and form upload where needed later.
- Local auth headers are isolated so they can be replaced by Entra ID/MSAL later.

### 11.4 Frontend Structure

Create the route and component structure:

```text
frontend/src/
  app/
    layout.tsx
    page.tsx
    intakes/
      page.tsx
      new/
        page.tsx
      [id]/
        page.tsx
    cases/
      page.tsx
      [id]/
        page.tsx
    documents/
      page.tsx
    integrations/
      page.tsx
    delivery/
      [token]/
        page.tsx
  components/
    app-shell.tsx
    status-badge.tsx
    loading-state.tsx
    error-state.tsx
    empty-state.tsx
  features/
    intakes/
      intake-list.tsx
      create-intake-form.tsx
      intake-detail.tsx
      ai-suggestion-panel.tsx
    cases/
      case-list.tsx
      case-detail.tsx
    integrations/
      integration-list.tsx
```

Acceptance criteria:

- Navigation links work.
- Pages have loading, error, and empty states.
- Forms have accessible labels, focus states, validation messages, and submit loading state.
- UI does not show fake data unless it came from seeded backend data.

### 11.5 Dashboard Connected to Backend

Replace hardcoded dashboard arrays in `frontend/src/app/page.tsx`.

Dashboard should fetch:

- `GET /api/metrics/overview`
- `GET /api/intakes`
- `GET /api/integrations`
- `GET /api/review-tasks`

Acceptance criteria:

- Metrics are backend-driven.
- Intake inbox preview is backend-driven.
- Integration status is backend-driven.
- Review queue button links to a real page or a real filtered section.
- Retry/sync actions make real API calls where supported.

### 11.6 Intake List and Create Intake

Implement:

- `/intakes`
- `/intakes/new`

Create intake form fields:

- source
- subject
- body
- customer name
- organization number
- category
- urgency

Acceptance criteria:

- `/intakes` lists `GET /api/intakes`.
- Each intake links to `/intakes/{id}`.
- `/intakes/new` posts to `POST /api/intakes`.
- After create, user is redirected to the intake detail page.
- Validation errors are visible without losing entered data.

### 11.7 Intake Detail and AI Review

Implement `/intakes/{id}`.

The page should show:

- subject
- body
- status
- source
- customer
- organization number
- category
- urgency
- latest AI suggestion, if one exists

Actions:

- `POST /api/intakes/{id}/analyze`
- Edit returned suggestion fields
- `POST /api/intakes/{id}/approve-ai`
- `POST /api/intakes/{id}/reject-ai`, if supported
- `POST /api/intakes/{id}/convert-to-case`

Acceptance criteria:

- Analyze button starts a backend AI analysis and displays the returned suggestion.
- User can edit customer name, organization number, category, and urgency before approval.
- Approved AI suggestions are persisted by backend.
- Convert creates a case and redirects to `/cases/{caseId}`.
- Re-running or invalid state transitions show useful errors.

### 11.8 Case List and Case Detail

Implement:

- `/cases`
- `/cases/{id}`

Case detail should show:

- case number
- title
- description
- status
- source intake
- customer
- tasks
- notes
- linked documents
- activity

Actions:

- `POST /api/cases/{id}/tasks`
- `POST /api/cases/{id}/notes`
- `GET /api/cases/{id}/activity`

Acceptance criteria:

- Created cases are visible in the case list.
- Case detail can be opened after converting an intake.
- User can add a task.
- User can add a note.
- Activity log updates after relevant actions.

### 11.9 Integrations Page

Implement `/integrations`.

The page should show:

- provider
- display name
- connection status
- last sync
- failed sync reason
- available actions

Actions:

- connect
- disconnect
- sync
- retry failed sync runs where backend supports it

Acceptance criteria:

- Integration data comes from `GET /api/integrations`.
- Buttons call real backend endpoints.
- Failed syncs are visible and retryable where supported.
- Unsupported actions are hidden or disabled with clear UI state.

### 11.10 Phase 11 Verification

Required checks:

```bash
npm --prefix frontend run lint
npm --prefix frontend run build
dotnet test backend/NorvixHub.sln --configuration Release
```

Manual acceptance test:

1. Start local dependencies and app.
2. Open dashboard.
3. Create a new intake.
4. Analyze with AI.
5. Edit and approve suggestion.
6. Convert to case.
7. Open the created case.
8. Add a task and note.
9. Confirm activity/log visibility.
10. Open integrations and trigger one supported sync action.

## Phase 12 - Document Workflow in the UI

Objective: the user can upload, classify, approve, and link documents to a case from the browser.

Routes:

- `/documents`
- `/documents/{id}`
- document section inside `/cases/{id}`

Features:

- Upload document with file validation.
- List documents.
- View document metadata and versions.
- Run document classification.
- Approve or reject classification.
- Link document to a case.
- Show classification confidence and review status.

Acceptance criteria:

- Upload sends a real multipart request to the backend.
- Uploaded file metadata is visible immediately after upload.
- Classification result is visible and requires approval.
- Approved document can be linked to an existing case.
- Case detail shows linked documents.
- Invalid file type, too-large files, and failed uploads show readable errors.

Backend cleanup for this phase:

- Implement `GET /api/documents/{id}/download` or expose a clear placeholder download route.
- Implement `POST /api/documents/{id}/reject-classification` if the UI exposes rejection.
- Confirm upload size limits exist at API/server level.

## Phase 13 - Delivery Package Flow

Objective: the user can create a client-facing delivery package from a case.

Browser flow:

Case -> Select Documents -> Create Delivery Package -> Generate PDF -> Create Link -> Open Public Link -> View Access Log

Features:

- Create delivery package from case.
- Select approved documents.
- Generate a real simple PDF summary.
- Create expiring public link.
- Open public delivery page.
- Download package documents or placeholder PDFs.
- Revoke link.
- Show access logs in internal UI.

Acceptance criteria:

- Delivery package is created from real case/document data.
- Generated PDF is an actual PDF file, not only metadata.
- PDF includes case title, customer, date, document list, generated timestamp, and Norvix WorkFlow Hub footer.
- Public link works without local dev auth headers.
- Expired and revoked links fail.
- Access is logged.
- Token returned by create-link belongs only to the newly created link, not all package links.

Backend fixes included in this phase:

- Fix delivery link response so a new token is returned only for the newly created link.
- Make `GET /delivery/{token}/documents/{documentId}` return a downloadable file or a generated placeholder PDF.
- Add rate limiting for public delivery endpoints.

## Phase 14 - API Contract and Backend Correctness

Objective: make backend behavior, docs, and frontend expectations consistent.

Contract sync tasks:

- Compare `docs/api-contract.md` with implemented endpoints.
- Mark unimplemented endpoints as planned or implement them if required by the UI.
- Remove or rename endpoints that no longer match the product flow.

Known endpoint gaps to decide:

- `PATCH /api/intakes/{id}`
- `POST /api/intakes/{id}/attachments`
- `GET /api/review-tasks/{id}`
- approve/reject review task endpoints
- `POST /api/cases`
- `PATCH /api/cases/{id}`
- `PATCH /api/cases/{id}/tasks/{taskId}`
- `GET /api/cases/{id}/missing-information`
- `POST /api/documents/{id}/reject-classification`
- `GET /api/documents/{id}/download`
- `POST /api/customers/{id}/refresh-brreg`
- admin endpoints

Backend bug fixes:

- Fix `BrregClient.GetByOrganizationNumberAsync` so `BaseAddress` is always set before relative requests.
- Prefer setting `BaseAddress` in DI for `IBrregClient`.
- Add regression test for calling `GetByOrganizationNumberAsync` before `SearchAsync`.

Acceptance criteria:

- API docs do not promise unsupported behavior.
- All endpoints required by the frontend exist and have tests.
- Brreg lookup works regardless of method call order.

## Phase 15 - Deployment Readiness and Security Hardening

Objective: make the public test deployment safe enough for fake/demo data and credible for customer evaluation.

Minimum hardening:

- `UseHttpsRedirection` in deployed environments.
- Secure response headers.
- Rate limiting for public delivery links and auth-sensitive endpoints.
- Consistent `ProblemDetails` error responses.
- Request correlation ID.
- Structured logging.
- Upload size limits.
- Clear CORS/proxy policy.
- Environment-based configuration.
- No production secrets in repo.
- Demo seed/reset command.

Authentication plan:

- Keep local dev headers only for local development.
- Add clear README/UI wording: "Local development auth only."
- Prepare Microsoft Entra ID/MSAL implementation after the working demo flow is complete.
- Do not present header-based auth as real production auth.

Acceptance criteria:

- Public deployment does not require exposing backend directly to browser CORS unless intentionally configured.
- Public delivery endpoints are rate limited.
- Error responses are consistent and do not leak internal details.
- Logs include correlation IDs for debugging.

## Phase 16 - Privacy and Data Operations

Objective: convert privacy documentation into operational product functions.

Required product functions before real customer data:

- Tenant/user data export.
- Delete or anonymize demo data.
- Retention settings.
- Visible subprocessor list.
- Tenant privacy settings.
- Audit export.
- AI enabled/disabled per tenant.
- Uploaded document deletion workflow.
- Customer data deletion workflow.
- Clear data processing mode in the UI.

Acceptance criteria:

- A tenant admin can export audit and relevant business data.
- Demo data can be reset safely.
- Uploaded documents can be deleted according to policy.
- AI processing can be disabled per tenant.
- Privacy settings are visible and auditable.

## Phase 17 - Real Integrations

Objective: replace mock adapters one by one after the core product flow is already usable.

Recommended order:

1. Entra ID / MSAL authentication.
2. Azure Blob Storage for document storage.
3. Real Brreg hardening and retry handling.
4. Microsoft Graph / SharePoint integration.
5. Tripletex or selected accounting/project integration.
6. Azure AI Document Intelligence.
7. Azure OpenAI.
8. Fabric / Power BI export.

Acceptance criteria:

- Each real integration is behind an adapter and feature flag/config option.
- Mock mode remains available for local demos and automated tests.
- Integration failures do not block core case workflow.
- Secrets are stored in deployment secret management, not in code or sample config.

## Phase 18 - Product Polish and Customer Trial Readiness

Objective: make the app credible for external testers.

Tasks:

- Replace README "Implemented MVP" wording with accurate current status.
- Update screenshots after UI is connected to backend.
- Update demo script to match real browser flow.
- Add Playwright smoke test for the full workflow.
- Run accessibility pass on forms, navigation, and public delivery page.
- Add empty states and error states for every main route.
- Add loading states for every server action.
- Review mobile layouts for dashboard, intake, case, document, and delivery pages.
- Add basic product copy in Norwegian/English where appropriate.

Acceptance criteria:

- A 5-minute demo can be performed without using Swagger/Postman.
- A new evaluator can run the app from README and complete the workflow.
- UI state survives expected failures such as failed AI analysis, failed upload, failed sync, and expired delivery link.
- Documentation matches the actual state of the product.

## Immediate Execution Backlog

Work in this exact order:

1. Fix frontend hardcoding.
2. Add Next.js proxy.
3. Add frontend API client and shared types.
4. Connect dashboard to API.
5. Create intake list.
6. Create intake form.
7. Add AI analyze and approval UI.
8. Add convert-to-case UI.
9. Add case list and case detail UI.
10. Add integration dashboard UI.
11. Add document upload UI.
12. Add document classify/approve UI.
13. Add delivery package UI.
14. Generate real PDF.
15. Fix Brreg `BaseAddress` bug.
16. Sync API contract with implementation.
17. Add security hardening for public deployment.
18. Add privacy/data operations.
19. Add Entra ID.
20. Add Azure Blob.
21. Add real Graph.
22. Add real Tripletex or selected accounting integration.
23. Add real Azure AI.
24. Add Fabric/Power BI.

## README Status Replacement

Replace the current "Implemented MVP" framing with:

```markdown
## Current Status

The backend workflow foundation is implemented with tenant-scoped APIs, PostgreSQL persistence, audit events, local dev auth, mock AI, mock integrations, document workflow, delivery links, analytics, and automated tests.

The frontend integration is the current priority. The next implementation phase connects the browser UI to the backend APIs and completes the visible flow from intake to AI review to case workspace, followed by document workflow and delivery package flow.
```

After Phase 11 is complete, update the second paragraph to:

```markdown
The frontend is connected to the backend for the Phase 11 flow: dashboard, intake creation, AI review approval, case conversion, case workspace, and integration status. Document workflow and delivery package UI remain the next implementation phases.
```

## Release Gates

### Internal Functional Demo Gate

Required before showing as a working product:

- Phase 11 complete.
- README status corrected.
- Frontend lint/build passes.
- Backend tests pass.
- Manual intake-to-case demo passes.

### Public Demo Gate

Required before external public testing:

- Phases 11, 12, and 13 complete.
- Public delivery download works.
- Security hardening minimums complete.
- Demo data reset/delete path exists.
- README setup works from clean checkout.
- End-to-end smoke test exists.

### Real Customer Data Gate

Required before processing real customer data:

- Production auth with Entra ID/MSAL.
- Real storage configured securely.
- Privacy/data operations from Phase 16 complete.
- Data processing agreement and subprocessors clarified.
- Backup/restore plan documented and tested.
- Monitoring and alerting configured.
- Pen-test or structured security review completed.
