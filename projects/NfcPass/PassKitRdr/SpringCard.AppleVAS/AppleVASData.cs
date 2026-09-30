/**
 *
 * \ingroup AppleVAS
 * 
 */
using Org.BouncyCastle.Crypto.Parameters;
using SpringCard.LibCs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpringCard.AppleVas
{
    /**
	 * \brief Encapsulate an Apple VAS message, as it is transmitted by the NFC-enabled iPhone or iPad
	 **/
    public class AppleVasData
    {
        /**
		 * \brief Default width (expressed in seconds) of the acceptable timestamp window. See IsTimestampAllowed()
		 **/
        public const int DefaultTimestampDelta = 100;
        private AppleVasConfig config;
        private byte[] rawMessage;
        private DateTime responseToVasDataWhen;
        private byte[] responseToVasDataMessage;

        private AppleVasData()
        {
        }

        /**
		 * \brief Try to decode a Message from the raw message provided by the iPhone or iPad
		 **/
        public static AppleVasData Create(AppleVasConfig config, byte[] rawMessage, out AppleVasError error)
        {
            AppleVasData result = new AppleVasData();
            result.config = config;
            result.rawMessage = rawMessage;
            if (!result.Decrypt(out error))
                return null;
            return result;
        }

        internal static byte[] CreateSharedInfo(int version, byte[] merchantId)
        {
            byte[] result = null;

            if (version == 1)
            {
                const string VasSharedInfo1 = "ApplePay encrypted VAS data";
                result = StrUtils.ToBytes(VasSharedInfo1);
            }
            else if (version == 2)
            {
                const byte VasSharedInfo2_0 = 0x0D;
                const string VasSharedInfo2_1 = "id-aes256-GCM";
                const string VasSharedInfo2_2 = "ApplePay encrypted VAS data";

                byte[] part1 = StrUtils.ToBytes(VasSharedInfo2_1);
                Logger.Debug("Part1={0}", BinConvert.ToHex(part1));

                byte[] part2 = StrUtils.ToBytes(VasSharedInfo2_2);
                Logger.Debug("Part2={0}", BinConvert.ToHex(part2));

                Logger.Debug("MerchantID={0}", BinConvert.ToHex(merchantId));

                result = new byte[1 + part1.Length + part2.Length + merchantId.Length];
                result[0] = VasSharedInfo2_0;

                Array.Copy(part1, 0, result, 1, part1.Length);
                Array.Copy(part2, 0, result, 1 + part1.Length, part2.Length);
                Array.Copy(merchantId, 0, result, 1 + part1.Length + part2.Length, merchantId.Length);
            }

            return result;
        }

        internal static byte[] CreateAdditionalAuthenticationData(int version, byte[] merchantId)
        {
            byte[] result = null;

            if (version == 1)
            {
                result = merchantId;
            }
            else if (version == 2)
            {

            }

            return result;
        }

        private static byte[] DecryptCryptogram(PrivateKey merchantPrivateKey, byte[] mobilePublicKey, byte[] mobileCryptogram, byte[] sharedInfo1, byte[] sharedInfo2, out AppleVasError error)
        {
            if (merchantPrivateKey == null)
            {
                Logger.Fatal("Merchant's PrivatKey is null");
            }

            ECPublicKeyParameters publicKey = AppleVasCrypto.ECC.ImportPublicKey(mobilePublicKey, true);
            if (publicKey == null)
            {
                Logger.Error("PublicKey received from mobile is invalid");
                error = AppleVasError.NoMobilePublicKey;
                return null;
            }

            byte[] derivedKey;
            if (merchantPrivateKey is PrivateKeySoftware)
            {
                PrivateKeySoftware l_privKey = (merchantPrivateKey as PrivateKeySoftware);
                if (!l_privKey.IsInitialized())
                {
                    Logger.Fatal("Private key (soft) not initialized");
                }
                Logger.Debug("Using software library to derive");
                derivedKey = AppleVasCrypto.ECC.Derive(l_privKey.PrivatePart, publicKey);
                if (derivedKey == null)
                {
                    error = AppleVasError.KeyDerivationFailed;
                    return null;
                }
                Logger.Debug("Key derivation OK");
            }
            else if (merchantPrivateKey is PrivateKeySecureElement)
            {
                PrivateKeySecureElement l_privKey = (merchantPrivateKey as PrivateKeySecureElement);
                if (!l_privKey.IsInitialized())
                {
                    Logger.Fatal("Private key (SE) not initialized");
                }
                Logger.Debug("Using secure element to derive");
                derivedKey = AppleVasCrypto.ECC.Derive(l_privKey.KeyIndex, l_privKey.CardChannel, publicKey);
                if (derivedKey == null)
                {
                    error = AppleVasError.KeyDerivationFailed;
                    return null;
                }
                Logger.Debug("Key derivation OK");
            }
            else
            {
                Logger.Error("Error, wrong, merchantPrivateKey type");
                error = AppleVasError.WrongParameter;
                return null;
            }

            byte[] aesKey = AppleVasCrypto.KDF.ANSI_X963_SHA256(derivedKey, sharedInfo1, 32);
            if (aesKey == null)
            {
                error = AppleVasError.KeyDerivationFailed;
                return null;
            }

            byte[] cipherText = mobileCryptogram;

            byte[] plainText = AppleVasCrypto.AES_GCM_Decrypt(aesKey, cipherText, sharedInfo2);
            if (plainText == null)
            {
                error = AppleVasError.MessageDecipherFailed;
                return null;
            }

            error = AppleVasError.Success;
            return plainText;
        }

        private static byte[] DecryptCryptogram(int version, byte[] merchantId, PrivateKey merchantPrivateKey, byte[] mobilePublicKey, byte[] mobileCryptogram, out AppleVasError error)
        {
            Logger.Debug("DecryptCryptogram using version={0}", version);

            byte[] sharedInfo = CreateSharedInfo(version, merchantId);
            Logger.Debug("\tSharedInfo={0}", BinConvert.ToHex(sharedInfo));

            byte[] additionalAuthenticationData = CreateAdditionalAuthenticationData(version, merchantId);
            Logger.Debug("\tAdditionalAuthenticationData={0}", BinConvert.ToHex(additionalAuthenticationData));

            byte[] result = DecryptCryptogram(merchantPrivateKey, mobilePublicKey, mobileCryptogram, sharedInfo, additionalAuthenticationData, out error);
            if (result != null)
            {
                error = AppleVasError.Success;
                Logger.Debug("Success!");
                Logger.Debug("PlainMessage={0}", BinConvert.ToHex(result));
            }

            return result;
        }

        internal static byte[] DecryptCryptogram(byte[] merchantId, PrivateKey merchantPrivateKey, byte[] mobileMessageCrypted, out AppleVasError error)
        {
            Logger.Debug("DecryptCryptogram");

            Logger.Debug("\tMerchantID={0}", BinConvert.ToHex(merchantId));

            Logger.Debug("\tCryptedMessage={0}", BinConvert.ToHex(mobileMessageCrypted));

            byte[] mobilePublicKey = BinUtils.Copy(mobileMessageCrypted, 0, 32);
            Logger.Debug("\tMobilePublicKey={0}", BinConvert.ToHex(mobilePublicKey));

            byte[] mobileCryptogram = BinUtils.Copy(mobileMessageCrypted, 32, mobileMessageCrypted.Length - 32);
            Logger.Debug("\tMobileCryptogram={0}", BinConvert.ToHex(mobileCryptogram));

            byte[] result;

            result = DecryptCryptogram(2, merchantId, merchantPrivateKey, mobilePublicKey, mobileCryptogram, out error);
            if (result == null)
                result = DecryptCryptogram(1, merchantId, merchantPrivateKey, mobilePublicKey, mobileCryptogram, out error);

            return result;
        }

        private bool Decrypt(out AppleVasError error)
        {
            Logger.Debug("Decrypting Apple VAS message");
            Logger.Debug("\tMessage=" + BinConvert.ToHex(rawMessage));

            uint merchantPublicKeyId = BinUtils.BytesToUInt32(rawMessage);
            Logger.Debug("\tMerchantPublicKeyId=" + BinConvert.ToHex(merchantPublicKeyId));

            PrivateKey merchantPrivateKey = config.GetPrivateKey(merchantPublicKeyId);
            if (merchantPrivateKey == null)
            {
                Logger.Debug("The merchant does not know this PublicKeyId");
                error = AppleVasError.UnknownMerchantPublicKeyId;
                return false;
            }

            byte[] mobileCrytogram = BinUtils.Copy(rawMessage, 4);
            Logger.Debug("MobileCrytogram=" + BinConvert.ToHex(mobileCrytogram));

            byte[] mobileMessagePlain = DecryptCryptogram(config.Id, merchantPrivateKey, mobileCrytogram, out error);
            if (mobileMessagePlain == null)
                return false;

            Logger.Debug("AppleVAS:MobileVasData:Plain={0}", BinConvert.ToHex(mobileCrytogram));

            if (!Decode(mobileMessagePlain, out responseToVasDataWhen, out responseToVasDataMessage))
                return false;

            return true;
        }

        /**
		 * \brief Provides the timestamp of the message, i.e. the date/time (in UTC) it has been created on the iPhone or iPad
		 **/
        public DateTime Timestamp
        {
            get
            {
                return responseToVasDataWhen;
            }
        }

        /**
		 * \brief Check whether the delta between the computer's current date/time and the timestamp of the message is within an acceptable range. The delta is expressed in seconds.
		 **/
        public bool IsTimestampAllowed(int delta = DefaultTimestampDelta)
        {
            return IsTimestampAllowed(responseToVasDataWhen, DateTime.UtcNow, delta);
        }

        /**
		 * \brief Check whether the delta between the specified date/time and the timestamp of the message is within an acceptable range. The delta is expressed in seconds.
		 **/
        public bool IsTimestampAllowed(DateTime reference, int delta = DefaultTimestampDelta)
        {
            return IsTimestampAllowed(responseToVasDataWhen, reference, delta);
        }

        /**
		 * \brief Raw content of the message provided by the iPhone or iPad
		 **/
        public byte[] Bytes
        {
            get
            {
                return responseToVasDataMessage;
            }
        }

        /**
		 * \brief Raw content of the message provided by the iPhone or iPad, expressed in hex
		 **/
        public string HexBytes
        {
            get
            {
                return BinConvert.ToHex(responseToVasDataMessage);
            }
        }

        /**
		 * \brief Raw content of the message provided by the iPhone or iPad, expressed in base64
		 **/
        public string Base64Bytes
        {
            get
            {
                return StrUtils.Base64UrlEncode(responseToVasDataMessage);
            }
        }

        /**
		 * \brief Text translation of the message provided by the iPhone or iPad
		 **/
        public string Text
        {
            get
            {
                return StrUtils.ToStr_UTF8(responseToVasDataMessage);
            }
        }

        /**
		 * \brief Is the timestam allowed ?
		 **/
        public static bool IsTimestampAllowed(DateTime when, DateTime now, int delta = DefaultTimestampDelta)
        {
            DateTime whenMin = now.AddSeconds(0 - delta);
            DateTime whenMax = now.AddSeconds(delta);

            bool result = ((when >= whenMin) && (when <= whenMax)) ? true : false;
            Logger.Debug("AppleVAS:Is {0} between {1} and {2}? {3}", when, whenMin, whenMax, result ? "yes" : "no");
            return result;
        }

        internal static bool Decode(byte[] decrypted, out DateTime when, out byte[] message)
        {
            when = new DateTime(2001, 1, 1);
            message = new byte[0];

            if ((decrypted == null) || (decrypted.Length < 4))
                return false;

            uint t = 0;
            for (int i = 0; i < 4; i++)
            {
                t <<= 8;
                t |= decrypted[i];
            }

            when = when.AddSeconds(t);
            message = BinUtils.Copy(decrypted, 4, decrypted.Length - 4);

            Logger.Debug("Apple VAS message is OK");
            Logger.Debug("\tTimeStamp={0}", when);
            Logger.Debug("\tMessage(hex)={0}", BinConvert.ToHex(message));
            Logger.Debug("\tMessage(string)={0}", StrUtils.ToStr_UTF8(message));

            return true;
        }
    }
}
