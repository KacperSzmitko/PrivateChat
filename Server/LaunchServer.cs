using DbLibrary;
using Newtonsoft.Json;
using Shared;
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
            //FriendsAddTest();
        }

        public static void CreateUser(string username, string pass)
        {
            ClientProcessing cp = new ClientProcessing();
            int id = cp.AddActiveUser();
            cp.CreateUser(String.Format("Username:{0}$$Password:{1}$$UserIV:{0}$$",username,pass),id);
        }


        public static void FriendsAddTest()
        {
            ClientProcessing cp = new ClientProcessing();
            int id = cp.AddActiveUser();
            int id1 = cp.AddActiveUser();
            Console.WriteLine(cp.Login("Username:test8$$Password:test1234$$", id));
            Console.WriteLine(cp.Login("Username:test7$$Password:12345678$$", id1));



            Console.WriteLine(cp.AddFriend("SecondUserName:test7$$", id));
            Console.WriteLine(cp.DhExchange("InvitationID:0$$PK:656665$$", id));
            Console.WriteLine(cp.SendInvitation("", id1));
            Console.WriteLine(cp.AcceptFriend("InvitationID:0$$PKB:4$$", id1));
            Console.WriteLine(cp.SendConversationKey("ConversationID:5$$ConversationKey:ytyt$$", id1));
            Console.WriteLine(cp.AcceptedFriend("", id));
            Console.WriteLine(cp.SendConversationKey("ConversationID:5$$ConversationKey:ytyoit$$", id));
        }

        public static void MessageTest()
        {
            ClientProcessing cp = new ClientProcessing();
            int id = cp.AddActiveUser();
            int id1 = cp.AddActiveUser();
            cp.Login("Username:test8$$Password:test1234$$", id);
            cp.Login("Username:test7$$Password:12345678$$", id1);


            Console.WriteLine(cp.ActivateConversation("ConversationID:6$$", id));
            Console.WriteLine(cp.NewMessages("", id));
            Message m = new Message { date = DateTime.Now, message = "test", username = "tar" };
            string s = JsonConvert.SerializeObject(m);
            Console.WriteLine(cp.SendMessage(String.Format("Username:test7$$Data:{0}$$", s), id));
            Console.WriteLine(cp.SendMessage(String.Format("Username:test7$$Data:{0}$$", s), id));
            Console.WriteLine(cp.Notification("", id1));
            Console.WriteLine(cp.Notification("", id1));
            Console.WriteLine(cp.ActivateConversation("ConversationID:6$$", id1));
            Console.WriteLine(cp.NewMessages("", id1));
        }

        public static void DH()
        {
            var par = Security.GenerateParameters();
            var Akeys = Security.GenerateKeys(par);
            var Bkeys = Security.GenerateKeys(par);
            Console.WriteLine(Security.ComputeSharedSecret(Security.GetPublicKey(Akeys), Bkeys.Private, par).ToString(16));
            Console.WriteLine(Security.ComputeSharedSecret(Security.GetPublicKey(Bkeys), Akeys.Private, par).ToString(16));
        }
    }
}
