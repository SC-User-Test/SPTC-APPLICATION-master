using System.Threading.Tasks;
using SPTC_APPLICATION.Database;
using SPTC_APPLICATION.Properties;

namespace SPTC_APPLICATION.Objects
{
    public static class Controller
    {

        // START UP INITIALIZATION
        public static async Task StartInitializationAsync()
        {
            string host = Settings.Default.Host;
            string port = Settings.Default.Port;
            string database = Settings.Default.Database;
            string username = Settings.Default.Username;
            string password = Settings.Default.Password;
            AppState.LoadFromJson();
            AppState.SaveToJson();

            DatabaseConnection.Builder builder = CreateDatabaseConnectionBuilder(host, port, database, username, password);

            bool isConnected = await builder.CreateAsync();

            int maxAttempts = 3;
            int attemptCount = 1;

            while (!IsConnectionSuccessful(isConnected, builder.Log) && attemptCount <= maxAttempts)
            {
                EventLogger.Post($"DTB :: Database Connection attempt ({attemptCount}) :{builder.Log.ToString()}");
                Settings.Default.Reload();
                UpdateSettingsFromDefault(ref host, ref port, ref database, ref username, ref password);

                builder = CreateDatabaseConnectionBuilder(host, port, database, username, password);
                isConnected = await builder.CreateAsync();

                attemptCount++;
            }

            if (isConnected && builder.Log == ConnectionLogs.ESTABLISHED)
            {
                EventLogger.Post("DTB :: Database Connection Established");
            }
            else
            {
                EventLogger.Post($"DTB :: Database Connection Failed: {builder.Log}");
            }
        }

        private static DatabaseConnection.Builder CreateDatabaseConnectionBuilder(string host, string port, string database, string username, string password)
        {
            return new DatabaseConnection.Builder(host, port, database, username, password);
        }

        private static bool IsConnectionSuccessful(bool isConnected, ConnectionLogs log)
        {
            return isConnected && log == ConnectionLogs.ESTABLISHED;
        }

        private static void UpdateSettingsFromDefault(ref string host, ref string port, ref string database, ref string username, ref string password)
        {
            host = Settings.Default.Host;
            port = Settings.Default.Port;
            database = Settings.Default.Database;
            username = Settings.Default.Username;
            password = Settings.Default.Password;
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
