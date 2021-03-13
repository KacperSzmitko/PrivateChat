using Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace Client
{
    public static class ServerCommands
    {

        //******************** COMMANDS RESPONSES (MULTIPLE FIELDS) ********************
        

        //******************** TOOLS FOR CREATING COMMANDS ********************

        private static string CreateClientMessage(int option, params string[] fields) {
            string result = "";
            try {
                result += AddField("option", option.ToString());
                switch (option) {
                    case 0:
                        break;
                    case 1:
                        result += AddField("Username", fields[0]);
                        result += AddField("Password", fields[1]);
                        break;
                    case 2:
                        result += AddField("Username", fields[0]);
                        result += AddField("Password", fields[1]);
                        break;
                    case 3:
                        result += AddField("Username", fields[0]);
                        break;
                    case 4:
                        break;
                    case 5:
                        break;
                    case 6:
                        result += AddField("SecondUserName", fields[0]);
                        break;
                    case 7:
                        result += AddField("SecondUserName", fields[0]);
                        break;
                    case 8:
                        result += AddField("ConversationID", fields[0]);
                        result += AddField("Key", fields[1]);
                        break;
                    case 9:
                        result += AddField("ConversationID", fields[0]);
                        result += AddField("Data", fields[1]);
                        break;
                    default: throw new ArgumentException("Invalid option!");
                }
            }
            catch (ArgumentOutOfRangeException) {
                throw new Exception("Invalid number of parametrs");
            }

            return result;
        }

        private static string AddField(string fieldName, string value) {
            return fieldName + ":" + value + "$$";
        }

        private static string[] GetArgArrayFromResponse(string response) {
            string[] args = response.Split("$$", StringSplitOptions.RemoveEmptyEntries);
            int numOfArgs = args.Length;

            string[] argArray = new string[numOfArgs];

            for (int i = 0; i < numOfArgs; i++) {
                string[] arg = args[i].Split(":", 2, StringSplitOptions.RemoveEmptyEntries);
                argArray[i] = arg[1];
            }

            return argArray;
        }

        private static string[,] Get2DArgArrayFromResponse(string response) {
            string[] args = response.Split("$$", StringSplitOptions.RemoveEmptyEntries);
            int numOfArgs = args.Length;

            string[,] argArray = new string[numOfArgs, 2];

            for (int i = 0; i < numOfArgs; i++) {
                string[] arg = args[i].Split(":", 2, StringSplitOptions.RemoveEmptyEntries);
                argArray[i, 0] = arg[0];
                argArray[i, 1] = arg[1];
            }

            return argArray;
        }

        //******************** COMMANDS ********************

        public static int DisconnectCommand(ref ServerConnection connection) {
            string command = CreateClientMessage((int)Options.DISCONNECT);
            connection.SendMessage(command);
            string[] args = GetArgArrayFromResponse(connection.ReadMessage());
            return Int32.Parse(args[0]);
        }

        public static int LoginCommand(ref ServerConnection connection, string username, string password) {
            string command = CreateClientMessage((int)Options.LOGIN, username, password);
            connection.SendMessage(command);
            string[] args = GetArgArrayFromResponse(connection.ReadMessage());
            return Int32.Parse(args[0]);
        }

        public static int RegisterUser(ref ServerConnection connection, string username, string password) {
            string command = CreateClientMessage((int)Options.CREATE_USER, username, password);
            connection.SendMessage(command);
            string[] args = GetArgArrayFromResponse(connection.ReadMessage());
            return Int32.Parse(args[0]);
        }

        public static int CheckUsernameExistCommand(ref ServerConnection connection, string username) {
            string command = CreateClientMessage((int)Options.CHECK_USER_NAME, username);
            connection.SendMessage(command);
            string[] args = GetArgArrayFromResponse(connection.ReadMessage());
            return Int32.Parse(args[0]);
        }

        public static int LogoutCommand(ref ServerConnection connection, string sessionID) {
            string command = CreateClientMessage((int)Options.LOGOUT, sessionID);
            connection.SendMessage(command);
            string[] args = GetArgArrayFromResponse(connection.ReadMessage());
            return Int32.Parse(args[0]);
        }

    }
}
