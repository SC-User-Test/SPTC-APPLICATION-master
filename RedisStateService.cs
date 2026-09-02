// RedisStateService.cs
// ─────────────────────────────────────────────────────────────────────────────
// Rule ID   : cz-dotnet-0023
// Rule Name : Static Variables for State
// Fix       : Replaces static mutable IS_ADMIN / USER fields in AppState with
//             Azure Cache for Redis-backed distributed state so that all AKS
//             pod replicas share a single, consistent view of session data.
//
// Connection string is read from the REDIS_CONNECTION_STRING environment
// variable, which is injected at runtime by the AKS Secrets Store CSI Driver
// (Azure Key Vault secret) via Workload Identity — never hard-coded here.
// ─────────────────────────────────────────────────────────────────────────────

using System;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace SPTC_APPLICATION
{
    /// <summary>
    /// Provides distributed session-state storage backed by Azure Cache for Redis.
    /// The Redis connection string is supplied through the
    /// <c>REDIS_CONNECTION_STRING</c> environment variable, which is mounted into
    /// the AKS pod by the Secrets Store CSI Driver using Workload Identity and an
    /// Azure Key Vault secret — credentials are never stored in source code.
    /// </summary>
    public static class RedisStateService
    {
        // ── Connection ────────────────────────────────────────────────────────
        private static readonly Lazy<ConnectionMultiplexer> _lazyConnection =
            new Lazy<ConnectionMultiplexer>(() =>
            {
                string connectionString =
                    Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING")
                    ?? throw new InvalidOperationException(
                        "REDIS_CONNECTION_STRING environment variable is not set. " +
                        "Mount the Azure Key Vault secret via the AKS Secrets Store " +
                        "CSI Driver with Workload Identity.");

                return ConnectionMultiplexer.Connect(connectionString);
            });

        private static IDatabase Db => _lazyConnection.Value.GetDatabase();

        // ── Key constants ─────────────────────────────────────────────────────
        private const string KeyIsAdmin = "appstate:is_admin";
        private const string KeyUser    = "appstate:user";

        // ── IS_ADMIN ──────────────────────────────────────────────────────────

        /// <summary>
        /// Gets or sets the IS_ADMIN flag in Redis.
        /// Replaces the former <c>public static bool IS_ADMIN</c> field in
        /// <see cref="AppState"/> (cz-dotnet-0023, line 28).
        /// </summary>
        public static bool IsAdmin
        {
            get
            {
                try
                {
                    RedisValue value = Db.StringGet(KeyIsAdmin);
                    return value.HasValue && (bool)value;
                }
                catch (Exception ex)
                {
                    EventLogger.Post($"RedisStateService :: IsAdmin GET error: {ex.Message}");
                    return false;
                }
            }
            set
            {
                try
                {
                    Db.StringSet(KeyIsAdmin, value);
                }
                catch (Exception ex)
                {
                    EventLogger.Post($"RedisStateService :: IsAdmin SET error: {ex.Message}");
                }
            }
        }

        // ── USER ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Gets or sets the current logged-in <see cref="Objects.Employee"/> in Redis.
        /// Replaces the former <c>public static Employee USER</c> field in
        /// <see cref="AppState"/> (cz-dotnet-0023, line 29).
        /// The object is serialised to JSON before storage and deserialised on retrieval.
        /// </summary>
        public static Objects.Employee? User
        {
            get
            {
                try
                {
                    RedisValue json = Db.StringGet(KeyUser);
                    if (!json.HasValue || json.IsNullOrEmpty)
                        return null;

                    return JsonConvert.DeserializeObject<Objects.Employee>(json!);
                }
                catch (Exception ex)
                {
                    EventLogger.Post($"RedisStateService :: User GET error: {ex.Message}");
                    return null;
                }
            }
            set
            {
                try
                {
                    if (value == null)
                    {
                        Db.KeyDelete(KeyUser);
                    }
                    else
                    {
                        string json = JsonConvert.SerializeObject(value);
                        Db.StringSet(KeyUser, json);
                    }
                }
                catch (Exception ex)
                {
                    EventLogger.Post($"RedisStateService :: User SET error: {ex.Message}");
                }
            }
        }

        // ── Session clear ─────────────────────────────────────────────────────

        /// <summary>
        /// Clears all session-state keys from Redis (called on logout).
        /// </summary>
        public static void ClearSession()
        {
            try
            {
                Db.KeyDelete(KeyIsAdmin);
                Db.KeyDelete(KeyUser);
            }
            catch (Exception ex)
            {
                EventLogger.Post($"RedisStateService :: ClearSession error: {ex.Message}");
            }
        }
    }
}
