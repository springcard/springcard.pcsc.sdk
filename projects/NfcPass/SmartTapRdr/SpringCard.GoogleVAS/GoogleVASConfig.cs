/**
 *
 * \ingroup GoogleVAS
 *
 */
using System;
using System.Reflection;
using SpringCard.LibCs;
using Org.BouncyCastle.Crypto.Parameters;
using System.IO;
using System.Collections.Generic;

namespace SpringCard.GoogleVas
{
    /**
	 * \brief Encapsulate the configuration of the Google VAS terminal
	 **/
    public class GoogleVasConfig
    {
        private bool valid = false;

        public GoogleVasConfig()
        {
            Request = new RequestDetails();
            Capabilities = new CapabilitiesDetails();
        }

        /**
         * \brief Return true if the minimal settings have been populated
         **/
        public bool IsValid()
        {
            return valid;
        }

        /**
         * \brief Load configuration and private key. Must be called after any change of the configuration.
         **/
        public void Validate()
        {
            try
            {
                Logger.Debug("Load long term key");
                if(UseSecureElement)
                {
                    Logger.Debug("Using key inside the secure element of the device");            
                }
                else
                {
                    Logger.Debug("Using key passed to the library");
                    TerminalLongTermKey = GoogleVasCrypto.ECC.ImportPrivateKey(LongTermPrivateKey);
                    Logger.Debug("Private key:");
                    Logger.Debug("\t{0}", BinConvert.ToHex(LongTermPrivateKey));
                    ECPublicKeyParameters TerminalLongTermPublicKey = GoogleVasCrypto.ECC.ExtractPublicKey(TerminalLongTermKey);
                    Logger.Trace("Public key:");
                    Logger.Trace("\t{0}", BinConvert.ToHex(GoogleVasCrypto.ECC.EncodePublic(TerminalLongTermPublicKey, false)));
                    Logger.Trace("\t{0}", GoogleVasCrypto.ECC.EncodePublicPem(TerminalLongTermPublicKey, 0));
                }
   
                valid = true;
            }
            catch
            {
                Logger.Error("The private key is invalid");
                valid = false;
            }
        }

        /**
         * \brief Use OSE command
         **/
        public bool UseOSE;

        /**
         * \brief Allow skipping Smart Tap 2 select
         **/
        public bool AllowSkippingSmartTap2Select;

        /**
         * \brief The version used in NDEF request records
         * (0x0000 for v2.0, 0x0001 for v2.1, 0x0002 for v2.2)
         **/
        public ushort SmartTapVersion;

        /**
         * \brief Request payment
         **/
        public bool DoPayment;

        /**
         * \brief Payment : Split valuables connection from payment
         **/
        public bool SplitValuablesConnectionFromPayment;

        /**
         * \brief Payment : Stop payment if smart tap fails
         **/
        public bool StopPaymentIfSmartTapFails;

        /**
         * \brief Merchant's Collector ID
         */
        public byte[] CollectorId;

        /**
         * \brief Merchant's Collector ID, truncated or padded to 4 bytes exactly
         */
        public byte[] CollectorId_4
        {
            get
            {
                if (CollectorId != null)
                {
                    if (CollectorId.Length > 4)
                        return BinUtils.Copy(CollectorId, CollectorId.Length - 4, 4);
                    if (CollectorId.Length < 4)
                        return BinUtils.EnsureSize(CollectorId, 4, 4, BinUtils.PaddMethod.Before);
                    return CollectorId;
                }
                return new byte[4];
            }
        }

        /**
         * \brief Location ID
         */
        public byte[] LocationId;

        /**
         * \brief Terminal ID
         */
        public byte[] TerminalId;

        /**
         * \brief Merchant name
         */
        public string MerchantName;

        /**
         * \brief Merchant category code
         * MCC on 2 bytes instead of 4 https://github.com/greggles/mcc-codes
         */
        public ushort MerchantCategoryCode;

        /**
         * Use encryption
         */
        public bool UseEncryption;

        internal ECPrivateKeyParameters TerminalLongTermKey;

        /**
         * Use presigned authentication
         */
        public bool PresignedAuth;

        /**
        * If this field is set to true and the device has a Secure element
        * the field LongTermPrivateKeyIndex will be used instead of LongTermPrivateKey
        */
        public bool UseSecureElement = false;

        /**
         * Long term private key
         */
        public byte[] LongTermPrivateKey;

