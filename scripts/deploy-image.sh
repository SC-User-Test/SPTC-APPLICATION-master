#!/bin/bash
# =============================================================================
# deploy-image.sh — SPTC APPLICATION
# Deploys the application to Azure Kubernetes Service (AKS).
# Run from the repository root directory.
# =============================================================================
set -e
set -o pipefail

APP_NAME="sptc-application"
NAMESPACE="sptc-application"

echo "=============================================="
echo "  SPTC APPLICATION — Deploy to Azure AKS"
echo "=============================================="
echo ""

# ── Azure resource group and AKS cluster ─────────────────────────────────────
read -rp "Enter Azure Resource Group name: " RESOURCE_GROUP
if [ -z "$RESOURCE_GROUP" ]; then
  echo "ERROR: Resource group name cannot be empty."
  exit 1
fi

read -rp "Enter AKS Cluster name: " CLUSTER_NAME
if [ -z "$CLUSTER_NAME" ]; then
  echo "ERROR: AKS cluster name cannot be empty."
  exit 1
fi

# ── Docker image URI ──────────────────────────────────────────────────────────
read -rp "Enter full Docker image URI (e.g. myregistry.azurecr.io/sptc-application:latest): " IMAGE_URI
if [ -z "$IMAGE_URI" ]; then
  echo "ERROR: Docker image URI cannot be empty."
  exit 1
fi

echo ""
echo "--- Application Environment Variables ---"
echo "Press Enter to skip any variable (it will remain as placeholder)."
echo ""

# ── Prompt for application-specific environment variables ────────────────────
read -rp "Enter value for MYSQL_HOST (MySQL server hostname): " MYSQL_HOST_VAL
read -rp "Enter value for MYSQL_PORT (default: 3306): " MYSQL_PORT_VAL
read -rp "Enter value for MYSQL_DATABASE (default: dtb_sptc): " MYSQL_DATABASE_VAL
read -rp "Enter value for MYSQL_USERNAME: " MYSQL_USERNAME_VAL
read -rsp "Enter value for MYSQL_PASSWORD: " MYSQL_PASSWORD_VAL
echo ""
read -rsp "Enter value for REDIS_CONNECTION_STRING: " REDIS_CONNECTION_STRING_VAL
echo ""

# Apply defaults
[ -z "$MYSQL_PORT_VAL" ] && MYSQL_PORT_VAL="3306"
[ -z "$MYSQL_DATABASE_VAL" ] && MYSQL_DATABASE_VAL="dtb_sptc"

echo ""
echo "=============================================="
echo "  Deployment Configuration"
echo "  Resource Group : $RESOURCE_GROUP"
echo "  AKS Cluster    : $CLUSTER_NAME"
echo "  Image URI      : $IMAGE_URI"
echo "  Namespace      : $NAMESPACE"
echo "=============================================="
echo ""

# ── Configure kubectl for AKS ─────────────────────────────────────────────────
echo "[1/7] Configuring kubectl for AKS cluster ..."
az aks get-credentials --resource-group "$RESOURCE_GROUP" --name "$CLUSTER_NAME" --overwrite-existing
echo "kubectl configured successfully."
echo ""

# ── Verify cluster connectivity ───────────────────────────────────────────────
echo "[2/7] Verifying cluster connectivity ..."
kubectl cluster-info || { echo "ERROR: Cannot connect to AKS cluster."; exit 1; }
echo ""

# ── Update Kubernetes manifests with actual values ────────────────────────────
echo "[3/7] Updating Kubernetes manifests ..."

# Create working copies of manifests
cp kubernetes/deployment.yaml kubernetes/deployment.yaml.bak
cp kubernetes/service.yaml kubernetes/service.yaml.bak
cp kubernetes/ingress.yaml kubernetes/ingress.yaml.bak
cp kubernetes/namespace.yaml kubernetes/namespace.yaml.bak

# Replace image URI placeholder
sed -i 's|{{IMAGE_URI}}|'"$IMAGE_URI"'|g' kubernetes/deployment.yaml

