// ============================================================
// Settings.cs  (replaces auto-generated Settings.Designer.cs)
// ============================================================
// The original Settings.Designer.cs used System.Configuration.ApplicationSettingsBase
// which is a Windows Forms / WPF pattern not available in ASP.NET Core on net8.0.
//
// Database connection settings are now read from environment variables so that
// they can be injected by the Azure Key Vault CSI Driver / Kubernetes Secrets
// on AKS without hardcoding credentials in source code.
//
// Environment variables (set via Kubernetes ConfigMap / Secret):
//   DB_HOST      – MySQL host (default: localhost)
//   DB_PORT      – MySQL port (default: 3306)
//   DB_DATABASE  – Database name (default: dtb_sptc)
//   DB_USERNAME  – MySQL username (default: root)
//   DB_PASSWORD  – MySQL password (default: empty)
// ============================================================

using System;

namespace SPTC_APPLICATION.Properties
{
    /// <summary>
    /// Provides database connection settings sourced from environment variables.
    /// Replaces the WPF/WinForms ApplicationSettingsBase pattern for
    /// Linux container (AKS) compatibility.
    /// </summary>
    public sealed class Settings
    {
        private static readonly Settings _default = new Settings();

        public static Settings Default => _default;

        public string Host
        {
            get => Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost";
            set => Environment.SetEnvironmentVariable("DB_HOST", value);
        }

        public string Port
        {
            get => Environment.GetEnvironmentVariable("DB_PORT") ?? "3306";
            set => Environment.SetEnvironmentVariable("DB_PORT", value);
        }

        public string Database
        {
            get => Environment.GetEnvironmentVariable("DB_DATABASE") ?? "dtb_sptc";
            set => Environment.SetEnvironmentVariable("DB_DATABASE", value);
        }

        public string Username
        {
            get => Environment.GetEnvironmentVariable("DB_USERNAME") ?? "root";
            set => Environment.SetEnvironmentVariable("DB_USERNAME", value);
        }

        public string Password
        {
            get => Environment.GetEnvironmentVariable("DB_PASSWORD") ?? string.Empty;
            set => Environment.SetEnvironmentVariable("DB_PASSWORD", value);
        }

        /// <summary>
        /// No-op: settings are read from environment variables at runtime.
        /// Provided for API compatibility with call-sites that previously
        /// called Settings.Default.Reload().
        /// </summary>
        public void Reload() { /* environment variables are always current */ }
    }
}
