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

            ClientProcessing cp = new ClientProcessing();
            int id = cp.AddActiveUser();
            cp.Login("Username:test8$$Password:12345678$$",id);
            Console.WriteLine(cp.GetConversation("SecondUserName:test$$", id));
            /*
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
