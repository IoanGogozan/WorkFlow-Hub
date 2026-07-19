# API Contract

Status markers:

- `Implemented` means the endpoint exists in the current backend.
- `Planned` means the endpoint belongs to the general product backlog and is
  not implemented yet. These entries are not the active demo delivery plan;
  see [Verifiable Integration Demo](verifiable-integration-demo.md).

## API Principles

- All `/api/*` endpoints require authentication except documented health endpoints.
- Public delivery links use random tokens, expiry, revocation, and access logging.
- Tenant context is derived from authenticated membership/session.
- All write operations validate role and object-level authorization.
- All important state changes write audit events.
- Use JSON request/response bodies unless file upload uses multipart form data.
- Responses include `X-Correlation-ID`. Clients may send `X-Correlation-ID`; valid values are echoed and written to audit/log context.

## Health

```http
GET /health
GET /health/ready
GET /health/version
```

## Auth and Session

```http
POST /api/demo-sessions
GET /api/me
GET /api/tenants
POST /api/tenants/{tenantId}/switch
```

`POST /api/demo-sessions` is public in Demo mode. It creates an isolated temporary demo tenant, demo user, tenant membership, fictional seed data, and returns the raw bearer token once.

```json
{
  "sessionId": "guid",
  "demoTenantId": "guid",
  "token": "random-demo-session-token",
  "expiresAt": "2026-05-17T12:00:00Z"
}
```

Authenticated public demo API requests use:

```http
Authorization: Bearer <demo-session-token>
```

Rules:

- the raw token is never stored server-side; only a hash is stored;
- the token resolves tenant/user context server-side;
- client-provided tenant headers are rejected outside Development;
- expired demo sessions return `401 Unauthorized`;
- public demo mode blocks arbitrary multipart document upload and uses `POST /api/documents/sample`.

## Intake

```http
GET /api/intakes
POST /api/intakes
GET /api/intakes/{id}
POST /api/intakes/{id}/analyze
POST /api/intakes/{id}/approve-ai
POST /api/intakes/{id}/reject-ai
POST /api/intakes/{id}/convert-to-case
```

Planned:

```http
PATCH /api/intakes/{id}
POST /api/intakes/{id}/attachments
```

## Review Tasks

```http
GET /api/review-tasks
```

Planned:

```http
GET /api/review-tasks/{id}
POST /api/review-tasks/{id}/approve
POST /api/review-tasks/{id}/reject
POST /api/review-tasks/{id}/request-changes
```

## Cases

```http
GET /api/cases
GET /api/cases/{id}
POST /api/cases/{id}/tasks
POST /api/cases/{id}/notes
GET /api/cases/{id}/activity
```

Planned:

```http
POST /api/cases
PATCH /api/cases/{id}
PATCH /api/cases/{id}/tasks/{taskId}
GET /api/cases/{id}/missing-information
```

## Documents

```http
GET /api/documents
POST /api/documents
POST /api/documents/sample
GET /api/documents/{id}
GET /api/documents/{id}/download
POST /api/documents/{id}/versions
POST /api/documents/{id}/analyze
POST /api/documents/{id}/approve-classification
POST /api/documents/{id}/link-to-case
```

Planned:

```http
POST /api/documents/{id}/reject-classification
```

## Organizations and Brreg

```http
GET /api/organizations/search?query=
GET /api/organizations/brreg/{orgNumber}
POST /api/customers/from-brreg
```

Planned:

```http
POST /api/customers/{id}/refresh-brreg
```

## Integrations

```http
GET /api/integrations
GET /api/integrations/{provider}
POST /api/integrations/{provider}/connect
POST /api/integrations/{provider}/disconnect
POST /api/integrations/{provider}/sync
GET /api/integrations/{provider}/sync-runs
POST /api/integrations/{provider}/sync-runs/{syncRunId}/retry
```

## Delivery

```http
POST /api/cases/{id}/delivery-packages
GET /api/delivery-packages/{id}
POST /api/delivery-packages/{id}/generate-pdf
POST /api/delivery-packages/{id}/create-link
POST /api/delivery-links/{id}/revoke
GET /delivery/{token}
GET /delivery/{token}/documents/{documentId}
```

Planned:

```http
PATCH /api/delivery-packages/{id}
POST /api/delivery-packages/{id}/items
DELETE /api/delivery-packages/{id}/items/{itemId}
```

## Analytics

```http
GET /api/metrics/overview
GET /api/metrics/cases
GET /api/metrics/intakes
GET /api/metrics/documents
GET /api/metrics/integrations
GET /api/metrics/export.csv
GET /api/metrics/export.json
```

## Admin

```http
GET /api/admin/users
POST /api/admin/users
PATCH /api/admin/users/{id}
GET /api/admin/audit-events
GET /api/admin/privacy/settings
PATCH /api/admin/privacy/settings
POST /api/admin/demo-data/export
POST /api/admin/demo-data/delete
```

## Error Format

Use a consistent problem details response:

```json
{
  "type": "https://httpstatuses.com/403",
  "title": "Forbidden",
  "status": 403,
  "detail": "The current user does not have access to this tenant resource.",
  "traceId": "00-..."
}
```

In non-Development environments, unhandled exceptions return a clean `application/problem+json` response without stack traces. The problem `instance` and the `X-Correlation-ID` response header use the same correlation ID.

## Required Authorization Tests

- Tenant A user cannot list Tenant B cases.
- Tenant A user cannot view Tenant B intake item by ID.
- Tenant A user cannot download Tenant B document.
- Viewer cannot edit integrations.
- OperationsUser cannot change tenant privacy settings.
- Expired delivery link returns 410 or 403.
- Revoked delivery link returns 410 or 403.
