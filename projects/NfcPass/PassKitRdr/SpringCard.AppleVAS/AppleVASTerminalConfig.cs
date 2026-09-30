/**
 *
 * \ingroup AppleVAS
 * 
 */
using SpringCard.LibCs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpringCard.AppleVas
{
    /**
	 * \brief Encapsulate the global configuration of an Apple VAS terminal : operation mode, capabilites, and list of merchant configurations
	 **/
    public class AppleVasTerminalConfig
    {
        /**
		 * \brief Terminal's P2 value for URL Only
		 **/
        public const byte P2_URL_Only = 0x00;
        /**
		 * \brief Terminal's P2 value for Full VAS
		 **/
        public const byte P2_Full_VAS = 0x01;
        /**
		 * \brief Terminal's Capability byte 3 (SET#1) for Dual Mode (VAS app AND payment)
		 **/
        public const byte CapabilitesSet1_DualMode = 0x00;
        /**
		 * \brief Terminal's Capability byte 3 (SET#1) for Single Mode (VAS app OR payment)
		 **/
        public const byte CapabilitesSet1_SingleMode = 0x01;
        /**
		 * \brief Terminal's Capability byte 3 (SET#1) for VAS Only (no payment at all)
		 **/
        public const byte CapabilitesSet1_VASOnly = 0x02;

        /**
		 * \brief Default configuration of the terminal - Suits most use cases
		 **/
        public static readonly AppleVasTerminalConfig Default = new AppleVasTerminalConfig();

        /**
		 * \brief Actual value of Terminal's P2
		 **/
        public byte P2 { get; private set; } = P2_Full_VAS;
        /**
		 * \brief Actual value of Terminal's Capability byte 3 (SET#1)
		 **/
        public byte CapabilitiesSet1 { get; private set; } = CapabilitesSet1_VASOnly;
        /**
		 * \brief Merchant(s) supported by the Terminal
		 **/
        public List<AppleVasConfig> Merchants { get; private set; } = new List<AppleVasConfig>();
        /**
		 * \brief Description of the configuration (for test only)
		 **/
        public string Description { get; private set; }


        public bool IsValid()
        {
            if ((Merchants == null) || (Merchants.Count == 0))
            {
                Logger.Error("No Merchant in terminal configuration");
                return false;
            }

            foreach (AppleVasConfig merchant in Merchants)
            {
                if (merchant == null)
                {
                    Logger.Error("Null Merchant in terminal configuration");
                    return false;
                }
                if (merchant.Id == null)
                {
                    Logger.Error("Merchant without a MerchantId in terminal configuration");
                    return false;
                }
                if ((merchant.privateKeys == null) || (merchant.privateKeys.Count == 0))
                {
                    Logger.Error("Merchant without a PrivateKey in terminal configuration");
                    return false;
                }
            }

            return true;
        }

        /**
		 * \brief Instantiate an empty Terminal configuration (copy of the default configuration until modified)
		 **/
        public AppleVasTerminalConfig()
        {

        }

        /**
         * \brief Import a terminal configuration form a JSON object
         **/
        public static AppleVasTerminalConfig LoadFromJson(JSONObject jsonData)
        {
            string strP2 = jsonData.GetString("P2");
            string strCapabilities = jsonData.GetString("Capabilities");
            JSONObject arrMerchants = jsonData.Get("Merchants");

            AppleVasTerminalConfig result = new AppleVasTerminalConfig();

            result.Description = jsonData.GetString("Description");

            if (!string.IsNullOrEmpty(strP2))
            {
                switch (strP2.ToLower())
                {
                    case "urlonly": result.P2 = P2_URL_Only; break;
                    case "fullvas": result.P2 = P2_Full_VAS; break;
                    default: throw new InvalidDataException("P2 shall be URLOnly or FullVAS");
                }
                Logger.Debug("P2={0}->{1:X02}", strP2, result.P2);
            }

            if (!string.IsNullOrEmpty(strCapabilities))
            {
                switch (strCapabilities.ToLower())
                {
                    case "vasmode": 
                    case "vasonly":
                        result.CapabilitiesSet1 = CapabilitesSet1_VASOnly;
                        break;
                    case "dualmode":
                        result.CapabilitiesSet1 = CapabilitesSet1_DualMode;
                        break;
                    case "singlemode":
                        result.CapabilitiesSet1 = CapabilitesSet1_SingleMode;
                        break;
                    default: throw new InvalidDataException("Capabilites shall be VASOnly, DualMode or SingleMode");
                }
                Logger.Debug("Capabilites={0}->{1:X02}", strCapabilities, result.CapabilitiesSet1);
            }

            foreach (JSONObject jsonMerchantData in arrMerchants.ArrayValue)
            {
                AppleVasConfig merchant = AppleVasConfig.LoadFromJson(jsonMerchantData);
                Logger.Debug("Merchant config is OK");
                result.Merchants.Add(merchant);
            }

            Logger.Debug("Terminal config is OK");
            Logger.Info(result.Description);

            return result;
        }

        /**
         * \brief Import a terminal configuration form a JSON string. Raise an exception on error
         **/
        public static AppleVasTerminalConfig LoadFromJson(string jsonString)
        {
            JSONObject jsonData;

            jsonData = JSONDecoder.Decode(jsonString);

            return LoadFromJson(jsonData);
        }

        /**
         * \brief Import a terminal configuration form a JSON string. Return false on error
         **/
        public static bool TryLoadFromJson(string jsonString, out AppleVasTerminalConfig terminalConfig)
        {
            try
            {
                terminalConfig = LoadFromJson(jsonString);
            }
            catch (Exception e)
            {
                Logger.Trace("Cant't load config from JSON ({1})", e.Message);
                terminalConfig = null;
                return false;
            }

            return true;
        }

        /**
         * \brief Import a terminal configuration form a JSON file. Raise an exception on error
         **/
        public static AppleVasTerminalConfig LoadFromJsonFile(string fileName)
        {
            string json;

            json = File.ReadAllText(fileName);

            return LoadFromJson(json);
        }

        /**
         * \brief Import a terminal configuration form a JSON file. Return false on error
         **/
        public static bool TryLoadFromJsonFile(string fileName, out AppleVasTerminalConfig terminalConfig)
        {
            try
            {
                terminalConfig = LoadFromJsonFile(fileName);
            }
            catch (Exception e)
            {
                Logger.Trace("Cant't load config from file {0} ({1})", fileName, e.Message);
                terminalConfig = null;
                return false;
            }

            return true;
        }

        public static AppleVasTerminalConfig SpringCardDemo()
        {
            AppleVasTerminalConfig result = new AppleVasTerminalConfig();
            result.Merchants.Add(AppleVasConfig.SpringCardDemo());
            return result;
        }

    }
}
