
namespace Shared
{
  public enum ErrorCodes
    {   // General
        NO_ERROR = 0,
        DISCONNECT_ERROR = 7,
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
    }
}
