// Program.cs — ASP.NET Core entry point
// Migrated from .NET Framework 4.8 WPF/WinExe (Windows-only) to ASP.NET Core net8.0
// as required by rule cz-dotnet-1023 (Web Forms Requiring Windows Containers).
//
// Secrets and connection strings MUST be supplied via environment variables or
// the Azure Key Vault CSI Driver + Workload Identity — do NOT hard-code credentials.

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// ── Health check registration (containerization / Linux AKS requirement) ──────
// Exposes GET /health → 200 OK {"status":"Healthy"} for liveness/readiness probes.
builder.Services.AddHealthChecks();

// ── Application services ──────────────────────────────────────────────────────
builder.Services.AddControllers();

var app = builder.Build();

// ── Middleware pipeline ───────────────────────────────────────────────────────
app.UseRouting();

// Health check endpoint — used by Kubernetes liveness / readiness probes
app.MapHealthChecks("/health");

app.MapControllers();

app.Run();
