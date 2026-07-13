# WorkFlow Hub - Verifiable Integration Demo
## Continuation plan, based on the current implemented state

> **Status: ACTIVE — single source of truth for future implementation.**
> Adopted on 2026-07-12. See [Plan registry](plans.md) for the status of older
> directions. Replaced implementation plans were removed to avoid ambiguity;
> implemented technical references remain available where useful.

**Repository:** `IoanGogozan/WorkFlow-Hub`

**Recommended branch:** `agent/verifiable-demo`

**Public domain:** `https://workflow.norvix.no`
**Main constraint:** the public demo uses controlled, verifiable integration boundaries.

---

# 1. Product decision

The demo must remain simple for a non-technical visitor, but every important result must be verifiable by a visitor who wants more detail.

The public experience should have two levels.

## Level 1 — Short commercial experience

The visitor sees:

```text
Mottatt → Kontrollert → Opprettet → Synkronisert
```

At completion:

```text
Fullført på 4,8 sekunder

✓ Brreg kontrollert
✓ Sak opprettet
✓ PDF generert
✓ SharePoint-simulator synkronisert
✓ ERP demo receiver mottok data
✓ Hendelser lagret
```

Primary actions:

- `Kjør live demo`
- `Se hva som faktisk ble opprettet`
- `Beskriv prosessen deres`

The visitor must not be required to read a long manual-process description before starting.

## Level 2 — Verifiable run evidence

A dedicated page for the exact run:

```text
/technical/live-runs/{runId}
```

This page shows:

- the fictional incoming request;
- Brreg lookup mode and duration;
- the case created by this run;
- the PDF created by this run;
- the SharePoint simulator folder, file, version/eTag, and operations;
- the ERP receiver request and receipt;
- the audit timeline;
- retry attempts;
- correlation and idempotency evidence;
- links to open the actual internal case and document.

The public page stays short. The evidence page provides depth.

---

# 2. Integration boundary

## 2.1 Real, free external integration

### Brreg

Use the existing live Brreg integration.

It must show:

- `Live` when the external request succeeded;
- `Fallback` when the stored snapshot was used;
- lookup duration;
- source timestamp;
- selected public organization data.

Never show fallback as live.

## 2.2 Real internal application operations

The live run creates real records in WorkFlow Hub:

- run and run steps;
- intake/request;
- customer;
- case;
- PDF;
- document and version;
- delivery basis;
- audit events.

These records must be openable from the result/evidence page.

## 2.3 Functional SharePoint simulator

Keep the current local SharePoint simulator.

It must remain explicitly labelled:

> Lokal SharePoint-simulator — ingen Microsoft 365-konto er tilkoblet.

The simulator must demonstrate real integration behavior inside the self-hosted environment:

- deterministic folder creation;
- document synchronization;
- metadata;
- version and eTag behavior;
- operation log;
- idempotency;
- restricted-site 403;
- stale eTag 412;
- optional throttle 429 and retry;
- cleanup.

Do not add Microsoft Graph packages, Entra registration, Microsoft tenant secrets, or SharePoint subscription requirements.

## 2.4 Real self-hosted ERP demo receiver

Build a separate local service that behaves like an external ERP API.

It must demonstrate:

- real HTTP request from the WorkFlow Hub worker;
- HMAC verification;
- timestamp validation;
- payload validation;
- idempotency key;
- persistent receipt;
- unique external receipt ID;
- fail-once mode;
- retry without duplicates.

It must be labelled:

> Norvix ERP demo receiver

It must never be labelled Tripletex, Visma, PowerOffice, or another commercial ERP.

## 2.5 Fictional request source

The incoming email/request remains fictional.

Label it:

> Fiktiv henvendelse

Do not imply that Outlook or a mailbox is connected.

No public text input, upload, URL, organization-number input, or email address is required for the MVP.

---

# 3. Current state to preserve

The following are already implemented and must not be rebuilt:

- demo session and isolated tenant;
- bearer-token public demo authentication;
- persistent live-demo runs and steps;
- run worker and polling frontend;
- internal artifact creation;
- Brreg live lookup with honest fallback;
- SharePoint simulator;
- simulator operation persistence;
- `/technical/sharepoint`;
- existing technical case/document/delivery pages;
- cleanup worker;
- tenant isolation tests;
- current `/live-preview`.

Known missing pieces:

- run-specific evidence endpoint;
- run-specific evidence page;
- result links to actual records;
- final concise public route;
- ERP demo receiver;
- HMAC and idempotency;
- fail-once/retry demonstration;
- final CI and deployed smoke path.

---

# 4. Final route structure

After completion:

```text
/demo
```

