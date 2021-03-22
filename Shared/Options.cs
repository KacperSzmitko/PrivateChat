
namespace Shared
{
    public enum Options
    {
        //                                  ADDITIONAl CLIENT FIELDS                                        SERVER RESPONSE
        LOGOUT = 0,                         //                                                              Error:<>$$
        LOGIN = 1,                          //  Username:<>$$Password:<>$$                                  Error:<>$$UserIV:<>$$UserKeyHash:<>$$
        CREATE_USER = 2,                    //  Username:<>$$Password:<>$$UserIV:<>$$UserKeyHash:<>         Error:<>$$
        CHECK_USER_NAME = 3,                //  Username:<>$$                                               Error:<>$$
        DISCONNECT = 4,                     //                                                              Error:<>$$
        GET_FRIENDS = 5,                    //                                                              Error:<>$$Data:<JSONFriend>$$
        GET_CONVERSATION = 6,               //  SecondUserName:<>                                           Error:<>$$ConversationKey:<>$$ConversationID:<>$$Data:<JSONMessage>$$

        ACTIVATE_CONVERSATION = 7,          //  ConversationID:<>$$                                         Error:<>$$
        SEND_MESSAGE = 8,                   //  ConversationID:<>$$Data:<JSON>$$                            Error:<>$$
        GET_NEW_MESSAGES = 9,               //                                                              Error:<>$$Data:<JSONDataMessage>$$
        GET_NOTIFICATIONS = 10,             //                                                              Error:<>$$Data:<JSONData>$$ (user:numerOfMessages)

        SEND_FRIEND_INVITATION = 11,        //  Username:<>$$                                               Error:<>$$Data:<JSONInvitationPK>$$
        SEND_DH_PK_INVITING = 12,           //  InvitationID:<>$$PK:<>$$                                    Error:<>$$
        GET_FRIEND_INVITATIONS = 13,        //                                                              Error:<>$$Data:<JSONInvitationPK>$$
        DECLINE_FRIEND_INVITATION = 14,     //  InvitationID:<>$$                                           Error:<>$$
        ACCPET_FRIEND_INVITATION = 15,      //  InvitationID:<>$$PKB:<>$$                                   Error:<>$$ConversationID:<>$$
        SEND_CONVERSATION_KEY = 16,         //  ConversationID:<>$$ConversationKey:<>$$                     Error:<>$$ 
        GET_ACCEPTED_FRIENDS = 17,          //                                                              Error:<>$$Data:<JSONExtendedInvatation>$$
    }
}