        /**
        * Long term private key index inside the device's secure element
        */
        public byte LongTermPrivateKeyIndex;

        /**
         * Long term key pair version
         */
        public uint LongTermKeyVersion;

        /**
         * Force terminal nonce (must be empty for random nonces)
         */
        public byte[] ForceTerminalNonce;

        /**
         * Force terminal ephemeral private key (must be empty for random private keys)
         */
        public byte[] ForceTerminalEphemeralPrivateKey;

        /**
         * \brief Send valuables request. See RequestDetails for details
         */
        public bool DoRequest;

        /**
         * \brief Details of a request
         */
        public class RequestDetails
        {
            /**
             * \brief Request all valuables
             */
            public bool AllValuables;
            /**
             * \brief Request all valuables except PPSE
             */
            public bool AllValuablesExceptPpse;
            /**
             * \brief Request customer information
             */
            public bool WalletCustomer;
            /**
             * \brief Request event tickets
             */
            public bool EventTickets;
            /**
             * \brief Request flights
             */
            public bool Flights;
            /**
             * \brief Request transit
             */
            public bool Transit;
            /**
             * \brief Request gift cards
             */
            public bool GiftCards;
            /**
             * \brief Request loyalty and rewards
             */
            public bool LoyaltyPrograms;
            /**
             * \brief Request coupons and offers
             */
            public bool Offers;
            /**
             * \brief Request private label cards
             */
            public bool PrivateLabelCards;
        }

        /**
         * \brief Details of the request
         */
        public RequestDetails Request;

        /**
         * \brief Push back data. See PushDetails for details
         */
        public bool DoPush;

        /**
         * \brief Details of the terminal capabilities
         */
        public class CapabilitiesDetails
        {
            /**
             * \brief System capabilities: Standalone
             **/
            public bool Standalone;

            /**
             * \brief System capabilities: Semi-integrated
             **/
            public bool SemiIntegrated;

            /**
             * \brief System capabilities: Unattended
             **/
            public bool Unattended;

            /**
             * \brief System capabilities: Online
             **/
            public bool Online;

            /**
             * \brief System capabilities: Offline
             **/
            public bool Offline;

            /**
             * \brief System capabilities: MMP
             **/
            public bool MMP;

            /**
             * \brief System capabilities: ZLIB compression
             **/
            public bool ZLib;

            /**
             * \brief UI capabilities: Printer
             **/
            public bool Printer;

            /**
             * \brief UI capabilities: Printer with graphics
             **/
            public bool PrinterGraphics;

            /**
             * \brief UI capabilities: Display
             **/
            public bool Display;

            /**
             * \brief UI capabilities: Images
             **/
            public bool Images;

            /**
             * \brief UI capabilities: Audio
             **/
            public bool Audio;

            /**
             * \brief UI capabilities: Animation
             **/
            public bool Animation;

            /**
             * \brief UI capabilities: Video
             **/
            public bool Video;

            /**
             * \brief Checkout capabilities: Payment
             **/
            public bool Payment;

            /**
             * \brief Checkout capabilities: Digital receipt
             **/
            public bool DigitalReceipt;

            /**
             * \brief Checkout capabilities: Service issuance
             **/
            public bool ServiceIssuance;

            /**
             * \brief Checkout capabilities: OTA POS data
             **/
            public bool OTAPOSData;

            /**
             * \brief CVM capabilities: Online PIN
             **/
            public bool OnlinePIN;

            /**
             * \brief CVM capabilities: CD PIN
             **/
            public bool CDPIN;

            /**
             * \brief CVM capabilities: Signature
             **/
            public bool Signature;

            /**
             * \brief CVM capabilities: None
             **/
            public bool NoCVM;

            /**
             * \brief CVM capabilities: Device generated code
             **/
            public bool DeviceGeneratedCode;

            /**
             * \brief CVM capabilities: SP generated code
             **/
            public bool SPGeneratedCode;

            /**
             * \brief CVM capabilities: ID capture
             **/
            public bool IDCapture;

            /**
             * \brief CVM capabilities: Biometric
             **/
            public bool Biometric;

            /**
             * \brief Tap capabilities: VAS only
             **/
            public bool PassOnly;

            /**
             * \brief Tap capabilities: Payment only
             **/
            public bool PaymentOnly;

