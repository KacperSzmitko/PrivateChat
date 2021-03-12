using DbLibrary;
using System;
using System.Collections.Generic;

namespace Server
{
    class LaunchServer
    {
        public static void Main(string[] args)
        {
            DbMethods db = new DbMethods();
            Console.WriteLine(db.GetFriends("Test", new List<string>() { "test1", "test4" }));
        }
    }
}