Creates the isolated demo session and redirects to `/`.

```text
/
```

Short live demo:

- hero;
- four compact stages;
- run result;
- evidence button;
- contact CTA;
- collapsed secondary explanation.

```text
/technical/live-runs/{runId}
```

Detailed evidence for one run.

```text
/technical
```

Broader application overview.

```text
/technical/sharepoint
```

Global/tenant SharePoint simulator view.

Existing routes remain:

- `/cases/{id}`;
- `/documents/{id}`;
- `/delivery-packages/{id}`;
- `/integrations`.

The old replay page should move to:

```text
/technical/legacy-story
```

or be removed after route compatibility is confirmed.

---

# 5. Codex operating rules

Apply these rules to every task.

1. Work only on `agent/verifiable-demo`.
2. Confirm branch with `git status -sb` before editing.
3. Implement exactly one numbered task per implementation iteration, unless the
   user explicitly groups tasks.
4. Stop after the task.
5. Do not push directly to `main`.
6. Do not merge.
7. Do not deploy or SSH unless an explicit deployment task is provided.
8. Do not add Microsoft Graph, Entra, or real SharePoint integration.
9. Do not add secrets to Git.
10. Do not rename a simulator as live.
11. Do not accept arbitrary public data.
12. Preserve tenant isolation.
13. Do not perform unrelated refactoring.
14. Do not upgrade unrelated dependencies.
15. Add/update tests for changed behavior.
16. Run the exact verification commands in the task.
17. Report:
    - branch;
    - summary;
    - exact files changed;
    - migration;
    - tests and results;
    - assumptions;
    - remaining risks;
    - suggested commit.
18. Stop and wait for review.

---

# Phase 0 — Branch, baseline, and plan alignment

## Task 0.1 — Create continuation branch and align the demo direction

### Goal

Record that the final demo uses free/self-hosted evidence instead of paid Microsoft services.

### Actions

Create and switch to:

```text
agent/verifiable-demo
```

Create:

```text
docs/verifiable-integration-demo.md
```

Document:

- two-level UX;
- Brreg live;
- internal real artifacts;
- SharePoint simulator;
- self-hosted ERP receiver;
- controlled self-hosted integration boundaries;
- route structure;
- evidence requirements;
- non-goals;
- definition of done.

Update:

```text
docs/sharepoint-simulator-amendment.md
docs/plans.md
README.md
```

Keep only one active implementation plan. Preserve the SharePoint document as
an implemented technical reference, not as a competing plan.

### Verification

```bash
git status -sb
git diff --check
```

### Acceptance

- branch is not `main`;
- no production code changed;
- documentation clearly states SharePoint is simulated;
- ERP receiver is self-hosted;
- the integration boundary is stated clearly.

### Suggested commit

```text
docs: define verifiable demo direction
```

---

## Task 0.2 — Record current baseline

### Goal

Confirm the current implementation is green before continuation.

### Run

```bash
npm --prefix frontend ci
npm --prefix frontend run lint
npm --prefix frontend run build
npm --prefix frontend audit --omit=dev --audit-level=high

dotnet test backend/NorvixHub.sln --configuration Release -nr:false

dotnet tool restore --tool-manifest dotnet-tools.json
dotnet tool run dotnet-ef -- migrations has-pending-model-changes \
  --project backend/src/NorvixHub.Infrastructure/NorvixHub.Infrastructure.csproj \
  --startup-project backend/src/NorvixHub.Api/NorvixHub.Api.csproj \
  --configuration Release

docker compose config --quiet
docker compose --env-file .env.home-server.example \
  -f compose.home-server.yml config --quiet
```

Record exact results in:

```text
docs/current-implementation-status.md
```

### Acceptance

- no dependency upgrade;
- no hidden failure;
- unavailable command recorded as `not run`.

### Suggested commit

```text
docs: record verifiable demo continuation baseline
```

---

# Phase 1 — Run-specific evidence API

## Task 1.1 — Define evidence contracts

### Goal

Create a stable response that can prove what one run created.

### New folder

```text
backend/src/NorvixHub.Contracts/LiveDemoEvidence/
```

### Add contracts

```text
LiveDemoEvidenceResponse
LiveDemoEvidenceRunResponse
LiveDemoEvidenceRequestResponse
LiveDemoEvidenceBrregResponse
LiveDemoEvidenceCaseResponse
LiveDemoEvidenceDocumentResponse
LiveDemoEvidenceSharePointResponse
LiveDemoEvidenceSharePointOperationResponse
LiveDemoEvidenceErpResponse
LiveDemoEvidenceAuditEventResponse
LiveDemoEvidenceLinksResponse
```

### Required shape

#### Run

