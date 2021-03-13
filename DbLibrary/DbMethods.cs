using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DbLibrary
{
    public class DbMethods : DbConnection
    {
        public bool AddNewUser(string username, string password)
        {
            string query = String.Format("INSERT INTO users(username,password_hash) VALUES('{0}','{1}')", username, password);
            //Create Command
            MySqlCommand cmd = new MySqlCommand(query, connection);
            //Create a data reader and Execute the command
            MySqlDataReader dataReader = cmd.ExecuteReader();
            dataReader.Close();
            return true;
        }

        public string GetUserPasswordHash(string username)
        {
            string query = "SELECT password_hash FROM users " + String.Format("WHERE username = '{0}'", username);
                //Create Command
                MySqlCommand cmd = new MySqlCommand(query, connection);
                //Create a data reader and Execute the command
                MySqlDataReader dataReader = cmd.ExecuteReader();

                try
                {
                    dataReader.Read();
                    string password = dataReader.GetString(0);
                    dataReader.Close();
                    return password;
                }
                catch
                {
                    throw new Exception("Nie ma takiego uzytkownika!");
                }
        }

        public bool CheckIfNameExist(string username)
        {
            string query = string.Format("SELECT username FROM users WHERE username = '{0}'", username);

            //Create Command
            MySqlCommand cmd = new MySqlCommand(query, connection);
            //Create a data reader and Execute the command
            MySqlDataReader dataReader = cmd.ExecuteReader();

            try
            {
                dataReader.Read();
                string usernameFromDatabase = dataReader.GetString(0);
            }
            catch
            {
                dataReader.Close();
                return false;
            }
            //close Data Reader
            dataReader.Close();
            return true;
        }

        public string GetFriends(string username,List<string> activeUsers)
        {
            string query = string.Format("SELECT u2.username FROM friends f JOIN users u ON u.user_id = f.user1_id " +
                "JOIN users u2 ON u2.user_id = f.user2_id  WHERE u.username LIKE '{0}' ", username);

            //Create Command
            MySqlCommand cmd = new MySqlCommand(query, connection);
            //Create a data reader and Execute the command
            MySqlDataReader dataReader = cmd.ExecuteReader();

            List<User> friends = new List<User>();

            try
            {
                for (int i = 0; i < 2; i++)
                {
                    while (dataReader.Read())
                    {
                        User friend = new User();
                        friend.username = dataReader.GetString(0);
                        if (activeUsers.Contains(friend.username)) friend.active = 1;
                        else friend.active = 0;
                        friends.Add(friend);
                    }
                    query = string.Format("SELECT u2.username FROM friends f JOIN users u ON u.user_id = f.user2_id " +
                                    "JOIN users u2 ON u2.user_id = f.user1_id  WHERE u.username LIKE '{0}' ", username);

                    dataReader.Close();
                    cmd = new MySqlCommand(query, connection);
                    dataReader = cmd.ExecuteReader();
                }
                dataReader.Close();
                return JsonConvert.SerializeObject(friends);
            }
            catch
            {
                return "";
            }

        }

        public void CloseConnection()
        {
            connection.Close();
        }
    }


}
