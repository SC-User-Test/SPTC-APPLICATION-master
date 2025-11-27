# SPTC Application - Deployment Guide

## Table of Contents
1. [Overview](#overview)
2. [Prerequisites](#prerequisites)
3. [Architecture Notes](#architecture-notes)
4. [Local Development](#local-development)
5. [Building Docker Images](#building-docker-images)
6. [AWS EKS Deployment](#aws-eks-deployment)
7. [Configuration Management](#configuration-management)
8. [Accessing the Application](#accessing-the-application)
9. [Troubleshooting](#troubleshooting)
10. [Security Considerations](#security-considerations)
11. [Monitoring and Maintenance](#monitoring-and-maintenance)

---

## Overview

**SPTC Application** is a .NET Framework 4.8 WPF desktop application designed for managing SPTC operations with MySQL database backend. This deployment guide covers containerization and deployment to AWS EKS.

### Technology Stack
- **Framework**: .NET Framework 4.8
- **UI**: WPF (Windows Presentation Foundation)
- **Database**: MySQL 8.1.0
- **Container Platform**: Windows Containers
- **Orchestration**: Kubernetes (AWS EKS)
- **Dependencies**: AForge (video processing), Newtonsoft.Json, MySql.Data

### Important Architecture Notes

**⚠️ CRITICAL**: This is a WPF desktop application, which is **NOT** a typical cloud-native architecture. WPF applications:
- Require Windows containers (cannot run on Linux)
- Need interactive GUI sessions (not headless)
- Are designed for single-user desktop environments
- Cannot be horizontally scaled like web services
- Require RDP access for user interaction

**Recommended Modernization**: For production cloud deployment, consider:
1. Converting the UI to a web application (Blazor, ASP.NET MVC, or React/Angular)
2. Separating business logic into REST APIs (ASP.NET Web API)
3. Using modern .NET (6/7/8) instead of .NET Framework
4. Implementing proper cloud-native patterns

---

## Prerequisites

### Development Machine Requirements

#### Windows Development
- Windows 10/11 Pro or Enterprise (for Windows containers)
- Docker Desktop for Windows (version 4.0+)
  - Windows container mode enabled
- Visual Studio 2019/2022 (optional, for local development)
- .NET Framework 4.8 SDK

#### Build and Deployment Tools
- **Docker**: 20.10+ with Windows container support
- **AWS CLI**: 2.x
  ```powershell
  # Install AWS CLI
  msiexec.exe /i https://awscli.amazonaws.com/AWSCLIV2.msi
  
  # Configure AWS credentials
  aws configure
  ```
- **kubectl**: 1.27+
  ```powershell
  # Install kubectl
  curl -LO https://dl.k8s.io/release/v1.27.0/bin/windows/amd64/kubectl.exe
  ```
- **eksctl** (optional but recommended)
  ```powershell
  # Install eksctl
  chocolatey install -y eksctl
  ```

### AWS Requirements

#### EKS Cluster with Windows Node Support
- EKS cluster version 1.27+
- **Windows node pools** (required for .NET Framework)
- VPC with proper networking configuration
- IAM roles and policies configured

#### Required AWS Permissions
- ECR: Push/pull images
- EKS: Cluster access and node management
- EC2: For Windows node pools
- VPC: Network configuration
- IAM: Role management

#### Setting Up Windows Nodes in EKS

```bash
# Create Windows node group
eksctl create nodegroup \
  --cluster sptc-cluster \
  --name windows-ng \
  --node-type t3.xlarge \
  --nodes 2 \
  --nodes-min 1 \
  --nodes-max 3 \
  --node-ami-family WindowsServer2022CoreContainer \
  --region us-east-1
```

**Important Windows Node Requirements**:
- Minimum instance type: t3.xlarge (Windows containers require more resources)
- Windows Server 2022 Core Container AMI
- Proper IAM roles for Windows nodes
- VPC CNI plugin configured for Windows

---

## Architecture Notes

### Container Architecture

```
┌─────────────────────────────────────────────────────────┐
│                    AWS EKS Cluster                      │
│                                                         │
│  ┌─────────────────────────────────────────────────┐  │
│  │          Windows Node Pool                      │  │
│  │                                                  │  │
│  │  ┌──────────────────────────────────────────┐  │  │
│  │  │  SPTC Application Pod                    │  │  │
│  │  │                                          │  │  │
│  │  │  ┌────────────────────────────────────┐ │  │  │
│  │  │  │  Windows Container                 │ │  │  │
│  │  │  │  .NET Framework 4.8 Runtime        │ │  │  │
│  │  │  │  SPTC APPLICATION.exe              │ │  │  │
│  │  │  │  RDP Server (Port 3389)            │ │  │  │
│  │  │  └────────────────────────────────────┘ │  │  │
│  │  └──────────────────────────────────────────┘  │  │
│  │                                                  │  │
│  └─────────────────────────────────────────────────┘  │
│                                                         │
│  ┌─────────────────────────────────────────────────┐  │
│  │  LoadBalancer Service (RDP - Port 3389)         │  │
│  └─────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────┘
                          │
                          ▼
              ┌─────────────────────────┐
              │  External MySQL         │
              │  Database Server        │
              └─────────────────────────┘
```

### Deployment Model

- **Replicas**: 1 (WPF apps cannot be load-balanced)
- **Access Method**: RDP via LoadBalancer
- **State**: Persistent volumes for application data
- **Database**: External MySQL (not containerized)

---

## Local Development

### Running Without Docker

1. **Open in Visual Studio**:
   ```powershell
   cd /path/to/STCCompTest
   start "SPTC APPLICATION.sln"
   ```

2. **Configure Database** (App.config):
   ```xml
   <SPTC_APPLICATION.Properties.Settings>
     <setting name="Host" serializeAs="String">
       <value>localhost</value>
     </setting>
     <setting name="Port" serializeAs="String">
       <value>3306</value>
     </setting>
     <setting name="Database" serializeAs="String">
       <value>dtb_sptc</value>
     </setting>
     <setting name="Username" serializeAs="String">
       <value>root</value>
     </setting>
     <setting name="Password" serializeAs="String">
       <value>your_password</value>
     </setting>
   </SPTC_APPLICATION.Properties.Settings>
   ```

3. **Build and Run**:
   - Press F5 in Visual Studio
   - Or build via command line:
   ```powershell
   msbuild "SPTC APPLICATION.csproj" /p:Configuration=Release
   ```

### Running with Docker Compose

**Note**: Docker Desktop must be in Windows container mode.

1. **Switch to Windows Containers**:
   - Right-click Docker Desktop system tray icon
   - Select "Switch to Windows containers..."

2. **Configure environment variables** (create `.env` file):
   ```env
   DB_HOST=your-mysql-host
   DB_PORT=3306
   DB_NAME=dtb_sptc
   DB_USER=root
   DB_PASSWORD=your_password
   ```

3. **Build and run**:
   ```powershell
   docker-compose build
   docker-compose up -d
   ```

4. **Access the container**:
   ```powershell
   # Connect via RDP
   mstsc /v:localhost:3389
   ```

---

## Building Docker Images

### Using Build Script (Recommended)

#### Windows:
```powershell
cd /path/to/STCCompTest
.\scripts\build-push.bat
```

#### Linux/Mac (with Docker Desktop and Windows containers):
```bash
cd /path/to/STCCompTest
./scripts/build-push.sh
```

The script will prompt for:
1. Registry type (AWS ECR or Docker Hub)
2. Registry credentials and details
3. Image tag

### Manual Build

#### Build locally:
```powershell
# Ensure Windows container mode
docker build -t sptc-application:latest -f Dockerfile .
```

#### Tag and push to ECR:
```powershell
# Authenticate
aws ecr get-login-password --region us-east-1 | docker login --username AWS --password-stdin 123456789012.dkr.ecr.us-east-1.amazonaws.com

# Tag
docker tag sptc-application:latest 123456789012.dkr.ecr.us-east-1.amazonaws.com/sptc-application:v1.0.0

# Push
docker push 123456789012.dkr.ecr.us-east-1.amazonaws.com/sptc-application:v1.0.0
```

---

## AWS EKS Deployment

### Step 1: Prepare EKS Cluster

1. **Create EKS cluster** (if not exists):
   ```bash
   eksctl create cluster \
     --name sptc-cluster \
     --region us-east-1 \
     --version 1.27 \
     --nodegroup-name linux-nodes \
     --node-type t3.medium \
     --nodes 2
   ```

2. **Add Windows node group**:
   ```bash
   eksctl create nodegroup \
     --cluster sptc-cluster \
     --name windows-nodes \
     --node-type t3.xlarge \
     --nodes 2 \
     --node-ami-family WindowsServer2022CoreContainer
   ```

3. **Verify Windows nodes**:
   ```bash
   kubectl get nodes -o wide
   # Should show Windows nodes with OS: Windows Server 2022
   ```

### Step 2: Deploy Application

#### Using Deployment Script (Recommended):

```powershell
# Windows
.\scripts\deploy-image.bat
```

```bash
# Linux/Mac
./scripts/deploy-image.sh
```

The script will prompt for:
- AWS region and EKS cluster name
- Docker image URI
- Database configuration (host, port, credentials)

#### Manual Deployment:

1. **Update manifests** with your values:
   ```bash
   # Edit kubernetes/deployment.yaml
   # Replace {{IMAGE_URI}}, {{DB_HOST}}, etc.
   ```

2. **Apply manifests**:
   ```bash
   kubectl apply -f kubernetes/namespace.yaml
   kubectl apply -f kubernetes/deployment.yaml
   kubectl apply -f kubernetes/service.yaml
   ```

3. **Monitor deployment**:
   ```bash
   kubectl rollout status deployment/sptc-application -n sptc-application
   kubectl get pods -n sptc-application -w
   ```

### Step 3: Verify Deployment

```bash
# Check all resources
kubectl get all -n sptc-application

# Check pod logs
kubectl logs -n sptc-application -l app=sptc-application

# Get LoadBalancer external IP
kubectl get svc sptc-application-service -n sptc-application
```

---

## Configuration Management

### Environment Variables

Set via Kubernetes deployment:
```yaml
env:
  - name: DB_HOST
    value: "mysql.example.com"
  - name: DB_PORT
    value: "3306"
  - name: DB_NAME
    value: "dtb_sptc"
  - name: DB_USER
    value: "app_user"
  - name: DB_PASSWORD
    valueFrom:
      secretKeyRef:
        name: sptc-application-secrets
        key: db-password
```

### Secrets Management

Create Kubernetes secret:
```bash
kubectl create secret generic sptc-application-secrets \
  --from-literal=db-password='your_secure_password' \
  -n sptc-application
```

### ConfigMaps

For application configuration:
```bash
kubectl create configmap sptc-application-config \
  --from-file=app.config=./config/App.config \
  -n sptc-application
```

---

## Accessing the Application

### Via LoadBalancer (External Access)

1. **Get LoadBalancer external IP**:
   ```bash
   kubectl get svc sptc-application-service -n sptc-application
   ```
   Output:
   ```
   NAME                        TYPE           EXTERNAL-IP                                                              
   sptc-application-service    LoadBalancer   a1234567890abcdef.us-east-1.elb.amazonaws.com
   ```

2. **Wait for DNS propagation** (2-5 minutes)

3. **Connect via RDP**:
   - **Windows**: `mstsc /v:a1234567890abcdef.us-east-1.elb.amazonaws.com:3389`
   - **Mac**: Use Microsoft Remote Desktop app
   - **Linux**: Use Remmina or rdesktop

4. **Launch application**: `C:\app\SPTC APPLICATION.exe`

### Via Port Forwarding (Development)

```bash
# Forward local port 3389 to pod
kubectl port-forward -n sptc-application deployment/sptc-application 3389:3389

# Connect via RDP
mstsc /v:localhost:3389
```

---

## Troubleshooting

### Pod Failures

#### Check pod status:
```bash
kubectl get pods -n sptc-application
kubectl describe pod <pod-name> -n sptc-application
```

#### Common issues:

1. **ImagePullBackOff**:
   - Verify ECR authentication
   - Check image URI is correct
   - Ensure IAM permissions for ECR

2. **CrashLoopBackOff**:
   - Check application logs:
     ```bash
     kubectl logs <pod-name> -n sptc-application
     ```
   - Verify database connectivity
   - Check environment variables

3. **Pending (Node Selection)**:
   - Ensure Windows nodes are available:
     ```bash
     kubectl get nodes -l kubernetes.io/os=windows
     ```
   - Check node selectors in deployment

### Database Connection Issues

```bash
# Test database connectivity from pod
kubectl exec -it <pod-name> -n sptc-application -- powershell

# Inside pod:
Test-NetConnection -ComputerName $env:DB_HOST -Port $env:DB_PORT
```

### Windows Container Issues

1. **Verify Windows container mode**:
   ```powershell
   docker info | Select-String "OSType"
   # Should show: OSType: windows
   ```

2. **Check Windows node configuration**:
   ```bash
   kubectl get nodes -o jsonpath='{.items[*].status.nodeInfo.osImage}'
   ```

3. **Windows pod scheduling**:
   - Pods may take longer to start (5-10 minutes first time)
   - Windows images are large (several GB)
   - Check node disk space

### LoadBalancer Issues

```bash
# Check service events
kubectl describe svc sptc-application-service -n sptc-application

# Verify AWS Load Balancer Controller
kubectl get pods -n kube-system -l app.kubernetes.io/name=aws-load-balancer-controller

# Check security groups
aws ec2 describe-security-groups --filters "Name=tag:kubernetes.io/cluster/sptc-cluster,Values=owned"
```

---

## Security Considerations

### Network Security

1. **Restrict RDP access** via security groups:
   ```bash
   # Allow only specific IP ranges
   aws ec2 authorize-security-group-ingress \
     --group-id sg-xxxxx \
     --protocol tcp \
     --port 3389 \
     --cidr 10.0.0.0/8
   ```

2. **Use VPN or bastion host** for production access

3. **Enable VPC Flow Logs** for network monitoring

### Container Security

1. **Run as non-administrator** (configured in Dockerfile)
2. **Use read-only root filesystem** where possible
3. **Scan images for vulnerabilities**:
   ```bash
   aws ecr start-image-scan --repository-name sptc-application --image-id imageTag=latest
   ```

### Database Security

1. **Use SSL/TLS** for MySQL connections
2. **Store credentials in AWS Secrets Manager**:
   ```bash
   aws secretsmanager create-secret \
     --name sptc/db/password \
     --secret-string '{"password":"secure_password"}'
   ```
3. **Use IAM database authentication** if possible
4. **Regular password rotation**

### Best Practices

- ✅ Use least-privilege IAM roles
- ✅ Enable pod security policies
- ✅ Implement network policies
- ✅ Regular security updates for base images
- ✅ Audit logging enabled
- ✅ Multi-factor authentication for RDP access

---

## Monitoring and Maintenance

### Logging

#### View application logs:
```bash
# Real-time logs
kubectl logs -f -n sptc-application -l app=sptc-application

# Last 100 lines
kubectl logs --tail=100 -n sptc-application -l app=sptc-application
```

#### CloudWatch Integration:
```bash
# Install Fluent Bit for Windows
kubectl apply -f https://raw.githubusercontent.com/aws-samples/amazon-cloudwatch-container-insights/latest/k8s-deployment-manifest-templates/deployment-mode/daemonset/container-insights-monitoring/fluent-bit/fluent-bit-windows.yaml
```

### Monitoring Metrics

1. **Install metrics server**:
   ```bash
   kubectl apply -f https://github.com/kubernetes-sigs/metrics-server/releases/latest/download/components.yaml
   ```

2. **View resource usage**:
   ```bash
   kubectl top nodes
   kubectl top pods -n sptc-application
   ```

3. **CloudWatch Container Insights**:
   ```bash
   aws cloudwatch get-metric-statistics \
     --namespace ContainerInsights \
     --metric-name pod_cpu_utilization \
     --dimensions Name=PodName,Value=sptc-application-xxxxx \
     --statistics Average \
     --start-time 2024-01-01T00:00:00Z \
     --end-time 2024-01-02T00:00:00Z \
     --period 3600
   ```

### Maintenance Tasks

#### Update application:
```bash
# Build new image with new tag
./scripts/build-push.sh

# Update deployment
kubectl set image deployment/sptc-application \
  sptc-application=<new-image-uri> \
  -n sptc-application

# Monitor rollout
kubectl rollout status deployment/sptc-application -n sptc-application
```

#### Rollback deployment:
```bash
# View rollout history
kubectl rollout history deployment/sptc-application -n sptc-application

# Rollback to previous version
kubectl rollout undo deployment/sptc-application -n sptc-application

# Rollback to specific revision
kubectl rollout undo deployment/sptc-application --to-revision=2 -n sptc-application
```

#### Backup and restore:
```bash
# Backup deployment configuration
kubectl get deployment sptc-application -n sptc-application -o yaml > backup-deployment.yaml

# Backup PVC data (use snapshot)
kubectl get pvc -n sptc-application
aws ec2 create-snapshot --volume-id vol-xxxxx
```

---

## Additional Resources

- [AWS EKS Documentation](https://docs.aws.amazon.com/eks/)
- [Windows Containers on EKS](https://docs.aws.amazon.com/eks/latest/userguide/windows-support.html)
- [.NET Framework Container Guide](https://docs.microsoft.com/en-us/dotnet/framework/docker/)
- [Kubernetes Documentation](https://kubernetes.io/docs/)

---

## Support and Contact

For issues or questions:
- Create an issue in the project repository
- Contact the development team
- Review AWS support resources

---

**Last Updated**: 2025-11-27  
**Version**: 1.0.0