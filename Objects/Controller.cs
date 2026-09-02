using System.Threading.Tasks;
using SPTC_APPLICATION.Database;

namespace SPTC_APPLICATION.Objects
{
    /// <summary>
    /// Application controller — startup initialization and database connection management.
    /// WPF/Window-specific overloads have been removed; the ASP.NET Core host (Program.cs)
    /// manages the application lifecycle on net8.0.
    /// </summary>
    public static class Controller
    {
        // START UP INITIALIZATION
        public static async Task<bool> StartInitializationAsync(string host, string port, string database, string username, string password)
        {
            AppState.LoadFromJson();
            AppState.SaveToJson();

            DatabaseConnection.Builder builder = new DatabaseConnection.Builder(host, port, database, username, password);

            bool isConnected = await builder.CreateAsync();

            int maxAttempts = 3;
            int attemptCount = 1;

            while (!IsConnectionSuccessful(isConnected, builder.Log) && attemptCount <= maxAttempts)
            {
                EventLogger.Post($"DTB :: Database Connection attempt ({attemptCount}) :{builder.Log}");
                builder = new DatabaseConnection.Builder(host, port, database, username, password);
                isConnected = await builder.CreateAsync();
                attemptCount++;
            }

            if (isConnected && builder.Log == ConnectionLogs.ESTABLISHED)
            {
                EventLogger.Post("DTB :: Connection established successfully.");
                return true;
            }
            else
            {
                HandleConnectionFailure(builder.Log);
                return false;
            }
        }

        private static bool IsConnectionSuccessful(bool isConnected, ConnectionLogs log)
        {
            return isConnected && log == ConnectionLogs.ESTABLISHED;
        }

        private static void HandleConnectionFailure(ConnectionLogs logType)
        {
            if (logType == ConnectionLogs.CANNOT_CONNECT)
            {
                EventLogger.Post($"DTB :: {DatabaseConnection.GetEnumDescription(logType)} — Check if database is online");
            }
            else if (logType == ConnectionLogs.WRONG_PASSWORD)
            {
                EventLogger.Post($"DTB :: {DatabaseConnection.GetEnumDescription(logType)} — Input the correct password and try again");
            }
            else
            {
                EventLogger.Post($"DTB :: Connection failure: {DatabaseConnection.GetEnumDescription(logType)}");
            }
        }

        //FOR DEBUG PURPOSE
        public static void CreateEmployee(int userindex)
        {
            Upsert employee = new Upsert(Table.EMPLOYEE, -1);
            employee.Insert(Field.PASSWORD, RequestQuery.Protect(AppState.DEFAULT_PASSWORD));
            employee.Insert(Field.POSITION_ID, userindex);
            employee.Save();
        }
    }
}
