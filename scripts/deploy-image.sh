#!/bin/bash
set -e
set -o pipefail

echo "=========================================="
echo "SPTC Application - EKS Deployment"
echo "=========================================="
echo ""

# Deployment configuration
APP_NAME="sptc-application"
NAMESPACE="sptc-application"

echo "Application: $APP_NAME"
echo "Namespace: $NAMESPACE"
echo ""

# AWS Configuration
echo "AWS EKS Configuration"
echo "--------------------"
read -p "Enter AWS Region (e.g., us-east-1): " AWS_REGION
read -p "Enter EKS Cluster Name: " CLUSTER_NAME
echo ""

# Docker Image Configuration
echo "Docker Image Configuration"
echo "-------------------------"
read -p "Enter full Docker image URI (registry/repo:tag): " IMAGE_URI
echo ""

# Database Configuration
echo "Database Configuration"
echo "---------------------"
read -p "Enter MySQL Host (e.g., mysql.example.com): " DB_HOST
read -p "Enter MySQL Port [3306]: " DB_PORT
DB_PORT=${DB_PORT:-3306}
read -p "Enter Database Name [dtb_sptc]: " DB_NAME
DB_NAME=${DB_NAME:-dtb_sptc}
read -p "Enter Database Username [root]: " DB_USER
DB_USER=${DB_USER:-root}
read -sp "Enter Database Password: " DB_PASSWORD
echo ""
echo ""

# Validate inputs
if [ -z "$AWS_REGION" ] || [ -z "$CLUSTER_NAME" ] || [ -z "$IMAGE_URI" ]; then
    echo "ERROR: AWS Region, Cluster Name, and Image URI are required"
    exit 1
fi

echo "Configuring kubectl for EKS..."
aws eks update-kubeconfig --region "$AWS_REGION" --name "$CLUSTER_NAME"

if [ $? -ne 0 ]; then
    echo "ERROR: Failed to configure kubectl"
    exit 1
fi

echo "Verifying cluster connectivity..."
kubectl cluster-info

if [ $? -ne 0 ]; then
    echo "ERROR: Cannot connect to Kubernetes cluster"
    exit 1
fi

echo ""
echo "Updating Kubernetes manifests..."

# Update deployment.yaml with actual values
sed -i "s|{{IMAGE_URI}}|$IMAGE_URI|g" kubernetes/deployment.yaml
sed -i "s|{{DB_HOST}}|$DB_HOST|g" kubernetes/deployment.yaml
sed -i "s|{{DB_PORT}}|$DB_PORT|g" kubernetes/deployment.yaml
sed -i "s|{{DB_NAME}}|$DB_NAME|g" kubernetes/deployment.yaml
sed -i "s|{{DB_USER}}|$DB_USER|g" kubernetes/deployment.yaml
sed -i "s|{{DB_PASSWORD}}|$DB_PASSWORD|g" kubernetes/deployment.yaml

echo "Manifests updated successfully"
echo ""

# Apply Kubernetes manifests
echo "Deploying to EKS..."
echo ""

echo "1. Creating namespace..."
kubectl apply -f kubernetes/namespace.yaml

echo "2. Deploying application..."
kubectl apply -f kubernetes/deployment.yaml

echo "3. Creating service..."
kubectl apply -f kubernetes/service.yaml

echo ""
echo "Waiting for deployment to complete..."
kubectl rollout status deployment/$APP_NAME -n $NAMESPACE --timeout=600s

if [ $? -ne 0 ]; then
    echo "ERROR: Deployment rollout failed"
    echo "Checking pod status..."
    kubectl get pods -n $NAMESPACE
    echo ""
    echo "Checking pod logs..."
    kubectl logs -n $NAMESPACE -l app=$APP_NAME --tail=50
    exit 1
fi

echo ""
echo "=========================================="
echo "Deployment Completed Successfully!"
echo "=========================================="
echo ""

# Show deployment status
echo "Resource Status:"
echo "---------------"
kubectl get all -n $NAMESPACE

echo ""
echo "Service Details:"
echo "---------------"
kubectl get svc $APP_NAME-service -n $NAMESPACE

echo ""
echo "=========================================="
echo "Access Information"
echo "=========================================="
echo ""
echo "This is a WPF Desktop Application running in Windows containers."
echo ""
echo "To access the application:"
echo "1. Get the LoadBalancer external IP:"
echo "   kubectl get svc $APP_NAME-service -n $NAMESPACE"
echo ""
echo "2. Wait for EXTERNAL-IP to be assigned (may take 2-5 minutes)"
echo ""
echo "3. Connect via Remote Desktop Protocol (RDP):"
echo "   - Host: <EXTERNAL-IP>"
echo "   - Port: 3389"
echo "   - Username: ContainerUser"
echo ""
echo "4. Once connected, launch: C:\\app\\SPTC APPLICATION.exe"
echo ""
echo "NOTE: Windows node pools required in EKS cluster"
echo "      for .NET Framework 4.8 applications"
echo ""