# Real Live Integration Demo V2

Status: active implementation plan. Phases through Brreg and the local
SharePoint simulator are implemented. The signed ERP demo receiver, final
public-page promotion, CI/release readiness, and deployment runbook remain.
This supersedes the earlier client-facing demo direction for future work; the
earlier document remains the historical record of the replay demo.

Implemented amendment: because a Microsoft 365 subscription is not justified
for this demo, SharePoint work follows
[the SharePoint simulator amendment](sharepoint-simulator-amendment.md). It is
an explicitly simulated local adapter and not a live SharePoint integration.

## Purpose and public experience

The public demo must process a new fictional request when a visitor starts a
run. In a short flow it demonstrates public company-data validation, internal
case and fictional PDF creation, an honestly labelled local SharePoint/Graph
simulation, and—after Phase 7—a signed request accepted by a separate **ERP
demo receiver**. The detailed application remains available at `/technical`.

The primary page uses four stages:

```text
Mottatt → Kontrollert → Opprettet → Synkronisert
```

It shows public-safe status, duration, provider, short result, and evidence
mode. Completion includes run ID, case number, Brreg mode, shortened external
references, ERP receipt, duration, and audit-event count. Never expose secrets,
tokens, raw external errors, administrative links, internal paths, or raw IDs
not needed by the visitor.

## Evidence boundaries

- Internal operations are persisted: run, intake, customer, case, fictional
  PDF, document/version, delivery basis, audit events, and step evidence.
- Brreg uses one allowlisted server-configured organization number. Fallback is
  permitted only after a failed real call and is always visibly labelled.
- SharePoint uses the local provider-based simulator: deterministic folders,
  metadata, versions/eTags, idempotency, restricted-site evidence, throttling,
  tenant scoping, and expiry cleanup. No Microsoft 365 tenant is connected.
- The ERP demo receiver validates HMAC, timestamp, payload, and idempotency;
  persists a receipt; and has a controlled fail-once demonstration. It is never
  described as a customer ERP or named accounting product.

## Architecture and API

Keep the current demo-session and tenant model. A persistent worker processes a
queued run; the browser polls every 750–1000 ms. Do not add SSE, WebSockets, a
workflow builder, broker, generic plugin system, or microservices beyond the
small ERP receiver.

```text
POST /api/live-demo-runs → queued run → LiveDemoRunWorker → persisted evidence
GET  /api/live-demo-runs/{runId} ← browser polling
```

Create tenant-scoped `LiveDemoRun` and `LiveDemoRunStep` entities in
`Domain/LiveDemo`. Use explicit state transitions, a unique
`TenantId + RunId + Key` step index, max two retries, and reuse of artifact IDs
to prevent duplicates. Internal steps are request-created, brreg-checked,
case-created, document-created, sharepoint-synced, erp-received, and
run-completed.

Public endpoints are create (`POST /api/live-demo-runs`), read
(`GET /api/live-demo-runs/{runId}`), retry, and capabilities. The browser may
start only a preset scenario; it cannot submit organization numbers, names,
email, free text, URLs, or files.

## Security and operations

- Limit creation to 3 runs per client IP per 10 minutes, 3 per session, and one
  queued/running run per session.
- All external actions are tenant-scoped, auditable, retry-safe, and
  public-safe.
- CI uses fake HTTP handlers/adapters and never calls Brreg, Graph, or ERP.
- Microsoft Graph mode is inactive. If activated later, Graph secrets remain
  worker-only and no mail permissions are required.
- Deployment and external infrastructure changes require explicit approval.

## Delivery sequence

Each task is an isolated, reviewable change.

1. Record this plan and baseline the repository.
2. Create static `/live-preview` UX and E2E approval coverage.
3. Add persistent model, EF mapping/migration, cleanup, and API contracts.
4. Add rate limiting plus create/read/capabilities/retry endpoints.
5. Add focused processor, reusable PDF, idempotent internal artifacts, worker,
   polling UI, and E2E.
6. Add Brreg live/fallback adapter and visible evidence.
7. Add and verify the local SharePoint simulator amendment. **Implemented.**
8. Add signed ERP demo receiver, idempotent client, fail-once, and Compose.
   **Not implemented.**
9. Align and promote the concise capability-driven page. **Not completed.**
10. Complete CI, smoke, deployment preparation, and the final release gate.
    **Not completed.**

## External gates and non-goals

Real Microsoft Graph remains deferred until the owner explicitly approves a
tenant, Entra app, dedicated site, and `Sites.Selected` access. This is not a
gate for the local simulator. Before ERP receiver activation, an ERP HMAC must
be generated and stored only in the server `.env`.

Do not add Outlook mailbox integration, public email, real customer ERP,
public upload, customer onboarding, billing, a workflow builder, broker,
Kubernetes, unrelated AI, real customer data, or additional main-page
scenarios.

## Definition of done

A visitor can start a new run and observe measured timings, live Brreg or
labelled fallback, newly created internal artifacts, clearly simulated
SharePoint evidence, and—after Phase 7—a signed ERP receipt with controlled
failure and duplicate-safe retry. Tenant isolation, idempotency, cleanup,
frontend/backend tests, migration checks, CI, and deployed smoke checks pass.

## Task rules

Work only on `agent/live-integration-demo-v2`. Never push directly to `main`,
merge, deploy, SSH, modify external infrastructure, expose secrets, or perform
unrelated cleanup. Implement exactly one numbered task per run, verify it, and
stop for review.
