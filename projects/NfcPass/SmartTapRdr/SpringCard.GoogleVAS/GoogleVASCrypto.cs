/**
 * \cond
 */
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.IO;
using Org.BouncyCastle;
using SpringCard.PCSC;
using SpringCard.LibCs;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Crypto.Agreement.Kdf;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Agreement;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Math.EC;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Modes.Gcm;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Asn1.Ocsp;
using Org.BouncyCastle.Utilities.IO.Pem;
using Org.BouncyCastle.Crypto.Macs;

namespace SpringCard.GoogleVas
{
    public static class GoogleVasCrypto
    {
        internal static byte[] AES_CTR_Decrypt(byte[] keyValue, byte[] cipherText, byte[] iv)
        {
            Logger.Debug("Decrypting...");
            Logger.Debug("\tAES Key: {0}", BinConvert.ToHex(keyValue));
            Logger.Debug("\tIV     : {0}", BinConvert.ToHex(iv));
            Logger.Debug("\tCipher : {0}", BinConvert.ToHex(cipherText));

            IBufferedCipher cipher = CipherUtilities.GetCipher("AES/CTR/NoPadding");
            cipher.Init(false, new ParametersWithIV(ParameterUtilities.CreateKeyParameter("AES", keyValue), iv));
            byte[] plainText = cipher.DoFinal(cipherText);

            Logger.Debug("\tPlain  : {0}", BinConvert.ToHex(plainText));
            return plainText;
        }

        internal static byte[] SHA256(byte[] buffer)
        {
            Sha256Digest digest = new Sha256Digest();
            if (buffer != null)
                digest.BlockUpdate(buffer, 0, buffer.Length);
            byte[] result = new byte[digest.GetDigestSize()];
            digest.DoFinal(result, 0);
            return result;
        }

        internal static byte[] HMAC_SHA_256(byte[] key, byte[] message)
        {
            if (key == null) throw new ArgumentNullException("key");
            if (message == null) throw new ArgumentNullException("message");

            HMac hmac;

            hmac = new HMac(new Sha256Digest());
            hmac.Init(new KeyParameter(key));

            byte[] result = new byte[hmac.GetMacSize()];
            hmac.BlockUpdate(message, 0, message.Length);
            hmac.DoFinal(result, 0);

            return result;
        }

        internal static class HKDF
        {
            public static byte[] Extract(byte[] salt, byte[] inputKeyMaterial)
            {
                return HMAC_SHA_256(salt, inputKeyMaterial);
            }

            public static byte[] Expand(byte[] prk, byte[] info, int outputLength)
            {
                byte[] resultBlock = new byte[0];
                byte[] result = new byte[outputLength];
                int bytesRemaining = outputLength;

                for (int i = 1; bytesRemaining > 0; i++)
                {
                    var currentInfo = new byte[resultBlock.Length + info.Length + 1];
                    Array.Copy(resultBlock, 0, currentInfo, 0, resultBlock.Length);
                    Array.Copy(info, 0, currentInfo, resultBlock.Length, info.Length);
                    currentInfo[currentInfo.Length - 1] = (byte)i;
                    resultBlock = HMAC_SHA_256(prk, currentInfo);
                    Array.Copy(resultBlock, 0, result, outputLength - bytesRemaining, Math.Min(resultBlock.Length, bytesRemaining));
                    bytesRemaining -= resultBlock.Length;
                }
                return result;
            }

            public static byte[] DeriveKey(byte[] salt, byte[] inputKeyMaterial, byte[] info, int outputLength)
            {
                byte[] prk = Extract(salt, inputKeyMaterial);
                byte[] result = Expand(prk, info, outputLength);
                return result;
            }

