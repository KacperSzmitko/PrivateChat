﻿using Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace Client
{
    public static class ServerCommands {

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
                        result += AddField("UserIV", fields[2]);
                        result += AddField("Hash", fields[3]);
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
                        result += AddField("ConversationID", fields[0]);
                        result += AddField("Data", fields[1]);
                        break;
                    case 9:
                        break;
                    case 10:
                        break;
                    case 11:
                        result += AddField("Username", fields[0]);
                        break;
                    case 12:
                        result += AddField("InvitationID", fields[0]);
                        result += AddField("PublicK", fields[1]);
                        result += AddField("PrivateK", fields[2]);
                        result += AddField("IV", fields[3]);
                        break;
                    case 13:
                        break;
                    case 14:
                        result += AddField("InvitationID", fields[0]);
                        break;
                    case 15:
                        result += AddField("InvitationID", fields[0]);
                        result += AddField("PKB", fields[1]);
                        break;
                    case 16:
                        result += AddField("ConversationID", fields[0]);
                        result += AddField("ConversationKey", fields[1]);
                        break;
                    case 17:
                        break;
                    case 18:
                        break;
                    case 19:
                        result += AddField("SecondUserName", fields[0]);
                        result += AddField("Amount", fields[1]);
                        break;
                    case 20:
                        result += AddField("SecondUserName", fields[0]);
                        result += AddField("Amount", fields[1]);
                        result += AddField("Offset", fields[2]);
                        break;
                    case 21:
                        result += AddField("ConversationID", fields[0]);
                        break;
                    case 22:
                        result += AddField("AttachmentID", fields[0]);
                        break;
                    case 23:
                        result += AddField("ConversationID", fields[0]);
                        result += AddField("Attachment", fields[1]);
                        result += AddField("Filename", fields[2]);
                        break;
                    case 24:
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

        public static (int error, string userIV, string userKeyHash) LoginCommand(ref ServerConnection connection, string username, string password) {
            string command = CreateClientMessage((int)Options.LOGIN, username, password);
            string[] args = GetArgArrayFromResponse(Communicate(ref connection, command));
            if (Int32.Parse(args[0]) != (int)ErrorCodes.NO_ERROR) return (Int32.Parse(args[0]), "", "");
            return (Int32.Parse(args[0]), args[1], args[2]);
        }

        public static int RegisterUser(ref ServerConnection connection, string username, string password, string userIV, string userKeyHash) {
            string command = CreateClientMessage((int)Options.CREATE_USER, username, password, userIV, userKeyHash);
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

        public static (int error, string friendsJSON) GetFriendsCommand(ref ServerConnection connection) {
            string command = CreateClientMessage((int)Options.GET_FRIENDS);
            string[] args = GetArgArrayFromResponse(Communicate(ref connection, command));
            if (Int32.Parse(args[0]) != (int)ErrorCodes.NO_ERROR) return (Int32.Parse(args[0]), "");
            return (Int32.Parse(args[0]), args[1]);
        }

        public static (int error, string p, string g, string invitationID) SendInvitationCommand(ref ServerConnection connection, string username) {
            string command = CreateClientMessage((int)Options.SEND_FRIEND_INVITATION, username);
            string[] args = GetArgArrayFromResponse(Communicate(ref connection, command));
            if (Int32.Parse(args[0]) != (int)ErrorCodes.NO_ERROR) return (Int32.Parse(args[0]), "", "", "");
            return (Int32.Parse(args[0]), args[1], args[2], args[3]);
        }

        public static int SendPublicDHKeyCommand(ref ServerConnection connection, string invitationID, string publicDHKey, string privateEncryptedDHKey, string privateEncryptedDHKeyIV) {
            string command = CreateClientMessage((int)Options.SEND_DH_PK_INVITING, invitationID, publicDHKey, privateEncryptedDHKey, privateEncryptedDHKeyIV);
            string[] args = GetArgArrayFromResponse(Communicate(ref connection, command));
            return Int32.Parse(args[0]);
        }

        public static (int error, string newMessagesInfoJSON) GetNotificationsCommand(ref ServerConnection connection) {
            string command = CreateClientMessage((int)Options.GET_NOTIFICATIONS);
            string[] args = GetArgArrayFromResponse(Communicate(ref connection, command));
            if (Int32.Parse(args[0]) != (int)ErrorCodes.NO_ERROR) return (Int32.Parse(args[0]), "");
            return (Int32.Parse(args[0]), args[1]);
        }

        public static (int error, string invitationsJSON) GetInvitationsCommand(ref ServerConnection connection) {
            string command = CreateClientMessage((int)Options.GET_FRIEND_INVITATIONS);
            string[] args = GetArgArrayFromResponse(Communicate(ref connection, command));
            if (Int32.Parse(args[0]) != (int)ErrorCodes.NO_ERROR) return (Int32.Parse(args[0]), "");
            return (Int32.Parse(args[0]), args[1]);
        }

        public static (int error, string conversationID, string conversationIV) AcceptFriendInvitationCommand(ref ServerConnection connection, string invitationID, string PKB) {
            string command = CreateClientMessage((int)Options.ACCPET_FRIEND_INVITATION, invitationID, PKB);
            string[] args = GetArgArrayFromResponse(Communicate(ref connection, command));
            if (Int32.Parse(args[0]) != (int)ErrorCodes.NO_ERROR) return (Int32.Parse(args[0]), "", "");
            return (Int32.Parse(args[0]), args[1], args[2]);
        }

        public static int DeclineFriendInvitationCommand(ref ServerConnection connection, string invitationID) {
            string command = CreateClientMessage((int)Options.DECLINE_FRIEND_INVITATION, invitationID);
            string[] args = GetArgArrayFromResponse(Communicate(ref connection, command));
            return Int32.Parse(args[0]);
        }

        public static int SendEncryptedConversationKeyCommand(ref ServerConnection connection, string conversationID, string encryptedConversationKey) {
            string command = CreateClientMessage((int)Options.SEND_CONVERSATION_KEY, conversationID, encryptedConversationKey);
            string[] args = GetArgArrayFromResponse(Communicate(ref connection, command));
            return Int32.Parse(args[0]);
        }

        public static (int error, string ExtendedInvitationJSON) GetAcceptedInvitationsCommand(ref ServerConnection connection) {
            string command = CreateClientMessage((int)Options.GET_ACCEPTED_FRIENDS);
            string[] args = GetArgArrayFromResponse(Communicate(ref connection, command));
            if (Int32.Parse(args[0]) != (int)ErrorCodes.NO_ERROR) return (Int32.Parse(args[0]), "");
            return (Int32.Parse(args[0]), args[1]);
        }

        public static (int error, string conversationID, string conversationKey, string conversationIV, string messagesJSON) GetConversationCommand(ref ServerConnection connection, string username)
        {
            string command = CreateClientMessage((int)Options.GET_CONVERSATION, username);
            string[] args = GetArgArrayFromResponse(Communicate(ref connection, command));
            if (Int32.Parse(args[0]) != (int)ErrorCodes.NO_ERROR) return (Int32.Parse(args[0]), "", "", "", "");
            return (Int32.Parse(args[0]), args[1], args[2], args[3], args[4]);
        }

        public static (int error, string conversationID, string conversationKey, string conversationIV, int fullMsgAmount, string messagesJSON) GetLastConversationCommand(ref ServerConnection connection, string username, int amount)
        {
            string command = CreateClientMessage((int)Options.GET_LAST_CONVERSATION, username, amount.ToString());
            string[] args = GetArgArrayFromResponse(Communicate(ref connection, command));
            if (Int32.Parse(args[0]) != (int)ErrorCodes.NO_ERROR) return (Int32.Parse(args[0]), "", "", "", 0, "");
            return (Int32.Parse(args[0]), args[1], args[2], args[3], Int32.Parse(args[4]), args[5]);
        }

        public static (int error, string conversationID, string conversationKey, string conversationIV, int fullMsgAmount, string messagesJSON) GetMoreConversationCommand(ref ServerConnection connection, string username, int amount, int offset)
        {
            string command = CreateClientMessage((int)Options.GET_PART_CONVERSATION, username, amount.ToString(), offset.ToString());
            string[] args = GetArgArrayFromResponse(Communicate(ref connection, command));
            if (Int32.Parse(args[0]) != (int)ErrorCodes.NO_ERROR) return (Int32.Parse(args[0]), "", "", "", 0, "");
            return (Int32.Parse(args[0]), args[1], args[2], args[3], Int32.Parse(args[4]), args[5]);
        }

        public static int ActivateConversationCommand(ref ServerConnection connection, string conversationID) {
            string command = CreateClientMessage((int)Options.ACTIVATE_CONVERSATION, conversationID);
            string[] args = GetArgArrayFromResponse(Communicate(ref connection, command));
            return Int32.Parse(args[0]);
        }

        public static (int error, string newMessegesJSON) GetNewMessagesCommand(ref ServerConnection connection)
        {
            string command = CreateClientMessage((int)Options.GET_NEW_MESSAGES);
            string[] args = GetArgArrayFromResponse(Communicate(ref connection, command));
            if (Int32.Parse(args[0]) != (int)ErrorCodes.NO_ERROR) return (Int32.Parse(args[0]), "");
            return (Int32.Parse(args[0]), args[1]);
        }

        public static (int error, string newMessegesJSON) GetNewAttachmentsCommand(ref ServerConnection connection, string conversationID)
        {
            string command = CreateClientMessage((int)Options.GET_NEW_ATTACHMENTS, conversationID);
            string[] args = GetArgArrayFromResponse(Communicate(ref connection, command));
            if (Int32.Parse(args[0]) != (int)ErrorCodes.NO_ERROR) return (Int32.Parse(args[0]), "");
            return (Int32.Parse(args[0]), args[1]);
        }

        public static (int error, string newMessegesJSON) GetAttachmentsCommand(ref ServerConnection connection, string conversationID)
        {
            string command = CreateClientMessage((int)Options.GET_ATTACHMENT_LIST, conversationID);
            string[] args = GetArgArrayFromResponse(Communicate(ref connection, command));
            if (Int32.Parse(args[0]) != (int)ErrorCodes.NO_ERROR) return (Int32.Parse(args[0]), "");
            return (Int32.Parse(args[0]), args[1]);
        }

        public static int SendMessageCommand(ref ServerConnection connection, string conversationID, string messageJSON)
        {
            string command = CreateClientMessage((int)Options.SEND_MESSAGE, conversationID, messageJSON);
            string[] args = GetArgArrayFromResponse(Communicate(ref connection, command));
            return Int32.Parse(args[0]);
        }

        public static (int error, uint attachmentID) SendAttachmentCommand(ref ServerConnection connection, string conversationID, string messageJSON, string filename)
        {
            string command = CreateClientMessage((int)Options.SEND_ATTACHMENT_FILE, conversationID, messageJSON, filename);
            string[] args = GetArgArrayFromResponse(Communicate(ref connection, command));
            return (Int32.Parse(args[0]), UInt32.Parse(args[1]));
        }

        public static (int error, String fileBase64) GetAttachmentCommand(ref ServerConnection connection, string attachmentID)
        {
            string command = CreateClientMessage((int)Options.GET_ATTACHMENT_FILE, attachmentID);
            string[] args = GetArgArrayFromResponse(Communicate(ref connection, command));
            if(args.Length>1)
                return (Int32.Parse(args[0]), args[1]);
            else
                return (Int32.Parse(args[0]), null);
        }

        public static int DeleteAccountCommand(ref ServerConnection connection) {
            string command = CreateClientMessage((int)Options.DELETE_ACCOUNT);
            string[] args = GetArgArrayFromResponse(Communicate(ref connection, command));
            return Int32.Parse(args[0]);
        }
    }
}
