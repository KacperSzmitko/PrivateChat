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
        public ConnectionMultiplexer redis { get; set; }
        public IDatabase db { get; set; }


        public bool AddMessage(int conversationID,string message)
        {
            string conversationId = conversationID.ToString();
            db = redis.GetDatabase(0);
            try
            {
                db.ListRightPush(conversationId, message);
                return true;
            }
            catch
            {
                return false;
            }
        }


        public string GetConversation(int conversationID)
        {
            string conversationId = conversationID.ToString();
            db = redis.GetDatabase(0);
            StringBuilder res = new StringBuilder("[");
            RedisValue[] conversation;
            try
            {
                conversation = db.ListRange(conversationId, 0, -1);
                var x = conversation[0];
            }
            catch
            {
                return "[{}]";
            }
            foreach (var message in conversation)
            {
                res.Append(message + ",");
            }
            res[res.Length - 1] = ']';
            return res.ToString();
        }


        public List<T> GetFromRedis<T>(string key) where T: ExtendedInvitation
        {
            db = redis.GetDatabase(1);

            try
            {
                var data = db.StringGet(key);
                return JsonConvert.DeserializeObject<List<T>>(data);
            }
            catch
            {
                return null;
            }
        }


        public void StoreInRedis<T>(List<T> data)
        {
            db = redis.GetDatabase(1);
            if (data.Count < 1) return;
            if(data[0].GetType() == typeof(Invitation))
            {
                db.StringSet("invs", JsonConvert.SerializeObject(data));
            }
            else
            {
                db.StringSet("messagesToSend", JsonConvert.SerializeObject(data));
            }
            
        }

        public void DeleteConversations(List<int> conversationsIds)
        {
            db = redis.GetDatabase(0);
            foreach(int key in conversationsIds)
            {
                db.KeyDelete(new RedisKey(key.ToString()));
            }
        }

        public Redis()
        {
            redis = ConnectionMultiplexer.Connect("10.8.0.1:6379,password=5fUwXohpL6rh5xvK");

        }
    }
}
