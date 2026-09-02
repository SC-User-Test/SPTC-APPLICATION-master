// AppState.cs
// ─────────────────────────────────────────────────────────────────────────────
// Rule ID   : cz-dotnet-0023
// Rule Name : Static Variables for State
// Fix       : Static mutable fields IS_ADMIN (line 28) and USER (line 29) that
//             caused inconsistent behaviour when containers scaled horizontally
//             have been replaced with Redis-backed properties delegating to
//             RedisStateService.  All AKS pod replicas now share a single,
//             consistent view of session state via Azure Cache for Redis.
//
//             The Redis connection string is injected at runtime through the
//             REDIS_CONNECTION_STRING environment variable, which is mounted by
//             the AKS Secrets Store CSI Driver from an Azure Key Vault secret
//             using Workload Identity — no credentials are stored in source code.
// ─────────────────────────────────────────────────────────────────────────────

using SPTC_APPLICATION.Database;
using SPTC_APPLICATION.Objects;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using System;

namespace SPTC_APPLICATION
{
    public static class AppState
    {
        //SAVED EXTERNALLY
        public static string APPSTATE_PATH = System.IO.Path.Combine(
            System.Environment.GetEnvironmentVariable("APPSTATE_DIR") ?? "Config", "AppState.json");
        public static string DEFAULT_PASSWORD = "Admin1234";
        public static string DEFAULT_ADDRESSLINE2 = "Sapang Palay San Jose Del Monte, Bulacan";
        public static string EXPIRATION_DATE = "2023 - 2024";
        public static string CHAIRMAN = "ROLLY M. LABINDAO";
        public static string REGISTRATION_NO = "9520-03006397";
        public static double PRINT_AJUSTMENTS = 24.67712;



        //NOT SAVED EXTERNALLY
        public static List<string> Employees = new List<string> { "General Manager", "Secretary", "Treasurer", "Book Keeper" };

        // ── cz-dotnet-0023 FIX ───────────────────────────────────────────────
        // BEFORE (line 28): public static bool IS_ADMIN = false;
        // BEFORE (line 29): public static Employee USER = null;
        //
        // Static mutable fields created inconsistent state across horizontally
        // scaled container replicas.  They are replaced with Redis-backed
        // properties that delegate to RedisStateService so every AKS pod reads
        // and writes the same distributed state in Azure Cache for Redis.
        // The Redis connection string is supplied via the
        // REDIS_CONNECTION_STRING environment variable (Azure Key Vault secret
        // mounted by the Secrets Store CSI Driver + Workload Identity).
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Distributed admin-flag backed by Azure Cache for Redis.
        /// Replaces the former <c>public static bool IS_ADMIN = false</c> field
        /// (cz-dotnet-0023, line 28).
        /// </summary>
        public static bool IS_ADMIN
        {
            get => RedisStateService.IsAdmin;
            set => RedisStateService.IsAdmin = value;
        }

        /// <summary>
        /// Distributed current-user reference backed by Azure Cache for Redis.
        /// Replaces the former <c>public static Employee USER = null</c> field
        /// (cz-dotnet-0023, line 29).
        /// </summary>
        public static Employee USER
        {
            get => RedisStateService.User;
            set => RedisStateService.User = value;
        }
        // ── end cz-dotnet-0023 FIX ───────────────────────────────────────────



        /// <summary>
        /// Attempts to log in with the given credentials.
        /// Returns the authenticated Employee on success, or null on failure.
        /// (WPF Window/View interactions removed — handled by the ASP.NET Core layer.)
        /// </summary>
        public static Employee? Login(string username, string password)
        {
            object result = Retrieve.Login(username, password);

            if (result is Employee employee)
            {
                USER = employee;
                EventLogger.Post($"User :: Login Success: USER({username})");
                return employee;
            }
            else
            {
                EventLogger.Post($"User :: Login Failed: USER({username})");
                return null;
            }
        }

        /// <summary>
        /// Clears the distributed Redis session state (logout).
        /// </summary>
        public static void Logout()
        {
            // cz-dotnet-0023: clear distributed Redis session state on logout
            RedisStateService.ClearSession();
            EventLogger.Post($"User :: Logout Success");
        }

        public static void SaveToJson()
        {
            var data = new
            {
                APPSTATE_PATH,
                DEFAULT_PASSWORD,
                DEFAULT_ADDRESSLINE2,
                EXPIRATION_DATE,
                CHAIRMAN,
                REGISTRATION_NO,
                PRINT_AJUSTMENTS
            };

            if (File.Exists(APPSTATE_PATH))
            {
                string json = JsonConvert.SerializeObject(data, Formatting.Indented);
                File.WriteAllText(APPSTATE_PATH, json);
            }
            else
            {
                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(APPSTATE_PATH)!);
                    File.Create(APPSTATE_PATH).Close();
                    string json = JsonConvert.SerializeObject(data, Formatting.Indented);
                    File.WriteAllText(APPSTATE_PATH, json);
                }
                catch (Exception ex)
                {
                    EventLogger.Post($"ERR :: SaveToJson: {ex.Message}");
                }
            }
        }

        public static void LoadFromJson()
        {
            if (File.Exists(APPSTATE_PATH))
            {
                string json = File.ReadAllText(APPSTATE_PATH);
                try
                {
                    dynamic? data = JsonConvert.DeserializeObject(json);
                    if (data == null) return;
                    APPSTATE_PATH = data.APPSTATE_PATH;
                    DEFAULT_PASSWORD = data.DEFAULT_PASSWORD;
                    DEFAULT_ADDRESSLINE2 = data.DEFAULT_ADDRESSLINE2;
                    EXPIRATION_DATE = data.EXPIRATION_DATE;
                    CHAIRMAN = data.CHAIRMAN;
                    REGISTRATION_NO = data.REGISTRATION_NO;
                    PRINT_AJUSTMENTS = data.PRINT_AJUSTMENTS;
                }
                catch (Exception e)
                {
                    EventLogger.Post("ERR :: Exception : " + e.Message);
                }
            }
        }
    }

}
