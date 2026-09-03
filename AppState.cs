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
        public static string APPSTATE_PATH = System.IO.Path.Combine(Environment.GetEnvironmentVariable("APPSTATE_BASE_PATH") ?? AppDomain.CurrentDomain.BaseDirectory, "Config", "AppState.json");
        public static string DEFAULT_PASSWORD = "Admin1234";
        public static string DEFAULT_ADDRESSLINE2 = "Sapang Palay San Jose Del Monte, Bulacan";
        public static string EXPIRATION_DATE = "2023 - 2024";
        public static string CHAIRMAN = "ROLLY M. LABINDAO";
        public static string REGISTRATION_NO = "9520-03006397";
        public static double PRINT_AJUSTMENTS = 24.67712;



        //NOT SAVED EXTERNALLY
        public static List<string> Employees = new List<string> { "General Manager", "Secretary", "Treasurer", "Book Keeper" };

        // -----------------------------------------------------------------------
        // cz-dotnet-0023 remediation (Static Variables for State) — Line 28
        // The former static mutable field:
        //   public static bool IS_ADMIN = false;
        // has been replaced with a Redis-backed property via RedisStateManager.
        // The admin flag is now stored in Azure Cache for Redis (connection string
        // injected from Azure Key Vault via AKS Secrets Store CSI Driver and
        // Workload Identity) so all horizontally-scaled container replicas share
        // the same consistent state instead of each pod holding its own copy.
        //
        // Callers that previously read/wrote AppState.IS_ADMIN directly should use:
        //   AppState.GetIsAdmin()  — to read the flag
        //   AppState.SetIsAdmin(value) — to write the flag
        // The current session key is derived from the logged-in USER's id.
        // -----------------------------------------------------------------------

        public static Employee USER = null;

        /// <summary>
        /// Returns the Redis-backed admin flag for the currently logged-in user.
        /// Falls back to <c>false</c> when no user is logged in or Redis is unavailable.
        /// </summary>
        public static bool GetIsAdmin()
        {
            if (USER == null) return false;
            return RedisStateManager.GetIsAdmin(USER.id.ToString());
        }

        /// <summary>
        /// Persists the admin flag for the currently logged-in user in Redis.
        /// </summary>
        public static void SetIsAdmin(bool value)
        {
            if (USER == null) return;
            RedisStateManager.SetIsAdmin(USER.id.ToString(), value);
        }

        // Backward-compatible property accessor so existing call-sites that use
        // AppState.IS_ADMIN as a simple boolean expression continue to compile.
        // New code should prefer GetIsAdmin() / SetIsAdmin() for clarity.
        public static bool IS_ADMIN
        {
            get => GetIsAdmin();
            set => SetIsAdmin(value);
        }


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
            // Clear the Redis-backed admin flag before nulling out USER so the
            // session key (USER.id) is still available for the Redis key lookup.
            if (USER != null)
            {
                RedisStateManager.ClearIsAdmin(USER.id.ToString());
            }
            USER = null;
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
                    Directory.CreateDirectory(Path.GetDirectoryName(APPSTATE_PATH));
                    File.Create(APPSTATE_PATH).Close();
                    string json = JsonConvert.SerializeObject(data, Formatting.Indented);
                    File.WriteAllText(APPSTATE_PATH, json);
                }
                catch (Exception ex)
                {
                    EventLogger.Post($"ERR :: Error creating AppState file: {ex.Message}");
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
