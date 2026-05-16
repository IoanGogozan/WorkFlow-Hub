# Public Demo Implementation Plan - Draft

Temporary document. Delete this file when the public demo implementation is finished and the durable docs have been updated.

## Target

Norvix WorkFlow Hub should become a public interactive demo for Norvix AS.

This is not a production SaaS release. The public demo should let a website visitor test a complete workflow with fictional data in an isolated sandbox, without creating a real customer account and without uploading real customer data.

## Product Scope

The demo is considered functionally complete when a visitor can:

- open the demo from the Norvix website;
- start an isolated demo workspace;
- see fictional seeded data;
- create a new intake request;
- run mock AI analysis;
- approve an AI suggestion;
- convert the intake into a case or project;
- select or attach a safe demo document;
- classify the document;
- create a delivery package;
- open a public delivery link;
- view audit trail and integration status.

Out of scope for this phase:

- real Tripletex integration;
- real Microsoft Graph integration;
- real Microsoft Fabric or Power BI integration;
- billing;
- real customer onboarding;
- production Entra ID login;
- real customer files;
- multi-tenant commercial SaaS operations.

## Recommended Demo Model

Use a resettable demo session sandbox.

Each visitor starts a temporary workspace:

- `demo_session_id`;
- `demo_tenant_id`;
- `expires_at`;
- random bearer token stored only as a hash server-side.

The backend should clone or seed fictional data for that tenant. The session should expire automatically, preferably after 24 hours for a public demo. A longer 7 day expiry can be used only if useful for sales follow-up, but that increases cleanup and privacy exposure.

Avoid a shared public editable tenant because users can interfere with each other and because it makes audit and cleanup less clear.

## Architecture Direction

Preferred public hosting target:

- frontend and backend: Azure App Service or Azure Container Apps;
- database: Azure Database for PostgreSQL Flexible Server;
- storage: Azure Blob Storage;
- secrets: Azure Key Vault;
- logs and telemetry: Application Insights;
- CI/CD: GitHub Actions;
- DNS: `demo.norvix.no`.

Initial deployment can stay simple:

- Next.js frontend on Azure App Service or Vercel;
- .NET backend on Azure App Service;
- PostgreSQL on Azure;
- Blob Storage on Azure.

Enterprise hardening can follow later:

- Azure Container Apps;
- managed identities;
- Key Vault integration everywhere;
- private networking.

## Implementation Phases

## Phase A - Public Demo Architecture

Objective: create a safe public demo mode with isolated temporary sessions.

Deliverables:

- `demo_sessions` persistence;
- `POST /api/demo-sessions`;
- demo session token generation and hashing;
- demo tenant creation from template or seed;
- demo user and membership creation;
- seeded fictional intakes, cases, documents, integrations, and delivery examples;
- bearer-token demo auth;
- local dev auth allowed only in Development;
- rejection of `X-Norvix-Tenant-Id` and `X-Norvix-User-Id` outside Development;
- session expiry handling;
- cleanup worker for expired demo tenants;
- frontend `/demo` start page;
- frontend token storage in `sessionStorage`;
- API client support for `Authorization: Bearer <demo-session-token>`;
- visible demo banner.

Acceptance criteria:

- a visitor can start a new isolated demo workspace without login;
- each session can access only its own demo tenant;
- expired sessions are rejected;
- frontend shows a clear expired-session state and can start a new demo;
- no public deployed environment trusts local development auth headers.

## Phase B - Complete Visible Demo Flow

Objective: make the visitor workflow work end to end from the browser.

Required flow:

1. Start demo.
2. Open dashboard.
3. Create intake.
4. Run mock AI analysis.
5. Approve suggestion.
6. Convert to case.
7. Select or attach demo document.
8. Classify document.
9. Approve classification.
10. Create delivery package.
11. Generate PDF.
12. Create public link.
13. Open public link.
14. Review audit trail.

Acceptance criteria:

- no step requires manual database edits or API calls outside the UI;
- every meaningful action creates audit events;
- integration status is visible and honestly marked as mock where applicable;
- demo data is fictional and tenant-scoped.

## Phase C - Real Simple PDF

Objective: replace placeholder delivery output with a real generated PDF.

PDF contents:

- package title;
- case number;
- customer name;
- document list;
- generated timestamp;
- delivery link ID;
- Norvix footer.

Acceptance criteria:

- delivery package can generate a downloadable PDF;
- PDF content reflects the selected package data;
- generated PDF does not claim to be a legally binding production document.

## Phase D - Public Hardening

Objective: make the public demo safe enough for internet exposure.

Backend controls:

- HTTPS only in deployed environments;
- secure headers;
- rate limiting for `POST /api/demo-sessions`;
- rate limiting for public delivery links;
- request body size limits;
- upload file size and type limits;
- centralized validation;
- ProblemDetails-style errors;
- no stack traces in public responses;
- correlation IDs;
- structured logging;
- no secrets or bearer tokens in logs;
- tenant/session isolation tests;
- delivery tokens random, expiring, and revocable;
- health endpoint without sensitive details.

Frontend controls:

- no secrets in frontend code;
- clean error states;
- no stack traces shown to users;
- visible public demo disclaimer;
- basic accessibility;
- no fake claims about real integrations.

