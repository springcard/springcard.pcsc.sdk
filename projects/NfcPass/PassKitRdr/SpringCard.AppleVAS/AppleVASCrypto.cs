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

namespace SpringCard.AppleVas
{
    public static class AppleVasCrypto
    {
        public static byte[] AES_GCM_Decrypt(byte[] keyValue, byte[] cipherText, byte[] additionalAuthenticationData)
        {
            Logger.Debug("Decrypting...");
            Logger.Debug("AES Key={0}", BinConvert.ToHex(keyValue));
            Logger.Debug("Cipher={0}", BinConvert.ToHex(cipherText));
            Logger.Debug("AAD={0}", BinConvert.ToHex(additionalAuthenticationData));

            GcmBlockCipher cipher = new GcmBlockCipher(new AesEngine(), new BasicGcmMultiplier());
            KeyParameter keyParameter = ParameterUtilities.CreateKeyParameter("AES", keyValue);
            byte[] IV = new byte[16];
            AeadParameters parameters = new AeadParameters(keyParameter, 128, IV, additionalAuthenticationData);
            cipher.Init(false, parameters);

            if (additionalAuthenticationData != null)
                cipher.ProcessAadBytes(additionalAuthenticationData, 0, additionalAuthenticationData.Length);

            byte[] plainText = new byte[cipher.GetOutputSize(cipherText.Length)];
            Logger.Debug("Expected length of plain={0}", plainText.Length);

            try
            {
                int rc = cipher.ProcessBytes(cipherText, 0, cipherText.Length, plainText, 0);
                Logger.Debug("ProcessBytes rc={0}", rc);
                rc = cipher.DoFinal(plainText, rc);
            }
            catch (Exception e)
            {
                Logger.Debug("AES-{0}-GCM exception: {1}", keyValue.Length * 8, e.Message);
                Logger.Trace("AES-{0}-GCM failed; message is invalid", keyValue.Length * 8);
                return null;
            }

            Logger.Debug("Plain={0}", BinConvert.ToHex(plainText));
            return plainText;
        }

        public static byte[] SHA256(byte[] buffer)
        {
            Sha256Digest digest = new Sha256Digest();
            if (buffer != null)
                digest.BlockUpdate(buffer, 0, buffer.Length);
            byte[] result = new byte[digest.GetDigestSize()];
            digest.DoFinal(result, 0);
            return result;
        }

        public static class ECC
        {
            public const string Curve = "secp256r1";
            private const string Algorithm = "ECDH";

            public static ECPublicKeyParameters ImportPublicKey(byte[] encoded, bool expand = false)
            {
                Logger.Debug("ImportPublicKey");
                Logger.Debug("\tEncoded value: " + BinConvert.ToHex(encoded));

                if (expand)
                {
                    byte[] t = new byte[encoded.Length + 1];
                    t[0] = 0x02;
                    Array.Copy(encoded, 0, t, 1, encoded.Length);
                    encoded = t;

                    Logger.Debug("\tExpanded: " + BinConvert.ToHex(encoded));
                }

                X9ECParameters curve = SecNamedCurves.GetByName(Curve);
                if (curve == null)
                {
                    Logger.Debug("\tUnknown curve");
                    return null;
                }

                ECDomainParameters curveSpec = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H, curve.GetSeed());
                ECPublicKeyParameters result = new ECPublicKeyParameters(Algorithm, curve.Curve.DecodePoint(encoded), curveSpec);

                if (result != null)
                {
                    Logger.Debug("Public key imported:");
                    DumpPublic(result);
                }

                return result;
            }

            public static ECPublicKeyParameters ImportPublicKeyFromPem(string pemFile)
            {
                var fileStream = System.IO.File.OpenText(pemFile);
                var pemReader = new Org.BouncyCastle.OpenSsl.PemReader(fileStream);
                var keyParameter = (Org.BouncyCastle.Crypto.AsymmetricKeyParameter)pemReader.ReadObject();

                return (ECPublicKeyParameters)keyParameter;
            }

            public static uint ComputeKeyIdFromPem(string pemFile)
            {
                ECPublicKeyParameters ecPubKey = ImportPublicKeyFromPem(pemFile);
                return ComputeKeyId(ecPubKey);
            }

