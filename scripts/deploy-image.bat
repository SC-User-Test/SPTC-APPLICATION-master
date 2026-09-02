@echo off
setlocal enabledelayedexpansion

REM =============================================================================
REM deploy-image.bat — SPTC APPLICATION
REM Deploys the application to Azure Kubernetes Service (AKS).
REM Run from the repository root directory.
REM =============================================================================

set "APP_NAME=sptc-application"
set "NAMESPACE=sptc-application"

echo ==============================================
echo   SPTC APPLICATION - Deploy to Azure AKS
echo ==============================================
echo.

REM ── Azure resource group and AKS cluster ─────────────────────────────────
set /p "RESOURCE_GROUP=Enter Azure Resource Group name: "
if "!RESOURCE_GROUP!"=="" (
    echo ERROR: Resource group name cannot be empty.
    exit /b 1
)

set /p "CLUSTER_NAME=Enter AKS Cluster name: "
if "!CLUSTER_NAME!"=="" (
    echo ERROR: AKS cluster name cannot be empty.
    exit /b 1
)

REM ── Docker image URI ──────────────────────────────────────────────────────
set /p "IMAGE_URI=Enter full Docker image URI (e.g. myregistry.azurecr.io/sptc-application:latest): "
if "!IMAGE_URI!"=="" (
    echo ERROR: Docker image URI cannot be empty.
    exit /b 1
)

echo.
echo --- Application Environment Variables ---
echo Press Enter to skip any variable.
echo.

REM ── Prompt for application-specific environment variables ─────────────────
set /p "MYSQL_HOST_VAL=Enter value for MYSQL_HOST (MySQL server hostname): "
set /p "MYSQL_PORT_VAL=Enter value for MYSQL_PORT (default: 3306): "
set /p "MYSQL_DATABASE_VAL=Enter value for MYSQL_DATABASE (default: dtb_sptc): "
set /p "MYSQL_USERNAME_VAL=Enter value for MYSQL_USERNAME: "
set /p "MYSQL_PASSWORD_VAL=Enter value for MYSQL_PASSWORD: "
set /p "REDIS_CONNECTION_STRING_VAL=Enter value for REDIS_CONNECTION_STRING: "

if "!MYSQL_PORT_VAL!"=="" set "MYSQL_PORT_VAL=3306"
if "!MYSQL_DATABASE_VAL!"=="" set "MYSQL_DATABASE_VAL=dtb_sptc"

echo.
echo ==============================================
echo   Deployment Configuration
echo   Resource Group : !RESOURCE_GROUP!
echo   AKS Cluster    : !CLUSTER_NAME!
echo   Image URI      : !IMAGE_URI!
echo   Namespace      : !NAMESPACE!
echo ==============================================
echo.

REM ── Configure kubectl for AKS ─────────────────────────────────────────────
echo [1/7] Configuring kubectl for AKS cluster ...
az aks get-credentials --resource-group !RESOURCE_GROUP! --name !CLUSTER_NAME! --overwrite-existing
if !ERRORLEVEL! neq 0 (
    echo ERROR: Failed to configure kubectl for AKS.
    exit /b 1
)
echo kubectl configured successfully.
echo.

REM ── Verify cluster connectivity ───────────────────────────────────────────
echo [2/7] Verifying cluster connectivity ...
kubectl cluster-info
if !ERRORLEVEL! neq 0 (
    echo ERROR: Cannot connect to AKS cluster.
    exit /b 1
)
echo.

REM ── Update Kubernetes manifests ───────────────────────────────────────────
echo [3/7] Updating Kubernetes manifests ...

copy kubernetes\deployment.yaml kubernetes\deployment.yaml.bak >nul
copy kubernetes\service.yaml kubernetes\service.yaml.bak >nul
copy kubernetes\ingress.yaml kubernetes\ingress.yaml.bak >nul
copy kubernetes\namespace.yaml kubernetes\namespace.yaml.bak >nul

powershell -NoProfile -Command "(Get-Content 'kubernetes\deployment.yaml') -replace '\{\{IMAGE_URI\}\}', '!IMAGE_URI!' | Set-Content 'kubernetes\deployment.yaml'"
if !ERRORLEVEL! neq 0 ( echo ERROR: Failed to update IMAGE_URI. & exit /b 1 )

