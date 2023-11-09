using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Handler.SecureHandler
{   
    public class SecureObject
    {
        private readonly byte[] key;

        public SecureObject()
        {
            key = new[]
            {
                (byte) 'H',
                (byte) 'O',
                (byte) 'S',
                (byte) 'E',
                (byte) 'O',
                (byte) 'N',
                (byte) 'G',
                (byte) '!'
            };
        }

        public string GetEncryptString(string targetString)
        {
            return GetEncryptString(key, targetString);
        }

        private string GetEncryptString(byte[] key, string targetString)
        {

            return string.Empty;
        }

    }
}
