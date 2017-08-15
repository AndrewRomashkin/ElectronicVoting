using System;
using System.IO;
using System.Security.Cryptography;

namespace Client.DataStructures
{
    [Serializable]
    class EncryptedUserInfo
    {
        public string Name;
        public byte[] EncryptedData;
        public byte[] PasswordHash;
        public byte[] IV;
        public string Salt;

        public static EncryptedUserInfo Load(string filename)
        {
            try
            {
                using (FileStream stream = new FileStream(filename, FileMode.Open))
                {
                    return stream.Deserialize<EncryptedUserInfo>();
                }
            }
            catch
            {
                return null;
            }
        }

        public EncryptedUserInfo(string name, string password)
        {
            Name = name;
            byte[] iv = null;
            EncryptedData = new UserInfo().Serialize().Encrypt(password, ref iv);
            SHA512Managed sha = new SHA512Managed();
        }
    }
}
