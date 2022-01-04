using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Shared;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;


namespace DbLibrary
{
    public class DbMethods : DbConnection
    {

        public bool SetUserConversationKey(int userID, string key, int conversationID)
        {
            string userId = userID.ToString();
            string conversationId = conversationID.ToString();

            string query = string.Format("UPDATE conversations SET user1_encrypted_conversation_key = '{0}' WHERE " +
                "conversation_id = '{1}' AND user1_id = '{2}'", key,conversationId,userId);
            MySqlCommand cmd = new MySqlCommand(query, connection);
            MySqlDataReader dataReader = cmd.ExecuteReader();
            dataReader.Read();
            dataReader.Close();

            query = string.Format("UPDATE conversations SET user2_encrypted_conversation_key = '{0}' WHERE " +
            "conversation_id = '{1}' AND user2_id = '{2}'", key, conversationId, userId);
            cmd = new MySqlCommand(query, connection);
            dataReader = cmd.ExecuteReader();
            dataReader.Read();
            dataReader.Close();
            return true;

        }

        public int GetSecondUserId(int conversationId,int userId)
        {
            string query = string.Format("SELECT user1_id,user2_id FROM conversations WHERE  conversation_id = {0}", conversationId);
            MySqlCommand cmd = new MySqlCommand(query, connection);
            MySqlDataReader dataReader = cmd.ExecuteReader();

            // Check if conversation already exist
            try
            {
                dataReader.Read();
                int secondUserId = dataReader.GetInt32("user1_id");
                if (secondUserId != userId) return secondUserId;
                return dataReader.GetInt32("user2_id");
            }
            catch
            {
                return 0;
                ;
            }
            finally
            {
                dataReader.Close();
            }

        }
        public string AddFriends(int userAID, string usernameB, string iv)
        {
            string userAId = userAID.ToString();
            string userBId = GetUserId(usernameB).ToString();
            string query = string.Format("INSERT INTO friends(user1_id,user2_id) VALUES({0},{1})", userAId, userBId);
            MySqlCommand cmd = new MySqlCommand(query, connection);
            MySqlDataReader dataReader = cmd.ExecuteReader();
            dataReader.Read();
            dataReader.Close();
            return CreateNewConversation(userAId, usernameB,iv);
        }

