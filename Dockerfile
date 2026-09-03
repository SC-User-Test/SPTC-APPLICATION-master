# =============================================================================
# Multi-Stage Dockerfile for SPTC APPLICATION
# Target Platform: Azure AKS (Linux node pool)
# Build Stage  : mcr.microsoft.com/dotnet/sdk:8.0
# Runtime Stage: mcr.microsoft.com/dotnet/framework/aspnet:4.8 (explicit)
#
# NOTE: The explicit runtime base image mcr.microsoft.com/dotnet/framework/aspnet:4.8
# is a Windows container image. For Linux AKS node pools, ensure your cluster
# has Windows node pools configured, or switch to mcr.microsoft.com/dotnet/runtime:8.0
# for Linux-compatible deployments.
# =============================================================================

# ── Stage 1: Build ────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

# Copy project file first to leverage Docker layer caching for NuGet restore
COPY ["SPTC APPLICATION.csproj", "./"]
RUN dotnet restore "SPTC APPLICATION.csproj" --runtime linux-x64

# Copy remaining source files (respects .dockerignore exclusions)
COPY . .

# Publish in Release mode – no PDB files, no source, trimmed output only
RUN dotnet publish "SPTC APPLICATION.csproj" \
    --configuration Release \
    --runtime linux-x64 \
    --self-contained false \
    --output /app/publish \
    /p:DebugType=None \
    /p:DebugSymbols=false

# ── Stage 2: Runtime ──────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/framework/aspnet:4.8 AS runtime

WORKDIR /app

# Copy only the published output – no source code, no PDB, no obj/bin folders
COPY --from=build /app/publish .

# ── Security: create and use a non-root user ──────────────────────────────────
# Note: Windows containers use different user management; adjust if targeting Linux
# RUN addgroup --system sptcgroup && adduser --system --ingroup sptcgroup sptcuser
# USER sptcuser

# ── Environment Variables ─────────────────────────────────────────────────────
# Health check port (used by HealthCheckService.cs)
ENV HEALTH_CHECK_PORT=8080

# Application state base path
ENV APPSTATE_BASE_PATH=/app/data

# MySQL connection settings (override at runtime via Kubernetes secrets/configmaps)
ENV MYSQL_HOST=localhost
ENV MYSQL_PORT=3306
ENV MYSQL_DATABASE=dtb_sptc
ENV MYSQL_USERNAME=root
ENV MYSQL_PASSWORD=""

# Redis connection string (injected via AKS Secrets Store CSI Driver / Azure Key Vault)
ENV REDIS_CONNECTION_STRING=""

# .NET runtime settings
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# ── Expose health check port ──────────────────────────────────────────────────
EXPOSE 8080

# ── Entry point ───────────────────────────────────────────────────────────────
ENTRYPOINT ["dotnet", "SPTC APPLICATION.dll"]
