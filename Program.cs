// ============================================================
// Program.cs – ASP.NET Core entry point
// ============================================================
// Replaces the WPF App.xaml / App.xaml.cs startup that required
// a Windows desktop subsystem.  This headless web host runs on
// Linux AKS nodes and exposes:
//   GET /health  – liveness / readiness probe (HealthCheckService)
//   GET /api/... – migrated Razor Pages / API controllers
//
// Secrets are injected via the Azure Key Vault CSI Driver and
// Workload Identity – no hardcoded credentials in source code.
// ============================================================

using SPTC_APPLICATION;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// ------------------------------------------------------------------
// Health checks (satisfies container liveness / readiness probes)
// ------------------------------------------------------------------
builder.Services.AddHealthChecks();

// ------------------------------------------------------------------
// MVC / Razor Pages (migrated from WPF XAML views)
// ------------------------------------------------------------------
builder.Services.AddRazorPages();
builder.Services.AddControllers();

// ------------------------------------------------------------------
// Application services
// Register the custom HTTP health-check listener using its fully
// qualified name to avoid ambiguity with the ASP.NET Core
// Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckService.
// ------------------------------------------------------------------
builder.Services.AddSingleton<SPTC_APPLICATION.HealthCheckService>();

// ------------------------------------------------------------------
// Kestrel – listen on the port supplied by the container platform
// ------------------------------------------------------------------
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

var app = builder.Build();

// ------------------------------------------------------------------
// Middleware pipeline
// ------------------------------------------------------------------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

// Map the /health endpoint for Kubernetes liveness / readiness probes
app.MapHealthChecks("/health");

app.MapRazorPages();
app.MapControllers();

// ------------------------------------------------------------------
// Start the background health-check HTTP listener (legacy support)
// ------------------------------------------------------------------
var healthCheckService = app.Services.GetRequiredService<SPTC_APPLICATION.HealthCheckService>();
healthCheckService.Start();

app.Lifetime.ApplicationStopping.Register(() => healthCheckService.Stop());

// ------------------------------------------------------------------
// Initialise application state (previously done in App.OnStartup)
// ------------------------------------------------------------------
AppState.LoadFromJson();

app.Run();
