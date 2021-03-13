using Newtonsoft.Json;
using Shared;
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


        public bool AddMessage(string conversationId,Message message)
        {
            try
            {
                db.ListRightPush(conversationId, JsonConvert.SerializeObject(message));
                return true;
            }
            catch
            {
                return false;
            }
        }


        public string GetConversation(string conversationId)
        {
            StringBuilder res = new StringBuilder("[");
            RedisValue[] conversation;
            try
            {
                conversation = db.ListRange(conversationId, 0, -1);
            }
            catch
            {
                return "";
            }
            foreach (var message in conversation)
            {
                res.Append(message + ",");
            }
            res[res.Length - 1] = ']';
            return res.ToString();
        }

        public void test()
        {
            //Message m = new Message { message = "Witaj", date = DateTime.Now, username= "Piotr" };
            StringBuilder res = new StringBuilder("[");
            var x = db.ListRange("3",0,-1);
            foreach(var k in x)
            {
                res.Append(k + ",");
            }
            res[res.Length-1] = ']';


            //List<Message> des = JsonConvert.DeserializeObject<List<Message>>(res.ToString());

        }

        public Redis()
        {
            redis = ConnectionMultiplexer.Connect("192.168.0.9:6381");
            db = redis.GetDatabase();

        }
    }
}
