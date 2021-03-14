using DbLibrary;
using System;
using System.Collections.Generic;


namespace Server
{
    public class User
    {
        public string name { get; set; }
        public Dictionary<List<int>,int> conversationsIds { get; set; }
        public DbMethods dbConnection { get; set; }
        public Redis redis { get; set; }
        public bool logged;

        public User()
        {
            name = "";
            dbConnection = new DbMethods();
            redis = new Redis();
            logged = false;
        }
    }
}
