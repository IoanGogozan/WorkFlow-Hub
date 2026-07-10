# Product Walkthrough

## Purpose

This document defines two presentation paths:

- a short client-facing integration story for non-technical visitors;
- a secondary technical walkthrough that verifies the broader implementation.

Both paths use fictional data and must describe demo adapters honestly. The
client-facing path is the primary product presentation.

## Client-Facing Walkthrough

Target length: 2–3 minutes.

Current route: `/demo` → `/`.

`/automation` is retained only as a compatibility redirect to `/`. The old
technical overview is available at `/technical`, and `/summary` redirects to
the single result presentation at `/#resultat`.

### 1. Introduce the Manual Problem

Open `/demo`.

Explain that a service request arrives by email and an employee normally needs
to check customer data, create a case, store attachments, update status, and
record what happened across several systems.

Make these boundaries visible:

- all business data is fictional;
- no login is required;
- no real customer systems are contacted;
- the temporary demo workspace expires automatically.

Primary action: **Se automatiseringen**.

### 2. Recognize the Incoming Request

Show the fictional email for service and documentation for pump station 14.
Point out the sender, customer, organization number, customer reference,
attachments, category, and requested outcome.

The visitor should immediately recognize a normal operational request rather
than a software dashboard.

### 3. Show the Manual Process

Show the concrete manual actions: interpret the message, copy customer and
reference data, check Brreg, create a case, create a document structure, save
attachments, update reporting/delivery preparation, and record the work.

The displayed manual-time range is explicitly an example estimate, not a
measured customer result. The savings calculator later in the story is editable.

### 4. Replay the Automated Flow

Select **Kjør automatisert flyt**.

Explain that the timeline replays a completed fictional workflow using evidence
already loaded from the demo workspace. It does not simulate direct calls to
customer systems.

The sequence should show:

1. email received;
2. data structured and validated;
3. company data checked;
4. case created;
5. document structure created;
6. reporting and delivery basis updated;
7. traceability stored.

Each step must state whether its evidence is implemented, public-data capable,
or provided through a demo adapter.

### 5. Explain the Result

Show the actual demo case number, customer, linked-document count, delivery
status, audit-event count, and the remaining human review point.

Use the before/after comparison to explain reduced repeated entry and improved
traceability without promising zero errors or guaranteed savings.

### 6. Use the Transparent Estimate

Change one calculator input and show that the result updates. Keep the disclaimer
visible:

> Eksempelberegning basert på valgte forutsetninger. Faktisk effekt må måles i
> en avgrenset pilot.

### 7. Verify Integration Honesty

Briefly show the integration list:

- email/Outlook as the scenario source;
- Brreg as public-data capable;
- ERP/project and reporting as demo adapters;
- SharePoint/document archive as a demo adapter alongside implemented internal
  document control;
- audit history as implemented.

Open the collapsed technical evidence only if the visitor wants implementation
details.

### 8. Close with One Next Step

Finish at **Har dere en lignende manuell prosess?** Explain that a first pilot
can map one limited workflow and measure its actual effect before expanding.

## Technical Evidence Walkthrough

Target length: up to 5 minutes. Use this path only after the client-facing story
or when the audience specifically requests technical detail.

### 1. Session and Isolation

Start at `/demo`, then open **Tekniske detaljer** or `/technical`. Explain
temporary tenant/user creation, bearer-token session
access, fictional seeding, automatic expiry, and cleanup. Do not describe local
development headers as production authentication.

### 2. Intake and Human-Controlled AI

Open the source intake. Show structured source data, mock AI suggestions, human
approval/editing, and the audit event. AI prepares work but does not make an
autonomous final decision.

### 3. Case Workspace

Open the linked case. Show tasks, notes, customer/reference data, linked
documents, delivery information, and aggregated activity.

### 4. Brreg and Documents

Show Brreg-capable organization enrichment and clearly state whether the current
view uses a deterministic snapshot or a live public lookup. Show document
versioning, classification, approval, and case linking using demo-safe files.

### 5. Integrations and Failure Evidence

Open integration status. Distinguish implemented capabilities from demo adapters.
Where available, show sync history, failure state, and retry without implying a
real Microsoft, accounting, or reporting connection.

### 6. Delivery and Audit

Show the delivery package, generated demo PDF summary, expiring/revocable public
link, recipient page, access logging, and related audit history.

### 7. Architecture and Production Boundary

Summarize the ASP.NET Core, Next.js, PostgreSQL, tenant-scoped data, adapter,
storage, audit, and automated-test foundation. Close by distinguishing the
public demo from a real customer deployment requiring production identity,
secrets, governed integrations, operational controls, and customer legal work.

## Presentation Rules

- Lead with the business process, not the technology stack.
- Keep the main experience to one scenario.
- Never use real customer data or confidential files.
- Never call mock AI or a demo adapter a production integration.
- Do not claim measured savings without a real pilot and publication permission.
- Keep AI optional and human-controlled.
- Use the technical application as evidence, not as the main navigation the
  client must learn.
