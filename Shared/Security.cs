using System;
using System.IO;
using System.Security.Cryptography;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace Shared
{
    public class Security
    {

        public static string HashPassword(string passwd)
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

        public static bool VerifyPassword(string passwdHash, string passwd)
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


        public static DHParameters GenerateParameters()
        {
            var generator = new DHParametersGenerator();
            generator.Init(512,10, new SecureRandom());
            return generator.GenerateParameters();
        }

        public static string GetG(DHParameters parameters)
        {
            return parameters.G.ToString();
        }

        public static string GetP(DHParameters parameters)
        {
            return parameters.P.ToString();
        }

        public static AsymmetricCipherKeyPair GenerateKeys(DHParameters parameters)
        {
            var keyGen = GeneratorUtilities.GetKeyPairGenerator("DH");
            var kgp = new DHKeyGenerationParameters(new SecureRandom(), parameters);
            keyGen.Init(kgp);
            return keyGen.GenerateKeyPair();
        }

        public static string GetPublicKey(AsymmetricCipherKeyPair keyPair)
        {
            var dhPublicKeyParameters = keyPair.Public as DHPublicKeyParameters;
            if (dhPublicKeyParameters != null)
            {
                return dhPublicKeyParameters.Y.ToString();
            }
            throw new NullReferenceException("The key pair provided is not a valid DH keypair.");
        }

        public static string GetPrivateKey(AsymmetricCipherKeyPair keyPair)
        {
            var dhPrivateKeyParameters = keyPair.Private as DHPrivateKeyParameters;
            if (dhPrivateKeyParameters != null)
            {
                return dhPrivateKeyParameters.X.ToString();
            }
            throw new NullReferenceException("The key pair provided is not a valid DH keypair.");
        }

        public static byte[] GetPublicKeyBytes(AsymmetricCipherKeyPair keyPair) {
            var dhPublicKeyParameters = keyPair.Public as DHPublicKeyParameters;
            if (dhPublicKeyParameters != null) {
                return dhPublicKeyParameters.Y.ToByteArray();
            }
            throw new NullReferenceException("The key pair provided is not a valid DH keypair.");
        }

        public static byte[] GetPrivateKeyBytes(AsymmetricCipherKeyPair keyPair) {
            var dhPrivateKeyParameters = keyPair.Private as DHPrivateKeyParameters;
            if (dhPrivateKeyParameters != null) {
                return dhPrivateKeyParameters.X.ToByteArray();
            }
            throw new NullReferenceException("The key pair provided is not a valid DH keypair.");
        }

        public static Org.BouncyCastle.Math.BigInteger ComputeSharedSecret(string A, AsymmetricKeyParameter bPrivateKey, DHParameters internalParameters)
        {
            var importedKey = new DHPublicKeyParameters(new Org.BouncyCastle.Math.BigInteger(A), internalParameters);
            var internalKeyAgree = AgreementUtilities.GetBasicAgreement("DH");
            internalKeyAgree.Init(bPrivateKey);
            return internalKeyAgree.CalculateAgreement(importedKey);
        }

        public static string ByteArrayToHexString(byte[] ba) {
            return BitConverter.ToString(ba).Replace("-", "");
        }

        public static byte[] HexStringToByteArray(string hex) {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

        public static byte[] CreateSHA256Hash(byte[] bytes) {
            return SHA256.Create().ComputeHash(bytes);
        }

        public static (byte[] key, byte[] iv) GenerateAESKeyAndIV() {
            using (Aes aes = Aes.Create()) {
                aes.Mode = CipherMode.CBC;
                aes.KeySize = 256;
                return (aes.Key, aes.IV);
            }
        }

        public static byte[] GenerateIV() {
            byte[] iv = new byte[8];
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider()) {
                rng.GetBytes(iv);
            }
            return iv;
        }

        public static byte[] AESEncrypt(byte[] bytesToEncrypt, byte[] encryptingKey, byte[] iv) {
            using (Aes aes = Aes.Create()) {
                aes.KeySize = 256;
                aes.BlockSize = 128;
                aes.Padding = PaddingMode.Zeros;
                aes.Key = encryptingKey;
                aes.IV = iv;
                using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV)) {
                    return AESPerformCryptography(bytesToEncrypt, encryptor);
                }
            }
        }

        public static byte[] AESDecrypt(byte[] bytesToDecrypt, byte[] decryptingKey, byte[] iv) {
            using (Aes aes = Aes.Create()) {
                aes.KeySize = 256;
                aes.BlockSize = 128;
                aes.Padding = PaddingMode.Zeros;
                aes.Key = decryptingKey;
                aes.IV = iv;
                using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV)) {
                    return AESPerformCryptography(bytesToDecrypt, decryptor);
                }
            }
        }
        
        private static byte[] AESPerformCryptography(byte[] data, ICryptoTransform cryptoTransform) {
            using (MemoryStream ms = new MemoryStream()) {
                using (CryptoStream cryptoStream = new CryptoStream(ms, cryptoTransform, CryptoStreamMode.Write)) {
                    cryptoStream.Write(data, 0, data.Length);
                    cryptoStream.FlushFinalBlock();
                    return ms.ToArray();
                }
            }
        }

        public static bool CompareByteArrays(byte[] ba1, byte[] ba2) {
            if (ba1.Length != ba2.Length) return false;
            for (int i = 0; i < ba1.Length; i++) {
                if (ba1[i] != ba2[i]) return false;
            }
            return true;

        }
    }
}