            /**
             * \brief Tap capabilities: VAS and payment
             **/
            public bool PassAndPayment;

            /**
             * \brief Tap capabilities: VAS over payment
             **/
            public bool PassOverPayment;
        }

        /**
         * \brief Details of the terminal capabilities
         */
        public CapabilitiesDetails Capabilities;

        private Type GetFieldType(string Name)
        {
            if (Name.StartsWith("Capabilities"))
            {
                Name = Name.Substring(12);
                if (string.IsNullOrEmpty(Name)) return null;
                FieldInfo info = typeof(CapabilitiesDetails).GetField(Name);
                return info.FieldType;
            }
            else if (Name.StartsWith("Request"))
            {
                Name = Name.Substring(7);
                if (string.IsNullOrEmpty(Name)) return null;
                FieldInfo info = typeof(RequestDetails).GetField(Name);
                return info.FieldType;
            }
            /*
            else if (Name.StartsWith("Push"))
            {
                Name = Name.Substring(4);
                if (string.IsNullOrEmpty(Name)) return null;
                FieldInfo info = typeof(TPush).GetField(Name);
                return info.FieldType;
            }
            */
            else
            {
                FieldInfo info = typeof(GoogleVasConfig).GetField(Name);
                return info.FieldType;
            }
        }

        /**
         * \brief Helper to access on the properties dynamically (read)
         */
        public object GetFieldValue(string Name)
        {
            if (Name.StartsWith("Capabilities"))
            {
                Name = Name.Substring(12);
                if (string.IsNullOrEmpty(Name)) return null;
                FieldInfo info = typeof(CapabilitiesDetails).GetField(Name);
                return info.GetValue(this.Capabilities);
            }
            else if (Name.StartsWith("Request"))
            {
                Name = Name.Substring(7);
                if (string.IsNullOrEmpty(Name)) return null;
                FieldInfo info = typeof(RequestDetails).GetField(Name);
                return info.GetValue(this.Request);
            }
            /*
            else if (Name.StartsWith("Push"))
            {
                Name = Name.Substring(4);
                FieldInfo info = typeof(TPush).GetField(Name);
                return info.GetValue(this.Push);
            }
            */
            else
            {
                FieldInfo info = typeof(GoogleVasConfig).GetField(Name);
                return info.GetValue(this);
            }
        }

        /**
         * \brief Helper to access on the properties dynamically (write)
         */
        public void SetFieldValue(string Name, object Value)
        {
            if (Name.StartsWith("Capabilities"))
            {
                Name = Name.Substring(12);
                if (string.IsNullOrEmpty(Name)) return;
                FieldInfo info = typeof(CapabilitiesDetails).GetField(Name);
                info.SetValue(this.Capabilities, Value);
            }
            else if (Name.StartsWith("Request"))
            {
                Name = Name.Substring(7);
                if (string.IsNullOrEmpty(Name)) return;
                FieldInfo info = typeof(RequestDetails).GetField(Name);
                info.SetValue(this.Request, Value);
            }
            /*
            else if (Name.StartsWith("Push"))
            {
                Name = Name.Substring(4);
                FieldInfo info = typeof(TPush).GetField(Name);
                info.SetValue(this.Push, Value);
            }
            */
            else
            {
                FieldInfo info = typeof(GoogleVasConfig).GetField(Name);
                info.SetValue(this, Value);
            }
        }

