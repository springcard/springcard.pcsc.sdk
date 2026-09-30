/*
 * Crée par SharpDevelop.
 * Utilisateur: johann
 * Date: 13/04/2012
 * Heure: 10:04
 * 
 * Pour changer ce modèle utiliser Outils | Options | Codage | Editer les en-têtes standards.
 */
using SpringCard.PCSC;
using System;
using System.Windows.Forms;

namespace PcscDiag2
{
    /// <summary>
    /// Description of ContextAndListForm.
    /// </summary>
    public partial class ContextAndListForm : Form
    {
        Settings settings;

        public ContextAndListForm(Settings settings)
        {
            this.settings = settings;
            InitializeComponent();
        }

        void BtnCancelClick(object sender, EventArgs e)
        {
            Close();
        }

        void BtnOKClick(object sender, EventArgs e)
        {
            if (rbUser.Checked) settings.ContextScope = SCARD.SCOPE_USER;
            if (rbSystem.Checked) settings.ContextScope = SCARD.SCOPE_SYSTEM;
            if (rbGroupAll.Checked) settings.ListGroup = SCARD.ALL_READERS;
            if (rbGroupDefault.Checked) settings.ListGroup = SCARD.DEFAULT_READERS;
            if (rbGroupSpecific.Checked && !eGroupSpecific.Text.Equals("")) settings.ListGroup = eGroupSpecific.Text;
            Close();
        }

        void ContextAndListFormLoad(object sender, EventArgs e)
        {
            switch (settings.ContextScope)
            {
                case SCARD.SCOPE_USER:
                    rbUser.Checked = true;
                    break;
                case SCARD.SCOPE_SYSTEM:
                    rbSystem.Checked = true;
                    break;
                default:
                    break;
            }

            if (settings.ListGroup.Equals(SCARD.ALL_READERS))
            {
                rbGroupAll.Checked = true;
            }
            else
            if (settings.ListGroup.Equals(SCARD.DEFAULT_READERS))
            {
                rbGroupDefault.Checked = true;
            }
            else
            {
                rbGroupSpecific.Checked = true;
                eGroupSpecific.Enabled = true;
                eGroupSpecific.Text = settings.ListGroup;
            }
        }

        void RbGroupAllCheckedChanged(object sender, EventArgs e)
        {
            eGroupSpecific.Enabled = rbGroupSpecific.Checked;
        }
    }
}
