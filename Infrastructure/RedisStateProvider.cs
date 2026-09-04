// ============================================================
// RedisStateProvider.cs
// ============================================================
// BLOCKER FIX: cz-dotnet-0023 – Static Variables for State
//
// Replaces the static mutable List<string> Employees field in
// AppState (line 28) with a Redis-backed state provider.
//
// The Redis connection string is NOT hardcoded here.  It is
// injected at runtime via one of two mechanisms:
//
//   1. Azure Key Vault CSI Driver (preferred for AKS):
//      The Secrets Store CSI Driver mounts the secret as an
//      environment variable REDIS_CONNECTION_STRING into the pod
//      using a SecretProviderClass that references the Key Vault
//      secret.  Workload Identity (IRSA / AAD Pod Identity) is
//      used so no credentials are stored in the image or in
//      Kubernetes Secrets.
//
//   2. Plain environment variable (local / CI):
//      Set REDIS_CONNECTION_STRING=<host>:<port>,password=<pwd>
//      before running the application.
//
// Usage:
//   var employees = await RedisStateProvider.GetEmployeesAsync();
//   await RedisStateProvider.SetEmployeesAsync(employees);
// ============================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StackExchange.Redis;
using Newtonsoft.Json;
using SPTC_APPLICATION.Objects;

namespace SPTC_APPLICATION.Infrastructure
{
    /// <summary>
    /// Provides access to shared application state stored in
    /// Azure Cache for Redis.  Replaces the static mutable
    /// <c>List&lt;string&gt; Employees</c> field that caused
    /// inconsistent behaviour when containers scaled horizontally
    /// (cz-dotnet-0023).
    /// </summary>
    public static class RedisStateProvider
    {
        // -------------------------------------------------------
        // Redis key constants
        // -------------------------------------------------------
        private const string EmployeesKey = "appstate:employees";

        // Default employee roles used when the key is absent from
        // Redis (first-run / cold-start scenario).
        private static readonly IReadOnlyList<string> DefaultEmployees =
            new List<string> { "General Manager", "Secretary", "Treasurer", "Book Keeper" };

        // -------------------------------------------------------
        // Lazy connection – created once per process lifetime.
        // The connection string is read from the environment so
        // that no credentials are embedded in source code.
        // -------------------------------------------------------
        private static readonly Lazy<IConnectionMultiplexer> _lazyConnection =
            new Lazy<IConnectionMultiplexer>(() =>
            {
                string connectionString =
                    Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING")
                    ?? throw new InvalidOperationException(
                        "REDIS_CONNECTION_STRING environment variable is not set. " +
                        "Configure it via the Azure Key Vault CSI Driver / " +
                        "Secrets Store CSI Driver with Workload Identity, or " +
                        "supply it directly for local development.");

                return ConnectionMultiplexer.Connect(connectionString);
            });

        private static IDatabase RedisDb =>
            _lazyConnection.Value.GetDatabase();

        // -------------------------------------------------------
        // Public API
        // -------------------------------------------------------

        /// <summary>
        /// Retrieves the employee-role list from Redis.
        /// Falls back to <see cref="DefaultEmployees"/> when the
        /// key does not exist (e.g. first run) and seeds Redis so
        /// subsequent calls are consistent across all replicas.
        /// </summary>
        public static async Task<List<string>> GetEmployeesAsync()
        {
            try
            {
                RedisValue raw = await RedisDb.StringGetAsync(EmployeesKey);

                if (raw.HasValue)
                {
                    var list = JsonConvert.DeserializeObject<List<string>>(raw!);
                    if (list != null && list.Count > 0)
                        return list;
                }

                // Key absent – seed Redis with the default values so all
                // horizontal replicas share the same initial state.
                var defaults = new List<string>(DefaultEmployees);
                await SetEmployeesAsync(defaults);
                return defaults;
            }
            catch (Exception ex)
            {
                EventLogger.Post($"RedisStateProvider :: GetEmployeesAsync error: {ex.Message}");
                // Degrade gracefully – return defaults so the application
                // remains functional even if Redis is temporarily unavailable.
                return new List<string>(DefaultEmployees);
            }
        }

        /// <summary>
        /// Persists the employee-role list to Redis so that all
        /// running container replicas see the updated value.
        /// </summary>
        public static async Task SetEmployeesAsync(List<string> employees)
        {
            try
            {
                string json = JsonConvert.SerializeObject(employees);
                await RedisDb.StringSetAsync(EmployeesKey, json);
            }
            catch (Exception ex)
            {
                EventLogger.Post($"RedisStateProvider :: SetEmployeesAsync error: {ex.Message}");
            }
        }

        /// <summary>
        /// Synchronous convenience wrapper around
        /// <see cref="GetEmployeesAsync"/> for call-sites that
        /// cannot use async/await (e.g. static field initialisers).
        /// </summary>
        public static List<string> GetEmployees()
        {
            try
            {
                return GetEmployeesAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                EventLogger.Post($"RedisStateProvider :: GetEmployees error: {ex.Message}");
                return new List<string>(DefaultEmployees);
            }
        }
    }
}