        /**
         * \brief Load the configuration from a JSON config file to a GoogleVasConfig object.
         **/
        public void LoadFromJson(IConfigReader configReader)
        {
            string s;

            SmartTapVersion = BinConvert.HexToWord(configReader.ReadString("smarttap_version"));

            UseOSE = configReader.ReadBoolean("use_ose");
            UseEncryption = configReader.ReadBoolean("encryption_supported");
            AllowSkippingSmartTap2Select = configReader.ReadBoolean("allow_skipping_smart_tap_2_select");

            DoPayment = configReader.ReadBoolean("request_payment");
            DoRequest = configReader.ReadBoolean("send_service_request");

            Request.PrivateLabelCards = configReader.ReadBoolean("request_private_label_cards");
            Request.AllValuablesExceptPpse = configReader.ReadBoolean("request_all_valuables_except_ppse");
            Request.AllValuables = configReader.ReadBoolean("request_all_valuables");
            Request.Offers = configReader.ReadBoolean("request_offers");
            Request.WalletCustomer = configReader.ReadBoolean("request_customer_info");
            Request.LoyaltyPrograms = configReader.ReadBoolean("request_loyalty_programs");
            Request.GiftCards = configReader.ReadBoolean("request_gift_cards");
            Request.EventTickets = configReader.ReadBoolean("request_event_tickets");
            Request.Flights = configReader.ReadBoolean("request_flights");

            /*
            Push.Receipt = configReader.ReadBoolean("push_receipt");
            Push.ManualUsage = configReader.ReadBoolean("push_manual_usage");
            Push.Signup = configReader.ReadBoolean("push_signup");
            Push.Survey = configReader.ReadBoolean("push_survey");
            Push.ValuableUsage = configReader.ReadBoolean("push_valuable_usage");
            Push.Goods = configReader.ReadBoolean("preference_push_goods");
            Push.NewValuable = configReader.ReadBoolean("preference_push_new_valuable");
            */

            Capabilities.Standalone = configReader.ReadBoolean("capabilities_standalone");
            Capabilities.SemiIntegrated = configReader.ReadBoolean("capabilities_semi_integrated");
            Capabilities.Unattended = configReader.ReadBoolean("capabilities_unattended");
            Capabilities.Online = configReader.ReadBoolean("capabilities_online");
            Capabilities.Offline = configReader.ReadBoolean("capabilities_offline");
            Capabilities.MMP = configReader.ReadBoolean("capabilities_mmp");
            Capabilities.ZLib = configReader.ReadBoolean("capabilities_zlib");

            Capabilities.Printer = configReader.ReadBoolean("capabilities_printer");
            Capabilities.PrinterGraphics = configReader.ReadBoolean("capabilities_printer_graphics");
            Capabilities.Display = configReader.ReadBoolean("capabilities_display");
            Capabilities.Images = configReader.ReadBoolean("capabilities_images");
            Capabilities.Audio = configReader.ReadBoolean("capabilities_audio");
            Capabilities.Animation = configReader.ReadBoolean("capabilities_animation");
            Capabilities.Video = configReader.ReadBoolean("capabilities_video");

            Capabilities.Payment = configReader.ReadBoolean("capabilities_payment");
            Capabilities.DigitalReceipt = configReader.ReadBoolean("capabilities_digital_receipt");
            Capabilities.ServiceIssuance = configReader.ReadBoolean("capabilities_service_issuance");
            Capabilities.OTAPOSData = configReader.ReadBoolean("capabilities_ota_pos_data");

            Capabilities.OnlinePIN = configReader.ReadBoolean("capabilities_online_pin");
            Capabilities.CDPIN = configReader.ReadBoolean("capabilities_cd_pin");
            Capabilities.Signature = configReader.ReadBoolean("capabilities_signature");
            Capabilities.NoCVM = configReader.ReadBoolean("capabilities_none");
            Capabilities.DeviceGeneratedCode = configReader.ReadBoolean("capabilities_device_gen_code");
            Capabilities.SPGeneratedCode = configReader.ReadBoolean("capabilities_sp_gen_code");
            Capabilities.IDCapture = configReader.ReadBoolean("capabilities_id_capture");
            Capabilities.Biometric = configReader.ReadBoolean("capabilities_biometric");

            Capabilities.PassOnly = configReader.ReadBoolean("capabilities_vas_only");
            Capabilities.PaymentOnly = configReader.ReadBoolean("capabilities_payment_only");
            Capabilities.PassAndPayment = configReader.ReadBoolean("capabilities_vas_and_payment");
            Capabilities.PassOverPayment = configReader.ReadBoolean("capabilities_vas_over_payment");

            PresignedAuth = configReader.ReadBoolean("presigned_auth");
            LongTermKeyVersion = configReader.ReadUnsigned("long_term_key_version");
            LongTermPrivateKey = BinConvert.HexToBytes(configReader.ReadString("terminal_long_term_private_key"));

            ForceTerminalNonce = BinConvert.HexToBytes(configReader.ReadString("terminal_nonce"));
            ForceTerminalEphemeralPrivateKey = BinConvert.HexToBytes(configReader.ReadString("terminal_ephemeral_private_key"));

            s = configReader.ReadString("collector_id");
            if (!string.IsNullOrEmpty(s))
                CollectorId = BinUtils.FromDword(BinConvert.ParseDW(s));
            s = configReader.ReadString("location_id");
            if (!string.IsNullOrEmpty(s))
                LocationId = BinConvert.HexToBytes(s);
            s = configReader.ReadString("terminal_id");
            if (!string.IsNullOrEmpty(s))
                TerminalId = BinConvert.HexToBytes(s);
            s = configReader.ReadString("merchant_name");
            if (!string.IsNullOrEmpty(s))
                MerchantName = s;
             MerchantCategoryCode = (ushort)(configReader.ReadUnsigned("merchant_category") & 0xFFFF);

            Validate();
        }

