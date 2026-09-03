#!/bin/bash
# =============================================================================
# build-push.sh – Build and push SPTC APPLICATION Docker image
# Supports: Azure Container Registry (ACR) and Docker Hub
# Usage: ./scripts/build-push.sh
# Run from repository root directory
# =============================================================================

set -e
set -o pipefail

PROJECT_NAME="sptc-application"
DOCKERFILE_PATH="Dockerfile"

echo "=============================================="
echo "  SPTC APPLICATION – Docker Build & Push"
echo "=============================================="
echo ""

# ── Tag sanitization ──────────────────────────────────────────────────────────
IMAGE_NAME=$(echo "$PROJECT_NAME" | tr '[:upper:]' '[:lower:]' | tr -cs 'a-z0-9' '-' | sed 's/^-*//;s/-*$//')

# ── Prompt for image tag ──────────────────────────────────────────────────────
read -rp "Enter image tag (press Enter for 'latest'): " IMAGE_TAG_INPUT
IMAGE_TAG=$(echo "$IMAGE_TAG_INPUT" | tr '[:upper:]' '[:lower:]' | tr -cs 'a-z0-9._-' '-' | sed 's/^-*//;s/-*$//')
if [ -z "$IMAGE_TAG" ]; then
  IMAGE_TAG="latest"
fi
echo "Using image tag: $IMAGE_TAG"
echo ""

# ── Registry selection ────────────────────────────────────────────────────────
echo "Select container registry:"
echo "  1. Azure Container Registry (ACR)"
echo "  2. Docker Hub"
read -rp "Enter choice [1 or 2]: " REGISTRY_CHOICE
echo ""

if [ "$REGISTRY_CHOICE" = "1" ]; then
  # ── Azure Container Registry ─────────────────────────────────────────────
  echo "--- Azure Container Registry (ACR) ---"
  read -rp "Enter ACR name (e.g., myregistry): " ACR_NAME
  ACR_NAME=$(echo "$ACR_NAME" | tr '[:upper:]' '[:lower:]' | sed 's/[^a-z0-9]//g')

  if [ -z "$ACR_NAME" ]; then
    echo "ERROR: ACR name cannot be empty." >&2
    exit 1
  fi

  REGISTRY="${ACR_NAME}.azurecr.io"
  FULL_IMAGE_NAME="${REGISTRY}/${IMAGE_NAME}:${IMAGE_TAG}"

  echo ""
  echo "Logging in to ACR: $REGISTRY"
  az acr login --name "$ACR_NAME"
  if [ $? -ne 0 ]; then
    echo "ERROR: ACR login failed." >&2
    exit 1
  fi

elif [ "$REGISTRY_CHOICE" = "2" ]; then
  # ── Docker Hub ───────────────────────────────────────────────────────────
  echo "--- Docker Hub ---"
  read -rp "Enter Docker Hub username: " DOCKER_USERNAME
  read -rsp "Enter Docker Hub password/token: " DOCKER_PASSWORD
  echo ""
  read -rp "Enter Docker Hub repository (e.g., myorg/sptc-application): " DOCKER_REPO

  if [ -z "$DOCKER_USERNAME" ] || [ -z "$DOCKER_PASSWORD" ] || [ -z "$DOCKER_REPO" ]; then
    echo "ERROR: Docker Hub credentials and repository cannot be empty." >&2
    exit 1
  fi

  FULL_IMAGE_NAME="${DOCKER_REPO}:${IMAGE_TAG}"

  echo ""
  echo "Logging in to Docker Hub..."
  echo "$DOCKER_PASSWORD" | docker login --username "$DOCKER_USERNAME" --password-stdin
  if [ $? -ne 0 ]; then
    echo "ERROR: Docker Hub login failed." >&2
    exit 1
  fi

else
  echo "ERROR: Invalid registry choice. Please enter 1 or 2." >&2
  exit 1
fi

echo ""
echo "Building Docker image: $FULL_IMAGE_NAME"
echo "Build context: . (repository root)"
echo "Dockerfile: $DOCKERFILE_PATH"
echo ""

# ── Docker build ──────────────────────────────────────────────────────────────
docker build -f "$DOCKERFILE_PATH" -t "$FULL_IMAGE_NAME" .
if [ $? -ne 0 ]; then
  echo "ERROR: Docker build failed." >&2
  exit 1
fi

echo ""
echo "Successfully built: $FULL_IMAGE_NAME"
echo ""

# ── Docker push ───────────────────────────────────────────────────────────────
echo "Pushing image to registry..."
docker push "$FULL_IMAGE_NAME"
if [ $? -ne 0 ]; then
  echo "ERROR: Docker push failed." >&2
  exit 1
fi

echo ""
echo "=============================================="
echo "  Build and push completed successfully!"
echo "  Image: $FULL_IMAGE_NAME"
echo "=============================================="
