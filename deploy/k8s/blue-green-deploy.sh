#!/usr/bin/env bash
set -euo pipefail

NAMESPACE="${1:-default}"
IMAGE_TAG="${2:-latest}"
REGISTRY_IMAGE="${3:-ghcr.io/owner/repo}"
SMOKE_PATH="${4:-/health}"

ACTIVE_TRACK=$(kubectl -n "$NAMESPACE" get svc web-api -o jsonpath='{.spec.selector.track}')
if [[ "$ACTIVE_TRACK" == "blue" ]]; then
  INACTIVE_TRACK="green"
else
  INACTIVE_TRACK="blue"
fi

MANIFEST_FILE="deploy/k8s/web-api-${INACTIVE_TRACK}.yaml"

kubectl -n "$NAMESPACE" apply -f "$MANIFEST_FILE"
kubectl -n "$NAMESPACE" set image "deployment/web-api-${INACTIVE_TRACK}" web-api="${REGISTRY_IMAGE}:${IMAGE_TAG}"
kubectl -n "$NAMESPACE" rollout status "deployment/web-api-${INACTIVE_TRACK}" --timeout=180s

kubectl -n "$NAMESPACE" port-forward "deployment/web-api-${INACTIVE_TRACK}" 18080:8080 >/tmp/port-forward.log 2>&1 &
PF_PID=$!
sleep 4
curl --fail --retry 5 --retry-delay 2 "http://localhost:18080${SMOKE_PATH}" >/dev/null
kill "$PF_PID"

kubectl -n "$NAMESPACE" patch service web-api -p "{\"spec\":{\"selector\":{\"app\":\"web-api\",\"track\":\"${INACTIVE_TRACK}\"}}}"
kubectl -n "$NAMESPACE" scale "deployment/web-api-${ACTIVE_TRACK}" --replicas=0

echo "Blue/green switch complete. Active track: ${INACTIVE_TRACK}"
