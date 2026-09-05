using Npgsql;

namespace SPTC_APPLICATION.Database
{
    public class Clean
    {
        // PostgreSQL uses "DELETE FROM table WHERE condition" (no asterisk)
        private string CLEANER = "DELETE FROM ";

        public Clean(string table)
        {
            CLEANER += table + " WHERE " + Where.ALL_DELETED;
        }

        public bool Start()
        {
            if (AppState.IS_ADMIN)
            {
                using (NpgsqlConnection connection = DatabaseConnection.GetConnection())
                {
                    connection.Open();
                    NpgsqlCommand command = new NpgsqlCommand(CLEANER, connection);
                    command.ExecuteNonQuery();
                }
                return true;
            }
            return false;
        }
    }
}
