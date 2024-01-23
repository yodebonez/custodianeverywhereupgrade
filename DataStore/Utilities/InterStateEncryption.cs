using NLog;
using System;
using System.IO;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace DataStore.Utilities
{
    public static class InterStateEncryption
    {
        private static int PROVIDER_RSA_FULL = 1;
        private static string KEY_CONTAINER_NAME = "FolioAPIKeyContainer";
        private static int KEY_SIZE = 2048;
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        // private static string IMPORT_FOLDER = @"C:\Users\oitaba\Desktop\icon";
        private static string KEY_FILE = @"CustodianKey.xml";
        public static string GetSignature(string text, string pubKeyPath = "")
        {
            //GenerateKeyPair(pubKeyPath);
            byte[] signatureBytes = SignText(text);
            if (signatureBytes != null)
            {
                return Convert.ToBase64String(signatureBytes);
            }
            return null;
        }

        public static bool VerifySignature(string text, string signature)
        {
            byte[] textBytes = Encoding.UTF8.GetBytes(text);
            byte[] signatureBytes = Convert.FromBase64String(signature);
            return VerifySignedText(textBytes, signatureBytes);
        }

        public static byte[] SignText(string text)
        {
            try
            {
                RSACryptoServiceProvider rsa = GetRSACryptoServiceProviderFromContainer();
                // Hash and sign the text. Pass a new instance of SHA512
                // to specify the hashing algorithm.
                return rsa.SignData(Encoding.UTF8.GetBytes(text), SHA512.Create());
            }
            catch (CryptographicException e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        public static bool VerifySignedText(byte[] text, byte[] signature)
        {
            try
            {
                RSACryptoServiceProvider rsa = GetRSACryptoServiceProviderFromContainer();
                // Verify the signed text using the signature. Pass a new instance of SHA512
                // to specify the hashing algorithm.
                return rsa.VerifyData(text, SHA512.Create(), signature);
            }
            catch (CryptographicException e)
            {
                // Console.WriteLine(e.Message);
                return false;
            }
        }
        public static dynamic GenerateKeys()
        {
            CspParameters cspParams = new CspParameters(PROVIDER_RSA_FULL);
            cspParams.KeyContainerName = KEY_CONTAINER_NAME;
            cspParams.Flags = CspProviderFlags.UseMachineKeyStore;
            cspParams.ProviderName = "Microsoft Strong Cryptographic Provider";
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(KEY_SIZE, cspParams);
            // Console.WriteLine("The RSA key pair with a key size of {0} bits was added to the container: \"{1}\".", rsa.KeySize, KEY_CONTAINER_NAME);
            // Important! The key pair needs to be loaded from the key container
            // for the correct key information that was stored to be displayed.
            // rsa.ImportParameters();
            //string privateKey = System.IO.File.ReadAllText(pubKeyPath);
            //rsa.FromXmlString(privateKey.Trim());
            rsa = GetRSACryptoServiceProviderFromContainer();
            return new
            {
                publickey = rsa.ToXmlString(false),
                privatekey = rsa.ToXmlString(true)
            };
            //var pub_key = rsa.ToXmlString(false); // export public key
            //var priv_key = rsa.ToXmlString(true); // export private key
            // Display the key information to the console.
            //Console.WriteLine($"Key information retrieved from container : \n {rsa.ToXmlString(true)}");
        }

        private static void GenerateKeyPair(string pubKeyPath)
        {

            CspParameters cspParams = new CspParameters(PROVIDER_RSA_FULL);
            cspParams.KeyContainerName = KEY_CONTAINER_NAME;
            cspParams.Flags = CspProviderFlags.UseMachineKeyStore;
            cspParams.ProviderName = "Microsoft Strong Cryptographic Provider";
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(KEY_SIZE, cspParams);
            CryptoKeyAccessRule rule = new CryptoKeyAccessRule("everyone", CryptoKeyRights.FullControl, AccessControlType.Allow);
            cspParams.CryptoKeySecurity = new CryptoKeySecurity();
            cspParams.CryptoKeySecurity.SetAccessRule(rule);
            // Console.WriteLine("The RSA key pair with a key size of {0} bits was added to the container: \"{1}\".", rsa.KeySize, KEY_CONTAINER_NAME);
            // Important! The key pair needs to be loaded from the key container
            // for the correct key information that was stored to be displayed.
            // rsa.ImportParameters();
            string privateKey = System.IO.File.ReadAllText(pubKeyPath);
            rsa.FromXmlString(privateKey.Trim());
            //rsa = GetRSACryptoServiceProviderFromContainer();
            //var pub_key = rsa.ToXmlString(false); // export public key
            //var priv_key = rsa.ToXmlString(true); // export private key
            // Display the key information to the console.
            //Console.WriteLine($"Key information retrieved from container : \n {rsa.ToXmlString(true)}");
        }
        public static void DeleteKeyPairFromContainer()
        {
            RSACryptoServiceProvider rsa = GetRSACryptoServiceProviderFromContainer();
            rsa.PersistKeyInCsp = false;
            // Call Clear to release resources and delete the key from the container.
            rsa.Clear();
            //Console.WriteLine("The RSA key pair was deleted from the container: \"{0}\".", KEY_CONTAINER_NAME);
        }
        public static RSACryptoServiceProvider GetRSACryptoServiceProviderFromContainer()
        {
            // Create the CspParameters object and set the key container
            // name used to store the RSA key pair.
            CspParameters cspParams = new CspParameters();
            cspParams.KeyContainerName = KEY_CONTAINER_NAME;
            // Create a new instance of RSACryptoServiceProvider that accesses
            // the key container.
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(KEY_SIZE, cspParams);
            return rsa;
        }

        public static void ImportKeyPairIntoContainer()
        {
            string path = HttpContext.Current.Server.MapPath("~/Cert");
            FileInfo fi = new FileInfo(Path.Combine(path, KEY_FILE));
            log.Info($"file path: {fi.FullName}");
            if (fi.Exists)
            {
                using (StreamReader reader = new StreamReader(Path.Combine(path, KEY_FILE)))
                {
                    RSACryptoServiceProvider rsa = GetRSACryptoServiceProviderFromContainer();
                    string keyText = reader.ReadToEnd();
                    rsa.FromXmlString(keyText);
                    rsa.PersistKeyInCsp = true;
                    //Console.WriteLine("The RSA key pair from \"{0}\\{1}\" was imported into the container: \"{2}\".", IMPORT_FOLDER, KEY_FILE, KEY_CONTAINER_NAME);
                }
            }
        }
        public static dynamic GetRSACryptoServiceProviderFromContainerString()
        {
            // Create the CspParameters object and set the key container
            // name used to store the RSA key pair.
            CspParameters cspParams = new CspParameters();
            cspParams.KeyContainerName = KEY_CONTAINER_NAME;
            // Create a new instance of RSACryptoServiceProvider that accesses
            // the key container.
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(KEY_SIZE, cspParams);

            return new
            {
                key = rsa.ToXmlString(true),
                containerName = rsa.CspKeyContainerInfo.KeyContainerName,
                uniqueName = rsa.CspKeyContainerInfo.UniqueKeyContainerName
            };
        }
    }
}
