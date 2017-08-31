using System;
using Org.BouncyCastle.Crypto.Paddings;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Engines;

namespace Common.DataStructures
{
    [Serializable]
    public class EncryptedData
    {
        public byte[] Data;
        public byte[] Iv;

        public EncryptedData(byte[] data, byte[] iv)
        {
            Data = data;
            Iv = iv;
        }

        public EncryptedData()
        {

        }

        public static EncryptedData Encrypt(byte[] data, byte[] symmetricKey)
        {
            EncryptedData result = new EncryptedData();
            IBlockCipher engine = Constants.symmetricCipher;
            result.Iv = new byte[engine.GetBlockSize()];
            Constants.rng.GetBytes(result.Iv);
            PaddedBufferedBlockCipher cipher = new PaddedBufferedBlockCipher(new CbcBlockCipher(engine));
            ICipherParameters parameters = new ParametersWithIV(new KeyParameter(symmetricKey), result.Iv);
            cipher.Init(true, parameters);
            result.Data = CipherData(cipher, data);
            return result;
        }

        public byte[] Decrypt(byte[] symmetricKey)
        {
            PaddedBufferedBlockCipher cipher = new PaddedBufferedBlockCipher(new CbcBlockCipher(Constants.symmetricCipher));
            ICipherParameters parameters = new ParametersWithIV(new KeyParameter(symmetricKey), Iv);
            cipher.Init(false, parameters);
            return CipherData(cipher, Data);
        }

        public byte[] Decrypt(string symmetricKey)
        {
            return Decrypt(Utils.GenerateKey(symmetricKey));
        }

        public T Decrypt<T>(string symmetricKey)
        {
            return Decrypt(symmetricKey).Deserialize<T>();
        }

        public T Decrypt<T>(byte[] symmetricKey)
        {
            return Decrypt(symmetricKey).Deserialize<T>();
        }

            private static byte[] CipherData(PaddedBufferedBlockCipher cipher, byte[] data)
        {
            int minSize = cipher.GetOutputSize(data.Length);
            byte[] outBuf = new byte[minSize];
            int length1 = cipher.ProcessBytes(data, 0, data.Length, outBuf, 0);
            int length2 = cipher.DoFinal(outBuf, length1);
            int actualLength = length1 + length2;
            byte[] result = new byte[actualLength];
            Array.Copy(outBuf, 0, result, 0, result.Length);
            return result;
        }
    }
}
