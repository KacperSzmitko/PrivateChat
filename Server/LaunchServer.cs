using DbLibrary;
using StackExchange.Redis;
using System;
using System.Collections.Generic;

namespace Server
{
    class LaunchServer
    {
        public static void Main(string[] args)
        {
            ServerConnection connection = new ServerConnection();

            /*
            ClientProcessing cp = new ClientProcessing();
            int id = cp.AddActiveUser();
            int id1 = cp.AddActiveUser();
            cp.Login("Username:test8$$Password:test1234$$", id);
            cp.Login("Username:test7$$Password:12345678$$", id1);
            Console.WriteLine(cp.AddFriend("SecondUserName:test7$$", id));
            Console.WriteLine(cp.DhExchange("InvitationID:0$$PK:656665$$",id));
            Console.WriteLine(cp.SendInvitation("",id1));
            Console.WriteLine(cp.AcceptFriend("InvitationID:0$$PKB:4$$",id1));
            Console.WriteLine(cp.SendConversationKey("ConversationID:5$$ConversationKey:ytyt$$", id1));
            Console.WriteLine(cp.AcceptedFriend("", id));
            Console.WriteLine(cp.SendConversationKey("ConversationID:5$$ConversationKey:ytyoit$$", id));


            
            Security s = new Security();
            var par = s.GenerateParameters();
            var Akeys = s.GenerateKeys(par);
            var Bkeys = s.GenerateKeys(par);
            Console.WriteLine(s.ComputeSharedSecret(s.GetPublicKey(Akeys), Bkeys.Private, par));
            Console.WriteLine(s.ComputeSharedSecret(s.GetPublicKey(Bkeys), Akeys.Private, par));
            */

        }
    }
}
