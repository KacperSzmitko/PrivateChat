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
            //ServerConnection connection = new ServerConnection();
            Redis redis = new Redis();
            //redis.AddMessage("23", new Shared.Message { date = DateTime.Now, message = "Nowa", username = "Łukasz" });


        }
    }
}
