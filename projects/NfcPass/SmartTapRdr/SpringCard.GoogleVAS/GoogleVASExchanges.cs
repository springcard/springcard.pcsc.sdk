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
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpringCard.GoogleVas
{
    public partial class GoogleVasTerminal
    {
        #region Global transaction

        /**
         * Duraction of the Google Smart tap transaction in ms
         **/
        public double TransactionDuration = 0;

        /**
          * \brief Run the Google VAS transaction
          **/
        public bool DoTransaction(ICardApduTransmitter cardChannel, out GoogleVasData data, out GoogleVasError error)
        {
            return DoTransaction(cardChannel, out data, out error, out RAPDU dummySelectOseresponse, null);
        }

        /**
          * \brief Run the Google VAS transaction
          **/
        public bool DoTransaction(ICardApduTransmitter cardChannel, out GoogleVasData data, out GoogleVasError error, out RAPDU selectOseResponse)
        {
            return DoTransaction(cardChannel, out data, out error, out selectOseResponse, null);
        }

        /**
  * \brief Run the Google VAS transaction
  **/
        public bool DoTransaction(ICardApduTransmitter cardChannel, out GoogleVasData data, out GoogleVasError error, RAPDU selectOseResponseProvided)
        {
            return DoTransaction(cardChannel, out data, out error, out RAPDU dummySelectOseresponse, selectOseResponseProvided);
        }

        /**
		 * \brief Run the Google VAS transaction
         * 
         * \note This method also return the response of the SelectOSE (this can be useful tyo speed up intergartion with other protocols like Apple Pay, Samsung Pay...)
		 **/
        private bool DoTransaction(ICardApduTransmitter cardChannel, out GoogleVasData data, out GoogleVasError error, out RAPDU selectOseResponse, RAPDU selectOseResponseProvided)
        {
            data = null;
            bool mustSelectSmartTap = true;
            byte[] responseBuffer;
            selectOseResponse = null;
            Stopwatch sw = new Stopwatch();
            
            /* Start a new session */
            if (session == null)
                session = new GoogleVasSession(config);

            sw = new Stopwatch();
            sw.Start();


            if (config.UseOSE)
            {                
                if(selectOseResponseProvided != null)
                {
                    responseBuffer = selectOseResponseProvided.data.Bytes;
                }
                else
                {
                    if (!SelectOSE(cardChannel, out mustSelectSmartTap, out error, out responseBuffer, out selectOseResponse))
                    {
                        Logger.Warning("Select OSE Failed, continue anyway...");
                        goto SelectSmartTap2;
                    }
                }


                if (!ProcessSelectOSEResponse(responseBuffer, out mustSelectSmartTap, out error))
                {
                    Logger.Warning("Error while processing Select OSE response, continue anyway...");
                }
                /* If Select OSE succeded, and if for Google Smart Tap, then it will not be possible to use this response for other protocols */
                selectOseResponse = null;
            }
            else
            {
                Logger.Debug("Skipping SelectOSE...");
            }

SelectSmartTap2:
            if (!config.AllowSkippingSmartTap2Select || mustSelectSmartTap)
            {
                if (!SelectSmartTap2(cardChannel, out error, out responseBuffer))
                {
                    Logger.Warning("SelectSmartTap2 Failed");
                    return false;
                }

                if(!ProcessSelectSmartTap2Response(responseBuffer, out error))
                {
                    Logger.Warning("Error while processing SelectSmartTap2 response");
                    return false;
                }
            }
            else
            {
                Logger.Debug("Skipping SelectSmartTap2...");
            }

            if (config.UseEncryption)
            {
                if (!NegotiateSecureSession(cardChannel, true, out error, out responseBuffer))
                {
                    Logger.Warning("NegotiateSecureSession Failed");
                    return false;
                }

                if (!ProcessSecureSessionResponse(responseBuffer, out error))
                {
                    Logger.Warning("Error while processing NegotiateSecureSession response");
                    return false;
                }
            }
            else
            {
                Logger.Debug("Skipping NegotiateSecureSession...");
            }

            if (config.DoRequest)
            {
                bool rc = GetData(cardChannel, out data, out error, out responseBuffer);
                sw.Stop();
                if (!rc)
                {
                    Logger.Warning("GetData Failed");
                    return false;
                }

                if (config.UseEncryption)
                {
                    /* We could compute the session keys just after receiving the "Negotiate Secure Session"    */
                    /* response but it will slow down the transaction from the point of view of the smartphone. */
                    /* To avoid that we only do this computation after sending the GetVasData command           */
                    /* By doing this we save about 16ms of processing time (ECDH + HKDF)                       */
                    ComputeSessionKeys();
                }

                if (!ProcessGetDataResponse(responseBuffer, out data, out error))
                {
                    Logger.Warning("Error while processing GetData response");
                    return false;
                }
            }
            else
            {
                Logger.Debug("Skipping GetData...");
                error = GoogleVasError.Success;
                data = null;
            }

            if (config.DoPush)
            {
                Logger.Warning("Push not implemented!");
            }

            if (config.DoPayment)
            {
                Logger.Warning("Payment not implemented!");
            }

            TransactionDuration = sw.Elapsed.TotalMilliseconds;
           
            return true;
        }
        #endregion

        #region APDU commands


        /**
		 * \brief Send the SelectOSE APDU to the the card, and check that the status word is 9000 and the FCI is OK
		 **/
        private bool SelectOSE(ICardApduTransmitter cardChannel, out bool mustSelectSmartTap, out GoogleVasError error, out byte[] responseBuffer, out RAPDU selctOseResponse)
        {
            if (session == null)
                session = new GoogleVasSession(config);

            responseBuffer = new byte[0];
            mustSelectSmartTap = true;
            selctOseResponse = null;

            Logger.Debug("GoogleVAS:SelectOSE");

            CAPDU capdu = new CAPDU(0x00, 0xA4, 0x04, 0x00, "OSE.VAS.01", 0x00);

            Logger.Debug("<" + capdu.AsString());

            RAPDU rapdu = cardChannel.Transmit(capdu);
            selctOseResponse = rapdu;

            if (rapdu == null)
            {
                Logger.Trace("\tNo response");
                error = GoogleVasError.Communication;
                return false;
            }

            Logger.Debug(">" + rapdu.AsString());

            if (rapdu.Length < 2)
            {
                Logger.Debug("\tResponse is too short");
                error = GoogleVasError.Communication;
                return false;
            }

            if (rapdu.SW != 0x9000)
            {
                Logger.Trace("\tSW={0:X04}", rapdu.SW);
                error = TranslateSW(rapdu.SW);
                return false;
            }

            responseBuffer = rapdu.GetBytes();
            error = GoogleVasError.Success;
            return true;
        }

        /**
		 * \brief Send the SelectSmartTap2 APDU to the the card, and check that the status word is 9000
		 **/
        private bool SelectSmartTap2(ICardApduTransmitter cardChannel, out GoogleVasError error, out byte[] responseBuffer)
        {
            int retry = 2;
            responseBuffer = new byte[0];
            error = GoogleVasError.Success;

            Logger.Debug("GoogleVAS:SelectSmartTap2");

        send_command:
            CAPDU capdu = new CAPDU(0x00, 0xA4, 0x04, 0x00, BinConvert.HexToBytes("A000000476D0000111"), 0x00);

            Logger.Debug("<" + capdu.AsString());

            RAPDU rapdu = cardChannel.Transmit(capdu);

            if (rapdu == null)
            {
                Logger.Trace("\tNo response");
                error = GoogleVasError.Communication;
                return false;
            }

            Logger.Debug(">" + rapdu.AsString());

            if (rapdu.Length < 2)
            {
                Logger.Debug("\tResponse is too short");
                error = GoogleVasError.Communication;
                return false;
            }

            if (rapdu.SW != 0x9000)
            {
                /* All responses other than 9000, 94XX or 92XX cause the smart tap app to terminate in the terminal. */
                Logger.Trace("\tSW={0:X04}", rapdu.SW);
                error = TranslateSW(rapdu.SW);

                /* If the SW is 94XX or 92XX we can retry one time */
                /* cf https://developers.google.com/pay/smart-tap/reference/apdu-commands/status-words */
                if(rapdu.SW1 == 0x92 || rapdu.SW1 == 0x94)
                {
                    if(retry > 0)
                    {
                        retry--;
                        Logger.Debug("\tPossible transient failure.. retry");
                        goto send_command;
                    }
                    else
                    {
                        Logger.Trace("\tMax retry");
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

            responseBuffer = rapdu.GetBytes();
            return true;
        }

        /**
		 * \brief Send the NegotiateSecureSession APDU to the the card, and check that the status word is 9000
		 **/
        private bool NegotiateSecureSession(ICardApduTransmitter cardChannel, bool ContinueOnAuthError, out GoogleVasError error, out byte[] responseBuffer)
        {
            int retry = 2;

            error = GoogleVasError.Success;
            responseBuffer = new byte[0];

            Logger.Debug("GoogleVAS:NegotiateSecureSession");

        send_command:
            byte[] commandData = prepareNegotiateCommand(cardChannel);

            CAPDU capdu = new CAPDU(0x90, 0x53, 0x00, 0x00, commandData, 0x00);

            Logger.Debug("<" + capdu.AsString());

            RAPDU rapdu = cardChannel.Transmit(capdu);

            if (rapdu == null)
            {
                Logger.Trace("\tNo response");
                error = GoogleVasError.Communication;
                return false;
            }

            Logger.Debug(">" + rapdu.AsString());

            if (rapdu.Length < 2)
            {
                Logger.Debug("\tResponse is too short");
                error = GoogleVasError.Communication;
                return false;
            }

            if (rapdu.SW != 0x9000 && rapdu.SW != 0x9002)
            {
                /* All responses other than 9000 cause the smart tap app to terminate in the terminal. */
                Logger.Trace("\tSW={0:X04}", rapdu.SW);
                error = TranslateSW(rapdu.SW);

                if (rapdu.SW == 0x9500)
                {
                    Logger.Debug("The device has rejected our signature!");
                    session.AuthenticationOK = false;
                    if (!ContinueOnAuthError)
                        return false;
                }
                else if (rapdu.SW1 == 0x92 || rapdu.SW1 == 0x94)
                {
                    if (retry > 0)
                    {
                        retry--;
                        Logger.Debug("\tPossible transient failure.. retry");
                        goto send_command;
                    }
                    else
                    {
                        Logger.Trace("\tMax retry");
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                session.AuthenticationOK = true;
                Logger.Debug("The device has accepted our signature!");
            }

            responseBuffer = rapdu.GetBytes();
            return true;
        }

        /**
		 * \brief Send the GetData APDU to the the card, and check that the status word is 9000
		 **/
        private bool GetData(ICardApduTransmitter cardChannel, out GoogleVasData data, out GoogleVasError error, out byte[] responseBuffer)
        {
            data = null;
            responseBuffer = new byte[0];
            error = GoogleVasError.Success;

            Logger.Debug("GoogleVAS:GetData");

            byte[] commandData = prepareGetDataCommand();
            byte[] responseData = new byte[0];

            CAPDU capdu = new CAPDU(0x90, 0x50, 0x00, 0x00, commandData, 0x00);

            bool done = false;
            while (!done)
            {
                Logger.Debug("<" + capdu.AsString());

                RAPDU rapdu = cardChannel.Transmit(capdu);

                if (rapdu == null)
                {
                    Logger.Trace("\tNo response");
                    error = GoogleVasError.Communication;
                    return false;
                }

                Logger.Debug(">" + rapdu.AsString());

                if (rapdu.Length < 2)
                {
                    Logger.Debug("\tResponse is too short");
                    error = GoogleVasError.Communication;
                    return false;
                }

                /* Retrieve response data */
                responseData = BinUtils.Concat(responseData, rapdu.data.Bytes);

                /* Handle status word */
                switch (rapdu.SW)
                {
                    case 0x9000:
                        Logger.Debug("\tGetData OK");
                        done = true;
                        break;

                    case 0x9001:
                        Logger.Trace("\tSW={0:X04}", rapdu.SW);
                        Logger.Debug("\tGetData OK -- no pass");
                        /* Success, no passes, and payment not ready. */
                        error = GoogleVasError.NoVasMerchant;
                        done = true;
                        break;

                    case 0x9002:
                        Logger.Trace("\tSW={0:X04}", rapdu.SW);
                        Logger.Debug("\tGetData OK -- auth was not live");
                        done = true;
                        break;

                    case 0x9100:
                        /* There is additionnal data */
                        Logger.Debug("\tMore data to follow...");
                        capdu = new CAPDU(0x90, 0xC0, 0x00, 0x00, 0x00);
                        continue;

                    case 0x9101:
                        Logger.Debug("\tGetData OK -- payment not ready");
                        done = true;
                        break;

                    case 0x9102:
                        Logger.Trace("\tSW={0:X04}", rapdu.SW);
                        Logger.Debug("\tGetData OK -- no pass -- payment not ready");
                        /* Success, no passes, and payment not ready. */
                        error = GoogleVasError.NoVasMerchant;

                        /* If auth was sucessful  it is an error */
                        if (session.AuthenticationOK)
                        {
                            return false;
                        }
                        /* If auth already failed, it is not an error, we can outputs the "uniq tap id" */
                        else
                        {
                            responseBuffer = responseData;
                            return true;
                        }

                    default:
                        if (rapdu.SW1 == 0x92 || rapdu.SW1 == 0x94)
                        {
                            /* Possible transient failure */
                            Logger.Debug("\t Possible transient failure.. retry");
                            /* rebuild CAPDU with session.SequenceNumber incremeted*/
                            commandData = prepareGetDataCommand();
                            capdu = new CAPDU(0x90, 0x50, 0x00, 0x00, commandData, 0x00);
                            /* Reset buffer */
                            responseData = new byte[0];
                            continue;
                        }
                        /* All responses other than 9000 cause the smart tap app to terminate in the terminal. */
                        Logger.Trace("\tSW={0:X04}", rapdu.SW);
                        error = GoogleVasError.WrongResponseStatusWord;
                        return false;
                }
            }

            responseBuffer = responseData;
            return true;
        }

        /**
		 * \brief Send the PushData APDU to the the card, and check that the status word is 9000
		 **/
        private bool PushData(ICardApduTransmitter cardChannel, GoogleVasConfig config, out GoogleVasError error)
        {
            error = GoogleVasError.Success;
            return false;
        }

#endregion

#region Utilities

        private byte[] prepareGetDataCommand()
        {
            /*
            * Prepare the command
            * -------------------
            */

            /* Service request NDEF header */
            NdefObject serviceRequestRecord = new NdefObject(NdefObject.NdefTypeNameAndFormat.NFC_RTD_EXT, "srq");
            serviceRequestRecord.MB = true;
            serviceRequestRecord.ME = true;
            serviceRequestRecord.PAYLOAD = new byte[0];
            /* Service request NDEF version */
            serviceRequestRecord.PAYLOAD = BinUtils.Concat(serviceRequestRecord.PAYLOAD, BinUtils.FromWord(config.SmartTapVersion));
            /* Session NDEF record */
            NdefObject sessionCommandRecord = CreateSessionRecord(++session.SequenceNumber, GoogleVasStatus.OK);
            /* Sequence number is incremented at each command AND at each response */
            session.SequenceNumber++;
            sessionCommandRecord.MB = true;
            serviceRequestRecord.PAYLOAD = BinUtils.Concat(serviceRequestRecord.PAYLOAD, sessionCommandRecord.Serialize());
            /* Merchant NDEF record */
            NdefObject merchantRecord = CreateMerchantRecord();
            serviceRequestRecord.PAYLOAD = BinUtils.Concat(serviceRequestRecord.PAYLOAD, merchantRecord.Serialize());
            /* Service list NDEF record */
            NdefObject serviceListRecord = CreateServiceListRecord();
            serviceRequestRecord.PAYLOAD = BinUtils.Concat(serviceRequestRecord.PAYLOAD, serviceListRecord.Serialize());
            /* POS capabilities record */
            NdefObject posCapabilitiesRecord = CreatePosCapabilitiesRecord();
            posCapabilitiesRecord.ME = true;
            serviceRequestRecord.PAYLOAD = BinUtils.Concat(serviceRequestRecord.PAYLOAD, posCapabilitiesRecord.Serialize());

            return serviceRequestRecord.Serialize();
        }

        private byte[] prepareNegotiateCommand(ICardApduTransmitter cardChannel)
        {
            /* Negotiate NDEF header */
            NdefObject negotiateCommandRecord = new NdefObject(NdefObject.NdefTypeNameAndFormat.NFC_RTD_EXT, "ngr");
            negotiateCommandRecord.MB = true;
            negotiateCommandRecord.ME = true;
            negotiateCommandRecord.PAYLOAD = new byte[0];
            /* Negotiate NDEF version */
            Logger.Debug("\tSmartTapVersion:            {0:X04}", config.SmartTapVersion);
            negotiateCommandRecord.PAYLOAD = BinUtils.Concat(negotiateCommandRecord.PAYLOAD, BinUtils.FromWord(config.SmartTapVersion));
            /* Session NDEF record */
            NdefObject sessionCommandRecord = CreateSessionRecord(++session.SequenceNumber, GoogleVasStatus.OK);
            /* Sequence number is incremented at each command AND at each response */
            session.SequenceNumber++;
            sessionCommandRecord.MB = true;
            negotiateCommandRecord.PAYLOAD = BinUtils.Concat(negotiateCommandRecord.PAYLOAD, sessionCommandRecord.Serialize());
            /* Cryptography params NDEF record */
            NdefObject cryptographyParametersRecord = CreateCryptographyParametersRecord(cardChannel);
            cryptographyParametersRecord.ME = true;
            negotiateCommandRecord.PAYLOAD = BinUtils.Concat(negotiateCommandRecord.PAYLOAD, cryptographyParametersRecord.Serialize());

            return negotiateCommandRecord.Serialize();
        }

        private static GoogleVasError TranslateSW(ushort sw)
        {
            switch (sw)
            {
                case 0x9000:
                    Logger.Trace("9000 - Success");
                    return GoogleVasError.Success;

                case 0x9001:
                    Logger.Trace("9001 - Success, no passes");
                    return GoogleVasError.Success;

                case 0x9002:
                    Logger.Trace("9002 - Success, pre-signed authentication");
                    return GoogleVasError.Success;

                case 0x9100:
                    Logger.Trace("9100 - More data available");
                    return GoogleVasError.WrongResponseStatusWord;

                case 0x9101:
                    Logger.Trace("9101 - Success with passes, payment not ready");
                    return GoogleVasError.Success;

                case 0x9102:
                    Logger.Trace("9101 - Success, no passes, payment not ready");
                    return GoogleVasError.Success;

                case 0x9200:
                    Logger.Trace("9200 - Unknown transient failure");
                    return GoogleVasError.WrongResponseStatusWord;

                case 0x9201:
                    Logger.Trace("9201 - Crypto exception");
                    return GoogleVasError.WrongResponseStatusWord;

                case 0x9202:
                    Logger.Trace("9202 - Timeout");
                    return GoogleVasError.WrongResponseStatusWord;

                case 0x9203:
                    Logger.Trace("9203 - Execution error");
                    return GoogleVasError.WrongResponseStatusWord;

                case 0x9300:
                    Logger.Trace("9300 - Mobile device locked");
                    return GoogleVasError.WaitingForActivation;

                case 0x9301:
                    Logger.Trace("9301 - No payment card");
                    return GoogleVasError.NoPaymentCard;

                case 0x9302:
                    Logger.Trace("9302 - Customer being asked to select a card");
                    return GoogleVasError.WaitingForIntervention;

                case 0x9400:
                    Logger.Trace("9400 - Unknown command");
                    return GoogleVasError.WrongCommand;

                case 0x9401:
                    Logger.Trace("9401 - Unknown NDEF record");
                    return GoogleVasError.WrongCommandData;

                case 0x9402:
                    Logger.Trace("9402 - Parsing failure");
                    return GoogleVasError.WrongCommandData;

                case 0x9403:
                    Logger.Trace("9403 - Invalid crypto parameters");
                    return GoogleVasError.WrongCommandData;

                case 0x9404:
                    Logger.Trace("9404 - Request more data not applicable");
                    return GoogleVasError.WrongCommandData;

                case 0x9405:
                    Logger.Trace("9405 - More data not available");
                    return GoogleVasError.WrongCommand;

                case 0x9406:
                    Logger.Trace("9406 - Too many requests");
                    return GoogleVasError.TooManyRequests;

                case 0x9407:
                    Logger.Trace("9407 - No merchant ID (collector ID) set");
                    return GoogleVasError.NoVasMerchant;

                case 0x9408:
                    Logger.Trace("9408 - Invalid pushback URI");
                    return GoogleVasError.InvalidUri;

                case 0x9500:
                    Logger.Trace("9500 - Unable to authenticate");
                    return GoogleVasError.AuthenticationFailed;

                case 0x9501:
                    Logger.Trace("9501 - Not authenticated");
                    return GoogleVasError.AuthenticationRequired;

                case 0x9502:
                    Logger.Trace("9502 - Versions not supported");
                    return GoogleVasError.VersionNotSupported;

                case 0x6A82:
                    Logger.Trace("6A82 - Not found");
                    return GoogleVasError.NoVasApplication;

                default:
                    Logger.Trace("{0:X02} - Unexpected SW", sw);
                    return GoogleVasError.WrongResponseStatusWord;
            }
        }

#endregion
    }
}