# Replace environment variable placeholders
if [ -n "$MYSQL_HOST_VAL" ]; then
  sed -i 's|{{MYSQL_HOST}}|'"$MYSQL_HOST_VAL"'|g' kubernetes/deployment.yaml
fi
if [ -n "$MYSQL_PORT_VAL" ]; then
  sed -i 's|{{MYSQL_PORT}}|'"$MYSQL_PORT_VAL"'|g' kubernetes/deployment.yaml
fi
if [ -n "$MYSQL_DATABASE_VAL" ]; then
  sed -i 's|{{MYSQL_DATABASE}}|'"$MYSQL_DATABASE_VAL"'|g' kubernetes/deployment.yaml
fi
if [ -n "$MYSQL_USERNAME_VAL" ]; then
  sed -i 's|{{MYSQL_USERNAME}}|'"$MYSQL_USERNAME_VAL"'|g' kubernetes/deployment.yaml
fi
if [ -n "$MYSQL_PASSWORD_VAL" ]; then
  sed -i 's|{{MYSQL_PASSWORD}}|'"$MYSQL_PASSWORD_VAL"'|g' kubernetes/deployment.yaml
fi
if [ -n "$REDIS_CONNECTION_STRING_VAL" ]; then
  sed -i 's|{{REDIS_CONNECTION_STRING}}|'"$REDIS_CONNECTION_STRING_VAL"'|g' kubernetes/deployment.yaml
fi

echo "Manifests updated."
echo ""

# ── Apply Kubernetes manifests ────────────────────────────────────────────────
echo "[4/7] Applying Kubernetes manifests ..."

echo "  Applying namespace ..."
kubectl apply -f kubernetes/namespace.yaml

echo "  Applying deployment ..."
kubectl apply -f kubernetes/deployment.yaml

echo "  Applying service ..."
kubectl apply -f kubernetes/service.yaml

echo "  Applying ingress ..."
kubectl apply -f kubernetes/ingress.yaml

echo "Manifests applied successfully."
echo ""

# ── Restore original manifests ────────────────────────────────────────────────
mv kubernetes/deployment.yaml.bak kubernetes/deployment.yaml
mv kubernetes/service.yaml.bak kubernetes/service.yaml
mv kubernetes/ingress.yaml.bak kubernetes/ingress.yaml
mv kubernetes/namespace.yaml.bak kubernetes/namespace.yaml

# ── Wait for rollout ──────────────────────────────────────────────────────────
echo "[5/7] Waiting for deployment rollout ..."
kubectl rollout status deployment/"$APP_NAME" -n "$NAMESPACE" --timeout=300s
echo "Deployment rollout complete."
echo ""

# ── Verify resources ──────────────────────────────────────────────────────────
echo "[6/7] Verifying deployed resources ..."
kubectl get pods,svc,ingress -n "$NAMESPACE"
echo ""

# ── Display application URL ───────────────────────────────────────────────────
echo "[7/7] Retrieving application URL ..."
INGRESS_IP=$(kubectl get ingress "$APP_NAME"-ingress -n "$NAMESPACE" -o jsonpath='{.status.loadBalancer.ingress[0].ip}' 2>/dev/null || echo "")
INGRESS_HOST=$(kubectl get ingress "$APP_NAME"-ingress -n "$NAMESPACE" -o jsonpath='{.spec.rules[0].host}' 2>/dev/null || echo "")

echo ""
echo "=============================================="
echo "  Deployment Complete!"
if [ -n "$INGRESS_IP" ]; then
  echo "  Application URL: http://$INGRESS_IP"
fi
if [ -n "$INGRESS_HOST" ]; then
  echo "  Ingress Host   : http://$INGRESS_HOST"
fi
echo "  Health Check   : http://<pod-ip>:8080/health"
echo "  Namespace      : $NAMESPACE"
echo "=============================================="
echo ""
echo "Rollback command (if needed):"
echo "  kubectl rollout undo deployment/$APP_NAME -n $NAMESPACE"
