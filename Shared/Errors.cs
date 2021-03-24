
namespace Shared
{
  public enum ErrorCodes
    {   // General
        NO_ERROR = 0,
        // Login
        NOT_LOGGED_IN = 1,
        USER_NOT_FOUND = 2,
        DB_CONNECTION_ERROR = 3,
        USER_ALREADY_LOGGED_IN = 4,
        INCORRECT_PASSWORD = 5,
        // Registry
        USER_ALREADY_EXISTS = 6,
        CONVERSATION_ALREADY_STARTED = 7,
        WRONG_CONVERSATION_ID = 8,
        ALREADY_FRIENDS = 9,
        DH_EXCHANGE_ERROR = 10,
        DECLINE_FRIEND_ERROR = 11,
        ADDING_FRIENDS_ERROR = 12,
        WRONG_INVATATION_ID = 13,
        NOTHING_TO_SEND = 14,
        CANNOT_ACTIVATE_CONVERSATION = 15,
        NO_MESSAGES = 16,
        NO_NOTIFICATIONS = 17,
        INVITATION_ALREADY_EXIST = 18,
        DB_DELETE_INVITATION_ERROR = 19,
        SELF_INVITE_ERROR = 20,
        DISCONNECT_ERROR = 21,
    }
}
