/**
 *
 * \ingroup GoogleVAS
 *
 */
using Org.BouncyCastle.Crypto.Parameters;
using SpringCard.LibCs;
using SpringCard.PCSC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpringCard.GoogleVas
{
    public partial class GoogleVasTerminal
    {
        private byte[] CreateMessageForSignature()
        {
            byte[] result = new byte[0];
            byte[] mobileDeviceNonce = new byte[0];
            if (config.PresignedAuth)
            {
                mobileDeviceNonce = Enumerable.Repeat((byte)0x00, 32).ToArray();
            }
            else
            {
                mobileDeviceNonce = session.GetMobileDeviceNonce();
            }
           
            Logger.Debug("Creating message for signature");

            Logger.Debug("\tTerminalNonce:              {0}", BinConvert.ToHex(session.TerminalNonce));
            result = BinUtils.Concat(result, session.TerminalNonce);
            Logger.Debug("\tMobileDeviceNonce:          {0}", BinConvert.ToHex(mobileDeviceNonce));
            result = BinUtils.Concat(result, mobileDeviceNonce);
            Logger.Debug("\tCollectorId (4):            {0} ({1})", BinConvert.ToHex(config.CollectorId_4), BinUtils.ToDword(config.CollectorId_4));
            result = BinUtils.Concat(result, config.CollectorId_4);
            Logger.Debug("\tTerminalEphemeralPublicKey: {0}", BinConvert.ToHex(session.TerminalEphemeralPublicKey));
            result = BinUtils.Concat(result, session.TerminalEphemeralPublicKey);

            Logger.Debug("Data to be signed:\n\t{0}", BinConvert.ToHex(result));

            return result;
        }

        private void ComputeSignature(ICardApduTransmitter cardChannel)
        {
            byte[] message = CreateMessageForSignature();
            if(config.UseSecureElement)
            {
                Logger.Debug("Using key inside the secure element of the device");
                CAPDU capdu = new CAPDU(0xFF, 0xF9, 0xE7, config.LongTermPrivateKeyIndex, message, 0x00);
                RAPDU rapdu = cardChannel.Transmit(capdu);

                if (rapdu.SW == 0x9000)
                {
                    session.Signature = rapdu.data.GetBytes();
                }
                else
                {
                    Logger.Error($"Failed to compute signature with the secure element, sw = {BinConvert.ToHex(rapdu.SW)}");
                }
            }
            else
            {
                Logger.Debug("Using key passed to the library");
                session.Signature = GoogleVasCrypto.ECC.Sign(config.TerminalLongTermKey, message);
            }
            
            Logger.Debug("Signature:\n\t{0}", BinConvert.ToHex(session.Signature));

#if false // optionnal, just used for tests purposes

            if (GoogleVasCrypto.ECC.Verify(GoogleVasCrypto.ECC.ExtractPublicKey(config.TerminalLongTermKey), message, session.Signature))
            {
                Logger.Debug("Signature is OK");
            }
            else
            {
                Logger.Debug("Signature is not correct");
            }
#endif
        }

        private void ComputeSessionKeys()
        {
            ECPublicKeyParameters mobilePubKey = GoogleVasCrypto.ECC.ImportPublicKey(session.MobileDeviceEphemeralPublicKey);
            session.SharedSecret = GoogleVasCrypto.ECC.Derive(session.TerminalEphemeralKey, mobilePubKey);
            Logger.Debug("Shared Secret={0}", BinConvert.ToHex(session.SharedSecret));

            byte[] salt = session.MobileDeviceEphemeralPublicKey;
            Logger.Debug("Salt={0}", BinConvert.ToHex(salt));

            byte[] info = new byte[0];
            info = BinUtils.Concat(info, session.TerminalNonce);
            info = BinUtils.Concat(info, session.GetMobileDeviceNonce());
            info = BinUtils.Concat(info, config.CollectorId_4);
            info = BinUtils.Concat(info, session.TerminalEphemeralPublicKey);
            info = BinUtils.Concat(info, session.Signature);

            Logger.Debug("Info={0}", BinConvert.ToHex(info));

            byte[] key = GoogleVasCrypto.HKDF.DeriveKey(salt, session.SharedSecret, info, 48);
            Logger.Debug("Shared Key={0}", BinConvert.ToHex(key));

            session.AesKey = BinUtils.Copy(key, 0, 16);
            Logger.Debug("AES Key={0}", BinConvert.ToHex(session.AesKey));
            session.HmacKey = BinUtils.Copy(key, 16, 32);
            Logger.Debug("HMAC Key={0}", BinConvert.ToHex(session.HmacKey));

            session.SessionKeyOK = true;
        }

        private byte[] DecryptPayload(byte[] payload)
        {
            byte[] iv = BinUtils.Copy(payload, 0, 12);
            Logger.Debug("IV={0}", BinConvert.ToHex(iv));
            byte[] cipher = BinUtils.Copy(payload, 12, -32);
            Logger.Debug("Cipher={0}", BinConvert.ToHex(cipher));
            byte[] hmac = BinUtils.Copy(payload, -32);
            Logger.Debug("HMAC={0}", BinConvert.ToHex(hmac));

            payload = BinUtils.Copy(payload, 0, -32);
            byte[] computedHmac = GoogleVasCrypto.HMAC_SHA_256(session.HmacKey, payload);

            if (!BinUtils.Equals(computedHmac, hmac))
            {
                Logger.Debug("HMAC is invalid");
                return null;
            }

            return GoogleVasCrypto.AES_CTR_Decrypt(session.AesKey, cipher, iv);
        }
    }
}
