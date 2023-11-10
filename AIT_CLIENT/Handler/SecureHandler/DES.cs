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
            return string.Empty;
        }

        public string Decrypt(string text) 
        {
            return string.Empty;
        }
    }
}
