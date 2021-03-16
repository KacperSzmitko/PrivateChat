using DbLibrary;
using Newtonsoft.Json;
using Shared;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Server
{
    /// <summary>
    /// Class which process all client messages sended here from ServerConnection,
    /// and gives back server response ready to send to client
    /// </summary>
    public class ClientProcessing
    {
        /// <summary>
        /// Delegate which represents function used to procces client data
        /// </summary>
        public delegate string Functions(string msg, int clientId);

        /// <summary>
        /// List of all avaliable functions, index of given function is number of that option
        /// </summary>
        public List<Functions> functions { get; set; }
        public List<User> activeUsers { get; set; }
        public Security security { get; set; }

        public List<ExtendedInvitation> invitations { get; set; }

        //D(recvUserId:Dict(convId:List<message>))
        public Dictionary<int,Dictionary<int,List<Message>>> messagesToSend { get; set; }

        // D(recvUserId:Dict(convId:notification))
        public Dictionary<int, Dictionary<int, Notification>> notifications { get; set; }
        // userId:convId
        public Dictionary<int,int> activeConversations { get; set; }


        /// <summary>
        /// Function that takes message from client procces it and return server response
        /// </summary>
        /// <param name="message"> Client message</param>
        /// <returns>Server response ready to send</returns>
        public string ProccesClient(string message, int clientId)
        {
            string[] fields = message.Split("$$", StringSplitOptions.RemoveEmptyEntries);
            int option = Int32.Parse(fields[0].Split(':', StringSplitOptions.RemoveEmptyEntries)[1]);

            //Remove option
            var list = new List<string>(fields);
            list.RemoveAt(0);

            lock (functions[option])
            {
                return functions[option](string.Join("$$", list), clientId);
            }
        }

        public string Logout(string msg,int clientId)
        {
            lock (activeUsers[clientId])
                activeUsers[clientId].logged = false;
            return TransmisionProtocol.CreateServerMessage(ErrorCodes.NO_ERROR,Options.LOGOUT);
        }

        public string Login(string msg, int clientId)
        {
            string[] fields = msg.Split("$$", StringSplitOptions.RemoveEmptyEntries);
            string username = fields[0].Split(':', StringSplitOptions.RemoveEmptyEntries)[1];
            string password = fields[1].Split(':', StringSplitOptions.RemoveEmptyEntries)[1];

            string passwordHash = "";
            DbMethods dbConnection = new DbMethods();
            lock (activeUsers[clientId]) { dbConnection = activeUsers[clientId].dbConnection; }
            try { passwordHash = dbConnection.GetUserPasswordHash(username); }
            catch { return TransmisionProtocol.CreateServerMessage(ErrorCodes.USER_NOT_FOUND, Options.LOGIN); }

            if (security.VerifyPassword(passwordHash, password))
            {
                lock (activeUsers)
                {
                    foreach (User u in activeUsers)
                    {
                        if (u != null)
                        {
                            if (u.name == username && u.logged)
                            {
                                return TransmisionProtocol.CreateServerMessage(ErrorCodes.USER_ALREADY_LOGGED_IN, Options.LOGIN);
                            }
                        }
                    }
                    activeUsers[clientId].logged = true;
                    activeUsers[clientId].name = username;
                    activeUsers[clientId].userId = dbConnection.GetUserId(username);

                }
                return TransmisionProtocol.CreateServerMessage(ErrorCodes.NO_ERROR, Options.LOGIN);
            }
            else return TransmisionProtocol.CreateServerMessage(ErrorCodes.INCORRECT_PASSWORD, Options.LOGIN);
        }

        public string CreateUser(string msg, int clientId)
        {
            string[] fields = msg.Split("$$", StringSplitOptions.RemoveEmptyEntries);
            string username = fields[0].Split(':', StringSplitOptions.RemoveEmptyEntries)[1];
            string password = fields[1].Split(':', StringSplitOptions.RemoveEmptyEntries)[1];
            DbMethods dbConnection = new DbMethods();
            lock (activeUsers[clientId]) { dbConnection = activeUsers[clientId].dbConnection; }
            if (dbConnection.CheckIfNameExist(username))
                return TransmisionProtocol.CreateServerMessage(ErrorCodes.USER_ALREADY_EXISTS, Options.CREATE_USER);

            password = security.HashPassword(password);
            if (dbConnection.AddNewUser(username, password)) return TransmisionProtocol.CreateServerMessage(ErrorCodes.NO_ERROR, Options.CREATE_USER);
            else return TransmisionProtocol.CreateServerMessage(ErrorCodes.NO_ERROR, Options.CREATE_USER);
        }

        public string CheckUserName(string msg, int clientId)
        {
            string[] fields = msg.Split("$$");
            string username = fields[0].Split(':')[1];
            DbMethods dbConnection = new DbMethods();
            lock (activeUsers[clientId]) { dbConnection = activeUsers[clientId].dbConnection; }
            if (!dbConnection.CheckIfNameExist(username))
                return TransmisionProtocol.CreateServerMessage(ErrorCodes.NO_ERROR, Options.CHECK_USER_NAME);
            return TransmisionProtocol.CreateServerMessage(ErrorCodes.USER_ALREADY_EXISTS, Options.CHECK_USER_NAME);
        }

        public string Disconnect(string msg, int clientId)
        {
            lock (activeConversations)
            {
                if (activeConversations.ContainsKey(activeUsers[clientId].userId)) activeConversations.Remove(activeUsers[clientId].userId);
            }
            if (DeleteActiveUser(clientId))
                return "";
            return TransmisionProtocol.CreateServerMessage(ErrorCodes.DISCONNECT_ERROR, Options.DISCONNECT);
        }

        public string GetFriends(string msg, int clientId)
        {
            DbMethods dbConnection = new DbMethods();
            string username;
            List<string> activeUsersNames = new List<string>();

            lock (activeUsers[clientId]) 
            { 
                if (!activeUsers[clientId].logged) return TransmisionProtocol.CreateServerMessage(ErrorCodes.NOT_LOGGED_IN, Options.LOGIN);
                dbConnection = activeUsers[clientId].dbConnection;
                username = activeUsers[clientId].name;
                foreach(User user in activeUsers)
                {
                    activeUsersNames.Add(user.name);
                }
            }
            return TransmisionProtocol.CreateServerMessage(ErrorCodes.NO_ERROR,Options.GET_FRIENDS,dbConnection.GetFriends(username,activeUsersNames));
        }

        // Tested TODO Make errors
        public string GetConversation(string msg, int clientId)
        {
            string[] fields = msg.Split("$$", StringSplitOptions.RemoveEmptyEntries);
            string secondUserName = fields[0].Split(":", StringSplitOptions.RemoveEmptyEntries)[1];
            

            lock(activeUsers[clientId])
            {
                if (!activeUsers[clientId].logged) return TransmisionProtocol.CreateServerMessage(ErrorCodes.NOT_LOGGED_IN, Options.LOGIN);
                string username = activeUsers[clientId].name;
                int conversationId = activeUsers[clientId].dbConnection.GetConversationId(username, secondUserName);
                string conversation = activeUsers[clientId].redis.GetConversation(conversationId);
                string conversationKey = activeUsers[clientId].dbConnection.GetConversationKey(conversationId, username);
                return TransmisionProtocol.CreateServerMessage(ErrorCodes.NO_ERROR,Options.GET_CONVERSATION,conversationKey,conversationId.ToString(),conversation);
            }

        }



        // Tested 
        public string AddFriend(string msg, int clientId)
        {
            lock(activeUsers[clientId])
            {
                if(!activeUsers[clientId].logged) return TransmisionProtocol.CreateServerMessage(ErrorCodes.NOT_LOGGED_IN, Options.LOGIN);
            }
            string[] fields = msg.Split("$$", StringSplitOptions.RemoveEmptyEntries);
            string userName = fields[0].Split(":", StringSplitOptions.RemoveEmptyEntries)[1];
            
            ExtendedInvitation ei = new ExtendedInvitation();

            // Check if we arent already friends
            lock (activeUsers[clientId])
            {
                if (activeUsers[clientId].dbConnection.CheckFriends(activeUsers[clientId].name, userName)) return TransmisionProtocol.CreateServerMessage(
                      ErrorCodes.ALREADY_FRIENDS, Options.LOGIN);
                ei.sender = activeUsers[clientId].name;
            }

            var param = security.GenerateParameters();
            ei.g = security.GetG(param); 
            ei.p = security.GetP(param);

            ei.reciver = userName;

            lock(invitations)
            {
                ei.invitationId = AddFriend(ei);
            }
            return TransmisionProtocol.CreateServerMessage(ErrorCodes.NO_ERROR, Options.ADD_FRIEND, ei.g, ei.p, ei.invitationId.ToString());
        }

        // Errors
        public string DhExchange(string msg, int clientId)
        {
            string[] fields = msg.Split("$$", StringSplitOptions.RemoveEmptyEntries);
            int invId = Int32.Parse(fields[0].Split(":", StringSplitOptions.RemoveEmptyEntries)[1]);
            string pk = fields[1].Split(":", StringSplitOptions.RemoveEmptyEntries)[1];

            try
            {
                lock (invitations[invId])
                {
                    invitations[invId].publicKeySender = pk;
                    return TransmisionProtocol.CreateServerMessage(ErrorCodes.NO_ERROR, Options.DH_EXCHANGE);
                }
            }
            catch
            {
                return TransmisionProtocol.CreateServerMessage(ErrorCodes.DH_EXCHANGE_ERROR, Options.DH_EXCHANGE);
            }

        }

        // Errors
        public string DeclineFriend(string msg, int clientId)
        {
            string[] fields = msg.Split("$$", StringSplitOptions.RemoveEmptyEntries);
            int invId = Int32.Parse(fields[0].Split(":", StringSplitOptions.RemoveEmptyEntries)[1]);
            try
            {
                lock (invitations)
                {
                    invitations[invId] = null;
                }
            }
            catch
            {
                return TransmisionProtocol.CreateServerMessage(ErrorCodes.DECLINE_FRIEND_ERROR, Options.DECLINE_FRIEND);
            }

            return TransmisionProtocol.CreateServerMessage(ErrorCodes.NO_ERROR, Options.DECLINE_FRIEND);
        }

        // TEST
        public string AcceptFriend(string msg, int clientId)
        {
            string[] fields = msg.Split("$$", StringSplitOptions.RemoveEmptyEntries);
            int invId = Int32.Parse(fields[0].Split(":", StringSplitOptions.RemoveEmptyEntries)[1]);
            string reciverPk = fields[1].Split(":", StringSplitOptions.RemoveEmptyEntries)[1];

            string conversationId = "";
            lock (activeUsers[clientId])
            {
                lock (invitations)
                {
                    try
                    {
                        conversationId = activeUsers[clientId].dbConnection.AddFriends(activeUsers[clientId].userId, invitations[invId].sender);
                    if(conversationId == "") return TransmisionProtocol.CreateServerMessage(ErrorCodes.ADDING_FRIENDS_ERROR, Options.LOGIN);


                        invitations[invId].publicKeyReciver = reciverPk;
                        invitations[invId].accepted = true;
                        invitations[invId].conversationId = conversationId;
                    }
                    catch
                    {
                        return TransmisionProtocol.CreateServerMessage(ErrorCodes.WRONG_INVATATION_ID, Options.LOGIN);
                    }
                }
            }

            return TransmisionProtocol.CreateServerMessage(ErrorCodes.NO_ERROR, Options.ACCEPT_FRIEND,conversationId);
        }

        // Errors
        public string SendConversationKey(string msg, int clientId)
        {
            string[] fields = msg.Split("$$", StringSplitOptions.RemoveEmptyEntries);
            string conversationId = fields[0].Split(":", StringSplitOptions.RemoveEmptyEntries)[1];
            string conversationKey = fields[1].Split(":", StringSplitOptions.RemoveEmptyEntries)[1];

 
            lock(activeUsers[clientId])
            {
                activeUsers[clientId].dbConnection.SetUserConversationKey(activeUsers[clientId].userId, conversationKey, int.Parse(conversationId));
            }
            return TransmisionProtocol.CreateServerMessage(ErrorCodes.NO_ERROR, Options.SEND_CONVERSATION_KEY);
        }


        public string SendInvitation(string msg, int clientId)
        {
            string username;
            lock(activeUsers[clientId])
            {
                if (!activeUsers[clientId].logged) return TransmisionProtocol.CreateServerMessage(ErrorCodes.NOTHING_TO_SEND, Options.LOGIN);
                username = activeUsers[clientId].name;
            }
            List<Invitation> invs = new List<Invitation>();

            lock(invitations)
            {
                for(int i=0;i<invitations.Count; i++)
                {
                    if(invitations[i].reciver == username && !invitations[i].sended)
                    {
                        invs.Add(invitations[i]);
                        invitations[i].sended = true;
                    }
                }
            }
            if (invs.Count > 0)
            {
                return TransmisionProtocol.CreateServerMessage(ErrorCodes.NO_ERROR, Options.SEND_INVITATION, JsonConvert.SerializeObject(invs));
            }
            else return TransmisionProtocol.CreateServerMessage(ErrorCodes.NOTHING_TO_SEND, Options.LOGIN);
        }

        public string AcceptedFriend(string msg, int clientId)
        {
            string username;
            if (!activeUsers[clientId].logged) return TransmisionProtocol.CreateServerMessage(ErrorCodes.NOTHING_TO_SEND, Options.LOGIN);
            username = activeUsers[clientId].name;
            lock(invitations)
            {
                for(int i=0;i<invitations.Count;i++)
                {
                    if(invitations[i] != null && invitations[i].sender == username && invitations[i].accepted)
                    {
                        int invId = invitations[i].invitationId;
                        string pk = invitations[i].publicKeyReciver;
                        string conversationId = invitations[i].conversationId;
                        invitations[i] = null;
                        return TransmisionProtocol.CreateServerMessage(ErrorCodes.NO_ERROR, Options.ACCEPTED_FRIEND, invId.ToString(), pk, conversationId);
                    }
                }
                return TransmisionProtocol.CreateServerMessage(ErrorCodes.NOTHING_TO_SEND, Options.LOGIN);
            }
        }


        public string Notification(string msg, int clientId)
        {
            if (!activeUsers[clientId].logged) return TransmisionProtocol.CreateServerMessage(ErrorCodes.NOT_LOGGED_IN, Options.LOGIN);
            int userId = activeUsers[clientId].userId;

            lock (notifications)
            {
                try
                {
                    string notify = JsonConvert.SerializeObject(notifications[userId].Values);
                    notifications[userId].Clear();
                    return TransmisionProtocol.CreateServerMessage(ErrorCodes.NO_ERROR, Options.NOTIFICATION,notify );
                }
                catch
                {
                    return TransmisionProtocol.CreateServerMessage(ErrorCodes.NO_NOTIFICATIONS, Options.NOTIFICATION, "");
                }

            }

        }

        //TODO Backup invitations

        //TODO store parameters and send them to second user
        public string ActivateConversation(string msg, int clientId)
        {
            string[] fields = msg.Split("$$", StringSplitOptions.RemoveEmptyEntries);
            int conversationId = int.Parse(fields[0].Split(":", StringSplitOptions.RemoveEmptyEntries)[1]);
            int userId = activeUsers[clientId].userId;
            try
            {
                lock(activeConversations)
                {
                    activeConversations[userId] = conversationId;
                    activeUsers[clientId].activeConversation = conversationId;
                }
                return TransmisionProtocol.CreateServerMessage(ErrorCodes.NO_ERROR, Options.ACTIVATE_CONVERSATION);
            }
            catch
            {
                return TransmisionProtocol.CreateServerMessage(ErrorCodes.CANNOT_ACTIVATE_CONVERSATION, Options.ACTIVATE_CONVERSATION);
            }   
        }


        //TEST
        public string SendMessage(string msg, int clientId)
        {
            string[] fields = msg.Split("$$", StringSplitOptions.RemoveEmptyEntries);
            string username = fields[0].Split(":", StringSplitOptions.RemoveEmptyEntries)[1];
            string message = fields[1].Split(":", 2,StringSplitOptions.RemoveEmptyEntries)[1];
            int id = activeUsers[clientId].dbConnection.GetUserId(username);
            int conversationId = activeUsers[clientId].dbConnection.GetConversationId(username, activeUsers[clientId].name);
            activeUsers[clientId].redis.AddMessage(conversationId, message);


            lock (messagesToSend)
            {

                if (messagesToSend.ContainsKey(id))
                {
                    messagesToSend[id][conversationId].Add(JsonConvert.DeserializeObject<Message>(message));
                }
                else
                {
                    messagesToSend.Add(id, new Dictionary<int, List<Message>>()
                    {
                        [conversationId] =
                        new List<Message> { JsonConvert.DeserializeObject<Message>(message) }
                    });
                }

            }
            lock (activeConversations)
            {
                if (!activeConversations.ContainsKey(id))
                {
                    activeConversations[id] = -1;
                }
                if (!(conversationId == activeConversations[id]))
                {
                    lock (notifications)
                    {
                        if (notifications.ContainsKey(id))
                        {
                            notifications[id][conversationId].numberOfMessages += 1;
                        }
                        else
                        {
                            notifications.Add(id, new Dictionary<int, Notification>()
                            {
                                [conversationId] =
                                new Notification { numberOfMessages = 1, username = username }
                            });
                        }
                    }
                }

            }
            return TransmisionProtocol.CreateServerMessage(ErrorCodes.NO_ERROR, Options.SEND_MESSAGE);
        }

        public string NewMessages(string msg, int clientId)
        {
            int id = activeUsers[clientId].userId;
            int activeConversationId = 0;
            lock (activeConversations)
            {
                activeConversationId = activeConversations[id];
            }
            try
            {               
                lock (messagesToSend[id])
                {
                    var messages = JsonConvert.SerializeObject(messagesToSend[id][activeConversationId]);
                    messagesToSend[id][activeConversationId].Clear();
                    return TransmisionProtocol.CreateServerMessage(ErrorCodes.NO_ERROR, Options.NEW_MESSAGES,
                        messages);
                }                
            }
            catch
            {
                return TransmisionProtocol.CreateServerMessage(ErrorCodes.NO_MESSAGES, Options.SEND_MESSAGE);
            }

        }










        public int AddActiveUser()
        {
            for(int i=0;i<activeUsers.Count;i++)
            {
                if(activeUsers[i] == null)
                {
                    activeUsers[i] = new User();
                    return i;
                }
            }
            activeUsers.Add(new User());
            return activeUsers.Count - 1;
        }

        public int AddFriend(ExtendedInvitation ei)
        {
            for (int i = 0; i < invitations.Count; i++)
            {
                if (activeUsers[i] == null)
                {
                    invitations[i] = ei;
                    return i;
                }
            }
           invitations.Add(ei);
            return invitations.Count - 1;
        }
        public bool DeleteActiveUser(int clientId)
        {
            try {
                lock (activeUsers[clientId])
                {
                    activeUsers[clientId].dbConnection.CloseConnection();
                    activeUsers[clientId].redis.redis.Close();
                    activeUsers[clientId] = null;
                }
            }
            catch
            {
                return false;
            }
            return true;

        }


        public ClientProcessing()
        {
            functions = new List<Functions>();        
            functions.Add(new Functions(Logout));
            functions.Add(new Functions(Login));
            functions.Add(new Functions(CreateUser));
            functions.Add(new Functions(CheckUserName));
            functions.Add(new Functions(Disconnect));
            functions.Add(new Functions(GetFriends));
            functions.Add(new Functions(GetConversation));
            functions.Add(new Functions(ActivateConversation));
            functions.Add(new Functions(SendMessage));
            functions.Add(new Functions(NewMessages));
            functions.Add(new Functions(Notification));
            functions.Add(new Functions(AddFriend));
            functions.Add(new Functions(DhExchange));
            functions.Add(new Functions(SendInvitation));
            functions.Add(new Functions(DeclineFriend));
            functions.Add(new Functions(AcceptFriend));
            functions.Add(new Functions(SendConversationKey));
            functions.Add(new Functions(AcceptedFriend));

            security = new Security();
            activeUsers = new List<User>();
            invitations = new List<ExtendedInvitation>();
            messagesToSend = new Dictionary<int, Dictionary<int, List<Message>>>();
            notifications = new Dictionary<int, Dictionary<int, Notification>>();
            activeConversations = new Dictionary<int, int>();
        }


    }
}
