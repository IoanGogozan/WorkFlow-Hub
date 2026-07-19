# Home Server Deployment Quickstart

This is the operational checklist for updating `https://workflow.norvix.no`.
Use it for every release. The detailed architecture, first-time setup, Caddy,
restore, and migration notes remain in [Home Server Deployment](deployment-home-server.md).

## How deployment works

```text
developer workstation
  -> push and merge to main
  -> GitHub-hosted CI passes
  -> trusted workstation connects to the server over LAN SSH
  -> server backup + exact Git commit + Docker Compose + smoke tests
  -> Caddy continues to expose only ports 80 and 443
```

The application was originally deployed this way. GitHub Actions did not
publish the home-server version. Do not expose SSH publicly and do not install
a repository runner on this server merely to deploy this public repository.

## Release rules

- Deploy only a commit already merged into `main` with successful CI.
- Use only fictional/demo-safe data.
- Run from a trusted workstation connected to the home LAN.
- Never print or copy the server `.env` file.
- Never use `docker compose down -v`.
- Stop if the server worktree is dirty, a backup fails, or a required secret is
  missing.
- Application rollback is automatic; database restore is always manual and
  requires explicit approval.

## 1. Select and validate the release

On the workstation:

```powershell
git switch main
git pull --ff-only origin main
$DeployRevision = git rev-parse HEAD
gh run list --commit $DeployRevision --limit 3
```

The CI run for `$DeployRevision` must show `success`. Record the full SHA in the
deployment notes.

## 2. Inspect the server without changing it

Connect using the workstation's SSH alias or the known LAN address:

```bash
ssh server@HOME_SERVER_LAN_IP
cd /srv/projects/workflow-hub

git rev-parse HEAD
git status --short
docker compose --env-file .env -f compose.home-server.yml ps
df -h /srv

for command in git docker curl jq flock; do
  command -v "$command" >/dev/null || echo "missing: $command"
done
docker compose version

test -n "$(sed -n 's/^POSTGRES_PASSWORD=//p' .env)" \
  && echo "POSTGRES_PASSWORD set" || echo "POSTGRES_PASSWORD missing"
test -n "$(sed -n 's/^ERP_DEMO_SIGNING_SECRET=//p' .env)" \
  && echo "ERP_DEMO_SIGNING_SECRET set" || echo "ERP_DEMO_SIGNING_SECRET missing"

curl --fail --silent --show-error \
  https://workflow.norvix.no/health/ready >/dev/null
```

Do not continue if `git status --short` prints anything. Save and review a
patch first:

```bash
stamp="$(date -u +%Y-%m-%dT%H%M%SZ)"
mkdir -p "/srv/backups/workflow-hub/pre-deploy-$stamp"
git diff --binary > "/srv/backups/workflow-hub/pre-deploy-$stamp/server-worktree.patch"
```

Only restore a modified tracked file after confirming that its change is
already present in the target commit or is no longer required.

## 3. Run the deployment

On the server, replace the placeholder with the approved full SHA:

```bash
cd /srv/projects/workflow-hub
DEPLOY_REVISION=REPLACE_WITH_APPROVED_MAIN_COMMIT

git fetch --prune origin main
git cat-file -e "$DEPLOY_REVISION^{commit}"
git show "$DEPLOY_REVISION:scripts/deploy-home-server.sh" \
  > /tmp/workflow-hub-deploy.sh
chmod 700 /tmp/workflow-hub-deploy.sh

PROJECT_DIR=/srv/projects/workflow-hub \
BACKUP_ROOT=/srv/backups/workflow-hub \
bash /tmp/workflow-hub-deploy.sh \
  "$DEPLOY_REVISION" \
  https://workflow.norvix.no

rm /tmp/workflow-hub-deploy.sh
```

The script performs these operations in order:

1. checks required commands, Docker Compose, worktree, and required secrets;
2. records the previous Git revision;
3. backs up PostgreSQL, documents, and ERP receiver SQLite data;
4. fetches `origin/main` and checks out the approved commit detached;
5. validates Compose and builds all four application images;
6. recreates services without deleting named volumes and removes orphans;
7. waits for health checks;
8. runs the basic and verifiable-demo smoke suites;
9. verifies that `/health/version` reports the approved commit;
10. rebuilds the previous application revision automatically if a deployment
    or smoke check fails.

## 4. Verify acceptance

From the workstation:

```powershell
curl.exe --fail --silent --show-error `
  https://workflow.norvix.no/health/version
curl.exe --fail --silent --show-error `
  https://workflow.norvix.no/health/ready
```

On the server:

```bash
cd /srv/projects/workflow-hub
git rev-parse HEAD
git status --short
docker compose --env-file .env -f compose.home-server.yml ps
docker compose --env-file .env -f compose.home-server.yml \
  logs --since 10m --no-color api worker frontend erp-receiver
```

Acceptance requires:

- `/health/version` reports the approved full SHA and `HomeServer`;
- readiness returns HTTP 200;
- API, frontend, ERP receiver, and database are healthy;
- worker is running;
- the server worktree is clean;
- both smoke suites passed;
- recent logs contain no unexpected fatal or unhandled errors.

## If deployment stops after backup

First inspect; do not repeat commands blindly:

```bash
cd /srv/projects/workflow-hub
git rev-parse HEAD
git status --short
docker compose --env-file .env -f compose.home-server.yml ps
curl --silent --show-error --write-out '\nHTTP %{http_code}\n' \
  https://workflow.norvix.no/health/version
```

If the old SHA is still checked out and all old containers are healthy, the
application was not replaced. Correct the reported fetch, checkout, permission,
or preflight error and restart the documented deployment once.

## Rollback and restore boundary

The deployment script keeps `.env` and named volumes and automatically
rebuilds the previous application revision after a failure. It does not revert
database migrations.

If the previous application cannot use the migrated database, stop and follow
[Restore procedure](../scripts/restore-home-server.md) using the matching
pre-deployment PostgreSQL, document, and ERP receiver backups. A restore changes
persistent data and must never be launched automatically from an error trap.
