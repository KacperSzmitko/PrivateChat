using System;
using System.Collections.Generic;
using System.Text;

namespace Client 
{
    public class User 
    {
        private readonly string username;

        public string Username { get { return username; } }

        public User(string username) {
            this.username = username;
        }

    }
}
