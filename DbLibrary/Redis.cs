using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;

namespace DbLibrary
{
    public class Redis
    {
        private ConnectionMultiplexer redis;
        public IDatabase db { get; set; }


        public void test()
        {
            Message m = new Message { message = "Witaj", date = DateTime.Now, username= "Piotr" };
            db.ListRightPush("3", JsonConvert.SerializeObject(m));
            var x = db.ListRange("3",0,-1);

            Message m1 = new Message { message = "Ziom", date = DateTime.Now, username = "Piotr" };
            db.ListRightPush("3", JsonConvert.SerializeObject(m1));

            string ret = "";
            foreach(var t in x)
            {
                ret += t;
            }

            Console.WriteLine(ret);
            
        }

        public Redis()
        {
            redis = ConnectionMultiplexer.Connect("192.168.0.9:6381");
            db = redis.GetDatabase();

        }
    }
}