Database controls:

- migrations applied through controlled process;
- backup enabled for hosted database;
- cleanup job for expired sessions;
- useful indexes on `tenant_id`, `expires_at`, and `token_hash`.

Storage controls:

- private containers;
- access through backend;
- short-lived SAS tokens only if needed;
- automatic cleanup for expired demo files.

CI/CD controls:

- frontend build;
- backend tests;
- lint or format checks;
- dependency audit where practical;
- migration check;
- deploy only from `main` or release tags;
- environment variables from secret store.

## Phase E - Public Deploy

Objective: publish the interactive demo under a clear demo domain.

Recommended domain:

- `demo.norvix.no`

Deployment checklist:

- environment mode set to Demo/Public;
- demo session auth enabled;
- local dev headers rejected;
- database migrations applied;
- cleanup worker active;
- rate limits active;
- privacy and terms pages linked;
- logs and health checks visible in Application Insights;
- smoke test completes the full workflow.

## Demo Session Data Model

Recommended table:

```text
demo_sessions
  id
  tenant_id
  token_hash
  created_at
  expires_at
  last_seen_at
  status
  ip_hash
  user_agent_hash
```

Optional event table:

```text
demo_session_events
  id
  demo_session_id
  event_type
  created_at
```

Notes:

- store only token hashes, never raw demo tokens;
- avoid storing raw IP address unless there is a documented reason;
- use hashed or aggregated telemetry where possible;
- status should support at least `Active`, `Expired`, and `Deleted` or equivalent.

## API Contract Draft

Create demo session:

```http
POST /api/demo-sessions
```

Response:

```json
{
  "sessionId": "guid",
  "demoTenantId": "guid",
  "token": "random-demo-session-token",
  "expiresAt": "2026-05-17T12:00:00Z"
}
```

Authenticated demo API requests:

```http
Authorization: Bearer <demo-session-token>
```

Rules:

- token resolves tenant and demo user server-side;
- client-provided tenant IDs are not trusted;
- expired token returns `401` or `403` with a stable error code;
- frontend handles the stable error code by showing "Demo session expired".

## Frontend Requirements

Add route:

```text
/demo
```

The page should state:

- this is a public demo of Norvix WorkFlow Hub;
- all business data is fictional;
- visitors must not upload personal or confidential information;
- demo data expires automatically.

Primary action:

```text
Start demo workspace
```

Global banner:

```text
Public demo - fictional data - expires automatically
```

Suggested visible badges:

- `Demo mode`;
- `Mock AI`;
- `Mock accounting integration`;
- `Real Brreg-capable integration` only where accurate.

## Legal And Privacy Pages

Add or update:

- `/privacy`;
- `/terms`;
- contact information for Norvix AS;
- data deletion note.

The pages should explain:

- demo data is fictional;
- technical logs may include session ID, timestamps, user agent, and network metadata;
- demo sessions expire and are deleted;
- confidential or personal uploads are not allowed;
- Norvix AS is controller for public demo visitor telemetry;
- the demo is not a production customer environment.

This document is not legal advice. Final text should be reviewed before public launch.

## Upload Policy

Preferred first public version:

- do not allow arbitrary upload;
- provide "Use sample document";
- optionally allow text-only mock document input.

If public upload is enabled:

- maximum 2-5 MB;
- allow only PDF, PNG, JPG/JPEG;
- delete files when the session expires;
- show a clear warning before upload;
- do not use uploaded files for AI training;
- keep malware scanning as a production backlog item unless implemented now.

## AI Policy

Preferred first public version:

- use deterministic mock AI;
- label suggestions as `Demo AI suggestion`;
- do not send visitor-uploaded files to a real model.

If real AI is enabled later:

- use sample documents only by default;
- minimize logged prompt and output data;
- document provider and processing behavior;
- keep human review before any state-changing action;
- support an environment flag such as `REAL_AI_ENABLED=false`.

## Test Plan

Required tests:

- demo session can access only its tenant;
- expired demo session is rejected;
- local dev headers are rejected outside Development;
- public delivery expired links are rejected;
- public delivery revoked links are rejected;
- cleanup removes expired demo tenants and related data;
- demo session token hash is stored, raw token is not stored;
- full visible workflow succeeds with seeded demo data.

## Documentation Updates Before Final Launch

When implementation is complete, update durable docs and remove this draft:

- `README.md`;
- `docs/security-and-privacy.md`;
- `docs/architecture.md`;
- `docs/api-contract.md`;
- `docs/current-implementation-status.md`;
- public demo acceptance checklist if one is created later;
- `infra/terraform/environments/demo/README.md`.

The durable docs should clearly distinguish:

- Development mode;
- Public demo mode;
- planned Production/SaaS mode.

## Final Definition Of Done

The public demo phase is complete when:

- `demo.norvix.no` or the chosen demo URL is live;
- a new visitor can complete the full workflow without help;
- all data is fictional or explicitly demo-safe;
- demo session data expires and is cleaned up;
- local dev auth is unavailable in deployed demo mode;
- privacy and terms pages are visible;
- public endpoints are rate-limited;
- logs contain enough detail for debugging without exposing secrets;
- the README and public UI honestly describe mock vs real capabilities;
- this temporary document has been deleted.
