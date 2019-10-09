/**h* SpringCard/PCSC_CcidOver
 *
 **/
using System;
using System.Diagnostics;
using System.Threading;
using SpringCard.PCSC;
using SpringCard.LibCs;
using System.Security.Cryptography;

namespace SpringCard.PCSC.ZeroDriver
{
    public abstract partial class SCardReaderList_CcidOver : SCardReaderList
    {
        public static bool DebugSecureConnection = false;

        protected SecureConnectionParameters.CommunicationMode SecureCommMode = SecureConnectionParameters.CommunicationMode.Plain;

        private const byte ProtocolCode = 0x00;

        private byte[] SessionEncKey;
        private byte[] SessionMacKey;
        private byte[] SessionSendIV;
        private byte[] SessionRecvIV;

        private enum ProtocolOpcode : byte
        {
            Success = 0x00,
            Authenticate = 0x0A,
            Following = 0xFF
        }

        public class SecureConnectionParameters
        {
            public enum AuthenticationMode
            {
                None,
                Aes128
            }
            public AuthenticationMode AuthMode = AuthenticationMode.None;

            public enum AuthenticationKeyIndex
            {
                None,
                User,
                Admin
            }
            public AuthenticationKeyIndex KeyIndex = AuthenticationKeyIndex.None;

            public byte[] KeyValue;

            public enum CommunicationMode
            {
                Plain,
                MACed,
                Secure
            }
            public CommunicationMode CommMode = CommunicationMode.Plain;
        }

        protected bool OpenSecureConnection()
        {
            if (secureConnectionParameters == null)
                return false;

            switch (secureConnectionParameters.AuthMode)
            {
                case SecureConnectionParameters.AuthenticationMode.None:
                    return true;

                case SecureConnectionParameters.AuthenticationMode.Aes128:
                    return OpenSecureConnectionAes128();

                default:
                    break;
            }

            return false;
        }

        protected byte[] DecryptCcidBuffer(byte[] ccid_buffer)
        {
            /* Extract the CMAC */
            byte[] received_cmac = BinUtils.Copy(ccid_buffer, -8);
            ccid_buffer = BinUtils.Copy(ccid_buffer, 0, -8);

            /* Extract the data */
            byte[] data = BinUtils.Copy(ccid_buffer, 10);

            if (DebugSecureConnection)
                Logger.Debug("   >     (crypted data) {0}", BinConvert.ToHex(data));

            /* Decipher the data */
            data = AesCbcDecrypt(SessionEncKey, SessionRecvIV, data);

            if (DebugSecureConnection)
                Logger.Debug("   >      (padded data) {0}", BinConvert.ToHex(data));

            int data_len = data.Length;
            while ((data_len > 0) && (data[data_len - 1] == 0x00))
                data_len -= 1;
            if ((data_len == 0) || (data[data_len - 1] != 0x80))
            {
                Logger.Debug("Padding is invalid (decryption failed/wrong session key?)");
                return null;
            }
            data_len -= 1;
            data = BinUtils.Copy(data, 0, data_len);

            if (DebugSecureConnection)
                Logger.Debug("   >       (plain data) {0}", BinConvert.ToHex(data));

            /* Extract the header and re-create a valid buffer */
            ccid_buffer = BinUtils.Copy(ccid_buffer, 0, 10);
            PC_to_RDR_SetLength(ccid_buffer, data.Length, false);
            ccid_buffer = BinUtils.Concat(ccid_buffer, data);

            /* Compute the CMAC */
            byte[] computed_cmac = ComputeCmac(SessionMacKey, SessionRecvIV, ccid_buffer);

            if (DebugSecureConnection)
                Logger.Debug("   >{0} -> CMAC={1}", BinConvert.ToHex(ccid_buffer), BinConvert.ToHex(computed_cmac, 8));

            if (!BinUtils.Equals(received_cmac, computed_cmac, 8))
            {
                Logger.Debug("CMAC is invalid (wrong session key?)");
                return null;
            }

            SessionRecvIV = computed_cmac;

            return ccid_buffer;
        }

