/*
 * Crée par SharpDevelop.
 * Utilisateur: johann
 * Date: 26/03/2012
 * Heure: 09:32
 * 
 * Pour changer ce modèle utiliser Outils | Options | Codage | Editer les en-têtes standards.
 */
using SpringCard.PCSC;
using System;
using System.Windows.Forms;

namespace PcscDiag2
{
    public partial class ReconnectForm : Form
    {
        public uint ShareMode;
        public uint Protocol;
        public uint Disposition;

        public ReconnectForm()
        {
            InitializeComponent();
        }

        void ReconnectFormLoad(object sender, EventArgs e)
        {
            ShareMode = Settings.DefaultReconnectShare;
            Protocol = Settings.DefaultReconnectProtocol;
            Disposition = Settings.DefaultReconnectDisposition;

            switch (ShareMode)
            {
                case SCARD.SHARE_SHARED:
                    rbShared.Checked = true;
                    break;
                case SCARD.SHARE_EXCLUSIVE:
                    rbExclusive.Checked = true;
                    break;
                default:
                    rbExclusive.Checked = true;
                    break;
            }

            switch (Protocol)
            {
                case SCARD.PROTOCOL_T0:
                    rbT0.Checked = true;
                    break;
                case SCARD.PROTOCOL_T1:
                    rbT1.Checked = true;
                    break;
                case SCARD.PROTOCOL_RAW:
                    rbRaw.Checked = true;
                    break;
                case (SCARD.PROTOCOL_T0 | SCARD.PROTOCOL_T1):
                default:
                    rbT0orT1.Checked = true;
                    break;
            }

            switch (Disposition)
            {
                case SCARD.LEAVE_CARD:
                    rbLeave.Checked = true;
                    break;
                case SCARD.RESET_CARD:
                    rbReset.Checked = true;
                    break;
                case SCARD.UNPOWER_CARD:
                    rbUnpower.Checked = true;
                    break;
                default:
                    rbReset.Checked = true;
                    break;
            }
        }

        void BtnOKClick(object sender, EventArgs e)
        {
            if (rbShared.Checked) ShareMode = SCARD.SHARE_SHARED;
            if (rbExclusive.Checked) ShareMode = SCARD.SHARE_EXCLUSIVE;

            if (rbT0.Checked) Protocol = SCARD.PROTOCOL_T0;
            if (rbT1.Checked) Protocol = SCARD.PROTOCOL_T1;
            if (rbT0orT1.Checked) Protocol = (SCARD.PROTOCOL_T0 | SCARD.PROTOCOL_T1);
            if (rbRaw.Checked) Protocol = SCARD.PROTOCOL_RAW;

            if (rbLeave.Checked) Disposition = SCARD.LEAVE_CARD;
            if (rbReset.Checked) Disposition = SCARD.RESET_CARD;
            if (rbUnpower.Checked) Disposition = SCARD.UNPOWER_CARD;

            Settings.DefaultReconnectShare = ShareMode;
            Settings.DefaultReconnectProtocol = Protocol;
            Settings.DefaultReconnectDisposition = Disposition;

            DialogResult = DialogResult.OK;
            Close();
        }

        void BtnCancelClick(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