            public static bool SelfTest(bool throwException = true)
            {
                {
                    var ikm = BinConvert.HexToBytes("0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b");
                    var salt = BinConvert.HexToBytes("000102030405060708090a0b0c");
                    var info = BinConvert.HexToBytes("f0f1f2f3f4f5f6f7f8f9");
                    var L = 42;

                    var prk = "077709362c2e32df0ddc3f0dc47bba6390b6c73bb50f9c3122ec844ad7c2b3e5";
                    var okm = "3cb25f25faacd57a90434f64d0362f2a2d2d0a90cf1a5a4c5db02d56ecc4c5bf34007208d5b887185865";

                    var actualPrk = BinConvert.tohex(Extract(salt, ikm));
                    var actualOkm = BinConvert.tohex(DeriveKey(salt, ikm, info, L));

                    if (prk != actualPrk)
                    {
                        if (throwException)
                            throw new Exception();
                        return false;
                    }

                    if (okm != actualOkm)
                    {
                        if (throwException)
                            throw new Exception();
                        return false;
                    }
                }

                {
                    var ikm = BinConvert.HexToBytes("000102030405060708090a0b0c0d0e0f" +
                                                "101112131415161718191a1b1c1d1e1f" +
                                                "202122232425262728292a2b2c2d2e2f" +
                                                "303132333435363738393a3b3c3d3e3f" +
                                                "404142434445464748494a4b4c4d4e4f");
                    var salt = BinConvert.HexToBytes("606162636465666768696a6b6c6d6e6f" +
                                                "707172737475767778797a7b7c7d7e7f" +
                                                "808182838485868788898a8b8c8d8e8f" +
                                                "909192939495969798999a9b9c9d9e9f" +
                                                "a0a1a2a3a4a5a6a7a8a9aaabacadaeaf");
                    var info = BinConvert.HexToBytes("b0b1b2b3b4b5b6b7b8b9babbbcbdbebf" +
                                                 "c0c1c2c3c4c5c6c7c8c9cacbcccdcecf" +
                                                 "d0d1d2d3d4d5d6d7d8d9dadbdcdddedf" +
                                                 "e0e1e2e3e4e5e6e7e8e9eaebecedeeef" +
                                                 "f0f1f2f3f4f5f6f7f8f9fafbfcfdfeff");
                    var L = 82;

                    var prk = "06a6b88c5853361a06104c9ceb35b45cef760014904671014a193f40c15fc244";
                    var okm = "b11e398dc80327a1c8e7f78c596a4934" +
                              "4f012eda2d4efad8a050cc4c19afa97c" +
                              "59045a99cac7827271cb41c65e590e09" +
                              "da3275600c2f09b8367793a9aca3db71" +
                              "cc30c58179ec3e87c14c01d5c1f3434f" +
                              "1d87";

                    var actualPrk = BinConvert.tohex(Extract(salt, ikm));
                    var actualOkm = BinConvert.tohex(DeriveKey(salt, ikm, info, L));

                    if (prk != actualPrk)
                    {
                        if (throwException)
                            throw new Exception();
                        return false;
                    }

                    if (okm != actualOkm)
                    {
                        if (throwException)
                            throw new Exception();
                        return false;
                    }
                }

                {
                    var ikm = BinConvert.HexToBytes("0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b");
                    var salt = new byte[0];
                    var info = new byte[0];
                    var L = 42;

                    var prk = "19ef24a32c717b167f33a91d6f648bdf96596776afdb6377ac434c1c293ccb04";
                    var okm = "8da4e775a563c18f715f802a063c5a31" +
                              "b8a11f5c5ee1879ec3454e5f3c738d2d" +
                              "9d201395faa4b61a96c8";

                    var actualPrk = BinConvert.tohex(Extract(salt, ikm));
                    var actualOkm = BinConvert.tohex(DeriveKey(salt, ikm, info, L));

                    if (prk != actualPrk)
                    {
                        if (throwException)
                            throw new Exception();
                        return false;
                    }

                    if (okm != actualOkm)
                    {
                        if (throwException)
                            throw new Exception();
                        return false;
                    }
                }

                return true;
            }
        }

        public static class ECC
        {
            public const string Curve = "secp256r1";
            private const string Algorithm = "ECDSA";

            public static ECPublicKeyParameters ImportPublicKey(byte[] encoded, bool expand = false)
            {
                if (expand)
                {
                    byte[] t = new byte[encoded.Length + 1];
                    t[0] = 0x02;
                    Array.Copy(encoded, 0, t, 1, encoded.Length);
                    encoded = t;
                }

                Logger.Debug("ImportPublicKey");
                Logger.Debug("\t" + BinConvert.ToHex(encoded));

                X9ECParameters curve = SecNamedCurves.GetByName(Curve);
                if (curve == null)
                {
                    Logger.Debug("\tUnknown curve");
                    return null;
                }

                ECDomainParameters curveSpec = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H, curve.GetSeed());
                ECPublicKeyParameters result = new ECPublicKeyParameters(Algorithm, curve.Curve.DecodePoint(encoded), curveSpec);

                if (result != null)
                    Logger.Debug("\tPublic key imported");

                return result;
            }

            public static ECPrivateKeyParameters ImportPrivateKey(byte[] encoded)
            {
                encoded = BinUtils.Concat(0x00, encoded);

                Logger.Debug("ImportPrivateKey");
                Logger.Debug("\t" + BinConvert.ToHex(encoded));

                X9ECParameters curve = SecNamedCurves.GetByName(Curve);
                if (curve == null)
                {
                    Logger.Debug("\tUnknown curve");
                    return null;
                }

                ECDomainParameters curveSpec = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H, curve.GetSeed());
                ECPrivateKeyParameters result = new ECPrivateKeyParameters(Algorithm, new BigInteger(encoded), curveSpec);

                if (result != null)
                    Logger.Debug("\tPrivate key imported");

                Logger.Debug("\t\tPri={0}", BinConvert.ToHex(result.D.ToByteArrayUnsigned()));

                return result;
            }

