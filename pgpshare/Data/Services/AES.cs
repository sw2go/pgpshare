using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;
using System.Text;

namespace PgpShare.Data.Services
{
    public class AES
    {
        // Siehe 1:1 aes.js
        const int GCM_TAG_SIZE = 120;       // Bits
        const int DERIVED_KEY_SIZE = 256;   // Bits
        const KeyDerivationPrf DERIV_HASH = KeyDerivationPrf.HMACSHA256;

        // Defaultwerte
        const int DEFAULT_ITERATIONS = 500000;

        public static void Hoi()
        {           
            string password = "123";
            string passphrase = "ABC";

            var rand = RandomNumberGenerator.Create();
            byte[] salt = new byte[8] { 38, 233, 174, 43, 157, 103, 101, 212 };
            //rand.GetBytes(salt);

            byte[] iv = new byte[12] { 239, 96, 94, 180, 7, 217, 22, 32, 96, 243, 0, 0 };
            //rand.GetBytes(iv);

            var iterations = 1000000;

            var keySize = 32;
            
            byte[] derivedKeyOld = KeyDerivation.Pbkdf2(password, salt, KeyDerivationPrf.HMACSHA256, iterations, keySize);


            //var wwww = decrypt("MTAwMDAwMA==.JumuK51nZdQ=.72BetAfZFiBg8wAA.JWWBq9iAL+kogiuMasgNxIcRTw==", "123");



            var derivedKey = new AesGcm(generatePBKDF2(password, new KeyParams() { Iterations = iterations, Salt = salt }));

            var xxxx = gcmEncrypt(passphrase, derivedKey, iv);

            var zzzz = gcmDecrypt(xxxx, derivedKey);


            byte[] plain = Encoding.UTF8.GetBytes(passphrase);

            byte[] cipher = new byte[plain.Length];
            byte[] tag = new byte[16];


            derivedKey.Encrypt(iv, plain, cipher, tag);

            string[] enc =
            {
                Convert.ToBase64String(Encoding.UTF8.GetBytes(iterations.ToString())),
                Convert.ToBase64String(salt),
                Convert.ToBase64String(iv),
                Convert.ToBase64String(Combine(cipher, tag))
            };

            var ddd = string.Join(".", enc);

            var ss = "MTAwMDAwMA==.JumuK51nZdQ=.72BetAfZFiBg8wAA.JWWBq9iAL+kogiuMasgNxIcRTw==";

            var sorter = "MTAwMDAwMA==.JumuK51nZdQ=.72BetAfZFiBg8wAA.JWWBq9iAL+kogiuMasgNxIcR";

            if (ddd == ss)
            {
                derivedKey.Decrypt(iv, cipher, tag, plain);
            }
        }

       
        public static byte[] Combine(byte[] first, byte[] second)
        {
            byte[] result = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, result, 0, first.Length);
            Buffer.BlockCopy(second, 0, result, first.Length, second.Length);
            return result;
        }


        public class KeyParams
        {
            public int? Iterations { get; set; }
            public byte[] Salt { get; set; }
        }

        public static byte[] generatePBKDF2(string password, KeyParams keyParams = null)
        {
            int iterations;
            byte[] salt;
            
            if (keyParams != null && keyParams.Salt != null)
            {
                salt = keyParams.Salt;
            } 
            else
            {
                salt = new byte[32];
                var rand = RandomNumberGenerator.Create();
                rand.GetBytes(salt);
            }

            if (keyParams != null && keyParams.Iterations.HasValue)
            {
                iterations = keyParams.Iterations.Value;
            }
            else
            {
                iterations = DEFAULT_ITERATIONS;
            }

            return KeyDerivation.Pbkdf2(password, salt, DERIV_HASH, iterations, DERIVED_KEY_SIZE / 8);
        }

        public static string gcmEncrypt(string message, AesGcm key, byte[] iv = null)
        {
            byte[] msg = Encoding.UTF8.GetBytes(message);            

            if (iv == null)
            {
                iv = new byte[12];
                var rand = RandomNumberGenerator.Create();
                rand.GetBytes(iv);
            }

            byte[] cipher = new byte[msg.Length];
            byte[] tag = new byte[GCM_TAG_SIZE / 8];
            
            key.Encrypt(iv, msg, cipher, tag);

            string[] enc =
            {
                Convert.ToBase64String(iv),
                Convert.ToBase64String(Combine(cipher, tag))
            };

            return string.Join(".", enc);
        }

        public static string gcmDecrypt(string message, AesGcm key)
        {
            string[] parts = message.Split(".");

            byte[] iv = Convert.FromBase64String(parts[parts.Length - 2]);

            byte[] enc = Convert.FromBase64String(parts[parts.Length - 1]);

            byte[] tag = new byte[GCM_TAG_SIZE / 8];
            byte[] cipher = new byte[enc.Length - tag.Length];
            byte[] plain = new byte[cipher.Length]; 

            Buffer.BlockCopy(enc, 0, cipher, 0, enc.Length - tag.Length);
            Buffer.BlockCopy(enc, cipher.Length, tag, 0, tag.Length);

            key.Decrypt(iv, cipher, tag, plain);

            return Encoding.UTF8.GetString(plain);
        }


        public static string encrypt(string message, string password, int? iterations = null)
        {
            int rounds = (iterations.HasValue) ? iterations.Value : DEFAULT_ITERATIONS;

            using (var key = new AesGcm(generatePBKDF2(password, new KeyParams() { Iterations = rounds })))
            {
                return gcmEncrypt(message, key);
            }
        }

        public static string decrypt(string message, string password)
        {
            string[] parts = message.Split(".");

            int iterations = int.Parse(Encoding.UTF8.GetString(Convert.FromBase64String(parts[0])));
            byte[] salt = Convert.FromBase64String(parts[1]);

            using(var key = new AesGcm(generatePBKDF2(password, new KeyParams() { Iterations = iterations, Salt = salt })))
            {
                return gcmDecrypt(message, key);
            }
        }
    }
}
