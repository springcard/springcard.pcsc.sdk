/*
 * Crée par SharpDevelop.
 * Utilisateur: johann
 * Date: 05/01/2012
 * Heure: 12:03
 * 
 * Pour changer ce modèle utiliser Outils | Options | Codage | Editer les en-têtes standards.
 */
using SpringCard.LibCs;
using SpringCard.PCSC.ReaderHelper;
using System;
using System.Reflection;
using System.Threading;

namespace SpringCard.PCSC.CardEmulation
{
    public static class Library
    {
        /**
         * \brief Read Library information
         **/
        public static readonly AppModuleInfo ModuleInfo = new AppModuleInfo(Assembly.GetExecutingAssembly());
    }

    public abstract class CardEmulBase
    {
        private string readerName;
        private SCardChannel cardChannel;
        volatile bool emulationRunning = false;
        private Thread emulationThread = null;

        public ushort EventTimeoutMs = 450;

        public CardEmulBase(string ReaderName)
        {
            this.readerName = ReaderName;
        }

        ~CardEmulBase()
        {
            StopEmulation();
            SetReaderInPollerMode();
        }

        public bool SetReaderInListenerMode()
        {
            Logger.Debug("SetReaderInListenerMode...");

            SCardChannel controlChannel = new SCardChannel(readerName);

            Logger.Debug("Connecting to the reader...");
            if (!controlChannel.ConnectDirect())
            {
                Logger.Warning("Failed to open a direct connection to reader {0} ({1}: {2})", readerName, controlChannel.LastError, controlChannel.LastErrorAsString);
                return false;
            }

            Logger.Debug("Send NFC_LISTENER control command...");
            /* https://docs.springcard.com/books/SpringCore/Host_interfaces/Logical/Direct_Protocol/CONTROL_class/NFC_modes/NFC_LISTENER */
            /* No cardlet - HCE operation / virtual card in the PC/SC slot */
            byte[] controlCommand = new byte[] {
                ReaderCommands.ControlCode,
                (byte)ReaderCommands.Opcode.NfcListener,
                0x11
            };

            Logger.Debug("<{0}", BinConvert.ToHex(controlCommand));

            byte[] controlResponse = controlChannel.Control(controlCommand);

            if ((controlResponse == null) || (controlResponse.Length == 0))
            {
                Logger.Warning("Failed to send the NFC_LISTENER command to the reader ({0}: {1})", controlChannel.LastError, controlChannel.LastErrorAsString);
                controlChannel.DisconnectLeave();
                return false;
            }

            Logger.Debug(">{0}", BinConvert.ToHex(controlResponse));

            if (controlResponse[0] != 0x00)
            {
                Logger.Warning("The NFC_LISTENER command has failed with error {0:X02}", controlResponse[0]);
                controlChannel.DisconnectLeave();
                return false;
            }

            /* Close the channel - the card shall now arrive in the slot */
            controlChannel.DisconnectLeave();
            return true;
        }

        public bool SetReaderInPollerMode()
        {
            Logger.Debug("SetReaderInPollerMode...");

            SCardChannel controlChannel;

            if ((cardChannel == null) || (!cardChannel.Connected))
            {
                Logger.Debug("Connecting to the reader...");
                controlChannel = new SCardChannel(readerName);

                if (!controlChannel.ConnectDirect())
                {
                    Logger.Warning("Failed to open a direct connection to reader {0} ({1}: {2})", readerName, controlChannel.LastError, controlChannel.LastErrorAsString);
                    return false;
                }
            }
            else
            {
                Logger.Debug("Already connected to the reader...");
                controlChannel = cardChannel;
                cardChannel = null;
            }

            Logger.Debug("Send NFC_POLLER control command...");
            /* https://docs.springcard.com/books/SpringCore/Host_interfaces/Logical/Direct_Protocol/CONTROL_class/NFC_modes/NFC_POLLER */
            /* No cardlet - HCE operation / virtual card in the PC/SC slot */
            byte[] controlCommand = new byte[] {
                ReaderCommands.ControlCode,
                (byte)ReaderCommands.Opcode.NfcPoller
            };

            Logger.Debug("<{0}", BinConvert.ToHex(controlCommand));

            byte[] controlResponse = controlChannel.Control(controlCommand);

            if ((controlResponse == null) || (controlResponse.Length == 0))
            {
                Logger.Warning("Failed to send the NFC_POLLER command to the reader ({0}: {1})", controlChannel.LastError, controlChannel.LastErrorAsString);
                controlChannel.DisconnectLeave();
                return false;
            }

            Logger.Debug(">{0}", BinConvert.ToHex(controlResponse));

            if (controlResponse[0] != 0x00)
            {
                Logger.Warning("The NFC_POLLER command has failed with error {0:X02}", controlResponse[0]);
                controlChannel.DisconnectLeave();
                return false;
            }

            /* Close the channel - the card shall now arrive in the slot */
            controlChannel.DisconnectLeave();
            Logger.Debug("Done");
            return true;
        }

