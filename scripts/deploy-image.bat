@echo off
setlocal enabledelayedexpansion

REM =============================================================================
REM deploy-image.bat - Deploy SPTC APPLICATION to Azure AKS
REM Target Platform: Azure Kubernetes Service (AKS)
REM Usage: scripts\deploy-image.bat
REM Run from repository root directory
REM =============================================================================

set "APP_NAME=sptc-application"
set "NAMESPACE=sptc-application"
set "K8S_DIR=kubernetes"

echo ==============================================
echo   SPTC APPLICATION - AKS Deployment
echo ==============================================
echo.

REM ── Prompt for Azure details ──────────────────────────────────────────────
set /p "RESOURCE_GROUP=Enter Azure Resource Group name: "
if "!RESOURCE_GROUP!"=="" (
    echo ERROR: Resource group cannot be empty.
    exit /b 1
)

set /p "CLUSTER_NAME=Enter AKS Cluster name: "
if "!CLUSTER_NAME!"=="" (
    echo ERROR: AKS cluster name cannot be empty.
    exit /b 1
)

set /p "IMAGE_URI=Enter full Docker image URI (e.g., myregistry.azurecr.io/sptc-application:latest): "
if "!IMAGE_URI!"=="" (
    echo ERROR: Image URI cannot be empty.
    exit /b 1
)

echo.
echo --- Application Configuration ---
echo The following environment variables are required by SPTC APPLICATION.
echo Press Enter to use the default value shown.
echo.

set /p "MYSQL_HOST_VAL=Enter MySQL host (default: localhost): "
if "!MYSQL_HOST_VAL!"=="" set "MYSQL_HOST_VAL=localhost"

set /p "MYSQL_PORT_VAL=Enter MySQL port (default: 3306): "
if "!MYSQL_PORT_VAL!"=="" set "MYSQL_PORT_VAL=3306"

set /p "MYSQL_DB_VAL=Enter MySQL database name (default: dtb_sptc): "
if "!MYSQL_DB_VAL!"=="" set "MYSQL_DB_VAL=dtb_sptc"

echo.
echo --- Configuring kubectl for AKS cluster ---
az aks get-credentials --resource-group !RESOURCE_GROUP! --name !CLUSTER_NAME! --overwrite-existing
if !ERRORLEVEL! neq 0 (
    echo ERROR: Failed to get AKS credentials.
    exit /b 1
)

echo.
echo Verifying cluster connectivity...
kubectl cluster-info
if !ERRORLEVEL! neq 0 (
    echo ERROR: Cannot connect to AKS cluster.
    exit /b 1
)

echo.
echo --- Updating Kubernetes manifests ---

REM Create working copies
copy /Y "!K8S_DIR!\deployment.yaml" "!K8S_DIR!\deployment.yaml.tmp" >nul
copy /Y "!K8S_DIR!\ingress.yaml" "!K8S_DIR!\ingress.yaml.tmp" >nul

REM Replace placeholders using PowerShell (handles special characters safely)
powershell -Command "(Get-Content '!K8S_DIR!\deployment.yaml.tmp') -replace '\{\{IMAGE_URI\}\}', '!IMAGE_URI!' | Set-Content '!K8S_DIR!\deployment.yaml.tmp'"
if !ERRORLEVEL! neq 0 (
    echo ERROR: Failed to update IMAGE_URI placeholder.
    exit /b 1
)

powershell -Command "(Get-Content '!K8S_DIR!\deployment.yaml.tmp') -replace '\{\{MYSQL_HOST\}\}', '!MYSQL_HOST_VAL!' | Set-Content '!K8S_DIR!\deployment.yaml.tmp'"
powershell -Command "(Get-Content '!K8S_DIR!\deployment.yaml.tmp') -replace '\{\{MYSQL_PORT\}\}', '!MYSQL_PORT_VAL!' | Set-Content '!K8S_DIR!\deployment.yaml.tmp'"
powershell -Command "(Get-Content '!K8S_DIR!\deployment.yaml.tmp') -replace '\{\{MYSQL_DATABASE\}\}', '!MYSQL_DB_VAL!' | Set-Content '!K8S_DIR!\deployment.yaml.tmp'"

echo.
echo --- Applying Kubernetes manifests ---

echo 1/4 Applying namespace...
kubectl apply -f "!K8S_DIR!\namespace.yaml"
if !ERRORLEVEL! neq 0 (
    echo ERROR: Failed to apply namespace.
    exit /b 1
)

echo 2/4 Applying deployment...
kubectl apply -f "!K8S_DIR!\deployment.yaml.tmp"
if !ERRORLEVEL! neq 0 (
    echo ERROR: Failed to apply deployment.
    exit /b 1
)

echo 3/4 Applying service...
kubectl apply -f "!K8S_DIR!\service.yaml"
if !ERRORLEVEL! neq 0 (
    echo ERROR: Failed to apply service.
    exit /b 1
)

echo 4/4 Applying ingress...
kubectl apply -f "!K8S_DIR!\ingress.yaml.tmp"
if !ERRORLEVEL! neq 0 (
    echo ERROR: Failed to apply ingress.
    exit /b 1
)

REM Clean up temp files
del /f /q "!K8S_DIR!\deployment.yaml.tmp" 2>nul
del /f /q "!K8S_DIR!\ingress.yaml.tmp" 2>nul

echo.
echo --- Waiting for deployment rollout ---
kubectl rollout status deployment/!APP_NAME! -n !NAMESPACE! --timeout=300s
if !ERRORLEVEL! neq 0 (
    echo ERROR: Deployment rollout failed. Check pod logs:
    echo   kubectl logs -l app=!APP_NAME! -n !NAMESPACE!
    echo.
    echo To rollback: kubectl rollout undo deployment/!APP_NAME! -n !NAMESPACE!
    exit /b 1
)

echo.
echo --- Verifying deployed resources ---
kubectl get pods,svc,ingress -n !NAMESPACE!

echo.
echo ==============================================
echo   Deployment completed successfully!
echo   Namespace: !NAMESPACE!
echo   Image    : !IMAGE_URI!
echo ==============================================
echo.
echo Useful commands:
echo   kubectl get pods -n !NAMESPACE!
echo   kubectl logs -l app=!APP_NAME! -n !NAMESPACE!
echo   kubectl describe deployment !APP_NAME! -n !NAMESPACE!
echo   kubectl rollout undo deployment/!APP_NAME! -n !NAMESPACE!

endlocal
exit /b 0
