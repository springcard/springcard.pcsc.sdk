/*
 * Crée par SharpDevelop.
 * Utilisateur: johann
 * Date: 20/03/2012
 * Heure: 12:15
 * 
 * Pour changer ce modèle utiliser Outils | Options | Codage | Editer les en-têtes standards.
 */
using SpringCard.PCSC;
using System;
using System.Windows.Forms;

namespace PcscDiag2
{
    public partial class ReaderInfoForm : Form
    {
        string reader_name;

        public ReaderInfoForm(string ReaderName) : base()
        {
            InitializeComponent();
            reader_name = ReaderName;
            Text = ReaderName;
        }

        void ReaderInfoFormShown(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;

            SCardChannel channel = new SCardChannel(reader_name);

            string t = SCARD.GetReaderDeviceInstanceId(channel.hContext, reader_name);
            t = t.Replace("&", "&&");
            lbDevice.Text = t;

            channel.Protocol = SCARD.PROTOCOL_DIRECT();
            channel.ShareMode = SCARD.SHARE_DIRECT;

            if (!channel.Connect())
            {
                Cursor = Cursors.Default;
                MessageBox.Show("Failed to connect to the reader, information can't be retrieved.");
                return;
            }

            //t = channel.GetAttribString(SCARD.ATTR_DEVICE_FRIENDLY_NAME);
            //MessageBox.Show(t);
            //t = channel.GetAttribString(SCARD.ATTR_DEVICE_SYSTEM_NAME);
            //MessageBox.Show(t);
            //t = channel.GetAttribString(SCARD.ATTR_VENDOR_IFD_SERIAL_NO);
            //MessageBox.Show(t);
            //t = channel.GetAttribString(SCARD.ATTR_VENDOR_IFD_TYPE);
            //MessageBox.Show(t);
            //t = channel.GetAttribString(SCARD.ATTR_VENDOR_IFD_VERSION);
            //MessageBox.Show(t);
            //t = channel.GetAttribString(SCARD.ATTR_VENDOR_NAME);
            //MessageBox.Show(t);

            CardBuffer r;

            /* Vendor name */
            r = channel.Control(new CardBuffer("582001"));
            if ((r != null) && (r.Length >= 1) && (r.GetByte(0) == 0x00))
            {
                string s = new String(r.GetChars(1, -1));
                lvReaderInfo.Items[0].SubItems.Add(s);
            }

            /* Product name */
            r = channel.Control(new CardBuffer("582002"));
            if ((r != null) && (r.Length >= 1) && (r.GetByte(0) == 0x00))
            {
                string s = new String(r.GetChars(1, -1));
                lvReaderInfo.Items[1].SubItems.Add(s);
            }

            /* Serial number */
            r = channel.Control(new CardBuffer("582003"));
            if ((r != null) && (r.Length >= 1) && (r.GetByte(0) == 0x00))
            {
                string s = new String(r.GetChars(1, -1));
                lvReaderInfo.Items[2].SubItems.Add(s);
            }

            /* Device descriptor */
            r = channel.Control(new CardBuffer("582004"));
            if ((r != null) && (r.Length >= 1) && (r.GetByte(0) == 0x00))
            {
                string s = new String(r.GetChars(1, -1));
                if (s.Length == 8)
                    s = "VID_" + s.Substring(0, 4) + "&PID_" + s.Substring(4, 4);
                lvReaderInfo.Items[3].SubItems.Add(s);
            }

            /* Device version */
            r = channel.Control(new CardBuffer("582005"));
            if ((r != null) && (r.Length >= 1) && (r.GetByte(0) == 0x00))
            {
                string s = new String(r.GetChars(1, -1));
                lvReaderInfo.Items[4].SubItems.Add(s);
            }

            /* Micore PID */
            r = channel.Control(new CardBuffer("582010"));
            if ((r != null) && (r.Length >= 1) && (r.GetByte(0) == 0x00))
            {
                string s = new String(r.GetChars(1, -1));
                lvReaderInfo.Items[5].SubItems.Add(s);
            }

            /* Gemcore PID */
            r = channel.Control(new CardBuffer("582011"));
            if ((r != null) && (r.Length >= 1) && (r.GetByte(0) == 0x00))
            {
                string s = new String(r.GetChars(1, -1));
                lvReaderInfo.Items[6].SubItems.Add(s);
            }

            /* Slot name */
            r = channel.Control(new CardBuffer("5821"));
            if ((r != null) && (r.Length >= 1) && (r.GetByte(0) == 0x00))
            {
                string s = new String(r.GetChars(1, -1));
                lvReaderInfo.Items[7].SubItems.Add(s);
            }

            channel.DisconnectLeave();
            Cursor = Cursors.Default;
        }

        void BtnCloseClick(object sender, EventArgs e)
        {
            Close();
        }
    }
}
