/**
 *
 * \ingroup GoogleVAS
 *
 */
using SpringCard.LibCs;
using SpringCard.NfcForum.Ndef;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpringCard.GoogleVas
{
    public partial class GoogleVasTerminal
    {
        private static void DebugBuffer(byte[] t)
        {
            for (int i = 0; i < t.Length; i += 16)
            {
                string s = "";
                if (i + 16 > t.Length)
                {
                    s += BinConvert.ToHex(t, i, t.Length - i);
                    while (s.Length < 32)
                        s += "  ";
                    s += "\t";
                    s += StrUtils.ToStr(t, i, t.Length - i);
                }
                else
                {
                    s += BinConvert.ToHex(t, i, 16);
                    s += "\t";
                    s += StrUtils.ToStr(t, i, 16);
                }

                Logger.Debug("\t{0}", s);
            }
        }

        #region OSE
        private bool ParseDiscretionaryTemplate(byte[] Data, out GoogleVasError error)
        {
            byte[] buffer = Data;
            while ((buffer != null) && (buffer.Length > 0))
            {
                Asn1Tlv tlv = Asn1Tlv.Unserialize(buffer, out buffer);
                if (tlv != null)
                {
                    switch (tlv.T_AsWord)
                    {
                        case 0xDF6D:
                            Logger.Debug("\t\t\t\tMinVersion={0}", BinConvert.ToHex(tlv.V));
                            session.SmartTapMinVersionSupported = BinUtils.BytesToUInt16(tlv.V);
                            break;

                        case 0xDF4D:
                            Logger.Debug("\t\t\t\tMaxVersion={0}", BinConvert.ToHex(tlv.V));                              
                            session.SmartTapMaxVersionSupported = BinUtils.BytesToUInt16(tlv.V);
                            break;

                        case 0xDF6E:
                            Logger.Debug("\t\t\t\tNonce={0}", BinConvert.ToHex(tlv.V));
                            session.SmartTap2FciNonce = tlv.V;
                            break;

                        case 0xDF62:
                            Logger.Debug("\t\t\t\tCapabilitiesBitmap={0}", BinConvert.ToHex(tlv.V));
                            if (tlv.L_Length > 0)
                            {
                                byte capabilities = tlv.V[0];
                                session.SmartTapIsImplicit = ((capabilities & 0x01) != 0);
                                session.SmartTapSupported = ((capabilities & 0x02) != 0);
                            }
                            break;

                        default:
                            Logger.Debug("\t\t\t\tGot unknown TLV {0}", BinConvert.ToHex(tlv.T));
                            break;
                    }
                }
            }

            error = GoogleVasError.Success;
            return true;
        }

        private bool ParseDirectoryEntry(byte[] Data, out GoogleVasError error)
        {
            byte[] buffer = Data;
            while ((buffer != null) && (buffer.Length > 0))
            {
                Asn1Tlv tlv = Asn1Tlv.Unserialize(buffer, out buffer);
                if (tlv != null)
                {
                    switch (tlv.T_AsWord)
                    {
                        case 0x4F:
                            Logger.Debug("\t\t\tADF name={0}", BinConvert.ToHex(tlv.V));
                            break;

                        case 0x50:
                            Logger.Debug("\t\t\tLabel={0}", BinConvert.ToHex(tlv.V));
                            break;

                        case 0x87:
                            Logger.Debug("\t\t\tPriority={0}", BinConvert.ToHex(tlv.V));
                            break;

                        case 0x73:
                            Logger.Debug("\t\t\tDiscretionary template={0}", BinConvert.ToHex(tlv.V));
                            if (ParseDiscretionaryTemplate(tlv.V, out error))
                            {
                                session.PhoneHasDiscretionaryTemplate = true;
                            }
                            else
                            {
                                return false;
                            }
                            break;

                        default:
                            Logger.Debug("\t\t\tGot unknown TLV {0}", BinConvert.ToHex(tlv.T));
                            break;
                    }
                }
            }

            error = GoogleVasError.Success;
            return true;
        }

        private bool ParseFCIProprietaryTemplate(byte[] Data, out GoogleVasError error)
        {
            Asn1Tlv tlv = Asn1Tlv.Unserialize(Data);
            if (tlv == null)
            {
                Logger.Debug("\tThe FCI proprietary template is invalid");
                error = GoogleVasError.WrongResponseData;
                return false;
            }

            if (tlv.T_AsWord != 0xBF0C)
            {
                Logger.Debug("\tThe FCI proprietary template does not start with a PPSE data block");
                error = GoogleVasError.WrongResponseData;
                return false;
            }

            Logger.Debug("\t\tFCI PPSE data={0}", BinConvert.ToHex(tlv.V));

            byte[] buffer = tlv.V;
            while ((buffer != null) && (buffer.Length > 0))
            {
                Logger.Debug("\t\tParsing {0}", BinConvert.ToHex(buffer));

                tlv = Asn1Tlv.Unserialize(buffer, out buffer);
                if (tlv != null)
                {
                    switch (tlv.T_AsWord)
                    {
                        case 0x61:
                            Logger.Debug("\t\tDirectory entry={0}", BinConvert.ToHex(tlv.V));
                            if (!ParseDirectoryEntry(tlv.V, out error))
                                return false;
                            break;

                        default:
                            Logger.Debug("\t\tGot unknown TLV {0}", BinConvert.ToHex(tlv.T));
                            break;
                    }
                }
            }

            error = GoogleVasError.Success;
            return true;
        }

        /**
         * \brief Process the response to the SelectOSE command
         */
        public bool ProcessSelectOSEResponse(byte[] responseData, out bool mustSelectSmartTap, out GoogleVasError error)
        {
            if (session == null)
                session = new GoogleVasSession(config);

            mustSelectSmartTap = false;

            Asn1Tlv tlv = Asn1Tlv.Unserialize(responseData);
            if (tlv == null)
            {
                Logger.Debug("\tResponse is not a valid FCI template");
                error = GoogleVasError.WrongResponseData;
                return false;
            }

            if (tlv.T_AsWord != 0x6F)
            {
                Logger.Debug("\tThe response does not start with a FCI template");
                error = GoogleVasError.WrongResponseData;
                return false;
            }

            byte[] buffer = tlv.V;
            while ((buffer != null) && (buffer.Length > 0))
            {
                tlv = Asn1Tlv.Unserialize(buffer, out buffer);
                if (tlv != null)
                {
                    switch (tlv.T_AsWord)
                    {
                        case 0x50:
                            Logger.Debug("\tApplicationLabel={0}", BinConvert.ToHex(tlv.V));
                            break;

                        case 0xC0:
                            Logger.Debug("\tVersion={0}", BinConvert.ToHex(tlv.V));
                            break;

                        case 0xC1:
                            Logger.Debug("\tTransactionalDetails={0}", BinConvert.ToHex(tlv.V));
                            break;

                        case 0xC2:
                            Logger.Debug("\tCryptographicMasterNonce={0}", BinConvert.ToHex(tlv.V));
                            session.CryptographicMasterNonce = tlv.V;
                            break;

                        case 0xC3:
                            Logger.Debug("\tMobileEphemeralKey={0}", BinConvert.ToHex(tlv.V));
                            break;

                        case 0xA5:
                            Logger.Debug("\tFCI proprietary template={0}", BinConvert.ToHex(tlv.V));
                            if (!ParseFCIProprietaryTemplate(tlv.V, out error))
                                return false;
                            break;

                        default:
                            Logger.Debug("\tGot unknown TLV {0}", BinConvert.ToHex(tlv.T));
                            break;
                    }
                }
            }

            if (!session.PhoneHasDiscretionaryTemplate)
            {
                Logger.Warning("No Discretionary template (used for Smart Tap 2) found in the Select OSE response");
                error = GoogleVasError.DiscretionaryTemplateMissing;
                return false;
            }

            if (session.SmartTapMinVersionSupported > config.SmartTapVersion || config.SmartTapVersion > session.SmartTapMaxVersionSupported)
            {
                Logger.Warning("The mobile does not support the version of the VAS protocol in the terminal");
                error = GoogleVasError.VersionNotSupported;
                return false;
            }

            if (!session.SmartTapSupported)
            {
                error = GoogleVasError.NoVAS;
                return false;
            }

            mustSelectSmartTap = !session.SmartTapIsImplicit;
            error = GoogleVasError.Success;
            return true;
        }
        #endregion

        #region SelectSmartTap2

        /**
         * \brief Process the response to the Select Smart Tap 2 command
         */
        public bool ProcessSelectSmartTap2Response(byte[] responseData, out GoogleVasError error)
        {
            if (session == null)
                session = new GoogleVasSession(config);

            if (responseData.Length < 8)
            {
                Logger.Debug("\tResponse is too short ({0})", responseData.Length);
                error = GoogleVasError.DataMissing;
                return false;
            }

            Logger.Debug("\tMinVersion={0}", BinConvert.ToHex(responseData, 0, 2));
            Logger.Debug("\tMaxVersion={0}", BinConvert.ToHex(responseData, 2, 2));

            NdefObject mobileNonceNdef;

            try
            {
                mobileNonceNdef = NdefObject.DeserializeOne(BinUtils.Copy(responseData, 4, 0));
            }
            catch
            {
                mobileNonceNdef = null;
            }

            if (mobileNonceNdef == null)
            {
                Logger.Debug("\tFailed to decode the mobile device nonce NDEF");
                error = GoogleVasError.DataMissing;
                return false;
            }

            if (mobileNonceNdef.TNF != NdefObject.NdefTypeNameAndFormat.NFC_RTD_EXT)
            {
                Logger.Debug("\tExpected mobile device nonce NDEF with TNF={0}, got TNF={1}", NdefObject.NdefTypeNameAndFormat.NFC_RTD_EXT, mobileNonceNdef.TNF);
                error = GoogleVasError.DataMissing;
                return false;
            }

            if (!mobileNonceNdef.ID.SequenceEqual(StrUtils.ToBytes_ASCII("mdn")))
            {
                Logger.Debug("\tExpected mobile device nonce NDEF (mdn), found type '{0}'", mobileNonceNdef.TYPE);
                error = GoogleVasError.DataMissing;
                return false;
            }

            if (mobileNonceNdef.PAYLOAD[0] == (byte) GoogleVasFormat.Binary)
            {
                session.SmartTap2MobileDeviceNonce = BinUtils.Copy(mobileNonceNdef.PAYLOAD, 1, mobileNonceNdef.PAYLOAD.Length - 1);
                Logger.Debug("\tMobileNonce={0}", BinConvert.ToHex(session.SmartTap2MobileDeviceNonce));
            }
            else
            {
                Logger.Debug("Wrong format for mobile device nonce NDEF (mdn) record");
                error = GoogleVasError.WrongResponseData;
                return false;
            }

            error = GoogleVasError.Success;
            return true;
        }
        #endregion

        #region SecureSession

        /**
         * \brief Process the response to the Secure Session command
         */
        public bool ProcessSecureSessionResponse(byte[] responseData, out GoogleVasError error)
        {
            NdefObject negotiateResponseRecord = NdefObject.DeserializeOne(responseData, true);

            if (negotiateResponseRecord == null)
            {
                Logger.Debug("The response is not a valid NDEF object");
                error = GoogleVasError.WrongResponseData;
                return false;
            }

            if (!negotiateResponseRecord.Is(NdefObject.NdefTypeNameAndFormat.NFC_RTD_EXT, "nrs"))
            {
                Logger.Debug("The response is not a NDEF \"nrs\" record");
                error = GoogleVasError.WrongResponseData;
                return false;
            }

            NdefObject sessionResponseRecord = negotiateResponseRecord.GetChild(0);
            NdefObject mobileEphemeralPublicKeyRecord = negotiateResponseRecord.GetChild(1);

            if (sessionResponseRecord == null)
            {
                Logger.Debug("The response NDEF record is empty");
                error = GoogleVasError.WrongResponseData;
                return false;
            }

            if (!sessionResponseRecord.Is(NdefObject.NdefTypeNameAndFormat.NFC_RTD_EXT, "ses"))
            {
                Logger.Debug("The first record in the response is not a NDEF \"ses\" record");
                error = GoogleVasError.WrongResponseData;
                return false;
            }

            if ((sessionResponseRecord.PAYLOAD == null) || (sessionResponseRecord.PAYLOAD.Length < 10))
            {
                Logger.Debug("The NDEF \"ses\" record is invalid");
                error = GoogleVasError.WrongResponseData;
                return false;
            }

            if (!BinUtils.Equals(sessionResponseRecord.PAYLOAD, 0, session.SessionId, 0, 8))
            {
                Logger.Debug("The response's session ID is invalid");
                error = GoogleVasError.WrongResponseData;
                return false;
            }

            Logger.Debug("Sequence number: {0}", sessionResponseRecord.PAYLOAD[8]);
            Logger.Debug("Status: {0}", sessionResponseRecord.PAYLOAD[9]);

            if (mobileEphemeralPublicKeyRecord != null)
            {
                if (!mobileEphemeralPublicKeyRecord.Is(NdefObject.NdefTypeNameAndFormat.NFC_RTD_EXT, "dpk"))
                {
                    Logger.Debug("The second record in the response is not a NDEF \"dpk\" record");
                    error = GoogleVasError.WrongResponseData;
                    return false;
                }

                if ((mobileEphemeralPublicKeyRecord.PAYLOAD == null) || (mobileEphemeralPublicKeyRecord.PAYLOAD.Length != 33))
                {
                    Logger.Debug("The NDEF \"dpk\" record is invalid");
                    error = GoogleVasError.WrongResponseData;
                    return false;
                }

                session.MobileDeviceEphemeralPublicKey = mobileEphemeralPublicKeyRecord.PAYLOAD;
                Logger.Debug("MobileDeviceEphemeralPublicKey: {0}", BinConvert.ToHex(session.MobileDeviceEphemeralPublicKey));

            }

            error = GoogleVasError.Success;
            return true;
        }
        #endregion

        #region GetData

        private bool ProcessGetDataRecordBundle(byte[] recordPayload, GoogleVasData data, out GoogleVasError error)
        {
            NdefObject[] serviceRecords = NdefObject.Deserialize(recordPayload, true);

            foreach (NdefObject serviceRecord in serviceRecords)
            {
                if (serviceRecord.Is(NdefObject.NdefTypeNameAndFormat.NFC_RTD_EXT, "asv"))
                {
                    Logger.Debug("\tProcessing asv");
                    Logger.Debug("\t\t{0}", BinConvert.ToHex(serviceRecord.PAYLOAD));
                    DebugBuffer(serviceRecord.PAYLOAD);

                    data.BeginRecord();
                    for (int i = 0; i < serviceRecord.GetChildCount(); i++)
                    {
                        NdefObject child = serviceRecord.GetChild(i);
                        data.ProcessServiceDataRecord(child);
                    }
                    data.EndRecord();
                }
                else
                {
                    Logger.Debug("\tFound unexpected record {0}:\"{1}\"", serviceRecord.TNF.ToString(), serviceRecord.TYPE);
                    Logger.Debug("\t\t{0}", BinConvert.ToHex(serviceRecord.PAYLOAD));
                }
            }

            error = GoogleVasError.Success;
            return true;
        }

        /**
         * \brief Process the response to the Get Data command
         */
        public bool ProcessGetDataResponse(byte[] responseData, out GoogleVasData data, out GoogleVasError error)
        {
            data = null;

            NdefObject serviceResponseRecord;
            try
            {
                serviceResponseRecord = NdefObject.DeserializeOne(responseData, true);
            }
            catch (Exception e)
            {
                Logger.Debug("Exception: " + e.Message);
                serviceResponseRecord = null;
            }

            if (serviceResponseRecord == null)
            {
                Logger.Debug("The response is not a valid NDEF object");
                error = GoogleVasError.WrongResponseData;
                return false;
            }

            if (!serviceResponseRecord.Is(NdefObject.NdefTypeNameAndFormat.NFC_RTD_EXT, "srs"))
            {
                Logger.Debug("The response is not a NDEF \"srs\" record");
                error = GoogleVasError.WrongResponseData;
                return false;
            }

            NdefObject sessionResponseRecord = serviceResponseRecord.GetChild(0);

            if (sessionResponseRecord == null)
            {
                Logger.Debug("The response NDEF record is empty");
                error = GoogleVasError.WrongResponseData;
                return false;
            }

            if (!sessionResponseRecord.Is(NdefObject.NdefTypeNameAndFormat.NFC_RTD_EXT, "ses"))
            {
                Logger.Debug("The first record in the response is not a NDEF \"ses\" record");
                error = GoogleVasError.WrongResponseData;
                return false;
            }

            if ((sessionResponseRecord.PAYLOAD == null) || (sessionResponseRecord.PAYLOAD.Length < 10))
            {
                Logger.Debug("The NDEF \"ses\" record is invalid");
                error = GoogleVasError.WrongResponseData;
                return false;
            }

            if (!BinUtils.Equals(sessionResponseRecord.PAYLOAD, 0, session.SessionId, 0, 8))
            {
                Logger.Debug("The response's session ID is invalid");
                error = GoogleVasError.WrongResponseData;
                return false;
            }

            Logger.Debug("Sequence number: {0}", sessionResponseRecord.PAYLOAD[8]);
            Logger.Debug("Status: {0}", sessionResponseRecord.PAYLOAD[9]);
            Logger.Debug("{0} record(s) following", serviceResponseRecord.GetChildCount() - 1);

            data = new GoogleVasData();

            for (int i = 1; i < serviceResponseRecord.GetChildCount(); i++)
            {
                NdefObject child = serviceResponseRecord.GetChild(i);

                if (child.Is(NdefObject.NdefTypeNameAndFormat.NFC_RTD_EXT, "reb"))
                {
                    byte[] recordBundlePayload = child.PAYLOAD;
                    Logger.Debug("\t{0}", BinConvert.ToHex(recordBundlePayload));

                    if ((recordBundlePayload[0] & 0x02) != 0)
                    {
                        Logger.Debug("Payload is compressed - not supported");
                        continue;
                    }

                    if ((recordBundlePayload[0] & 0x01) != 0)
                    {
                        Logger.Debug("Payload is encrypted");
                        
#if false // we can return the uniq tap id even if there are no passes associated to the merchant id
  if (!session.AuthenticationOK || !session.SessionKeyOK)
                        {
                            Logger.Debug("Can't decrypt the payload - No shared security context!");
                            error = GoogleVasError.MessageDecipherFailed;
                            return false;
                        }
#endif

                        recordBundlePayload = DecryptPayload(BinUtils.Copy(recordBundlePayload, 1));
                        if (recordBundlePayload == null)
                        {
                            Logger.Debug("Failed to decrypt the payload");
                            error = GoogleVasError.MessageDecipherFailed;
                            return false;
                        }

                        DebugBuffer(recordBundlePayload);
                    }
                    else
                    {
                        /* Drop the header */
                        recordBundlePayload = BinUtils.Copy(recordBundlePayload, 1);
                    }

                    /* Process the payload */
                    if (!ProcessGetDataRecordBundle(recordBundlePayload, data, out error))
                    {
                        return false;
                    }
                }
                else
                {
                    Logger.Debug("Found unexpected record {0}:\"{1}\"", child.TNF.ToString(), child.TYPE);
                    Logger.Debug("\t{0}", BinConvert.ToHex(child.PAYLOAD));
                }
            }

            Logger.Debug("\n" + data.GetJsonString());

            error = GoogleVasError.Success;
            return true;
        }
#endregion
    }
}
