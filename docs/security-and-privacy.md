# Security and Privacy

This document is not legal advice. The project should still be designed as if it may later be used by real Norwegian B2B customers.

## Security Baseline

Target:

- OWASP ASVS Level 2;
- OWASP API Security Top 10 awareness;
- NSM ICT Security Principles alignment;
- Digdir information security/internal control alignment.

## Authentication

- Use Microsoft Entra ID / OpenID Connect for production direction.
- Use local dev auth stub only in development.
- Validate JWTs server-side.
- Do not rely on frontend-only authorization.
- All `/api/*` endpoints require auth except documented health endpoints and public delivery-token endpoints.

## RBAC

Roles:

- `TenantOwner`
- `Admin`
- `OperationsUser`
- `Reviewer`
- `Viewer`
- `ExternalRecipient`

Server-side checks are mandatory for:

- tenant administration;
- integration settings;
- document access;
- delivery link creation/revocation;
- privacy settings;
- exports and deletion.

## Tenant Isolation

Absolute rule:

> No tenant-scoped query without tenant context.

Controls:

- derive tenant context from authenticated membership;
- never trust tenant ID from client alone;
- filter every tenant-scoped query by `tenant_id`;
- test direct ID access across tenants;
- audit cross-tenant authorization failures.

Mandatory tests:

- tenant A user cannot see tenant B cases;
- tenant A user cannot view tenant B intake item by ID;
- tenant A user cannot download tenant B documents;
- user without admin role cannot edit integrations;
- expired delivery link does not work;
- revoked delivery link does not work.

## File Upload Security

Controls:

- global request body size limit;
- centralized file size limit;
- allowlist: PDF, PNG, JPG/JPEG;
- validate extension and MIME type;
- generate random blob names;
- store original filename only for display;
- sanitize display filename;
- block executable formats;
- store files outside web root;
- serve files through authorized endpoints;
- demo session cleanup must delete both database records and stored files/blobs;
- deleting a missing demo file during cleanup must be idempotent and must not stop database cleanup;
- public arbitrary upload remains disabled until the deployed environment has explicit upload enablement, user warnings, abuse controls, durable storage cleanup, and malware scanning;
- current public demo mode rejects arbitrary multipart document upload outside Development;
- current Development upload accepts only the configured demo-safe file allowlist and size limit;
- malware scan backlog for production.

## Delivery Links

Controls:

- random high-entropy token;
- store token hash, not raw token;
- expiry required;
- revocation supported;
- access logs;
- rate limiting;
- no tenant admin access through delivery link;
- delivery recipient sees only selected package items.

## Security Headers and Errors

Controls:

- `X-Correlation-ID` response header on all requests;
- accepted client `X-Correlation-ID` values are echoed, used as `HttpContext.TraceIdentifier`, added to logging scope, and written to audit events;
- `X-Content-Type-Options: nosniff`;
- `X-Frame-Options: DENY`;
- `Referrer-Policy: no-referrer`;
- restrictive `Permissions-Policy`;
- restrictive API `Content-Security-Policy`;
- non-Development unhandled exceptions return clean problem responses without stack traces.

## Reverse Proxy and HTTPS

Controls:

- forwarded headers are processed before routing and security middleware;
- forwarded `proto`, `host`, and client IP can be used behind Azure/App Service/Container Apps proxies;
- HTTPS redirection and HSTS can be enabled through deployment configuration;
- unknown proxies are not trusted unless explicitly configured.

## AI Security

AI is assistive only.

Controls:

- documents and emails are untrusted input;
- prompt injection guard;
- structured prompt templates;
- strict output schema validation;
- output saved as suggestion;
- human review before final actions;
- model/provider/prompt version logged;
- confidence stored;
- AI can be disabled per tenant;
- customer data is not used for model training without explicit contractual permission.

## Privacy Principles

Implement GDPR-oriented principles:

- privacy by design;
- privacy by default;
- data minimization;
- purpose limitation;
- role-based access;
- retention policy;
- export support;
- deletion/anonymization support;
- audit log.

## Data Categories

Likely personal data:

- employee names and emails;
- customer contact names and emails;
- phone numbers;
- email body text;
- file metadata;
- IP addresses;
- user agent strings;
- access logs;
- document contents.

Avoid special category data in the demo.

## Retention Defaults

Suggested defaults:

- intake items: 12 months;
- delivery links: 14 or 30 days;
- delivery access logs: 12 months;
- audit events: configurable, longer retention;
- deleted files: soft-delete window, then purge.

## Secrets

- no secrets in repository;
- use user secrets or environment variables locally;
- use Azure Key Vault in cloud;
- rotate secrets after suspected exposure;
- document required configuration in `.env.example`, not real `.env`.

## Observability

Use:

- structured application logs;
- correlation IDs propagated through response headers, logs, clean error responses, and audit events;
- audit events;
- integration sync logs;
- health endpoints;
- Application Insights in cloud.

Do not log:

- access tokens;
- raw delivery tokens;
- passwords;
- unnecessary document contents;
- personal data beyond operational need.

## Incident Response Notes

Document procedures for:

- disabling a tenant;
- revoking delivery links;
- rotating secrets;
- disabling AI provider;
- exporting audit logs for a period;
- identifying affected files;
- notifying customers where contractually required.
