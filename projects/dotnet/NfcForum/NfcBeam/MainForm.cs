/* Created by SharpDevelop.
 * User: johann
 * Date: 02/03/2012
 * Time: 17:56
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using SpringCard.LibCs;
using SpringCard.LibCs.Windows;
using SpringCard.LibCs.Windows.Forms;
using SpringCard.NFC;
using SpringCard.NfcForum.Ndef;
using SpringCard.PCSC;
using SpringCard.PCSC.Forms;
using SpringCardApplication.Controls;
using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace SpringCardApplication
{
    public partial class MainForm : Form
    {
        bool ShowSplash = true;

        enum Status
        {
            Idle,
            RecvOnly_Init,
            RecvOnly_Ready,
            SendOnly_Init,
            SendOnly_Ready,
            RecvThenSend_Init,
            RecvThenSend_RecvReady,
            RecvThenSend_RecvDone,
            RecvThenSend_SendReady,
            SendThenRecv_Init,
            SendThenRecv_SendReady,
            SendThenRecv_SendDone,
            SendThenRecv_RecvReady,
            SendAndRecv_Init,
            SendAndRecv_Ready
        }

        NdefDisplayForm display = null;
        SCardReader reader = null;
        SCardChannel channel = null;
        RtdControl control = null;

        LLCP llcp = null;

        Status status = Status.Idle;

        bool ready = false;

        public MainForm()
        {
            InitializeComponent();

            //TopMost = true;

            Settings s = new Settings();
            if (s.ShowConsole)
            {
                SystemConsole.Show();
                Trace.Listeners.Add(new ConsoleTraceListener());
            }
            miShowConsole.Checked = s.ShowConsole;

			Text = AppUtils.ApplicationTitle(true);

            ready = true;
        }

        void PerformStatusChange()
        {
            switch (status)
            {
                case Status.SendAndRecv_Init:
                    AnnounceReceiver(true);
                    toolStripStatusLabelMode.Text = "Sender and Receiver";
                    status = Status.SendAndRecv_Ready;
                    break;
                case Status.SendAndRecv_Ready:
                    break;

                case Status.RecvOnly_Init:
                    AnnounceReceiver(true);
                    toolStripStatusLabelMode.Text = "Receiver";
                    status = Status.RecvOnly_Ready;
                    break;
                case Status.RecvOnly_Ready:
                    break;

                case Status.SendOnly_Init:
                    toolStripStatusLabelMode.Text = "Sender";
                    AnnounceReceiver(false);
                    status = Status.SendOnly_Ready;
                    break;
                case Status.SendOnly_Ready:
                    break;

                case Status.RecvThenSend_Init:
                    toolStripStatusLabelMode.Text = "Receiver (will send afterwards)";
                    AnnounceReceiver(true);
                    status = Status.RecvThenSend_RecvReady;
                    break;
                case Status.RecvThenSend_RecvReady:
                    break;
                case Status.RecvThenSend_RecvDone:
                    toolStripStatusLabelMode.Text = "Sender (receive OK)";
                    AnnounceReceiver(false);
                    status = Status.RecvThenSend_SendReady;
                    break;
                case Status.RecvThenSend_SendReady:
                    break;

                case Status.SendThenRecv_Init:
                    toolStripStatusLabelMode.Text = "Sender (will receive afterwards)";
                    AnnounceReceiver(false);
                    status = Status.SendThenRecv_SendReady;
                    break;
                case Status.SendThenRecv_SendReady:
                    break;
                case Status.SendThenRecv_SendDone:
                    toolStripStatusLabelMode.Text = "Receiver (sent OK)";
                    AnnounceReceiver(true);
                    status = Status.SendThenRecv_RecvReady;
                    break;
                case Status.SendThenRecv_RecvReady:
                    break;

                default:
                    toolStripStatusLabelMode.Text = status.ToString();
                    break;
            }
        }

        delegate void ReaderStatusChangedInvoker(uint ReaderState, CardBuffer CardAtr);
        void ReaderStatusChanged(uint ReaderState, CardBuffer CardAtr)
        {
            /* The ReaderStatusChanged function is called as a delegate (callback) by the SCardReader object    */
            /* within its backgroung thread. Therefore we must use the BeginInvoke syntax to switch back from   */
            /* the context of the background thread to the context of the application's main thread. Overwise   */
            /* we'll get a security violation when trying to access the window's visual components (that belong */
            /* to the application's main thread and can't be safely manipulated by background threads).         */
            if (InvokeRequired)
            {
                this.BeginInvoke(new ReaderStatusChangedInvoker(ReaderStatusChanged), ReaderState, CardAtr);
                return;
            }

            string s = SCARD.ReaderStatusToString(ReaderState);

            Logger.Trace("ReaderStatusChanged: " + s);
            eReaderStatus.Text = s;

            if (CardAtr != null)
            {
                s = CardAtr.AsString(" ");
                eCardAtr.Text = s;
                Logger.Trace("\t" + s);
            }
            else
            {
                eCardAtr.Text = "";
            }

            if (ReaderState == SCARD.STATE_UNAWARE)
            {
                if (llcp != null)
                {
                    llcp.Stop();
                    llcp = null;
                }

                MessageBox.Show("The reader we were working on has disappeared from the system. This application will terminate now.",
                                "The reader has been removed",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Exclamation);

                Application.Exit();

            }
            else if ((ReaderState & SCARD.STATE_UNAVAILABLE) != 0)
            {
                Logger.Trace("Card unavailable");
            }
            else if ((ReaderState & SCARD.STATE_MUTE) != 0)
            {
                Logger.Trace("Card mute");
            }
            else if ((ReaderState & SCARD.STATE_INUSE) != 0)
            {
                Logger.Trace("Card in use");
            }
            else if ((ReaderState & SCARD.STATE_EMPTY) != 0)
            {
                Logger.Trace("Card absent");

                if (llcp != null)
                {
                    Logger.Trace("*** Target lost ***");

                    llcp.Stop();
                    llcp = null;

                    PerformStatusChange();
                }

                pControl.Enabled = true;
            }
            else if ((ReaderState & SCARD.STATE_PRESENT) != 0)
            {
                Logger.Trace("Card present");

                reader.StopMonitor();
                channel = new SCardChannel(reader);

                if (channel.Connect())
                {
                    Logger.Trace("Connected to the card");

                    pControl.Enabled = false;
                    Logger.Trace("*** Starting LLCP ***");

                    llcp = new LlcpInitiator(channel);

                    llcp.OnLinkClosed = new LLCP.LinkClosedCallback(RestartMonitor);

                    switch (status)
                    {
                        case Status.SendAndRecv_Ready:
                            /* We are both a receiver and a sender */
                            StartReceiver();
                            StartSender();
                            break;

                        case Status.RecvOnly_Ready:
                        case Status.RecvThenSend_RecvReady:
                        case Status.SendThenRecv_RecvReady:
                            /* We are a receiver */
                            StartReceiver();
                            break;

                        case Status.SendOnly_Ready:
                        case Status.RecvThenSend_SendReady:
                        case Status.SendThenRecv_SendReady:
                            /* We are a sender */
                            StartSender();
                            break;

                        default:
                            break;
                    }

                    if (!llcp.Start())
                    {
                        Logger.Trace("*** Failed to start LLCP ***");
                        MessageBox.Show("Failed to instantiate LLCP layer on the newly inserted card. This is likely to be caused by another application taking control of the card before us. Please close all other PC/SC-aware applications. It may also be required to instruct Windows to stop probing the card, see the paragraph pcsc_no_minidriver in the Readme of the PC/SC SDK for more informations.", "Communication error");
                        llcp = null;
                        pControl.Enabled = true;
                        reader.StartMonitor(new SCardReader.StatusChangeCallback(ReaderStatusChanged));
                    }
                }
                else
                {
                    Logger.Trace("Can't connect to the card - will be retried later on");
                    reader.StartMonitor(new SCardReader.StatusChangeCallback(ReaderStatusChanged));
                }                
            }
        }

        void RestartMonitor()
        {
            reader.StartMonitor(new SCardReader.StatusChangeCallback(ReaderStatusChanged));
        }

        void StartReceiver()
        {
            /* Instanciate a SNEP server - if a message arrives, it will call our OnNdefMessageReceived method */
            Logger.Trace("Create (and start) SNEP Server (receiver)");
            llcp.RegisterServer(new SNEP_Server(OnNdefMessageReceived));
        }

        void StartSender()
        {
            /* Instanciate a SNEP client */
            NdefObject myNdef = CreateMyNdefMessage();

            if (myNdef != null)
            {
                Logger.Trace("Create SNEP Client (sender)");
                SNEP_Client client = new SNEP_Client(myNdef);
                client.OnMessageSent = new SNEP_Client.SNEPMessageSentCallback(OnNdefMessageSent);
                client.MessageSentCallbackOnAcknowledge = true;
                llcp.RegisterClient(client);
            }
            else
            {
                Logger.Trace("Can't create SNEP Client: nothing to send");
            }
        }

        void MainFormShown(object sender, EventArgs e)
        {
            if (ShowSplash)
            {
                Logger.Trace("Showing splash form");
                SplashForm.DoShowDialog(this, FormStyle.ModernMarroon);
            }

            Logger.Trace("Loading settings from registry");

            Settings s = new Settings();

            Logger.Trace("Selected mode?");

            try
            {
                switch (s.SelectedMode)
                {
                    case Settings.MODE_RECV_ONLY:
                        rbRecvOnly.Checked = true;
                        Logger.Trace("Mode set to Receiver");
                        break;
                    case Settings.MODE_SEND_ONLY:
                        rbSendOnly.Checked = true;
                        Logger.Trace("Mode set to Sender");
                        break;
                    case Settings.MODE_RECV_THEN_SEND:
                        rbRecvThenSend.Checked = true;
                        Logger.Trace("Mode set to Receiver, then Sender");
                        break;
                    case Settings.MODE_SEND_THEN_RECV:
                        rbSendThenRecv.Checked = true;
                        Logger.Trace("Mode set to Sender, then Receiver");
                        break;
                    case Settings.MODE_SEND_AND_RECV:
                    default:
                        rbSendAndRecv.Checked = true;
                        Logger.Trace("Mode set to Sender+Receiver");
                        break;
                }
            }
            catch
            {
                rbRecvOnly.Checked = true;
                Logger.Trace("Mode set to Receiver (default)");
            }

            if ((s.SelectedType >= 0) && (s.SelectedType < cbNdefType.Items.Count))
                cbNdefType.SelectedIndex = s.SelectedType;
            else
                cbNdefType.SelectedIndex = 0;

            ReaderSelectForm readerSelect = new ReaderSelectForm(this.imgHeader.BackColor);

            if (readerSelect.SelectedReader == null)
            {
                readerSelect.Preselect("SpringCard|NFC|tactless");
                for (; ; )
                {
                    readerSelect.ShowDialog();
                    if (readerSelect.SelectedReader != null)
                        break;

                    if (MessageBox.Show("This application can't run without a reader. Do you want to leave the application now ?",
                                        "No reader selected",
                                        MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                    {
                        Application.Exit();
                        return;
                    }
                }
            }

            eReaderStatus.Text = "";
            eCardAtr.Text = "";

            SelectReader(readerSelect.SelectedReader);
        }

        void MainFormFormClosed(object sender, FormClosedEventArgs e)
        {
            if (llcp != null)
            {
                llcp.Stop();
                llcp = null;
            }

            ReleaseReader();

            Settings s = new Settings();

            s.SelectedMode = Settings.MODE_SEND_AND_RECV;
            if (rbRecvOnly.Checked) s.SelectedMode = Settings.MODE_RECV_ONLY;
            else
            if (rbSendOnly.Checked) s.SelectedMode = Settings.MODE_SEND_ONLY;
            else
            if (rbRecvThenSend.Checked) s.SelectedMode = Settings.MODE_RECV_THEN_SEND;
            else
            if (rbSendThenRecv.Checked) s.SelectedMode = Settings.MODE_SEND_THEN_RECV;
            else
            if (rbSendAndRecv.Checked) s.SelectedMode = Settings.MODE_SEND_AND_RECV;

            s.SelectedType = cbNdefType.SelectedIndex;

            s.Save();
        }

        void AboutToolStripMenuItemClick(object sender, EventArgs e)
        {
            AboutForm f = new AboutForm();
            f.SetHeaderColor(imgHeader.BackColor);
            f.ShowDialog();
        }


        void ChangeReader()
        {
            ReleaseReader();

            eReaderStatus.Text = "";
            eCardAtr.Text = "";

            ReaderSelectForm readerSelect = new ReaderSelectForm(imgHeader.BackColor);
            readerSelect.SelectedReader = eReaderName.Text;
            readerSelect.ShowDialog();
            if (readerSelect.SelectedReader != null)
                SelectReader(readerSelect.SelectedReader);
        }

        void SelectReader(string ReaderName)
        {
            eReaderName.Text = ReaderName;
            reader = new SCardReader(ReaderName);

            LimitToP2PProtocols(true);

            CbModeCheckedChanged(null, null);
            CbNdefTypeSelectedIndexChanged(null, null);

            reader.StartMonitor(new SCardReader.StatusChangeCallback(ReaderStatusChanged));
        }

        void ReleaseReader()
        {
            if (reader != null)
            {
                reader.StopMonitor();
                LimitToP2PProtocols(false);
                reader = null;
            }
        }

        void MainFormFormClosing(object sender, FormClosingEventArgs e)
        {

        }

        void imgLogoClick(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.springcard.com");
        }

        void BtnReaderChangeClick(object sender, System.EventArgs e)
        {
            ChangeReader();
        }

        void QuitToolStripMenuItemClick(object sender, EventArgs e)
        {
            Close();
        }

        NdefObject CreateMyNdefMessage()
        {
            if (control == null)
                return null;

            return control.GetContent();
        }


        delegate void OnNdefMessageReceivedInvoker(NdefObject hisNdef);
        void OnNdefMessageReceived(NdefObject hisNdef)
        {
            /* The OnNdefMessageReceived function is called as a delegate (callback) within the NfcLlcp         */
            /* object's backgroung thread. Therefore we must use the BeginInvoke syntax to switch back from     */
            /* the context of the background thread to the context of the application's main thread. Overwise   */
            /* we'll get a security violation when trying to access the window's visual components (that belong */
            /* to the application's main thread and can't be safely manipulated by background threads).         */
            if (InvokeRequired)
            {
                this.BeginInvoke(new OnNdefMessageReceivedInvoker(OnNdefMessageReceived), hisNdef);
                return;
            }

            /* Message received, what shall we do now ? */
            /* ---------------------------------------- */

            Logger.Trace("*** NDEF message received ***");

            switch (status)
            {
                case Status.SendThenRecv_RecvReady:
                    /* Thank you very much, back to send mode */
                    status = Status.SendThenRecv_Init;
                    if (llcp != null)
                        llcp.StopAndResetPeer();
                    break;

                case Status.RecvThenSend_RecvReady:
                    /* Thank you very much, going to send */
                    status = Status.RecvThenSend_RecvDone;
                    if (llcp != null)
                        llcp.StopAndResetPeer();
                    break;

                default:
                    break;
            }

            if (hisNdef == null)
            {
                Logger.Trace("The message is empty");
                return;
            }

            if (display != null)
            {
                /* We already are displaying a message, we must wait until the user closes the form */
                return;
            }


            display = new NdefDisplayForm();

            //  	  string t = (new CardBuffer(hisNdef.GetBytes())).AsString(" ");
            //  	  MessageBox.Show(t);


            display.SetNdef(hisNdef);
            display.ShowDialog();
            display = null;
        }

        delegate void OnNdefMessageSentInvoker();
        void OnNdefMessageSent()
        {
            if (InvokeRequired)
            {
                this.BeginInvoke(new OnNdefMessageSentInvoker(OnNdefMessageSent));
                return;
            }

            Logger.Trace("*** NDEF message sent ***");

            switch (status)
            {
                case Status.SendThenRecv_SendReady:
                    /* Sent OK */
                    status = Status.SendThenRecv_SendDone;
                    break;

                case Status.RecvThenSend_SendReady:
                    /* Send OK */
                    status = Status.RecvThenSend_Init;
                    break;

                default:
                    break;
            }
        }

        void CbNdefTypeSelectedIndexChanged(object sender, EventArgs e)
        {
            if (control != null)
            {
                pMain.Controls.Remove(control);
                control.Dispose();
                control = null;
            }

            switch (cbNdefType.SelectedIndex)
            {
                case 0:
                    /* Text */
                    control = new RtdTextControl();
                    break;

                case 1:
                    /* URI */
                    control = new RtdUriControl();
                    break;

                case 2:
                    /* SmartPoster */
                    control = new RtdSmartPosterControl();
                    break;

                case 3:
                    /* vCard */
                    control = new RtdVCardControl();
                    break;

                case 4:
                    /* Arbitrary MIME Media (text) */
                    control = new RtdMediaControl();
                    break;

                case 5:
                    /* WifiHandover */
                    control = new RtdWifiHandoverControl();
                    break;

                default:
                    break;
            }

            if (control != null)
            {
                control.Dock = DockStyle.Fill;
                pMain.Controls.Add(control);
            }
        }

        void MiShowConsoleCheckedChanged(object sender, EventArgs e)
        {
            if (!ready)
                return;

            Settings s = new Settings();
            s.ShowConsole = miShowConsole.Checked;
            s.Save();
            if (miShowConsole.Checked)
            {
                SystemConsole.Show();
                Trace.Listeners.Add(new ConsoleTraceListener());
            }
            else
            {
                Trace.Listeners.Clear();
                SystemConsole.Hide();
            }
        }

        bool AnnounceReceiver(bool yes)
        {
            if (reader != null)
            {
                if (yes)
                    Logger.Trace("Announce receiver: yes");
                else
                    Logger.Trace("Announce receiver: no");

                if (!LlcpInitiator.AnnounceSnepServer(reader, yes))
                {
                    MessageBox.Show("Warning: problem with ATR GI");
                    return false;
                }
            }
            return true;
        }

        bool LimitToP2PProtocols(bool yes)
        {
            if (reader != null)
            {
                if (yes)
                    Logger.Trace("Limit to P2P protocols: yes");
                else
                    Logger.Trace("Limit to P2P protocols: no");

                if (!LlcpInitiator.LimitToP2PProtocols(reader, yes))
                {
                    MessageBox.Show("Warning: problem with Control");
                    return false;
                }
            }
            return true;
        }

        void CbModeCheckedChanged(object sender, EventArgs e)
        {
            if (rbSendAndRecv.Checked)
            {
                status = Status.SendAndRecv_Init;
            }
            else if (rbSendOnly.Checked)
            {
                status = Status.SendOnly_Init;
            }
            else if (rbSendThenRecv.Checked)
            {
                status = Status.SendThenRecv_Init;
            }
            else if (rbRecvOnly.Checked)
            {
                status = Status.RecvOnly_Init;
            }
            else if (rbRecvThenSend.Checked)
            {
                status = Status.RecvThenSend_Init;
            }

            switch (status)
            {
                case Status.SendAndRecv_Init:
                case Status.SendOnly_Init:
                case Status.SendThenRecv_Init:
                case Status.RecvThenSend_Init:
                    /* We could send */
                    cbNdefType.Enabled = true;
                    pMain.Enabled = true;
                    break;

                case Status.RecvOnly_Init:
                    /* We couldn't */
                    cbNdefType.Enabled = false;
                    pMain.Enabled = false;
                    break;

                default:
                    break;
            }

            PerformStatusChange();
        }
    }
}
