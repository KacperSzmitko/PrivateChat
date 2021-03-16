using Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace Client
{
    public static class ServerCommands
    {

        //******************** COMMANDS RESPONSES (MULTIPLE FIELDS) ********************

        public struct GetFriendsCommandResponse
        {
            public readonly int error;
            public readonly string friendsJSON;
            public GetFriendsCommandResponse(int error, string friendsJSON) {
                this.error = error;
                this.friendsJSON = friendsJSON;
            }
        }

        public struct SendInvitationCommandResponse
        {
            public readonly int error;
            public readonly string g;
            public readonly string p;
            public readonly int invitationID;

            public SendInvitationCommandResponse(int error, string g, string p, int invitationID) {
                this.error = error;
                this.g = g;
                this.p = p;
                this.invitationID = invitationID;
            }
        }

        private static readonly object comunicateLock = new object(); //Only one thread can use client-server communcation methods at the same time

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
                        result += AddField("ConversationID", fields[0]);
                        break;
                    case 8:
                        result += AddField("Username", fields[0]);
                        result += AddField("Data", fields[1]);
                        break;
                    case 11:
                        result += AddField("Username", fields[0]);
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

        public static string Communicate(ref ServerConnection connection, string command) {
            string response = "";
            lock (comunicateLock) {
                connection.SendMessage(command);
                response = connection.ReadMessage();
            }
            return response;
        }

        //******************** COMMANDS ********************

        public static int DisconnectCommand(ref ServerConnection connection) {
            string command = CreateClientMessage((int)Options.DISCONNECT);
            string[] args = GetArgArrayFromResponse(Communicate(ref connection, command));
            return Int32.Parse(args[0]);
        }

        public static int LoginCommand(ref ServerConnection connection, string username, string password) {
            string command = CreateClientMessage((int)Options.LOGIN, username, password);
            string[] args = GetArgArrayFromResponse(Communicate(ref connection, command));
            return Int32.Parse(args[0]);
        }

        public static int RegisterUser(ref ServerConnection connection, string username, string password) {
            string command = CreateClientMessage((int)Options.CREATE_USER, username, password);
            string[] args = GetArgArrayFromResponse(Communicate(ref connection, command));
            return Int32.Parse(args[0]);
        }

        public static int CheckUsernameExistCommand(ref ServerConnection connection, string username) {
            string command = CreateClientMessage((int)Options.CHECK_USER_NAME, username);
            string[] args = GetArgArrayFromResponse(Communicate(ref connection, command));
            return Int32.Parse(args[0]);
        }

        public static int LogoutCommand(ref ServerConnection connection) {
            string command = CreateClientMessage((int)Options.LOGOUT);
            string[] args = GetArgArrayFromResponse(Communicate(ref connection, command));
            return Int32.Parse(args[0]);
        }

        public static GetFriendsCommandResponse GetFriendsCommand(ref ServerConnection connection) {
            string command = CreateClientMessage((int)Options.GET_FRIENDS);
            string[] args = GetArgArrayFromResponse(Communicate(ref connection, command));
            if (Int32.Parse(args[0]) != (int)ErrorCodes.NO_ERROR) return new GetFriendsCommandResponse(Int32.Parse(args[0]), "");
            return new GetFriendsCommandResponse(Int32.Parse(args[0]), args[1]);
        }

        public static SendInvitationCommandResponse SendInvitationCommand(ref ServerConnection connection, string username) {
            string command = CreateClientMessage((int)Options.ADD_FRIEND, username);
            string[] args = GetArgArrayFromResponse(Communicate(ref connection, command));
            if (Int32.Parse(args[0]) != (int)ErrorCodes.NO_ERROR) return new SendInvitationCommandResponse(Int32.Parse(args[0]), "", "", 0);
            return new SendInvitationCommandResponse(Int32.Parse(args[0]), args[1], args[2], Int32.Parse(args[3]));
        }

    }
}
