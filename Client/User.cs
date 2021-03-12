using System;
using System.Collections.Generic;
using System.Text;

namespace Client 
{
    public class User 
    {
        private readonly string sessionID;
        private readonly string username;
        private int elo;

        public string SessionID { get { return sessionID; } }
        public string Username { get { return username; } }
        public int Elo { get { return elo; } set { elo = value; } }

        public User(string sessionID, string username, int elo) {
            this.sessionID = sessionID;
            this.username = username;
            this.elo = elo;
        }

    }
}
