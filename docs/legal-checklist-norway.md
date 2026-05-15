# Norway Legal Checklist

This checklist is not legal advice. It is a product and engineering checklist for building the demo in a way that is credible for Norwegian customers.

## GDPR and Personopplysningsloven

Norway applies GDPR through the Norwegian Personal Data Act.

Engineering implications:

- privacy by design;
- privacy by default;
- data minimization;
- purpose limitation;
- role-based access;
- tenant isolation;
- audit logging;
- retention controls;
- export/deletion support;
- clear documentation of subprocessors.

## Controller and Processor Model

For real customers:

- the customer is usually `behandlingsansvarlig` / controller for operational and customer data;
- Norvix AS is usually `databehandler` / processor if it hosts, maintains, or processes customer data;
- a data processing agreement / `databehandleravtale` is required before production use;
- subprocessors must be listed.

Likely subprocessors:

- Microsoft/Azure;
- email provider;
- logging/monitoring provider;
- AI provider if real AI is enabled;
- hosting/CDN provider if used.

## DPIA Screening

Maintain `docs/dpia-screening.md`.

Full DPIA may be required if:

- AI processing becomes significant;
- sensitive data is processed;
- large-scale personal data processing is introduced;
- systematic monitoring is introduced;
- automated decisions with legal or similar significant effects are introduced.

MVP position:

- AI is assistive;
- human review is mandatory;
- no autonomous decisions;
- use fake demo data.

## AI Act / KI-loven Direction

Treat AI as assistive workflow support:

- label AI suggestions clearly;
- require human review;
- avoid automated decisions with legal or similar significant effect;
- log model, provider, prompt version, output, confidence, and review result;
- allow AI to be disabled per tenant;
- do not train on customer data without explicit agreement.

## Accessibility / Universell Utforming

Build UI with WCAG 2.1 A/AA principles:

- keyboard navigation;
- visible focus states;
- sufficient contrast;
- semantic HTML;
- labels on form controls;
- clear validation errors;
- screen-reader-friendly status messages.

## Security Expectations

Align with:

- NSM grunnprinsipper for IKT-sikkerhet;
- Digdir internkontroll for informasjonssikkerhet;
- OWASP ASVS Level 2;
- OWASP API Security Top 10.

Minimum controls:

- Entra ID authentication;
- JWT validation;
- server-side RBAC;
- object-level authorization;
- tenant isolation;
- secure headers;
- input validation;
- file upload allowlist;
- rate limiting;
- audit log;
- encryption in transit and at rest;
- secrets in Key Vault;
- dependency scanning;
- backup/restore plan;
- incident response notes.

## Demo Data

Rules:

- use fictional company and person data only;
- do not upload real customer documents;
- do not connect real accounting production accounts;
- do not issue real invoices;
- clearly mark mock integrations;
- document any sample data source.

## Production Readiness Before Real Customer Use

Before real production use, prepare:

- DPA template;
- subprocessor list;
- privacy notice;
- retention policy;
- security architecture review;
- backup and restore test;
- incident response process;
- DPIA screening and, if needed, full DPIA;
- AI provider data processing review;
- penetration test or structured security review.

## References

- Datatilsynet: GDPR and Norwegian Personal Data Act.
- Datatilsynet: Data protection by design and by default.
- Datatilsynet: DPIA guidance.
- Nkom: AI Act / KI-forordningen information.
- Regjeringen: KI-loven hearing information.
- NSM: ICT Security Principles.
- Digdir: information security internal control.
- Uutilsynet: WCAG requirements.
