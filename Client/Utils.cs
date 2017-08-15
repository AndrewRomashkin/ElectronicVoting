using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;

namespace Client
{
    static class Utils
    {
        public static byte[] Serialize(this object obj)
        {
            try
            {
                if (obj == null)
                    return null;
                BinaryFormatter bf = new BinaryFormatter();
                using (MemoryStream ms = new MemoryStream())
                {
                    bf.Serialize(ms, obj);
                    return ms.ToArray();
                }
            }
            catch { return null; }
        }

        public static T Deserialize<T>(this Stream stream)
        {
            try
            {
                BinaryFormatter formatter = new BinaryFormatter();
                object deserialized = formatter.Deserialize(stream);
                if (deserialized is T)
                    return (T)deserialized;
                else
                    return default(T);
            }
            catch
            {
                return default(T);
            }
        }

        public static T Deserialize<T>(this byte[] array)
        {
            return new MemoryStream(array).Deserialize<T>();
        }

        public static byte[] Encrypt(this byte[] data, string password, ref byte[] iv)
        {
            try
            {
                using (AesManaged aes = new AesManaged())
                {
                    aes.Key = Encoding.UTF8.GetBytes(password);
                    if (iv == null)
                    {
                        aes.GenerateIV();
                        iv = aes.IV;
                    }
                    else
                        aes.IV = iv;
                    aes.Mode = CipherMode.CBC;
                    using (ICryptoTransform encryptor = aes.CreateEncryptor())
                    {
                        using (MemoryStream msEncrypt = new MemoryStream())
                        {
                            using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                            {
                                using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                                {
                                    swEncrypt.Write(data);
                                }
                                return msEncrypt.ToArray();
                            }
                        }
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        public static byte[] Decrypt(this byte[] data, string password, byte[] iv)
        {
            try
            {
                using (AesManaged aes = new AesManaged())
                {
                    aes.Key = Encoding.UTF8.GetBytes(password);
                    aes.Mode = CipherMode.CBC;
                    aes.IV = iv;
                    using (ICryptoTransform decryptor = aes.CreateDecryptor())
                    {
                        using (MemoryStream msDecrypt = new MemoryStream())
                        {
                            using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Write))
                            {
                                using (StreamWriter swDecrypt = new StreamWriter(csDecrypt))
                                {
                                    swDecrypt.Write(data);
                                }
                                return msDecrypt.ToArray();
                            }
                        }
                    }
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