- run ID;
- status;
- correlation ID shortened;
- created/started/completed timestamps;
- total duration;
- retry count;
- fictional scenario label.

#### Request

- title;
- fictional body;
- customer reference;
- source label;
- created timestamp.

#### Brreg

- mode: `live` or `fallback`;
- organization number;
- organization name;
- lookup duration;
- source/update timestamp;
- safe status message.

#### Case

- case number;
- title;
- status;
- customer name;
- created timestamp;
- `caseHref`.

#### Document

- document ID;
- title;
- filename;
- size;
- content type;
- version number;
- shortened hash if available;
- created timestamp;
- `documentHref`;
- `downloadHref`.

#### SharePoint simulator

- mode: `simulated`;
- site name;
- library name;
- folder path;
- shortened folder ID;
- shortened file ID;
- version;
- eTag;
- metadata summary;
- operation list;
- `technicalSharePointHref`.

#### ERP

Initially nullable/unavailable:

- mode;
- status;
- external receipt ID;
- idempotency key shortened;
- attempts;
- last duration;
- safe error.

#### Audit

Ordered events:

- timestamp;
- event type;
- actor/operation label;
- entity type;
- result;
- correlation ID shortened.

#### Links

- case;
- document;
- download;
- delivery package;
- SharePoint technical page;
- integration dashboard.

### Security

Do not expose:

- database connection;
- storage path;
- access token;
- settings JSON;
- user agent;
- IP;
- HMAC signature/secret;
- raw exceptions;
- raw SharePoint payload.

### Tests

Create contract serialization/shape tests.

### Verification

```bash
dotnet test backend/tests/NorvixHub.ContractTests/NorvixHub.ContractTests.csproj \
  --configuration Release -nr:false
```

### Suggested commit

```text
feat: define live run evidence contracts
```

---

## Task 1.2 — Implement tenant-scoped evidence endpoint

### Route

```http
GET /api/live-demo-runs/{runId}/evidence
Authorization: Bearer <demo token>
```

### New file

```text
backend/src/NorvixHub.Api/Endpoints/LiveDemoEvidenceEndpoints.cs
```

### Requirements

- require tenant context;
- run must belong to tenant;
- load exact artifact IDs saved on `LiveDemoRun`;
- never select the first or latest generic case/document;
- load exact Brreg information associated with the run;
- load exact SharePoint simulator folder/file/operations associated with the run;
- load exact audit events related to run/artifacts/correlation;
- order operations and audit events by timestamp;
- return 404 for another tenant;
- use `AsNoTracking`;
- return public-safe values only.

### Update

Register endpoint in:

```text
backend/src/NorvixHub.Api/Program.cs
```

### Required integration tests

- own run returns 200;
- exact case from run returned;
- exact document from run returned;
- SharePoint simulator operation evidence returned;
- audit ordered;
- another tenant receives 404;
- unauthenticated receives 401;
- no settings/secrets/paths/IP returned;
- missing artifact is represented safely, not a 500.

### Verification

```bash
dotnet test backend/tests/NorvixHub.IntegrationTests/NorvixHub.IntegrationTests.csproj \
  --configuration Release -nr:false --filter LiveDemoEvidence
```

### Suggested commit

```text
feat: expose tenant-scoped live run evidence
```

---

## Task 1.3 — Add evidence links to live run result

### Goal

The result API should provide one obvious evidence URL.

### Changes

Extend:

```text
LiveDemoRunResultResponse
```

with:

```text
EvidenceHref
CaseHref
DocumentHref
DocumentDownloadHref
SharePointEvidenceHref
AuditHref
```

Prefer server-created relative links.

Update endpoint mapping and tests.

### Acceptance

Completed run response includes:

```text
/technical/live-runs/{runId}
```

and actual record links.

### Verification

```bash
dotnet test backend/NorvixHub.sln --configuration Release -nr:false
```

### Suggested commit

```text
feat: link live run results to verifiable records
```

---

# Phase 2 — Detailed evidence page

## Task 2.1 — Add frontend evidence types and loader

### New files

```text
frontend/src/lib/live-demo-evidence.ts
frontend/src/app/technical/live-runs/[runId]/page.tsx
frontend/src/components/live-demo-evidence/live-demo-evidence-page.tsx
```

### Requirements

- use existing demo token API helper;
- load `/api/live-demo-runs/{runId}/evidence`;
- redirect expired/no session using current behavior;
- loading state;
- public-safe error state;
- use existing technical shell conventions.

At this task, render a basic structured JSON-free summary only.

### Verification

```bash
npm --prefix frontend run lint
npm --prefix frontend run build
```

### Suggested commit

