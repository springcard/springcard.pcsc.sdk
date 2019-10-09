/*
 * Crée par SharpDevelop.
 * Utilisateur: johann
 * Date: 19/03/2012
 * Heure: 14:16
 * 
 * Pour changer ce modèle utiliser Outils | Options | Codage | Editer les en-têtes standards.
 */
using Be.Windows.Forms;
using SpringCard.LibCs.Windows;
using SpringCard.PCSC;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace PcscDiag2
{
    /// <summary>
    /// Description of CardForm.
    /// </summary>
    public partial class CardForm : Form
    {
        const int StatusImageUnknown = 0;
        const int StatusImageError = 1;
        const int StatusImageUnavailable = 2;
        const int StatusImageAbsent = 3;
        const int StatusImagePresent = 4;
        const int StatusImageMute = 5;
        const int StatusImageExclusive = 6;
        const int StatusImageInUse = 7;

        int hist_apdu_idx = -1;
        int hist_ctrl_idx = -1;

        SCardChannel channel = null;
        string reader_name;

        public delegate void OnCloseCallback(string ReaderName);
        public OnCloseCallback on_close = null;

        public CardForm(string ReaderName, Image ReaderImage, OnCloseCallback OnClose)
            : base()
        {
            InitializeComponent();
            reader_name = ReaderName;
            on_close = OnClose;

            Text = ReaderName;
            Icon = ImageConvert.ImageToIcon(ReaderImage, 16, true);

            hexBoxCApdu.ByteProvider = new DynamicByteProvider(new List<Byte>());
            hexBoxCCtrl.ByteProvider = new DynamicByteProvider(new List<Byte>());
        }

        public string ReaderName
        {
            get
            {
                return reader_name;
            }
        }

        public void ShowSuccess()
        {
            eTransmitError.BackColor = BackColor;
            eTransmitError.Text = "";
            eTransmitErrorExplain.BackColor = BackColor;
            eTransmitErrorExplain.Text = "";
            eControlError.BackColor = BackColor;
            eControlError.Text = "";
            eControlErrorExplain.BackColor = BackColor;
            eControlErrorExplain.Text = "";

            UpdateButtons();
        }

        public void ShowError()
        {
            long error = channel.LastError;
            string error_explain = channel.LastErrorAsString;
            string error_hex;

            if ((error & 0x80000000) != 0)
                error_hex = String.Format("{0:X8}", error);
            else
                error_hex = String.Format("{0}", error);

            eTransmitError.Text = error_hex;
            eTransmitError.BackColor = eCardAtr.BackColor;
            eTransmitErrorExplain.Text = error_explain;
            eTransmitErrorExplain.BackColor = eCardAtr.BackColor;
            eControlError.Text = error_hex;
            eControlError.BackColor = eCardAtr.BackColor;
            eControlErrorExplain.Text = error_explain;
            eControlErrorExplain.BackColor = eCardAtr.BackColor;

            UpdateButtons();
        }

        void UpdateButtons()
        {
            bool connected = false;
            bool to_a_card = false;

            if ((channel != null) && (channel.Connected))
            {
                /* We are connected */
                connected = true;
                if ((channel.Protocol == SCARD.PROTOCOL_T0) || (channel.Protocol == SCARD.PROTOCOL_T1))
                {
                    /* We are connected to a card (and not directly to the reader) */
                    to_a_card = true;
                }
            }

            btnConnect.Enabled = !connected;
            btnDisconnect.Enabled = connected;
            btnReconnect.Enabled = connected && to_a_card;
            btnTransmit.Enabled = connected && to_a_card;
            btnControl.Enabled = connected;

            btnCApduPrev.Enabled = hist_apdu_idx < Settings.HistoryTransmit.Length() - 1;
            btnCApduNext.Enabled = hist_apdu_idx > 0;
            btnCCtrlPrev.Enabled = hist_ctrl_idx < Settings.HistoryControl.Length() - 1;
            btnCCtrlNext.Enabled = hist_ctrl_idx > 0;
        }

        public bool Connect(uint ShareMode, uint Protocol)
        {
            if (channel != null)
            {
                /* Drop existing connection */
                /* ------------------------ */
                if (channel.Connected)
                    channel.DisconnectReset();
                channel = null;
            }

            /* Instanciate a new connection */
            /* ---------------------------- */

            channel = new SCardChannel(ReaderName);
            channel.ShareMode = ShareMode;
            channel.Protocol = Protocol;

            if (ShareMode == SCARD.SHARE_DIRECT)
            {
                /* DIRECT mode opens Control page */
                /* ------------------------------ */

                tabPages.SelectedIndex = 1;
                hexBoxCCtrl.Focus();
                btnControl.Visible = true;

            }
            else
            {
                /* SHARED or EXCLUSIVE mode opens Transmit page */
                /* -------------------------------------------- */

                tabPages.SelectedIndex = 0;
                hexBoxCApdu.Focus();
                btnTransmit.Visible = true;

            }

            if (!channel.Connect())
            {
                ShowError();
                return false;
            }
            ShowSuccess();

            eCardAtr.Text = channel.CardAtr.AsString();
            eProtocol.Text = channel.ProtocolAsString;
            eMode.Text = channel.ShareModeAsString;

            return true;
        }

        /*
     * SCardReconnect
     * --------------
     */
        public bool Reconnect(uint ShareMode, uint Protocol, uint Disposition)
        {
            if ((channel == null) || (!channel.Connected))
                return false;

            channel.ShareMode = ShareMode;
            channel.Protocol = Protocol;
            if (!channel.Reconnect(Disposition))
            {
                ShowError();
                return false;
            }
            ShowSuccess();

            eCardAtr.Text = channel.CardAtr.AsString();
            eProtocol.Text = channel.ProtocolAsString;
            eMode.Text = channel.ShareModeAsString;

            return true;
        }

        /*
     * SCardDisconnect
     * ---------------
     */
        public void Disconnect(uint Disposition)
        {
            eCardAtr.Text = "";
            eProtocol.Text = "";
            eMode.Text = "";

            if ((channel != null) && !channel.Disconnect(Disposition))
            {
                ShowError();
            }
            else
            {
                ShowSuccess();
            }
            channel = null;
        }

        /* 
     * SCardTransmit
     * ------------
     */
        void BtnTransmitClick(object sender, EventArgs e)
        {
            RApduClear();

            DynamicByteProvider p;
            byte[] b;

            p = (DynamicByteProvider)hexBoxCApdu.ByteProvider;

            b = new byte[p.Length];

            for (int i = 0; i < p.Length; i++)
                b[i] = p.ReadByte(i);

            CAPDU capdu = new CAPDU(b);

            Settings.HistoryTransmit.Add(capdu.AsString());
            hist_apdu_idx = -1;

            RAPDU rapdu = channel.Transmit(capdu);

            if (rapdu == null)
            {
                ShowError();

            }
            else
            {
                ShowSuccess();

                b = rapdu.GetBytes();

                p = new DynamicByteProvider(b);

                hexBoxRApdu.ByteProvider = p;

                eStatusWord.Text = String.Format("{0:X2} {1:X2}", rapdu.SW1, rapdu.SW2);
                eStatusWordExplain.Text = SCARD.CardStatusWordsToString(rapdu.SW);

                hexBoxRApdu.BackColor = hexBoxCApdu.BackColor;
                eStatusWord.BackColor = eCardAtr.BackColor;
                eStatusWordExplain.BackColor = eCardAtr.BackColor;
            }
        }

        /*
     * SCardControl
     * ------------
     */
        void BtnControlClick(object sender, EventArgs e)
        {
            RCtrlClear();

            DynamicByteProvider p;
            byte[] b;

            p = (DynamicByteProvider)hexBoxCCtrl.ByteProvider;

            b = new byte[p.Length];

            for (int i = 0; i < p.Length; i++)
                b[i] = p.ReadByte(i);

            CardBuffer cctrl = new CardBuffer(b);

            Settings.HistoryControl.Add(cctrl.AsString());
            hist_ctrl_idx = -1;

            CardBuffer rctrl = channel.Control(cctrl);

            if (rctrl == null)
            {
                ShowError();

            }
            else
            {
                ShowSuccess();

                b = rctrl.GetBytes();

                p = new DynamicByteProvider(b);

                hexBoxRCtrl.ByteProvider = p;

                if (b.Length > 0)
                {
                    eResultByte.Text = String.Format("{0}", 0 - b[0]);

                    if (b[0] == 0)
                    {
                        eResultByteExplain.Text = "Success";
                    }
                    else
                    {
                        ushort sw = (ushort)(0x6F00 | b[0]);
                        eResultByteExplain.Text = SCARD.CardStatusWordsToString(sw);
                    }
                }

                hexBoxRCtrl.BackColor = hexBoxCApdu.BackColor;
                eResultByte.BackColor = eCardAtr.BackColor;
                eResultByteExplain.BackColor = eCardAtr.BackColor;
            }
        }


        void CardFormFormClosed(object sender, FormClosedEventArgs e)
        {
            Disconnect(SCARD.RESET_CARD);
            if (on_close != null)
                on_close(reader_name);
        }

        void CApduClear()
        {
            hexBoxCApdu.ByteProvider = new DynamicByteProvider(new List<Byte>());
        }

        void RApduClear()
        {
            hexBoxRApdu.ByteProvider = new DynamicByteProvider(new List<Byte>());
            hexBoxRApdu.BackColor = BackColor;
            eStatusWord.Text = "";
            eStatusWord.BackColor = BackColor;
            eStatusWordExplain.Text = "";
            eStatusWordExplain.BackColor = BackColor;
            eTransmitError.Text = "";
            eTransmitError.BackColor = BackColor;
            eTransmitErrorExplain.Text = "";
            eTransmitErrorExplain.BackColor = BackColor;
        }

        void CCtrlClear()
        {
            hexBoxCCtrl.ByteProvider = new DynamicByteProvider(new List<Byte>());
        }

        void RCtrlClear()
        {
            hexBoxRCtrl.ByteProvider = new DynamicByteProvider(new List<Byte>());
            hexBoxRCtrl.BackColor = BackColor;
            eResultByte.Text = "";
            eResultByte.BackColor = BackColor;
            eResultByteExplain.Text = "";
            eResultByteExplain.BackColor = BackColor;
            eControlError.Text = "";
            eControlError.BackColor = BackColor;
            eControlErrorExplain.Text = "";
            eControlErrorExplain.BackColor = BackColor;
        }

        void BtnCApduClearClick(object sender, EventArgs e)
        {
            hist_apdu_idx = 0;
            UpdateButtons();
            CApduClear();
            RApduClear();
            hexBoxCApdu.Focus();
        }

        void BtnCtrlClearClick(object sender, EventArgs e)
        {
            hist_ctrl_idx = 0;
            UpdateButtons();
            CCtrlClear();
            RCtrlClear();
            hexBoxCCtrl.Focus();
        }

        public void HistoryTransmit(int way)
        {
            CApduClear();
            RApduClear();

            int i = hist_apdu_idx;

            if (way > 0)
                i++;
            if (way < 0)
                i--;

            string s = Settings.HistoryTransmit.Get(i);

            if (!s.Equals(""))
            {
                hist_apdu_idx = i;

                byte[] b = (new CardBuffer(s)).GetBytes();

                hexBoxCApdu.ByteProvider = new DynamicByteProvider(b);
            }

            UpdateButtons();
        }

        public void HistoryControl(int way)
        {
            CCtrlClear();
            RCtrlClear();

            int i = hist_ctrl_idx;

            if (way > 0)
                i++;
            if (way < 0)
                i--;

            string s = Settings.HistoryControl.Get(i);

            if (!s.Equals(""))
            {
                hist_ctrl_idx = i;

                byte[] b = (new CardBuffer(s)).GetBytes();

                hexBoxCCtrl.ByteProvider = new DynamicByteProvider(b);
            }

            UpdateButtons();
        }

        void BtnCApduPrevClick(object sender, EventArgs e)
        {
            HistoryTransmit(1);
        }

        void BtnCApduNextClick(object sender, EventArgs e)
        {
            HistoryTransmit(-1);
        }

        void BtnCApduBookmarkClick(object sender, EventArgs e)
        {
            RApduClear();
        }

        void BtnCloseClick(object sender, EventArgs e)
        {
            Close();
        }

        void BtnCtrlPrevClick(object sender, EventArgs e)
        {
            HistoryControl(1);
        }

        void BtnCtrlNextClick(object sender, EventArgs e)
        {
            HistoryControl(-1);
        }

        void BtnCtrlBookmarkClick(object sender, EventArgs e)
        {
            RApduClear();
        }

        void TabPagesSelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabPages.SelectedIndex == 0)
            {
                btnControl.Visible = false;
                btnTransmit.Visible = true;
                btnTransmit.Enabled = ((channel != null) && (channel.Connected));
                hexBoxCApdu.Focus();
            }
            if (tabPages.SelectedIndex == 1)
            {
                btnTransmit.Visible = false;
                btnControl.Visible = true;
                btnControl.Enabled = ((channel != null) && (channel.Connected));
                hexBoxCCtrl.Focus();
            }
        }

        void BtnConnectClick(object sender, EventArgs e)
        {
            ConnectForm f = new ConnectForm();
            if (f.ShowDialog() == DialogResult.OK)
            {
                Connect(f.ShareMode, f.Protocol);
            }
        }

        void BtnReconnectClick(object sender, EventArgs e)
        {
            ReconnectForm f = new ReconnectForm();
            if (f.ShowDialog() == DialogResult.OK)
            {
                Reconnect(f.ShareMode, f.Protocol, f.Disposition);
            }
        }

        void BtnDisconnectClick(object sender, EventArgs e)
        {
            DisconnectForm f = new DisconnectForm();
            if (f.ShowDialog() == DialogResult.OK)
            {
                Disconnect(f.Disposition);
            }
        }

        void HexBoxCApduKeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                /* Enter key -> Click Transmit button */
                if (btnTransmit.Visible && btnTransmit.Enabled)
                {
                    BtnTransmitClick(sender, e);
                    e.Handled = true;
                }
            }
            else if (e.KeyChar == (char)Keys.Escape)
            {
                CApduClear();
                RApduClear();
                e.Handled = true;
            }
        }

        void HexBoxCCtrlKeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                /* Enter key -> Click Transmit button */
                if (btnControl.Visible && btnControl.Enabled)
                {
                    BtnControlClick(sender, e);
                    e.Handled = true;
                }
            }
            else if (e.KeyChar == (char)Keys.Escape)
            {
                CCtrlClear();
                RCtrlClear();
                e.Handled = true;
            }
        }

    }
}