        /**
         * \brief Get an array of 5 bytes representing the Capabilities configured in the terminal
         **/
        public static byte[] GetCapabilitiesByteArray(GoogleVasConfig config)
        {
            byte[] result = new byte[5] { 0x00, 0x00, 0x00, 0x00, 0x00 };

            if (config.Capabilities.Standalone)
                result[0] |= 0x01;
            if (config.Capabilities.SemiIntegrated)
                result[0] |= 0x02;
            if (config.Capabilities.Unattended)
                result[0] |= 0x04;
            if (config.Capabilities.Online)
                result[0] |= 0x08;
            if (config.Capabilities.Offline)
                result[0] |= 0x10;
            if (config.Capabilities.MMP)
                result[0] |= 0x20;
            if (config.Capabilities.ZLib)
                result[0] |= 0x40;

            if (config.Capabilities.Printer)
                result[1] |= 0x01;
            if (config.Capabilities.PrinterGraphics)
                result[1] |= 0x02;
            if (config.Capabilities.Display)
                result[1] |= 0x04;
            if (config.Capabilities.Images)
                result[1] |= 0x08;
            if (config.Capabilities.Audio)
                result[1] |= 0x10;
            if (config.Capabilities.Animation)
                result[1] |= 0x20;
            if (config.Capabilities.Video)
                result[1] |= 0x40;

            if (config.Capabilities.Payment)
                result[2] |= 0x01;
            if (config.Capabilities.DigitalReceipt)
                result[2] |= 0x02;
            if (config.Capabilities.ServiceIssuance)
                result[2] |= 0x04;
            if (config.Capabilities.OTAPOSData)
                result[2] |= 0x08;

            if (config.Capabilities.OnlinePIN)
                result[3] |= 0x01;
            if (config.Capabilities.CDPIN)
                result[3] |= 0x02;
            if (config.Capabilities.Signature)
                result[3] |= 0x04;
            if (config.Capabilities.NoCVM)
                result[3] |= 0x08;
            if (config.Capabilities.DeviceGeneratedCode)
                result[3] |= 0x10;
            if (config.Capabilities.SPGeneratedCode)
                result[3] |= 0x20;
            if (config.Capabilities.IDCapture)
                result[3] |= 0x40;
            if (config.Capabilities.Biometric)
                result[3] |= 0x80;

            if (config.Capabilities.PassOnly)
                result[4] |= 0x01;
            if (config.Capabilities.PaymentOnly)
                result[4] |= 0x02;
            if (config.Capabilities.PassAndPayment)
                result[4] |= 0x04;
            if (config.Capabilities.PassOverPayment)
                result[4] |= 0x08;

            return result;
        }