```text
feat: add live run evidence route
```

---

## Task 2.2 — Build evidence overview header

### New component

```text
frontend/src/components/live-demo-evidence/evidence-overview.tsx
```

### Show

- `Kjøringsbevis`;
- run ID shortened;
- status;
- start/completion;
- total duration;
- retry count;
- fictional-data badge;
- correlation ID shortened;
- button back to live demo.

### Requirements

- no raw GUID wall;
- copy button may copy run ID only;
- clear “Fiktive data” label.

### Verification

```bash
npm --prefix frontend run lint
npm --prefix frontend run build
```

### Suggested commit

```text
feat: show live run evidence overview
```

---

## Task 2.3 — Add request and Brreg evidence

### New components

```text
request-evidence-card.tsx
brreg-evidence-card.tsx
```

### Request card

- source: fictional request;
- title;
- customer reference;
- timestamp;
- short body;
- no implication of Outlook connection.

### Brreg card

Show:

- Live or Fallback badge;
- organization number;
- organization name;
- duration;
- source timestamp;
- explanation of fallback if used.

### Acceptance

A visitor can distinguish live Brreg from fallback immediately.

### Suggested commit

```text
feat: show request and Brreg evidence
```

---

## Task 2.4 — Add case and document evidence

### New components

```text
case-evidence-card.tsx
document-evidence-card.tsx
```

### Case

- case number;
- title;
- status;
- customer;
- creation time;
- button `Åpne saken`.

### Document

- title;
- filename;
- size;
- type;
- version;
- shortened hash/reference;
- creation time;
- buttons:
  - `Åpne dokumentdetaljer`;
  - `Åpne demo-PDF`.

### Requirements

- buttons point to actual records generated by this run;
- download/open endpoint remains tenant protected;
- no local file path.

### Suggested commit

```text
feat: expose created case and PDF evidence
```

---

## Task 2.5 — Add SharePoint simulator evidence

### New components

```text
sharepoint-simulator-evidence.tsx
sharepoint-operation-table.tsx
```

### Header

```text
Lokal SharePoint-simulator
Ingen Microsoft 365-konto er tilkoblet
```

### Show

- simulated site;
- library;
- folder path/tree;
- folder ID shortened;
- file ID shortened;
- filename;
- version;
- eTag;
- selected sanitized metadata.

### Operation history

Columns:

- time;
- method;
- Graph-like route/action;
- result status;
- duration;
- attempt;
- idempotency result.

Examples:

```text
POST  /children       201 Created
PUT   /content        201 Created
PATCH /listItem       200 OK
GET   /children       200 OK
```

### Action

Button:

```text
Åpne full simulatorvisning
```

links to `/technical/sharepoint`.

### Requirements

- no external fake URL made clickable;
- no “live Microsoft” badge;
- show simulator operation evidence from the exact run.

### Suggested commit

```text
feat: show run-specific SharePoint simulator evidence
```

---

## Task 2.6 — Add audit timeline

### New component

```text
audit-evidence-timeline.tsx
```

### Show

Chronological events:

- run queued;
- run started;
- Brreg started/completed;
- customer created/reused;
- case created;
- PDF generated/stored;
- SharePoint simulator steps;
- ERP later;
- run completed/failed;
- retry events.

Each event:

- timestamp;
- action;
- provider;
- result;
- duration if applicable;
- attempt;
- shortened correlation ID.

### Requirements

- collapsed to first 8–12 events if very long;
- `Vis alle hendelser`;
- no raw payload.

### Suggested commit

```text
feat: add verifiable run audit timeline
```

---

## Task 2.7 — Evidence-page E2E

### New file

```text
frontend/e2e/live-run-evidence.spec.ts
```

### Test flow

1. create demo session;
2. start fresh run;
3. wait for completed;
4. click evidence link;
5. verify same case number;
6. open case;
7. return;
8. verify document and PDF actions;
9. verify SharePoint simulator label;
10. verify at least one operation;
11. verify audit events;
12. verify no Microsoft-live claim.

### Verification

```bash
npm --prefix frontend run test:e2e -- live-run-evidence.spec.ts
```

### Suggested commit

```text
test: verify run-specific evidence journey
```

### Review gate

Stop here and manually review whether evidence is credible before ERP work.

---

# Phase 3 — Simplify the public page

## Task 3.1 — Create final compact live page structure

### Goal

Reduce required reading.

### Update

```text
frontend/src/components/live-demo/live-demo-preview-page.tsx
frontend/src/components/live-demo/live-demo-hero.tsx
frontend/src/components/live-demo/live-demo-run-panel.tsx
frontend/src/components/live-demo/live-demo-stage-card.tsx
```

