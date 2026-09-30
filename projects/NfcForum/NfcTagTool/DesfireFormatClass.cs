using SpringCard.LibCs;
using SpringCard.PCSC;
using SpringCard.PCSC.CardHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpringCardApplication
{
    class DesfireFormatClass
    {
        SCardChannel cardchannel;
        Desfire desfire;

        readonly byte[] BlankKey = new byte[16] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

        readonly byte[] ISO_AID = new byte[] { 0xD2, 0x76, 0x00, 0x00, 0x85, 0x01, 0x01 };

        const ushort ISO_DF = 0xE100;
        const ushort ISO_CC_File = 0xE103;
        const ushort ISO_Data_File = 0xE104;

        const uint DF_Aid = 0xEEEE10;
        const byte DF_CC_File = 0x03;
        const byte DF_Data_File = 0x04;

        public DesfireFormatClass(SCardChannel cardchannel)
        {
            this.cardchannel = cardchannel;               
        }

        public bool Verify()
        {
            desfire = new Desfire(cardchannel, Desfire.DesfireIsoWrapping.Card);

            long rc = desfire.GetVersion(out Desfire.DesfireVersionInfo VersionInfo);
            if (rc != SCARD.S_SUCCESS)
            {
                Logger.Warning("Desfire 'get version' command failed.");
                return false;
            }

            Logger.Trace("Found a Desfire card, VersionInfo={0}", VersionInfo.ToString());
            Logger.Trace("SofwareVersion={0}", VersionInfo.SwMajorVersion);
            if (VersionInfo.SwMajorVersion < 1)
            {
                Logger.Warning("Software version is below EV1.");
                return false;
            }

            return true;
        }

        public bool Format(byte[] MasterKey)
        {
            long rc;

            Logger.Info("Formating the card");
            Logger.Info("\t.1 Trying to get authenticated over root application");

            rc = desfire.SelectApplication(0);
            if (rc != SCARD.S_SUCCESS)
            {
                Logger.Warning("Desfire 'select application 0' command failed.");
                return false;
            }

            if (MasterKey == null)
            {
                if (AuthenticateDefaultAndSetKey(null))
                {
                    Logger.Info("Authenticated with default key");
                }
                else
                {
                    return false;
                }
            }
            else
            {
                rc = desfire.AuthenticateAes(0, MasterKey);
                if (rc == SCARD.S_SUCCESS)
                {
                    Logger.Info("Authenticated with master key");
                }
                else if (AuthenticateDefaultAndSetKey(MasterKey))
                {
                    Logger.Info("Authenticated with default key, and new master key set");
                    rc = desfire.AuthenticateAes(0, MasterKey);
                    if (rc != SCARD.S_SUCCESS)
                    {
                        Logger.Warning("Failed to authenticate with new master key");
                        return false;
                    }
                    Logger.Info("Now authenticated with new master key");
                }
                else
                {
                    return false;
                }
            }

            Logger.Info("\t.2 Erasing the card completely");

            rc = desfire.FormatPICC();
            if (rc != SCARD.S_SUCCESS)
            {
                Logger.Warning("Failed to format the card");
                return false;
            }

            Logger.Info("\t.3 Creating the NFC Forum application and its files");

            rc = desfire.CreateApplication(DF_Aid, 0xFF, 0x20 | 0, ISO_DF, ISO_AID, (byte)ISO_AID.Length);
            if (rc != SCARD.S_SUCCESS)
            {
                Logger.Warning("Failed to create the NFC Forum application in the card");
                return false;
            }

            rc = desfire.SelectApplication(DF_Aid);
            if (rc != SCARD.S_SUCCESS)
            {
                Logger.Warning("Failed to select the NFC Forum application");
                return false;
            }

            rc = desfire.CreateIsoStdDataFile(DF_CC_File, ISO_CC_File, 0, 0xEEEE, 32);
            if (rc != SCARD.S_SUCCESS)
            {
                Logger.Warning("Failed to create the NFC Forum CC file");
                return false;
            }

            rc = desfire.GetFreeMemory(out var free_bytes);
            if (rc != SCARD.S_SUCCESS)
            {
                Logger.Warning("Get free memory failed");
                return false;
            }

            if (free_bytes < 256)
            {
                Logger.Warning("The card does not have enough memory");
                return false;
            }

            uint ndef_size = free_bytes - 32;
            rc = desfire.CreateIsoStdDataFile(DF_Data_File, ISO_Data_File, 0, 0xEEEE, (int)ndef_size);
            if (rc != SCARD.S_SUCCESS)
            {
                Logger.Warning("Failed to create the NFC Forum data file");
                return false;
            }

            byte[] cc_content = new byte[16];

            cc_content[0] = 0x00;
            cc_content[1] = 0x0F;
            cc_content[2] = 0x20; /* NDEF version 2 */
            cc_content[3] = 0x00;
            cc_content[4] = 0x3B;
            cc_content[5] = 0x00;
            cc_content[6] = 0x34;
            cc_content[7] = 0x04; /* File control tag */
            cc_content[8] = 0x06;
            cc_content[9] = ISO_Data_File >> 8;
            cc_content[10] = ISO_Data_File & 0x00FF;
            cc_content[11] = (byte)(ndef_size >> 8);
            cc_content[12] = (byte)(ndef_size & 0x00FF);
            cc_content[13] = 0x00;
            cc_content[14] = 0x00;

            rc = desfire.WriteData2(DF_CC_File, 0, cc_content.Length, cc_content);
            if (rc != SCARD.S_SUCCESS)
            {
                Logger.Warning("Failed to write the NFC Forum CC file");
                return false;
            }

            return true;
        }

        public bool AuthenticateDefaultAndSetKey(byte[] MasterKey)
        {
            long rc;

            rc = desfire.AuthenticateAes(0, BlankKey);
            if (rc == SCARD.S_SUCCESS)
            {
                Logger.Info("Authenticated with default key (EV1, AES)");
                if (MasterKey == null)
                    return true;
                rc = desfire.ChangeKeyAes(0, 0, MasterKey, null);
                if (rc == SCARD.S_SUCCESS)
                    return true;
                Logger.Warning("Unable to set the master key.");
                return false;
            }

            rc = desfire.Authenticate(0, BlankKey);
            if (rc == SCARD.S_SUCCESS)
            {
                Logger.Info("Authenticated with default key (EV0, DES/3DES)");
                if (MasterKey == null)
                    return true;
                rc = desfire.ChangeKeyAes(0, 0, MasterKey, null);
                if (rc == SCARD.S_SUCCESS)
                    return true;
                Logger.Warning("Unable to set the master key.");
                return true;
            }

            rc = desfire.AuthenticateISO(0, BlankKey);
            if (rc == SCARD.S_SUCCESS)
            {
                Logger.Info("Authenticated with default key (EV1, DES/3DES)");
                if (MasterKey == null)
                    return true;
                rc = desfire.ChangeKeyAes(0, 0, BlankKey, null);
                if (rc == SCARD.S_SUCCESS)
                {
                    rc = desfire.AuthenticateAes(0, BlankKey);
                    if (rc == SCARD.S_SUCCESS)
                    {
                        rc = desfire.ChangeKeyAes(0, 0, MasterKey, null);
                        if (rc == SCARD.S_SUCCESS)
                            return true;
                    }
                }
                Logger.Warning("Unable to set the master key.");
                return false;
            }

            Logger.Warning("Failed to get authenticated on the card with default key.");
            return false;
        }
    }
}
