using Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace Client
{
    public static class ServerCommands
    {

        //******************** COMMANDS RESPONSES (MULTIPLE FIELDS) ********************

        public struct LoginCommandResponse
        {
            public readonly int error;
            public readonly string sessionID;
            public readonly int elo;
            public LoginCommandResponse(int error, string sessionID, int elo) {
                this.error = error;
                this.sessionID = sessionID;
                this.elo = elo;
            }
        }

        //******************** TOOLS FOR CREATING COMMANDS ********************

        /// <summary>
        /// Function which formats client data by using our transmition protocol
        /// </summary>
        /// <param name="result">Reference to string where u want your result to be stored</param>
        /// <param name="option">*0 - Logout  1 - MatchHistory  2 - Rank  3 - SearchGame  4 - EndGame  5 - Login  6 - CreateUser  7 - SendMove  8 - Disconnect  9 - CheckUserName  12 - StopSearchingForMatch</param>
        /// <param name="fields">*0-4, 12 : SessionID    5-6 : Username Password   7 : SessionID Move   9 : Username</param>
        private static string CreateClientMessage(int option, params string[] fields) {
            string result = "";
            try {
                result += AddField("option", option.ToString());
                //Logout MatchHistory Rank SearchGame EndGame
                if (option >= 0 && option <= 4 || option == 12) {
                    result += AddField("sessionid", fields[0]);
                }
                //Login UserCreate
                else if (option <= 6) {
                    result += AddField("username", fields[0]);
                    result += AddField("password", fields[1]);
                }
                //SendMove
                else if (option == 7) {
                    result += AddField("sessionid", fields[0]);
                    result += AddField("move", fields[1]);
                }
                else if (option == 9) {
                    result += AddField("username", fields[0]);
                }
                else if (option != 8) throw new ArgumentException("Invalid option!");
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

        public static LoginCommandResponse LoginCommand(ref ServerConnection connection, string username, string password) {
            string command = CreateClientMessage((int)Options.LOGIN, username, password);
            connection.SendMessage(command);
            string[] args = GetArgArrayFromResponse(connection.ReadMessage());
            if (Int32.Parse(args[0]) != (int)ErrorCodes.NO_ERROR) return new LoginCommandResponse(Int32.Parse(args[0]), "", 0);
            return new LoginCommandResponse(Int32.Parse(args[0]), args[1], Int32.Parse(args[2]));
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