### Primary page order

1. hero;
2. compact four-stage strip;
3. run panel/result;
4. CTA;
5. collapsed details.

### Remove from required flow

- old incoming-email card;
- nine manual-action cards;
- old replay timeline;
- large before/after table;
- calculator before result;
- broad integration list before result.

Do not delete components yet.

### Hero content

```text
Live integrasjon med fiktive data

Fra henvendelse til sak, dokument og systemoppdatering

Se en ny kjøring bli kontrollert, opprettet og synkronisert i Norvix sitt
selvhostede demomiljø.
```

Actions:

- `Kjør live demo`;
- `Beskriv prosessen deres`.

Trust line:

```text
Brreg kan kontrolleres live. SharePoint vises i en funksjonell lokal simulator.
ERP-integrasjonen kjøres mot en separat Norvix demo receiver.
```

Until ERP is implemented, capability wording must say it is unavailable.

### Acceptance

A visitor understands the purpose in 10 seconds.

### Suggested commit

```text
refactor: shorten live demo public journey
```

---

## Task 3.2 — Make result cards interactive

### Update

```text
live-demo-result-card.tsx
```

### Result items

Render compact cards:

#### Brreg

- Live/Fallback;
- duration.

#### Case

- case number;
- `Åpne saken`.

#### PDF

- filename;
- `Åpne PDF`.

#### SharePoint simulator

- synchronized;
- `Se simulatorbevis`.

#### ERP receiver

- unavailable until implemented;
- later receipt ID.

#### Audit

- event count;
- `Se hendelseslogg`.

Main button:

```text
Se hva som faktisk ble opprettet
```

links to run evidence page.

### Requirements

- do not show “simulated adapter” as a long English sentence;
- all labels Norwegian;
- clear simulator badge;
- contact CTA visible at all times.

### Suggested commit

```text
feat: make live demo results verifiable
```

---

## Task 3.3 — Move secondary explanations into details

### Update

```text
live-demo-details.tsx
```

Collapsed sections:

- `Hva ble automatisert?`
- `Hvordan beregnes mulig tidsbesparelse?`
- `Hva er ekte og hva er simulert?`
- `Tekniske detaljer`

Reuse existing manual process/calculator where useful.

### Requirements

- collapsed by default;
- accessible `<details>/<summary>`;
- no important CTA hidden inside;
- no repeated text.

### Suggested commit

```text
refactor: move explanatory content behind optional details
```

---

## Task 3.4 — Promote live page to `/`

### Preconditions

- evidence page approved;
- result links work;
- public page reviewed at 375/768/1280;
- current live run stable.

### Changes

- `/` renders final live page;
- `/live-preview` redirects to `/`;
- old replay moves to `/technical/legacy-story`;
- `/automation` redirects to `/`;
- `/demo` session creation redirects to `/`;
- `/technical` remains unchanged.

### Tests

- no redirect loops;
- expired token redirects to `/demo`;
- legacy technical route works;
- result/evidence route works.

### Suggested commit

```text
refactor: promote verifiable live demo to root
```

---

# Phase 4 — Self-hosted ERP demo receiver

## Task 4.1 — Add ERP receiver project

### New project

```text
backend/src/NorvixHub.ErpDemoReceiver/
```

Add to solution.

### Requirements

- ASP.NET Core minimal API;
- `/health`;
- `/health/ready`;
- no main API project dependency;
- no external port required in production;
- clear README/description:
  `Norvix ERP demo receiver — fictional integration target`.

### Verification

```bash
dotnet build backend/NorvixHub.sln --configuration Release
```

### Suggested commit

```text
feat: add self-hosted ERP demo receiver
```

---

## Task 4.2 — Add ERP receipt model and persistence

### Preferred storage

SQLite with a named Docker volume.

This creates real separation from the WorkFlow Hub PostgreSQL database.

### Model

```text
ErpDemoReceipt
```

Fields:

- ID;
- external receipt ID;
- idempotency key;
- payload hash;
- customer reference;
- case number;
- document reference;
- received timestamp;
- attempt count;
- fail-once state.

### Requirements

- unique idempotency key;
- persists after container restart;
- no real personal data;
- no secrets in DB.

### Tests

- insert;
- retrieve;
- duplicate;
- restart/persistence where practical.

### Suggested commit

```text
feat: persist ERP demo receipts
```

---

## Task 4.3 — Implement signed receive endpoint

### Route

```http
POST /api/demo-orders
```

### Required headers

```text
X-Norvix-Timestamp
X-Norvix-Signature
Idempotency-Key
```

### Signature

```text
HMAC-SHA256(secret, timestamp + "." + rawBody)
```

