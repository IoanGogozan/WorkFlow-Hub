#!/usr/bin/env bash
set -Eeuo pipefail

PROJECT_DIR="${PROJECT_DIR:-/srv/projects/workflow-hub}"
ENV_FILE="${ENV_FILE:-$PROJECT_DIR/.env}"
COMPOSE_FILE="$PROJECT_DIR/compose.home-server.yml"
BACKUP_ROOT="${BACKUP_ROOT:-/srv/backups/workflow-hub}"
DEPLOY_REVISION="${1:?Usage: deploy-home-server.sh COMMIT_SHA [BASE_URL]}"
BASE_URL="${2:-https://workflow.norvix.no}"
previous_revision=""
deployment_started=0

cd "$PROJECT_DIR"

if [[ -n "$(git status --porcelain)" ]]; then
  echo "The server worktree is not clean; refusing to deploy." >&2
  exit 1
fi

for name in POSTGRES_PASSWORD ERP_DEMO_SIGNING_SECRET; do
  value="$(sed -n "s/^${name}=//p" "$ENV_FILE" | tail -n 1)"
  if [[ -z "$value" ]]; then
    echo "$name is missing from $ENV_FILE." >&2
    exit 1
  fi
done

previous_revision="$(git rev-parse HEAD)"

rollback() {
  local exit_code=$?
  trap - ERR
  if [[ -n "$previous_revision" ]]; then
    echo "Deployment failed; restoring repository revision $previous_revision." >&2
    git checkout --detach "$previous_revision"
  fi
  if [[ "$deployment_started" -eq 1 && -n "$previous_revision" ]]; then
    echo "Rolling the application containers back to $previous_revision." >&2
    export GIT_SHA="$previous_revision"
    export BUILD_DATE="$(date -u +%Y-%m-%dT%H:%M:%SZ)"
    docker compose --env-file "$ENV_FILE" -f "$COMPOSE_FILE" build
    docker compose --env-file "$ENV_FILE" -f "$COMPOSE_FILE" up -d --wait --wait-timeout 240
  fi
  exit "$exit_code"
}
trap rollback ERR

BACKUP_ROOT="$BACKUP_ROOT" PROJECT_DIR="$PROJECT_DIR" ENV_FILE="$ENV_FILE" \
  bash "$PROJECT_DIR/scripts/backup-home-server.sh"

git fetch --prune origin "$DEPLOY_REVISION"
git checkout --detach "$DEPLOY_REVISION"
test "$(git rev-parse HEAD)" = "$(git rev-parse "$DEPLOY_REVISION")"
test -z "$(git status --porcelain)"

export GIT_SHA="$(git rev-parse HEAD)"
export BUILD_DATE="$(date -u +%Y-%m-%dT%H:%M:%SZ)"

docker compose --env-file "$ENV_FILE" -f "$COMPOSE_FILE" config --quiet
docker compose --env-file "$ENV_FILE" -f "$COMPOSE_FILE" build --pull
deployment_started=1
docker compose --env-file "$ENV_FILE" -f "$COMPOSE_FILE" up -d --wait --wait-timeout 240
bash "$PROJECT_DIR/scripts/smoke-home-server.sh" "$BASE_URL"
bash "$PROJECT_DIR/scripts/smoke-verifiable-demo.sh" "$BASE_URL"

deployed_commit="$(curl --fail --silent --show-error "${BASE_URL%/}/health/version" | jq -r '.commit')"
test "$deployed_commit" = "$GIT_SHA"

deployment_started=0
trap - ERR
echo "Successfully deployed $GIT_SHA to $BASE_URL."
