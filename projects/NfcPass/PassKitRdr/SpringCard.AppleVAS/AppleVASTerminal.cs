/**
 *
 * \ingroup AppleVAS
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
using System.IO;
using System.Reflection;
using System.Threading;

namespace SpringCard.AppleVas
{
    /**
	 * \brief This object runs the transaction with the Apple VAS-enabled NFC card (i.e. an iPhone or iPad running in card emulation mode)
	 **/
    public class AppleVasTerminal
    {
        private AppleVasTerminalConfig terminalConfig = new AppleVasTerminalConfig();

        /**
		 * \brief Create an Apple VAS controller - add merchant(s) using AddConfig
		 **/
        public AppleVasTerminal()
        {

        }

        /**
		 * \brief Create an Apple VAS controller, providing the complete configuration of the terminal
		 **/
        public AppleVasTerminal(AppleVasTerminalConfig terminalConfig)
        {
            this.terminalConfig = terminalConfig;
        }

        /**
		 * \brief Create an Apple VAS controller for the given Merchant
		 **/
        public AppleVasTerminal(AppleVasConfig merchantConfig)
        {
            this.terminalConfig.Merchants.Add(merchantConfig);
        }

        /**
		 * \brief Create an Apple VAS controller for more than one Merchant
		 **/
        public AppleVasTerminal(List<AppleVasConfig> merchantConfigs)
        {
            this.terminalConfig.Merchants.AddRange(merchantConfigs);
        }

        /**
		 * \brief Add a merchant to the controller
		 **/
        public void AddConfig(AppleVasConfig merchantConfig)
        {
            terminalConfig.Merchants.Add(merchantConfig);
        }

        /**
          * \brief Run the Apple VAS transaction
          **/
        public bool DoTransaction(ICardApduTransmitter cardChannel, out AppleVasData data, out AppleVasError error)
        {
            return DoTransaction(cardChannel, out data, out error, out RAPDU dummySelectOseresponse, null);
        }


        /**
          * \brief Run the Apple VAS transaction
          **/
        public bool DoTransaction(ICardApduTransmitter cardChannel, out AppleVasData data, out AppleVasError error, RAPDU selectOseResponseProvided)
        {
            return DoTransaction(cardChannel, out data, out error, out RAPDU dummySelectOseresponse, selectOseResponseProvided);
        }

        /**
          * \brief Run the Apple VAS transaction
          **/
        public bool DoTransaction(ICardApduTransmitter cardChannel, out AppleVasData data, out AppleVasError error, out RAPDU selectOseResponse)
        {
            return DoTransaction(cardChannel, out data, out error, out selectOseResponse, null);
        }


        /**
		 * \brief Run the Apple VAS transaction
         * 
         * \note This method also return the response of the SelectOSE (this can be useful tyo speed up intergartion with other protocols like Google Pay, Samsung Pay...)
		 **/
        private bool DoTransaction(ICardApduTransmitter cardChannel, out AppleVasData data, out AppleVasError error, out RAPDU selectOseResponse, RAPDU selectOseResponseProvided)
        {
            data = null;
            selectOseResponse = null;

            if(selectOseResponseProvided != null)
            {
                selectOseResponse = selectOseResponseProvided;
            }
            else
            {
                if (!SelectOSE(cardChannel, out error, out selectOseResponse))
                    return false;
            }

            if (!ProcessSelectOSEResponse(selectOseResponse, out error))
                return false;

            foreach (AppleVasConfig merchantConfig in terminalConfig.Merchants)
            {
                data = GetVasData(cardChannel, terminalConfig, merchantConfig, out error);
                if (data != null)
                {
                    EmvRemoval(cardChannel);
                    return true;
                }
            }
            
            if (error == AppleVasError.WaitingForActivation)
            {
                Logger.Debug("Resetting the RF field for activation...");
            }
            else
            {
                EmvRemoval(cardChannel);
            }

            if (error == AppleVasError.Success)
                return true;

            return false;
        }

        private void EmvRemoval(ICardApduTransmitter cardChannel)
        {
            RAPDU rapdu = cardChannel.Transmit(new CAPDU(0xFF, 0xF1, 0x10, 0x04, 0x00));
            Logger.Debug("EMV Removal <- {0}", rapdu.AsString());
        }

        public void FieldReset(ICardApduTransmitter cardChannel, int duration)
        {
            cardChannel.Transmit(new CAPDU(0xFF, 0xF1, 0x10, 0x00, 0x00));
            Thread.Sleep(duration);
            cardChannel.Transmit(new CAPDU(0xFF, 0xF1, 0x10, 0x01, 0x00));
        }

        /**
         * \brief Generate the SelectOSE command
         * 
         * \deprecated Use DotTansaction instead
         */
        public static CAPDU CreateSelectOSECommand()
        {
            return new CAPDU(0x00, 0xA4, 0x04, 0x00, "OSE.VAS.01", 0x00);
        }


        /**
         * \brief Process the response to the SelectOSE command
         * 
         * \deprecated Use DotTansaction instead
         */
        public static bool ProcessSelectOSEResponse(RAPDU response, out AppleVasError error)
        {
            if (response.SW != 0x9000)
            {
                Logger.Trace("\tSW={0:X04}", response.SW);

                switch (response.SW)
                {
                    case 0x6700:
                    case 0x6A82:
                        error = AppleVasError.NoVasApplication;
                        break;

                    case 0x6A80:
                        error = AppleVasError.DataMissing;
                        break;

                    case 0x6B00:
                        error = AppleVasError.WrongCommand;
                        break;

                    default:
                        error = AppleVasError.WrongResponseStatusWord;
                        break;
                }

                return false;
            }

            Asn1Tlv tlv = Asn1Tlv.Unserialize(response.DataBytes);
            if (tlv == null)
            {
                Logger.Debug("\tResponse is not a valid FCI template");
                error = AppleVasError.WrongResponseData;
                return false;
            }

            bool versionValid = false;
            bool loyaltyEnabled = false;

            byte[] buffer = tlv.V;
            while ((buffer != null) && (buffer.Length > 0))
            {
                tlv = Asn1Tlv.Unserialize(buffer, out buffer);
                if (tlv != null)
                {
                    switch (tlv.T_AsWord)
                    {
                        case 0x9F21:
                            if (tlv.L == 2)
                            {
                                Logger.Debug("\tVersionNumber={0}", BinConvert.ToHex(tlv.V));
                                ushort t = (ushort) ((tlv.V[0] << 8) | tlv.V[1]);
                                if (t >= 0x0100)
                                    versionValid = true;
                                break;
                            }
                            else
                            {
                                Logger.Debug("\tVersionNumber TLV has a bad length");
                            }
                            break;
                        case 0x9F24:
                            Logger.Debug("\tMobileNonce={0}", BinConvert.ToHex(tlv.V));
                            break;
                        case 0x9F23:
                            if (tlv.L == 4)
                            {
                                Logger.Debug("\tMobileCapabilities={0}", BinConvert.ToHex(tlv.V));

                                switch (tlv.V[3] & 0x0C)
                                {
                                    case 0x04:
                                        Logger.Debug("\t\tLoyalty supported, skip Loyalty protocol");
                                        break;

                                    case 0x0C:
                                        Logger.Debug("\t\tLoyalty supported, proceed with Loyalty protocol");
                                        loyaltyEnabled = true;
                                        break;

                                    default:
                                        Logger.Debug("\t\t(Invalid value)");
                                        break;
                                }

                                break;
                            }
                            else
                            {
                                Logger.Debug("\tMobileCapabilities TLV has a bad length");
                            }
                            break;

                        default:
                            Logger.Debug("\tGot unknown TLV {0}", BinConvert.ToHex(tlv.T));
                            break;
                    }
                }
            }

            if (!versionValid)
            {
                error = AppleVasError.VasVersionConflict;
                return false;
            }

            if (!loyaltyEnabled)
            {
                error = AppleVasError.VasDisabled;
                return false;
            }

            error = AppleVasError.Success;
            return true;
        }

        /**
		 * \brief Send the SelectOSE APDU to the the card, and check that the status word is 9000 and the FCI is OK
         * This method DO NOT call ProcessSelectOSEResponse()!
         * 
         * \deprecated Use DotTansaction instead
		 **/
        public bool SelectOSE(ICardApduTransmitter cardChannel, out AppleVasError error, out RAPDU selectOseResponse)
        {
            Logger.Debug("AppleVAS:SelectOSE");

            selectOseResponse = null;

            CAPDU command = CreateSelectOSECommand();

            Logger.Debug("<" + command.AsString());

            RAPDU response = cardChannel.Transmit(command);
            selectOseResponse = response;

            if (response == null)
            {
                Logger.Trace("\tNo response");
                error = AppleVasError.Communication;
                return false;
            }

            Logger.Debug(">" + response.AsString());

            if (response.Length < 2)
            {
                Logger.Debug("\tResponse is too short");
                error = AppleVasError.Communication;
                return false;
            }

            error = AppleVasError.Success;
            return true;
        }

        /**
         * \brief Send the SelectOSE APDU to the the card, and check that the status word is 9000 and the FCI is OK.
         * This method call ProcessSelectOSEResponse()
         * 
         * \deprecated Use DotTansaction instead
         **/
        public bool SelectOSE(ICardApduTransmitter cardChannel)
        {
            RAPDU selectOseResponse;

            if (!SelectOSE(cardChannel, out AppleVasError dummy, out selectOseResponse))
                return false;

            if (!ProcessSelectOSEResponse(selectOseResponse, out AppleVasError dummy2))
                return false;

            return true;
        }


        /**
         * \brief Send the SelectOSE APDU to the the card, and check that the status word is 9000 and the FCI is OK.
         * This method call ProcessSelectOSEResponse()
         * 
         * \deprecated Use DotTansaction instead
         **/
        public bool SelectOSE(ICardApduTransmitter cardChannel, out AppleVasError error)
        {
            RAPDU selectOseResponse;

            if (!SelectOSE(cardChannel, out error, out selectOseResponse))
                return false;

            if (!ProcessSelectOSEResponse(selectOseResponse, out error))
                return false;

            return true;
        }


        /**
         * \brief Generate the GetVasData command, for the given Merchant config. Default Terminal configuration is used
         * 
         * \deprecated Use DoTransaction instead
         */
        public static CAPDU CreateGetVasDataCommand(AppleVasConfig merchantConfig)
        {
            return CreateGetVasDataCommand(AppleVasTerminalConfig.Default, merchantConfig);
        }

        /**
         * \brief Generate the GetVasData command, for the given Merchant config
         * 
         * \deprecated Use DoTransaction instead
         */
        public static CAPDU CreateGetVasDataCommand(AppleVasTerminalConfig terminalConfig, AppleVasConfig merchantConfig)
        {
            bool last = true;

            if (terminalConfig.Merchants != null)
            {
                for (int i = 0; i < terminalConfig.Merchants.Count - 1; i++)
                {
                    if (merchantConfig == terminalConfig.Merchants[i])
                    {
                        last = false;
                        break;
                    }
                }
            }

            byte[] data = new byte[0];
            Asn1Tlv tlv;

            tlv = new Asn1Tlv(0x9F22, new byte[] { 0x01, 0x00 });
            data = BinUtils.Concat(data, tlv.Serialize());

            Logger.Debug("\tMerchantId={0}", BinConvert.ToHex(merchantConfig.Id));
            tlv = new Asn1Tlv(0x9F25, merchantConfig.Id);
            data = BinUtils.Concat(data, tlv.Serialize());

            tlv = new Asn1Tlv(0x9F28, new byte[4]);
            data = BinUtils.Concat(data, tlv.Serialize());

            byte[] capabilities = new byte[4];
            capabilities[3] = terminalConfig.CapabilitiesSet1;
            if (!last)
                capabilities[3] |= 0x80; /* More GET VAS DATA command(s) forthcoming */

            tlv = new Asn1Tlv(0x9F26, capabilities);
            data = BinUtils.Concat(data, tlv.Serialize());

            if (!string.IsNullOrEmpty(merchantConfig.Url))
            {
                Logger.Debug("\tMerchant.Url={0}", merchantConfig.Url);
                tlv = new Asn1Tlv(0x9F29, StrUtils.ToBytes(merchantConfig.Url));
                data = BinUtils.Concat(data, tlv.Serialize());
            }

            return new CAPDU(0x80, 0xCA, 0x01, terminalConfig.P2, data, 0x00);
        }

        /**
		 * \brief Retrieve the VAS message from the reponse to the GetVasData command
         * 
         * \deprecated Use DotTansaction instead
		 **/
        public static bool ExtractVasDataResponse(byte[] responseData, out byte[] rawMessage, out AppleVasError error)
        {
            rawMessage = null;

            Logger.Debug("AppleVAS:ProcessVasDataResponse({0})", BinConvert.ToHex(responseData));

            if ((responseData == null) || (responseData.Length == 0))
            {
                error = AppleVasError.Success;
                return true;
            }

            Asn1Tlv tlv = Asn1Tlv.Unserialize(responseData);
            if ((tlv == null) || (tlv.T_AsWord != 0x0070))
            {
                Logger.Debug("\tResponse is not a valid VAS data template");
                error = AppleVasError.WrongResponseData;
                return false;
            }

            responseData = tlv.V;
            while ((responseData != null) && (responseData.Length > 0))
            {
                tlv = Asn1Tlv.Unserialize(responseData, out responseData);
                if (tlv != null)
                {
                    switch (tlv.T_AsWord)
                    {
                        case 0x9F2A:
                            Logger.Debug("\tMobileToken={0}", BinConvert.ToHex(tlv.V));
                            break;
                        case 0x9F27:
                            Logger.Debug("\tVasData={0}", BinConvert.ToHex(tlv.V));
                            rawMessage = tlv.V;
                            break;
                        default:
                            Logger.Debug("\tGot unknown TLV {0}", BinConvert.ToHex(tlv.T));
                            break;
                    }
                }
            }

            if (rawMessage == null)
            {
                Logger.Debug("\tNo VAS message in the response");
                error = AppleVasError.NoVasMessage;
                return false;
            }

            error = AppleVasError.Success;
            return true;
        }

        /**
         * \brief Process the response to the GetVasData command and extract the raw VAS message
         * 
         * \deprecated Use DotTansaction instead
         */
        public static bool ProcessGetVasDataResponse(RAPDU response, out byte[] rawMessage, out AppleVasError error)
        {
            rawMessage = null;

            if (response.SW != 0x9000)
            {
                Logger.Debug("\tSW={0:X04}", response.SW);

                switch (response.SW)
                {
                    case 0x6A83:
                        /* VAS Data not found */
                        error = AppleVasError.NoVasMerchant;
                        break;
                    case 0x6287:
                        /* VAS Data not activated */
                        error = AppleVasError.WaitingForActivation;
                        break;
                    case 0x6984:
                        /* User intervention required on mobile device */
                        error = AppleVasError.WaitingForIntervention;
                        break;

                    default:
                        error = AppleVasError.WrongResponseStatusWord;
                        break;
                }
                return false;
            }

            return ExtractVasDataResponse(response.DataBytes, out rawMessage, out error);
        }

        /**
         * \brief Process the response to the GetVasData command
         * 
         * \deprecated Use DotTansaction instead
         */
        public static AppleVasData ProcessGetVasDataResponse(RAPDU response, AppleVasConfig config, out AppleVasError error)
        {
            if (!ProcessGetVasDataResponse(response, out byte[] rawMessage, out error))
            {
                return null;
            }

            if ((rawMessage == null) || (rawMessage.Length == 0))
            {
                error = AppleVasError.Success;
                return null;
            }

            return AppleVasData.Create(config, rawMessage, out error);
        }

        /**
		 * \brief Send the GetVasData APDU to the the card for the given Merchant config, process the response and extract the raw VAS message. Default Terminal configuration is used.
         * 
         * \deprecated Use DotTansaction instead
		 **/
        public bool GetVasData(ICardApduTransmitter cardChannel, AppleVasConfig merchantConfig, out byte[] rawMessage, out AppleVasError error)
        {
            return GetVasData(cardChannel, AppleVasTerminalConfig.Default, merchantConfig, out rawMessage, out error);
        }

        /**
		 * \brief Send the GetVasData APDU to the the card for the given Merchant config, process the response and extract the raw VAS message
         * 
         * \deprecated Use DotTansaction instead
		 **/
        public bool GetVasData(ICardApduTransmitter cardChannel, AppleVasTerminalConfig terminalConfig, AppleVasConfig merchantConfig, out byte[] rawMessage, out AppleVasError error)
        {
            Logger.Debug("AppleVAS:GetVasData");

            CAPDU command = CreateGetVasDataCommand(terminalConfig, merchantConfig);

            Logger.Debug("<" + command.AsString());

            RAPDU response = cardChannel.Transmit(command);

            if (response == null)
            {
                Logger.Debug("\tNo response");
                error = AppleVasError.Communication;
                rawMessage = null;
                return false;
            }

            Logger.Debug(">" + response.AsString());

            if (response.Length < 2)
            {
                Logger.Debug("\tResponse is too short");
                error = AppleVasError.Communication;
                rawMessage = null;
                return false;
            }

            return ProcessGetVasDataResponse(response, out rawMessage, out error);
        }

        /**
		 * \brief Send the GetVasData APDU to the the card for the given Merchant config, process the response and extract the raw VAS message. Default Terminal configuration is used
         * 
         * \deprecated Use DotTansaction instead
		 **/
        public bool GetVasData(ICardApduTransmitter cardChannel, AppleVasConfig merchantConfig, out byte[] rawMessage)
        {
            return GetVasData(cardChannel, AppleVasTerminalConfig.Default, merchantConfig, out rawMessage);
        }

        /**
		 * \brief Send the GetVasData APDU to the the card for the given Merchant config, process the response and extract the raw VAS message
         * 
         * \deprecated Use DotTansaction instead
		 **/
        public bool GetVasData(ICardApduTransmitter cardChannel, AppleVasTerminalConfig terminalConfig, AppleVasConfig merchantConfig, out byte[] rawMessage)
        {
            return GetVasData(cardChannel, terminalConfig, merchantConfig, out rawMessage, out AppleVasError dummy);
        }

        /**
		 * \brief Send the GetVasData APDU to the the card for the given Merchant config, process the response and return the decrypted VAS message. Default Terminal configuration is used
         * 
         * \deprecated Use DotTansaction instead
		 **/
        public AppleVasData GetVasData(ICardApduTransmitter cardChannel, AppleVasConfig merchantConfig, out AppleVasError error)
        {
            return GetVasData(cardChannel, AppleVasTerminalConfig.Default, merchantConfig, out error);
        }

        /**
		 * \brief Send the GetVasData APDU to the the card for the given Merchant config, process the response and return the decrypted VAS message
         * 
         * \deprecated Use DotTansaction instead
		 **/
        public AppleVasData GetVasData(ICardApduTransmitter cardChannel, AppleVasTerminalConfig terminalConfig, AppleVasConfig merchantConfig, out AppleVasError error)
        {
            if (!GetVasData(cardChannel, terminalConfig, merchantConfig, out byte[] rawMessage, out error))
                return null;

            if ((rawMessage == null) || (rawMessage.Length == 0))
            {
                Logger.Debug("\tNo VAS message in response");
                error = AppleVasError.Success;
                return null;
            }

            /* Retrieve Pub key and KeyId in secure element if necessary */
            uint[] keyIds = merchantConfig.PrivateKeyIds;
            for(int i=0; i<merchantConfig.privateKeys.Count; i++)
            {
                if (merchantConfig.privateKeys[i] == null)
                {
                    Logger.Debug("The merchant does not know this PublicKeyId");
                    continue;
                }

                if (!merchantConfig.privateKeys[i].IsInitialized())
                {
                    if (merchantConfig.privateKeys[i] is PrivateKeySoftware)
                    {
                        Logger.Error("Error, merchantPrivateKey should have been initialized");
                        continue;
                    }
                    else if (merchantConfig.privateKeys[i] is PrivateKeySecureElement)
                    {
                        Logger.Error($"Initializing PrivateKeySecureElement idx={(merchantConfig.privateKeys[i] as PrivateKeySecureElement).KeyIndex}");
                        (merchantConfig.privateKeys[i] as PrivateKeySecureElement).Initialize(cardChannel);
                    }
                }
            }


            return AppleVasData.Create(merchantConfig, rawMessage, out error);
        }

        /**
		 * \brief SelfTest
		 **/
        public static bool SelfTest(bool throwException = true)
        {
            Console.WriteLine("Running SelftTest");

            if (!SelfTest_SC1(throwException))
                return false;

            if (!SelfTest_ITN(throwException))
                return false;


            return true;
        }

        internal static bool SelfTest_ITN(bool throwException = true)
        {
            AppleVasError error;


            /*                      1                   2                   3                   4                   5                   6                   7                   8         */
            /*  0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 */
            /* 70539F2A009F274DE4C68B59AE8D96FE4197A178C0E3AA8E6217F7F219CA59078F9D1FEDE60B4DEAC9A3FA0F0BBF75E2F6686A40D39592CDD5FBDC61F475BF9D2AFF47FED8264B99CA0037B38FF2EDB9A883B0026C */
            /* ++++++++                                                                                                                                                                   */
            /*         ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++                                                                                                   */
            /*                                                                         ++++++++                                                                                           */
            /*                                                                                 ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++                                 */
            /*                                                                                                                                           ++++++++++++++++++++++++++++++++ */
            string mobileResponseHex = "70539F2A009F274DE4C68B59AE8D96FE4197A178C0E3AA8E6217F7F219CA59078F9D1FEDE60B4DEAC9A3FA0F0BBF75E2F6686A40D39592CDD5FBDC61F475BF9D2AFF47FED8264B99CA0037B38FF2EDB9A883B0026C";
            string merchantPublicKeyBase64 = "MDkwEwYHKoZIzj0CAQYIKoZIzj0DAQcDIgACKkPmF8igCoMCt8JvJxPD/sGOvR9zfHDDzexImIuX73k=";
            string merchantPrivateKeyBase64 = "MHcCAQEEIHzUrsTwxMJUBGxDn9K8kkCMbB6oGStDXyMVngpCOK+YoAoGCCqGSM49AwEHoUQDQgAEKkPmF8igCoMCt8JvJxPD/sGOvR9zfHDDzexImIuX73l6GpqQNuoBgsoaoMrNv8UI8snXE7tgZgHxcSwhnOWyTg==";
            string merchantName = "pass.com.itnint.test";

            AppleVasConfig config = new AppleVasConfig(merchantName);

            Console.WriteLine("MerchantId=" + BinConvert.ToHex(config.Id));

            byte[] merchantPrivateKeyRaw = StrUtils.Base64Decode(merchantPrivateKeyBase64);
            Console.WriteLine("PrivateKeyRaw=" + BinConvert.ToHex(merchantPrivateKeyRaw));

            if (!config.AddPrivateKey(merchantPrivateKeyRaw))
            {
                Console.WriteLine("Failed to load the private key");
                return false;
            }

            Console.WriteLine("PrivateKeyId=" + BinConvert.ToHex(config.PrivateKeyIds[0]));


            byte[] mobileResponse = BinConvert.HexToBytes(mobileResponseHex);


            byte[] mobileVasData;
            if (!AppleVasTerminal.ExtractVasDataResponse(mobileResponse, out mobileVasData, out error))
                return false;

            Console.WriteLine("AppleVAS:SelfTest:VAS data={0}", BinConvert.ToHex(mobileVasData));

            byte[] merchantPublicKeyXHash = BinUtils.Copy(mobileVasData, 0, 4);
            Console.WriteLine("AppleVAS:SelfTest:merchantPublicKeyXHash={0}", BinConvert.ToHex(merchantPublicKeyXHash));
            byte[] merchantPublicKey = StrUtils.Base64Decode(merchantPublicKeyBase64);
            Console.WriteLine("AppleVAS:SelfTest:merchantPublicKey={0}", BinConvert.ToHex(merchantPublicKey));
            byte[] merchantPublicKeyX = BinUtils.Copy(merchantPublicKey, merchantPublicKey.Length - 32, 32);
            Console.WriteLine("AppleVAS:SelfTest:merchantPublicKeyX={0}", BinConvert.ToHex(merchantPublicKeyX));

            byte[] t = AppleVasCrypto.SHA256(merchantPublicKeyX);
            Console.WriteLine("AppleVAS:SelfTest:merchantPublicKeyXHash={0}", BinConvert.ToHex(t));

            if (!BinUtils.Equals(merchantPublicKeyXHash, t, 4))
            {
                Console.WriteLine("AppleVAS:SelfTest:Wrong hash");
                return false;
            }

            Console.WriteLine("AppleVAS:SelfTest:Hash is OK");

            Console.WriteLine();

            byte[] mobileCrytogram = BinUtils.Copy(mobileVasData, 4);
            Console.WriteLine("AppleVAS:SelfTest:mobileCrytogram={0}", BinConvert.ToHex(mobileCrytogram));

            Console.WriteLine();


            Console.WriteLine();

            byte[] mobileMessagePlain = AppleVasData.DecryptCryptogram(config.Id, config.GetPrivateKey(config.PrivateKeyIds[0]), mobileCrytogram, out error);
            if (mobileMessagePlain == null)
                return false;

            DateTime mobileMessageWhen;
            byte[] mobileMessageText;
            if (!AppleVasData.Decode(mobileMessagePlain, out mobileMessageWhen, out mobileMessageText))
                return false;

            AppleVasData.IsTimestampAllowed(mobileMessageWhen, DateTime.UtcNow);

            AppleVasData vasData = AppleVasData.Create(config, mobileResponse, out error);

            if (vasData == null)
                return false;

            Console.WriteLine("SelftTest1 OK");
            return true;
        }

        internal static bool SelfTest_SC1(bool throwException = true)
        {
            AppleVasError error;

            byte[] merchantId = new byte[]
            {
                0x73, 0xF1, 0x63, 0x9A, 0xBB, 0x7A, 0xB7, 0xAE, 0x44, 0xA6, 0x9D, 0xCC, 0xF6, 0xA7, 0x3D, 0xC2,
                0xED, 0x6D, 0x64, 0x53, 0x36, 0x3D, 0x18, 0xE7, 0xBC, 0x28, 0x19, 0x1E, 0xFF, 0xFD, 0xC3, 0xD3
            };
            byte[] privateKey = new byte[]
            {
                0x2A, 0x7E, 0x3D, 0x3E, 0xCA, 0xF0, 0x54, 0x12, 0x3A, 0x2D, 0x87, 0x10, 0x3D, 0x40, 0xAF, 0x94,
                0x6A, 0xC7, 0x13, 0x76, 0x12, 0x48, 0x8A, 0x24, 0xC0, 0x92, 0x7D, 0xA2, 0xF8, 0x86, 0xD6, 0x0D
            };

            byte[] respSelectOse = new byte[]
            {
                0x6F, 0x1D, 0x50, 0x08, 0x41, 0x70, 0x70, 0x6C, 0x65, 0x50, 0x61, 0x79, 0x9F, 0x21, 0x02, 0x01,
                0x00, 0x9F, 0x24, 0x04, 0xA6, 0xF1, 0xB9, 0xFE, 0x9F, 0x23, 0x04, 0x00, 0x00, 0x00, 0x1E
            };
            byte[] respGetVasData = new byte[]
            {
                0x70, 0x5E, 0x9F, 0x2A, 0x00, 0x9F, 0x27, 0x58, 0x85, 0x8B, 0x52, 0x42, 0x06, 0xAF, 0xF5, 0x19,
                0x1C, 0x67, 0xC3, 0x79, 0x1A, 0xEF, 0x04, 0x7B, 0x2E, 0x5E, 0x06, 0x91, 0xBE, 0x74, 0xF5, 0xEF,
                0x3F, 0xEF, 0x02, 0x62, 0xE4, 0x41, 0x9C, 0x77, 0x7C, 0xDF, 0x59, 0xE5, 0x2A, 0xF7, 0x99, 0x8B,
                0x1B, 0x08, 0x57, 0x16, 0xF5, 0x89, 0x71, 0x9E, 0x83, 0x72, 0x50, 0x70, 0x8F, 0xC7, 0x88, 0x3D,
                0xF3, 0xD5, 0xCD, 0xEC, 0x8E, 0x93, 0xBE, 0xC1, 0xAB, 0xA4, 0x0C, 0x2F, 0xD1, 0xFC, 0x89, 0xEA,
                0xC5, 0xFE, 0x09, 0xC5, 0xA2, 0xCD, 0xDF, 0x9A, 0x37, 0x2E, 0x55, 0x03, 0xD4, 0xFF, 0x4A, 0xE0
            };


            AppleVasConfig config = new AppleVasConfig(merchantId);

            Console.WriteLine("MerchantId=" + BinConvert.ToHex(config.Id));
            Console.WriteLine("PrivateKeyRaw=" + BinConvert.ToHex(privateKey));

            if (!config.AddPrivateKey(privateKey))
            {
                Console.WriteLine("Failed to load the private key");
                return false;
            }

            Console.WriteLine("PrivateKeyId=" + BinConvert.ToHex(config.PrivateKeyIds[0]));

            byte[] mobileVasData;
            if (!AppleVasTerminal.ExtractVasDataResponse(respGetVasData, out mobileVasData, out error))
            {
                Console.WriteLine(error);
                return false;
            }

            Console.WriteLine("AppleVAS:SelfTest:VAS data={0}", BinConvert.ToHex(mobileVasData));

            byte[] mobileCrytogram = BinUtils.Copy(mobileVasData, 4);
            Console.WriteLine("AppleVAS:SelfTest:mobileCrytogram={0}", BinConvert.ToHex(mobileCrytogram));

            byte[] mobileMessagePlain = AppleVasData.DecryptCryptogram(config.Id, config.GetPrivateKey(config.PrivateKeyIds[0]), mobileCrytogram, out error);
            if (mobileMessagePlain == null)
            {
                Console.WriteLine(error);
                return false;
            }

            DateTime mobileMessageWhen;
            byte[] mobileMessageText;
            if (!AppleVasData.Decode(mobileMessagePlain, out mobileMessageWhen, out mobileMessageText))
            {
                Console.WriteLine(error);
                return false;
            }

            AppleVasData.IsTimestampAllowed(mobileMessageWhen, DateTime.UtcNow);

            AppleVasData vasData = AppleVasData.Create(config, respGetVasData, out error);

            if (vasData == null)
            {
                Console.WriteLine(error);
                return false;
            }

            Console.WriteLine("SelftTest2 OK");
            return true;
        }
    }
}