        public bool Running()
        {
            return emulationRunning;
        }

        public void StopEmulation()
        {
            Logger.Debug("StopEmulation...");

            emulationRunning = false;

            if (emulationThread != null)
            {
                Logger.Debug("Waiting for emulation thread...");
                Thread.Sleep(600);
                emulationThread.Abort();
                emulationThread.Join();
                emulationThread = null;
            }

            if (cardChannel != null)
            {
                /* HCE CONTROL STOP */
                Logger.Debug("Sending HCE CONTROL STOP instruction...");
                CAPDU capdu = new CAPDU(0xFF, 0xF7, 0x02, 0x00);
                Logger.Debug("<{0}", capdu.AsString());
                RAPDU rapdu = cardChannel.Transmit(capdu);

                if (rapdu == null)
                {
                    Logger.Warning("Failed to send the HCE CONTROL STOP instruction to the reader ({0}: {1})", cardChannel.LastError, cardChannel.LastErrorAsString);
                }
                else
                {
                    Logger.Debug(">{0}", rapdu.AsString());
                    if (rapdu.SW != 0x9000)
                        Logger.Warning("The HCE CONTROL STOP instruction has failed with error {0:X04}", rapdu.SW);
                }

                Logger.Debug("Closing the (virtual) card channel...");
                cardChannel.DisconnectLeave();
                cardChannel = null;
            }

            Logger.Debug("Done");
        }

        public bool StartEmulation()
        {
            if (cardChannel == null)
            {
                cardChannel = new SCardChannel(readerName);
                if (!cardChannel.ConnectExclusive())
                {
                    Logger.Warning("Failed to open a card connection to reader {0} ({1}: {2})", readerName, cardChannel.LastError, cardChannel.LastErrorAsString);
                    return false;
                }
            }

            if (emulationRunning)
            {
                StopEmulation();
            }

            /* HCE CONTROL START */
            CAPDU capdu = new CAPDU(0xFF, 0xF7, 0x02, 0x01);
            Logger.Debug("<{0}", capdu.AsString());
            RAPDU rapdu = cardChannel.Transmit(capdu);

            if (rapdu == null)
            {
                Logger.Warning("Failed to send the HCE CONTROL START instruction to the reader ({0}: {1})", cardChannel.LastError, cardChannel.LastErrorAsString);
                cardChannel.DisconnectLeave();
                cardChannel = null;
                return false;
            }

            Logger.Debug(">{0}", rapdu.AsString());

            if (rapdu.SW != 0x9000)
            {
                Logger.Warning("The HCE CONTROL START instruction has failed with error {0:X04}", rapdu.SW);
                cardChannel.DisconnectLeave();
                cardChannel = null;
                return false;
            }

            emulationThread = new Thread(HceProc);
            emulationRunning = true;
            emulationThread.Start();

            return true;
        }

