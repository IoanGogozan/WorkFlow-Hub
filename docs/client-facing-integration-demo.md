# Client-Facing Integration Demo

Status: implemented historical direction. Superseded for new work by
[Real Live Integration Demo V2](live-integration-demo-v2.md).
Public display name: **Automatisert serviceflyt**

This is the historical durable repository version of the direction defined in
`WORKFLOW_HUB_IMPLEMENTATION_PLAN.md` on 2026-07-10. Implementation should be
performed one task at a time, with verification and review between tasks. Its
implemented replay experience remains in the product, but this document is not
the active delivery plan.

## Purpose

Norvix WorkFlow Hub will be presented as a concrete, client-facing integration
case study. It is not presented as a generic workflow platform or a commercial
multi-company SaaS product.

The public experience must communicate within 30 seconds that Norvix connects
systems a company already uses, allowing information received by email or form
to be verified, registered, archived, reported, and traced without repeated
copy/paste.

Public subtitle:

> Fra e-post og vedlegg til opprettet sak, dokumentstruktur og rapportering –
> uten dobbeltregistrering.

## Target Visitor

The primary visitor is a decision-maker in a Norwegian technical or service
company with approximately 20–250 employees, for example a general manager,
operations manager, project manager, quality manager, administration manager,
IT manager, or digitalization lead.

The typical environment includes Outlook or a shared inbox, Microsoft 365 and
SharePoint, an ERP/accounting/service/project system, and Excel, Power BI, or
manual reporting. The public demo must require no software-development
knowledge.

## Public Message and Product Principles

- Show one primary service-request scenario, not multiple workflows or modules.
- Keep the customer's existing systems visible; the story is integration, not
  replacement.
- Keep technical complexity behind the interface and expose it only as evidence.
- Treat AI as optional assistance with human approval, never as the core value.
- Use fictional data only.
- Retain the existing backend, data model, demo sessions, tenant isolation,
  document and delivery workflows, integrations, audit history, and tests.
- Prefer the smallest coherent implementation change and avoid unnecessary
  rewrites, migrations, dependencies, or infrastructure.

## One Scenario

The demo follows one fictional request for service and documentation for pump
station 14:

1. A service request arrives by email with a customer reference and attachments.
2. Relevant information is identified and validated.
3. Company information is checked.
4. A case or project is created.
5. Documents are stored, classified, and linked to the case.
6. Reporting and delivery preparation are updated.
7. Important actions are recorded in the audit history.

The manual comparison should show seven to nine recognizable actions such as
reading the email, copying fields, checking Brreg, creating the case and folder
structure, saving attachments, updating status, and recording the work.

## Integration Honesty Model

Every publicly displayed integration must use one of these modes:

- `implemented`: the current application performs and stores the action.
- `public-data-capable`: an external public-data capability exists, while the
  public demo may use a deterministic snapshot for reliability.
- `demo-adapter`: the contract, status, retry, and payload flow are demonstrated
  without sending data to a real customer system.

Planned extensions must not appear in the primary timeline. The UI must map the
stable modes to clear Norwegian wording and must never imply that a demo adapter
is connected to a real customer service.

## MVP Public Flow

1. The visitor opens `/demo` and understands the manual business problem.
2. **Se automatiseringen** creates an isolated temporary demo session.
3. The visitor is redirected to the primary experience at `/`.
4. One realistic incoming email and its usual manual process are shown.
5. **Kjør automatisert flyt** replays deterministic evidence already loaded
   from the backend; it does not pretend to make live external calls.
6. The resulting case, documents, integration state, and audit evidence appear.
7. A before/after comparison and editable savings example explain the value.
8. Technical evidence remains available in a secondary, collapsed view.
9. A final CTA asks whether the visitor has a similar manual process.

## Route Strategy

Implement the new experience in parallel to reduce regression risk:

- Build the new single-scenario experience in parallel before promotion.
- Initially retain `/` and the existing detailed technical routes.
- After acceptance, move the existing overview to `/technical`.
- Keep `/automation` as a compatibility redirect to `/`.
- Keep record-detail routes accessible from the technical view.
- Keep `/summary` as a compatibility redirect to `/#resultat`.

Route promotion requires explicit owner approval after the parallel page and its
tests pass.

## Backend Evidence Read Model

