using System;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Reflection;
using SpringCard.PCSC;
using SpringCard.LibCs;

namespace SpringCard.PCSC.CardHelpers
{
    public partial class SamAV
    {
        private byte[] VersionData;

        public bool ReadVersion()
        {
            CAPDU capdu = new CAPDU(0x80, 0x60, 0x00, 0x00, 0x00);
            RAPDU rapdu = samReader.Transmit(capdu);

            if (rapdu == null)
            {
                OnCommunicationError();
                return false;
            }
            if (rapdu.SW != 0x9000)
            {
                OnStatusWordError(rapdu.SW);
                return false;
            }

            VersionData = rapdu.DataBytes;
            Logger.Debug("VersionData={0}", BinConvert.ToHex(VersionData));
            return true;
        }

        public enum SamVersionE
        {
            Unknown,
            AV1,
            AV1_on_AV2,
            AV2
        }

        public SamVersionE Version
        {
            get
            {
                if (VersionData == null)
                    if (!ReadVersion())
                        return SamVersionE.Unknown;

                if (VersionData.Length == 30)
                {
                    return SamVersionE.AV1;
                }
                else if (VersionData.Length == 31)
                {
                    switch (VersionData[30])
                    {
                        case 0xA1:
                            return SamVersionE.AV1_on_AV2;
                        case 0xA2:
                            return SamVersionE.AV2;
                    }
                }

                return SamVersionE.Unknown;
            }
        }

        public byte[] Uid
        {
            get
            {
                if (VersionData == null)
                    if (!ReadVersion())
                        return null;

                if (VersionData.Length >= 30)
                {
                    return BinUtils.Copy(VersionData, 14, 7);
                }

                return null;
            }
        }

        public bool SamIsAV1()
        {
            Debug("Checking if SAM is in AV1 mode ...");

            byte[] capdu_byte = { 0x80, 0x60, 0x00, 0x00, 0x00 };
            CAPDU capdu = new CAPDU(capdu_byte);
            RAPDU rapdu = samReader.Transmit(capdu);
            if (rapdu == null)
            {
                OnCommunicationError();
                return false;
            }

            if (rapdu.GetBytes().Length != 33)
            {
                userInteraction.Error("The response from 'Get Version' command is invalid\nThe SAM may not be from NXP");
                Debug("The response from 'Get Version' command is invalid. The SAM may not be from NXP: " + rapdu.AsString());
                return false;
            }

            if (rapdu.GetByte(rapdu.GetBytes().Length - 3) != 0xA1)
            {
                Debug("The SAM is not an AV1. Response from GetVersion: " + rapdu.AsString());
                return false;
            }
            else
            {
                return true;
            }
        }

