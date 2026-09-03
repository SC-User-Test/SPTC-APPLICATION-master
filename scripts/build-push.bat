@echo off
setlocal enabledelayedexpansion

REM =============================================================================
REM build-push.bat - Build and push SPTC APPLICATION Docker image
REM Supports: Azure Container Registry (ACR) and Docker Hub
REM Usage: scripts\build-push.bat
REM Run from repository root directory
REM =============================================================================

set "PROJECT_NAME=sptc-application"
set "DOCKERFILE_PATH=Dockerfile"

echo ==============================================
echo   SPTC APPLICATION - Docker Build and Push
echo ==============================================
echo.

REM ── Prompt for image tag ──────────────────────────────────────────────────
set /p "IMAGE_TAG_INPUT=Enter image tag (press Enter for 'latest'): "
if "!IMAGE_TAG_INPUT!"=="" (
    set "IMAGE_TAG=latest"
) else (
    set "IMAGE_TAG=!IMAGE_TAG_INPUT!"
)
echo Using image tag: !IMAGE_TAG!
echo.

REM ── Registry selection ────────────────────────────────────────────────────
echo Select container registry:
echo   1. Azure Container Registry (ACR)
echo   2. Docker Hub
set /p "REGISTRY_CHOICE=Enter choice [1 or 2]: "
echo.

if "!REGISTRY_CHOICE!"=="1" goto :acr_login
if "!REGISTRY_CHOICE!"=="2" goto :dockerhub_login
echo ERROR: Invalid registry choice. Please enter 1 or 2.
exit /b 1

REM ── Azure Container Registry ──────────────────────────────────────────────
:acr_login
echo --- Azure Container Registry (ACR) ---
set /p "ACR_NAME=Enter ACR name (e.g., myregistry): "

if "!ACR_NAME!"=="" (
    echo ERROR: ACR name cannot be empty.
    exit /b 1
)

set "REGISTRY=!ACR_NAME!.azurecr.io"
set "FULL_IMAGE_NAME=!REGISTRY!/sptc-application:!IMAGE_TAG!"

echo.
echo Logging in to ACR: !REGISTRY!
az acr login --name !ACR_NAME!
if !ERRORLEVEL! neq 0 (
    echo ERROR: ACR login failed.
    exit /b 1
)
goto :build_image

REM ── Docker Hub ────────────────────────────────────────────────────────────
:dockerhub_login
echo --- Docker Hub ---
set /p "DOCKER_USERNAME=Enter Docker Hub username: "
set /p "DOCKER_PASSWORD=Enter Docker Hub password/token: "
set /p "DOCKER_REPO=Enter Docker Hub repository (e.g., myorg/sptc-application): "

if "!DOCKER_USERNAME!"=="" (
    echo ERROR: Docker Hub username cannot be empty.
    exit /b 1
)
if "!DOCKER_PASSWORD!"=="" (
    echo ERROR: Docker Hub password cannot be empty.
    exit /b 1
)
if "!DOCKER_REPO!"=="" (
    echo ERROR: Docker Hub repository cannot be empty.
    exit /b 1
)

set "FULL_IMAGE_NAME=!DOCKER_REPO!:!IMAGE_TAG!"

echo.
echo Logging in to Docker Hub...
echo !DOCKER_PASSWORD! | docker login --username !DOCKER_USERNAME! --password-stdin
if !ERRORLEVEL! neq 0 (
    echo ERROR: Docker Hub login failed.
    exit /b 1
)
goto :build_image

REM ── Docker build ──────────────────────────────────────────────────────────
:build_image
echo.
echo Building Docker image: !FULL_IMAGE_NAME!
echo Build context: . (repository root)
echo Dockerfile: !DOCKERFILE_PATH!
echo.

docker build -f "!DOCKERFILE_PATH!" -t "!FULL_IMAGE_NAME!" .
if !ERRORLEVEL! neq 0 (
    echo ERROR: Docker build failed.
    exit /b 1
)

echo.
echo Successfully built: !FULL_IMAGE_NAME!
echo.

REM ── Docker push ───────────────────────────────────────────────────────────
echo Pushing image to registry...
docker push "!FULL_IMAGE_NAME!"
if !ERRORLEVEL! neq 0 (
    echo ERROR: Docker push failed.
    exit /b 1
)

echo.
echo ==============================================
echo   Build and push completed successfully!
echo   Image: !FULL_IMAGE_NAME!
echo ==============================================

endlocal
exit /b 0
