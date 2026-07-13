# Home Server Deployment

This runbook deploys the fictional WorkFlow Hub demo to
`https://workflow.norvix.no` behind the existing Caddy reverse proxy.

## Boundaries

- Only Caddy publishes host ports 80 and 443.
- The frontend joins the external Docker network `proxy`.
- API, worker, ERP demo receiver, and PostgreSQL share an internal-only network. The API also joins
  `proxy` so Caddy can route only `/api/*` and `/health*` to it; it publishes no
  host port.
- The ERP receiver publishes no host port and is reachable only by its internal
  service name from the worker.
- PostgreSQL, demo documents, and ERP receipts use named Docker volumes.
- Real secrets live only in `/srv/projects/workflow-hub/.env`.
- The deployment is demo-only and must not receive real customer data.

## Server Layout

```text
/srv/projects/workflow-hub/
  compose.home-server.yml
  .env
  backend/
  frontend/
  scripts/

/srv/proxy/sites/workflow.norvix.no.caddy
/srv/backups/workflow-hub/
```

## DNS

Create this record in Cloudflare and use **DNS only** for initial certificate
issuance:

```text
Type: A
Name: workflow
Target: current home public IPv4 address
```

Only router ports 80 and 443 should forward to `192.168.50.23`.

## First Deployment

Copy the repository to `/srv/projects/workflow-hub`, then create the private
environment file:

```bash
cd /srv/projects/workflow-hub
cp .env.home-server.example .env
chmod 600 .env
nano .env
```

Generate separate PostgreSQL and ERP HMAC secrets with a local tool such as:

```bash
openssl rand -base64 36
openssl rand -hex 32
```

Set the generated values as `POSTGRES_PASSWORD` and
`ERP_DEMO_SIGNING_SECRET` in `.env`. Do not commit that file or reuse either
secret elsewhere.

Validate and start:

```bash
docker compose --env-file .env -f compose.home-server.yml config --quiet
docker compose --env-file .env -f compose.home-server.yml up -d --build
docker compose --env-file .env -f compose.home-server.yml ps
```

## Caddy

Create `/srv/proxy/sites/workflow.norvix.no.caddy`:

```caddy
workflow.norvix.no {
    encode zstd gzip

    handle /api/* {
        reverse_proxy workflow-hub-api:8080
    }

    handle /health* {
        reverse_proxy workflow-hub-api:8080
    }

    handle {
        reverse_proxy workflow-hub-frontend:3000
    }
}
```

Validate and reload Caddy:

```bash
docker compose -f /srv/proxy/compose.yml exec -T caddy \
  caddy validate --config /etc/caddy/Caddyfile
docker compose -f /srv/proxy/compose.yml exec -T caddy \
  caddy reload --config /etc/caddy/Caddyfile
```

## Verification

```bash
bash scripts/smoke-home-server.sh https://workflow.norvix.no
docker compose --env-file .env -f compose.home-server.yml ps
docker compose --env-file .env -f compose.home-server.yml logs --tail=100 api worker frontend erp-receiver
```

Also complete one browser session from `/demo`, including timeline replay,
calculator, technical evidence, privacy, and terms.

## Backup

`scripts/backup-home-server.sh` creates a PostgreSQL custom-format dump and
compressed copies of the document and ERP receiver volumes. It briefly stops
the ERP receiver for a consistent SQLite archive, prevents overlapping
executions, and keeps the most recent 14 days of backups.

For a server-wide backup location, run it with a user permitted to write under
`/srv/backups/workflow-hub`:

```bash
BACKUP_ROOT=/srv/backups/workflow-hub bash scripts/backup-home-server.sh
```

If the deployment user cannot write to `/srv/backups`, use this application
directory as a temporary local destination:

```bash
BACKUP_ROOT=/srv/projects/workflow-hub/backups bash scripts/backup-home-server.sh
```

Schedule the same command daily with the deployment user's crontab. Backups on
the same server do not protect against disk loss; copy the backup root to an
external destination before treating the deployment as production-grade.

Follow `scripts/restore-home-server.md` to restore one matching timestamp in
the required database, document, and ERP receiver order. Never use
`docker compose down -v` as part of backup, restore, or application rollback.

## Rollback

Redeploy a previously validated Git revision, keeping `.env` and named volumes:

```bash
git checkout PREVIOUS_GOOD_REVISION
docker compose --env-file .env -f compose.home-server.yml up -d --build
```

Do not remove or recreate volumes during an application rollback.
