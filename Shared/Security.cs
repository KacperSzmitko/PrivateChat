using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

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

        public void test()
        {
            DHParameters d = GenerateParameters();
            

        }

        public DHParameters GenerateParameters()
        {
            var generator = new DHParametersGenerator();
            generator.Init(512,10, new SecureRandom());
            return generator.GenerateParameters();
        }

        public string GetG(DHParameters parameters)
        {
            return parameters.G.ToString();
        }

        public string GetP(DHParameters parameters)
        {
            return parameters.P.ToString();
        }

        public AsymmetricCipherKeyPair GenerateKeys(DHParameters parameters)
        {
            var keyGen = GeneratorUtilities.GetKeyPairGenerator("DH");
            var kgp = new DHKeyGenerationParameters(new SecureRandom(), parameters);
            keyGen.Init(kgp);
            return keyGen.GenerateKeyPair();
        }

        // This returns A
        public string GetPublicKey(AsymmetricCipherKeyPair keyPair)
        {
            var dhPublicKeyParameters = keyPair.Public as DHPublicKeyParameters;
            if (dhPublicKeyParameters != null)
            {
                return dhPublicKeyParameters.Y.ToString();
            }
            throw new NullReferenceException("The key pair provided is not a valid DH keypair.");
        }

        // This returns a
        public string GetPrivateKey(AsymmetricCipherKeyPair keyPair)
        {
            var dhPrivateKeyParameters = keyPair.Private as DHPrivateKeyParameters;
            if (dhPrivateKeyParameters != null)
            {
                return dhPrivateKeyParameters.X.ToString();
            }
            throw new NullReferenceException("The key pair provided is not a valid DH keypair.");
        }

        public Org.BouncyCastle.Math.BigInteger ComputeSharedSecret(string A, AsymmetricKeyParameter bPrivateKey, DHParameters internalParameters)
        {
            var importedKey = new DHPublicKeyParameters(new Org.BouncyCastle.Math.BigInteger(A), internalParameters);
            var internalKeyAgree = AgreementUtilities.GetBasicAgreement("DH");
            internalKeyAgree.Init(bPrivateKey);
            return internalKeyAgree.CalculateAgreement(importedKey);
        }
    }
}
