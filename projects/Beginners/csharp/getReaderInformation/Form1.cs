using System;
using System.Windows.Forms;
using SpringCard.PCSC;

namespace getReaderInformation
{
    public partial class Form1 : Form
    {
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

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
        }

        private void cbReaders_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnRead.Enabled = true;
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            refreshReaders();
        }

        private void btnRead_Click(object sender, EventArgs e)
        {
            txtInfoReader.Text = "";
            if (cbReaders.Items.Count == 0)
                return;

            if (cbReaders.SelectedIndex == -1)
                return;

            string readerName = this.cbReaders.GetItemText(cbReaders.SelectedItem);
            if (readerName.Trim() == "")
                return;

            SCardChannel channel = new SCardChannel(readerName);
            channel.Protocol = SCARD.PROTOCOL_DIRECT();
            channel.ShareMode = SCARD.SHARE_DIRECT;
            if (!channel.Connect())
                return;
            CardBuffer r;

            // If you want to make the reader beep and change the LEDs color
            //"FFF00000031C0260", // Beep
            //"FFF00000041E967D96",   // <= Pink ledsLEDS - To come back to normal LEDs => FFF00000011E
            string[] apdus = new string[] {
                        "582001",       // Vendor's Name
                        "582002",       // Product's name
                        "582003",       // Product's Serial number
                        "582004",       // USB vendor ID and product ID
                        "582005",       // Product's version
                        "582010",       // NXP MfRCxxx product code
                        "582011",       // Gemalto Gemcore product name and version
                        "5821",         // Name of the current slot
                        "582100",       // Name of slot 0
                        "582101",       // Name of slot 1
                    };

            int line = 0;
            foreach (string apdu in apdus)
            {
                r = channel.Control(new CardBuffer(apdu));
                if ((r != null) && (r.Length >= 1) && (r.GetByte(0) == 0x00))
                {
                    txtInfoReader.Text += new String(r.GetChars(1, -1)) + System.Environment.NewLine;
                }
                line++;
            }
            channel.DisconnectLeave();
        }
    }
}


