using MySql.Data.MySqlClient;
using System.Configuration;
using System.Collections.Specialized;

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
            server = ConfigurationManager.AppSettings.Get("db-ip");
            database = ConfigurationManager.AppSettings.Get("db-name");
            uid = ConfigurationManager.AppSettings.Get("db-username");
            password = ConfigurationManager.AppSettings.Get("db-password");
            string connectionString;
            connectionString = "SERVER=" + server + ";" + "DATABASE=" +
            database + ";" + "UID=" + uid + ";" + "PASSWORD=" + password + ";";

            connection = new MySqlConnection(connectionString);
            connection.Open();
        }

    }
}
