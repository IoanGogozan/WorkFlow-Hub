#!/usr/bin/env bash
set -euo pipefail

PROJECT_DIR="${PROJECT_DIR:-/srv/projects/workflow-hub}"
COMPOSE_FILE="${COMPOSE_FILE:-$PROJECT_DIR/compose.home-server.yml}"
ENV_FILE="${ENV_FILE:-$PROJECT_DIR/.env}"
BACKUP_ROOT="${BACKUP_ROOT:-/srv/backups/workflow-hub}"
RETENTION_DAYS="${RETENTION_DAYS:-14}"

db_dir="$BACKUP_ROOT/db"
documents_dir="$BACKUP_ROOT/documents"
erp_dir="$BACKUP_ROOT/erp-receiver"
lock_file="$BACKUP_ROOT/.backup.lock"
erp_stopped=0

mkdir -p "$db_dir" "$documents_dir" "$erp_dir"
exec 9>"$lock_file"
flock -n 9 || {
  echo "A WorkFlow Hub backup is already running." >&2
  exit 1
}

stamp="$(date -u +%Y-%m-%dT%H%M%SZ)"
db_target="$db_dir/workflow-hub-$stamp.dump"
documents_target="$documents_dir/workflow-hub-documents-$stamp.tar.gz"
erp_target="$erp_dir/workflow-hub-erp-receiver-$stamp.tar.gz"

cd "$PROJECT_DIR"

restart_erp_receiver() {
  if [[ "$erp_stopped" -eq 1 ]]; then
    docker compose --env-file "$ENV_FILE" -f "$COMPOSE_FILE" up -d erp-receiver >/dev/null
  fi
}
trap restart_erp_receiver EXIT

docker compose --env-file "$ENV_FILE" -f "$COMPOSE_FILE" exec -T db \
  sh -c 'pg_dump -U "$POSTGRES_USER" -d "$POSTGRES_DB" --format=custom' \
  > "$db_target.tmp"
mv "$db_target.tmp" "$db_target"

docker run --rm \
  -v workflow-hub_document_data:/data:ro \
  -v "$documents_dir:/backup" \
  alpine:3.21 \
  tar -czf "/backup/$(basename "$documents_target").tmp" -C /data .
mv "$documents_target.tmp" "$documents_target"

# Stop the SQLite writer briefly so the volume archive is transactionally consistent.
# Leave an already-stopped receiver stopped after the backup.
if [[ -n "$(docker compose --env-file "$ENV_FILE" -f "$COMPOSE_FILE" ps --status running -q erp-receiver)" ]]; then
  docker compose --env-file "$ENV_FILE" -f "$COMPOSE_FILE" stop erp-receiver >/dev/null
  erp_stopped=1
fi
docker run --rm \
  -v workflow-hub_erp_receiver_data:/data:ro \
  -v "$erp_dir:/backup" \
  alpine:3.21 \
  tar -czf "/backup/$(basename "$erp_target").tmp" -C /data .
mv "$erp_target.tmp" "$erp_target"
restart_erp_receiver
erp_stopped=0

find "$db_dir" -type f -name 'workflow-hub-*.dump' -mtime "+$RETENTION_DAYS" -delete
find "$documents_dir" -type f -name 'workflow-hub-documents-*.tar.gz' -mtime "+$RETENTION_DAYS" -delete
find "$erp_dir" -type f -name 'workflow-hub-erp-receiver-*.tar.gz' -mtime "+$RETENTION_DAYS" -delete

printf 'Backup complete:\n- %s\n- %s\n- %s\n' "$db_target" "$documents_target" "$erp_target"
