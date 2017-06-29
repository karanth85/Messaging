namespace DataAccess
{
    using System;
    using System.Configuration;
    using System.Data.SqlClient;
    using Model;

    public static class MessageDataStoreDao
    {
        private const string TableName = "tblMessageStore";
        //private static string ConnectionString = ConfigurationManager.ConnectionStrings["MessagContext"].ConnectionString;
        
        public static long InsertRecord(Message message)
        {
            long insertedId = 0;

            var query = string.Format("INSERT INTO {0} (Topic,Source,Content,Created,Received) VALUES(@Topic, @Source, @Content, @Created, @Received);SELECT SCOPE_IDENTITY()", TableName);

            var connectionString = ConfigurationManager.ConnectionStrings["MessageContext"].ConnectionString;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                var cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@Topic",  (object) message.Topic ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Source", message.Source);
                cmd.Parameters.AddWithValue("@Content", message.Content);
                cmd.Parameters.AddWithValue("@Created", message.Created);
                cmd.Parameters.AddWithValue("@Received", message.Received);

                try
                {
                    connection.Open();
                    connection.CreateCommand();
                    insertedId = (long)cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    connection.Close();
                }
            }

            return insertedId;
        }
    }
}