Add a tenant-scoped, read-only `GET /api/demo-story` endpoint. It should compose
the relevant seeded intake, customer, case, documents, delivery package,
integrations, audit evidence, and technical links into one stable public-safe
response.

The endpoint must:

- use the existing tenant/demo-session context;
- return evidence only for the current tenant;
- use read-only queries where appropriate;
- return a stable public-safe `404` when the scenario is unavailable;
- avoid migrations and unstable external calls;
- never expose settings JSON, token hashes, storage paths, IP data, user-agent
  data, secrets, or raw internal identifiers that are not required publicly.

## Measurement Disclaimer

The savings calculator uses editable assumptions, initially:

- 40 requests per week;
- 15 manual minutes per request;
- 70% estimated reduction;
- 7.5 hours per workday.

Formula:

```text
monthly_hours_saved =
requests_per_week * 4.33 * manual_minutes_per_request * reduction_percentage / 60
```

The disclaimer must always be visible:

> Eksempelberegning basert på valgte forutsetninger. Faktisk effekt må måles i
> en avgrenset pilot.

No fixed or guaranteed savings claim may be presented as a measured customer
result without evidence from a real pilot and permission to publish it.

## Delivery Sequence

Changes should be small, independently reviewable, and tested after each task.

### Phase 0 — Safety and Direction

1. Add this durable direction document and link it from the README.
2. Capture the current frontend, backend, and Compose verification baseline.

### Phase 1 — Stable Backend Evidence API

1. Add focused demo-story response contracts.
2. Add the tenant-scoped read-only endpoint.
3. Add security, missing-scenario, and cross-tenant negative tests.
4. Align fictional seeded copy and evidence without changing the schema.

### Phase 2 — New Client-Facing Page in Parallel

1. Build a minimal client demo shell in a temporary parallel route (completed;
   `/automation` now redirects to `/`).
2. Add the incoming request and manual-process sections.
3. Add deterministic, replayable automation timeline behavior with reduced-motion
   support.
4. Add the real outcome summary and before/after comparison.
5. Add the transparent, client-side savings calculator.
6. Add honestly labeled integration and collapsed technical evidence.
7. Add the final CTA and responsive presentation polish.

### Phase 3 — Entry and Route Promotion

1. Redesign `/demo` around the one service process.
2. Add a short public-demo E2E while preserving the technical E2E.
3. Conduct the owner acceptance checkpoint.
4. After explicit approval, promote the client demo to `/` and move the old
   overview to `/technical`.
5. Consolidate the old summary presentation.

### Phase 4 — Quality and Documentation

1. Complete accessibility, keyboard, focus, contrast, and reduced-motion checks.
2. Complete responsive and Norwegian Bokmål copy checks.
3. Align durable repository documentation with this direction and remove or
   archive superseded temporary documents.
4. Run the final regression, dependency, migration-drift, Compose, and E2E gates.

### Phase 5 — Optional After MVP Validation

Only after target-user feedback, consider a resilient real Brreg action, a
controlled Microsoft Graph sandbox proof, a technical integration-failure demo,
and a real pilot-measurement package.

## Explicit Non-Goals

Unless separately approved, do not:

- rewrite the backend or introduce microservices or a message broker;
- replace PostgreSQL or add a billing/onboarding system;
- build a generic workflow editor, chatbot, or autonomous AI decision flow;
- add multiple industries or scenarios to the main experience;
- add new state-management or UI libraries or major framework upgrades;
- deploy new cloud resources or expose credentials;
- use real customer documents;
- remove existing technical capabilities merely because the public UI hides them.

## Implementation Rules

For every task:

1. Inspect the named files and confirm the current behavior before editing.
2. Implement only the named task and preserve unrelated behavior.
3. Keep tenant scoping explicit and all demo data fictional.
4. Label mock/demo adapters honestly.
5. Add or update tests for changed behavior.
6. Run the task-specific verification commands.
7. If the baseline already fails, report it before making unrelated fixes.
8. Report the summary, exact files changed, tests and results, assumptions, and
   remaining risks.
9. Stop after one implementation task for review.

## Definition of Done

The redesign is complete when one service-integration story is understandable
within 30 seconds; the workflow replay and every evidence mode are honest; the
result and editable estimate use operational language; the technical application
remains available; tenant isolation, security, frontend, backend, migration,
Compose, and Playwright checks pass; mobile and accessibility requirements are
met; documentation matches the new message; and no real data or unsupported
claims are present.
