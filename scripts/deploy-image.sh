#!/bin/bash
# =============================================================================
# deploy-image.sh – Deploy SPTC APPLICATION to Azure AKS
# Target Platform: Azure Kubernetes Service (AKS)
# Usage: ./scripts/deploy-image.sh
# Run from repository root directory
# =============================================================================

set -e
set -o pipefail

APP_NAME="sptc-application"
NAMESPACE="sptc-application"
K8S_DIR="kubernetes"

echo "=============================================="
echo "  SPTC APPLICATION – AKS Deployment"
echo "=============================================="
echo ""

# ── Prompt for Azure details ──────────────────────────────────────────────────
read -rp "Enter Azure Resource Group name: " RESOURCE_GROUP
if [ -z "$RESOURCE_GROUP" ]; then
  echo "ERROR: Resource group cannot be empty." >&2
  exit 1
fi

read -rp "Enter AKS Cluster name: " CLUSTER_NAME
if [ -z "$CLUSTER_NAME" ]; then
  echo "ERROR: AKS cluster name cannot be empty." >&2
  exit 1
fi

read -rp "Enter full Docker image URI (e.g., myregistry.azurecr.io/sptc-application:latest): " IMAGE_URI
if [ -z "$IMAGE_URI" ]; then
  echo "ERROR: Image URI cannot be empty." >&2
  exit 1
fi

echo ""
echo "--- Application Configuration ---"
echo "The following environment variables are required by SPTC APPLICATION."
echo "Press Enter to skip any optional variable (it will use the placeholder)."
echo ""

read -rp "Enter MySQL host (e.g., mysql-server.mysql.database.azure.com): " MYSQL_HOST_VAL
if [ -z "$MYSQL_HOST_VAL" ]; then MYSQL_HOST_VAL="localhost"; fi

read -rp "Enter MySQL port (default: 3306): " MYSQL_PORT_VAL
if [ -z "$MYSQL_PORT_VAL" ]; then MYSQL_PORT_VAL="3306"; fi

read -rp "Enter MySQL database name (default: dtb_sptc): " MYSQL_DB_VAL
if [ -z "$MYSQL_DB_VAL" ]; then MYSQL_DB_VAL="dtb_sptc"; fi

echo ""
echo "--- Configuring kubectl for AKS cluster ---"
az aks get-credentials --resource-group "$RESOURCE_GROUP" --name "$CLUSTER_NAME" --overwrite-existing
if [ $? -ne 0 ]; then
  echo "ERROR: Failed to get AKS credentials." >&2
  exit 1
fi

echo ""
echo "Verifying cluster connectivity..."
kubectl cluster-info || { echo "ERROR: Cannot connect to AKS cluster." >&2; exit 1; }

echo ""
echo "--- Updating Kubernetes manifests ---"

# Create working copies of manifests
cp "$K8S_DIR/deployment.yaml" "$K8S_DIR/deployment.yaml.tmp"
cp "$K8S_DIR/ingress.yaml" "$K8S_DIR/ingress.yaml.tmp"

# Replace placeholders using pipe delimiter (safe for URIs with slashes)
sed -i 's|{{IMAGE_URI}}|'"$IMAGE_URI"'|g' "$K8S_DIR/deployment.yaml.tmp"
sed -i 's|{{MYSQL_HOST}}|'"$MYSQL_HOST_VAL"'|g' "$K8S_DIR/deployment.yaml.tmp"
sed -i 's|{{MYSQL_PORT}}|'"$MYSQL_PORT_VAL"'|g' "$K8S_DIR/deployment.yaml.tmp"
sed -i 's|{{MYSQL_DATABASE}}|'"$MYSQL_DB_VAL"'|g' "$K8S_DIR/deployment.yaml.tmp"

echo ""
echo "--- Applying Kubernetes manifests ---"

echo "1/4 Applying namespace..."
kubectl apply -f "$K8S_DIR/namespace.yaml"

echo "2/4 Applying deployment..."
kubectl apply -f "$K8S_DIR/deployment.yaml.tmp"

echo "3/4 Applying service..."
kubectl apply -f "$K8S_DIR/service.yaml"

echo "4/4 Applying ingress..."
kubectl apply -f "$K8S_DIR/ingress.yaml.tmp"

# Clean up temp files
rm -f "$K8S_DIR/deployment.yaml.tmp" "$K8S_DIR/ingress.yaml.tmp"

echo ""
echo "--- Waiting for deployment rollout ---"
kubectl rollout status deployment/"$APP_NAME" -n "$NAMESPACE" --timeout=300s
if [ $? -ne 0 ]; then
  echo "ERROR: Deployment rollout failed. Check pod logs:" >&2
  echo "  kubectl logs -l app=$APP_NAME -n $NAMESPACE" >&2
  echo ""
  echo "To rollback: kubectl rollout undo deployment/$APP_NAME -n $NAMESPACE" >&2
  exit 1
fi

echo ""
echo "--- Verifying deployed resources ---"
kubectl get pods,svc,ingress -n "$NAMESPACE"

echo ""
echo "--- Application Access ---"
INGRESS_IP=$(kubectl get ingress "${APP_NAME}-ingress" -n "$NAMESPACE" -o jsonpath='{.status.loadBalancer.ingress[0].ip}' 2>/dev/null || echo "pending")
INGRESS_HOST=$(kubectl get ingress "${APP_NAME}-ingress" -n "$NAMESPACE" -o jsonpath='{.spec.rules[0].host}' 2>/dev/null || echo "sptc-application.example.com")

echo "  Ingress Host : $INGRESS_HOST"
echo "  Ingress IP   : $INGRESS_IP"
echo "  Health Check : http://$INGRESS_HOST/health"
echo ""
echo "=============================================="
echo "  Deployment completed successfully!"
echo "  Namespace: $NAMESPACE"
echo "  Image    : $IMAGE_URI"
echo "=============================================="
echo ""
echo "Useful commands:"
echo "  kubectl get pods -n $NAMESPACE"
echo "  kubectl logs -l app=$APP_NAME -n $NAMESPACE"
echo "  kubectl describe deployment $APP_NAME -n $NAMESPACE"
echo "  kubectl rollout undo deployment/$APP_NAME -n $NAMESPACE  # rollback"
