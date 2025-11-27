# escape=`

# Build stage
FROM mcr.microsoft.com/dotnet/framework/sdk:4.8-windowsservercore-ltsc2022 AS builder

# Set working directory
WORKDIR /src

# Install NuGet CLI
RUN powershell -Command `
    Invoke-WebRequest -Uri https://dist.nuget.org/win-x86-commandline/latest/nuget.exe -OutFile C:\nuget.exe

# Copy project files for dependency caching
COPY ["SPTC APPLICATION.csproj", "./"]
COPY ["packages.config", "./"]

# Restore NuGet packages
RUN C:\nuget.exe restore "SPTC APPLICATION.csproj" -PackagesDirectory packages

# Copy all source files
COPY . .

# Build the application
RUN msbuild "SPTC APPLICATION.csproj" /p:Configuration=Release /p:Platform=AnyCPU /p:OutputPath=C:\app\publish /p:DeployOnBuild=true /p:PublishProfile=Release /v:m

# Runtime stage
FROM mcr.microsoft.com/dotnet/framework/runtime:4.8-windowsservercore-ltsc2022

# Set working directory
WORKDIR /app

# Copy published application from builder
COPY --from=builder /app/publish .

# Copy configuration files
COPY ["App.config", "./SPTC APPLICATION.exe.config"]

# Environment variables for database configuration
ENV DB_HOST=localhost `
    DB_PORT=3306 `
    DB_NAME=dtb_sptc `
    DB_USER=root `
    DB_PASSWORD=""

# Note: WPF applications require interactive session
# This application cannot run headless without modification
# Consider converting to Windows Service or Web API for container deployment

# For Windows containers with RDP access, expose RDP port
EXPOSE 3389

# Default command (requires modification for non-interactive execution)
CMD ["SPTC APPLICATION.exe"]