            public static ECPrivateKeyParameters ImportPrivateKey(byte[] encoded)
            {
                Logger.Debug("ImportPrivateKey");
                Logger.Debug("\tEncoded value: " + BinConvert.ToHex(encoded));

                byte[] privateKeyValue;

                if ((encoded.Length == 32) || (encoded.Length == 33))
                {
                    privateKeyValue = encoded;
                }
                else
                {
                    /* Must be a PEM-encode key */
                    if (encoded.Length < (7 + 32)) /* MBA instead of if (encoded.Length < 7 + 32) */
                    {
                        Logger.Debug("\tToo short");
                        return null;
                    }

                    /* TODO : decode PEM correctly */
                    byte[] expectedPemHeader = new byte[] { 0x30, 0x77, 0x02, 0x01, 0x01, 0x04, 0x20, 0x2A };
                    if (!BinUtils.Equals(expectedPemHeader, encoded, 7))
                    {
                        Logger.Debug("\tUnsupported PEM header");
                        return null;
                    }

                    privateKeyValue = BinUtils.Copy(encoded, 7, 32);
                }

                X9ECParameters curve = SecNamedCurves.GetByName(Curve);
                if (curve == null)
                {
                    Logger.Debug("\tUnknown curve");
                    return null;
                }

                if ((privateKeyValue[0] & 0x80) != 0)
                {
                    /* Force a positive big int */
                    privateKeyValue = BinUtils.Concat(0x00, privateKeyValue);
                }

                Logger.Debug("\tRaw number: " + BinConvert.ToHex(privateKeyValue));

                ECDomainParameters curveSpec = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H, curve.GetSeed());
                ECPrivateKeyParameters result = new ECPrivateKeyParameters(Algorithm, new BigInteger(privateKeyValue), curveSpec);

                if (result != null)
                {
                    Logger.Debug("Private key imported, public key is:");
                    DumpPublic(ExtractPublicKey(result));
                }

                return result;
            }

            public static byte[] EncodePrivate(ECPrivateKeyParameters privateKey)
            {
                return privateKey.D.ToByteArrayUnsigned();
            }

            public static string EncodePrivatePem(ECPrivateKeyParameters privateKey, int lineLength = 0)
            {
                byte[] data = new byte[0];
                data = BinUtils.Concat(data, BinConvert.HexToBytes("30770201010420"));
                data = BinUtils.Concat(data, privateKey.D.ToByteArrayUnsigned());
                data = BinUtils.Concat(data, BinConvert.HexToBytes("A00A06082A8648CE3D030107A144034200"));
                data = BinUtils.Concat(data, ExtractPublicKey(privateKey).Q.GetEncoded());

                string pem = "";

                pem += StrUtils.Base64Encode(data, lineLength);

                return pem;
            }

            public static uint ComputeKeyId(ECPrivateKeyParameters privateKey)
            {
                ECPublicKeyParameters publicKey = ExtractPublicKey(privateKey);
                return ComputeKeyId(publicKey);
            }

            public static uint ComputeKeyId(ECPublicKeyParameters publicKey)
            {
                byte[] X = publicKey.Q.XCoord.GetEncoded();
                byte[] H = SHA256(X);
                uint result = BinUtils.BytesToUInt32(H);
                return result;
            }