if not "!MYSQL_HOST_VAL!"=="" (
    powershell -NoProfile -Command "(Get-Content 'kubernetes\deployment.yaml') -replace '\{\{MYSQL_HOST\}\}', '!MYSQL_HOST_VAL!' | Set-Content 'kubernetes\deployment.yaml'"
)
if not "!MYSQL_PORT_VAL!"=="" (
    powershell -NoProfile -Command "(Get-Content 'kubernetes\deployment.yaml') -replace '\{\{MYSQL_PORT\}\}', '!MYSQL_PORT_VAL!' | Set-Content 'kubernetes\deployment.yaml'"
)
if not "!MYSQL_DATABASE_VAL!"=="" (
    powershell -NoProfile -Command "(Get-Content 'kubernetes\deployment.yaml') -replace '\{\{MYSQL_DATABASE\}\}', '!MYSQL_DATABASE_VAL!' | Set-Content 'kubernetes\deployment.yaml'"
)
if not "!MYSQL_USERNAME_VAL!"=="" (
    powershell -NoProfile -Command "(Get-Content 'kubernetes\deployment.yaml') -replace '\{\{MYSQL_USERNAME\}\}', '!MYSQL_USERNAME_VAL!' | Set-Content 'kubernetes\deployment.yaml'"
)
if not "!MYSQL_PASSWORD_VAL!"=="" (
    powershell -NoProfile -Command "(Get-Content 'kubernetes\deployment.yaml') -replace '\{\{MYSQL_PASSWORD\}\}', '!MYSQL_PASSWORD_VAL!' | Set-Content 'kubernetes\deployment.yaml'"
)
if not "!REDIS_CONNECTION_STRING_VAL!"=="" (
    powershell -NoProfile -Command "(Get-Content 'kubernetes\deployment.yaml') -replace '\{\{REDIS_CONNECTION_STRING\}\}', '!REDIS_CONNECTION_STRING_VAL!' | Set-Content 'kubernetes\deployment.yaml'"
)

echo Manifests updated.
echo.

REM ── Apply Kubernetes manifests ────────────────────────────────────────────
echo [4/7] Applying Kubernetes manifests ...

echo   Applying namespace ...
kubectl apply -f kubernetes\namespace.yaml
if !ERRORLEVEL! neq 0 ( echo ERROR: Failed to apply namespace. & goto RESTORE )

echo   Applying deployment ...
kubectl apply -f kubernetes\deployment.yaml
if !ERRORLEVEL! neq 0 ( echo ERROR: Failed to apply deployment. & goto RESTORE )

echo   Applying service ...
kubectl apply -f kubernetes\service.yaml
if !ERRORLEVEL! neq 0 ( echo ERROR: Failed to apply service. & goto RESTORE )

echo   Applying ingress ...
kubectl apply -f kubernetes\ingress.yaml
if !ERRORLEVEL! neq 0 ( echo ERROR: Failed to apply ingress. & goto RESTORE )

echo Manifests applied successfully.
echo.

REM ── Restore original manifests ────────────────────────────────────────────
:RESTORE
move /y kubernetes\deployment.yaml.bak kubernetes\deployment.yaml >nul
move /y kubernetes\service.yaml.bak kubernetes\service.yaml >nul
move /y kubernetes\ingress.yaml.bak kubernetes\ingress.yaml >nul
move /y kubernetes\namespace.yaml.bak kubernetes\namespace.yaml >nul

REM ── Wait for rollout ──────────────────────────────────────────────────────
echo [5/7] Waiting for deployment rollout ...
kubectl rollout status deployment/!APP_NAME! -n !NAMESPACE! --timeout=300s
if !ERRORLEVEL! neq 0 (
    echo ERROR: Deployment rollout failed.
    echo Rollback command: kubectl rollout undo deployment/!APP_NAME! -n !NAMESPACE!
    exit /b 1
)
echo Deployment rollout complete.
echo.

REM ── Verify resources ──────────────────────────────────────────────────────
echo [6/7] Verifying deployed resources ...
kubectl get pods,svc,ingress -n !NAMESPACE!
echo.

REM ── Display application URL ───────────────────────────────────────────────
echo [7/7] Deployment complete.
echo.
echo ==============================================
echo   Deployment Complete!
echo   Health Check   : http://^<pod-ip^>:8080/health
echo   Namespace      : !NAMESPACE!
echo ==============================================
echo.
echo Rollback command (if needed):
echo   kubectl rollout undo deployment/!APP_NAME! -n !NAMESPACE!

endlocal
