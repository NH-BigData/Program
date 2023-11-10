using System;
using System.IO;
using System.Security.Cryptography;

namespace Handler.SecureHandler
{
    internal class DES
    {
        private readonly byte[] key;

        public DES(byte[] key)
        {
            this.key = key;
        }

        public string Encrypt(string text)
        {
            if (string.IsNullOrEmpty(text) || key.Length != 8) return string.Empty;

            var provider = new DESCryptoServiceProvider();
            var memoryStream = new MemoryStream();
            var cryptoStream = new CryptoStream(memoryStream, provider.CreateEncryptor(key, key), CryptoStreamMode.Write);
            var streamWriter = new StreamWriter(cryptoStream);
            streamWriter.Write(text);
            streamWriter.Flush();
            cryptoStream.FlushFinalBlock();
            streamWriter.Flush();
            return Convert.ToBase64String(memoryStream.GetBuffer(), 0, (int) memoryStream.Length);
        }

        public string Decrypt(string text) 
        {
            if (string.IsNullOrEmpty(text) || key.Length != 8) return string.Empty;

            var provider = new DESCryptoServiceProvider();
            var memoryStream = new MemoryStream(Convert.FromBase64String(text));
            var cryptoStream = new CryptoStream(memoryStream, provider.CreateDecryptor(key, key), CryptoStreamMode.Read);
            var streamReader = new StreamReader(cryptoStream);

            return streamReader.ReadToEnd();
        }
    }
}
