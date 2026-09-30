using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using SpringCard.LibCs;
using SpringCard.LibCs.Windows;
using SpringCard.LibCs.Windows.Forms;
using SpringCard.PCSC;
using SpringCard.GoogleVas;
using SpringCard.AppleVas;
using SmartTapRdr;
using PassKitRdr;
using System.Text.RegularExpressions;

namespace SpringPassRdr
{
    public partial class MainForm : Form
    {
        bool ShowSplash = true;

        SCardReader activeReader;
        GoogleVasConfig googleConfig;
        AppleVasTerminalConfig appleConfig;

        public MainForm(string[] args)
        {
            Logger.Trace("SpringPass starting");

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

        public bool LoadGoogleConfigFromFile(string FileName)
        {
            googleConfig = GoogleVasConfig.LoadFromJsonFile(FileName);
            return true;
        }

        public bool LoadAppleConfigFromFile(string FileName)
        {
            appleConfig = AppleVasTerminalConfig.LoadFromJsonFile(FileName);
            return true;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            /* Load configurations */
            GoogleVASConfigForm googleConfigForm = new GoogleVASConfigForm();
            googleConfig = googleConfigForm.GetConfig();
            AppleVASConfigForm appleConfigForm = new AppleVASConfigForm();
            appleConfig = appleConfigForm.GetConfig();

            /* Load readers list */
            lkRefresh_LinkClicked(sender, null);
        }

        private bool CanPlay()
        {
            // TODO check apple and google config
            if (cbReaders.SelectedIndex >= 0)
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
            lkSetAppleConfig.Enabled = yesno;
            lkSetGoogleConfig.Enabled = yesno;
            lkLicense.Enabled = yesno;
            lkAbout.Enabled = yesno;
            lkSubscribe.Enabled = yesno;
            cbReaders.Enabled = yesno;
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


        delegate void ShowResultInvoker(string StatusMsg, string DataMsg, bool CleanData);
        void ShowResult(string StatusMsg, string DataMsg, bool CleanData)
        {
            if (InvokeRequired)
            {
                this.BeginInvoke(new ShowResultInvoker(ShowResult), StatusMsg, DataMsg, CleanData);
                return;
            }

            toolStripStatusLabel1.Text = StatusMsg;
            if (DataMsg != null)
            {
                eSpringPassData.Text = DataMsg;
            }
            else if (CleanData)
            {
                eSpringPassData.Text = "";
            }
        }

        void CardRemovedCallback()
        {
            ShowResult(null, null, false);
        }

        void CardConnectedCallback(SCardChannel cardChannel)
        {
            /* The CardConnected function is called as a delegate (callback) by the SCardReader object */
            /* within its backgroung thread. Therefore it is not allowed to use the UI objects.        */

            Logger.Trace("Card connected - Trying to run the transaction");


            /* Apple exhange */

            AppleVasTerminal appleTerminal = new AppleVasTerminal();
            foreach (AppleVasConfig merchConfig in appleConfig.Merchants)
            {
                appleTerminal.AddConfig(merchConfig);
            }

            if (appleTerminal.DoTransaction(cardChannel, out AppleVasData dataApple, out AppleVasError errorApple, out RAPDU selectOseResponseApple))
            {
                string value = null;

                if (dataApple != null)
                {
                    Logger.Debug("AppleVasData: {0}", dataApple.ToString());
                    value = dataApple.Text;
                }

                if (value != null)
                {
                    ShowResult("Apple VAS message", value, true);
                }
                else if (errorApple != AppleVasError.Success)
                {
                    ShowResult(string.Format("Read Apple VAS error {0}", errorApple), null, true);
                }
                else
                {
                    ShowResult("Read Apple VAS OK, but SpringPass data is empty", null, true);
                }
                return;
            }

            /* Google exhange */

            GoogleVasTerminal googleTerminal = new GoogleVasTerminal(googleConfig);

            /* Provide selectOseResponseApple to speed up the transaction */
            if (googleTerminal.DoTransaction(cardChannel, out GoogleVasData dataGoogle, out GoogleVasError errorGoogle)) // , selectOseResponseApple))
            {
                string value = null;

                if (dataGoogle != null)
                {
                    Logger.Debug("GoogleVasData: {0}", dataGoogle.ToString());
                    try
                    {
                        /* Convert JSON to SpringPass data with regex */
                        Regex rx = new Regex(@"[0-9a-zA-Z]+\|[a-z0-9.-_]+@[a-z.]+", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                        string json = dataGoogle.GetJsonString();
                        Logger.Debug("GoogleVasData: {0}", json);
                        MatchCollection matches = rx.Matches(json);
                        if ((matches != null) && (matches.Count >= 1))
                            value = matches[0].Value;
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e.Message);
                    }
                }

                if (value != null)
                {
                    ShowResult("Google VAS message", value, true);
                }
                else if (errorGoogle != GoogleVasError.Success)
                {
                    ShowResult(string.Format("Read Google VAS error {0}", errorGoogle), null, true);
                }
                else
                {
                    ShowResult("Read Google VAS OK, but SpringPass data is missing", null, true);
                }
                return;
            }

            ShowResult("Read message failed, reason: ", null, false);
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

        private void lkSetGoogleConfiguration_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            GoogleVASConfigForm f = new GoogleVASConfigForm();
            f.ShowDialog();
            googleConfig = f.GetConfig();
            btnPlay.Enabled = (cbReaders.SelectedIndex >= 0) ? googleConfig.IsValid() : false;
        }

        private void lkSetAppleConfiguration_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            AppleVASConfigForm f = new AppleVASConfigForm();
            f.ShowDialog();
            appleConfig = f.GetConfig();
            // TODO check apple config
            //btnPlay.Enabled = (cbReaders.SelectedIndex >= 0) ? configApple.IsValid() : false;
        }

        private void lkClear_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            eSpringPassData.Text = "";
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            if (ShowSplash)
            {
                Logger.Trace("Showing splash form");
                SplashForm.DoShowDialog(this, FormStyle.ModernRed);
            }
        }
    }
}