### Behavior

- validate timestamp skew;
- validate HMAC with constant-time comparison;
- validate payload;
- first request returns `201`;
- same key/same payload returns `200` with same receipt;
- same key/different payload returns `409`;
- invalid signature returns `401`.

### Response

```json
{
  "receiptId": "ERP-DEMO-...",
  "status": "Received",
  "duplicate": false,
  "receivedAt": "..."
}
```

### Tests

All paths above.

### Suggested commit

```text
feat: receive signed idempotent ERP demo messages
```

---

## Task 4.4 — Add deterministic fail-once behavior

### Goal

Demonstrate retry.

### Header

```text
X-Demo-Fail-Once: true
```

### Behavior

When enabled by configuration:

- first request for key returns `503`;
- failure marker persists;
- second request succeeds;
- later duplicate returns same receipt;
- no duplicate row.

### Tests

- first 503;
- second 201;
- third 200 duplicate;
- disabled mode does not allow fail-once.

### Suggested commit

```text
feat: support controlled ERP failure demonstration
```

---

## Task 4.5 — Add main-app ERP client

### New files

```text
backend/src/NorvixHub.Application/LiveDemo/IErpDemoClient.cs
backend/src/NorvixHub.Application/LiveDemo/ErpDemoRequest.cs
backend/src/NorvixHub.Application/LiveDemo/ErpDemoResult.cs
backend/src/NorvixHub.Infrastructure/LiveDemo/ErpDemoClient.cs
backend/src/NorvixHub.Infrastructure/LiveDemo/ErpDemoOptions.cs
```

### Requirements

- use HttpClient;
- create canonical JSON;
- sign request;
- use run ID-derived idempotency key;
- optional fail-once flag from run;
- timeout;
- map response safely;
- no secret/signature logging.

### Tests

Fake HTTP handler:

- 201;
- duplicate 200;
- 401;
- 409;
- 503;
- timeout.

### Suggested commit

```text
feat: call signed ERP demo receiver
```

---

## Task 4.6 — Integrate ERP into run processor

### Internal step

```text
erp-received
```

### Requirements

- after internal case/document and SharePoint simulator;
- payload includes:
  - fictional customer reference;
  - case number;
  - document reference;
  - run ID;
- store receipt ID;
- store attempt count and duration;
- on fail-once, run becomes Failed;
- retry continues from ERP;
- completed prior steps are not repeated;
- no duplicate case/document/folder/file/receipt.

### Required tests

- normal success;
- fail once;
- retry success;
- duplicate-safe;
- worker restart;
- another tenant cannot see receipt;
- raw signature absent from audit/log response.

### Suggested commit

```text
feat: integrate ERP receiver with safe retry
```

---

## Task 4.7 — Add ERP evidence to endpoint and page

### Backend evidence

Return:

- receiver mode: self-hosted;
- status;
- receipt ID;
- attempts;
- duration;
- idempotency-key shortened;
- failure/retry history.

### Frontend

Card:

```text
Norvix ERP demo receiver
Melding mottatt
Kvittering: ERP-DEMO-...
Forsøk: 2
```

If fail-once occurred:

```text
Første forsøk feilet kontrollert. Ny kjøring fullførte uten duplikater.
```

### Suggested commit

```text
feat: show ERP receiver and retry evidence
```

---

# Phase 5 — Compose, backup, and local full-stack tests

## Task 5.1 — Add receiver to development Compose

### Update

```text
docker-compose.yml
```

### Requirements

- receiver internal network;
- local development port only if necessary and documented;
- health check;
- SQLite volume;
- generated development HMAC value only through `.env`, not committed.

### Validation

```bash
docker compose config --quiet
docker compose up -d db erp-receiver
```

### Suggested commit

```text
feat: run ERP receiver in local Compose
```

---

## Task 5.2 — Add receiver to home-server Compose

### Update

```text
compose.home-server.yml
.env.home-server.example
docs/deployment-home-server.md
```

### Requirements

- no public `ports`;
- internal network only;
- health check;
- named SQLite volume;
- worker gets receiver URL and secret;
- receiver gets same secret;
- secret empty in example;
- worker waits for receiver healthy when ERP enabled.

### Validation

```bash
docker compose --env-file .env.home-server.example \
  -f compose.home-server.yml config --quiet
```

### Suggested commit

```text
feat: configure self-hosted ERP receiver deployment
```

---

## Task 5.3 — Add backup and restore instructions

### Add/update scripts

```text
scripts/backup-home-server.sh
scripts/restore-home-server.md
```

Back up:

- PostgreSQL;
- document volume;
- ERP SQLite volume/file.

### Requirements