            public static ECPublicKeyParameters ExtractPublicKey(ECPrivateKeyParameters key)
            {
                Logger.Debug("Using key passed to the library");
                X9ECParameters curve = SecNamedCurves.GetByName(Curve);
                ECDomainParameters curveSpec = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H, curve.GetSeed());
                ECPoint Q = curve.G.Multiply(key.D);
                return new ECPublicKeyParameters(Algorithm, Q, curveSpec);
            }

            public static bool ExtractPublicKey(byte privateKeyIndex, ICardApduTransmitter cardChannel, out ECPublicKeyParameters publicKey)
            {
                Logger.Debug("Using key inside the secure element of the device");
                CAPDU capdu = new CAPDU(0xFF, 0xF9, 0xE3, privateKeyIndex, "", 0x00);
                RAPDU rapdu = cardChannel.Transmit(capdu);

                if (rapdu.SW != 0x9000)
                {
                    Logger.Error($"Failed to get public key from the secure element, sw = {BinConvert.ToHex(rapdu.SW)}");
                    publicKey = null;
                    return false;
                }

                List<byte> rawPubKey = new List<byte>(rapdu.data.GetBytes());

                publicKey = ECC.ImportPublicKey(rawPubKey.GetRange(0, 32).ToArray(), true);
                if (publicKey == null)
                {
                    Logger.Error("Unable to extract public key");
                    return false;
                }
                return true;
            }

            public static byte[] EncodePublic(ECPublicKeyParameters key)
            {
                byte[] result = new byte[1];
                if (key.Q.YCoord.TestBitZero())
                    result[0] = 3;
                else
                    result[0] = 2;
                result = BinUtils.Concat(result, key.Q.XCoord.GetEncoded());
                return result;
            }

            public static void DumpPublic(ECPublicKeyParameters publicKey)
            {
                byte[] X = publicKey.Q.XCoord.GetEncoded();
                Logger.Debug("\tX: {0}", BinConvert.ToHex(X));
                byte[] Y = publicKey.Q.YCoord.GetEncoded();
                Logger.Debug("\tY: {0}", BinConvert.ToHex(Y));

                byte[] encoded = EncodePublic(publicKey);               
                Logger.Debug("\t(raw, hex)   : {0}", BinConvert.ToHex(encoded));

                byte[] pem = AppleVasConfig.EncodePublicPem(encoded);
                Logger.Debug("\t(pem, hex)   : {0}", BinConvert.ToHex(pem));
                Logger.Debug("\t(pem, base64): {0}", StrUtils.Base64Encode(pem));

                Logger.Debug("\tId: {0}", BinConvert.ToHex(ComputeKeyId(publicKey)));
            }

            public static byte[] Derive(ECPrivateKeyParameters privateKey, ECPublicKeyParameters publicKey)
            {
                if (privateKey == null)
                    Logger.Error("PrivateKey is Null");
                if (publicKey == null)
                    Logger.Error("PublicKey is Null");

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

            public static byte[] Derive(byte privateKeyIndex, ICardApduTransmitter cardChannel, ECPublicKeyParameters publicKey)
            {
                Logger.Debug("Using key inside the secure element of the device");
                CAPDU capdu = new CAPDU(0xFF, 0xF9, 0xE8, privateKeyIndex, publicKey.Q.XCoord.GetEncoded(), 0x00);
                RAPDU rapdu = cardChannel.Transmit(capdu);

                if (rapdu == null)
                {
                    Logger.Error($"Failed to compute ECDH with the secure element, RAPDU is null");
                    return null;
                }

                if (rapdu.SW != 0x9000)
                {
                    Logger.Error($"Failed to compute ECDH with the secure element, sw = {BinConvert.ToHex(rapdu.SW)}");
                    return null;
                }

                if (rapdu == null || rapdu.SW != 0x9000)
                {
                    Logger.Error($"Failed to compute ECDH with the secure element, sw = {BinConvert.ToHex(rapdu.SW)}");
                    return null;
                }
                return rapdu.data.GetBytes();
            }
        }

        public static class KDF
        {
            private static byte[] Counter(int iteration)
            {
                byte[] result = new byte[4];

                result[0] = (byte)((iteration >> 24) & 0x0FF);
                result[1] = (byte)((iteration >> 16) & 0x0FF);
                result[2] = (byte)((iteration >> 8) & 0x0FF);
                result[3] = (byte)((iteration) & 0x0FF);

                return result;
            }

            public static byte[] ANSI_X963_SHA256(byte[] Z, byte[] SharedInfo, int KeyLen)
            {
                Sha256Digest digest = new Sha256Digest();
                int iterations = KeyLen / digest.GetDigestSize();
                byte[] counter;
                byte[] result = new byte[0];

                for (int i = 0; i <= iterations; i++)
                {
                    counter = Counter(i + 1);

                    digest.Reset();
                    digest.BlockUpdate(Z, 0, Z.Length);
                    digest.BlockUpdate(counter, 0, counter.Length);
                    if (SharedInfo != null)
                        digest.BlockUpdate(SharedInfo, 0, SharedInfo.Length);
                    byte[] hash = new byte[digest.GetDigestSize()];
                    digest.DoFinal(hash, 0);

                    result = BinUtils.Concat(result, hash);
                }

                Logger.Debug("ANSI_X963_SHA256 SharedSecret={0}", BinConvert.ToHex(result));
                result = BinUtils.EnsureSize(result, KeyLen, KeyLen);
                return result;
            }
        }
    }
}
/**
 * \endcond
 */