            public static ECPublicKeyParameters ExtractPublicKey(ECPrivateKeyParameters key)
            {
                X9ECParameters curve = SecNamedCurves.GetByName(Curve);
                ECDomainParameters curveSpec = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H, curve.GetSeed());
                ECPoint Q = curve.G.Multiply(key.D);
                ECPublicKeyParameters result = new ECPublicKeyParameters(Algorithm, Q, curveSpec);
                Logger.Debug("\t\tPub={0}", BinConvert.ToHex(Q.GetEncoded()));
                return result;
            }

            public static byte[] EncodePublic(ECPublicKeyParameters key, bool compressed)
            {
                return key.Q.GetEncoded(compressed);
            }

            public static string EncodePublicPem(ECPublicKeyParameters key, int lineLength = 0, bool addMarkers = false)
            {
                byte[] data = new byte[0];
                data = BinUtils.Concat(data, BinConvert.HexToBytes("3059301306072A8648CE3D020106082A8648CE3D030107034200"));
                data = BinUtils.Concat(data, key.Q.GetEncoded(false));

                string pem = "";

                if (addMarkers)
                    pem += "-----BEGIN PUBLIC KEY----- \n";

                pem += StrUtils.Base64Encode(data, lineLength);

                if (addMarkers)
                    pem += " \n-----END PUBLIC KEY-----\n";

                pem = pem.Replace("\n\n", "\n");

                return pem;
            }

            public static bool Verify(ECPublicKeyParameters key, byte[] message, byte[] signature)
            {
                ISigner ecdsaSign = SignerUtilities.GetSigner("SHA256withECDSA");

                ecdsaSign.Init(false, key);

                ecdsaSign.BlockUpdate(message, 0, message.Length);

                return ecdsaSign.VerifySignature(signature);
            }

            public static byte[] Sign(ECPrivateKeyParameters key, byte[] message)
            {
                ISigner ecdsaSign = SignerUtilities.GetSigner("SHA256withECDSA");

                ecdsaSign.Init(true, key);

                ecdsaSign.BlockUpdate(message, 0, message.Length);

                byte[] buffer = ecdsaSign.GenerateSignature();


                /* If msb is 1 (0x80) it considered as a negative number, to avoid that we add 0x00 byte inn the front */

                byte topLevelTag = buffer[0];
                byte topLevelLength = buffer[1];

                byte part1Tag = buffer[2];
                byte part1Length = buffer[3];
                byte[] part1Value = BinUtils.Copy(buffer, 4, part1Length);

                byte part2Tag = buffer[4 + part1Length];
                byte part2Length = buffer[5 + part1Length];
                byte[] part2Value = BinUtils.Copy(buffer, 6 + part1Length, part2Length);

                if ((part1Value[0] & 0x80) != 0) part1Value = BinUtils.Concat(0x00, part1Value);
                if ((part2Value[0] & 0x80) != 0) part2Value = BinUtils.Concat(0x00, part2Value);

                byte[] result = new byte[2 + 2 + part1Value.Length + 2 + part2Value.Length];

                result[0] = topLevelTag;
                result[1] = (byte) (2 + part1Value.Length + 2 + part2Value.Length);

                result[2] = part1Tag;
                result[3] = (byte)part1Value.Length;
                Array.Copy(part1Value, 0, result, 4, part1Value.Length);

                result[4 + part1Value.Length] = part2Tag;
                result[5 + part1Value.Length] = (byte)part2Value.Length;
                Array.Copy(part2Value, 0, result, 6 + part1Value.Length, part2Value.Length);

                return result;
            }

            public static byte[] Derive(ECPrivateKeyParameters privateKey, ECPublicKeyParameters publicKey)
            {
                ECDHBasicAgreement agreement = new ECDHBasicAgreement();
                agreement.Init(privateKey);
                byte[] result = agreement.CalculateAgreement(publicKey).ToByteArray();
                if (result != null)
                {
                    if (result.Length > 32)
                        result = BinUtils.Copy(result, result.Length - 32, 32);
                    Logger.Debug("SharedKey={0}", BinConvert.ToHex(result));
                }
                return result;
            }


            public static void SavePublicKey(ECPublicKeyParameters publicKey, string fileName)
            {
                string pem = EncodePublicPem(publicKey, 64, true);
                File.WriteAllText(fileName, pem);
            }

            public static void SavePrivateKey(ECPrivateKeyParameters privateKey, string fileName)
            {
                byte[] data = new byte[0];
                data = BinUtils.Concat(data, BinConvert.HexToBytes("30770201010420"));
                data = BinUtils.Concat(data, privateKey.D.ToByteArrayUnsigned());
                data = BinUtils.Concat(data, BinConvert.HexToBytes("A00A06082A8648CE3D030107A144034200"));
                data = BinUtils.Concat(data, ExtractPublicKey(privateKey).Q.GetEncoded());

                string pem = "";
                pem += "-----BEGIN EC PARAMETERS-----\n";
                pem += "BggqhkjOPQMBBw==\n";
                pem += "-----END EC PARAMETERS-----\n";
                pem += "-----BEGIN EC PRIVATE KEY-----\n";
                pem += StrUtils.Base64Encode(data, 64);
                pem += "\n";
                pem += "-----END EC PRIVATE KEY-----\n";
                File.WriteAllText(fileName, pem);
            }
        }
    }
}
/**
 * \endcond
 */
