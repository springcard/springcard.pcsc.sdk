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

namespace getCardUid
{
    public partial class Form1 : Form
    {
        private SCardReader reader = null;      // The reader's object
        private SCardChannel channel = null;    // A channel to the reader and card
        private Dictionary<string, string> cardsNames = new Dictionary<string, string>();
        private Dictionary<int, string> protocols = new Dictionary<int, string>();

        public Form1()
        {
            InitializeComponent();
        }

        private void loadProtocols()
        {
            protocols.Add(0, "No information given");
            protocols.Add(1, "ISO 14443 A, level 1");
            protocols.Add(2, "ISO 14443 A, level 2");
            protocols.Add(3, "ISO 14443 A, level 3 or 4 (and Mifare)");
            protocols.Add(5, "ISO 14443 B, level 1");
            protocols.Add(6, "ISO 14443 B, level 2");
            protocols.Add(7, "ISO 14443 B, level 3 or 4");
            protocols.Add(9, "ICODE 1");
            protocols.Add(11, "ISO 15693");
        }

        private void loadCardsNames()
        {
            cardsNames.Add("0001", "NXP Mifare Standard 1k");
            cardsNames.Add("0002", "NXP Mifare Standard 4k");
            cardsNames.Add("0003", "NXP Mifare UltraLight, Other Type 2 NFC Tags(NFC Forum) with a capacity <= 64 bytes");
            cardsNames.Add("0006", "ST Micro Electronics SR176");
            cardsNames.Add("0007", "ST Micro Electronics SRI4K, SRIX4K, SRIX512, SRI512, SRT512");
            cardsNames.Add("000A", "Atmel AT88SC0808CRF");
            cardsNames.Add("000B", "Atmel AT88SC1616CRF");
            cardsNames.Add("000C", "Atmel AT88SC3216CRF");
            cardsNames.Add("000D", "Atmel AT88SC6416CRF");
            cardsNames.Add("0012", "Texas Instruments TAG IT");
            cardsNames.Add("0013", "ST Micro Electronics LRI512");
            cardsNames.Add("0014", "NXP ICODE SLI");
            cardsNames.Add("0015", "NXP ICODE1");
            cardsNames.Add("0021", "ST Micro Electronics LRI64");
            cardsNames.Add("0024", "ST Micro Electronics LR12");
            cardsNames.Add("0025", "ST Micro Electronics LRI128");
            cardsNames.Add("0026", "NXP Mifare Mini");
            cardsNames.Add("002F", "Innovision Jewel");
            cardsNames.Add("0030", "Innovision Topaz (NFC Forum type 1 tag)");
            cardsNames.Add("0034", "Atmel AT88RF04C");
            cardsNames.Add("0035", "NXP ICODE SL2");
            cardsNames.Add("003A", "NXP Mifare UltraLight C. Other Type 2 NFC Tags(NFC Forum) with a capacity > 64 bytes");
            cardsNames.Add("FFA0", "Generic/unknown 14443-A card");
            cardsNames.Add("FFA1", "Kovio RF barcode");
            cardsNames.Add("FFB0", "Generic/unknown 14443-B card");
            cardsNames.Add("FFB1", "ASK CTS 256B");
            cardsNames.Add("FFB2", "ASK CTS 512B");
            cardsNames.Add("FFB3", "Pre-standard ST Micro Electronics SRI 4K");
            cardsNames.Add("FFB4", "Pre-standard ST Micro Electronics SRI X512");
            cardsNames.Add("FFB5", "Pre-standard ST Micro Electronics SRI 512");
            cardsNames.Add("FFB6", "Pre-standard ST Micro Electronics SRT 512");
            cardsNames.Add("FFB7", "Inside Contactless PICOTAG/PICOPASS");
            cardsNames.Add("FFB8", "Generic Atmel AT88SC / AT88RF card");
            cardsNames.Add("FFC0", "Calypso card using the Innovatron protoco");
            cardsNames.Add("FFD0", "Generic ISO 15693 from unknown manufacturer");
            cardsNames.Add("FFD1", "Generic ISO 15693 from EM Marin (or Legic)");
            cardsNames.Add("FFD2", "Generic ISO 15693 from ST Micro Electronics, block number on 8 bits");
            cardsNames.Add("FFD3", "Generic ISO 15693 from ST Micro Electronics, block number on 16");
            cardsNames.Add("FFFF", "Virtual card (test only)");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            loadProtocols();
            loadCardsNames();
            refreshReaders();
            lblProtocol.Text = "";
            lblStatus.Text = "";
            lblCardAtr.Text = "";
            lblCardUid.Text = "";
            lblCardType.Text = "";
        }

        private void refreshReaders()
        {
            cbReaders.Items.Clear();
            try
            {
                string[] readers = SCARD.Readers;
                foreach (string reader in readers)
                    cbReaders.Items.Add(reader);

                if(cbReaders.Items.Count > 0)
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

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            refreshReaders();
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

			lblCardUid.Text = "";
            lblCardAtr.Text = "";
            lblCardType.Text = "";
            lblProtocol.Text = "";
            lblStatus.Text = SCARD.ReaderStatusToString(readerState);

            if (cardAtr != null)
            {
                lblCardAtr.Text = cardAtr.AsString(" ");
                channel = new SCardChannel(reader);
                if(!channel.Connect())
                {
                    lblCardUid.Text = "Error, can't connect to the card";
                    return;
                }
                CAPDU capdu = new CAPDU(0xFF, 0xCA, 0x00, 0x00);    // Command sent to the reader
                RAPDU rapdu = channel.Transmit(capdu);              // Response sent from card
                if(rapdu.SW != 0x9000)                              // Something failed
                {
                    lblCardUid.Text = "Get UID APDU failed!";
                    return;
                }
                // Display card's UID formated as an hexadecimal string
                byte[] rapduB = rapdu.data.GetBytes();
                string hexadecimalResult = BitConverter.ToString(rapduB);
                lblCardUid.Text = hexadecimalResult.Replace("-", " ");

                // Card's type
                if(cardAtr.Length >= 19)
                {
                    // Card's protocol
                    int cardProdocol = cardAtr[12];
                    if (protocols.ContainsKey(cardProdocol))
                        lblProtocol.Text = protocols[cardProdocol];

                    if (cardAtr[4] == 0x80 && cardAtr[5] == 0x4F && cardAtr[6] == 0x0C && cardAtr[7] == 0xA0 && cardAtr[8] == 0x00 && cardAtr[9] == 0x00 && cardAtr[10] == 0x03 && cardAtr[11] == 0x06)
                    {
                        string pixSs = cardAtr[13].ToString("X2") + cardAtr[14].ToString("X2");
                        lblCardType.Text = "Wired-logic: ";
                        if (cardsNames.ContainsKey(pixSs))
                            lblCardType.Text += cardsNames[pixSs];
                    }
                    else
                    {
                        lblCardType.Text = "Unknow card type";
                    }
                }
                else
                {
                    lblCardType.Text = "Smartcard";
                }
            }                
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
