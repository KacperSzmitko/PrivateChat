    

namespace Shared
{
    public enum Options
    {
        //                          ADDITIONAl CLIENT FIELDS                        SERVER RESPONSE
        LOGOUT = 0,                 //                                              Error:<>$$
        LOGIN = 1,                  //  Username:<>$$Password:<>$$                  Error:<>$$
        CREATE_USER = 2,            //  Username:<>$$Password:<>$$                  Error:<>$$
        CHECK_USER_NAME = 3,        //  Username:<>$$                               Error:<>$$
        DISCONNECT = 4,             //                                              Error:<>$$
        GET_FRIENDS = 5,            //                                              Error:<>$$Data:<JSONFriend>$$
        GET_CONVERSATION = 6,       //  SecondUserName:<>                           Error:<>$$ConversationKey:<>$$ConversationID:<>$$Data:<JSONMessage>$$

        ACTIVATE_CONVERSATION = 7,  //  ConversationID:<>$$                         Error:<>$$
        SEND_MESSAGE = 8,           //  Username:<>$$Data:<JSON>$$                  Error:<>$$
        NEW_MESSAGES = 9,           //  SERVER ONLY                                 Error:<>$$Data:<JSONDataMessage>$$
        NOTIFICATION = 10,          //  SERVER ONLY                                 Error:<>$$Data:<JSONData>$$ (user:numerOfMessages)


        ADD_FRIEND = 11,            //  Username:<>$$                               Error:<>$$G:<>$$P:<>$$InvitationId:<>$$
        DH_EXCHANGE = 12,           //  InvitationID:<>$$PK:<>$$                    Error:<>$$
        SEND_INVITATION = 13,       //  SERVER ONLY                                 Error:<>$$Data:<JSONInvitationPK>$$
        DECLINE_FRIEND = 14,        //  InvitationID:<>$$                           Error:<>$$
        ACCEPT_FRIEND = 15,         //  InvitationID:<>$$PKB:<>$$                   Error:<>$$ConversationID:<>$$
        SEND_CONVERSATION_KEY = 16, //  ConversationID:<>$$ConversationKey:<>$$     Error:<>$$ 
        ACCEPTED_FRIEND = 17,       //  SERVER ONLY                                 Error:<>$$InvitationID:<>$$PK:<>$$ConversationID:<>$$
    }
}
