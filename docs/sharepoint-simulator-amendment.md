# SharePoint Simulator Amendment

Status: implemented temporary direction. The local simulator is active by
default; Microsoft Graph remains intentionally inactive and unconfigured.

This amendment replaces the live SharePoint prerequisite in the Live
Integration Demo V2 plan while no Microsoft 365 tenant is available. It does
not represent a live Microsoft Graph or SharePoint Online connection.

## Honest product boundary

WorkFlow Hub will provide a provider-based SharePoint document adapter with two
modes:

- `Simulated` — fully functional local simulator, the default mode;
- `MicrosoftGraph` — a configuration-validated future provider, with no live
  Graph calls until a tenant, restricted site, and credentials are explicitly
  approved.

Every user-facing and technical view must label `Simulated` as a local
SharePoint/Microsoft Graph simulator. It must never claim that a Microsoft 365
tenant, SharePoint site, Entra application, or Graph connection exists.

The public live-demo may show a completed simulated adapter step only with the
explicit evidence mode `simulated-sharepoint`. It cannot be labelled live or
connected. The existing internal document archive remains the actual document
storage.

## Existing components to reuse

- `IFileStorage`, `DocumentRecord`, and `DocumentVersion` hold the document
  bytes and versions; no duplicate storage is added.
- `IntegrationConnection` and `IntegrationSyncRun` remain the integration
  dashboard model; the generic Microsoft Graph mock is narrowed to orchestrate
  real simulator operation counts.
- `AuditEvent` records application audit events. Simulator operation evidence
  is persisted separately only where detailed idempotency and safe technical
  history require it.
- The tenant, demo-session, worker, retry, cleanup, and technical-page
  conventions remain unchanged.

## Minimal simulator scope

The simulator models only the Graph concepts this workflow needs:

- fixed simulated site `site-demo-service`, named `Service Operations Demo`;
- library `Shared Documents` with drive `drive-shared-documents`;
- deterministic, SharePoint-safe customer/case folders and Incoming, Approved,
  and Delivery subfolders;
- upload, metadata update, document listing, version/eTag progression, and
  idempotent repeated synchronization;
- safe operation history using Graph-like HTTP method/status terminology;
- one allowed site with a simulated `Sites.Selected` write permission and a
  deterministic 403 result for all other sites;
- an optional one-time 429 throttling demonstration followed by retry.

The fake URL is display-only and never an external link. Raw document content,
credentials, tokens, filesystem paths, and raw exceptions are never logged or
returned by technical endpoints.

## Configuration

Use typed `SharePoint` options with safe defaults:

```text
SharePoint:Mode=Simulated
SharePoint:SimulatedSiteId=site-demo-service
SharePoint:SimulatedSiteName=Service Operations Demo
SharePoint:SimulatedDriveId=drive-shared-documents
SharePoint:SimulatedLibraryName=Shared Documents
SharePoint:SimulateThrottling=false
```

Future `MicrosoftGraph` mode validates, without making a network call, the
presence of tenant ID, client ID, client secret, site ID, and drive ID. Secrets
are server environment variables only, absent from defaults and examples.

## Incremental delivery

### Task S0.1 — Record amendment and baseline

- Link this amendment from the V2 plan and README.
- Confirm existing integration/dashboard tests as the baseline.

### Task S1.1 — Provider contract and safe configuration

- Add the SharePoint adapter contract, typed options, `Simulated` resolution,
  and a safe not-configured `MicrosoftGraph` placeholder.
- Do not add a migration or change the public workflow.
- Test default selection and safe missing Graph configuration.

### Task S1.2 — Persistent simulator evidence

- Add the smallest tenant-scoped metadata/operation model and migration.
- Store stable simulated IDs, folder path, eTag/version, sanitized metadata,
  status, duration, and idempotency key; do not store bytes or secrets.
- Extend demo-session cleanup and tenant-isolation tests.

### Task S2.1 — Deterministic simulated adapter

- Provision folders, upload/reuse a document version, update metadata, and
  list items using existing file/document records.
- Record Graph-like operations and enforce the single allowed simulated site.
- Test idempotency, new version/eTag, stale eTag (412), and denied site (403).

### Task S2.2 — Existing dashboard orchestration

- Replace the fixed `microsoft-graph` mock count with counts derived from the
  simulator operations, without changing other mock providers.
- Add dashboard/API tests for safe tenant-scoped results.

### Task S3.1 — Live-demo integration and public honesty

- Execute `sharepoint-synced` through the simulated adapter after internal
  document creation.
- Persist only shortened safe references on the run.
- Show `Simulated SharePoint adapter — no Microsoft 365 tenant connected` in
  the public result; never show a connected/live badge.

### Task S3.2 — Technical evidence view

- Add protected technical endpoints and `/technical/sharepoint` using existing
  technical UI conventions.
- Show simulation status, safe folder tree, documents, operation history, and
  a controlled restricted-site test.

### Task S3.3 — Controlled throttling and verification

- Add an opt-in, deterministic one-time 429 followed by retry success.
- Run backend unit/integration tests, frontend lint/build, relevant Playwright,
  migration checks, and documentation review.

## Deferred work

No Microsoft Graph package, real HTTP call, external site, Entra setup, secret,
deployment change, or real SharePoint claim is part of this amendment. A later
explicit approval can activate the existing provider seam after the original
SharePoint prerequisites are met.

## Running and verifying the simulator

The safe default requires no Microsoft tenant or secret:

```text
SharePoint__Mode=Simulated
SharePoint__SimulateThrottling=false
```

The worker provisions deterministic SharePoint-like folders, synchronizes
existing WorkFlow Hub document records, stores safe external metadata and
Graph-like operation evidence, and preserves tenant isolation. The technical
evidence page is `/technical/sharepoint` and is limited to tenant owners and
administrators.

To demonstrate a controlled retry locally, set
`SharePoint__SimulateThrottling=true`. The first upload attempt for a document
version returns a simulated 429 with a two-second retry instruction; the next
identical attempt succeeds. Normal operation never introduces random failure.

`MicrosoftGraph` mode performs configuration validation only. It makes no live
Microsoft Graph call. A real provider still requires a dedicated tenant,
approved `Sites.Selected` access, worker-only credentials, and explicit owner
approval.
