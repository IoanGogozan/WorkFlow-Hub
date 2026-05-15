# DPIA Screening

This is a preliminary screening document, not a full DPIA and not legal advice.

## System

Norvix WorkFlow Hub.

## Processing Summary

The system processes operational requests, customer/company metadata, document metadata, uploaded files, AI suggestions, review decisions, delivery links, and audit logs for a tenant organization.

## Demo Mode Position

The MVP/demo uses fake data only. No real customer data, real employee data, production accounting data, or sensitive personal data should be used.

## Likely Personal Data

- user names;
- user emails;
- customer contact names;
- customer contact emails;
- phone numbers;
- email body text;
- document metadata;
- IP addresses;
- user agent strings;
- access logs;
- audit events;
- document contents if real documents are later used.

## Purpose

- manage operational intake;
- create and track cases;
- classify and review documents;
- prepare delivery packages;
- provide secure external delivery;
- produce operational dashboards;
- maintain auditability.

## Lawful Basis

For real customers this must be assessed per customer and use case. Likely bases may include contract, legitimate interest, or legal obligation depending on context.

For the demo, use only fictional data.

## Controller/Processor Assessment

Expected real-world model:

- customer: controller;
- Norvix AS: processor when hosting or maintaining the service.

Required before production:

- data processing agreement;
- subprocessor list;
- security measures appendix;
- deletion/export process.

## High-Risk Indicators

Potential risk indicators:

- AI-assisted processing;
- document content may include personal data;
- external delivery links;
- access logs and audit trails;
- multi-tenant architecture;
- integrations with email, SharePoint, accounting/project systems.

Risk reducers:

- human review required;
- no autonomous legal/significant decisions;
- tenant isolation;
- object-level authorization;
- access logging;
- AI disable option;
- mocked integrations during MVP;
- fake demo data.

## Preliminary DPIA Result

For demo with fake data: full DPIA is not required.

For real customer production: complete DPIA screening per customer. A full DPIA may be required if customer data includes sensitive data, large-scale processing, systematic monitoring, or high-impact AI usage.

## Required Mitigations

- tenant isolation tests;
- RBAC and object-level authorization;
- secure delivery token design;
- audit logs;
- retention controls;
- AI output review;
- AI provider logging;
- file upload validation;
- secrets management;
- incident response notes.

## Open Questions Before Production

- Which customer data categories will be processed?
- Will documents contain sensitive or special category data?
- Which Azure region will host data?
- Which subprocessors will be used?
- Will real AI provider calls include document contents?
- What retention period does each customer require?
- What deletion/export workflow is contractually required?
- Does the customer require a full DPIA?
