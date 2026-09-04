# SPTC APPLICATION – Deployment Guide

## Overview

This guide covers building, pushing, and deploying the **SPTC APPLICATION** (ASP.NET Core / .NET 8.0) to **Azure Kubernetes Service (AKS)**.

The application was originally a .NET Framework 4.8 WPF desktop application and has been migrated to a headless ASP.NET Core web application targeting Linux containers on AKS.

---

## Table of Contents

1. [Prerequisites](#prerequisites)
2. [Project Structure](#project-structure)
3. [Local Development with Docker Compose](#local-development-with-docker-compose)
4. [Build and Push Docker Image](#build-and-push-docker-image)
5. [Azure AKS Deployment](#azure-aks-deployment)
6. [Kubernetes Manifest Descriptions](#kubernetes-manifest-descriptions)
7. [Configuration and Environment Variables](#configuration-and-environment-variables)
8. [Scaling and Management](#scaling-and-management)
9. [Troubleshooting](#troubleshooting)
10. [Security Considerations](#security-considerations)
11. [.NET-Specific Notes](#net-specific-notes)

---

## Prerequisites

### Local Development
- [Docker Desktop](https://www.docker.com/products/docker-desktop) 20.10+
- [Docker Compose](https://docs.docker.com/compose/) v2+
- .NET 8.0 SDK (for local builds without Docker)

### Azure AKS Deployment
- [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli) 2.50+
- [kubectl](https://kubernetes.io/docs/tasks/tools/) 1.27+
- Active Azure subscription with:
  - Azure Kubernetes Service (AKS) cluster
  - Azure Container Registry (ACR) or Docker Hub account
  - Azure Cache for Redis (required for state management)
  - Azure Database for MySQL (required for data persistence)

### Install Azure CLI
```bash
# macOS
brew install azure-cli

# Ubuntu/Debian
curl -sL https://aka.ms/InstallAzureCLIDeb | sudo bash

# Windows
winget install Microsoft.AzureCLI
```

### Install kubectl
```bash
# macOS
brew install kubectl

# Ubuntu/Debian
sudo az aks install-cli

# Windows
az aks install-cli
```

---

## Project Structure

```
Dotnet container testing/
├── Dockerfile                    # Multi-stage build (SDK → runtime)
├── .dockerignore                 # Excludes build artefacts from context
├── docker-compose.yml            # Local development compose file
├── SPTC APPLICATION.csproj       # SDK-style project file (net8.0)
├── SPTC APPLICATION.sln          # Visual Studio solution
├── Program.cs                    # ASP.NET Core entry point
├── HealthCheckService.cs         # /health endpoint implementation
├── Infrastructure/
│   └── RedisStateProvider.cs     # Redis-backed state management
├── kubernetes/
│   ├── namespace.yaml            # Kubernetes namespace
│   ├── deployment.yaml           # Application deployment
│   ├── service.yaml              # ClusterIP service
│   └── ingress.yaml              # Azure Application Gateway ingress
├── scripts/
│   ├── build-push.sh             # Linux/macOS build & push script
│   ├── build-push.bat            # Windows build & push script
│   ├── deploy-image.sh           # Linux/macOS AKS deploy script
│   └── deploy-image.bat          # Windows AKS deploy script
└── docs/
    └── DEPLOYMENT.md             # This file
```

---

## Local Development with Docker Compose

### 1. Configure Environment Variables

Create a `.env` file in the project root:

```env
MYSQL_HOST=localhost
MYSQL_PORT=3306
MYSQL_DATABASE=dtb_sptc
MYSQL_USERNAME=root
MYSQL_PASSWORD=your_password
REDIS_CONNECTION_STRING=localhost:6379
```

> **Note**: The `.env` file is automatically loaded by Docker Compose. Never commit this file to source control.

### 2. Start the Application

```bash
# Build and start the application container
docker-compose up --build

# Run in detached mode
docker-compose up -d --build

# View logs
docker-compose logs -f sptc-application

# Stop the application
docker-compose down
```

### 3. Verify the Application

```bash
# Check health endpoint
curl http://localhost:8080/health

# Expected response:
# {"status":"UP","application":"SPTC-APPLICATION"}
```

---

## Build and Push Docker Image

### Linux / macOS

```bash
# Make the script executable
chmod +x scripts/build-push.sh

# Run from the project root
./scripts/build-push.sh
```

The script will prompt you to:
1. Enter an image tag (default: `latest`)
2. Select registry type (ACR or Docker Hub)
3. Provide registry credentials

### Windows

```cmd
# Run from the project root
scripts\build-push.bat
```

### Manual Build (without scripts)

```bash
# Build the image
docker build -f Dockerfile -t sptc-application:latest .

# Tag for ACR
docker tag sptc-application:latest <acr-name>.azurecr.io/sptc-application:latest

# Push to ACR
az acr login --name <acr-name>
docker push <acr-name>.azurecr.io/sptc-application:latest
```

---

## Azure AKS Deployment

### Step 1: Create AKS Cluster (if not existing)

```bash
# Login to Azure
az login

# Create resource group
az group create --name sptc-rg --location eastus

# Create AKS cluster
az aks create \
  --resource-group sptc-rg \
  --name sptc-aks \
  --node-count 2 \
  --node-vm-size Standard_D2s_v3 \
  --enable-addons monitoring \
  --generate-ssh-keys

# Attach ACR to AKS (allows AKS to pull images from ACR without credentials)
az aks update \
  --resource-group sptc-rg \
  --name sptc-aks \
  --attach-acr <acr-name>
```

### Step 2: Install Application Gateway Ingress Controller (AGIC)

```bash
# Enable AGIC add-on
az aks enable-addons \
  --resource-group sptc-rg \
  --name sptc-aks \
  --addons ingress-appgw \
  --appgw-name sptc-appgw \
  --appgw-subnet-cidr "10.225.0.0/16"
```

### Step 3: Deploy Using Script

#### Linux / macOS
```bash
chmod +x scripts/deploy-image.sh
./scripts/deploy-image.sh
```

#### Windows
```cmd
scripts\deploy-image.bat
```

The script will prompt for:
- Azure Resource Group name
- AKS Cluster name
- Full Docker image URI (e.g., `myregistry.azurecr.io/sptc-application:v1.0.0`)
- MySQL connection details
- Redis connection string

### Step 4: Manual Deployment (without scripts)

```bash
# Configure kubectl
az aks get-credentials --resource-group sptc-rg --name sptc-aks

# Update image URI in deployment manifest
sed -i 's|{{IMAGE_URI}}|myregistry.azurecr.io/sptc-application:latest|g' kubernetes/deployment.yaml

# Apply manifests in order
kubectl apply -f kubernetes/namespace.yaml
kubectl apply -f kubernetes/deployment.yaml
kubectl apply -f kubernetes/service.yaml
kubectl apply -f kubernetes/ingress.yaml

# Wait for rollout
kubectl rollout status deployment/sptc-application -n sptc-application

# Verify
kubectl get pods,svc,ingress -n sptc-application
```

---

## Kubernetes Manifest Descriptions

### namespace.yaml
Creates the `sptc-application` Kubernetes namespace to isolate all application resources.

### deployment.yaml
Defines the application deployment with:
- **2 replicas** for high availability
- **Rolling update** strategy (zero-downtime deployments)
- **Resource limits**: CPU 500m / Memory 1Gi; Requests: CPU 250m / Memory 512Mi
- **Liveness probe**: `GET /health` every 30s (restarts unhealthy pods)
- **Readiness probe**: `GET /health` every 15s (removes unready pods from load balancer)
- **Non-root user** (UID 1001) for security
- **Environment variables** for MySQL and Redis configuration

### service.yaml
Creates a `ClusterIP` service that routes traffic to application pods on port 8080.

### ingress.yaml
Configures Azure Application Gateway Ingress Controller to expose the application externally.
Update the `host` field (`sptc-application.example.com`) to your actual domain name.

---

## Configuration and Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `ASPNETCORE_ENVIRONMENT` | ASP.NET Core environment | `Production` |
| `ASPNETCORE_URLS` | Kestrel listen URLs | `http://+:8080` |
| `PORT` | Application port | `8080` |
| `HEALTH_CHECK_PORT` | Health check listener port | `8080` |
| `MYSQL_HOST` | MySQL server hostname | *(required)* |
| `MYSQL_PORT` | MySQL server port | `3306` |
| `MYSQL_DATABASE` | MySQL database name | `dtb_sptc` |
| `MYSQL_USERNAME` | MySQL username | *(required)* |
| `MYSQL_PASSWORD` | MySQL password | *(required)* |
| `REDIS_CONNECTION_STRING` | Redis connection string | *(required)* |

### Using Azure Key Vault CSI Driver (Recommended for Production)

```bash
# Install Secrets Store CSI Driver
az aks enable-addons \
  --addons azure-keyvault-secrets-provider \
  --name sptc-aks \
  --resource-group sptc-rg

# Create Key Vault secrets
az keyvault secret set --vault-name sptc-kv --name mysql-password --value "your_password"
az keyvault secret set --vault-name sptc-kv --name redis-connection-string --value "your_redis_conn"
```

---

## Scaling and Management

### Manual Scaling

```bash
# Scale to 3 replicas
kubectl scale deployment sptc-application -n sptc-application --replicas=3

# Check scaling status
kubectl get pods -n sptc-application
```

### Horizontal Pod Autoscaler (HPA)

```bash
# Create HPA (scale between 2-10 pods based on CPU)
kubectl autoscale deployment sptc-application \
  -n sptc-application \
  --cpu-percent=70 \
  --min=2 \
  --max=10

# Check HPA status
kubectl get hpa -n sptc-application
```

### Rolling Updates

```bash
# Update image
kubectl set image deployment/sptc-application \
  sptc-application=myregistry.azurecr.io/sptc-application:v2.0.0 \
  -n sptc-application

# Monitor rollout
kubectl rollout status deployment/sptc-application -n sptc-application
```

### Rollback

```bash
# Rollback to previous version
kubectl rollout undo deployment/sptc-application -n sptc-application

# Rollback to specific revision
kubectl rollout history deployment/sptc-application -n sptc-application
kubectl rollout undo deployment/sptc-application -n sptc-application --to-revision=2
```

---

## Troubleshooting

### Pod Not Starting

```bash
# Check pod status
kubectl get pods -n sptc-application

# Describe pod for events
kubectl describe pod <pod-name> -n sptc-application

# View pod logs
kubectl logs <pod-name> -n sptc-application

# View previous pod logs (if crashed)
kubectl logs <pod-name> -n sptc-application --previous
```

### Health Check Failures

```bash
# Test health endpoint from within the cluster
kubectl exec -it <pod-name> -n sptc-application -- wget -qO- http://localhost:8080/health

# Check liveness/readiness probe events
kubectl describe pod <pod-name> -n sptc-application | grep -A 10 "Liveness\|Readiness"
```

### Image Pull Errors

```bash
# Check if ACR is attached to AKS
az aks check-acr --resource-group sptc-rg --name sptc-aks --acr <acr-name>

# Re-attach ACR
az aks update --resource-group sptc-rg --name sptc-aks --attach-acr <acr-name>
```

### Service / Ingress Issues

```bash
# Check service endpoints
kubectl get endpoints sptc-application-service -n sptc-application

# Check ingress status
kubectl describe ingress sptc-application-ingress -n sptc-application

# Get ingress IP
kubectl get ingress -n sptc-application
```

### Redis Connection Issues

```bash
# Verify REDIS_CONNECTION_STRING is set correctly
kubectl exec -it <pod-name> -n sptc-application -- env | grep REDIS

# Check application logs for Redis errors
kubectl logs <pod-name> -n sptc-application | grep -i redis
```

---

## Security Considerations

1. **Non-root container**: The application runs as UID 1001 (non-root) for security.
2. **Secrets management**: Use Azure Key Vault CSI Driver for sensitive values (passwords, connection strings). Never store secrets in Kubernetes manifests or environment variables in plain text for production.
3. **Network policies**: Consider adding Kubernetes NetworkPolicy to restrict pod-to-pod communication.
4. **Image scanning**: Enable Azure Defender for Containers to scan images for vulnerabilities.
5. **RBAC**: Use Kubernetes RBAC and Azure AD integration for access control.
6. **TLS**: Configure TLS termination at the Application Gateway level for HTTPS.
7. **Resource limits**: Always set resource requests and limits to prevent resource exhaustion.

---

## .NET-Specific Notes

### Multi-Stage Build
The Dockerfile uses a two-stage build:
- **Stage 1** (`mcr.microsoft.com/dotnet/sdk:8.0`): Restores NuGet packages and publishes the application.
- **Stage 2** (`mcr.microsoft.com/dotnet/framework/sdk:4.8`): Contains only the published output (no SDK, no source code).

### Health Endpoint
The `/health` endpoint is implemented in `HealthCheckService.cs` and registered via `app.MapHealthChecks("/health")` in `Program.cs`. It returns:
```json
{"status":"UP","application":"SPTC-APPLICATION"}
```

### Redis State Management
The application uses `RedisStateProvider.cs` to store shared state in Azure Cache for Redis. The `REDIS_CONNECTION_STRING` environment variable must be set for the application to function correctly in a multi-replica deployment.

### .NET GC Configuration
For containerized .NET applications, consider setting:
```yaml
env:
  - name: DOTNET_GCHeapHardLimit
    value: "805306368"  # 768 MB (75% of 1Gi memory limit)
  - name: DOTNET_GCConservatoryMode
    value: "1"
```

### Startup Time
.NET 8 applications typically start in 2-5 seconds. The `initialDelaySeconds: 20` in the readiness probe accounts for this. Adjust if needed based on observed startup times.

---

*Generated for SPTC APPLICATION – Azure AKS Deployment*
