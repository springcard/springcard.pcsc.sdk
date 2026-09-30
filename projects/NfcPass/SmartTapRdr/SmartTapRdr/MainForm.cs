using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SpringCard.LibCs;
using SpringCard.LibCs.Windows;
using SpringCard.LibCs.Windows.Forms;
using SpringCard.PCSC;
using SpringCard.GoogleVas;
using System.Threading;
using SpringCard.Licenses;
using SpringCard.Licenses.Forms;

namespace SmartTapRdr
{
    public partial class MainForm : Form
    {
        bool ShowSplash = true;

        SCardReader activeReader;
        GoogleVasConfig config;

        public MainForm(string[] args)
        {
            Logger.Trace("SmartTapRdr starting");

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

        public bool LoadConfigFromFile(string FileName)
        {
            config = GoogleVasConfig.LoadFromJsonFile(FileName);
            return true;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            if (config == null)
                LoadConfigFromRegistry();

            lkRefresh_LinkClicked(sender, null);
        }

        private void LoadConfigFromRegistry()
        {
            RegistryCfgFile registry = RegistryCfgFile.OpenApplicationSectionReadOnly("TerminalSettings");
            config = new GoogleVasConfig();
            config.Load(registry);
        }

        private bool CanPlay()
        {
            if (cbReaders.SelectedIndex >= 0)
                if (config.IsValid())
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
            lkSetConfig.Enabled = yesno;
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


        delegate void ShowResultInvoker(string StatusMsg, GoogleVasData DataMsg, bool CleanData);
        void ShowResult(string StatusMsg, GoogleVasData DataMsg, bool CleanData)
        {
            if (InvokeRequired)
            {
                this.BeginInvoke(new ShowResultInvoker(ShowResult), StatusMsg, DataMsg, CleanData);
                return;
            }

            toolStripStatusLabel1.Text = StatusMsg;
            if (DataMsg != null)
            {
                eSmartTapData.Text = DataMsg.GetJsonString();
            }
            else if (CleanData)
            {
                eSmartTapData.Text = "";
            }
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

            GoogleVasTerminal terminal = new GoogleVasTerminal(config);

            if (terminal.DoTransaction(cardChannel, out GoogleVasData data, out GoogleVasError error))
            {
                ShowResult("Message is OK", data, true);
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

        private void lkSetDefault_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            GoogleVASConfigForm f = new GoogleVASConfigForm();
            f.ShowDialog();
            LoadConfigFromRegistry();
            btnPlay.Enabled = (cbReaders.SelectedIndex >= 0) ? config.IsValid() : false;
        }

        private void lkClear_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            eSmartTapData.Text = "";
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
