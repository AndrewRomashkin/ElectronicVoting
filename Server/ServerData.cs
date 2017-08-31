using Org.BouncyCastle.Crypto;
using System;
using System.Runtime.Serialization;
using Common;
using System.IO;

namespace Server
{
    [Serializable]
    class ServerData
    {
        [NonSerialized]
        public AsymmetricCipherKeyPair ServerKeyPair;
        string ServerKeyPairAsPem;

        public AsymmetricCipherKeyPair PersonalKeyPair;

        [OnSerializing()]
        internal void OnSerializing(StreamingContext context)
        {
            ServerKeyPairAsPem = Utils.KeyPairToPem(ServerKeyPair);
        }

        [OnDeserialized()]
        internal void OnDeserializing(StreamingContext context)
        {
            ServerKeyPair = Utils.PemToKeyPair(ServerKeyPairAsPem);
        }

        public void Save()
        {
            File.WriteAllBytes("state.bin", this.Serialize());
        }

        public static ServerData Load()
        {
            try
            {
                return File.ReadAllBytes("state.bin").Deserialize<ServerData>();
            }
            catch
            {
                return null;
            }
        }
    }
}