        /**
         * \brief Get a list of types to request in the Get Data command
         **/
        public static List<GoogleVasServiceType> GetServiceListToRequest(GoogleVasConfig config)
        {
            List<GoogleVasServiceType> serviceTypeToRequests = new List<GoogleVasServiceType>();

            if (config.Request.AllValuables)
            {
                serviceTypeToRequests.Add(GoogleVasServiceType.AllServices);
            }
            else if (config.Request.AllValuablesExceptPpse)
            {
                serviceTypeToRequests.Add(GoogleVasServiceType.AllServicesExceptPPSE);
            }
            else
            {
                if (config.Request.LoyaltyPrograms)
                {
                    serviceTypeToRequests.Add(GoogleVasServiceType.Loyalty);
                }

                if (config.Request.Offers)
                {
                    serviceTypeToRequests.Add(GoogleVasServiceType.Offer);
                }

                if (config.Request.GiftCards)
                {
                    serviceTypeToRequests.Add(GoogleVasServiceType.GiftCard);
                }

                if (config.Request.PrivateLabelCards)
                {
                    serviceTypeToRequests.Add(GoogleVasServiceType.PrivateLabelCard);
                }

                if (config.Request.EventTickets)
                {
                    serviceTypeToRequests.Add(GoogleVasServiceType.EventTicket);
                }

                if (config.Request.Flights)
                {
                    serviceTypeToRequests.Add(GoogleVasServiceType.Flight);
                }

                if (config.Request.Transit)
                {
                    serviceTypeToRequests.Add(GoogleVasServiceType.Transit);
                }

                if (config.Request.WalletCustomer)
                {
                    serviceTypeToRequests.Add(GoogleVasServiceType.WalletCustomer);
                }

                // TODO CRA: for the moment those are not specified in the test case json
                // But they are specified in the doc https://developers.google.com/pay/smart-tap/reference/apdu-commands/get-data
#if false
                if (config.Request.)
                {
                    serviceTypeToRequests.Add(GoogleVasServiceType.CloudBasedWallet);
                }

                if (config.Request.)
                {
                    serviceTypeToRequests.Add(GoogleVasServiceType.MobileMarketingPlatform);
                }


                if (config.Request.)
                {
                    serviceTypeToRequests.Add(GoogleVasServiceType.PPSE);
                }
#endif
            }
            return serviceTypeToRequests;
        }
        /**
         * \brief Save configuration to a specific file or register
         **/
        private void Save(IConfigWriter configWriter, string Name)
        {
            Type type = GetFieldType(Name);
            object value = GetFieldValue(Name);

            if (type == null)
            {
                if ((Name != "Request") && (Name != "Push") && (Name != "Capabilities"))
                    Logger.Warning("Unknown config field {0}", Name);
            }
            else if (type == typeof(byte[]))
            {
                string strValue = BinConvert.ToHex((byte[])value);
                configWriter.WriteString(Name, strValue);
            }
            else if (type == typeof(bool))
            {
                configWriter.WriteBoolean(Name, (bool)value);
            }
            else if (type == typeof(byte))
            {
                configWriter.WriteUnsigned(Name, (byte)value);
            }
            else if (type == typeof(ushort))
            {
                configWriter.WriteUnsigned(Name, (ushort)value);
            }
            else if (type == typeof(uint))
            {
                configWriter.WriteUnsigned(Name, (uint)value);
            }
            else if (type == typeof(int))
            {
                configWriter.WriteInteger(Name, (int)value);
            }
            else if (type == typeof(string))
            {
                configWriter.WriteString(Name, (string)value);
            }
            else
            {
                Logger.Warning("Unsupported type {1} for config field {0}", Name, type);
            }
        }
        /**
         * \brief Save configuration to a file or a register
         **/
        public void Save(IConfigWriter configWriter)
        {
            foreach (FieldInfo info in typeof(GoogleVasConfig).GetFields())
                Save(configWriter, info.Name);
            foreach (FieldInfo info in typeof(CapabilitiesDetails).GetFields())
                Save(configWriter, "Capabilities" + info.Name);
            foreach (FieldInfo info in typeof(RequestDetails).GetFields())
                Save(configWriter, "Request" + info.Name);
            //foreach (FieldInfo info in typeof(PushDetails).GetFields())
            //    Save(configWriter, "Push" + info.Name);
        }

        private void Load(IConfigReader configReader, string Name)
        {
            Type type = GetFieldType(Name);

            if (type == null)
            {
                if ((Name != "Request") && (Name != "Push") && (Name != "Capabilities"))
                    Logger.Warning("Unknown config field {0}", Name);
            }
            else if (type == typeof(byte[]))
            {
                string Value = configReader.ReadString(Name);
                SetFieldValue(Name, BinConvert.HexToBytes(Value));
            }
            else if (type == typeof(bool))
            {
                bool Value = configReader.ReadBoolean(Name);
                SetFieldValue(Name, configReader.ReadBoolean(Name));
            }
            else if (type == typeof(byte))
            {
                uint Value = configReader.ReadUnsigned(Name);
                SetFieldValue(Name, (byte)Value);
            }
            else if (type == typeof(ushort))
            {
                uint Value = configReader.ReadUnsigned(Name);
                SetFieldValue(Name, (ushort) Value);
            }
            else if (type == typeof(uint))
            {
                uint Value = configReader.ReadUnsigned(Name);
                SetFieldValue(Name, Value);
            }
            else if (type == typeof(int))
            {
                int Value = configReader.ReadInteger(Name);
                SetFieldValue(Name, Value);
            }
            else if (type == typeof(string))
            {
                string Value = configReader.ReadString(Name);
                SetFieldValue(Name, Value);
            }
            else
            {
                Logger.Warning("Unsupported type {1} for config field {0}", Name, type);
            }
        }