        protected byte[] EncryptCcidBuffer(byte[] ccid_buffer)
        {
            /* Compute the CMAC of the plain buffer */
            byte[] cmac = ComputeCmac(SessionMacKey, SessionSendIV, ccid_buffer);

            if (DebugSecureConnection)
                Logger.Debug("   <{0} -> CMAC={1}", BinConvert.ToHex(ccid_buffer), BinConvert.ToHex(cmac, 8));

            /* Extract the data */
            byte[] data = BinUtils.Copy(ccid_buffer, 10);

            if (DebugSecureConnection)
                Logger.Debug("   <       (plain data) {0}", BinConvert.ToHex(data));

            /* Cipher the data */
            data = BinUtils.Concat(data, 0x80);
            while ((data.Length % 16) != 0) data = BinUtils.Concat(data, 0x00);

            if (DebugSecureConnection)
                Logger.Debug("   <      (padded data) {0}", BinConvert.ToHex(data));

            data = AesCbcEncrypt(SessionEncKey, SessionSendIV, data);

            if (DebugSecureConnection)
                Logger.Debug("   <     (crypted data) {0}", BinConvert.ToHex(data));

            /* Update the length */
            PC_to_RDR_SetLength(ccid_buffer, data.Length + 8, true);

            /* Re-create the buffer */
            ccid_buffer = BinUtils.Copy(ccid_buffer, 0, 10);
            ccid_buffer = BinUtils.Concat(ccid_buffer, data);
            ccid_buffer = BinUtils.Concat(ccid_buffer, BinUtils.Copy(cmac, 0, 8));

            SessionSendIV = cmac;

            return ccid_buffer;
        }

        private bool OpenSecureConnectionAes128()
        {
            switch (secureConnectionParameters.KeyIndex)
            {
                case SecureConnectionParameters.AuthenticationKeyIndex.None:
                    break;

                case SecureConnectionParameters.AuthenticationKeyIndex.User:
                    if (OpenSecureConnectionAes128(0, secureConnectionParameters.KeyValue))
                    {
                        SecureCommMode = secureConnectionParameters.CommMode;
                        return true;
                    }
                    break;

                case SecureConnectionParameters.AuthenticationKeyIndex.Admin:
                    if (OpenSecureConnectionAes128(1, secureConnectionParameters.KeyValue))
                    {
                        SecureCommMode = secureConnectionParameters.CommMode;
                        return true;
                    }
                    break;

                default:
                    break;
            }

            return false;
        }


        private byte[] GetRandom(int length)
        {
            byte[] result = new byte[length];
            if (DebugSecureConnection)
            {
                for (int i=0; i<length; i++)
                {
                    result[i] = (byte)(0xA0 | (i & 0x0F));
                }
            }
            else
            {
                RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
                rng.GetBytes(result);
            }
            return result;
        }

        private byte[] ComputeCmac(byte[] key, byte[] iv, byte[] buffer)
        {
            byte[] cmac;
            int actual_length;
            int i, j;

            if (iv != null)
                cmac = iv;
            else
                cmac = new byte[16];

            if (DebugSecureConnection)
                Logger.Debug("Compute CMAC");

            actual_length = buffer.Length + 1;
            while ((actual_length % 16) != 0) actual_length++;

            for (i = 0; i < actual_length; i += 16)
            {
                byte[] block = new byte[16];

                for (j = 0; j < 16; j++)
                {
                    if ((i + j) < buffer.Length)
                    {
                        block[j] = buffer[i + j];
                    }
                    else if ((i + j) == buffer.Length)
                    {
                        block[j] = 0x80;
                    }
                    else
                    {
                        block[j] = 0x00;
                    }
                }

                if (DebugSecureConnection)
                    Logger.Debug("\tBlock={0}, IV={1}", BinConvert.ToHex(block), BinConvert.ToHex(cmac));

                for (j = 0; j < 16; j++)
                    block[j] ^= cmac[j];

                cmac = AesEcbEncrypt(key, block);

                if (DebugSecureConnection)
                    Logger.Debug("\t\t-> {0}", BinConvert.ToHex(cmac));
            }

            return cmac;
        }

        private byte[] AesCbcEncrypt(byte[] key, byte[] iv, byte[] buffer)
        {
            if (iv == null)
                iv = new byte[16];

            AesCryptoServiceProvider aesCSP = new AesCryptoServiceProvider();
            aesCSP.BlockSize = 128;
            aesCSP.Key = key;
            aesCSP.IV = iv;
            aesCSP.Padding = PaddingMode.Zeros;
            aesCSP.Mode = CipherMode.CBC;

            ICryptoTransform xfrm = aesCSP.CreateEncryptor(key, iv);
            byte[] result = xfrm.TransformFinalBlock(buffer, 0, buffer.Length);
            return result;
        }

