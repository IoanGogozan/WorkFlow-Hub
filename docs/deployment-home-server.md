# Home Server Deployment

This runbook deploys the fictional WorkFlow Hub demo to
`https://workflow.norvix.no` behind the existing Caddy reverse proxy.

The normal release path is the manual GitHub Actions workflow
`.github/workflows/deploy-home-server.yml`. It runs CI, waits for approval in
the protected `home-demo` environment, connects to the server using a dedicated
SSH key, backs up persistent data, deploys the exact workflow commit, runs both
smoke suites, and rolls the application containers back if verification fails.
Azure is a separate, optional reference target and is not used for this site.

Configure these GitHub environment values for `home-demo`:

- secrets: `HOME_SERVER_SSH_PRIVATE_KEY`, `HOME_SERVER_SSH_KNOWN_HOSTS`;
- variables: `HOME_SERVER_HOST`, `HOME_SERVER_SSH_USER`;
- optional variables: `HOME_SERVER_SSH_PORT`, `HOME_SERVER_PROJECT_DIR`,
  `HOME_SERVER_BASE_URL`.

Generate `HOME_SERVER_SSH_KNOWN_HOSTS` from a trusted host-key fingerprint,
not from an unverified connection during deployment. Limit the deploy key on
the server to the deployment account and repository. The account needs Docker
access and write permission for the project and backup directories.

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

## Deployment prerequisites

Before updating an existing deployment:

- obtain explicit owner approval for the exact Git commit;
- confirm that the commit passed CI and contains fictional/demo-safe data only;
- record the currently deployed commit as `PREVIOUS_GOOD_REVISION`;
- require a clean server worktree; and
- create a current backup before changing code or containers.

```bash
cd /srv/projects/workflow-hub
export PREVIOUS_GOOD_REVISION="$(git rev-parse HEAD)"
git status --short
BACKUP_ROOT=/srv/backups/workflow-hub bash scripts/backup-home-server.sh
```

Stop if `git status --short` prints any output or if the backup fails. Keep the
recorded revision and the three matching, timestamped backup files until the
new deployment has been accepted.

## Private environment

For the first deployment, copy the repository to `/srv/projects/workflow-hub`,
then create the private environment file:

```bash
cd /srv/projects/workflow-hub
cp .env.home-server.example .env
chmod 600 .env
nano .env
```

The current `.env` additions are:

```dotenv
ERP_DEMO_SIGNING_SECRET=<64 hexadecimal characters>
SHAREPOINT_MODE=Simulated
SHAREPOINT_SIMULATE_THROTTLING=false
```

Keep the Microsoft/SharePoint Online values empty. Generate separate PostgreSQL
and ERP HMAC secrets on the server:

```bash
openssl rand -base64 36
openssl rand -hex 32
```

Set the generated values as `POSTGRES_PASSWORD` and
`ERP_DEMO_SIGNING_SECRET` in `.env`. Do not commit that file or reuse either
secret elsewhere.

For an existing deployment, add missing keys manually instead of overwriting
`.env`. Restrict the file after editing and verify required values without
printing them:

```bash
chmod 600 .env
test -n "$(sed -n 's/^POSTGRES_PASSWORD=//p' .env)"
test "$(sed -n 's/^ERP_DEMO_SIGNING_SECRET=//p' .env | wc -c)" -eq 65
```

Never run `cat .env`, paste its contents into a report, or enable shell tracing
while handling secrets.

## Persistent volumes

Compose retains three named volumes across rebuilds and application rollbacks:

- `workflow-hub_postgres_data` for PostgreSQL;
- `workflow-hub_document_data` for generated documents; and
- `workflow-hub_erp_receiver_data` for the ERP receiver SQLite database at
  `/data/erp-demo-receiver.db`.

The SQLite volume is part of the backup and restore set. Never run
`docker compose down -v`, and do not delete or recreate these volumes during a
deployment or rollback.

## Pull, validate, build, migrate, and start

For an approved manual deployment outside GitHub Actions, use the same
idempotent release script as CI:

