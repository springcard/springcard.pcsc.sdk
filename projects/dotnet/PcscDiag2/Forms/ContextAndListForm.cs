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
        public ContextAndListForm()
        {
            //
            // The InitializeComponent() call is required for Windows Forms designer support.
            //
            InitializeComponent();

            //
            // TODO: Add constructor code after the InitializeComponent() call.
            //
        }

        void BtnCancelClick(object sender, EventArgs e)
        {
            Close();
        }

        void BtnOKClick(object sender, EventArgs e)
        {
            if (rbUser.Checked) Settings.ContextScope = SCARD.SCOPE_USER;
            if (rbSystem.Checked) Settings.ContextScope = SCARD.SCOPE_SYSTEM;
            if (rbGroupAll.Checked) Settings.ListGroup = SCARD.ALL_READERS;
            if (rbGroupDefault.Checked) Settings.ListGroup = SCARD.DEFAULT_READERS;
            if (rbGroupSpecific.Checked && !eGroupSpecific.Text.Equals("")) Settings.ListGroup = eGroupSpecific.Text;
            Close();
        }

        void ContextAndListFormLoad(object sender, EventArgs e)
        {
            switch (Settings.ContextScope)
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

            if (Settings.ListGroup.Equals(SCARD.ALL_READERS))
            {
                rbGroupAll.Checked = true;
            }
            else
            if (Settings.ListGroup.Equals(SCARD.DEFAULT_READERS))
            {
                rbGroupDefault.Checked = true;
            }
            else
            {
                rbGroupSpecific.Checked = true;
                eGroupSpecific.Enabled = true;
                eGroupSpecific.Text = Settings.ListGroup;
            }
        }

        void RbGroupAllCheckedChanged(object sender, EventArgs e)
        {
            eGroupSpecific.Enabled = rbGroupSpecific.Checked;
        }
    }
}
