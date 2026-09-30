using System;
using System.Windows.Forms;
using SpringCard.LibCs;
using SpringCard.LibCs.Windows;
using SpringCard.LibCs.Windows.Forms;
using SpringCard.PCSC;
using SpringCard.AppleVas;

namespace PassKitRdr
{
    public partial class MainForm : Form
    {
        SCardReader activeReader;
        AppleVasTerminalConfig terminalConfig;
        bool ShowSplash = true;

        public MainForm(string[] args)
        {
            Logger.Trace("AppleVasRdr starting");
            InitializeComponent();

            if (args != null)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    string s = args[i].ToLower();
                    if ((s == "--no_splash") || (s == "-q"))
                    {
                        ShowSplash = false;
                    }
                }
            }

            Text = AppUtils.ApplicationTitle(true);
        }

        void DumpTlv(Asn1Tlv tlv, int depth = 0)
        {
            string lineHeader = "";
            for (int i = 0; i < depth; i++) lineHeader += "\t";
            Logger.Debug("{0}Tag={1:X02}, L={2}, V={3}", lineHeader, tlv.T_AsByte, tlv.L, BinConvert.ToHex(tlv.V));
            if (tlv.T_AsByte == 0x30)
            {
                /* Sequence */
                byte[] buffer = tlv.V;
                while ((buffer != null) && (buffer.Length != 0))
                {
                    Asn1Tlv childTlv = Asn1Tlv.Unserialize(buffer, out buffer);
                    if (childTlv == null)
                        break;
                    DumpTlv(childTlv, depth + 1);
                }
            }
        }

        void Test(string pubKeyStr)
        {
            byte[] publicKey = StrUtils.Base64Decode(pubKeyStr);
            Logger.Debug("Raw public key:" + BinConvert.ToHex(publicKey));
            Asn1Tlv tlv = Asn1Tlv.Unserialize(publicKey);
            DumpTlv(tlv);
        }


        private void MainForm_Load(object sender, EventArgs e)
        {
            /* Load config */
            AppleVASConfigForm f = new AppleVASConfigForm();
            terminalConfig = f.GetConfig();

            /* Refresh readers list */
            lkRefresh_LinkClicked(sender, null);

            if ((terminalConfig == null) || (!terminalConfig.IsValid()))
            {
                Logger.Debug("Terminal configuration is invalid");
            }
        }

        private bool CanPlay()
        {
            if (cbReaders.SelectedIndex >= 0)
                if ((terminalConfig != null) && (terminalConfig.IsValid()))
                    return true;
            return false;
        }

        private void lkRefresh_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string oldReader = AppConfig.ReadSettingString("Reader");
            string[] readers = SCARD.Readers;
            cbReaders.Items.Clear();
            if (readers != null)
                foreach (string reader in readers)
                {
                    cbReaders.Items.Add(reader);
                    if (reader == oldReader)
                        cbReaders.SelectedIndex = cbReaders.Items.Count - 1;
                }
            btnPlay.Enabled = CanPlay();
        }

        private void cbReaders_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbReaders.SelectedIndex >= 0)
            {
                string reader = cbReaders.Text;
                Logger.Trace("Reader={0}", reader);
                AppConfig.WriteSettingString("Reader", reader);
            }
            btnPlay.Enabled = CanPlay();
        }

        private void EnableControls(bool yesno)
        {
            lkRefresh.Enabled = yesno;
            lkLicense.Enabled = yesno;
            lkAbout.Enabled = yesno;
            lkSubscribe.Enabled = yesno;
            cbReaders.Enabled = yesno;
            lkSetConfig.Enabled = yesno;
        }

        private void btnPlay_Click(object sender, EventArgs e)
        {
            if (cbReaders.SelectedIndex >= 0)
            {
                btnPlay.Enabled = false;
                EnableControls(false);

                string reader = cbReaders.Text;
                Logger.Trace("Starting...");

                ShowResult("Waiting for NFC mobile...", null, true);

                activeReader = new SCardReader(reader);
                activeReader.StartWaitCard(new SCardReader.CardConnectedCallback(CardConnectedCallback), new SCardReader.CardRemovedCallback(CardRemovedCallback));
                btnPause.Enabled = true;
            }
        }

        private void btnPause_Click(object sender, EventArgs e)
        {
            if (activeReader != null)
            {
                btnPause.Enabled = false;

                Logger.Trace("Stopping...");
                activeReader.StopWaitCard();
                activeReader = null;

                ShowResult("Press the \"Play\" button when ready.", null, false);

                EnableControls(true);
                btnPlay.Enabled = CanPlay();
            }
        }

        delegate void ShowResultInvoker(string StatusMsg, AppleVasData DataMsg, bool CleanData);
        void ShowResult(string StatusMsg, AppleVasData DataMsg, bool CleanData)
        {
            if (InvokeRequired)
            {
                this.BeginInvoke(new ShowResultInvoker(ShowResult), StatusMsg, DataMsg, CleanData);
                return;
            }

            if (StatusMsg != null)
                toolStripStatusLabel1.Text = StatusMsg;

            if (DataMsg != null)
            {
                ePassMessage.Text = DataMsg.Text;
                ePassTimestamp.Text = DataMsg.Timestamp.ToString();
                if (DataMsg.IsTimestampAllowed())
                {
                    imgCross.Visible = false;
                    imgCheck.Visible = true;
                    toolStripStatusLabel1.Text = "Message is OK";
                }
                else
                {
                    imgCheck.Visible = false;
                    imgCross.Visible = true;
                    toolStripStatusLabel1.Text = "Message is OK but clock screw detected";
                }
            }
            else if (CleanData)
            {
                imgCheck.Visible = false;
                imgCross.Visible = false;
            }

            /* if (AppConfig.ReadBoolean("AutoClear", true))
             {
                 timerAutoClear.Interval = 1000 * AppConfig.ReadInteger("ClearAfter", 10);
                 timerAutoClear.Enabled = true;
             }*/
        }


        void CardRemovedCallback()
        {
            ShowResult("Waiting for NFC mobile...", null, false);
        }

        void CardConnectedCallback(SCardChannel cardChannel)
        {
            /* The CardConnected function is called as a delegate (callback) by the SCardReader object */
            /* within its backgroung thread. Therefore it is not allowed to use the UI objects.        */

            Logger.Trace("Card connected - Trying to run the transaction");

            AppleVasTerminal terminal = new AppleVasTerminal();
            foreach (AppleVasConfig merchConfig in terminalConfig.Merchants)
            {
                terminal.AddConfig(merchConfig);
            }

            if (terminal.DoTransaction(cardChannel, out AppleVasData data, out AppleVasError error))
            {
                ShowResult(null, data, true);
            }
            else
            {
                ShowResult("Read message failed, reason: " + error.ToString(), null, true);
            }
        }


        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (activeReader != null)
            {
                activeReader.StopWaitCard();
                activeReader = null;
            }
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void lkSubscribe_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://playground.springpass.prod.springcard.com/");
        }

        private void lkAbout_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            AboutForm.DoShowDialog(this, FormStyle.ModernRed);
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            if (ShowSplash)
            {
                Logger.Trace("Showing splash form");
                SplashForm.DoShowDialog(this, FormStyle.ModernRed);
            }
        }

        private void lkSetConfiguration_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            AppleVASConfigForm f = new AppleVASConfigForm();
            f.ShowDialog();
            terminalConfig = f.GetConfig();
            btnPlay.Enabled = CanPlay();
        }

        private void Clear()
        {
            ePassMessage.Text = "";
            ePassTimestamp.Text = "";
            imgCheck.Visible = false;
            imgCross.Visible = false;
        }

        private void imgCross_Click(object sender, EventArgs e)
        {
            Clear();
        }

        private void imgCheck_Click(object sender, EventArgs e)
        {
            Clear();
        }

        private void timerAutoClear_Tick(object sender, EventArgs e)
        {
            timerAutoClear.Enabled = false;
            Clear();
        }
    }
}
