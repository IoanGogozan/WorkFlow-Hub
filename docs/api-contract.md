# API Contract Draft

## API Principles

- All `/api/*` endpoints require authentication except documented health endpoints.
- Public delivery links use random tokens, expiry, revocation, and access logging.
- Tenant context is derived from authenticated membership/session.
- All write operations validate role and object-level authorization.
- All important state changes write audit events.
- Use JSON request/response bodies unless file upload uses multipart form data.

## Health

```http
GET /health
GET /health/ready
```

## Auth and Session

```http
GET /api/me
GET /api/tenants
POST /api/tenants/{tenantId}/switch
```

## Intake

```http
GET /api/intakes
POST /api/intakes
GET /api/intakes/{id}
PATCH /api/intakes/{id}
POST /api/intakes/{id}/attachments
POST /api/intakes/{id}/analyze
POST /api/intakes/{id}/approve-ai
POST /api/intakes/{id}/reject-ai
POST /api/intakes/{id}/convert-to-case
```

## Review Tasks

```http
GET /api/review-tasks
GET /api/review-tasks/{id}
POST /api/review-tasks/{id}/approve
POST /api/review-tasks/{id}/reject
POST /api/review-tasks/{id}/request-changes
```

## Cases

```http
GET /api/cases
POST /api/cases
GET /api/cases/{id}
PATCH /api/cases/{id}
POST /api/cases/{id}/tasks
PATCH /api/cases/{id}/tasks/{taskId}
POST /api/cases/{id}/notes
GET /api/cases/{id}/activity
GET /api/cases/{id}/missing-information
```

## Documents

```http
GET /api/documents
POST /api/documents
GET /api/documents/{id}
POST /api/documents/{id}/versions
POST /api/documents/{id}/analyze
POST /api/documents/{id}/approve-classification
POST /api/documents/{id}/reject-classification
POST /api/documents/{id}/link-to-case
GET /api/documents/{id}/download
```

## Organizations and Brreg

```http
GET /api/organizations/search?query=
GET /api/organizations/brreg/{orgNumber}
POST /api/customers/from-brreg
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
PATCH /api/delivery-packages/{id}
POST /api/delivery-packages/{id}/items
DELETE /api/delivery-packages/{id}/items/{itemId}
POST /api/delivery-packages/{id}/generate-pdf
POST /api/delivery-packages/{id}/create-link
POST /api/delivery-links/{id}/revoke
GET /delivery/{token}
GET /delivery/{token}/documents/{documentId}
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

## Required Authorization Tests

- Tenant A user cannot list Tenant B cases.
- Tenant A user cannot view Tenant B intake item by ID.
- Tenant A user cannot download Tenant B document.
- Viewer cannot edit integrations.
- OperationsUser cannot change tenant privacy settings.
- Expired delivery link returns 410 or 403.
- Revoked delivery link returns 410 or 403.
