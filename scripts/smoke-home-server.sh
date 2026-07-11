#!/usr/bin/env bash
set -euo pipefail

BASE_URL="${1:-https://workflow.norvix.no}"
BASE_URL="${BASE_URL%/}"

check() {
  local label="$1"
  local url="$2"
  curl --fail --silent --show-error --location --max-time 20 "$url" >/dev/null
  printf 'ok %s\n' "$label"
}

check "demo entry" "$BASE_URL/demo"
check "health" "$BASE_URL/health"
check "readiness" "$BASE_URL/health/ready"
check "privacy" "$BASE_URL/privacy"
check "terms" "$BASE_URL/terms"

api_status="$(curl --silent --output /dev/null --write-out '%{http_code}' --max-time 20 "$BASE_URL/api/demo-story")"
if [[ "$api_status" != "401" ]]; then
  echo "expected unauthenticated /api/demo-story to return 401, got $api_status" >&2
  exit 1
fi
printf 'ok unauthenticated API blocked\n'

echo "Home-server smoke tests passed for $BASE_URL"