        public string GetVersionString()
        {
            Debug("------------------------------Getting version------------------------------");

            byte[] capdu_byte = { 0x80, 0x60, 0x00, 0x00, 0x00 };

            CAPDU capdu = new CAPDU(capdu_byte);
            RAPDU rapdu = samReader.Transmit(capdu);
            if (rapdu == null)
            {
                OnCommunicationError();
                return null;
            }

            Debug("capdu=" + capdu.AsString());
            Debug("rapdu=" + rapdu.AsString());

            if (rapdu.GetBytes().Length != 33)
            {
                userInteraction.Error("The response from 'Get Version' command is invalid\nThe SAM may not be from NXP.");
                Debug("The response from 'Get Version' command is invalid. The SAM may not be from NXP: " + rapdu.AsString());
                return null;
            }

            string ret = "";
            byte[] get_version = rapdu.GetBytes();
            ret += "--- Hardware version ---" + "\n";
            ret += "Vendor ID: " + String.Format("{0:X02}", get_version[0]) + "\n";
            ret += "Type: " + String.Format("{0:X02}", get_version[1]) + "\n";
            ret += "Subtype: " + String.Format("{0:X02}", get_version[2]) + "\n";
            ret += "Major version number: " + String.Format("{0:X02}", get_version[3]) + "\n";
            ret += "Minor version number: " + String.Format("{0:X02}", get_version[4]) + "\n";
            ret += "Storage size: " + String.Format("{0:X02}", get_version[5]) + "\n";
            ret += "Communication protocol type: " + String.Format("{0:X02}", get_version[6]) + "\n";

            ret += "\n--- Software version ---" + "\n";
            ret += "Vendor ID: " + String.Format("{0:X02}", get_version[7]) + "\n";
            ret += "Type: " + String.Format("{0:X02}", get_version[8]) + "\n";
            ret += "Subtype: " + String.Format("{0:X02}", get_version[9]) + "\n";
            ret += "Major version number: " + String.Format("{0:X02}", get_version[10]) + "\n";
            ret += "Minor version number: " + String.Format("{0:X02}", get_version[11]) + "\n";
            ret += "Storage size: " + String.Format("{0:X02}", get_version[12]) + "\n";
            ret += "Communication protocol type: " + String.Format("{0:X02}", get_version[13]) + "\n";

            ret += "\n--- Manufacturer data ---" + "\n";
            ret += "Unique serial number: "
                              + String.Format("{0:X02}", get_version[14]) + " "
                                 + String.Format("{0:X02}", get_version[15]) + " "
                                 + String.Format("{0:X02}", get_version[16]) + " "
                                 + String.Format("{0:X02}", get_version[17]) + " "
                                 + String.Format("{0:X02}", get_version[18]) + " "
                                 + String.Format("{0:X02}", get_version[19]) + " "
                                 + String.Format("{0:X02}", get_version[20]) + " "
                                + "\n";

            ret += "Production batch number: "
                                 + String.Format("{0:X02}", get_version[21]) + " "
                                 + String.Format("{0:X02}", get_version[22]) + " "
                                 + String.Format("{0:X02}", get_version[23]) + " "
                                 + String.Format("{0:X02}", get_version[24]) + " "
                                 + String.Format("{0:X02}", get_version[25]) + " "
                                 + "\n";

            ret += "Day of production: " + String.Format("{0:X02}", get_version[26]) + "\n";
            ret += "Month of production: " + String.Format("{0:X02}", get_version[27]) + "\n";
            ret += "Year of production: " + String.Format("{0:X02}", get_version[28]) + "\n";
            ret += "Global crypto settings: " + String.Format("{0:X02}", get_version[29]) + "\n";
            ret += "Mode: " + String.Format("{0:X02}", get_version[30]) + "\n";

            return ret;
        }

        public string GetSernoString()
        {
            byte[] capdu_byte = { 0x80, 0x60, 0x00, 0x00, 0x00 };
            CAPDU capdu = new CAPDU(capdu_byte);
            RAPDU rapdu = samReader.Transmit(capdu);
            if (rapdu == null)
            {
                OnCommunicationError();
                return null;
            }

            if (rapdu.GetBytes().Length != 33)
            {
                userInteraction.Error("The response from 'Get Version' command is invalid\nThe SAM may not be from NXP");
                Debug("The response from 'Get Version' command is invalid. The SAM may not be from NXP: " + rapdu.AsString());
                return null;
            }

            /*
			if (rapdu.GetByte(rapdu.GetBytes().Length - 3) != 0xA2)
			{
				userInterface.Error("The SAM is not an AV2.", "Error getting SAM version");
				log("The SAM is not an AV2: " + rapdu.AsString());
				return null;				
			}
			*/

            Debug("SAM full version =" + rapdu.AsString());
            string ret = "";
            byte[] get_version = rapdu.GetBytes();
            ret += String.Format("{0:X02}", get_version[14]) + " "
                     + String.Format("{0:X02}", get_version[15]) + " "
                     + String.Format("{0:X02}", get_version[16]) + " "
                     + String.Format("{0:X02}", get_version[17]) + " "
                     + String.Format("{0:X02}", get_version[18]) + " "
                     + String.Format("{0:X02}", get_version[19]) + " "
                     + String.Format("{0:X02}", get_version[20]);

            return ret;
        }
    }
}