        private void HceProc()
        {
            bool selected = false;
            bool field_present = false;

            Logger.Trace("Entering card emulation thread...");

//            try
            {
                while (emulationRunning)
                {
                    if (!HcePoll(out byte status, out CAPDU capdu, EventTimeoutMs))
                    {
                        Logger.Warning("HcePoll failed, exiting");
                        break;
                    }

                    if (!emulationRunning)
                        break;

                    if ((status & 0x08) != 0x00)
                    {
                        /* Deselected */
                        if (selected)
                        {
                            Logger.Trace("Deselect");
                            OnDeselect();
                            selected = false;
                        }
                    }

                    if ((status & 0x01) != 0x00)
                    {
                        if (!field_present)
                        {
                            Logger.Trace("Field ON");
                            field_present = true;
                        }
                    }
                    else
                    {
                        if (field_present)
                        {
                            Logger.Trace("Field OFF");
                            field_present = false;
                        }
                    }

                    if ((status & 0x02) != 0x00)
                    {
                        Logger.Trace("Select");
                        selected = true;
                        OnSelect();
                    }

                    if (capdu != null)
                    {
                        Logger.Trace("C-APDU: {0}", capdu.AsString());

                        RAPDU rapdu = OnApdu(capdu);

                        if (!emulationRunning)
                            break;

                        if (rapdu == null)
                        {
                            Logger.Trace("No R-APDU?");
                        }
                        else
                        {
                            Logger.Trace("R-APDU: {0}", rapdu.AsString());

                            if (!HcePush(rapdu))
                            {
                                Logger.Warning("HcePush failed, exiting");
                                break;
                            }
                        }
                    }
                }
            }
//            catch (Exception e)
            {
//                Logger.Warning("Exception caught: " + e.Message);
            }

            Logger.Trace("Leaving card emulation thread...");

            if (emulationRunning)
            {
                emulationRunning = false;
                OnError();
            }
        }

        private bool HcePoll(out byte status, out CAPDU received_capdu, uint timeout_ms)
        {
            status = 0x00;
            received_capdu = null;

            /* HCE POLL */
            CAPDU capdu = new CAPDU(0xFF, 0xF7, 0x00, (byte) (timeout_ms / 100));
            Logger.Debug("<{0}", capdu.AsString());
            RAPDU rapdu = cardChannel.Transmit(capdu);

            if (rapdu == null)
            {
                Logger.Warning("Failed to send the HCE POLL instruction to the reader ({0}: {1})", cardChannel.LastError, cardChannel.LastErrorAsString);
                cardChannel.DisconnectLeave();
                cardChannel = null;
                return false;
            }

            Logger.Debug(">{0}", rapdu.AsString());

            if ((rapdu.SW & 0xF000) != 0x9000)
            {
                Logger.Warning("The HCE POLL instruction has failed with error {0:X04}", rapdu.SW);
                cardChannel.DisconnectLeave();
                cardChannel = null;
                return false;
            }

            Logger.Debug("Poll>{0}", rapdu.AsString());

            status = rapdu.SW2;

            if ((rapdu.DataBytes != null) && (rapdu.DataBytes.Length > 0))
            {
                received_capdu = new CAPDU(rapdu.DataBytes);
            }

            return true;
        }

        private bool HcePush(RAPDU send_rapdu)
        {
            /* HCE PUSH */
            CAPDU capdu = new CAPDU(0xFF, 0xF7, 0x01, 0x00, send_rapdu.Bytes);
            Logger.Debug("<{0}", capdu.AsString());
            RAPDU rapdu = cardChannel.Transmit(capdu);

            if (rapdu == null)
            {
                Logger.Warning("Failed to send the HCE PUSH instruction to the reader ({1}: {2})", cardChannel.LastError, cardChannel.LastErrorAsString);
                cardChannel.DisconnectLeave();
                cardChannel = null;
                return false;
            }

            Logger.Debug(">{0}", rapdu.AsString());

            if (rapdu.SW != 0x9000)
            {
                Logger.Warning("The HCE PUSH instruction has failed with error {0:X04}", rapdu.SW);
                cardChannel.DisconnectLeave();
                cardChannel = null;
                return false;
            }

            return true;
        }


        protected abstract RAPDU OnApdu(CAPDU capdu);
        protected abstract void OnSelect();
        protected abstract void OnDeselect();
        protected abstract void OnError();
    }
}