        private byte[] AesCbcDecrypt(byte[] key, byte[] iv, byte[] buffer)
        {
            if (iv == null)
                iv = new byte[16];

            AesCryptoServiceProvider aesCSP = new AesCryptoServiceProvider();
            aesCSP.BlockSize = 128;
            aesCSP.Key = key;
            aesCSP.IV = iv;
            aesCSP.Padding = PaddingMode.Zeros;
            aesCSP.Mode = CipherMode.CBC;

            ICryptoTransform xfrm = aesCSP.CreateDecryptor(key, iv);
            byte[] result = xfrm.TransformFinalBlock(buffer, 0, buffer.Length);
            return result;
        }

        private byte[] AesEcbEncrypt(byte[] key, byte[] buffer)
        {
            byte[] iv = new byte[16];

            AesCryptoServiceProvider aesCSP = new AesCryptoServiceProvider();
            aesCSP.BlockSize = 128;
            aesCSP.Key = key;
            aesCSP.IV = iv;
            aesCSP.Padding = PaddingMode.Zeros;
            aesCSP.Mode = CipherMode.CBC;

            ICryptoTransform xfrm = aesCSP.CreateEncryptor(key, iv);
            byte[] result = xfrm.TransformFinalBlock(buffer, 0, buffer.Length);
            return result;
        }

        private byte[] AesEcbDecrypt(byte[] key, byte[] buffer)
        {
            byte[] iv = new byte[16];

            AesCryptoServiceProvider aesCSP = new AesCryptoServiceProvider();
            aesCSP.BlockSize = 128;
            aesCSP.Key = key;
            aesCSP.IV = iv;
            aesCSP.Padding = PaddingMode.Zeros;
            aesCSP.Mode = CipherMode.CBC;

            ICryptoTransform xfrm = aesCSP.CreateDecryptor(key, iv);
            byte[] result = xfrm.TransformFinalBlock(buffer, 0, buffer.Length);
            return result;
        }

        public void CleanupAuthentication()
        {
            SessionEncKey = null;
            SessionMacKey = null;
            SessionSendIV = null;
            SessionRecvIV = null;
        }

