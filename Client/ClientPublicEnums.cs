using System;
using System.Collections.Generic;
using System.Text;

namespace Client
{
    public enum InvitationStatuses
    {
        NO_INVITATION,
        WAITING_FOR_RESPONSE,
        INVITATION_SENT,
        USER_NOT_FOUND,
        USER_ALREADY_A_FRIEND,
        SELF_INVITATION,
        INVITATION_ALREADY_EXIST
    }

    public enum MessageStatuses
    {
        NOT_LAST_MESSAGE,
        MESSAGE_SENT,
        MESSAGE_RECEIVED_SERVER,
        MESSAGE_RECEIVED_FRIEND
    }

}
