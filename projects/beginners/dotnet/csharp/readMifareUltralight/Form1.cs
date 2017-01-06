using System;
using System.Windows.Forms;
using SpringCard.PCSC;

namespace readMifareUltralight
{
    public partial class Form1 : Form
    {
        private SCardReader reader = null;      // The reader's object
        private SCardChannel channel = null;    // A channel to the reader and card

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
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

        private void btnRead_Click(object sender, EventArgs e)
        {
            txtAsciiContent.Text = "";
            txtHexData.Text = "";

            byte[] readApdu = new byte[] { 0xFF, 0xB0, 0x00, 0x04, 0x30 };  // Read binary
            CAPDU capdu = new CAPDU(readApdu);                          // Command sent to the reader
            RAPDU rapdu = channel.Transmit(capdu);                      // Response sent from card
            if (rapdu == null)
            {
                txtFinalStatus.Text = "Problem while reading";
                return;
            }
            if (rapdu.SW != 0x9000)                                  // Something failed
            {
                txtFinalStatus.Text = "Error:" + String.Format("{0:X}", rapdu.SW) + ": " + SCARD.CardStatusWordsToString(rapdu.SW);
                return;
            }
            txtHexData.Text = BitConverter.ToString(rapdu.GetBytes()).Replace('-', ' ');
            txtAsciiContent.Text = ByteArrayToString(rapdu.GetBytes());
            txtFinalStatus.Text = "Read with success";
        }

        public string ByteArrayToString(byte[] val, bool stopOnZero = true, bool hideNonAscii = true)
        {
            string s = "";
            if (val != null)
            {
                for (int i = 0; i < val.Length; i++)
                {
                    if ((val[i] == 0) && stopOnZero)
                        break;
                    if ((val[i] <= ' ') && hideNonAscii)
                    {
                        s = s + '.';
                    }
                    else
                    {
                        s = s + (char)val[i];
                    }
                }
            }
            return s;
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
            btnRead.Enabled = false;
            txtAsciiContent.Text = "";
            txtFinalStatus.Text = "";
            txtHexData.Text = "";
            lblCardAtr.Text = "";
            lblStatus.Text = SCARD.ReaderStatusToString(readerState);

            if (cardAtr != null)
            {
                lblCardAtr.Text = cardAtr.AsString(" ");
                channel = new SCardChannel(reader);
                if (!channel.Connect())
                {
                    lblStatus.Text = "Error, can't connect to the card";
                    return;
                }
                CAPDU capdu = new CAPDU(0xFF, 0xCA, 0x00, 0x00);    // Command sent to the reader
                RAPDU rapdu = channel.Transmit(capdu);              // Response sent from card
                if (rapdu.SW != 0x9000)                              // Something failed
                {
                    lblStatus.Text = "Get UID APDU failed!";
                    return;
                }
                btnRead.Enabled = true;
            }
        }
        
        private void cbReaders_SelectedIndexChanged_1(object sender, EventArgs e)
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

        private void Form1_FormClosed_1(object sender, FormClosedEventArgs e)
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

        private void btnRefresh_Click_1(object sender, EventArgs e)
        {
            refreshReaders();
        }
    }
}
