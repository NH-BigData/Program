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
            var encryptObject = new DES(key);
            return encryptObject.Encrypt(targetString);
        }

        public string GetDecryptString(string targetString)
        {
            return GetDecryptString(key, targetString);
        }

        private string GetDecryptString(byte[] key, string targetString)
        {
            var decryptObject = new DES(key);
            return decryptObject.Decrypt(targetString);
        }
        
    }
}
