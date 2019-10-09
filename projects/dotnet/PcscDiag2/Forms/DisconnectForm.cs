/*
 * Crée par SharpDevelop.
 * Utilisateur: johann
 * Date: 26/03/2012
 * Heure: 09:21
 * 
 * Pour changer ce modèle utiliser Outils | Options | Codage | Editer les en-têtes standards.
 */
using SpringCard.PCSC;
using System;
using System.Windows.Forms;

namespace PcscDiag2
{
    /// <summary>
    /// Description of DisconnectForm.
    /// </summary>
    public partial class DisconnectForm : Form
    {
        public uint Disposition;

        public DisconnectForm()
        {
            InitializeComponent();
        }

        void BtnCancelClick(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        void BtnOKClick(object sender, EventArgs e)
        {
            if (rbLeave.Checked) Disposition = SCARD.LEAVE_CARD;
            if (rbReset.Checked) Disposition = SCARD.RESET_CARD;
            if (rbUnpower.Checked) Disposition = SCARD.UNPOWER_CARD;
            if (rbEject.Checked) Disposition = SCARD.EJECT_CARD;

            Settings.DefaultDisconnectDisposition = Disposition;

            DialogResult = DialogResult.OK;
            Close();
        }

        void DisconnectFormLoad(object sender, EventArgs e)
        {
            Disposition = Settings.DefaultDisconnectDisposition;

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
                case SCARD.EJECT_CARD:
                    rbEject.Checked = true;
                    break;
                default:
                    rbReset.Checked = true;
                    break;
            }
        }
    }
}
