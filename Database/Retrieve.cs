using System;
using System.Collections.Generic;
using System.Reflection;
using Npgsql;
using SPTC_APPLICATION.Objects;
using SPTC_APPLICATION.View;

namespace SPTC_APPLICATION.Database
{
    //USAGE:
    /*
     all static class
    Login(username password) for Employees only
    await Retrieve.GetData<object>() this is the main functionality of the Retrieve, it will retrieve from database whatever (tablename, selectquery, wherequery, npgsqlparameters) inputted
    make sure that every <object> has the receiver constructor like this
    public object(NpgsqlDataReader reader)
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
        private static Employee? ExecuteQueryAsync(string query, params NpgsqlParameter[] parameters)
        {
            using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
            {
                connection.Open();

                NpgsqlCommand command = new NpgsqlCommand(query, connection);
                command.Parameters.AddRange(parameters);

                using (NpgsqlDataReader reader = command.ExecuteReader())
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
                NpgsqlParameter usernameParam = new NpgsqlParameter("titleParam", username);
                NpgsqlParameter passwordParam = new NpgsqlParameter("passwordParam", RequestQuery.Protect(password));

                Employee? employee = ExecuteQueryAsync(RequestQuery.LOGIN_EMPLOYEE, usernameParam, passwordParam);
                if (employee != null)
                {
                    return employee;
                }
                else
                {
                    return ControlWindow.ShowDialog("Wrong Password", "Username and Password not Match.", Icons.ERROR);
                }
            }
            catch (NpgsqlException ex)
            {
                return ControlWindow.ShowDialog("TRY AGAIN", "Exception Occurred: \n" + ex.Message, Icons.ERROR);
            }
        }

        public static List<T> GetData<T>(string tableName, string selectQuery, string whereQuery, params NpgsqlParameter[] parameters)
        {
            Type type = typeof(T);
            ConstructorInfo? constructor = type.GetConstructor(Type.EmptyTypes);
            List<T> results = new List<T>();
            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                {
                    connection.Open();

                    string query = $"SELECT {selectQuery} FROM {tableName} WHERE {whereQuery}";

                    NpgsqlCommand command = new NpgsqlCommand(query, connection);
                    command.Parameters.AddRange(parameters);

                    using (NpgsqlDataReader reader = command.ExecuteReader())
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
                            results.Add(default(T)!);
                        }
                    }

                    return results;
                }
            }
            catch (NpgsqlException ex)
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
                        results.Add(default(T)!);
                    }
                }

                return results;
            }
        }

        private static T ReadData<T>(NpgsqlDataReader reader)
        {
            Type type = typeof(T);
            ConstructorInfo? readerConstructor = type.GetConstructor(new[] { typeof(NpgsqlDataReader) });
            ConstructorInfo? emptyConstructor = type.GetConstructor(Type.EmptyTypes);

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

            return default(T)!;
        }

        public static T GetValueOrDefault<T>(NpgsqlDataReader reader, string columnName)
        {
            try
            {
                int ordinal = reader.GetOrdinal(columnName);
                return reader.IsDBNull(ordinal) ? default(T)! : reader.GetFieldValue<T>(ordinal);
            }
            catch (Exception ex)
            {
                EventLogger.Post($"ERR :: Reader<{typeof(T)}>(columnName) {ex.Message}");
                return default(T)!;
            }
        }
    }
}
