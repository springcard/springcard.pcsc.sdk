/**
 *
 * \defgroup GoogleVAS   SpringCard GoogleVAS Library for .NET
 *
 * \brief Terminal-side implementation of Google VAS protocol, to read Smart Tap data through NFC.
 *
 * This is the terminal-side implementation of Google Pay Smart Tap protocol
 * Reference: https://developers.google.com/pay/smart-tap/ (date of implementation: 20/03/2019)
 *
 * Basic usage:
 * - create a GoogleVasConfig object, holding your merchant data
 * - create a GoogleVasTerminal, and feed it with the GoogleVasConfig
 * - connect to a PC/SC reader using the SpringCard.PCSC Class Library. The connection to the Android mobile will be a CardChannel instance
 * - run the GoogleVasTerminal.DoTransaction method over the CardChannel instance
 * - DoTransaction returns an GoogleVasData object holding the data provided by the terminal
 * 
 * \copyright
 *   Copyright (c) 2019 SpringCard - www.springcard.com
 *   All right reserved
 *
 * \author
 *   Johann.D et al. / SpringCard
 *
 */
/*
 * Read LICENSE.txt for license details and restrictions.
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Org.BouncyCastle.Crypto.Parameters;
using SpringCard.LibCs;
using SpringCard.PCSC;

namespace SpringCard.GoogleVas
{
    /**
	 * \brief Library information
	 **/
    public static class Library
    {
        /**
         * \brief Read Library information
         **/
        public static readonly AppModuleInfo ModuleInfo = new AppModuleInfo(Assembly.GetExecutingAssembly());
    }

    /**
     * \cond
     */
    internal class GoogleVasSession
    {
        private GoogleVasConfig config;

        public bool SmartTapIsImplicit;
        public bool SmartTapSupported = true;
        public byte[] SessionId;
        public byte SequenceNumber;
        public byte[] CryptographicMasterNonce;
        public byte[] SmartTap2FciNonce;
        public byte[] SmartTap2MobileDeviceNonce;
        public byte[] MobileDeviceEphemeralPublicKey;
        public byte[] TerminalNonce;
        internal ECPrivateKeyParameters TerminalEphemeralKey;
        public byte[] TerminalEphemeralPrivateKey;
        public byte[] TerminalEphemeralPublicKey;
        public byte[] Signature;
        public bool AuthenticationOK;
        public bool SessionKeyOK;
        public byte[] SharedSecret;
        public byte[] AesKey;
        public byte[] HmacKey;
        public bool PhoneHasDiscretionaryTemplate = false;
        public ushort SmartTapMaxVersionSupported;
        public ushort SmartTapMinVersionSupported;

        public GoogleVasSession(GoogleVasConfig config)
        {
            this.config = config;
            SessionId = PRNG.Generate(8);

            if ((config.ForceTerminalNonce == null) || (config.ForceTerminalNonce.Length == 0))
            {
                TerminalNonce = PRNG.Generate(32);
                Logger.Debug("Generating a nonce: {0}", BinConvert.ToHex(TerminalNonce));
            }
            else
            {
                TerminalNonce = config.ForceTerminalNonce;
                Logger.Debug("Nonce is forced: {0}", BinConvert.ToHex(TerminalNonce));
            }

            if ((config.ForceTerminalEphemeralPrivateKey == null) || (config.ForceTerminalEphemeralPrivateKey.Length == 0))
            {
                TerminalEphemeralPrivateKey = PRNG.Generate(32);
                Logger.Debug("Generating an ephemeral private key: {0}", BinConvert.ToHex(TerminalEphemeralPrivateKey));
            }
            else
            { 
                TerminalEphemeralPrivateKey = config.ForceTerminalEphemeralPrivateKey;
                Logger.Debug("Ephemeral private key is forced: {0}", BinConvert.ToHex(TerminalEphemeralPrivateKey));
            }

            Logger.Debug("Load ephemeral key");
            TerminalEphemeralKey = GoogleVasCrypto.ECC.ImportPrivateKey(TerminalEphemeralPrivateKey);
            TerminalEphemeralPublicKey = GoogleVasCrypto.ECC.EncodePublic(GoogleVasCrypto.ECC.ExtractPublicKey(TerminalEphemeralKey), true);
        }

        public byte[] GetMobileDeviceNonce()
        {
            if (SmartTap2MobileDeviceNonce != null)
                return SmartTap2MobileDeviceNonce;
            if (SmartTap2FciNonce != null)
                return SmartTap2FciNonce;
            if (CryptographicMasterNonce != null)
                return CryptographicMasterNonce;
            return null;
        }
    }
    /**
     * \endcond
     */

    /**
	 * \brief This object runs the transaction with the Google VAS-enabled NFC card (i.e. an Android mobile running in card emulation mode, with Google Pay enabled)
	 **/
    public partial class GoogleVasTerminal
	{
        private GoogleVasConfig config;
        private GoogleVasSession session;

        /**
		 * \brief Create an Google VAS controller for the given Merchant
		 **/
        public GoogleVasTerminal(GoogleVasConfig config)
		{
            this.config = config;
		}

        /**
		 * \brief SelfTest
		 **/
        public static bool SelfTest(GoogleVasConfig config, bool throwException = true)
		{
            if (!GoogleVasCrypto.HKDF.SelfTest(throwException))
                return false;

            GoogleVasTerminal controller = new GoogleVasTerminal(config);
            controller.session = new GoogleVasSession(config);
            controller.session.CryptographicMasterNonce = BinConvert.HexToBytes("BF33FDB1B1A0F1324CC5720DB2A8592859CA18FBB55374A756D18581DF29599F");
            controller.session.MobileDeviceEphemeralPublicKey = BinConvert.HexToBytes("03D42FF76A15675CE80622E3B15E8DF828D871424FD949B6D4C610BCAD028EE297");
//            controller.ComputeSignature();
            controller.session.Signature = BinConvert.HexToBytes("3044022077ED8CDB01C4216DF0B3F9CCC256AC8278B89E52A6A1B01E13736214E732C72F02202E32E44F5A1DB16DEDDC7BB0F45B42B0FE36F6FE371B33865710BC2DA2AF5193");
            controller.ComputeSessionKeys();

            controller.DecryptPayload(BinConvert.HexToBytes("014EF67D2461C6E179FC18F1EEEEFBAF3DD19A0B5E6B34575A45BDE572C4D7F9E7BE70FEFA48534A2AAE11B52A046B2413080B9A2FAF6A412C2D9A70F8ADFC6CE3029A61C3FEDB8F60DE0F3EE213281A2CF99383DE81CB324026EEC1CEA0281A6386D379C8F647315F3680C083B11B4479294F9D158BCC281356A03243956D590ABC15FF9E59D59AB66C03BF1896E4C9"));

            return true;
		}
	}
}
