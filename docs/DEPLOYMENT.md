# SPTC APPLICATION — Deployment Guide
## Azure Kubernetes Service (AKS) Deployment

---

## Table of Contents
1. [Overview](#overview)
2. [Prerequisites](#prerequisites)
3. [Local Development with Docker Compose](#local-development-with-docker-compose)
4. [Build and Push Docker Image](#build-and-push-docker-image)
5. [Azure AKS Deployment](#azure-aks-deployment)
6. [Kubernetes Manifest Descriptions](#kubernetes-manifest-descriptions)
7. [Configuration Management](#configuration-management)
8. [AKS Scaling and Management](#aks-scaling-and-management)
9. [Troubleshooting](#troubleshooting)
10. [Security Considerations](#security-considerations)
11. [Technology-Specific Notes](#technology-specific-notes)

---

## Overview

**Application**: SPTC APPLICATION  
**Technology**: ASP.NET Core (.NET 8.0) — migrated from .NET Framework 4.8 WPF  
**Runtime Base Image**: `mcr.microsoft.com/dotnet/framework/aspnet:4.8` (explicit)  
**Build Image**: `mcr.microsoft.com/dotnet/sdk:8.0`  
**Application Port**: `8080`  
**Health Endpoint**: `GET /health`  
**Target Platform**: Azure Kubernetes Service (AKS)  
**Dependencies**: MySQL (database), Azure Cache for Redis (distributed state)

---

## Prerequisites

### Local Development
- Docker Desktop 24.x or later
- Docker Compose v2.x or later
- .NET 8.0 SDK (for local development without Docker)

### Azure AKS Deployment
- Azure CLI (`az`) 2.50.0 or later — [Install](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli)
- `kubectl` 1.27 or later — [Install](https://kubernetes.io/docs/tasks/tools/)
- Active Azure subscription with:
  - Azure Container Registry (ACR) or Docker Hub account
  - AKS cluster provisioned
  - Application Gateway Ingress Controller (AGIC) installed on AKS
- Azure Cache for Redis instance
- MySQL server (Azure Database for MySQL or self-managed)

---

## Local Development with Docker Compose

### 1. Configure Environment Variables

Create a `.env` file in the project root:

```env
MYSQL_HOST=your-mysql-host
MYSQL_PORT=3306
MYSQL_DATABASE=dtb_sptc
MYSQL_USERNAME=your-username
MYSQL_PASSWORD=your-password
REDIS_CONNECTION_STRING=your-redis-host:6380,password=your-redis-key,ssl=True,abortConnect=False
```

### 2. Build and Start the Application

```bash
# Build and start the application container
docker compose up --build

# Run in detached mode
docker compose up --build -d

# View logs
docker compose logs -f sptc-application

# Stop the application
docker compose down
```

### 3. Verify the Application

```bash
# Check health endpoint
curl http://localhost:8080/health

# Expected response:
# {"status":"Healthy"}
```

---

## Build and Push Docker Image

### Linux/macOS — build-push.sh

```bash
# Make the script executable
chmod +x scripts/build-push.sh

# Run from the repository root
./scripts/build-push.sh
```

The script will prompt you to:
1. Enter an image tag (default: `latest`)
2. Select registry type (1 = Azure ACR, 2 = Docker Hub)
3. Enter registry credentials

**Example for Azure ACR:**
```
Enter image tag (press Enter for 'latest'): v1.0.0
Select container registry:
  1. Azure Container Registry (ACR)
  2. Docker Hub
Enter choice [1 or 2]: 1
Enter ACR name (e.g. myregistry): myacrregistry
```

### Windows — build-push.bat

```cmd
REM Run from the repository root
scripts\build-push.bat
```

---

## Azure AKS Deployment

### Step 1: Provision AKS Cluster (if not already done)

```bash
# Create resource group
az group create --name sptc-rg --location eastus

# Create AKS cluster with Application Gateway Ingress Controller
az aks create \
  --resource-group sptc-rg \
  --name sptc-aks \
  --node-count 2 \
  --enable-addons ingress-appgw \
  --appgw-name sptc-appgw \
  --appgw-subnet-cidr "10.225.0.0/16" \
  --generate-ssh-keys

# Attach ACR to AKS (if using Azure ACR)
az aks update \
  --resource-group sptc-rg \
  --name sptc-aks \
  --attach-acr <your-acr-name>
```

### Step 2: Build and Push the Image

```bash
chmod +x scripts/build-push.sh
./scripts/build-push.sh
# Follow prompts to push to your registry
```

### Step 3: Deploy to AKS

#### Linux/macOS — deploy-image.sh

```bash
chmod +x scripts/deploy-image.sh
./scripts/deploy-image.sh
```

The script will prompt for:
- Azure Resource Group name
- AKS Cluster name
- Full Docker image URI (e.g., `myregistry.azurecr.io/sptc-application:v1.0.0`)
- MySQL connection details
- Redis connection string

#### Windows — deploy-image.bat

```cmd
scripts\deploy-image.bat
```

### Step 4: Verify Deployment

```bash
# Check pods
kubectl get pods -n sptc-application

# Check services
kubectl get svc -n sptc-application

# Check ingress
kubectl get ingress -n sptc-application

# View pod logs
kubectl logs -l app=sptc-application -n sptc-application --tail=100

# Check health endpoint from within cluster
kubectl exec -it <pod-name> -n sptc-application -- wget -qO- http://localhost:8080/health
```

---

## Kubernetes Manifest Descriptions

### `kubernetes/namespace.yaml`
Creates the `sptc-application` namespace to isolate all application resources.

### `kubernetes/deployment.yaml`
Defines the application deployment with:
- **2 replicas** for high availability
- **Resource limits**: CPU 500m, Memory 1Gi
- **Resource requests**: CPU 250m, Memory 512Mi
- **Liveness probe**: `GET /health` on port 8080 (starts after 15s, every 20s)
- **Readiness probe**: `GET /health` on port 8080 (starts after 5s, every 10s)
- **Non-root security context**: runs as UID 1000
- **Environment variable placeholders** for MySQL and Redis (replaced by deploy script)

### `kubernetes/service.yaml`
Creates a `ClusterIP` service exposing port 80 → container port 8080.

### `kubernetes/ingress.yaml`
Creates an Azure Application Gateway Ingress with:
- Host: `sptc-application.example.com` (update to your actual domain)
- Path: `/` (all traffic routed to the application)
- Backend: `sptc-application-service:80`

**Update the ingress host before deploying:**
```bash
# Edit kubernetes/ingress.yaml and replace:
# host: sptc-application.example.com
# with your actual domain name
```

---

## Configuration Management

### Environment Variables

| Variable | Description | Required |
|---|---|---|
| `ASPNETCORE_ENVIRONMENT` | ASP.NET Core environment | Yes (default: Production) |
| `ASPNETCORE_URLS` | Kestrel binding URL | Yes (default: http://+:8080) |
| `MYSQL_HOST` | MySQL server hostname | Yes |
| `MYSQL_PORT` | MySQL server port | Yes (default: 3306) |
| `MYSQL_DATABASE` | MySQL database name | Yes (default: dtb_sptc) |
| `MYSQL_USERNAME` | MySQL username | Yes |
| `MYSQL_PASSWORD` | MySQL password | Yes |
| `REDIS_CONNECTION_STRING` | Azure Cache for Redis connection string | Yes |
| `APPSTATE_DIR` | Directory for AppState.json persistence | No (default: /app/Config) |
| `HEALTH_CHECK_PORT` | Health check listener port | No (default: 8080) |

### Using Azure Key Vault with AKS Secrets Store CSI Driver

For production deployments, use the Secrets Store CSI Driver to inject secrets from Azure Key Vault:

```bash
# Enable the Secrets Store CSI Driver add-on
az aks enable-addons \
  --addons azure-keyvault-secrets-provider \
  --name sptc-aks \
  --resource-group sptc-rg

# Create a SecretProviderClass to mount Key Vault secrets
# (see Azure documentation for full configuration)
```

---

## AKS Scaling and Management

### Manual Scaling

```bash
# Scale to 3 replicas
kubectl scale deployment sptc-application --replicas=3 -n sptc-application
```

### Horizontal Pod Autoscaler (HPA)

```bash
# Create HPA (scale between 2-10 pods based on CPU)
kubectl autoscale deployment sptc-application \
  --cpu-percent=70 \
  --min=2 \
  --max=10 \
  -n sptc-application

# Check HPA status
kubectl get hpa -n sptc-application
```

### Rolling Updates

```bash
# Update the image
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
kubectl rollout undo deployment/sptc-application --to-revision=2 -n sptc-application
```

---

## Troubleshooting

### Pod Not Starting

```bash
# Check pod status
kubectl describe pod <pod-name> -n sptc-application

# Check pod logs
kubectl logs <pod-name> -n sptc-application

# Check events
kubectl get events -n sptc-application --sort-by='.lastTimestamp'
```

### Health Check Failures

```bash
# Verify health endpoint is accessible
kubectl port-forward svc/sptc-application-service 8080:80 -n sptc-application
# Then in another terminal:
curl http://localhost:8080/health
```

### MySQL Connection Issues

```bash
# Verify MySQL environment variables are set correctly
kubectl exec -it <pod-name> -n sptc-application -- env | grep MYSQL

# Check application logs for connection errors
kubectl logs <pod-name> -n sptc-application | grep -i mysql
```

### Redis Connection Issues

```bash
# Verify Redis connection string is set
kubectl exec -it <pod-name> -n sptc-application -- env | grep REDIS

# Check application logs for Redis errors
kubectl logs <pod-name> -n sptc-application | grep -i redis
```

### Ingress Not Accessible

```bash
# Check ingress status
kubectl describe ingress sptc-application-ingress -n sptc-application

# Check Application Gateway health
az network application-gateway show \
  --resource-group sptc-rg \
  --name sptc-appgw \
  --query "operationalState"
```

### Image Pull Errors

```bash
# Check if ACR is attached to AKS
az aks check-acr \
  --resource-group sptc-rg \
  --name sptc-aks \
  --acr <your-acr-name>

# Re-attach ACR if needed
az aks update \
  --resource-group sptc-rg \
  --name sptc-aks \
  --attach-acr <your-acr-name>
```

---

## Security Considerations

1. **Non-root container**: The application runs as UID 1000 (non-root) for security.
2. **Secrets management**: Use Azure Key Vault with the Secrets Store CSI Driver — never hard-code credentials.
3. **Network policies**: Consider adding Kubernetes NetworkPolicies to restrict pod-to-pod communication.
4. **Image scanning**: Enable Azure Defender for Containers to scan ACR images for vulnerabilities.
5. **HTTPS**: Configure TLS termination at the Application Gateway level using Azure-managed certificates.
6. **RBAC**: Use Azure AD Workload Identity for pod-level Azure resource access.
7. **Resource limits**: CPU and memory limits are set to prevent resource exhaustion attacks.

---

## Technology-Specific Notes

### .NET 8.0 ASP.NET Core on AKS

- **Startup time**: .NET 8 applications typically start in 2-5 seconds. The readiness probe `initialDelaySeconds: 5` accounts for this.
- **GC configuration**: For containerized .NET apps, the runtime automatically detects container memory limits and configures the GC accordingly.
- **Thread pool**: .NET's thread pool adapts to container CPU limits automatically.
- **Graceful shutdown**: ASP.NET Core handles `SIGTERM` gracefully, allowing in-flight requests to complete.

### Redis Distributed State (cz-dotnet-0023)

The application uses Azure Cache for Redis for distributed session state (`IS_ADMIN`, `USER`). This replaces the original static fields that caused inconsistency across horizontally scaled pods.

**Redis connection string format:**
```
<hostname>.redis.cache.windows.net:6380,password=<access-key>,ssl=True,abortConnect=False
```

### MySQL Database (dtb_sptc)

The application connects to MySQL using `MySql.Data` 8.1.0. Ensure the MySQL server:
- Allows connections from the AKS node pool subnet
- Has the `dtb_sptc` database created (use `dtb_sptc.sql` for schema initialization)
- Has the configured user with appropriate permissions

### Health Check Endpoint

The `/health` endpoint is registered in `Program.cs` via `app.MapHealthChecks("/health")` and returns:
```json
{"status":"Healthy"}
```

This endpoint is used by both Kubernetes liveness and readiness probes.

### Explicit Base Image Note

The runtime stage uses `mcr.microsoft.com/dotnet/framework/aspnet:4.8` as specified by the `EXPLICIT_BASE_IMAGE` parameter. The build stage uses `mcr.microsoft.com/dotnet/sdk:8.0` to compile the .NET 8 application. The published output is copied into the explicit runtime image.
