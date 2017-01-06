using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SpringCard.PCSC;

namespace writeMifareUltralight
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

        /// <summary>
        /// Split a string into chunks
        /// </summary>
        /// <param name="str"></param>
        /// <param name="maxChunkSize"></param>
        /// <returns></returns>
        static IEnumerable<string> ChunksUpto(string str, int maxChunkSize)
        {
            for (int i = 0; i < str.Length; i += maxChunkSize)
                yield return str.Substring(i, Math.Min(maxChunkSize, str.Length - i));
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

        private void btnWrite_Click(object sender, EventArgs e)
        {
            string dataToWrite = txtData.Text.Trim().PadRight(47) + (char) 0x00;
            int page = 4;
            bool success = true;
            txtDataSent.Text = "";

            foreach (var chunck in ChunksUpto(dataToWrite, 4))
            {
                txtDataSent.Text += chunck + System.Environment.NewLine;
                byte[] content = Encoding.ASCII.GetBytes(chunck.PadRight(4, ' '));
                byte[] header = new byte[] { 0xFF, 0xD6, 0x00, (byte) page, 0x04}; // Update Binary
                byte[] writeApdu = Combine(header, content);
                txtApduSent.Text += BitConverter.ToString(writeApdu).Replace('-', ' ') + System.Environment.NewLine;
                
                CAPDU capdu = new CAPDU(writeApdu);                          // Command sent to the reader
                RAPDU rapdu = channel.Transmit(capdu);                      // Response sent from card
                if(rapdu == null)
                {
                    txtFinalStatus.Text = "Problem while writing";
                    success = false;
                    break;
                }
                if (rapdu.SW != 0x9000)                                  // Something failed
                {
                    txtFinalStatus.Text = "Error:" + String.Format("{0:X}", rapdu.SW) + ": " + SCARD.CardStatusWordsToString(rapdu.SW);
                    success = false;
                    break;
                }
                page++;
            }

            if(success)
                txtFinalStatus.Text = "Write with success";
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
            btnWrite.Enabled = false;
            txtDataSent.Text = "";
            txtFinalStatus.Text = "";
            txtApduSent.Text = "";
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
                btnWrite.Enabled = true;
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            refreshReaders();
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

        /// <summary>
        /// Combine several bytes array into a single one
        /// </summary>
        /// <param name="arrays"></param>
        /// <returns></returns>
        private byte[] Combine(params byte[][] arrays)
        {
            byte[] rv = new byte[arrays.Sum(a => a.Length)];
            int offset = 0;
            foreach (byte[] array in arrays)
            {
                System.Buffer.BlockCopy(array, 0, rv, offset, array.Length);
                offset += array.Length;
            }
            return rv;
        }
    }
}
