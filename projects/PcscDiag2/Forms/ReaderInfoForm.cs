/*
 * Crée par SharpDevelop.
 * Utilisateur: johann
 * Date: 20/03/2012
 * Heure: 12:15
 * 
 * Pour changer ce modèle utiliser Outils | Options | Codage | Editer les en-têtes standards.
 */
using SpringCard.LibCs;
using SpringCard.PCSC;
using SpringCard.PCSC.ReaderHelpers;
using System;
using System.Windows.Forms;
using System.Collections.Generic;

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

            if (SystemInfo.GetRuntimeSystem() == SystemInfo.RuntimeSystem.Windows)
            {
                string t = SCARD.GetReaderDeviceInstanceId(channel.hContext, reader_name);
                t = t.Replace("&", "&&");
                lbDevice.Text = t;
            }

            channel.Protocol = SCARD.PROTOCOL_DIRECT();
            channel.ShareMode = SCARD.SHARE_DIRECT;

            if (!channel.Connect())
            {
                Cursor = Cursors.Default;
                MessageBox.Show(this, "Failed to connect to the reader, information can't be retrieved.");
                return;
            }

            SpringCardReader readerHelper = new SpringCardReader(channel);

            if (!readerHelper.ReadData())
            {
                channel.DisconnectLeave();
                Cursor = Cursors.Default;
                MessageBox.Show(this, "Failed to read any information from the reader.");
                return;
            }

            channel.DisconnectLeave();

            foreach (KeyValuePair<string, string> entry in readerHelper.Data)
            {
                lvReaderInfo.Items.Add(new ListViewItem(new string[] { entry.Key, entry.Value }));
            }

            Cursor = Cursors.Default;

        }

        void BtnCloseClick(object sender, EventArgs e)
        {
            Close();
        }
    }
}
