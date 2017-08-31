using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Engines;
using System.Security.Cryptography;
using System;

namespace Common
{
    public static class Constants
    {
        public static RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();

        public static IDigest hashFunction
        {
            get
            {
                return new KeccakDigest(256);
            }
        }

        public static IDigest signingHashFunction
        {
            get
            {
                return new KeccakDigest(288);
            }
        }

        public static IBlockCipher symmetricCipher
        {
            get
            {
                return new AesEngine();
            }
        }
    }
}