```bash
cd /srv/projects/workflow-hub
bash scripts/deploy-home-server.sh REPLACE_WITH_APPROVED_COMMIT_SHA \
  https://workflow.norvix.no
```

The lower-level commands below document what the script performs and remain
useful for diagnosis.

Set the approved immutable commit, fetch it, and verify the clean checkout:

```bash
cd /srv/projects/workflow-hub
export DEPLOY_REVISION=REPLACE_WITH_APPROVED_COMMIT_SHA
git fetch --prune origin
git checkout --detach "$DEPLOY_REVISION"
test "$(git rev-parse HEAD)" = "$(git rev-parse "$DEPLOY_REVISION")"
test -z "$(git status --porcelain)"
```

Validate Compose and build all application images before changing running
containers:

```bash
docker compose --env-file .env -f compose.home-server.yml config --quiet
docker compose --env-file .env -f compose.home-server.yml build --pull
```

The API applies committed EF Core migrations on startup through
`Database__ApplyMigrationsOnStartup=true`. Start PostgreSQL first, then the API
and wait for its migration/readiness gate before starting the remaining
services:

```bash
docker compose --env-file .env -f compose.home-server.yml up -d db
docker compose --env-file .env -f compose.home-server.yml up -d api
for attempt in $(seq 1 24); do
  if docker compose --env-file .env -f compose.home-server.yml exec -T api \
    bash -c 'exec 3<>/dev/tcp/127.0.0.1/8080; printf "GET /health/ready HTTP/1.1\r\nHost: localhost\r\nX-Forwarded-Proto: https\r\nConnection: close\r\n\r\n" >&3; grep -q "200 OK" <&3'; then
    break
  fi
  test "$attempt" -lt 24 || exit 1
  sleep 5
done
docker compose --env-file .env -f compose.home-server.yml up -d
docker compose --env-file .env -f compose.home-server.yml ps
```

If API readiness does not pass promptly, inspect its logs and roll back. Do not
start repeated migration attempts blindly.

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

## Health and smoke verification

```bash
curl --fail --silent --show-error https://workflow.norvix.no/health >/dev/null
curl --fail --silent --show-error https://workflow.norvix.no/health/ready >/dev/null
curl --fail --silent --show-error https://workflow.norvix.no/health/version | jq .
bash scripts/smoke-home-server.sh https://workflow.norvix.no
bash scripts/smoke-verifiable-demo.sh https://workflow.norvix.no
docker compose --env-file .env -f compose.home-server.yml ps
docker compose --env-file .env -f compose.home-server.yml logs --tail=100 api worker frontend erp-receiver
```

After the normal smoke passes, optionally verify controlled ERP retry and
idempotency. This creates another fictional demo run:

```bash
bash scripts/smoke-verifiable-demo.sh https://workflow.norvix.no --fail-once
```

Also complete one browser session from `/demo`, including timeline replay,
calculator, technical evidence, privacy, and terms.

Record `git rev-parse HEAD` as the deployed revision only after both smoke modes
and the browser check pass. Reports may contain commit IDs and check outcomes,
but never `.env` values, bearer tokens, HMAC signatures, or secrets.

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

Rollback is an application redeploy, not a data restore. Use the previously
recorded, validated revision while keeping `.env` and all named volumes:

```bash
cd /srv/projects/workflow-hub
git checkout --detach "$PREVIOUS_GOOD_REVISION"
docker compose --env-file .env -f compose.home-server.yml config --quiet
docker compose --env-file .env -f compose.home-server.yml build
docker compose --env-file .env -f compose.home-server.yml up -d
bash scripts/smoke-home-server.sh https://workflow.norvix.no
bash scripts/smoke-verifiable-demo.sh https://workflow.norvix.no
```

Do not remove or recreate volumes during an application rollback. If a database
migration is not backward-compatible with the previous application revision,
stop the services and follow `scripts/restore-home-server.md` using the three
matching pre-deployment backup files. A data restore is destructive and must be
explicitly approved; never improvise a reverse migration.
