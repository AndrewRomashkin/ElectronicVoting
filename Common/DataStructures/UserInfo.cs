using Org.BouncyCastle.Crypto;
using System;
using System.Runtime.Serialization;

namespace Common.DataStructures
{
    [Serializable]
    public class UserInfo
    {
        public String Username;
        string PersonalKeyPairAsPem;
        public byte[] Signature;
        public string Password;
        [NonSerialized]
        public AsymmetricCipherKeyPair PersonalKeyPair;

        [OnSerializing()]
        internal void OnSerializing(StreamingContext context)
        {
            PersonalKeyPairAsPem = Utils.KeyPairToPem(PersonalKeyPair);
        }

        [OnDeserialized()]
        internal void OnDeserializing(StreamingContext context)
        {
            PersonalKeyPair = Utils.PemToKeyPair(PersonalKeyPairAsPem);
            PersonalKeyPairAsPem = null;
        }
    }
}
