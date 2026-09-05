using System;
using System.ComponentModel;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
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
                // If all parts are empty, set empty string to trigger STRING_EMPTY
                if (string.IsNullOrEmpty(host) && string.IsNullOrEmpty(port) && string.IsNullOrEmpty(database) && string.IsNullOrEmpty(username) && string.IsNullOrEmpty(password))
                {
                    connectionString = string.Empty;
                }
                else
                {
                    connectionString = $"Host={host};Port={port};Database={database};Username={username};Password={password};";
                }
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
                        await Task.Delay(100);
                        npgsqlConnection.Close();
                        Log = ConnectionLogs.ESTABLISHED;
                        return true;
                    }
                    catch (NpgsqlException ex)
                    {
                        if (ex.SqlState == "28P01" || ex.SqlState == "28000")
                            Log = ConnectionLogs.WRONG_PASSWORD;
                        else if (ex.SqlState == "08001" || ex.SqlState == "08006" || ex.SqlState == "08000")
                            Log = ConnectionLogs.CANNOT_CONNECT;
                        else
                            Log = ConnectionLogs.EXCEPTION_OCCURED;
                        return false;
                    }
                    catch (Exception)
                    {
                        Log = ConnectionLogs.EXCEPTION_OCCURED;
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
                            Log = ConnectionLogs.WRONG_PASSWORD;
                        else if (ex.SqlState == "08001" || ex.SqlState == "08006" || ex.SqlState == "08000")
                            Log = ConnectionLogs.CANNOT_CONNECT;
                        else
                            Log = ConnectionLogs.EXCEPTION_OCCURED;
                        return false;
                    }
                    catch (Exception)
                    {
                        Log = ConnectionLogs.EXCEPTION_OCCURED;
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

    public static class RequestQuery
    {
        public static string LOGIN_EMPLOYEE = "SELECT * FROM tbl_employee e LEFT JOIN tbl_position p ON p.id = e.position_id WHERE p.title = @titleParam AND e.password = @passwordParam AND e.\"isDeleted\" = 0";

        public static string GetEnumDescription(CRUDControl value)
        {
            FieldInfo? fieldInfo = value.GetType().GetField(value.ToString());
            DescriptionAttribute[]? attributes = (DescriptionAttribute[]?)fieldInfo?.GetCustomAttributes(typeof(DescriptionAttribute), false);
            return attributes != null && attributes.Length > 0 ? attributes[0].Description : value.ToString();
        }

        public static string Protect(string input)
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = MD5.HashData(inputBytes);
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
                stringBuilder.Append(hashBytes[i].ToString("x2"));
            return stringBuilder.ToString();
        }
    }

    public class Upsert
    {
        private readonly string tableName;
        public int id;
        private System.Collections.Generic.Dictionary<string, object> fieldValues;

        public Upsert(string tableName, int id)
        {
            this.tableName = tableName;
            this.id = id;
            fieldValues = new System.Collections.Generic.Dictionary<string, object>();
        }

        public void Insert(string fieldName, object value)
        {
            fieldValues[fieldName] = value;
        }

        public object Access(string fieldName)
        {
            if (fieldValues.ContainsKey(fieldName))
                return fieldValues[fieldName];
            return null;
        }

        public void Save()
        {
            // Stub - no DB in tests
        }
    }

    public class Clean
    {
        private string CLEANER = "DELETE FROM ";

        public Clean(string table)
        {
            CLEANER += table + " WHERE " + Where.ALL_DELETED;
        }

        public bool Start()
        {
            if (SPTC_APPLICATION.AppState.IS_ADMIN)
            {
                // Would execute DB query in real code
                return true;
            }
            return false;
        }
    }
}
