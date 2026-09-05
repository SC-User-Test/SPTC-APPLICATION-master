using System.ComponentModel;
using System.Reflection;
using System.Threading.Tasks;
using Npgsql;

namespace SPTC_APPLICATION.Database
{
    public class DatabaseConnection
    {
        private static string? connectionString;

        public DatabaseConnection(string connectionString)
        {
            DatabaseConnection.connectionString = connectionString;
        }

        public static NpgsqlConnection GetConnection()
        {
            // .NET 8: connectionString may be null if not initialized; guard against it
            return new NpgsqlConnection(DatabaseConnection.connectionString ?? string.Empty);
        }

        public static string GetEnumDescription(ConnectionLogs value)
        {
            FieldInfo? fieldInfo = value.GetType().GetField(value.ToString());

            DescriptionAttribute[]? attributes = (DescriptionAttribute[]?)fieldInfo?.GetCustomAttributes(typeof(DescriptionAttribute), false);

            return attributes != null && attributes.Length > 0 ? attributes[0].Description : value.ToString();
        }

        public class Builder
        {
            private string connectionString;

            public ConnectionLogs Log { private set; get; }

            public Builder(string host, string port, string database, string username, string password)
            {
                // PostgreSQL connection string format using Npgsql
                connectionString = $"Host={host};Port={port};Database={database};Username={username};Password={password};";
            }

            public async Task<bool> CreateAsync()
            {
                if (!string.IsNullOrEmpty(connectionString))
                {
                    try
                    {
                        DatabaseConnection connection = new DatabaseConnection(connectionString);

                        NpgsqlConnection npgsqlConnection = DatabaseConnection.GetConnection();
                        await npgsqlConnection.OpenAsync();
                        await Task.Delay(1000);
                        npgsqlConnection.Close();

                        Log = ConnectionLogs.ESTABLISHED;
                        return true;
                    }
                    catch (NpgsqlException ex)
                    {
                        // PostgreSQL error codes: 28P01 = invalid_password, 3D000 = invalid_catalog_name
                        // 08001 = sqlclient_unable_to_establish_sqlconnection
                        if (ex.SqlState == "28P01" || ex.SqlState == "28000")
                        {
                            Log = ConnectionLogs.WRONG_PASSWORD;
                        }
                        else if (ex.SqlState == "08001" || ex.SqlState == "08006" || ex.SqlState == "08000")
                        {
                            Log = ConnectionLogs.CANNOT_CONNECT;
                        }
                        else
                        {
                            Log = ConnectionLogs.EXCEPTION_OCCURED;
                        }
                        return false;
                    }
                }
                else
                {
                    Log = ConnectionLogs.STRING_EMPTY;
                    return false;
                }
            }

            public bool Connect()
            {
                if (!string.IsNullOrEmpty(connectionString))
                {
                    try
                    {
                        DatabaseConnection connection = new DatabaseConnection(connectionString);

                        NpgsqlConnection npgsqlConnection = DatabaseConnection.GetConnection();
                        npgsqlConnection.Open();
                        npgsqlConnection.Close();

                        Log = ConnectionLogs.ESTABLISHED;
                        return true;
                    }
                    catch (NpgsqlException ex)
                    {
                        if (ex.SqlState == "28P01" || ex.SqlState == "28000")
                        {
                            Log = ConnectionLogs.WRONG_PASSWORD;
                        }
                        else if (ex.SqlState == "08001" || ex.SqlState == "08006" || ex.SqlState == "08000")
                        {
                            Log = ConnectionLogs.CANNOT_CONNECT;
                        }
                        else
                        {
                            Log = ConnectionLogs.EXCEPTION_OCCURED;
                        }
                        return false;
                    }
                }
                else
                {
                    Log = ConnectionLogs.STRING_EMPTY;
                    return false;
                }
            }
        }
    }

    public enum ConnectionLogs
    {
        [Description("Empty Connection string")]
        STRING_EMPTY,

        [Description("Connection Established")]
        ESTABLISHED,

        [Description("Exception Occurred")]
        EXCEPTION_OCCURED,

        [Description("Wrong Password")]
        WRONG_PASSWORD,

        [Description("Cannot Connect")]
        CANNOT_CONNECT,
    }
}
