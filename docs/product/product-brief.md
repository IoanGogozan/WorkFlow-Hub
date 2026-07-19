# Product Brief: Norvix WorkFlow Hub

## Problem

Technical service companies often have good individual tools but still move information between them manually. A request arrives by email, customer data is checked, a case is created, attachments are stored, project or accounting data is updated, and reporting is prepared. The same identifiers and status changes are copied several times, while evidence is spread across systems.

WorkFlow Hub demonstrates how one bounded service workflow can be coordinated without replacing the systems a company already uses.

## Target Audience

- Technical evaluators reviewing full-stack and integration engineering.
- Norwegian service companies exploring a small workflow-automation pilot.
- Product and engineering teams interested in traceable, human-controlled automation.

## Portfolio Goal

The project demonstrates:

- a coherent business scenario rather than disconnected CRUD screens;
- tenant-scoped backend and data boundaries;
- asynchronous orchestration, retries, and idempotency;
- public, simulated, and self-hosted integration patterns;
- inspectable evidence tied to an exact workflow run;
- honest separation between a portfolio sandbox and production readiness.

## Primary Demo Scenario

A fictional request for service and documentation at a pump station is received. WorkFlow Hub structures the request, checks the organization, creates a case and document, synchronizes a functional SharePoint simulator, sends a signed order to a separate ERP demo receiver, and records audit evidence.

The main public journey should take approximately three minutes. A secondary technical view exposes the detailed run evidence.

## Product Principles

- Lead with the operational problem, not the technology stack.
- Automate a bounded flow rather than claim a universal platform.
- Keep human review visible where interpretation or approval is required.
- Treat failures and retries as product behavior, not hidden implementation detail.
- Label every external boundary according to what the demo actually does.
- Use fictional data only.

## Success Criteria

A visitor should understand within the first screen:

1. what manual process is being improved;
2. what the application does from start to finish;
3. which systems are real, simulated, or represented by a demo receiver;
4. how to run and verify the scenario;
5. that this is a portfolio demonstration rather than a production claim.

## Non-Goals

- Replacing a complete CRM, ERP, document-management, or reporting product.
- Claiming a production Microsoft 365 or accounting integration.
- Processing real customer or employee data in the public demo.
- Claiming measured cost or time savings without a customer pilot.
- Allowing AI output to become an unreviewed final decision.
