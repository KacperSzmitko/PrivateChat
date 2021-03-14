using Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    public class TransmisionProtocol
    {
        /// <summary>
        /// Function which formats server data by using our transmition protocol
        /// </summary>
        public static string CreateServerMessage(int errorCode, int option, params string[] fields)
        {
            string result = "";

            // We got it! Prepare answer from given data
            if (errorCode == 0)
            {
                AddFields(new[] {"Error"}, ref result,((int)ErrorCodes.NO_ERROR).ToString());
                try
                {

                    if (option < 0) throw new ArgumentException("Invalid option!");

                    else if (option == (int)Options.GET_CONVERSATION)
                    {
                        AddFields(new[] { "ConversationKey","ConversationID", "Data" }, ref result, fields);
                    }
                    else if (option == (int)Options.GET_FRIENDS || option == (int)Options.NEW_MESSAGES || option == (int)Options.NOTIFICATION
                           ||option == (int)Options.SEND_INVITATION)
                    {
                        AddFields(new[] { "Data" }, ref result, fields);
                    }
                    else if (option == (int)Options.ADD_FRIEND)
                    {
                        AddFields(new[] { "G", "P", "InvitationId" }, ref result, fields);
                    }
                    else if (option == (int)Options.ACCEPT_FRIEND)
                    {
                        AddFields(new[] { "ConversationID"}, ref result, fields);
                    }
                    else if (option == (int)Options.ACCEPTED_FRIEND)
                    {
                        AddFields(new[] { "InvitationID","PK","ConversationID" }, ref result, fields);
                    }
                }
                catch (ArgumentOutOfRangeException)
                {
                    throw new Exception("Invalid number of parametrs");
                }
            }
            //Tell client what went wrong
            else
            {
                AddFields(new[] { "Error" }, ref result,((int)errorCode).ToString());
            }
            return result;

        }

        /// <summary>
        /// Function used to add single arguments to message
        /// </summary>
        /// <param name="fieldsNames">Name of filed which you want to add</param>
        /// <param name="values">Values of fields</param>
        /// <param name="result">Reference to your result message</param>
        public static void AddFields(string[] fieldsNames, ref string result, params string[] values)
        {
            for (int i = 0; i < fieldsNames.Length; i++)
                result += fieldsNames[i] + ":" + values[i] + "$$";
        }
    }
}
