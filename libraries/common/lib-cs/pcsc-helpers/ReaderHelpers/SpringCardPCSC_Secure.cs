/**h* SpringCard/PCSC_Utils
 *
 * NAME
 *   PCSC : PCSC_Utils
 * 
 * DESCRIPTION
 *   SpringCard's misc utilities for the PC/SC API
 *
 * COPYRIGHT
 *   Copyright (c) 2018-2018 SpringCard - www.springcard.com
 *
 * AUTHOR
 *   Johann.D / SpringCard
 *
 **/
using System;
using System.Drawing;
using System.Collections.Generic;
using SpringCard.PCSC;
using SpringCard.LibCs;
using System.Security.Cryptography;

namespace SpringCard.PCSC.ReaderHelper
{
    public partial class ReaderHelper
    {
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

        public enum KeySelect : byte
        {
            HostCommUserKey = 0,
            HostCommAdminKey = 1
        }

        private byte[] GetRandom(int length)
        {
            byte[] result = new byte[length];
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            rng.GetBytes(result);
            return result;
        }

        private byte[] AesEncrypt(byte[] key, byte[] iv, byte[] buffer)
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
            Logger.Debug("Encrypt {0} -> {1}", BinConvert.ToHex(buffer), BinConvert.ToHex(result));
            return result;
        }

        private byte[] AesDecrypt(byte[] key, byte[] iv, byte[] buffer)
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
            Logger.Debug("Decrypt {0} -> {1}", BinConvert.ToHex(buffer), BinConvert.ToHex(result));
            return result;
        }

        public void CleanupAuthentication()
        {
            SessionEncKey = null;
            SessionMacKey = null;
            SessionSendIV = null;
            SessionRecvIV = null;
        }

        public bool AuthenticateAes(KeySelect keySelect, byte[] keyValue)
        {
            CleanupAuthentication();

            Logger.Trace("Running AES mutual authentication using {0}", keySelect.ToString());

            /* Generate host nonce */
            byte[] rndA = GetRandom(16);

            Logger.Debug("key={0}", BinConvert.ToHex(keyValue));
            Logger.Debug("rndA={0}", BinConvert.ToHex(rndA));

            /* Host->Device AUTHENTICATE command */
            /* --------------------------------- */

            byte[] cmdAuthenticate = new byte[4];

            cmdAuthenticate[0] = ProtocolCode;
            cmdAuthenticate[1] = (byte) ProtocolOpcode.Authenticate;
            cmdAuthenticate[2] = 0x01; /* Version & mode = AES128 */
            cmdAuthenticate[3] = (byte) keySelect;

            byte[] rspStep1 = channel.Control(cmdAuthenticate);

            if (rspStep1 == null)
            {
                Logger.Trace("Authentication failed at step 1 ({0})", channel.LastErrorAsString);
                return false;
            }

            /* Device->Host Authentication Step 1 */
            /* ---------------------------------- */

            if (rspStep1.Length < 2)
            {
                Logger.Trace("Authentication failed at step 1 (response is too short)");
                return false;
            }

            if (rspStep1[0] != ProtocolCode)
            {
                Logger.Trace("Authentication failed at step 1 (unexpected response)");
                return false;
            }

            if (rspStep1[1] != (byte)ProtocolOpcode.Following)
            {
                Logger.Trace("Authentication failed at step 1 (the device has reported an error: {0:X02})", rspStep1[1]);
                return false;
            }

            if (rspStep1.Length != 18)
            {
                Logger.Trace("Authentication failed at step 1 (response does not have the expected format)");
                return false;
            }

            byte[] t = BinUtils.Copy(rspStep1, 2, 16);
            byte[] rndB = AesDecrypt(keyValue, null, t);

            Logger.Debug("rndB={0}", BinConvert.ToHex(rndA));

            /* Host->Device Authentication Step 2 */
            /* ---------------------------------- */

            byte[] cmdStep2 = new byte[34];

            cmdStep2[0] = ProtocolCode;
            cmdStep2[1] = (byte) ProtocolOpcode.Following;

            t = AesEncrypt(keyValue, null, rndA);
            BinUtils.CopyTo(cmdStep2, 2, t, 0,  16);
            t = BinUtils.RotateLeftOneByte(rndB);
            t = AesEncrypt(keyValue, null, t);
            BinUtils.CopyTo(cmdStep2, 18, t, 0, 16);

            byte[] rspStep3 = channel.Control(cmdStep2);

            /* Device->Host Authentication Step 3 */
            /* ---------------------------------- */

            if (rspStep3.Length < 2)
            {
                Logger.Trace("Authentication failed at step 3 ({0})", channel.LastErrorAsString);
                return false;
            }

            if (rspStep3[0] != ProtocolCode)
            {
                Logger.Trace("Authentication failed at step 3 (unexpected response)");
                return false;
            }

            if (rspStep3[1] != (byte)ProtocolOpcode.Success)
            {
                Logger.Trace("Authentication failed at step 3 (the device has reported an error: {0:X02})", rspStep3[1]);
                return false;
            }

            if (rspStep3.Length != 18)
            {
                Logger.Trace("Authentication failed at step 3 (response does not have the expected format)");
                return false;
            }

            t = BinUtils.Copy(rspStep3, 2, 16);
            t = BinUtils.RotateRightOneByte(t);

            if (!BinUtils.Equals(t, rndA))
            {
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

            Logger.Debug("SV1={0}", BinConvert.ToHex(sv1));

            byte[] sv2 = new byte[16];
            BinUtils.CopyTo(sv2, 0, rndA, 4, 4);
            BinUtils.CopyTo(sv2, 4, rndB, 4, 4);
            BinUtils.CopyTo(sv2, 8, rndA, 12, 4);
            BinUtils.CopyTo(sv2, 12, rndB, 12, 4);

            Logger.Debug("SV2={0}", BinConvert.ToHex(sv2));

            SessionEncKey = AesEncrypt(keyValue, null, sv1);
            Logger.Debug("Kenc={0}", BinConvert.ToHex(SessionEncKey));

            SessionMacKey = AesEncrypt(keyValue, null, sv2);
            Logger.Debug("Kmac={0}", BinConvert.ToHex(SessionMacKey));

            t = BinUtils.XOR(rndA, rndB);
            t = AesEncrypt(SessionMacKey, null, t);

            Logger.Debug("IV0={0}", BinConvert.ToHex(t));

            SessionSendIV = t;
            SessionRecvIV = t;

            return true;
        }


    }
}
