using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    public class DbMethods : DbConnection
    {
      public int GetClientElo(string username)
       {
            string query = "SELECT elo FROM user u INNER JOIN ranking r ON r.user_id = u.id " + String.Format("WHERE u.login = '{0}'", username);

            //Create Command
            MySqlCommand cmd = new MySqlCommand(query, connection);
            //Create a data reader and Execute the command
            MySqlDataReader dataReader = cmd.ExecuteReader();

            try
            {
                dataReader.Read();
                string elo = dataReader.GetString(0);
                dataReader.Close();
                return Int32.Parse(elo);
            }
            catch
            {
                throw new Exception("Nie ma takiego uzytkownika!");
            }
        }

        public void CloseConnection()
        {
            connection.Close();
        }
    }


}
