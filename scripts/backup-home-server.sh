#!/usr/bin/env bash
set -euo pipefail

PROJECT_DIR="${PROJECT_DIR:-/srv/projects/workflow-hub}"
COMPOSE_FILE="${COMPOSE_FILE:-$PROJECT_DIR/compose.home-server.yml}"
ENV_FILE="${ENV_FILE:-$PROJECT_DIR/.env}"
BACKUP_ROOT="${BACKUP_ROOT:-/srv/backups/workflow-hub}"
RETENTION_DAYS="${RETENTION_DAYS:-14}"

db_dir="$BACKUP_ROOT/db"
documents_dir="$BACKUP_ROOT/documents"
lock_file="$BACKUP_ROOT/.backup.lock"

mkdir -p "$db_dir" "$documents_dir"
exec 9>"$lock_file"
flock -n 9 || {
  echo "A WorkFlow Hub backup is already running." >&2
  exit 1
}

stamp="$(date -u +%Y-%m-%dT%H%M%SZ)"
db_target="$db_dir/workflow-hub-$stamp.dump"
documents_target="$documents_dir/workflow-hub-documents-$stamp.tar.gz"

cd "$PROJECT_DIR"

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

find "$db_dir" -type f -name 'workflow-hub-*.dump' -mtime "+$RETENTION_DAYS" -delete
find "$documents_dir" -type f -name 'workflow-hub-documents-*.tar.gz' -mtime "+$RETENTION_DAYS" -delete

printf 'Backup complete: %s and %s\n' "$db_target" "$documents_target"
