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
            //Message m = new Message { message = "Witaj", date = DateTime.Now, username= "Piotr" };
            //db.ListRightPush("3", JsonConvert.SerializeObject(m));
            StringBuilder res = new StringBuilder("[");
            var x = db.ListRange("3",0,-1);
            foreach(var k in x)
            {
                res.Append(k + ",");
            }
            res[res.Length-1] = ']';
            
            
            //Console.WriteLine(res);

            List<Message> des = JsonConvert.DeserializeObject<List<Message>>(res.ToString());
            Console.WriteLine(des);

        }

        public Redis()
        {
            redis = ConnectionMultiplexer.Connect("192.168.0.9:6381");
            db = redis.GetDatabase();

        }
    }
}
