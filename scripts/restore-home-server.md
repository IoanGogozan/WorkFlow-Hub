# Restore a home-server backup

These steps restore one matching timestamp of the PostgreSQL dump, document
volume, and fictional ERP receiver SQLite volume. They preserve Docker volumes
explicitly; do not use `docker compose down -v` during a restore.

## Before restoring

Set the deployment paths and select three backup files carrying the same UTC
timestamp:

```bash
cd /srv/projects/workflow-hub
export COMPOSE_FILE=/srv/projects/workflow-hub/compose.home-server.yml
export ENV_FILE=/srv/projects/workflow-hub/.env
export DB_BACKUP=/srv/backups/workflow-hub/db/workflow-hub-2026-01-01T020000Z.dump
export DOCUMENTS_BACKUP=/srv/backups/workflow-hub/documents/workflow-hub-documents-2026-01-01T020000Z.tar.gz
export ERP_BACKUP=/srv/backups/workflow-hub/erp-receiver/workflow-hub-erp-receiver-2026-01-01T020000Z.tar.gz
```

Confirm that all files exist and keep a copy of the current volumes before a
destructive restore. Commands below never print values from the private
environment file.

## Restore order

1. Stop request processing while keeping PostgreSQL available:

   ```bash
   docker compose --env-file "$ENV_FILE" -f "$COMPOSE_FILE" stop frontend api worker erp-receiver
   docker compose --env-file "$ENV_FILE" -f "$COMPOSE_FILE" up -d db
   ```

2. Restore PostgreSQL:

   ```bash
   docker compose --env-file "$ENV_FILE" -f "$COMPOSE_FILE" exec -T db \
     sh -c 'pg_restore -U "$POSTGRES_USER" -d "$POSTGRES_DB" --clean --if-exists --no-owner --no-privileges' \
     < "$DB_BACKUP"
   ```

3. Replace the document volume contents:

   ```bash
   docker run --rm \
     -e ARCHIVE="$(basename "$DOCUMENTS_BACKUP")" \
     -v workflow-hub_document_data:/data \
     -v "$(dirname "$DOCUMENTS_BACKUP"):/restore:ro" \
     alpine:3.21 \
     sh -c 'find /data -mindepth 1 -delete && tar -xzf "/restore/$ARCHIVE" -C /data'
   ```

4. Replace the ERP SQLite volume contents while its receiver remains stopped:

   ```bash
   docker run --rm \
     -e ARCHIVE="$(basename "$ERP_BACKUP")" \
     -v workflow-hub_erp_receiver_data:/data \
     -v "$(dirname "$ERP_BACKUP"):/restore:ro" \
     alpine:3.21 \
     sh -c 'find /data -mindepth 1 -delete && tar -xzf "/restore/$ARCHIVE" -C /data'
   ```

5. Start dependencies first, then the application:

   ```bash
   docker compose --env-file "$ENV_FILE" -f "$COMPOSE_FILE" up -d db erp-receiver
   docker compose --env-file "$ENV_FILE" -f "$COMPOSE_FILE" up -d api worker frontend
   docker compose --env-file "$ENV_FILE" -f "$COMPOSE_FILE" ps
   ```

Verify that PostgreSQL and the ERP receiver are healthy, open a restored case
and PDF, and confirm that ERP evidence from before the backup is still visible.
