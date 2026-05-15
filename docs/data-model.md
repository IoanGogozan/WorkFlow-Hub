# Data Model

## Principles

- Every business table is tenant-scoped.
- Use UUID primary keys.
- Use UTC timestamps with `timestamptz`.
- Keep audit records append-only.
- Store file binaries in blob storage, not PostgreSQL.
- Store AI outputs as suggestions with review status.

## Standard Tenant-Scoped Columns

Every tenant-scoped table should include:

```sql
id uuid primary key,
tenant_id uuid not null,
created_at timestamptz not null,
updated_at timestamptz not null,
created_by uuid null,
updated_by uuid null
```

## Minimum Tables

```txt
tenants
users
tenant_memberships
customers
intake_items
intake_attachments
cases
case_tasks
case_notes
documents
document_versions
document_links
ai_analysis_runs
review_tasks
integration_connections
integration_sync_runs
delivery_packages
delivery_package_items
delivery_links
delivery_access_logs
audit_events
api_keys
webhook_events
```

## Core Tables

### tenants

- `id`
- `name`
- `slug`
- `organization_number`
- `country_code`
- `ai_enabled`
- `retention_policy_json`
- `created_at`
- `updated_at`

### users

- `id`
- `display_name`
- `email`
- `entra_object_id`
- `is_active`
- `created_at`
- `updated_at`

### tenant_memberships

- `id`
- `tenant_id`
- `user_id`
- `role`
- `created_at`
- `updated_at`

Roles:

- `TenantOwner`
- `Admin`
- `OperationsUser`
- `Reviewer`
- `Viewer`
- `ExternalRecipient`

### customers

- standard tenant columns
- `name`
- `organization_number`
- `brreg_data_json`
- `source`
- `source_updated_at`
- `primary_contact_name`
- `primary_contact_email`

### intake_items

- standard tenant columns
- `source`
- `status`
- `subject`
- `body`
- `customer_name`
- `organization_number`
- `category`
- `urgency`
- `received_at`
- `assigned_to_user_id`
- `converted_case_id`

Statuses:

- `New`
- `AIAnalyzed`
- `NeedsReview`
- `Approved`
- `ConvertedToCase`
- `Rejected`

### intake_attachments

- standard tenant columns
- `intake_item_id`
- `document_id`
- `original_filename`
- `content_type`
- `size_bytes`

### cases

- standard tenant columns
- `case_number`
- `title`
- `description`
- `customer_id`
- `status`
- `owner_user_id`
- `due_date`
- `missing_information_json`
- `external_project_id`

Statuses:

- `Draft`
- `Open`
- `WaitingForCustomer`
- `WaitingForInternalReview`
- `ReadyForDelivery`
- `Delivered`
- `Closed`

### case_tasks

- standard tenant columns
- `case_id`
- `title`
- `description`
- `status`
- `assigned_to_user_id`
- `due_date`
- `completed_at`

### case_notes

- standard tenant columns
- `case_id`
- `body`
- `visibility`

### documents

- standard tenant columns
- `title`
- `status`
- `document_type`
- `current_version_id`
- `case_id`
- `customer_id`
- `expiry_date`
- `classification_reviewed_by`
- `classification_reviewed_at`

Statuses:

- `Uploaded`
- `Processing`
- `AIClassified`
- `NeedsReview`
- `Approved`
- `Rejected`
- `Archived`

### document_versions

- standard tenant columns
- `document_id`
- `version_number`
- `blob_container`
- `blob_name`
- `original_filename`
- `content_type`
- `size_bytes`
- `sha256_hash`
- `uploaded_by_user_id`

### document_links

- standard tenant columns
- `document_id`
- `entity_type`
- `entity_id`

### ai_analysis_runs

- `id`
- `tenant_id`
- `entity_type`
- `entity_id`
- `provider`
- `model`
- `prompt_version`
- `input_hash`
- `output_json`
- `confidence`
- `status`
- `reviewed_by`
- `reviewed_at`
- `created_at`

Statuses:

- `Pending`
- `Completed`
- `NeedsReview`
- `Approved`
- `Rejected`
- `Failed`

### review_tasks

- standard tenant columns
- `entity_type`
- `entity_id`
- `review_type`
- `status`
- `assigned_to_user_id`
- `ai_analysis_run_id`
- `decision_json`
- `decided_by`
- `decided_at`

### integration_connections

- standard tenant columns
- `provider`
- `display_name`
- `mode`
- `status`
- `settings_json`
- `last_successful_sync_at`
- `last_failed_sync_at`

Modes:

- `Mock`
- `Real`

### integration_sync_runs

- standard tenant columns
- `integration_connection_id`
- `provider`
- `status`
- `started_at`
- `completed_at`
- `items_processed`
- `error_message`
- `correlation_id`

### delivery_packages

- standard tenant columns
- `case_id`
- `title`
- `status`
- `summary_pdf_document_id`
- `generated_at`

### delivery_package_items

- standard tenant columns
- `delivery_package_id`
- `document_id`
- `sort_order`

### delivery_links

- standard tenant columns
- `delivery_package_id`
- `token_hash`
- `recipient_email`
- `expires_at`
- `revoked_at`
- `created_by_user_id`

### delivery_access_logs

- standard tenant columns
- `delivery_link_id`
- `accessed_at`
- `ip_address`
- `user_agent`
- `action`

### audit_events

- `id`
- `tenant_id`
- `actor_user_id`
- `actor_type`
- `entity_type`
- `entity_id`
- `action`
- `before_json`
- `after_json`
- `ip_address`
- `user_agent`
- `correlation_id`
- `created_at`

### api_keys

- standard tenant columns
- `name`
- `key_hash`
- `scopes_json`
- `expires_at`
- `revoked_at`
- `last_used_at`

### webhook_events

- standard tenant columns
- `provider`
- `event_type`
- `payload_json`
- `status`
- `received_at`
- `processed_at`
- `error_message`

## Mandatory Indexes

- `(tenant_id, id)` on tenant-scoped tables.
- `(tenant_id, status)` on workflow tables.
- `(tenant_id, created_at)` on audit and event tables.
- Unique tenant-safe business keys, for example `(tenant_id, case_number)`.
- Unique organization number where appropriate per tenant: `(tenant_id, organization_number)`.

## Migration Priority

Phase 0/1:

- `tenants`
- `users`
- `tenant_memberships`
- `audit_events`

Phase 2/3:

- `intake_items`
- `intake_attachments`
- `ai_analysis_runs`
- `review_tasks`

Phase 4/6:

- `customers`
- `cases`
- `case_tasks`
- `case_notes`
- `documents`
- `document_versions`
- `document_links`

Phase 7/9:

- `integration_connections`
- `integration_sync_runs`
- `delivery_packages`
- `delivery_package_items`
- `delivery_links`
- `delivery_access_logs`
- `api_keys`
- `webhook_events`
