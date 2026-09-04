# =============================================================================
# Dockerfile – SPTC APPLICATION (ASP.NET Core / net8.0)
# Target: Azure AKS (Linux node pool)
# =============================================================================
# Multi-stage build:
#   Stage 1 (build)   – mcr.microsoft.com/dotnet/sdk:8.0
#                        Restores NuGet packages, compiles and publishes.
#   Stage 2 (runtime) – mcr.microsoft.com/dotnet/framework/sdk:4.8
#                        Contains only the published output; no SDK, no source.
# =============================================================================

# ---------------------------------------------------------------------------
# Stage 1: Build / Publish
# ---------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

# Copy project file first to leverage Docker layer caching for NuGet restore
COPY ["SPTC APPLICATION.csproj", "SPTC APPLICATION/"]

# Restore NuGet packages (cached layer – only invalidated when .csproj changes)
RUN dotnet restore "SPTC APPLICATION/SPTC APPLICATION.csproj"

# Copy the rest of the source code
COPY . "SPTC APPLICATION/"

WORKDIR "/src/SPTC APPLICATION"

# Publish in Release configuration
RUN dotnet publish "SPTC APPLICATION.csproj" \
    --no-restore \
    -c Release \
    -r linux-x64 \
    --self-contained false \
    -o /app/publish

# ---------------------------------------------------------------------------
# Stage 2: Runtime image
# ---------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/framework/sdk:4.8 AS runtime

# Run as non-root user for AKS security best practices
RUN addgroup --system --gid 1001 appgroup \
 && adduser  --system --uid 1001 --ingroup appgroup --no-create-home appuser

WORKDIR /app

# Copy ONLY the published output from the build stage
COPY --from=build /app/publish .

# Change ownership to non-root user
RUN chown -R appuser:appgroup /app

USER appuser

# Expose the application port
EXPOSE 8080

# Environment defaults (overridden by AKS ConfigMap / Secret / Key Vault CSI)
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    HEALTH_CHECK_PORT=8080 \
    PORT=8080

ENTRYPOINT ["dotnet", "SPTC APPLICATION.dll"]
