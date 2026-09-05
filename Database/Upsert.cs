using System;
using System.Collections.Generic;
using System.Linq;
using Npgsql;
using SPTC_APPLICATION.Objects;

namespace SPTC_APPLICATION.Database
{
    //USAGE: 
    /*
     new Create() for every new object with connection to database
    Insert(fieldname, value) for inserting new values, use the Field static variables
    Save() always to save the progress to database
     */
    public class Upsert
    {
        private readonly string tableName;
        public int id;
        private Dictionary<string, object> fieldValues;

        public Upsert(string tableName, int id)
        {
            this.tableName = tableName;
            this.id = id;

            if (id == -1)
            {
                fieldValues = new Dictionary<string, object>();
                //EventLogger.Post($"DTB :: Attempt to create new Information at ({tableName})");
            }
            else
            {
                fieldValues = LoadDataFromDatabase();
                //EventLogger.Post($"DTB :: Attempt to access ({tableName}) pk: {id}");
            }

        }

        public void Insert(string fieldName, object value)
        {
            fieldValues[fieldName] = value;
        }

        public object Access(string fieldName)
        {
            if (fieldValues.ContainsKey(fieldName))
            {
                return fieldValues[fieldName];
            }

            return null;
        }

        public void Save()
        {
            if (id == -1)
            {
                InsertDataToDatabase();

            }
            else
            {
                UpdateDataInDatabase();

            }
        }

        private Dictionary<string, object> LoadDataFromDatabase()
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                {
                    connection.Open();
                    string query = $"SELECT * FROM {tableName} WHERE id = @id";
                    NpgsqlCommand command = new NpgsqlCommand(query, connection);
                    command.Parameters.AddWithValue("@id", id);

                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                string fieldName = reader.GetName(i);
                                object fieldValue = reader.GetValue(i);
                                result[fieldName] = fieldValue;
                            }
                        }
                    }
                }
            }
            catch (NpgsqlException ex)
            {
                EventLogger.Post($"DTB :: Exception : {ex.Message}");
            }

            return result;
        }

        private void InsertDataToDatabase()
        {
            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                {
                    connection.Open();
                    string columns = string.Join(", ", fieldValues.Keys);
                    string values = string.Join(", ", fieldValues.Keys.Select(key => "@" + key));

                    // PostgreSQL: Use RETURNING id to get the auto-generated primary key
                    string query = $"INSERT INTO {tableName} ({columns}) VALUES ({values}) RETURNING id";
                    NpgsqlCommand command = new NpgsqlCommand(query, connection);

                    foreach (var entry in fieldValues)
                    {
                        command.Parameters.AddWithValue("@" + entry.Key, entry.Value ?? DBNull.Value);
                    }

                    try
                    {
                        // ExecuteScalar returns the RETURNING id value
                        object? result = command.ExecuteScalar();
                        id = (result != null && result != DBNull.Value) ? Convert.ToInt32(result) : -1;
                        EventLogger.Post($"DTB :: Insert into ({tableName}) pk: {id}");
                    }
                    catch (NpgsqlException ex)
                    {
                        // PostgreSQL unique violation error code: 23505
                        if (ex.SqlState == "23505")
                        {
                            EventLogger.Post($"DTB :: Insert Exception: Existing Duplicate: {ex.Message}");
                            if (tableName == Table.NAME)
                            {
                                id = GetExistingRecordId(fieldValues);
                            }
                            else if (tableName == Table.ADDRESS)
                            {
                                Dictionary<string, object> uniqueAttributes = new Dictionary<string, object>
                                {
                                    { Field.ADDRESSLINE1, fieldValues[Field.ADDRESSLINE1] },
                                    { Field.ADDRESSLINE2, fieldValues[Field.ADDRESSLINE2] }
                                };

                                id = GetExistingRecordId(uniqueAttributes);
                            }
                            else
                            {
                                id = GetExistingRecordId();
                            }
                        }
                        else
                        {
                            EventLogger.Post($"DTB :: Insert Exception: {ex.Message}");
                        }
                    }
                }
            }
            catch (NpgsqlException ex)
            {
                EventLogger.Post($"DTB :: Insert Main Exception : {ex.Message}");
            }
        }

        private void UpdateDataInDatabase()
        {
            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                {
                    connection.Open();

                    string setValues = string.Join(", ", fieldValues.Keys.Select(key => $"{key} = @{key}"));

                    string query = $"UPDATE {tableName} SET {setValues} WHERE id=@idkey";
                    NpgsqlCommand command = new NpgsqlCommand(query, connection);
                    command.Parameters.AddWithValue("@idkey", id);

                    foreach (var entry in fieldValues)
                    {
                        command.Parameters.AddWithValue("@" + entry.Key, entry.Value ?? DBNull.Value);
                    }

                    command.ExecuteNonQuery();
                    EventLogger.Post($"DTB :: Update ({tableName}) pk: {id}");
                }
            }
            catch (NpgsqlException ex)
            {
                EventLogger.Post($"DTB :: Update Main Exception : {ex.Message}");
            }
        }

        private int GetExistingRecordId(Dictionary<string, object> uniqueAttributes = null)
        {
            try
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                {
                    connection.Open();
                    NpgsqlCommand command;
                    if (uniqueAttributes != null && uniqueAttributes.Count > 0)
                    {
                        // Handle tables with unique sets
                        string whereClause = string.Join(" AND ", uniqueAttributes.Keys.Select(key => $"{key} = @{key}"));
                        string query = $"SELECT id FROM {tableName} WHERE {whereClause}";
                        command = new NpgsqlCommand(query, connection);

                        // Add parameters for each field in the unique set
                        foreach (var kvp in uniqueAttributes)
                        {
                            command.Parameters.AddWithValue($"@{kvp.Key}", kvp.Value ?? DBNull.Value);
                        }
                    }
                    else
                    {
                        // Handle tables without unique sets
                        string uniqueAttribute = GetUniqueAttributeName();
                        string query = $"SELECT id FROM {tableName} WHERE {uniqueAttribute} = @{uniqueAttribute}";
                        command = new NpgsqlCommand(query, connection);
                        command.Parameters.AddWithValue($"@{uniqueAttribute}", fieldValues[uniqueAttribute] ?? DBNull.Value);
                    }

                    object result = command.ExecuteScalar();

                    return (result != null && result != DBNull.Value) ? Convert.ToInt32(result) : -1;
                }
            }
            catch (NpgsqlException ex)
            {
                EventLogger.Post($"DTB :: Existing Record Exception : {ex.Message}");
            }
            return -1;
        }

        private string GetUniqueAttributeName()
        {
            if (tableName == Table.FRANCHISE)
            {
                return Field.BODY_NUMBER;
            }

            return fieldValues.Keys.FirstOrDefault();
        }
    }
}