        private bool OpenSecureConnectionAes128(byte keySelect, byte[] keyValue)
        {
            uint rc;

            CleanupAuthentication();

            Logger.Trace("Running AES mutual authentication using key {0:X02}", keySelect);

            /* Generate host nonce */
            byte[] rndA = GetRandom(16);

            if (DebugSecureConnection)
            {
                Logger.Debug("key={0}", BinConvert.ToHex(keyValue));
                Logger.Debug("rndA={0}", BinConvert.ToHex(rndA));
            }

            /* Host->Device AUTHENTICATE command */
            /* --------------------------------- */

            byte[] cmdAuthenticate = new byte[4];

            cmdAuthenticate[0] = ProtocolCode;
            cmdAuthenticate[1] = (byte)ProtocolOpcode.Authenticate;
            cmdAuthenticate[2] = 0x01; /* Version & mode = AES128 */
            cmdAuthenticate[3] = keySelect;

            if (DebugSecureConnection)
                Logger.Debug("   <                    {0}", BinConvert.ToHex(cmdAuthenticate));

            rc = Escape(cmdAuthenticate, out byte[] rspStep1);
            if (rc != SCARD.S_SUCCESS)
            {
                Logger.Trace("Authentication failed at step 1 (error {0})", rc);
                return false;
            }

            if (DebugSecureConnection)
                Logger.Debug("   >                    {0}", BinConvert.ToHex(rspStep1));

            /* Device->Host Authentication Step 1 */
            /* ---------------------------------- */

            if (rspStep1.Length < 1)
            {
                Logger.Trace("Authentication failed at step 1 (response is too short)");
                return false;
            }

            if (rspStep1[0] != (byte)ProtocolOpcode.Following)
            {
                Logger.Trace("Authentication failed at step 1 (the device has reported an error: {0:X02})", rspStep1[0]);
                return false;
            }

            if (rspStep1.Length != 17)
            {
                Logger.Trace("Authentication failed at step 1 (response does not have the expected format)");
                return false;
            }

            byte[] t = BinUtils.Copy(rspStep1, 1, 16);
            byte[] rndB = AesEcbDecrypt(keyValue, t);

            if (DebugSecureConnection)
                Logger.Debug("rndB={0}", BinConvert.ToHex(rndB));

            /* Host->Device Authentication Step 2 */
            /* ---------------------------------- */

            byte[] cmdStep2 = new byte[34];

            cmdStep2[0] = ProtocolCode;
            cmdStep2[1] = (byte)ProtocolOpcode.Following;

            t = AesEcbEncrypt(keyValue, rndA);
            BinUtils.CopyTo(cmdStep2, 2, t, 0, 16);
            t = BinUtils.RotateLeftOneByte(rndB);
            t = AesEcbEncrypt(keyValue, t);
            BinUtils.CopyTo(cmdStep2, 18, t, 0, 16);

            if (DebugSecureConnection)
                Logger.Debug("   <                    {0}", BinConvert.ToHex(cmdStep2));

            rc = Escape(cmdStep2, out byte[] rspStep3);
            if (rc != SCARD.S_SUCCESS)
            {
                Logger.Trace("Authentication failed at step 3 (error {0})", rc);
                return false;
            }

            if (DebugSecureConnection)
                Logger.Debug("   >                    {0}", BinConvert.ToHex(rspStep3));

            /* Device->Host Authentication Step 3 */
            /* ---------------------------------- */

            if (rspStep3.Length < 1)
            {
                Logger.Trace("Authentication failed at step 3");
                return false;
            }

            if (rspStep3[0] != (byte)ProtocolOpcode.Success)
            {
                Logger.Trace("Authentication failed at step 3 (the device has reported an error: {0:X02})", rspStep3[0]);
                return false;
            }

            if (rspStep3.Length != 17)
            {
                Logger.Trace("Authentication failed at step 3 (response does not have the expected format)");
                return false;
            }

            t = BinUtils.Copy(rspStep3, 1, 16);
            t = AesEcbDecrypt(keyValue, t);
            t = BinUtils.RotateRightOneByte(t);

            if (!BinUtils.Equals(t, rndA))
            {
                Logger.Debug("{0}!={1}", BinConvert.ToHex(t), BinConvert.ToHex(rndA));
                Logger.Trace("Authentication failed at step 3 (device's cryptogram is invalid)");
                return false;
            }

            /* Session keys and first init vector */
            /* ---------------------------------- */

            byte[] sv1 = new byte[16];
            BinUtils.CopyTo(sv1, 0, rndA, 0, 4);
            BinUtils.CopyTo(sv1, 4, rndB, 0, 4);
            BinUtils.CopyTo(sv1, 8, rndA, 8, 4);
            BinUtils.CopyTo(sv1, 12, rndB, 8, 4);

            if (DebugSecureConnection)
                Logger.Debug("SV1={0}", BinConvert.ToHex(sv1));

            byte[] sv2 = new byte[16];
            BinUtils.CopyTo(sv2, 0, rndA, 4, 4);
            BinUtils.CopyTo(sv2, 4, rndB, 4, 4);
            BinUtils.CopyTo(sv2, 8, rndA, 12, 4);
            BinUtils.CopyTo(sv2, 12, rndB, 12, 4);

            if (DebugSecureConnection)
                Logger.Debug("SV2={0}", BinConvert.ToHex(sv2));

            SessionEncKey = AesEcbEncrypt(keyValue, sv1);
            if (DebugSecureConnection)
                Logger.Debug("Kenc={0}", BinConvert.ToHex(SessionEncKey));

            SessionMacKey = AesEcbEncrypt(keyValue, sv2);
            if (DebugSecureConnection)
                Logger.Debug("Kmac={0}", BinConvert.ToHex(SessionMacKey));

            t = BinUtils.XOR(rndA, rndB);
            t = AesEcbEncrypt(SessionMacKey, t);

            if (DebugSecureConnection)
                Logger.Debug("IV0={0}", BinConvert.ToHex(t));

            SessionSendIV = t;
            SessionRecvIV = t;

            return true;
        }
    }
}
