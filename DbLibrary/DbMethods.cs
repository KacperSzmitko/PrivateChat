using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Shared;
using System;
using System.Collections.Generic;
using System.Text;


namespace DbLibrary
{
    public class DbMethods : DbConnection
    {

        public bool SetUserConversationKey(int userID, string key, int conversationID)
        {
            string userId = userID.ToString();
            string conversationId = conversationID.ToString();

            string query = string.Format("UPDATE conversations SET user1_encrypted_key = '{0}' WHERE " +
                "conversation_id = '{1}' AND user1_id = '{2}'", key,conversationId,userId);
            MySqlCommand cmd = new MySqlCommand(query, connection);
            MySqlDataReader dataReader = cmd.ExecuteReader();
            dataReader.Read();
            dataReader.Close();

            query = string.Format("UPDATE conversations SET user2_encrypted_key = '{0}' WHERE " +
            "conversation_id = '{1}' AND user2_id = '{2}'", key, conversationId, userId);
            cmd = new MySqlCommand(query, connection);
            dataReader = cmd.ExecuteReader();
            dataReader.Read();
            dataReader.Close();
            return true;

        }

        public string AddFriends(int userAID, string usernameB)
        {
            string userAId = userAID.ToString();
            string userBId = GetUserId(usernameB).ToString();
            string query = string.Format("INSERT INTO friends(user1_id,user2_id) VALUES({0},{1})", userAId, userBId);
            MySqlCommand cmd = new MySqlCommand(query, connection);
            MySqlDataReader dataReader = cmd.ExecuteReader();
            dataReader.Read();
            dataReader.Close();
            return CreateNewConversation(userAId, usernameB);
            
        }



        public bool CheckFriends(string usernameA, string usernameB)
        {
            string userAId = GetUserId(usernameA).ToString();
            string userBId = GetUserId(usernameB).ToString();

            string query = string.Format("SELECT friend_id FROM friends WHERE (user1_id = {0} AND user2_id = {1}) OR (user1_id = {1}  AND user2_id = {0})", userAId, userBId);
            MySqlCommand cmd = new MySqlCommand(query, connection);
            MySqlDataReader dataReader = cmd.ExecuteReader();
            dataReader.Read();
            try
            {
                string Id = dataReader.GetString(0);
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                dataReader.Close();
            }

        }

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

            MySqlCommand cmd = new MySqlCommand(query, connection);
            MySqlDataReader dataReader = cmd.ExecuteReader();

            List<Friend> friends = new List<Friend>();

            try
            {
                for (int i = 0; i < 2; i++)
                {
                    while (dataReader.Read())
                    {
                        Friend friend = new Friend();
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

        // TEST
        public string CreateNewConversation(string IdA, string bUsername)
        {
            string IdB = GetUserId(bUsername).ToString();
            string query = string.Format("SELECT * FROM conversations WHERE (user1_id = '{0}' AND user2_id = '{1}') OR (user1_id = '{1}'  AND user2_id = '{0}')", IdA, IdB);
            MySqlCommand cmd = new MySqlCommand(query, connection);
            MySqlDataReader dataReader = cmd.ExecuteReader();

            // Check if conversation already exist
            try
            {
                dataReader.Read();
                dataReader.GetString(0);
                return "";
            }
            catch
            {
                ;
            }
            finally { dataReader.Close(); }


            query = string.Format("INSERT INTO conversations(user1_id,user2_id) VALUES({0},{1})", IdA, IdB);
            cmd = new MySqlCommand(query, connection);
            dataReader = cmd.ExecuteReader();
            dataReader.Close();

            query = "SELECT LAST_INSERT_ID()";
            cmd = new MySqlCommand(query, connection);
            dataReader = cmd.ExecuteReader();
            dataReader.Read();
            try
            {
                return dataReader.GetString(0);
            }
            catch
            {
                return "";
            }
            finally
            {
                dataReader.Close();
            }
        }

        public int GetConversationId(string usernameA, string usernameB)
        {
            string userAId = GetUserId(usernameA).ToString();
            string userBId = GetUserId(usernameB).ToString();

            string query = string.Format("SELECT conversation_id FROM conversations WHERE (user1_id = '{0}' AND user2_id = '{1}') OR (user1_id = '{1}'  AND user2_id = '{0}')", userAId, userBId);
            MySqlCommand cmd = new MySqlCommand(query, connection);
            MySqlDataReader dataReader = cmd.ExecuteReader();
            dataReader.Read();
            string Id = dataReader.GetString(0);
            dataReader.Close();
            return int.Parse(Id);
        }

        public string GetConversationKey(int conversationID, string username)
        {
            string conversationId = conversationID.ToString();
            string id = GetUserId(username).ToString();
            string query = string.Format("SELECT * FROM conversations WHERE conversation_id = '{0}'", conversationId);
            MySqlCommand cmd = new MySqlCommand(query, connection);
            MySqlDataReader dataReader = cmd.ExecuteReader();
            dataReader.Read();

            try
            {
                if (dataReader.GetString(1) == id)
                {
                    return dataReader.GetString(3);
                }
                else if(dataReader.GetString(2) == id)
                {
                    return dataReader.GetString(4);
                }
            }
            catch
            {
                return "";
            }
            finally
            {
                dataReader.Close();
            }
            return "";
        }

        public int GetUserId(string username)
        {
            string query = "SELECT user_id FROM users " + String.Format("WHERE username = '{0}'", username);

            MySqlCommand cmd = new MySqlCommand(query, connection);
            MySqlDataReader dataReader = cmd.ExecuteReader();

            try
            {
                dataReader.Read();
                string userId = dataReader.GetString(0);
                dataReader.Close();
                return int.Parse(userId);
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
