using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Server
{
    public class Security
    {
        public string HashPassword(string passwd)
        {
            int saltSize = 16;
            int totalSize = saltSize + passwd.Length;
            byte[] salt;
            new RNGCryptoServiceProvider().GetBytes(salt = new byte[saltSize]);

            var pbkdf2 = new Rfc2898DeriveBytes(passwd, salt, 100000);
            byte[] hash = pbkdf2.GetBytes(passwd.Length);

            byte[] hashBytes = new byte[totalSize];
            Array.Copy(salt, 0, hashBytes, 0, saltSize);
            Array.Copy(hash, 0, hashBytes, saltSize, passwd.Length);
            return Convert.ToBase64String(hashBytes);
        }

        public bool VerifyPassword(string passwdHash, string passwd)
        {
            byte[] hashBytes = Convert.FromBase64String(passwdHash);
            /* Get the salt */
            byte[] salt = new byte[16];
            Array.Copy(hashBytes, 0, salt, 0, 16);
            /* Compute the hash on the password the user entered */
            var pbkdf2 = new Rfc2898DeriveBytes(passwd, salt, 100000);
            byte[] hash = pbkdf2.GetBytes(passwd.Length);
            /* Compare the results */
            for (int i = 0; i < passwd.Length; i++)
                if (hashBytes[i + 16] != hash[i])
                    return false;
            return true;
        }
    }
}