- no secrets in backup logs;
- timestamped backup;
- restore order;
- do not use `down -v`.

### Suggested commit

```text
docs: back up ERP demo receiver state
```

---

## Task 5.4 — Full E2E normal run

### Test

- start demo session;
- run fresh flow;
- Brreg uses fake HTTP in CI;
- internal case/PDF created;
- SharePoint simulator completes;
- ERP receiver receives;
- result contains evidence link;
- evidence page matches run;
- open case/PDF;
- receipt shown.

### Suggested commit

```text
test: cover full verifiable integration success
```

---

## Task 5.5 — Full E2E failure and retry

### Test

- start run with fail-once;
- wait for Failed;
- previous completed evidence remains;
- click retry;
- wait for Completed;
- same case number;
- same document;
- same SharePoint IDs;
- one ERP receipt;
- attempts = 2;
- audit shows fail and retry.

### Suggested commit

```text
test: cover ERP failure and idempotent retry
```

---

# Phase 6 — CI and deployed smoke

## Task 6.1 — Add dedicated CI E2E job

### CI services

- PostgreSQL;
- API;
- worker;
- frontend;
- ERP receiver.

Use:

- fake Brreg;
- SharePoint simulator;
- real local ERP receiver.

### Required checks

- normal run;
- fail/retry;
- evidence page;
- tenant isolation.

### Suggested commit

```text
ci: validate verifiable integration demo
```

---

## Task 6.2 — Add deployed smoke script

### New script

```text
scripts/smoke-verifiable-demo.sh
```

### Flow

1. create demo session;
2. create live run;
3. poll;
4. assert completed;
5. request evidence;
6. assert case/document;
7. assert SharePoint mode is `simulated`;
8. assert ERP receipt;
9. assert audit;
10. do not print bearer token;
11. delete temporary files.

Optional flag:

```text
--fail-once
```

tests retry path.

### Suggested commit

```text
test: add deployed verifiable demo smoke
```

---

## Task 6.3 — Capability-driven public copy

### Goal

Never claim unavailable capabilities.

Capabilities should expose:

- Brreg live enabled;
- SharePoint simulator enabled;
- ERP receiver enabled;
- failure demo enabled.

Public trust line generated from capabilities:

```text
Brreg: live ved tilgjengelig tjeneste
SharePoint: lokal simulator
ERP: separat selvhostet demo receiver
```

If ERP is disabled, hide ERP success claim.

### Suggested commit

```text
feat: align public claims with enabled capabilities
```

---

## Task 6.4 — Accessibility and responsive pass

Verify:

- 375;
- 768;
- 1280;
- keyboard;
- screen-reader status;
- reduced motion;
- focus after start/retry;
- evidence-page tables usable on mobile;
- simulator/live status not color-only.

### Suggested commit

```text
fix: polish verifiable demo accessibility
```

---

## Task 6.5 — Final release gate

Run:

```bash
npm --prefix frontend ci
npm --prefix frontend run lint
npm --prefix frontend run build
npm --prefix frontend audit --omit=dev --audit-level=high

dotnet restore backend/NorvixHub.sln
dotnet build backend/NorvixHub.sln --configuration Release --no-restore
dotnet test backend/NorvixHub.sln --configuration Release --no-build -nr:false

dotnet tool restore --tool-manifest dotnet-tools.json
dotnet tool run dotnet-ef -- migrations has-pending-model-changes \
  --project backend/src/NorvixHub.Infrastructure/NorvixHub.Infrastructure.csproj \
  --startup-project backend/src/NorvixHub.Api/NorvixHub.Api.csproj \
  --configuration Release

docker compose config --quiet
docker compose --env-file .env.home-server.example \
  -f compose.home-server.yml config --quiet
docker compose --env-file .env.home-server.example \
  -f compose.home-server.yml build

npm run test:e2e:public-demo
```

### Acceptance

- all tests pass;
- no pending migration;
- no high production vulnerability;
- no secrets;
- external dependencies are explicitly identified;
- SharePoint clearly simulated;
- ERP receiver internal only;
- normal and retry paths pass;
- exact-run evidence is accessible;
- public page is short.

### Suggested commit

```text
chore: verify integration demo release
```

---

# Phase 7 — Deployment

## Task 7.1 — Prepare deployment runbook only

Do not deploy.

Update runbook with:

- backup;
- `.env` additions;
- ERP HMAC generation;
- SQLite volume;
- pull/build/migrate/start;
- health;
- smoke;
- rollback.

Generate secret server-side:

```bash
openssl rand -hex 32
```

Do not print it in reports.

### Suggested commit

```text
docs: finalize demo deployment runbook
```

---

