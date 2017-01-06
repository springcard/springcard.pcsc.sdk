using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SpringCard.PCSC;
using SpringCard.PCSC.Desfire;

namespace desfireInformation
{
    public partial class Form1 : Form
    {
        private SCardReader reader = null;      // The reader's object
        private SCardChannel channel = null;    // A channel to the reader and card
        private string CR = System.Environment.NewLine;

        const byte DF_ISO_WRAPPING_CARD = 1;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            refreshReaders();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            refreshReaders();
        }

        private void refreshReaders()
        {
            cbReaders.Items.Clear();
            try
            {
                string[] readers = SCARD.Readers;
                foreach (string reader in readers)
                    cbReaders.Items.Add(reader);

                if (cbReaders.Items.Count > 0)
                    cbReaders.SelectedIndex = 0;
            }
            catch (Exception)
            {
                MessageBox.Show("There was a problem while searching for the list of readers, may be there's no reader?");
            }
        }

        private void cbReaders_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbReaders.SelectedIndex == -1)
                return;

            if (reader != null)
                reader.StopMonitor();

            try
            {
                string readerName = this.cbReaders.GetItemText(cbReaders.SelectedItem);
                reader = new SCardReader(readerName);
                reader.StartMonitor(readerStatusChanged);
            }
            catch (Exception Ex)
            {
                MessageBox.Show("There was an error while creating the reader's object : " + Ex.Message);
                return;
            }
        }

        delegate void readerStatusChangedInvoker(uint readerState, CardBuffer cardAtr);

        /// <summary>
        /// Callback used when the reader's status change
        /// As this method is called from a thread, you can't directly modify the user interface
        /// </summary>
        /// <param name="readerState"></param>
        /// <param name="cardAtr"></param>
        private void readerStatusChanged(uint readerState, CardBuffer cardAtr)
        {
            // When you are in a thread you can't directly modify the user interface
            if (InvokeRequired)
            {
                this.BeginInvoke(new readerStatusChangedInvoker(readerStatusChanged), readerState, cardAtr);
                return;
            }
            lblStatus.Text = "";
            txtHardwareVendor.Text = "";
            txtSoftwareVendor.Text = "";
            txtApplicationsList.Text = "";

            if (cardAtr == null)
                return;

            channel = new SCardChannel(reader);
            //channel.ShareMode = SCARD.SHARE_EXCLUSIVE;

            if (!channel.Connect())
            {
                lblStatus.Text = "Error, can't connect to the card";
                return;
            }

            int rc = SCARD_DESFIRE.AttachLibrary(channel.hCard);
            if (rc != SCARD.S_SUCCESS)
            {
                lblStatus.Text = "Error, failed to instantiate the PC/SC Desfire DLL";
                return;
            }

            rc = SCARD_DESFIRE.IsoWrapping(channel.hCard, DF_ISO_WRAPPING_CARD);
            if (rc != SCARD.S_SUCCESS)
            {
                lblStatus.Text = "Error, failed to select the ISO 7816 wrapping mode";
                return;
            }

            // ***********************
            // * Get card's versions *
            // ***********************
            byte[] versionInfo = new byte[30];

            rc = SCARD_DESFIRE.GetVersion(channel.hCard, versionInfo);
            if (rc != SCARD.S_SUCCESS)
            {
                lblStatus.Text = "Error, Desfire 'GetVersion' command failed - error: " + rc;
                return;
            }

            lblStatus.Text = "Desfire 'GetVersion' command ok";
                
            txtHardwareVendor.Text = "Vendor Id: " + versionInfo[0] + CR + "Type: " + versionInfo[1] + CR + "SubType: " + versionInfo[2] + CR + "Version: " + versionInfo[3] + "." + versionInfo[4];
            txtSoftwareVendor.Text = "Vendor Id: " + versionInfo[7] + CR + "Type: " + versionInfo[8] + CR + "SubType: " + versionInfo[9] + CR + "Version: " + versionInfo[10] + "." + versionInfo[11];
            if ((versionInfo[0] != 0x04) || (versionInfo[7] != 0x04))
            {
                txtSoftwareVendor.Text += CR + "Manufacturer is not NXP";
            }

            if ((versionInfo[1] != 0x01) || (versionInfo[8] != 0x01))
            {
                txtSoftwareVendor.Text += CR + "Type is not Desfire";
            }

            if (versionInfo[10] < 1)
                txtSoftwareVendor.Text += CR + "Software version is below EV1";

            // *************************
            // * Get applications list *
            // *************************
            UInt32[] aidsList = new UInt32[28];
            byte aidCount = 0;
            rc = SCARD_DESFIRE.GetApplicationIDs(channel.hCard, (byte) 28, aidsList, ref aidCount);
            if (rc != SCARD.S_SUCCESS)
            {
                lblStatus.Text = "Error, Desfire 'GetApplicationIDs' command failed - error: " + rc;
                return;
            }
            txtApplicationsList.Text = "Applications'count: "+ aidCount + CR;
            if (aidCount == 0)
                return;

            for(byte i = 0; i < aidCount; i++)
            {
                txtApplicationsList.Text += "AID: " + "0x" + aidsList[i].ToString("X6") + CR;
            }

            rc = SCARD_DESFIRE.DetachLibrary(channel.hCard);
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            // Stop properly
            if (channel != null)
            {
                channel.Disconnect();
                channel = null;
            }

            if (reader != null)
            {
                reader.StopMonitor();
                reader = null;
            }
        }
    }
}
