# Norvix ERP demo receiver

Norvix ERP demo receiver — fictional integration target.

This standalone ASP.NET Core service represents an ERP system owned by a
customer. It is intentionally separated from the WorkFlow Hub API and receives
only fictional demo payloads.

The service exposes:

- `GET /health` for liveness;
- `GET /health/ready` for readiness.
- `POST /api/demo-orders` for signed fictional ERP messages.

`POST /api/demo-orders` requires `X-Norvix-Timestamp` (Unix seconds),
`X-Norvix-Signature` (hex HMAC-SHA256), and `Idempotency-Key`. Configure the
shared secret through `ErpDemoReceiver__SigningSecret`; never store it in the
database or commit it to source control.

For controlled retry demonstrations only, set
`ErpDemoReceiver__EnableFailOnce=true` and send `X-Demo-Fail-Once: true`. The
first attempt is persisted and returns `503`; the next identical request
completes the same receipt. The feature is disabled by default.

Receipts are stored in a separate SQLite database. Configure its persistent
location with `ConnectionStrings__ErpDemoReceiver`; the default is
`Data Source=data/erp-demo-receiver.db`. A named Docker volume will mount this
path when the service is added to Compose.

In production it is intended for private container-network access. It does not
require a host or public port; the WorkFlow Hub worker will call it by its
internal service address.
