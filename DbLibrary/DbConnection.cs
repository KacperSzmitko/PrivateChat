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
            server = "192.168.1.250";
            database = "projekt_pt";
            uid = "user_io";
            password = "DN8OHj8mUkNmXBRm";
            string connectionString;
            connectionString = "SERVER=" + server + ";" + "DATABASE=" +
            database + ";" + "UID=" + uid + ";" + "PASSWORD=" + password + ";";

            connection = new MySqlConnection(connectionString);
            connection.Open();
        }

    }
}
