# Product Walkthrough

## Goal

Show the public interactive demo of Norvix WorkFlow Hub: a visitor starts an isolated demo workspace, uses fictional data, completes the main workflow, and sees how intake, AI-assisted review, case handling, documents, delivery, audit, and integrations fit together.

Target length: 5 minutes.

This walkthrough is for product evaluation and sales presentation. It must stay aligned with the actual browser UI and must not claim that mock adapters are production integrations.

## Setup

Public demo tenant:

- Created per demo session from fictional seed data.
- Expires automatically.

Reference customer:

- Agder Drift & Service AS or equivalent fictional Norwegian service company.

Reference case:

- Incoming service request with attachments and incomplete customer/document metadata.

## Positioning

"Norvix WorkFlow Hub is a workflow control layer for organizations that already use Microsoft 365, document libraries, accounting/project tools, and reporting dashboards. This public demo shows the workflow with fictional data in a temporary sandbox."

Public demo boundaries:

- Demo data is fictional and expires automatically.
- Visitors should not upload personal or confidential information.
- AI behavior is demo-safe and mock-backed unless explicitly configured otherwise.
- Microsoft Graph/SharePoint, accounting, and Power BI/Fabric integrations are mock adapters.
- Brreg lookup can be shown as a real-capable integration where implemented.
- Delivery links are functional; production PDF rendering remains a hardening item until implemented.

## Step 1 - Start Demo

Open `/demo`.

Show:

- public demo notice;
- fictional data warning;
- automatic expiry message;
- `Start demo workspace` button.

Message:

"Each visitor gets a temporary sandbox. The demo is interactive, but it is not a production customer environment."

## Step 2 - Dashboard

Show:

- new intakes;
- waiting review;
- missing information;
- documents needing review;
- integration failures;
- cases ready for delivery.

Message:

"This is the operational control layer. It shows what entered the workflow, what needs review, what is blocked, and what is ready for delivery."

## Step 3 - Intake Inbox

Open or create an intake.

Show:

- source;
- subject/body;
- attachments when available;
- status `New`.

Message:

"Requests can arrive from manual entry, email/form adapters, or API. The first value is that they become structured and trackable."

## Step 4 - AI Suggestions

Run AI analysis.

Show suggestions:

- customer;
- organization number;
- category;
- urgency;
- tasks;
- document metadata;
- missing information;
- summary.

Message:

"AI prepares the work, but it does not decide. Suggestions stay pending until a person approves or edits them."

## Step 5 - Human Review

Approve or edit suggestions.

Show:

- proposed fields;
- changed values;
- status change;
- audit event when visible.

Message:

"This is AI-assisted administration with human control and auditability."

## Step 6 - Convert to Case

Convert intake to case.

Show:

- case title and status;
- case fields;
- tasks;
- notes;
- linked documents;
- delivery section;
- activity.

Message:

"The approved intake becomes a case workspace, not another spreadsheet row."

## Step 7 - Brreg Lookup

Search Bronnoysundregistrene by organization number or name.

Show:

- selected company data;
- organization number;
- organization form;
- municipality/address;
- deleted status;
- customer creation/enrichment.

Message:

"Norwegian company data can be looked up and stored with source traceability."

## Step 8 - Document Workflow

Select a sample document or use the demo-safe document input. Do not use personal or confidential files in the public demo.

Show:

- document version;
- AI classification;
- human approval;
- expiry metadata;
- linked case.

Message:

"Documents are no longer just files in a folder. They become reviewed, linked, versioned case evidence."

## Step 9 - Workflow Readiness

Show the case readiness checklist.

Message:

"The system shows what is blocking delivery instead of requiring someone to remember it manually."

## Step 10 - Integration Dashboard

Show connector states:

- Brreg;
- Microsoft Graph/SharePoint adapter;
- accounting/project adapter;
- Power BI/Fabric export adapter.

Message:

"Each integration has visible status, sync history, failure handling, and retry. Mock adapters are labelled until real provider credentials are configured."

## Step 11 - Delivery Package

Generate a delivery package and create a delivery link.

Show:

- selected documents;
- expiring secure link;
- external recipient page;
- access log;
- revoke action.

Message:

"The recipient gets a controlled delivery link, not a loose email thread with attachments."

## Step 12 - Audit and Analytics

Show:

- audit events;
- case status metrics;
- export option.

Message:

"Management gets operational visibility, and the organization gets a traceable record of what happened."

## Closing Message

"Norvix WorkFlow Hub is being prepared as a public interactive demo: functional, honest about mock integrations, and safe to try with fictional data. The later production SaaS work is separate: real authentication, real provider credentials, durable storage, governed AI, customer contracts, and operational runbooks."
