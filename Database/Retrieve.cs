using System;
using System.Collections.Generic;
using System.Reflection;
using MySql.Data.MySqlClient;
using SPTC_APPLICATION.Objects;

namespace SPTC_APPLICATION.Database
{
    //USAGE:
    /*
     all static class
    Login(username password) for Employees only
    await Retrieve.GetData<object>() this is the main functionality of the Retrieve, it will retrieve from database whatever (tablename, selectquery, wherequery, mysqlparameters) inputted
    make sure that every <object> has the receiver constructor like this
    public object(MySqlDataReader reader)
    {
         //SET THE RESULT OF reader to each class attribute
    }

    for result containing a foreign key use
    Retrieve.GetValueOrDefault(reader, ForeignKey Field)

    it is recommended if there are foreign keys to create a Populate(ForeignKeyResults int) then await Retrieve each and store to each respective class attr
    create also an Empty constructor without parameters to Specify empty or null result from database
     */
    public class Retrieve
    {
        private static Employee ExecuteQueryAsync(string query, params MySqlParameter[] parameters)
        {
            using (MySqlConnection connection = DatabaseConnection.GetConnection())
            {
                connection.Open();

                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddRange(parameters);

                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        return ReadData<Employee>(reader);
                    }
                }
            }

            return null;
        }

        public static object Login(string username, string password)
        {
            try
            {
                MySqlParameter usernameParam = new MySqlParameter("titleParam", username);
                MySqlParameter passwordParam = new MySqlParameter("passwordParam", RequestQuery.Protect(password));

                Employee employee = ExecuteQueryAsync(RequestQuery.LOGIN_EMPLOYEE, usernameParam, passwordParam);
                if (employee != null)
                {
                    return employee;
                }
                else
                {
                    EventLogger.Post($"Login :: Wrong Password for user: {username}");
                    return null;
                }
            }
            catch (MySqlException ex)
            {
                EventLogger.Post($"Login :: Exception Occurred: {ex.Message}");
                return null;
            }
        }

        public static List<T> GetData<T>(string tableName, string selectQuery, string whereQuery, params MySqlParameter[] parameters)
        {
            Type type = typeof(T);
            ConstructorInfo constructor = type.GetConstructor(Type.EmptyTypes);
            List<T> results = new List<T>();
            try
            {
                using (MySqlConnection connection = DatabaseConnection.GetConnection())
                {
                    connection.Open();

                    string query = $"SELECT {selectQuery} FROM {tableName} WHERE {whereQuery}";

                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddRange(parameters);

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            T item = ReadData<T>(reader);
                            results.Add(item);
                        }
                    }

                    if (results.Count == 0)
                    {
                        if (typeof(T).IsClass && constructor != null)
                        {
                            results.Add((T)constructor.Invoke(null));
                        }
                        else
                        {
                            results.Add(default(T));
                        }
                    }

                    return results;
                }
            }
            catch (MySqlException ex)
            {
                EventLogger.Post($"DTB :: {ex.Message}");

                if (results.Count == 0)
                {
                    if (typeof(T).IsClass && constructor != null)
                    {
                        results.Add((T)constructor.Invoke(null));
                    }
                    else
                    {
                        results.Add(default(T));
                    }
                }

                return results;
            }
        }

        private static T ReadData<T>(MySqlDataReader reader)
        {
            Type type = typeof(T);
            ConstructorInfo readerConstructor = type.GetConstructor(new[] { typeof(MySqlDataReader) });
            ConstructorInfo emptyConstructor = type.GetConstructor(Type.EmptyTypes);

            if (readerConstructor != null)
            {
                object[] parameters = new object[] { reader };
                try
                {
                    return (T)readerConstructor.Invoke(parameters);
                }
                catch (Exception ex)
                {
                    EventLogger.Post($"ERR :: ReadData<{typeof(T)}> {ex.Message}");
                }
            }

            if (emptyConstructor != null)
            {
                return (T)emptyConstructor.Invoke(null);
            }

            return default(T);
        }

        public static T GetValueOrDefault<T>(MySqlDataReader reader, string columnName)
        {
            try
            {
                int ordinal = reader.GetOrdinal(columnName);
                return reader.IsDBNull(ordinal) ? default(T) : reader.GetFieldValue<T>(ordinal);
            }
            catch (Exception ex)
            {
                EventLogger.Post($"ERR :: Reader<{typeof(T)}>(columnName) {ex.Message}");
                return default(T);
            }
        }
    }
}
