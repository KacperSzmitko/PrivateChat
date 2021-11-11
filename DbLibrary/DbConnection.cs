using MySql.Data.MySqlClient;

namespace DbLibrary
{
    public abstract class DbConnection
    {
        protected MySqlConnection connection;
        protected string server;
        protected string database;
        protected string uid;
        protected string password;
        public DbConnection()
        {
            server = "37.187.107.7";
            database = "PUT_INF_OD";
            uid = "PUT_INF_OD";
            password = "kjXZefuStZBj9tHG";
            string connectionString;
            connectionString = "SERVER=" + server + ";" + "DATABASE=" +
            database + ";" + "UID=" + uid + ";" + "PASSWORD=" + password + ";";

            connection = new MySqlConnection(connectionString);
            connection.Open();
        }

    }
}
