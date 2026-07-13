#!/usr/bin/env bash
set -euo pipefail

BASE_URL="https://workflow.norvix.no"
FAIL_ONCE=false
TIMEOUT_SECONDS="${SMOKE_TIMEOUT_SECONDS:-180}"

usage() {
  echo "Usage: bash scripts/smoke-verifiable-demo.sh [BASE_URL] [--fail-once]"
}

for argument in "$@"; do
  case "$argument" in
    --fail-once)
      FAIL_ONCE=true
      ;;
    -h|--help)
      usage
      exit 0
      ;;
    http://*|https://*)
      BASE_URL="$argument"
      ;;
    *)
      echo "Unknown argument: $argument" >&2
      usage >&2
      exit 2
      ;;
  esac
done

for command in curl jq mktemp; do
  if ! command -v "$command" >/dev/null 2>&1; then
    echo "Required command is not installed: $command" >&2
    exit 1
  fi
done

BASE_URL="${BASE_URL%/}"
work_dir="$(mktemp -d)"
trap 'rm -rf "$work_dir"' EXIT

session_file="$work_dir/session.json"
run_file="$work_dir/run.json"
retry_file="$work_dir/retry.json"
evidence_file="$work_dir/evidence.json"
failed_evidence_file="$work_dir/failed-evidence.json"

curl_json() {
  curl --fail-with-body --silent --show-error --location --max-time 30 \
    --header "Accept: application/json" \
    "$@"
}

authenticated_json() {
  curl_json --header "Authorization: Bearer $token" "$@"
}

poll_run() {
  local deadline=$((SECONDS + TIMEOUT_SECONDS))
  while (( SECONDS < deadline )); do
    authenticated_json "$BASE_URL/api/live-demo-runs/$run_id" --output "$run_file"
    RUN_STATUS="$(jq -er '.status' "$run_file")"
    if [[ "$RUN_STATUS" == "Completed" || "$RUN_STATUS" == "Failed" ]]; then
      return 0
    fi
    sleep 2
  done

  echo "Timed out waiting for live run $run_id" >&2
  return 1
}

curl_json --request POST "$BASE_URL/api/demo-sessions" --output "$session_file"
token="$(jq -er '.token | select(length > 0)' "$session_file")"
printf 'ok demo session created\n'

request_body='{"simulateErpFailureOnce":false}'
if [[ "$FAIL_ONCE" == true ]]; then
  request_body='{"simulateErpFailureOnce":true}'
fi

authenticated_json \
  --request POST \
  --header "Content-Type: application/json" \
  --data "$request_body" \
  "$BASE_URL/api/live-demo-runs" \
  --output "$run_file"
run_id="$(jq -er '.runId | select(length > 0)' "$run_file")"
printf 'ok live run created\n'

poll_run

if [[ "$FAIL_ONCE" == true ]]; then
  if [[ "$RUN_STATUS" != "Failed" ]]; then
    echo "Expected controlled ERP failure before retry, got $RUN_STATUS" >&2
    exit 1
  fi

  authenticated_json \
    "$BASE_URL/api/live-demo-runs/$run_id/evidence" \
    --output "$failed_evidence_file"
  jq -e \
    --arg run_id "$run_id" \
    '.run.runId == $run_id
      and .run.status == "Failed"
      and .case != null
      and .document != null
      and .sharePoint.mode == "simulated"
      and .erp.status == "Failed"
      and .erp.attempts == 1' \
    "$failed_evidence_file" >/dev/null
  printf 'ok controlled ERP failure preserved completed evidence\n'

  authenticated_json \
    --request POST \
    "$BASE_URL/api/live-demo-runs/$run_id/retry" \
    --output "$retry_file"
  jq -e --arg run_id "$run_id" \
    '.runId == $run_id and .status == "Queued" and .retryCount == 1' \
    "$retry_file" >/dev/null
  printf 'ok retry queued\n'

  poll_run
fi

if [[ "$RUN_STATUS" != "Completed" ]]; then
  public_error="$(jq -r '.publicErrorCode // "unknown"' "$run_file")"
  echo "Expected completed live run, got $RUN_STATUS ($public_error)" >&2
  exit 1
fi
printf 'ok live run completed\n'

authenticated_json \
  "$BASE_URL/api/live-demo-runs/$run_id/evidence" \
  --output "$evidence_file"

expected_attempts=1
if [[ "$FAIL_ONCE" == true ]]; then
  expected_attempts=2
fi

jq -e \
  --arg run_id "$run_id" \
  --argjson expected_attempts "$expected_attempts" \
  '.run.runId == $run_id
    and .run.status == "Completed"
    and (.case.caseNumber | startswith("LIVE-"))
    and (.links.caseHref | type == "string" and length > 0)
    and (.document.fileName | endswith(".pdf"))
    and .document.contentType == "application/pdf"
    and (.links.downloadHref | type == "string" and length > 0)
    and .sharePoint.mode == "simulated"
    and (.sharePoint.operations | length > 0)
    and .erp.status == "Received"
    and (.erp.externalReceiptId | startswith("ERP-DEMO-"))
    and .erp.attempts == $expected_attempts
    and (.auditEvents | length > 0)
    and any(.auditEvents[]; .eventType == "LiveDemoStepCompleted")
    and (if $expected_attempts == 2 then
      any(.auditEvents[]; .eventType == "LiveDemoStepFailed")
      and any(.auditEvents[]; .eventType == "LiveDemoRunRetried")
    else true end)' \
  "$evidence_file" >/dev/null

printf 'ok case and document evidence verified\n'
printf 'ok SharePoint simulator evidence verified\n'
printf 'ok ERP receipt and audit verified\n'
printf 'Verifiable demo smoke passed for %s\n' "$BASE_URL"
