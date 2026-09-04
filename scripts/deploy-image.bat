@echo off
setlocal enabledelayedexpansion

:: =============================================================================
:: deploy-image.bat - Deploy SPTC APPLICATION to Azure AKS
:: =============================================================================

set "APP_NAME=sptc-application"
set "NAMESPACE=sptc-application"
set "K8S_DIR=kubernetes"
set "TEMP_DIR=%TEMP%\sptc-k8s-deploy"

echo ==============================================
echo   SPTC APPLICATION - AKS Deployment
echo ==============================================
echo.

:: ---- Azure / AKS credentials ----
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

:: ---- Docker image URI ----
set /p "IMAGE_URI=Enter full Docker image URI (e.g. myregistry.azurecr.io/sptc-application:latest): "
if "!IMAGE_URI!"=="" (
    echo ERROR: Image URI cannot be empty.
    exit /b 1
)

echo.
echo --- Application Environment Variables ---
echo Press Enter to skip any variable.
echo.

set /p "MYSQL_HOST=Enter MYSQL_HOST (e.g. mysql-server.mysql.database.azure.com): "
set /p "MYSQL_PORT=Enter MYSQL_PORT [3306]: "
if "!MYSQL_PORT!"=="" set "MYSQL_PORT=3306"
set /p "MYSQL_DATABASE=Enter MYSQL_DATABASE [dtb_sptc]: "
if "!MYSQL_DATABASE!"=="" set "MYSQL_DATABASE=dtb_sptc"
set /p "MYSQL_USERNAME=Enter MYSQL_USERNAME: "
set /p "MYSQL_PASSWORD=Enter MYSQL_PASSWORD: "
set /p "REDIS_CONNECTION_STRING=Enter REDIS_CONNECTION_STRING: "

echo.
echo Configuring kubectl for AKS cluster: !CLUSTER_NAME! in resource group: !RESOURCE_GROUP! ...
az aks get-credentials --resource-group !RESOURCE_GROUP! --name !CLUSTER_NAME! --overwrite-existing
if !ERRORLEVEL! neq 0 (
    echo ERROR: Failed to get AKS credentials.
    exit /b 1
)

echo.
echo Verifying cluster connectivity ...
kubectl cluster-info
if !ERRORLEVEL! neq 0 (
    echo ERROR: Cannot connect to AKS cluster.
    exit /b 1
)

echo.
echo Preparing Kubernetes manifests ...
if exist "!TEMP_DIR!" rmdir /s /q "!TEMP_DIR!"
xcopy /e /i /q "!K8S_DIR!" "!TEMP_DIR!" >nul
if !ERRORLEVEL! neq 0 (
    echo ERROR: Failed to copy Kubernetes manifests.
    exit /b 1
)

:: Replace placeholders using PowerShell
echo Updating manifests with provided values ...

powershell -NoProfile -Command ^
  "$f = '!TEMP_DIR!\deployment.yaml'; " ^
  "$c = Get-Content $f -Raw; " ^
  "$c = $c -replace '{{IMAGE_URI}}','!IMAGE_URI!'; " ^
  "if ('!MYSQL_HOST!' -ne '') { $c = $c -replace '{{MYSQL_HOST}}','!MYSQL_HOST!' }; " ^
  "if ('!MYSQL_PORT!' -ne '') { $c = $c -replace '{{MYSQL_PORT}}','!MYSQL_PORT!' }; " ^
  "if ('!MYSQL_DATABASE!' -ne '') { $c = $c -replace '{{MYSQL_DATABASE}}','!MYSQL_DATABASE!' }; " ^
  "if ('!MYSQL_USERNAME!' -ne '') { $c = $c -replace '{{MYSQL_USERNAME}}','!MYSQL_USERNAME!' }; " ^
  "if ('!MYSQL_PASSWORD!' -ne '') { $c = $c -replace '{{MYSQL_PASSWORD}}','!MYSQL_PASSWORD!' }; " ^
  "if ('!REDIS_CONNECTION_STRING!' -ne '') { $c = $c -replace '{{REDIS_CONNECTION_STRING}}','!REDIS_CONNECTION_STRING!' }; " ^
  "Set-Content $f $c -NoNewline"

if !ERRORLEVEL! neq 0 (
    echo ERROR: Failed to update deployment manifest.
    exit /b 1
)

echo.
echo Applying Kubernetes manifests ...

echo   [1/4] Applying namespace ...
kubectl apply -f "!TEMP_DIR!\namespace.yaml"
if !ERRORLEVEL! neq 0 ( echo ERROR: Failed to apply namespace. & exit /b 1 )

echo   [2/4] Applying deployment ...
kubectl apply -f "!TEMP_DIR!\deployment.yaml"
if !ERRORLEVEL! neq 0 ( echo ERROR: Failed to apply deployment. & exit /b 1 )

echo   [3/4] Applying service ...
kubectl apply -f "!TEMP_DIR!\service.yaml"
if !ERRORLEVEL! neq 0 ( echo ERROR: Failed to apply service. & exit /b 1 )

echo   [4/4] Applying ingress ...
kubectl apply -f "!TEMP_DIR!\ingress.yaml"
if !ERRORLEVEL! neq 0 ( echo ERROR: Failed to apply ingress. & exit /b 1 )

echo.
echo Waiting for deployment rollout ...
kubectl rollout status deployment/!APP_NAME! -n !NAMESPACE! --timeout=300s
if !ERRORLEVEL! neq 0 (
    echo ERROR: Deployment rollout failed. Initiating rollback ...
    kubectl rollout undo deployment/!APP_NAME! -n !NAMESPACE!
    echo Rollback initiated. Check pod status with: kubectl get pods -n !NAMESPACE!
    exit /b 1
)

echo.
echo Verifying deployed resources ...
kubectl get pods,svc,ingress -n !NAMESPACE!

echo.
echo ==============================================
echo   Deployment completed successfully!
echo   Namespace : !NAMESPACE!
echo   Image     : !IMAGE_URI!
echo   App URL   : http://sptc-application.example.com
echo   Get IP    : kubectl get ingress -n !NAMESPACE!
echo ==============================================

:: Cleanup temp files
rmdir /s /q "!TEMP_DIR!" >nul 2>&1

endlocal
exit /b 0