        /**
         * \brief Load configuration
         **/
        public void Load(IConfigReader configReader)
        {
            foreach (FieldInfo info in typeof(GoogleVasConfig).GetFields())
                Load(configReader, info.Name);
            foreach (FieldInfo info in typeof(CapabilitiesDetails).GetFields())
                Load(configReader, "Capabilities" + info.Name);
            foreach (FieldInfo info in typeof(RequestDetails).GetFields())
                Load(configReader, "Request" + info.Name);
            //foreach (FieldInfo info in typeof(PushDetails).GetFields())
            //    Load(configWriter, "Push" + info.Name);
            Validate();
        }

        /**
         * \brief Load a configuration from a JSON file (using the format of Google-provided test Terminal App)
         */
        public static GoogleVasConfig LoadFromJsonFile(string FileName)
        {
            if (!File.Exists(FileName))
            {
                Logger.Error($"File not found: {FileName}");
                return null;
            }

            IConfigReader configReader = JsonCfgFile.OpenReadOnly(FileName, "preference_");
            if (configReader == null)
            {
                Logger.Error("Invalid Config file!");
                return null;
            }
                
            GoogleVasConfig result = new GoogleVasConfig();
            result.LoadFromJson(configReader);
            return result;
        }

        /**
         * \brief Save config object to file in multiconf format (for SpringCore device)
         **/
        public static bool SaveToSpringCoreConfigFile(GoogleVasConfig config, string fileName)
        {
            /* Clear old File */
            if(File.Exists(fileName))
            {
                File.Delete(fileName);
            }
            CfgFile cfgFile = new CfgFile(fileName, false, false);

            /* Init extra index and values */
            UInt16 index = 0x0501;
            byte[] value = new byte[0];
            List<byte> valueList = new List<byte>();


            /* Multiconf file headers */
            cfgFile.WriteName("[general]");
            cfgFile.WriteString("product", "score-full");
            cfgFile.WriteName("[raw]");

            /* Short template part */
            /*---------------------*/

            /* 0x0310 - Template = Google Smart Tap */
            WriteRegister(cfgFile, 0x0310,  0xD2);

            /* 0x0313 - Collector ID */
            WriteRegister(cfgFile, 0x0313, config.CollectorId_4);

            /* 0x0314 - Long Term Key version */
            value = BitConverter.GetBytes(config.LongTermKeyVersion);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(value);
            }
            WriteRegister(cfgFile, 0x0314, value);

            /* 0x0315 - Long Term Key */
            WriteRegister(cfgFile, 0x0315, config.LongTermPrivateKey);

            /* Extended template part */
            /*------------------------*/

            /* 0x0501 - Capabilities */
            value = GoogleVasConfig.GetCapabilitiesByteArray(config);
            WriteRegister(cfgFile, index++, value);

            /* 0x0502 - Request Types */
            List<GoogleVasServiceType> serviceTypeToRequests = GoogleVasConfig.GetServiceListToRequest(config);
            valueList.Clear();
            foreach(GoogleVasServiceType type in serviceTypeToRequests)
            {
                valueList.Add((byte)type);
            }
            WriteRegister(cfgFile, index++, valueList.ToArray());

            /* 0x0503 - UseOSE */
            WriteRegister(cfgFile, index++, config.UseOSE);

            /* 0x0504 - UseEncryption */
            WriteRegister(cfgFile, index++, config.UseEncryption);

            /* 0x0505 - AllowSkippingSmartTap2Select */
            WriteRegister(cfgFile, index++, config.AllowSkippingSmartTap2Select);

            /* 0x0506 - DoPayment */
            WriteRegister(cfgFile, index++, config.DoPayment);

            /* 0x0507 - DoRequest */
            WriteRegister(cfgFile, index++, config.DoRequest);

            /* 0x0508 - PresignedAuth */
            WriteRegister(cfgFile, index++, config.PresignedAuth);

            /* 0x0509 - ForceTerminalNonce */
            WriteRegister(cfgFile, index++, config.ForceTerminalNonce);

