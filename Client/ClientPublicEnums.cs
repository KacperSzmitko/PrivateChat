using System;
using System.Collections.Generic;
using System.Text;

namespace Client
{
    public enum InvitationStatus
    {
        NO_INVITATION,
        WAITING_FOR_RESPONSE,
        INVITATION_SENT,
        USER_NOT_FOUND,
        USER_ALREADY_A_FRIEND,
        SELF_INVITATION
    }

}
