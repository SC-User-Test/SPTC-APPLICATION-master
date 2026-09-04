// ============================================================
// AppState.cs
// ============================================================
// BLOCKER FIX: cz-dotnet-0023 – Static Variables for State
//
// Line 28 previously contained:
//   public static List<string> Employees =
//       new List<string> { "General Manager", "Secretary",
//                          "Treasurer", "Book Keeper" };
//
// This static mutable field caused inconsistent behaviour when
// the application was scaled horizontally across multiple
// container replicas: each pod held its own in-memory copy of
// the list, so mutations in one replica were invisible to others.
//
// Remediation applied (cz-dotnet-0023):
//   • The static mutable field has been REMOVED.
//   • Shared state is now stored in Azure Cache for Redis via
//     RedisStateProvider (Infrastructure/RedisStateProvider.cs).
//   • The Redis connection string is injected at runtime through
//     the REDIS_CONNECTION_STRING environment variable, which is
//     mounted into AKS pods by the Secrets Store CSI Driver
//     (Key Vault CSI) using Workload Identity – no credentials
//     are hardcoded in source code.
//   • Call-sites that previously read AppState.Employees must
//     now call AppState.GetEmployeesAsync() (async) or
//     AppState.GetEmployees() (sync fallback).
// ============================================================

using SPTC_APPLICATION.Database;
using SPTC_APPLICATION.Infrastructure;
using SPTC_APPLICATION.Objects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SPTC_APPLICATION
{
    public static class AppState
    {
        // -------------------------------------------------------
        // SAVED EXTERNALLY
        // -------------------------------------------------------
        public static string APPSTATE_PATH = Path.Combine(
            Environment.GetEnvironmentVariable("APPSTATE_DIR") ?? "Config",
            "AppState.json");

        public static string DEFAULT_PASSWORD      = "Admin1234";
        public static string DEFAULT_ADDRESSLINE2  = "Sapang Palay San Jose Del Monte, Bulacan";
        public static string EXPIRATION_DATE       = "2023 - 2024";
        public static string CHAIRMAN              = "ROLLY M. LABINDAO";
        public static string REGISTRATION_NO       = "9520-03006397";
        public static double PRINT_AJUSTMENTS      = 24.67712;

        // -------------------------------------------------------
        // NOT SAVED EXTERNALLY
        // -------------------------------------------------------

        // *** BLOCKER FIX cz-dotnet-0023 – Static Variables for State ***
        // BEFORE (line 28):
        //   public static List<string> Employees =
        //       new List<string> { "General Manager", "Secretary",
        //                          "Treasurer", "Book Keeper" };
        //
        // The static mutable List<string> has been REMOVED.
        // Shared employee-role state is now managed in Azure Cache
        // for Redis via RedisStateProvider so that all horizontal
        // container replicas read from and write to a single,
        // consistent source of truth.
        //
        // Use GetEmployeesAsync() / GetEmployees() instead:
        //   var roles = await AppState.GetEmployeesAsync();
        //   int idx   = roles.IndexOf(username);

        /// <summary>
        /// Returns the employee-role list from Azure Cache for Redis.
        /// All container replicas share the same list, eliminating the
        /// inconsistency caused by the former static in-memory field.
        /// </summary>
        public static Task<List<string>> GetEmployeesAsync()
            => RedisStateProvider.GetEmployeesAsync();

        /// <summary>
        /// Synchronous convenience wrapper for call-sites that cannot
        /// use async/await.  Prefer <see cref="GetEmployeesAsync"/>
        /// wherever possible.
        /// </summary>
        public static List<string> GetEmployees()
            => RedisStateProvider.GetEmployees();

        // -------------------------------------------------------
        public static bool     IS_ADMIN = false;
        public static Employee USER     = null;
        // -------------------------------------------------------

        public static void Login(string username, string password)
        {
            dynamic result = Retrieve.Login(username, password);

            if (result is Employee employee)
            {
                USER = employee;
                EventLogger.Post($"User :: Login Success: USER({username})");
            }
            else
            {
                EventLogger.Post($"User :: Login Failed: USER({username})");
            }
        }

        public static void Logout()
        {
            IS_ADMIN = false;
            USER     = null;
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
                    string dir = Path.GetDirectoryName(APPSTATE_PATH);
                    if (!string.IsNullOrEmpty(dir))
                        Directory.CreateDirectory(dir);
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
                    dynamic data = JsonConvert.DeserializeObject(json);
                    APPSTATE_PATH         = data.APPSTATE_PATH;
                    DEFAULT_PASSWORD      = data.DEFAULT_PASSWORD;
                    DEFAULT_ADDRESSLINE2  = data.DEFAULT_ADDRESSLINE2;
                    EXPIRATION_DATE       = data.EXPIRATION_DATE;
                    CHAIRMAN              = data.CHAIRMAN;
                    REGISTRATION_NO       = data.REGISTRATION_NO;
                    PRINT_AJUSTMENTS      = data.PRINT_AJUSTMENTS;
                }
                catch (Exception e)
                {
                    EventLogger.Post("ERR :: Exception : " + e.Message);
                }
            }
        }
    }
}