## Task 7.2 — Deploy only after explicit approval

This requires a separate owner instruction.

Deployment checklist:

1. PR reviewed and merged;
2. current server backup;
3. clean server Git state;
4. ERP HMAC stored in `.env`;
5. pull exact commit;
6. validate Compose;
7. build;
8. migrate;
9. start;
10. wait for health;
11. run normal smoke;
12. run fail-once smoke;
13. verify browser;
14. record deployed commit;
15. preserve rollback commit.

Never run:

```text
docker compose down -v
```

---

# 6. What must not be built

Do not add:

- Microsoft Graph;
- SharePoint Online;
- Microsoft 365 subscription;
- Outlook integration;
- real Tripletex;
- real Power BI;
- Azure subscription resources;
- paid monitoring;
- paid email service;
- generic workflow builder;
- AI/chatbot;
- public uploads;
- customer onboarding;
- billing;
- more public scenarios;
- more dashboards.

The proof should come from:

- a new run;
- real Brreg call;
- actual WorkFlow Hub records;
- functional SharePoint simulator;
- real HTTP request to the separate ERP receiver;
- failure/retry evidence;
- exact-run drill-down.

---

# 7. Definition of done

The project is complete when a visitor can:

1. understand the demo in 10 seconds;
2. start a new run;
3. see four compact stages;
4. see live/fallback Brreg status;
5. see a new case and PDF;
6. see SharePoint simulator clearly labelled;
7. see ERP receipt from a separate service;
8. deliberately trigger an ERP failure;
9. retry successfully;
10. verify no duplicate case/document/SharePoint item/receipt;
11. open `Se hva som faktisk ble opprettet`;
12. inspect the exact case, document, simulator operations, receipt, and audit;
13. contact Norvix without reading optional details.

The project is technically complete when:

- tenant isolation passes;
- evidence endpoint passes;
- HMAC and idempotency tests pass;
- normal and retry E2E pass;
- cleanup and backup are documented;
- root is the concise live demo;
- old replay is not the main sales page;
- integration dependencies are documented;
- no false live Microsoft claim exists.

---

# 8. Standard prompt for each Codex task

```text
Implement Task [NUMBER] — [TASK NAME] from
docs/verifiable-integration-demo.md.

Mandatory rules:
- First run `git status -sb`.
- Work only on branch `agent/verifiable-demo`.
- Implement exactly this task.
- Do not start later tasks.
- Do not push to main or merge.
- Do not deploy or SSH.
- Do not add Microsoft Graph.
- Keep SharePoint explicitly simulated.
- Keep all public data fictional.
- Preserve tenant isolation.
- Do not expose secrets, raw errors, IPs, file paths, settings JSON, signatures, or tokens.
- Do not perform unrelated refactors or dependency upgrades.
- Add/update the tests required by this task.
- Run the exact verification commands.
- At completion report:
  1. branch;
  2. implementation summary;
  3. exact files changed;
  4. migration name if any;
  5. commands/tests and results;
  6. assumptions;
  7. remaining risks;
  8. suggested commit.
- Stop after this task.
```

---

# 9. First prompt to send to Codex

```text
Implement Task 0.1 — Create continuation branch and align the demo direction.

Use the approved continuation plan for WorkFlow Hub.

Mandatory rules:
- Run `git status -sb` first.
- Create and switch to `agent/verifiable-demo`.
- Do not work on main.
- Create `docs/verifiable-integration-demo.md`.
- Document:
  - short public page plus detailed run evidence;
  - Brreg live/fallback;
  - real internal records;
  - functional local SharePoint simulator;
  - separate self-hosted ERP demo receiver;
  - controlled self-hosted integration boundaries;
  - route plan;
  - evidence requirements;
  - non-goals;
  - definition of done.
- Keep `docs/plans.md`, `docs/sharepoint-simulator-amendment.md`, and
  `README.md` aligned with the active plan.
- Do not change production code.
- Do not install packages.
- Do not deploy or SSH.
- Run:
  - `git diff --check`
  - `git status -sb`
- Report exact files changed and stop.
```

---

# 10. Recommended execution order

Give Codex one task at a time:

```text
0.1 → 0.2
1.1 → 1.2 → 1.3
2.1 → 2.2 → 2.3 → 2.4 → 2.5 → 2.6 → 2.7
3.1 → 3.2 → 3.3 → 3.4
4.1 → 4.2 → 4.3 → 4.4 → 4.5 → 4.6 → 4.7
5.1 → 5.2 → 5.3 → 5.4 → 5.5
6.1 → 6.2 → 6.3 → 6.4 → 6.5
7.1
```

Only after review and explicit approval:

```text
7.2 deployment
```
