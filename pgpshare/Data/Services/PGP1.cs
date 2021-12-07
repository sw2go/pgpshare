using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PgpShare.Data.Services
{
    public class PGP1
    {
        public static void testc()
        {
            string inputMessage = "Test Message";
            UTF8Encoding utf8enc = new UTF8Encoding();
            // Converting the string message to byte array
            byte[] inputBytes = utf8enc.GetBytes(inputMessage);
            // RSAKeyPairGenerator generates the RSA Key pair based on the random number and strength of key required
            RsaKeyPairGenerator rsaKeyPairGnr = new RsaKeyPairGenerator();
            rsaKeyPairGnr.Init(new Org.BouncyCastle.Crypto.KeyGenerationParameters(new SecureRandom(), 512));
            Org.BouncyCastle.Crypto.AsymmetricCipherKeyPair keyPair = rsaKeyPairGnr.GenerateKeyPair();
            // Extracting the public key from the pair
            RsaKeyParameters publicKey = (RsaKeyParameters)keyPair.Public;
            // Creating the RSA algorithm object
            IAsymmetricBlockCipher cipher = new RsaEngine();

            // Initializing the RSA object for Encryption with RSA public key. Remember, for encryption, public key is needed
            cipher.Init(true, publicKey);
        
            //Encrypting the input bytes
            byte[] cipheredBytes = cipher.ProcessBlock(inputBytes, 0, inputMessage.Length);





            RsaKeyParameters privateKey = (RsaKeyParameters)keyPair.Private;
            cipher.Init(false, privateKey);
            byte[] deciphered = cipher.ProcessBlock(cipheredBytes, 0,
            cipheredBytes.Length);
            string decipheredText = utf8enc.GetString(deciphered);





        }


        public static void testx()
        {
            // Encryption steps -----------------------------------
            SHA256Managed hash = new SHA256Managed();
            SecureRandom randomNumber = new SecureRandom();
            byte[] encodingParam =
            hash.ComputeHash(Encoding.UTF8.GetBytes(randomNumber.ToString()));
            string inputMessage = "Test Message";
            UTF8Encoding utf8enc = new UTF8Encoding();
            // Converting the string message to byte array
            byte[] inputBytes = utf8enc.GetBytes(inputMessage);
            // RSAKeyPairGenerator generates the RSA Key pair based on the random number and strength of key required
            RsaKeyPairGenerator rsaKeyPairGnr = new RsaKeyPairGenerator();
            rsaKeyPairGnr.Init(new Org.BouncyCastle.Crypto.KeyGenerationParameters(new SecureRandom(), 1024));
            Org.BouncyCastle.Crypto.AsymmetricCipherKeyPair keyPair = rsaKeyPairGnr.GenerateKeyPair();
            RsaKeyParameters publicKey = (RsaKeyParameters)keyPair.Public;
            RsaKeyParameters privateKey = (RsaKeyParameters)keyPair.Private;
            IAsymmetricBlockCipher cipher = new OaepEncoding(new RsaEngine(), new Sha256Digest(), encodingParam);
            cipher.Init(true, publicKey);
            byte[] ciphered = cipher.ProcessBlock(inputBytes, 0, inputMessage.Length);
            string cipheredText = utf8enc.GetString(ciphered);
            // Decryption steps --------------------------------------------
            cipher.Init(false, privateKey);
            byte[] deciphered = cipher.ProcessBlock(ciphered, 0, ciphered.Length);
            string decipheredText = utf8enc.GetString(deciphered);
            //---------------------------------------------------------------

        }


        public static void testa()
        {
            byte[] pass = Encoding.UTF8.GetBytes("pass");



        }





    }
}
