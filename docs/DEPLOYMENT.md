# SPTC APPLICATION – Deployment Guide

## Overview

This guide covers the complete deployment process for **SPTC APPLICATION** on **Azure Kubernetes Service (AKS)**.

- **Application**: SPTC APPLICATION (San Pablo Tricycle Cooperative)
- **Technology**: .NET 8.0 (migrated from .NET Framework 4.8)
- **Target Platform**: Azure AKS (Linux node pool, `linux-x64`)
- **Runtime Base Image**: `mcr.microsoft.com/dotnet/framework/aspnet:4.8` (explicit)
- **Health Endpoint**: `GET /health` on port `8080`
- **Key Dependencies**: MySQL, Azure Cache for Redis, Newtonsoft.Json, Google.Protobuf

---

## Table of Contents

1. [Prerequisites](#prerequisites)
2. [Project Structure](#project-structure)
3. [Local Development with Docker Compose](#local-development-with-docker-compose)
4. [Build and Push Docker Image](#build-and-push-docker-image)
5. [Azure AKS Prerequisites](#azure-aks-prerequisites)
6. [AKS Cluster Setup](#aks-cluster-setup)
7. [Kubernetes Secrets Configuration](#kubernetes-secrets-configuration)
8. [Kubernetes Deployment](#kubernetes-deployment)
9. [Verify Deployment](#verify-deployment)
10. [Scaling and Management](#scaling-and-management)
11. [Troubleshooting](#troubleshooting)
12. [Security Considerations](#security-considerations)
13. [Configuration Reference](#configuration-reference)

---

## Prerequisites

### Local Development Tools
- **Docker Desktop** 24.x or later
- **Docker Compose** v2.x or later
- **.NET SDK 8.0** (for local builds)
- **Azure CLI** 2.50+ (`az --version`)
- **kubectl** 1.27+ (`kubectl version --client`)

### Azure Resources Required
- Azure Subscription with Contributor access
- Azure Container Registry (ACR)
- Azure Kubernetes Service (AKS) cluster
- Azure Database for MySQL (Flexible Server) — external service
- Azure Cache for Redis — external service
- Azure Key Vault (for secrets management)

---

## Project Structure

```
Comp Sapp checking/
├── Dockerfile                    # Multi-stage build (SDK 8.0 → explicit runtime)
├── .dockerignore                 # Excludes bin/, obj/, .vs/, .vscode/, etc.
├── docker-compose.yml            # Local development (app only)
├── SPTC APPLICATION.csproj       # .NET 8.0 SDK-style project
├── App.config                    # Legacy configuration (MySQL settings)
├── AppState.cs                   # Application state management
├── HealthCheckService.cs         # HTTP health endpoint on port 8080
├── RedisStateManager.cs          # Redis-backed distributed state
├── Database/                     # MySQL data access layer
├── Objects/                      # Domain model objects
├── kubernetes/
│   ├── namespace.yaml            # Kubernetes namespace
│   ├── deployment.yaml           # AKS deployment (2 replicas)
│   ├── service.yaml              # ClusterIP service
│   └── ingress.yaml              # Azure Application Gateway ingress
├── scripts/
│   ├── build-push.sh             # Linux: build & push to ACR/Docker Hub
│   ├── build-push.bat            # Windows: build & push to ACR/Docker Hub
│   ├── deploy-image.sh           # Linux: deploy to AKS
│   └── deploy-image.bat          # Windows: deploy to AKS
└── docs/
    └── DEPLOYMENT.md             # This file
```

---

## Local Development with Docker Compose

### 1. Configure Environment Variables

Create a `.env` file in the project root:

```env
# MySQL connection (point to your local or remote MySQL instance)
MYSQL_HOST=localhost
MYSQL_PORT=3306
MYSQL_DATABASE=dtb_sptc
MYSQL_USERNAME=root
MYSQL_PASSWORD=your_password_here

# Redis connection string
REDIS_CONNECTION_STRING=localhost:6379

# Application settings
HEALTH_CHECK_PORT=8080
APPSTATE_BASE_PATH=/app/data
```

### 2. Build and Start the Application

```bash
# Build the image
docker compose build

# Start the application
docker compose up -d

# View logs
docker compose logs -f sptc-application

# Stop the application
docker compose down
```

### 3. Verify Health Check

```bash
curl http://localhost:8080/health
# Expected: {"status":"UP","application":"SPTC-APPLICATION","timestamp":"..."}
```

---

## Build and Push Docker Image

### Linux / macOS

```bash
# Make the script executable
chmod +x scripts/build-push.sh

# Run from repository root
./scripts/build-push.sh
```

The script will prompt you to:
1. Enter an image tag (default: `latest`)
2. Select registry type (ACR or Docker Hub)
3. Enter registry credentials

### Windows

```cmd
REM Run from repository root
scripts\build-push.bat
```

### Manual Build (ACR Example)

```bash
# Login to ACR
az acr login --name <your-acr-name>

# Build image
docker build -f Dockerfile -t <your-acr-name>.azurecr.io/sptc-application:latest .

# Push image
docker push <your-acr-name>.azurecr.io/sptc-application:latest
```

---

## Azure AKS Prerequisites

### Install Required Tools

```bash
# Install Azure CLI
curl -sL https://aka.ms/InstallAzureCLIDeb | sudo bash

# Install kubectl
az aks install-cli

# Login to Azure
az login

# Set subscription
az account set --subscription "<your-subscription-id>"
```

---

## AKS Cluster Setup

### 1. Create Resource Group

```bash
az group create \
  --name sptc-rg \
  --location eastus
```

### 2. Create Azure Container Registry

```bash
az acr create \
  --resource-group sptc-rg \
  --name sptcregistry \
  --sku Basic
```

### 3. Create AKS Cluster

```bash
az aks create \
  --resource-group sptc-rg \
  --name sptc-aks \
  --node-count 2 \
  --node-vm-size Standard_DS2_v2 \
  --enable-addons monitoring \
  --attach-acr sptcregistry \
  --generate-ssh-keys
```

### 4. Configure kubectl

```bash
az aks get-credentials \
  --resource-group sptc-rg \
  --name sptc-aks

# Verify connection
kubectl cluster-info
kubectl get nodes
```

---

## Kubernetes Secrets Configuration

SPTC APPLICATION requires the following Kubernetes secrets before deployment:

### Create MySQL Secret

```bash
kubectl create namespace sptc-application

kubectl create secret generic sptc-db-secret \
  --namespace sptc-application \
  --from-literal=username='<mysql-username>' \
  --from-literal=password='<mysql-password>'
```

### Create Redis Secret

```bash
kubectl create secret generic sptc-redis-secret \
  --namespace sptc-application \
  --from-literal=connection-string='<redis-connection-string>'
```

### Using Azure Key Vault (Recommended for Production)

```bash
# Create Key Vault
az keyvault create \
  --name sptc-keyvault \
  --resource-group sptc-rg \
  --location eastus

# Store secrets
az keyvault secret set --vault-name sptc-keyvault --name mysql-password --value '<password>'
az keyvault secret set --vault-name sptc-keyvault --name redis-connection-string --value '<connection-string>'

# Enable AKS Secrets Store CSI Driver
az aks enable-addons \
  --addons azure-keyvault-secrets-provider \
  --name sptc-aks \
  --resource-group sptc-rg
```

---

## Kubernetes Deployment

### Automated Deployment (Recommended)

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
- Full Docker image URI (e.g., `sptcregistry.azurecr.io/sptc-application:latest`)
- MySQL host, port, and database name

### Manual Deployment

```bash
# 1. Apply namespace
kubectl apply -f kubernetes/namespace.yaml

# 2. Update image URI in deployment.yaml
sed -i 's|{{IMAGE_URI}}|sptcregistry.azurecr.io/sptc-application:latest|g' kubernetes/deployment.yaml
sed -i 's|{{MYSQL_HOST}}|your-mysql-host.mysql.database.azure.com|g' kubernetes/deployment.yaml
sed -i 's|{{MYSQL_PORT}}|3306|g' kubernetes/deployment.yaml
sed -i 's|{{MYSQL_DATABASE}}|dtb_sptc|g' kubernetes/deployment.yaml

# 3. Apply manifests
kubectl apply -f kubernetes/deployment.yaml
kubectl apply -f kubernetes/service.yaml
kubectl apply -f kubernetes/ingress.yaml

# 4. Wait for rollout
kubectl rollout status deployment/sptc-application -n sptc-application
```

### Kubernetes Manifest Descriptions

| File | Description |
|------|-------------|
| `namespace.yaml` | Creates `sptc-application` namespace |
| `deployment.yaml` | 2 replicas, health probes on `/health:8080`, resource limits |
| `service.yaml` | ClusterIP service, port 80 → 8080 |
| `ingress.yaml` | Azure Application Gateway ingress, host: `sptc-application.example.com` |

---

## Verify Deployment

```bash
# Check all resources
kubectl get all -n sptc-application

# Check pod status
kubectl get pods -n sptc-application

# Check pod logs
kubectl logs -l app=sptc-application -n sptc-application

# Describe deployment
kubectl describe deployment sptc-application -n sptc-application

# Check ingress
kubectl get ingress -n sptc-application

# Test health endpoint (port-forward for local testing)
kubectl port-forward svc/sptc-application-service 8080:80 -n sptc-application
curl http://localhost:8080/health
```

---

## Scaling and Management

### Manual Scaling

```bash
# Scale to 3 replicas
kubectl scale deployment sptc-application --replicas=3 -n sptc-application
```

### Horizontal Pod Autoscaler (HPA)

```bash
kubectl autoscale deployment sptc-application \
  --cpu-percent=70 \
  --min=2 \
  --max=10 \
  -n sptc-application

kubectl get hpa -n sptc-application
```

### Rolling Update

```bash
# Update image
kubectl set image deployment/sptc-application \
  sptc-application=sptcregistry.azurecr.io/sptc-application:v2.0.0 \
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
# Check pod events
kubectl describe pod <pod-name> -n sptc-application

# Check logs
kubectl logs <pod-name> -n sptc-application --previous
```

**Common causes:**
- Missing Kubernetes secrets (`sptc-db-secret`, `sptc-redis-secret`)
- Invalid image URI or ACR authentication failure
- Insufficient node resources

### Health Check Failing

```bash
# Port-forward and test manually
kubectl port-forward <pod-name> 8080:8080 -n sptc-application
curl -v http://localhost:8080/health
```

**Common causes:**
- `HEALTH_CHECK_PORT` environment variable mismatch
- Application startup taking longer than `initialDelaySeconds` (20s)
- MySQL or Redis connection failure preventing startup

### MySQL Connection Issues

```bash
# Verify secret exists
kubectl get secret sptc-db-secret -n sptc-application

# Check environment variables in pod
kubectl exec -it <pod-name> -n sptc-application -- env | grep MYSQL
```

### Redis Connection Issues

```bash
# Verify Redis secret
kubectl get secret sptc-redis-secret -n sptc-application

# Check Redis connection string format
# Expected: <host>:<port>,password=<password>,ssl=True,abortConnect=False
```

### Ingress Not Accessible

```bash
# Check ingress status
kubectl describe ingress sptc-application-ingress -n sptc-application

# Verify Application Gateway Ingress Controller is installed
kubectl get pods -n kube-system | grep ingress-appgw
```

---

## Security Considerations

1. **Secrets Management**: Never store credentials in YAML files. Use Kubernetes Secrets or Azure Key Vault CSI Driver.
2. **Non-root User**: The Dockerfile is configured for non-root execution where supported.
3. **Image Scanning**: Enable ACR vulnerability scanning: `az acr task create --registry sptcregistry --name scan --cmd "mcr.microsoft.com/acr/tasks:latest" --file /dev/null`
4. **Network Policies**: Restrict pod-to-pod communication using Kubernetes NetworkPolicy.
5. **RBAC**: Use least-privilege service accounts for AKS workloads.
6. **Workload Identity**: Use AKS Workload Identity for Azure Key Vault access instead of connection strings in environment variables.
7. **TLS**: Configure TLS termination at the Application Gateway level for HTTPS.

---

## Configuration Reference

### Environment Variables

| Variable | Default | Description |
|----------|---------|-------------|
| `HEALTH_CHECK_PORT` | `8080` | Port for HTTP health check endpoint |
| `APPSTATE_BASE_PATH` | `/app/data` | Base path for application state files |
| `MYSQL_HOST` | `localhost` | MySQL server hostname |
| `MYSQL_PORT` | `3306` | MySQL server port |
| `MYSQL_DATABASE` | `dtb_sptc` | MySQL database name |
| `MYSQL_USERNAME` | — | MySQL username (from secret) |
| `MYSQL_PASSWORD` | — | MySQL password (from secret) |
| `REDIS_CONNECTION_STRING` | — | Azure Cache for Redis connection string (from secret) |
| `DOTNET_RUNNING_IN_CONTAINER` | `true` | .NET container detection flag |
| `DOTNET_SYSTEM_GLOBALIZATION_INVARIANT` | `false` | Enable globalization support |

### Resource Limits

| Resource | Request | Limit |
|----------|---------|-------|
| CPU | 250m | 500m |
| Memory | 512Mi | 1Gi |

### Health Check Configuration

| Probe | Path | Port | Initial Delay | Period |
|-------|------|------|---------------|--------|
| Liveness | `/health` | 8080 | 20s | 30s |
| Readiness | `/health` | 8080 | 15s | 15s |

---

## .NET-Specific Notes

- **Framework Migration**: This application was migrated from .NET Framework 4.8 (WPF) to .NET 8.0 (library) for Linux container compatibility.
- **WPF Exclusion**: WPF/XAML UI files are excluded from the Linux build. Only backend/data-layer files are compiled.
- **OutputType Library**: The project compiles as a `Library` (no `Main` entry point). The `ENTRYPOINT` in the Dockerfile invokes the DLL via `dotnet`.
- **Redis State**: Admin session state is stored in Azure Cache for Redis via `RedisStateManager` to support horizontal scaling.
- **Health Service**: `HealthCheckService.cs` provides a lightweight HTTP listener on port 8080 responding to `GET /health`.
- **Globalization**: `DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false` is set to support Philippine locale and culture settings.
