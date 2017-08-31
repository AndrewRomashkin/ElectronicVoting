using Common.DataStructures;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Common
{
    public static class Utils
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

        public static EncryptedData EncryptSymmetrically(this object obj, byte[] key)
        {
            return EncryptedData.Encrypt(obj.Serialize(), key);
        }

        public static EncryptedData EncryptSymmetrically(this object obj, string key)
        {
            return EncryptedData.Encrypt(obj.Serialize(), GenerateKey(key));
        }

        public static EncryptedData EncryptSymmetrically(this byte[] data, string key)
        {
            return EncryptedData.Encrypt(data, GenerateKey(key));
        }

        public static byte[] Hash(this byte[] data)
        {
            IDigest hash = Constants.hashFunction;
            byte[] result = new byte[hash.GetDigestSize()];
            hash.BlockUpdate(data, 0, data.Length);
            hash.DoFinal(result, 0);
            return result;
        }

        public static byte[] Hash(this object obj)
        {
            return obj.Serialize().Hash();
        }

        public static byte[] GenerateKey(string str)
        {
            return str.Hash();
        }

        //public static string RSAPrivateKeyToBase64(AsymmetricKeyParameter key)
        //{
        //    return new String(Encoding.ASCII.GetChars(Base64.Encode(PrivateKeyInfoFactory.CreatePrivateKeyInfo(key).ToAsn1Object().GetDerEncoded()))) + "==";
        //}

        //public static string RSAPublicKeyToBase64(AsymmetricKeyParameter key)
        //{
        //    return new String(Encoding.ASCII.GetChars(Base64.Encode(SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(key).ToAsn1Object().GetDerEncoded())));
        //}

        public static string KeyPairToPem(AsymmetricCipherKeyPair keyPair)
        {
            TextWriter textWriter = new StringWriter();
            PemWriter pemWriter = new PemWriter(textWriter);
            pemWriter.WriteObject(keyPair);
            pemWriter.Writer.Flush();
            return textWriter.ToString();
        }

        public static AsymmetricCipherKeyPair PemToKeyPair(string keyPair)
        {
            TextReader textReader = new StringReader(keyPair);
            PemReader pemReader = new PemReader(textReader);
            Object o;
            while ((o = pemReader.ReadObject()) != null)
                if (o is AsymmetricCipherKeyPair)
                    return o as AsymmetricCipherKeyPair;
            return null;
        }

        public static string RsaKeyToPem(AsymmetricKeyParameter key)
        {
            TextWriter textWriter = new StringWriter();
            PemWriter pemWriter = new PemWriter(textWriter);
            pemWriter.WriteObject(key);
            pemWriter.Writer.Flush();
            return textWriter.ToString();
        }

        public static AsymmetricKeyParameter[] PemToRsaKey(string key)
        {
            TextReader textReader = new StringReader(key);
            PemReader pemReader = new PemReader(textReader);
            List<AsymmetricKeyParameter> result = new List<AsymmetricKeyParameter>();
            Object o;
            while ((o = pemReader.ReadObject()) != null)
            {
                if (o is AsymmetricKeyParameter)
                    result.Add(o as AsymmetricKeyParameter);
                if (o is AsymmetricCipherKeyPair)
                {
                    result.Add((o as AsymmetricCipherKeyPair).Private);
                    result.Add((o as AsymmetricCipherKeyPair).Public);
                }
            }
            return result.ToArray();
        }

        public static byte[] RsaEncrypt(byte[] data, AsymmetricKeyParameter key)
        {
            return Rsa(data, key, true);
        }

        public static byte[] RsaDecrypt(byte[] data, AsymmetricKeyParameter key)
        {
            return Rsa(data, key, false);
        }

        static byte[] Rsa(byte[] data, AsymmetricKeyParameter key, bool encrypt)
        {
            var encryptEngine = new RsaEngine();
            encryptEngine.Init(encrypt, key);
            return encryptEngine.ProcessBlock(data, 0, data.Length);
        }

        public static BigInteger GenerateBlindingFactor(RsaKeyParameters signersPublicKey)
        {
            RsaBlindingFactorGenerator blindFactorGen = new RsaBlindingFactorGenerator();
            blindFactorGen.Init(signersPublicKey);
            return blindFactorGen.GenerateBlindingFactor();
        }

        public static byte[] BlindData(byte[] data, BigInteger blindingFactor, RsaKeyParameters signersPublicKey)
        {
            PssSigner blindSigner = new PssSigner(new RsaBlindingEngine(), Constants.signingHashFunction, 20);
            blindSigner.Init(true, new ParametersWithRandom(new RsaBlindingParameters(signersPublicKey, blindingFactor), new SecureRandom()));
            blindSigner.BlockUpdate(data, 0, data.Length);
            return blindSigner.GenerateSignature();
        }

        public static byte[] UnblindData(byte[] data, BigInteger blindingFactor, RsaKeyParameters signersPublicKey)
        {
            RsaBlindingEngine blindingEngine = new RsaBlindingEngine();
            blindingEngine.Init(false, new RsaBlindingParameters(signersPublicKey, blindingFactor));
            return blindingEngine.ProcessBlock(data, 0, data.Length);
        }

        public static byte[] Sign(byte[] data, AsymmetricKeyParameter privateKey)
        {
            PssSigner signer = new PssSigner(new RsaEngine(), Constants.signingHashFunction, 20);
            signer.Init(true, privateKey);
            signer.BlockUpdate(data, 0, data.Length);
            return signer.GenerateSignature();
        }

        public static bool VerifySignature(byte[] signature, byte[] data, AsymmetricKeyParameter publicKey)
        {
            PssSigner signer = new PssSigner(new RsaEngine(), Constants.signingHashFunction, 20);
            signer.Init(false, publicKey);
            signer.BlockUpdate(data, 0, data.Length);
            return signer.VerifySignature(signature);
        }

        public static string GenerateCSR(AsymmetricCipherKeyPair keyPair, string name)
        {
            IDictionary attrs = new Hashtable();

            attrs.Add(X509Name.GivenName, name);

            X509Name subject = new X509Name(new ArrayList(attrs.Keys), attrs);

            TextWriter textWriter = new StringWriter();
            PemWriter pemWriter = new PemWriter(textWriter);
#pragma warning disable CS0618 // Тип или член устарел
            Pkcs10CertificationRequest first = new Pkcs10CertificationRequest("SHA512WITHRSA", subject, keyPair.Public, null, keyPair.Private);
            pemWriter.WriteObject(new Pkcs10CertificationRequest(first.GetEncoded()));
#pragma warning restore CS0618 // Тип или член устарел
            pemWriter.Writer.Flush();
            return textWriter.ToString();
        }

        public static void SaveUserInfo(UserInfo userInfo)
        {
            File.WriteAllBytes("users/"+userInfo.Username+".usr", new EncryptedUserInfo(userInfo).Serialize());
        }
    }

}
