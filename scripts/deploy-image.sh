#!/bin/bash
# =============================================================================
# deploy-image.sh – Deploy SPTC APPLICATION to Azure AKS
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

# ---- Azure / AKS credentials ----
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

# ---- Docker image URI ----
read -rp "Enter full Docker image URI (e.g. myregistry.azurecr.io/sptc-application:latest): " IMAGE_URI
if [ -z "$IMAGE_URI" ]; then
  echo "ERROR: Image URI cannot be empty." >&2
  exit 1
fi

echo ""
echo "--- Application Environment Variables ---"
echo "Press Enter to skip any variable (placeholder will remain in manifest)."
echo ""

read -rp "Enter MYSQL_HOST (e.g. mysql-server.mysql.database.azure.com): " MYSQL_HOST
read -rp "Enter MYSQL_PORT [3306]: " MYSQL_PORT
MYSQL_PORT="${MYSQL_PORT:-3306}"
read -rp "Enter MYSQL_DATABASE [dtb_sptc]: " MYSQL_DATABASE
MYSQL_DATABASE="${MYSQL_DATABASE:-dtb_sptc}"
read -rp "Enter MYSQL_USERNAME: " MYSQL_USERNAME
read -rsp "Enter MYSQL_PASSWORD: " MYSQL_PASSWORD
echo ""
read -rp "Enter REDIS_CONNECTION_STRING (e.g. myredis.redis.cache.windows.net:6380,password=...): " REDIS_CONNECTION_STRING

echo ""
echo "Configuring kubectl for AKS cluster: ${CLUSTER_NAME} in resource group: ${RESOURCE_GROUP} ..."
az aks get-credentials --resource-group "$RESOURCE_GROUP" --name "$CLUSTER_NAME" --overwrite-existing
if [ $? -ne 0 ]; then
  echo "ERROR: Failed to get AKS credentials." >&2
  exit 1
fi

echo ""
echo "Verifying cluster connectivity ..."
kubectl cluster-info || { echo "ERROR: Cannot connect to AKS cluster." >&2; exit 1; }

echo ""
echo "Updating Kubernetes manifests with provided values ..."

# Work on copies to avoid modifying originals
cp -r "${K8S_DIR}" /tmp/sptc-k8s-deploy

# Replace IMAGE_URI placeholder
sed -i 's|{{IMAGE_URI}}|'"${IMAGE_URI}"'|g' /tmp/sptc-k8s-deploy/deployment.yaml

# Replace environment variable placeholders
if [ -n "$MYSQL_HOST" ]; then
  sed -i 's|{{MYSQL_HOST}}|'"${MYSQL_HOST}"'|g' /tmp/sptc-k8s-deploy/deployment.yaml
fi
if [ -n "$MYSQL_PORT" ]; then
  sed -i 's|{{MYSQL_PORT}}|'"${MYSQL_PORT}"'|g' /tmp/sptc-k8s-deploy/deployment.yaml
fi
if [ -n "$MYSQL_DATABASE" ]; then
  sed -i 's|{{MYSQL_DATABASE}}|'"${MYSQL_DATABASE}"'|g' /tmp/sptc-k8s-deploy/deployment.yaml
fi
if [ -n "$MYSQL_USERNAME" ]; then
  sed -i 's|{{MYSQL_USERNAME}}|'"${MYSQL_USERNAME}"'|g' /tmp/sptc-k8s-deploy/deployment.yaml
fi
if [ -n "$MYSQL_PASSWORD" ]; then
  sed -i 's|{{MYSQL_PASSWORD}}|'"${MYSQL_PASSWORD}"'|g' /tmp/sptc-k8s-deploy/deployment.yaml
fi
if [ -n "$REDIS_CONNECTION_STRING" ]; then
  sed -i 's|{{REDIS_CONNECTION_STRING}}|'"${REDIS_CONNECTION_STRING}"'|g' /tmp/sptc-k8s-deploy/deployment.yaml
fi

echo ""
echo "Applying Kubernetes manifests ..."

echo "  [1/4] Applying namespace ..."
kubectl apply -f /tmp/sptc-k8s-deploy/namespace.yaml

echo "  [2/4] Applying deployment ..."
kubectl apply -f /tmp/sptc-k8s-deploy/deployment.yaml

echo "  [3/4] Applying service ..."
kubectl apply -f /tmp/sptc-k8s-deploy/service.yaml

echo "  [4/4] Applying ingress ..."
kubectl apply -f /tmp/sptc-k8s-deploy/ingress.yaml

echo ""
echo "Waiting for deployment rollout ..."
kubectl rollout status deployment/${APP_NAME} -n ${NAMESPACE} --timeout=300s
if [ $? -ne 0 ]; then
  echo "ERROR: Deployment rollout failed. Running rollback ..." >&2
  kubectl rollout undo deployment/${APP_NAME} -n ${NAMESPACE}
  echo "Rollback initiated. Check pod status with: kubectl get pods -n ${NAMESPACE}"
  exit 1
fi

echo ""
echo "Verifying deployed resources ..."
kubectl get pods,svc,ingress -n ${NAMESPACE}

echo ""
INGRESS_IP=$(kubectl get ingress ${APP_NAME}-ingress -n ${NAMESPACE} -o jsonpath='{.status.loadBalancer.ingress[0].ip}' 2>/dev/null || echo "pending")
echo "=============================================="
echo "  Deployment completed successfully!"
echo "  Namespace : ${NAMESPACE}"
echo "  Image     : ${IMAGE_URI}"
if [ "$INGRESS_IP" != "pending" ] && [ -n "$INGRESS_IP" ]; then
  echo "  App URL   : http://${INGRESS_IP}"
else
  echo "  App URL   : http://sptc-application.example.com (update DNS to point to ingress IP)"
  echo "  Get IP    : kubectl get ingress -n ${NAMESPACE}"
fi
echo "=============================================="

# Cleanup temp files
rm -rf /tmp/sptc-k8s-deploy
