using System;
using System.IO;
using System.Text;
using System.Linq;

namespace Common.DataStructures
{
    [Serializable]
    public class EncryptedUserInfo
    {
        public string Name;
        public EncryptedData EncryptedUserData;
        public byte[] PasswordHash;
        public string Salt;

        public static EncryptedUserInfo Load(string filename)
        {
            try
            {
                return File.ReadAllBytes(filename).Deserialize<EncryptedUserInfo>();
            }
            catch
            {
                return null;
            }
        }

        public EncryptedUserInfo(UserInfo userInfo)
        {
            Name = userInfo.Username;
            EncryptedUserData = userInfo.EncryptSymmetrically(userInfo.Password);
            byte[] saltBytes = new byte[32];
            Constants.rng.GetBytes(saltBytes);
            Salt = new string(Encoding.ASCII.GetChars(saltBytes));
            PasswordHash = (userInfo.Password + Salt).Hash();
        }

        public bool CheckPassword(string password)
        {
            return Utils.Hash(password + Salt).SequenceEqual(PasswordHash);
        }
    }
}
