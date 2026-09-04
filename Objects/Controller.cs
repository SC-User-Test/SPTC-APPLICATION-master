using System.Threading.Tasks;
using SPTC_APPLICATION.Database;
using SPTC_APPLICATION.Objects;

namespace SPTC_APPLICATION.Objects
{
    /// <summary>
    /// Application controller – headless ASP.NET Core version.
    /// WPF-specific UI interactions (ProgressBar, Window, TextBox) have been
    /// removed. Database initialisation is now performed asynchronously at
    /// startup via the ASP.NET Core hosted service pipeline.
    /// </summary>
    public static class Controller
    {
        // ------------------------------------------------------------------
        // Database initialisation (called from Program.cs / startup)
        // ------------------------------------------------------------------

        /// <summary>
        /// Attempts to establish a database connection using the supplied
        /// credentials and returns true when the connection is successful.
        /// </summary>
        public static async Task<bool> InitialiseDatabaseAsync(
            string host, string port, string database,
            string username, string password)
        {
            AppState.LoadFromJson();
            AppState.SaveToJson();

            var builder = new DatabaseConnection.Builder(host, port, database, username, password);
            bool isConnected = await builder.CreateAsync();

            if (isConnected && builder.Log == ConnectionLogs.ESTABLISHED)
            {
                EventLogger.Post($"DTB :: Database connection established.");
                return true;
            }
            else
            {
                EventLogger.Post($"DTB :: Database connection failed: {builder.Log}");
                return false;
            }
        }

        // ------------------------------------------------------------------
        // FOR DEBUG PURPOSE
        // ------------------------------------------------------------------
        public static void CreateEmployee(int userindex)
        {
            Upsert employee = new Upsert(Table.EMPLOYEE, -1);
            employee.Insert(Field.PASSWORD, RequestQuery.Protect(AppState.DEFAULT_PASSWORD));
            employee.Insert(Field.POSITION_ID, userindex);
            employee.Save();
        }
    }
}
