using System;
using StackExchange.Redis;

namespace SPTC_APPLICATION
{
    /// <summary>
    /// Manages distributed session/admin state via Azure Cache for Redis.
    ///
    /// Containerization remediation for rule cz-dotnet-0023 (Static Variables for State):
    /// The original static mutable field <c>AppState.IS_ADMIN</c> caused inconsistent
    /// behaviour when containers scaled horizontally because each pod held its own
    /// in-process copy of the flag.  This class replaces that pattern by persisting
    /// the flag in Azure Cache for Redis so every pod reads and writes the same value.
    ///
    /// Connection string is supplied through the environment variable
    /// <c>REDIS_CONNECTION_STRING</c>, which is injected at runtime by the
    /// AKS Secrets Store CSI Driver backed by Azure Key Vault and Workload Identity —
    /// no credentials are stored in source code or configuration files.
    ///
    /// Usage:
    ///   RedisStateManager.SetIsAdmin(sessionKey, true);
    ///   bool isAdmin = RedisStateManager.GetIsAdmin(sessionKey);
    ///   RedisStateManager.ClearIsAdmin(sessionKey);
    /// </summary>
    public static class RedisStateManager
    {
        // -----------------------------------------------------------------------
        // Redis connection — lazily initialised from the environment variable
        // REDIS_CONNECTION_STRING injected via AKS Key Vault CSI / Workload Identity
        // -----------------------------------------------------------------------
        private static readonly Lazy<IDatabase> _redisDb = new Lazy<IDatabase>(() =>
        {
            string connectionString = Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING")
                ?? throw new InvalidOperationException(
                    "REDIS_CONNECTION_STRING environment variable is not set. " +
                    "Ensure the AKS Secrets Store CSI Driver mounts the secret from Azure Key Vault.");

            ConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(connectionString);
            return multiplexer.GetDatabase();
        });

        private static IDatabase RedisDb => _redisDb.Value;

        // Key TTL — admin flag expires after 8 hours to prevent stale sessions
        private static readonly TimeSpan AdminFlagTtl = TimeSpan.FromHours(8);

        // -----------------------------------------------------------------------
        // IS_ADMIN — replaces the former static bool AppState.IS_ADMIN
        // -----------------------------------------------------------------------

        /// <summary>
        /// Persists the admin flag for the given session key in Redis.
        /// </summary>
        public static void SetIsAdmin(string sessionKey, bool value)
        {
            if (string.IsNullOrWhiteSpace(sessionKey))
                throw new ArgumentNullException(nameof(sessionKey));

            string redisKey = BuildAdminKey(sessionKey);
            RedisDb.StringSet(redisKey, value ? "1" : "0", AdminFlagTtl);
        }

        /// <summary>
        /// Retrieves the admin flag for the given session key from Redis.
        /// Returns <c>false</c> when the key does not exist or Redis is unavailable.
        /// </summary>
        public static bool GetIsAdmin(string sessionKey)
        {
            if (string.IsNullOrWhiteSpace(sessionKey))
                return false;

            try
            {
                string redisKey = BuildAdminKey(sessionKey);
                RedisValue value = RedisDb.StringGet(redisKey);
                return value.HasValue && value == "1";
            }
            catch (RedisException ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[RedisStateManager] Redis read error for IS_ADMIN: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Removes the admin flag for the given session key from Redis (used on logout).
        /// </summary>
        public static void ClearIsAdmin(string sessionKey)
        {
            if (string.IsNullOrWhiteSpace(sessionKey))
                return;

            try
            {
                string redisKey = BuildAdminKey(sessionKey);
                RedisDb.KeyDelete(redisKey);
            }
            catch (RedisException ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[RedisStateManager] Redis delete error for IS_ADMIN: {ex.Message}");
            }
        }

        // -----------------------------------------------------------------------
        // Helpers
        // -----------------------------------------------------------------------

        private static string BuildAdminKey(string sessionKey) =>
            $"sptc:session:{sessionKey}:is_admin";
    }
}
