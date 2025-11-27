@echo off
setlocal enabledelayedexpansion

echo ==========================================
echo SPTC Application - EKS Deployment
echo ==========================================
echo.

REM Deployment configuration
set APP_NAME=sptc-application
set NAMESPACE=sptc-application

echo Application: %APP_NAME%
echo Namespace: %NAMESPACE%
echo.

REM AWS Configuration
echo AWS EKS Configuration
echo --------------------
set /p AWS_REGION="Enter AWS Region (e.g., us-east-1): "
set /p CLUSTER_NAME="Enter EKS Cluster Name: "
echo.

REM Docker Image Configuration
echo Docker Image Configuration
echo -------------------------
set /p IMAGE_URI="Enter full Docker image URI (registry/repo:tag): "
echo.

REM Database Configuration
echo Database Configuration
echo ---------------------
set /p DB_HOST="Enter MySQL Host (e.g., mysql.example.com): "
set /p DB_PORT="Enter MySQL Port [3306]: "
if "!DB_PORT!"=="" set DB_PORT=3306
set /p DB_NAME="Enter Database Name [dtb_sptc]: "
if "!DB_NAME!"=="" set DB_NAME=dtb_sptc
set /p DB_USER="Enter Database Username [root]: "
if "!DB_USER!"=="" set DB_USER=root
set /p DB_PASSWORD="Enter Database Password: "
echo.

REM Validate inputs
if "!AWS_REGION!"=="" (
    echo ERROR: AWS Region is required
    exit /b 1
)
if "!CLUSTER_NAME!"=="" (
    echo ERROR: Cluster Name is required
    exit /b 1
)
if "!IMAGE_URI!"=="" (
    echo ERROR: Image URI is required
    exit /b 1
)

echo Configuring kubectl for EKS...
aws eks update-kubeconfig --region !AWS_REGION! --name !CLUSTER_NAME!

if !ERRORLEVEL! neq 0 (
    echo ERROR: Failed to configure kubectl
    exit /b 1
)

echo Verifying cluster connectivity...
kubectl cluster-info

if !ERRORLEVEL! neq 0 (
    echo ERROR: Cannot connect to Kubernetes cluster
    exit /b 1
)

echo.
echo Updating Kubernetes manifests...

REM Update deployment.yaml with actual values using PowerShell
powershell -Command "(Get-Content kubernetes/deployment.yaml) -replace '{{IMAGE_URI}}','!IMAGE_URI!' | Set-Content kubernetes/deployment.yaml"
powershell -Command "(Get-Content kubernetes/deployment.yaml) -replace '{{DB_HOST}}','!DB_HOST!' | Set-Content kubernetes/deployment.yaml"
powershell -Command "(Get-Content kubernetes/deployment.yaml) -replace '{{DB_PORT}}','!DB_PORT!' | Set-Content kubernetes/deployment.yaml"
powershell -Command "(Get-Content kubernetes/deployment.yaml) -replace '{{DB_NAME}}','!DB_NAME!' | Set-Content kubernetes/deployment.yaml"
powershell -Command "(Get-Content kubernetes/deployment.yaml) -replace '{{DB_USER}}','!DB_USER!' | Set-Content kubernetes/deployment.yaml"
powershell -Command "(Get-Content kubernetes/deployment.yaml) -replace '{{DB_PASSWORD}}','!DB_PASSWORD!' | Set-Content kubernetes/deployment.yaml"

echo Manifests updated successfully
echo.

REM Apply Kubernetes manifests
echo Deploying to EKS...
echo.

echo 1. Creating namespace...
kubectl apply -f kubernetes/namespace.yaml

echo 2. Deploying application...
kubectl apply -f kubernetes/deployment.yaml

echo 3. Creating service...
kubectl apply -f kubernetes/service.yaml

echo.
echo Waiting for deployment to complete...
kubectl rollout status deployment/%APP_NAME% -n %NAMESPACE% --timeout=600s

if !ERRORLEVEL! neq 0 (
    echo ERROR: Deployment rollout failed
    echo Checking pod status...
    kubectl get pods -n %NAMESPACE%
    echo.
    echo Checking pod logs...
    kubectl logs -n %NAMESPACE% -l app=%APP_NAME% --tail=50
    exit /b 1
)

echo.
echo ==========================================
echo Deployment Completed Successfully!
echo ==========================================
echo.

REM Show deployment status
echo Resource Status:
echo ---------------
kubectl get all -n %NAMESPACE%

echo.
echo Service Details:
echo ---------------
kubectl get svc %APP_NAME%-service -n %NAMESPACE%

echo.
echo ==========================================
echo Access Information
echo ==========================================
echo.
echo This is a WPF Desktop Application running in Windows containers.
echo.
echo To access the application:
echo 1. Get the LoadBalancer external IP:
echo    kubectl get svc %APP_NAME%-service -n %NAMESPACE%
echo.
echo 2. Wait for EXTERNAL-IP to be assigned (may take 2-5 minutes)
echo.
echo 3. Connect via Remote Desktop Protocol (RDP):
echo    - Host: ^<EXTERNAL-IP^>
echo    - Port: 3389
echo    - Username: ContainerUser
echo.
echo 4. Once connected, launch: C:\app\SPTC APPLICATION.exe
echo.
echo NOTE: Windows node pools required in EKS cluster
echo       for .NET Framework 4.8 applications
echo.

endlocal