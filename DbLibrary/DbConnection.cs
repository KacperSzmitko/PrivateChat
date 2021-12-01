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
            System.Configuration.Configuration config = System.Configuration.ConfigurationManager.OpenExeConfiguration("Server.config");
            server = config.AppSettings.Settings["db-ip"].Value;
            database = config.AppSettings.Settings["db-name"].Value;
            uid = config.AppSettings.Settings["db-username"].Value;
            password = config.AppSettings.Settings["db-password"].Value;
            string connectionString;
            connectionString = "SERVER=" + server + ";" + "DATABASE=" +
            database + ";" + "UID=" + uid + ";" + "PASSWORD=" + password + ";";

            connection = new MySqlConnection(connectionString);
            connection.Open();
        }

    }
}