        public String AddAttachment(int conversationID, int userID, string nameOri, string nameServer)
        {
            string userId = userID.ToString();
            string conversationId = conversationID.ToString();
            string query = string.Format("INSERT INTO attachments(conversation_id,sent_by_user,attachment_ori_name,attachment_server_name) VALUES({0},{1},\"{2}\",\"{3}\")", conversationId, userId, nameOri, nameServer);
            MySqlCommand cmd = new MySqlCommand(query, connection);
            MySqlDataReader dataReader = cmd.ExecuteReader();
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


        public bool AddNewUser(string username, string password, string IV,string keyHash)
        {
            string query = String.Format("INSERT INTO users(username,password_hash,iv_to_decrypt_user_key,user_key_hash) VALUES('{0}','{1}','{2}','{3}')", username, password,IV, keyHash);
            //Create Command
            MySqlCommand cmd = new MySqlCommand(query, connection);
            //Create a data reader and Execute the command
            MySqlDataReader dataReader = cmd.ExecuteReader();
            dataReader.Close();
            return true;
        }

        public string GetFromUser(string fieldName, string username)
        {
            string query = String.Format("SELECT {0} FROM users WHERE username = '{1}'",fieldName, username);
            //Create Command
            MySqlCommand cmd = new MySqlCommand(query, connection);
            //Create a data reader and Execute the command
            MySqlDataReader dataReader = cmd.ExecuteReader();

            try
            {
                dataReader.Read();
                string password = dataReader.GetString(0);
                return password;
            }
            catch
            {
                throw new Exception("Nie ma takiego uzytkownika!");
            }
            finally {
                dataReader.Close();
            }
        }

        public Dictionary<int,ExtendedInvitation> GetInvitations()
        {
            string query = String.Format("SELECT * FROM invitations_view");
            //Create Command
            MySqlCommand cmd = new MySqlCommand(query, connection);
            //Create a data reader and Execute the command
            MySqlDataReader dataReader = cmd.ExecuteReader();
            Dictionary<int, ExtendedInvitation> result = new Dictionary<int, ExtendedInvitation>();
            while(dataReader.Read())
            {
                ExtendedInvitation ei = new ExtendedInvitation();
                ei.invitationId = dataReader.GetInt32("invitation_id");
                ei.sender = dataReader.GetString("sender");
                ei.reciver = dataReader.GetString("reciver");
                ei.p = dataReader.GetString("p");
                ei.g = dataReader.GetString("g");
                ei.sended = dataReader.GetBoolean("sended");
                ei.accepted = dataReader.GetBoolean("accepted");
                ei.encryptedSenderPrivateKey = dataReader.GetString("sender_encypted_private_dh_key"); 
                ei.ivToDecryptSenderPrivateKey = dataReader.GetString("sender_iv_to_decrypt_private_dh_key");
                ei.publicKeySender = dataReader.GetString("sender_public_dh_key");

                if (ei.accepted)
                {
                    ei.publicKeyReciver = dataReader.GetString("reciver_public_dh_key");
                }

                result[ei.invitationId] = ei;
                }
            dataReader.Close();
            return result;
        }

        public int CreateNewInvitation(string senderName,string reciverName,string p, string g)
        {
            int senderId = GetUserId(senderName);
            int reciverId = GetUserId(reciverName);
            string query = String.Format("INSERT INTO invitations(sender,reciver,p,g) VALUES('{0}','{1}','{2}','{3}')", senderId, reciverId,p,g);
            //Create Command
            MySqlCommand cmd = new MySqlCommand(query, connection);
            //Create a data reader and Execute the command
            MySqlDataReader dataReader = cmd.ExecuteReader();

            try
            {
                dataReader.Read();
                dataReader.Close();
            }
            catch
            {
                return -1;
            }
            query = "SELECT LAST_INSERT_ID()";
            cmd = new MySqlCommand(query, connection);
            dataReader = cmd.ExecuteReader();
            dataReader.Read();
            try
            {
                return dataReader.GetInt32(0);
            }
            catch
            {
                return -1;
            }
            finally
            {
                dataReader.Close();
            }
        }

        public bool InsertDHKeysToInvitation(int invitationId, string publicKeySender, string encryptedPrivateKeySender, string ivToDecryptPrivateKeySender)
        {
            string query = String.Format
                ("UPDATE invitations SET sender_public_dh_key = '{0}',sender_encypted_private_dh_key = '{1}',sender_iv_to_decrypt_private_dh_key = '{2}'" +
                " WHERE invitation_id = {3}",
                publicKeySender, encryptedPrivateKeySender, ivToDecryptPrivateKeySender, invitationId);
            //Create Command
            MySqlCommand cmd = new MySqlCommand(query, connection);
            //Create a data reader and Execute the command
            MySqlDataReader dataReader = cmd.ExecuteReader();
            try
            {
                dataReader.Read();
                dataReader.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        public bool InsertDHPublicReciverKey(int invitationId,string reciverPublicKey)
        {
            string query = String.Format
            ("UPDATE invitations SET reciver_public_dh_key = '{0}'" +
            " WHERE invitation_id = {1}",
            reciverPublicKey,  invitationId);
            //Create Command
            MySqlCommand cmd = new MySqlCommand(query, connection);
            //Create a data reader and Execute the command
            MySqlDataReader dataReader = cmd.ExecuteReader();
            try
            {
                dataReader.Read();
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
        
        public bool DeleteInvitation(int invitationId)
        {
            string query = String.Format("DELETE FROM invitations WHERE invitation_id = {0}", invitationId);

            //Create Command
            MySqlCommand cmd = new MySqlCommand(query, connection);
            //Create a data reader and Execute the command
            MySqlDataReader dataReader = cmd.ExecuteReader();
            try
            {
                dataReader.Read();
                dataReader.Close();
                return true;
            }
            catch
            {
                return false;
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


        public string CreateNewConversation(string IdA, string bUsername, string iv)
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
            dataReader.Close();


            query = string.Format("INSERT INTO conversations(user1_id,user2_id,iv_to_decrypt_converstion_key) VALUES({0},{1},'{2}')", IdA, IdB, iv);
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

        public string GetConversationIv(int conversationID)
        {
            string conversationId = conversationID.ToString();
            string query = string.Format("SELECT iv_to_decrypt_converstion_key FROM conversations WHERE conversation_id = '{0}'", conversationId);
            MySqlCommand cmd = new MySqlCommand(query, connection);
            MySqlDataReader dataReader = cmd.ExecuteReader();
            dataReader.Read();

            try
            {
                return dataReader.GetString("iv_to_decrypt_converstion_key");
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

        public int GetUserId(string username)
        {
            string query = "SELECT user_id FROM users " + String.Format("WHERE username = '{0}'", username);

            MySqlCommand cmd = new MySqlCommand(query, connection);
            MySqlDataReader dataReader = cmd.ExecuteReader();

            try
            {
                dataReader.Read();
                int userId = dataReader.GetInt32(0);
                dataReader.Close();
                return userId;
            }
            catch
            {
                throw new Exception("Nie ma takiego uzytkownika!");
            }
            finally
            {
                dataReader.Close();
            }
        }



        public int GetInvitationId(string senderUserName, string reciverUserName)
        {
            string query = String.Format("SELECT invitation_id i FROM invitations JOIN users u ON u.user_id = i.sender JOIN users u2 ON u2.user_id = i.reciver WHERE u.username = {0} AND u2.username = {1}"
                , senderUserName, reciverUserName);

            MySqlCommand cmd = new MySqlCommand(query, connection);
            MySqlDataReader dataReader = cmd.ExecuteReader();

            try
            {
                dataReader.Read();
                int invitationId = dataReader.GetInt32(0);
                dataReader.Close();
                return invitationId;
            }
            catch
            {
                throw new Exception("Nie ma takiego uzytkownika!");
            }
        }

        public List<int> GetAllUserConversations(int userId)
        {
            List<int> res = new List<int>();
            string query = String.Format("SELECT conversation_id FROM conversations WHERE user1_id = {0} OR user2_id = {0}", userId);
            MySqlCommand cmd = new MySqlCommand(query, connection);
            MySqlDataReader dataReader = cmd.ExecuteReader();

            while(dataReader.Read())
            {
                res.Add(dataReader.GetInt32(0));
            }
            dataReader.Close();
            return res;
        }

        public bool DeleteUser(int userId)
        {
            string query = String.Format("DELETE FROM users WHERE user_id = {0}", userId);
            MySqlCommand cmd = new MySqlCommand(query, connection);
            MySqlDataReader dataReader = cmd.ExecuteReader();
            try
            {
                dataReader.Read();
                dataReader.Close();
            }
            catch
            {
                return false;
            }
            finally
            {
                dataReader.Close();
            }
            return true;
        }

        public void CloseConnection()
        {
            connection.Close();
        }
    }


}
