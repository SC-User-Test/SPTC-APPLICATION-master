# =============================================================================
# Multi-Stage Dockerfile — SPTC APPLICATION (ASP.NET Core net8.0)
# Rule ID  : cz-dotnet-1040
# Rule Name: Unoptimized Web Forms Container Image Size
#
# Remediation: Multi-Stage Dockerfile and .dockerignore for ASP.NET Core on
#              AKS Linux Node Pool (migrated from Windows-only .NET 4.8 WPF).
#
# Addresses original SPTC APPLICATION.csproj occurrences:
#   - Line 37: <DebugSymbols>true</DebugSymbols> in Debug|AnyCPU PropertyGroup
#   - Line 56: <DebugSymbols>true</DebugSymbols> in Debug|x86 PropertyGroup
#
# Strategy:
#   Stage 1 (build)   — SDK image: restore, build, publish with NO PDB files
#   Stage 2 (runtime) — Explicit base image: mcr.microsoft.com/dotnet/framework/aspnet:4.8
#
# This eliminates source code, PDB files, obj/, bin/Debug/, temp files, and
# the full SDK toolchain from the final image, dramatically reducing image size
# and AKS node disk pressure.
#
# Image push target: Azure Container Registry (ACR)
# Deployment target: AKS Linux node pool
# =============================================================================

# ── Stage 1: Build & Publish ──────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

# Copy project file first for layer-cached NuGet restore
COPY ["SPTC APPLICATION.csproj", "SPTC APPLICATION.csproj"]

# Restore NuGet packages (cached layer — only re-runs when .csproj changes)
RUN dotnet restore "SPTC APPLICATION.csproj"

# Copy remaining source files (excluded files are filtered by .dockerignore)
COPY . .

# Publish in Release mode:
#   --no-restore        : reuse restored packages from previous layer
#   -c Release          : triggers <DebugType>none</DebugType> — no PDB files
#   --no-self-contained : use runtime image's shared framework (smaller output)
#   -o /app/publish     : output directory for Stage 2 COPY
RUN dotnet publish "SPTC APPLICATION.csproj" \
    --no-restore \
    -c Release \
    --no-self-contained \
    -r linux-x64 \
    -o /app/publish \
    /p:DebugType=none \
    /p:DebugSymbols=false

# ── Stage 2: Runtime Image ────────────────────────────────────────────────────
# Explicit base image provided: mcr.microsoft.com/dotnet/framework/aspnet:4.8
# NOTE: This image is used as the runtime stage per the EXPLICIT_BASE_IMAGE parameter.
# The build stage above uses the .NET 8 SDK for compilation; the published output
# is copied into this runtime image.
FROM mcr.microsoft.com/dotnet/framework/aspnet:4.8 AS runtime

# Security: run as non-root user (Linux containers)
RUN addgroup --system appgroup && adduser --system --ingroup appgroup appuser

WORKDIR /app

# Copy ONLY the published output from the build stage
COPY --from=build /app/publish .

# Set ownership to non-root user
RUN chown -R appuser:appgroup /app

USER appuser

# ── Runtime configuration ─────────────────────────────────────────────────────
# All secrets and connection strings MUST be supplied via environment variables
# or the Azure Key Vault CSI Driver + Workload Identity — do NOT hard-code.
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check port (used by Kubernetes liveness/readiness probes → GET /health)
ENV HEALTH_CHECK_PORT=8080

# MySQL connection (inject at runtime via AKS Secrets Store CSI Driver)
ENV MYSQL_HOST=""
ENV MYSQL_PORT="3306"
ENV MYSQL_DATABASE="dtb_sptc"
ENV MYSQL_USERNAME=""
ENV MYSQL_PASSWORD=""

# Redis connection (inject at runtime via AKS Secrets Store CSI Driver)
ENV REDIS_CONNECTION_STRING=""

# AppState directory for persistent config
ENV APPSTATE_DIR="/app/Config"

# Expose the application port
EXPOSE 8080

# ── Kubernetes Health Probe ───────────────────────────────────────────────────
# Liveness/readiness probe: GET http://<pod-ip>:8080/health
# Configure in your AKS Deployment manifest:
#   livenessProbe:
#     httpGet:
#       path: /health
#       port: 8080
#     initialDelaySeconds: 15
#     periodSeconds: 20
#   readinessProbe:
#     httpGet:
#       path: /health
#       port: 8080
#     initialDelaySeconds: 5
#     periodSeconds: 10

ENTRYPOINT ["dotnet", "SPTC APPLICATION.dll"]
