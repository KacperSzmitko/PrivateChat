using DbLibrary;
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
        public delegate string Functions(string msg, int clientID);

        /// <summary>
        /// List of all avaliable functions, index of given function is number of that option
        /// </summary>
        public List<Functions> functions { get; set; }
        public List<User> activeUsers { get; set; }
        public Security security { get; set; }
        /// <summary>
        /// Function that takes message from client procces it and return server response
        /// </summary>
        /// <param name="message"> Client message</param>
        /// <returns>Server response ready to send</returns>
        public string ProccesClient(string message, int clientID)
        {
            string[] fields = message.Split("$$", StringSplitOptions.RemoveEmptyEntries);
            int option = Int32.Parse(fields[0].Split(':', StringSplitOptions.RemoveEmptyEntries)[1]);

            //Remove option
            var list = new List<string>(fields);
            list.RemoveAt(0);

            lock (functions[option])
            {
                return functions[option](string.Join("$$", list), clientID);
            }
        }

        public string Logout(string msg,int clientId)
        {
            lock (activeUsers[clientId])
                activeUsers[clientId].logged = false;
            return TransmisionProtocol.CreateServerMessage((int)ErrorCodes.NO_ERROR,(int)Options.LOGOUT);
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
            catch { return TransmisionProtocol.CreateServerMessage((int)ErrorCodes.USER_NOT_FOUND, (int)Options.LOGIN); }

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
                                return TransmisionProtocol.CreateServerMessage((int)ErrorCodes.USER_ALREADY_LOGGED_IN, (int)Options.LOGIN);
                            }
                        }
                    }
                    activeUsers[clientId].name = username;
                }
                return TransmisionProtocol.CreateServerMessage((int)ErrorCodes.NO_ERROR, (int)Options.LOGIN);
            }
            else return TransmisionProtocol.CreateServerMessage((int)ErrorCodes.INCORRECT_PASSWORD, (int)Options.LOGIN);
        }

        public string CreateUser(string msg, int clientId)
        {
            string[] fields = msg.Split("$$", StringSplitOptions.RemoveEmptyEntries);
            string username = fields[0].Split(':', StringSplitOptions.RemoveEmptyEntries)[1];
            string password = fields[1].Split(':', StringSplitOptions.RemoveEmptyEntries)[1];
            DbMethods dbConnection = new DbMethods();
            lock (activeUsers[clientId]) { dbConnection = activeUsers[clientId].dbConnection; }
            if (dbConnection.CheckIfNameExist(username))
                return TransmisionProtocol.CreateServerMessage((int)ErrorCodes.USER_ALREADY_EXISTS, (int)Options.CREATE_USER);

            password = security.HashPassword(password);
            if (dbConnection.AddNewUser(username, password)) return TransmisionProtocol.CreateServerMessage((int)ErrorCodes.NO_ERROR, (int)Options.CREATE_USER);
            else return TransmisionProtocol.CreateServerMessage((int)ErrorCodes.NO_ERROR, (int)Options.CREATE_USER);
        }

        public string CheckUserName(string msg, int clientId)
        {
            string[] fields = msg.Split("$$");
            string username = fields[0].Split(':')[1];
            DbMethods dbConnection = new DbMethods();
            lock (activeUsers[clientId]) { dbConnection = activeUsers[clientId].dbConnection; }
            if (!dbConnection.CheckIfNameExist(username))
                return TransmisionProtocol.CreateServerMessage((int)ErrorCodes.NO_ERROR, (int)Options.CHECK_USER_NAME);
            return TransmisionProtocol.CreateServerMessage((int)ErrorCodes.USER_ALREADY_EXISTS, (int)Options.CHECK_USER_NAME);
        }

        public string Disconnect(string msg, int clientId)
        {
            if (DeleteActiveUser(clientId))
                return "";
            return TransmisionProtocol.CreateServerMessage((int)ErrorCodes.DISCONNECT_ERROR, (int)Options.DISCONNECT);
        }

        public string GetFriends(string msg, int clientId)
        {
            DbMethods dbConnection = new DbMethods();
            string username;
            List<string> activeUsersNames = new List<string>();

            lock (activeUsers[clientId]) 
            { 
                if (!activeUsers[clientId].logged) return TransmisionProtocol.CreateServerMessage((int)ErrorCodes.NOT_LOGGED_IN, (int)Options.LOGIN);
                dbConnection = activeUsers[clientId].dbConnection;
                username = activeUsers[clientId].name;
                foreach(User user in activeUsers)
                {
                    if (user.logged) activeUsersNames.Add(user.name);
                }
            }
            return TransmisionProtocol.CreateServerMessage((int)ErrorCodes.NO_ERROR,(int)Options.LOGIN,dbConnection.GetFriends(username,activeUsersNames));
        }

        public string GetConversation(string msg, int clientId)
        {

            return "";
        }
        public string StartNewConversation(string msg, int clientId)
        {
            return "";
        }
        public string SendConversationKey(string msg, int clientId)
        {
            return "";
        }
        public string SendMessage(string msg, int clientId)
        {
            return "";
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

        public bool DeleteActiveUser(int userId)
        {
            try { lock(activeUsers[userId]) activeUsers[userId] = null; }
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
            functions.Add(new Functions(StartNewConversation));
            functions.Add(new Functions(SendConversationKey));
            functions.Add(new Functions(SendMessage));

            security = new Security();
            activeUsers = new List<User>();
        }
    }
}
