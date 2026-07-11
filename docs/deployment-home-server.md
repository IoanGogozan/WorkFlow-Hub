# Home Server Deployment

This runbook deploys the fictional WorkFlow Hub demo to
`https://workflow.norvix.no` behind the existing Caddy reverse proxy.

## Boundaries

- Only Caddy publishes host ports 80 and 443.
- The frontend joins the external Docker network `proxy`.
- API, worker, and PostgreSQL share an internal-only network. The API also joins
  `proxy` so Caddy can route only `/api/*` and `/health*` to it; it publishes no
  host port.
- PostgreSQL and demo documents use named Docker volumes.
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

Generate the PostgreSQL password with a local tool such as:

```bash
openssl rand -base64 36
```

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
docker compose --env-file .env -f compose.home-server.yml logs --tail=100 api worker frontend
```

Also complete one browser session from `/demo`, including timeline replay,
calculator, technical evidence, privacy, and terms.

## Backup

Database backup:

```bash
mkdir -p /srv/backups/workflow-hub/db
stamp=$(date +%F-%H%M%S)
docker compose --env-file .env -f compose.home-server.yml exec -T db \
  sh -c 'pg_dump -U "$POSTGRES_USER" -d "$POSTGRES_DB" --format=custom' \
  > "/srv/backups/workflow-hub/db/workflow-hub-$stamp.dump"
```

Document-volume backup:

```bash
mkdir -p /srv/backups/workflow-hub/documents
stamp=$(date +%F-%H%M%S)
docker run --rm \
  -v workflow-hub_document_data:/data:ro \
  -v /srv/backups/workflow-hub/documents:/backup \
  alpine tar -czf "/backup/workflow-hub-$stamp.tar.gz" -C /data .
```

## Rollback

Redeploy a previously validated Git revision, keeping `.env` and named volumes:

```bash
git checkout PREVIOUS_GOOD_REVISION
docker compose --env-file .env -f compose.home-server.yml up -d --build
```

Do not remove or recreate volumes during an application rollback.
