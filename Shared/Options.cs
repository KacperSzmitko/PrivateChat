    

namespace Shared
{
    public enum Options
    {
        //                          ADDITIONAl CLIENT FIELDS                      SERVER RESPONSE
        LOGOUT = 0,                 //                                              Error:<>$$
        LOGIN = 1,                  //  username:<>$$password:<>$$                  Error:<>$$
        CREATE_USER = 2,            //  username:<>$$password:<>$$                  Error:<>$$
        CHECK_USER_NAME = 3,        //  username:<>$$                               Error:<>$$
        DISCONNECT = 4,             //                                              Error:<>$$
        GET_FRIENDS = 5,            //                                              Error:<>$$Data:<XMLData>$$
        GET_CONVERSATION = 6,       //  SecondUserName:<>$$                         Error:<>$$Data:<JSONData>$$ConversationKey:<>$$ConversationID:<>$$
        START_NEW_CONVERSATION = 7, //  SecondUserName:<>$$                         Error:<>$$ConversationID:<>$$N:<>$$G:<>$$
        SEND_CONVERSATION_KEY = 8,  //  Key:<>$$                                    Error:<>$$    
        SEND_MESSAGE = 9,           //  ConversationID:<>$$Data:<JSON>$$            Error:<>$$
        

    }
}