            /* 0x050A - ForceTerminalEphemeralPrivateKey */
            WriteRegister(cfgFile, index++, config.ForceTerminalEphemeralPrivateKey);

            /* 0x050B - LocationId */
            WriteRegister(cfgFile, index++, config.LocationId);

            /* 0x050C - MerchantName */
            if(config.MerchantName != null && config.MerchantName.Length > 0)
            {
                WriteRegister(cfgFile, index++, BinConvert.AsciiToHex(config.MerchantName));
            }
            else
            {
                WriteRegister(cfgFile, index++, "");
            }

            /* 0x050D - MerchantCategoryCode */
            WriteRegister(cfgFile, index++, BinConvert.ToHex(config.MerchantCategoryCode));

            /* 0x050E - TerminalId */
            WriteRegister(cfgFile, index++, config.TerminalId);

            /* Save file */
            if (!cfgFile.Save())
            {
                Logger.Warning("Saving failed");
                return false;
            }

            return true;
        }

        private static void WriteRegister(CfgFile cfgFile, UInt16 registerIndex, bool value)
        {
            byte byteValue = 0;
            if (value)
                byteValue = 0x01;
            else
                byteValue = 0x00;
            WriteRegister(cfgFile, registerIndex, BinConvert.ToHex(byteValue));
        }

        private static void WriteRegister(CfgFile cfgFile, UInt16 registerIndex, byte value)
        {
            WriteRegister(cfgFile, registerIndex, BinConvert.ToHex(value));
        }

        private static void WriteRegister(CfgFile cfgFile, UInt16 registerIndex, byte[] value)
        {
            WriteRegister(cfgFile, registerIndex, BinConvert.ToHex(value));
        }

        private static void WriteRegister(CfgFile cfgFile, UInt16 registerIndex, string value)
        {
            if(value != null && value.Length > 0)
            {
                cfgFile.WriteString(BinConvert.ToHex(registerIndex), value);
            }
            else
            {
                cfgFile.WriteString(BinConvert.ToHex(registerIndex), "");
            }
        }

        /**
         * \brief Load SpringCard's default test configuration
         */
        public static GoogleVasConfig GoogleDemo()
        {
            GoogleVasConfig result = new GoogleVasConfig();

            result.SmartTapVersion = 0x0001;
            result.UseOSE = true;
            result.AllowSkippingSmartTap2Select = true;

            ulong.TryParse("20180608", out ulong qwCollectorId);
            result.CollectorId = BinUtils.FromQword(qwCollectorId);
            Logger.Debug("{0}", BinConvert.ToHex(result.CollectorId));

            result.DoRequest = true;
            result.Request.AllValuables = true;

            result.Capabilities.PassOnly = true;

            result.UseEncryption = true;
            result.LongTermKeyVersion = 1;
            //result.LongTermPrivateKeyIndex = 0x0D;
            result.LongTermPrivateKey = BinConvert.HexToBytes("826d17e50767b165b0e4d9e332f8d1d1e20224284fb4daf1e50a03246e70797d");

            result.Validate();
            return result;
        }

        /**
         * \brief Load SpringCard's default test configuration
         */
        public static GoogleVasConfig SpringCardDemo()
        {
            GoogleVasConfig result = new GoogleVasConfig();

            result.SmartTapVersion = 0x0001;
            result.UseOSE = true;
            result.AllowSkippingSmartTap2Select = false;

            ulong.TryParse("50888355", out ulong qwCollectorId);
            result.CollectorId = BinUtils.FromQword(qwCollectorId);
            Logger.Debug("{0}", BinConvert.ToHex(result.CollectorId));

            result.DoRequest = true;
            result.Request.AllValuables = true;

            result.Capabilities.PassOnly = true;

            result.UseEncryption = true;
            result.LongTermKeyVersion = 1;
            //result.LongTermPrivateKeyIndex = 0x0D;
            result.LongTermPrivateKey = BinConvert.HexToBytes("A2977C3661CD84CAF4E0FE9D3468493A5AC8F75679E22601AAD2DCD5979FCB83");
            //A3F3D54CFD1E811B7A4CF5FB6521421F98731090F8A40C1FDE617A290FA5D4AC DEMO
            //A2977C3661CD84CAF4E0FE9D3468493A5AC8F75679E22601AAD2DCD5979FCB83 SAS

            result.Validate();
            return result;
        }
    }
}
