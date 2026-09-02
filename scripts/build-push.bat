@echo off
setlocal enabledelayedexpansion

REM =============================================================================
REM build-push.bat — SPTC APPLICATION
REM Builds the Docker image and pushes it to Azure ACR or Docker Hub.
REM Run from the repository root directory.
REM =============================================================================

set "PROJECT_NAME=SPTC APPLICATION"
set "DOCKERFILE_PATH=Dockerfile"

echo ==============================================
echo   SPTC APPLICATION - Docker Build and Push
echo ==============================================
echo.

REM ── Tag sanitization (PowerShell) ─────────────────────────────────────────
for /f "delims=" %%i in ('powershell -NoProfile -Command "$n = 'SPTC APPLICATION'.ToLower() -replace '[^a-z0-9]+','-'; $n.Trim('-')"') do set "IMAGE_NAME=%%i"
echo Sanitized image name: !IMAGE_NAME!
echo.

REM ── Prompt for image tag ──────────────────────────────────────────────────
set /p "IMAGE_TAG_INPUT=Enter image tag (press Enter for 'latest'): "
if "!IMAGE_TAG_INPUT!"=="" (
    set "IMAGE_TAG=latest"
) else (
    for /f "delims=" %%t in ('powershell -NoProfile -Command "$t = '!IMAGE_TAG_INPUT!'.ToLower() -replace '[^a-z0-9._-]+','-'; $t.Trim('-'); if ($t -eq '') { 'latest' }"') do set "IMAGE_TAG=%%t"
)
echo Using tag: !IMAGE_TAG!
echo.

REM ── Registry selection ────────────────────────────────────────────────────
echo Select container registry:
echo   1. Azure Container Registry (ACR)
echo   2. Docker Hub
set /p "REGISTRY_CHOICE=Enter choice [1 or 2]: "
echo.

if "!REGISTRY_CHOICE!"=="1" goto ACR_FLOW
if "!REGISTRY_CHOICE!"=="2" goto DOCKERHUB_FLOW
echo ERROR: Invalid registry choice. Please enter 1 or 2.
exit /b 1

REM ── Azure ACR ─────────────────────────────────────────────────────────────
:ACR_FLOW
echo --- Azure Container Registry (ACR) ---
set /p "ACR_NAME=Enter ACR name (e.g. myregistry): "
for /f "delims=" %%a in ('powershell -NoProfile -Command "'!ACR_NAME!'.ToLower() -replace '[^a-z0-9]',''"') do set "ACR_NAME=%%a"

set "FULL_IMAGE_NAME=!ACR_NAME!.azurecr.io/!IMAGE_NAME!:!IMAGE_TAG!"
echo.
echo Full image name: !FULL_IMAGE_NAME!
echo.

echo [1/3] Logging in to Azure ACR: !ACR_NAME! ...
az acr login --name !ACR_NAME!
if !ERRORLEVEL! neq 0 (
    echo ERROR: ACR login failed.
    exit /b 1
)
echo ACR login successful.
echo.

echo [2/3] Building Docker image ...
docker build -f "!DOCKERFILE_PATH!" -t "!FULL_IMAGE_NAME!" .
if !ERRORLEVEL! neq 0 (
    echo ERROR: Docker build failed.
    exit /b 1
)
echo Docker build successful.
echo.

echo [3/3] Pushing image to ACR ...
docker push "!FULL_IMAGE_NAME!"
if !ERRORLEVEL! neq 0 (
    echo ERROR: Docker push failed.
    exit /b 1
)
echo Image pushed successfully: !FULL_IMAGE_NAME!
goto END

REM ── Docker Hub ────────────────────────────────────────────────────────────
:DOCKERHUB_FLOW
echo --- Docker Hub ---
set /p "DOCKER_USERNAME=Enter Docker Hub username: "
set /p "DOCKER_PASSWORD=Enter Docker Hub password/token: "
set /p "DOCKER_REPO=Enter Docker Hub repository name (e.g. myrepo): "

set "FULL_IMAGE_NAME=!DOCKER_USERNAME!/!DOCKER_REPO!:!IMAGE_TAG!"
echo.
echo Full image name: !FULL_IMAGE_NAME!
echo.

echo [1/3] Logging in to Docker Hub ...
echo !DOCKER_PASSWORD! | docker login --username !DOCKER_USERNAME! --password-stdin
if !ERRORLEVEL! neq 0 (
    echo ERROR: Docker Hub login failed.
    exit /b 1
)
echo Docker Hub login successful.
echo.

echo [2/3] Building Docker image ...
docker build -f "!DOCKERFILE_PATH!" -t "!FULL_IMAGE_NAME!" .
if !ERRORLEVEL! neq 0 (
    echo ERROR: Docker build failed.
    exit /b 1
)
echo Docker build successful.
echo.

echo [3/3] Pushing image to Docker Hub ...
docker push "!FULL_IMAGE_NAME!"
if !ERRORLEVEL! neq 0 (
    echo ERROR: Docker push failed.
    exit /b 1
)
echo Image pushed successfully: !FULL_IMAGE_NAME!

:END
echo.
echo ==============================================
echo   Build and Push Complete!
echo   Image: !FULL_IMAGE_NAME!
echo ==============================================
endlocal
