#!/bin/bash
# =============================================================================
# build-push.sh — SPTC APPLICATION
# Builds the Docker image and pushes it to Azure ACR or Docker Hub.
# Run from the repository root directory.
# =============================================================================
set -e
set -o pipefail

PROJECT_NAME="SPTC APPLICATION"
DOCKERFILE_PATH="Dockerfile"

echo "=============================================="
echo "  SPTC APPLICATION — Docker Build & Push"
echo "=============================================="
echo ""

# ── Tag sanitization ──────────────────────────────────────────────────────────
IMAGE_NAME=$(echo "$PROJECT_NAME" | tr '[:upper:]' '[:lower:]' | tr -cs 'a-z0-9' '-' | sed 's/^-*//;s/-*$//')
echo "Sanitized image name: $IMAGE_NAME"
echo ""

# ── Prompt for image tag ──────────────────────────────────────────────────────
read -rp "Enter image tag (press Enter for 'latest'): " IMAGE_TAG_INPUT
IMAGE_TAG=$(echo "$IMAGE_TAG_INPUT" | tr '[:upper:]' '[:lower:]' | tr -cs 'a-z0-9._-' '-' | sed 's/^-*//;s/-*$//')
if [ -z "$IMAGE_TAG" ]; then
  IMAGE_TAG="latest"
fi
echo "Using tag: $IMAGE_TAG"
echo ""

# ── Registry selection ────────────────────────────────────────────────────────
echo "Select container registry:"
echo "  1. Azure Container Registry (ACR)"
echo "  2. Docker Hub"
read -rp "Enter choice [1 or 2]: " REGISTRY_CHOICE
echo ""

if [ "$REGISTRY_CHOICE" = "1" ]; then
  # ── Azure ACR ──────────────────────────────────────────────────────────────
  echo "--- Azure Container Registry (ACR) ---"
  read -rp "Enter ACR name (e.g. myregistry): " ACR_NAME
  ACR_NAME=$(echo "$ACR_NAME" | tr '[:upper:]' '[:lower:]' | sed 's/[^a-z0-9]//g')

  FULL_IMAGE_NAME="${ACR_NAME}.azurecr.io/${IMAGE_NAME}:${IMAGE_TAG}"
  echo ""
  echo "Full image name: $FULL_IMAGE_NAME"
  echo ""

  echo "[1/3] Logging in to Azure ACR: $ACR_NAME ..."
  az acr login --name "$ACR_NAME"
  echo "ACR login successful."
  echo ""

  echo "[2/3] Building Docker image ..."
  docker build -f "$DOCKERFILE_PATH" -t "$FULL_IMAGE_NAME" .
  echo "Docker build successful."
  echo ""

  echo "[3/3] Pushing image to ACR ..."
  docker push "$FULL_IMAGE_NAME"
  echo "Image pushed successfully: $FULL_IMAGE_NAME"

elif [ "$REGISTRY_CHOICE" = "2" ]; then
  # ── Docker Hub ─────────────────────────────────────────────────────────────
  echo "--- Docker Hub ---"
  read -rp "Enter Docker Hub username: " DOCKER_USERNAME
  read -rsp "Enter Docker Hub password/token: " DOCKER_PASSWORD
  echo ""
  read -rp "Enter Docker Hub repository name (e.g. myrepo): " DOCKER_REPO

  FULL_IMAGE_NAME="${DOCKER_USERNAME}/${DOCKER_REPO}:${IMAGE_TAG}"
  echo ""
  echo "Full image name: $FULL_IMAGE_NAME"
  echo ""

  echo "[1/3] Logging in to Docker Hub ..."
  echo "$DOCKER_PASSWORD" | docker login --username "$DOCKER_USERNAME" --password-stdin
  echo "Docker Hub login successful."
  echo ""

  echo "[2/3] Building Docker image ..."
  docker build -f "$DOCKERFILE_PATH" -t "$FULL_IMAGE_NAME" .
  echo "Docker build successful."
  echo ""

  echo "[3/3] Pushing image to Docker Hub ..."
  docker push "$FULL_IMAGE_NAME"
  echo "Image pushed successfully: $FULL_IMAGE_NAME"

else
  echo "ERROR: Invalid registry choice. Please enter 1 or 2."
  exit 1
fi

echo ""
echo "=============================================="
echo "  Build & Push Complete!"
echo "  Image: $FULL_IMAGE_NAME"
echo "=============================================="
