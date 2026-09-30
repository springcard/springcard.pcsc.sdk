/**
 *
 * \ingroup GoogleVAS
 *
 */
using SpringCard.LibCs;
using SpringCard.NfcForum.Ndef;
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
        #region Session
        private NdefObject CreateSessionRecord(byte sequenceNumber, GoogleVasStatus status)
        {
            NdefObject result = new NdefObject(NdefObject.NdefTypeNameAndFormat.NFC_RTD_EXT, "ses");
            result.PAYLOAD = new byte[0];
            result.PAYLOAD = BinUtils.Concat(result.PAYLOAD, session.SessionId);
            result.PAYLOAD = BinUtils.Concat(result.PAYLOAD, sequenceNumber);
            result.PAYLOAD = BinUtils.Concat(result.PAYLOAD, (byte)status);
            return result;
        }
        #endregion

        #region NegociateSecureSession
        private NdefObject CreateCryptographyParametersRecord(ICardApduTransmitter cardChannel)
        {
            NdefObject result = new NdefObject(NdefObject.NdefTypeNameAndFormat.NFC_RTD_EXT, "cpr");

            result.PAYLOAD = new byte[0];
            result.PAYLOAD = BinUtils.Concat(result.PAYLOAD, session.TerminalNonce);
            byte authenticationByte = 0x00;
            if (!config.PresignedAuth)
                authenticationByte |= 0x01;
            Logger.Debug("\tAuthenticationByte:         {0:X02}", authenticationByte);
            result.PAYLOAD = BinUtils.Concat(result.PAYLOAD, authenticationByte);
            Logger.Debug("\tTerminalEphemeralPublicKey: {0}", BinConvert.ToHex(session.TerminalEphemeralPublicKey));
            result.PAYLOAD = BinUtils.Concat(result.PAYLOAD, session.TerminalEphemeralPublicKey);
            Logger.Debug("\tLongTermKeyVersion:         {0:X08}", BinConvert.ToHex(config.LongTermKeyVersion));
            result.PAYLOAD = BinUtils.Concat(result.PAYLOAD, BinUtils.FromDword(config.LongTermKeyVersion));

            ComputeSignature(cardChannel);

            NdefObject signatureIdNdef = new NdefObject(NdefObject.NdefTypeNameAndFormat.NFC_RTD_EXT, "sig");
            signatureIdNdef.PAYLOAD = BinUtils.Concat((byte)GoogleVasFormat.Binary, session.Signature);
            signatureIdNdef.MB = true;
            result.PAYLOAD = BinUtils.Concat(result.PAYLOAD, signatureIdNdef.Serialize());

            NdefObject collectorIdNdef = new NdefObject(NdefObject.NdefTypeNameAndFormat.NFC_RTD_EXT, "cld");
            collectorIdNdef.PAYLOAD = BinUtils.Concat((byte)GoogleVasFormat.Binary, config.CollectorId);
            collectorIdNdef.ME = true;
            result.PAYLOAD = BinUtils.Concat(result.PAYLOAD, collectorIdNdef.Serialize());

            return result;
        }
        #endregion

        #region GetData
        private NdefObject CreateMerchantRecord()
        {
            NdefObject result = new NdefObject(NdefObject.NdefTypeNameAndFormat.NFC_RTD_EXT, "mer");

            {
                NdefObject collectorIdRecord = new NdefObject(NdefObject.NdefTypeNameAndFormat.NFC_RTD_EXT, "cld");
                collectorIdRecord.PAYLOAD = BinUtils.Concat((byte)GoogleVasFormat.Binary, config.CollectorId);
                Logger.Debug("\tCollectorId: {0}", BinConvert.ToHex(collectorIdRecord.PAYLOAD));
                result.AddChild(collectorIdRecord);
            }

            if ((config.LocationId != null) && (config.LocationId.Length > 0))
            {
                NdefObject locationIdRecord = new NdefObject(NdefObject.NdefTypeNameAndFormat.NFC_RTD_EXT, "lid");
                locationIdRecord.PAYLOAD = BinUtils.Concat((byte)GoogleVasFormat.Binary, config.LocationId);
                Logger.Debug("\tLocationId: {0}", BinConvert.ToHex(locationIdRecord.PAYLOAD));
                result.AddChild(locationIdRecord);
            }

            if ((config.TerminalId != null) && (config.TerminalId.Length > 0))
            {
                NdefObject terminalIdRecord = new NdefObject(NdefObject.NdefTypeNameAndFormat.NFC_RTD_EXT, "tid");
                terminalIdRecord.PAYLOAD = BinUtils.Concat((byte)GoogleVasFormat.Binary, config.TerminalId);
                Logger.Debug("\tTerminalId: {0}", BinConvert.ToHex(terminalIdRecord.PAYLOAD));
                result.AddChild(terminalIdRecord);
            }

            if (!string.IsNullOrEmpty(config.MerchantName))
            {
                NdefObject merchantNameRecord = new RtdText(config.MerchantName, "en", "mnr");
                Logger.Debug("\tMerchantName: {0}", BinConvert.ToHex(merchantNameRecord.PAYLOAD));
                result.AddChild(merchantNameRecord);
            }

            if (config.MerchantCategoryCode > 0)
            {
                NdefObject merchantCategoryRecord = new NdefObject(NdefObject.NdefTypeNameAndFormat.NFC_RTD_EXT, "mcr");
                merchantCategoryRecord.PAYLOAD = BinUtils.Concat((byte)GoogleVasFormat.Binary, BinUtils.UInt16ToBytes(config.MerchantCategoryCode));
                Logger.Debug("\tMerchantCategory: {0}", BinConvert.ToHex(merchantCategoryRecord.PAYLOAD));
                result.AddChild(merchantCategoryRecord);
            }

            return result;
        }

        private NdefObject CreateServiceListRecord()
        {
            NdefObject result = new NdefObject(NdefObject.NdefTypeNameAndFormat.NFC_RTD_EXT, "slr");

            result.PAYLOAD = new byte[0];

            List<GoogleVasServiceType> serviceTypeToRequests = GoogleVasConfig.GetServiceListToRequest(config);

            for (int i=0; i< serviceTypeToRequests.Count; i++)
            {
                NdefObject serviceTypeRecord = new NdefObject(NdefObject.NdefTypeNameAndFormat.NFC_RTD_EXT, "str");

                /* In the middle by default */
                serviceTypeRecord.MB = false;
                serviceTypeRecord.ME = false;

                /* First */
                if (i==0)
                {
                    serviceTypeRecord.MB = true;
                }
                /* Last */
                if (i==serviceTypeToRequests.Count-1)
                {
                    serviceTypeRecord.ME = true;
                }
                
                serviceTypeRecord.PAYLOAD = new byte[1];
                serviceTypeRecord.PAYLOAD[0] = (byte)serviceTypeToRequests[i];
                result.PAYLOAD = BinUtils.Concat(result.PAYLOAD, serviceTypeRecord.Serialize());
            }

            Logger.Debug("\tServiceList: {0}", BinConvert.ToHex(result.PAYLOAD));

            return result;
        }

        private NdefObject CreatePosCapabilitiesRecord()
        {
            NdefObject result = new NdefObject(NdefObject.NdefTypeNameAndFormat.NFC_RTD_EXT, "pcr");

            result.PAYLOAD = GoogleVasConfig.GetCapabilitiesByteArray(config);

            Logger.Debug("\tPosCapabilities: {0}", BinConvert.ToHex(result.PAYLOAD));

            return result;
        }
#endregion
    }
}
