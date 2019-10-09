/*
 * Crée par SharpDevelop.
 * Utilisateur: johann
 * Date: 20/03/2012
 * Heure: 12:15
 * 
 * Pour changer ce modèle utiliser Outils | Options | Codage | Editer les en-têtes standards.
 */
using SpringCard.PCSC.CardAnalysis;
using System;
using System.Windows.Forms;

namespace PcscDiag2
{
    /// <summary>
    /// Description of CardInfoForm.
    /// </summary>
    public partial class CardInfoForm : Form
    {
        string reader_name;
        string card_atr;

        public CardInfoForm(string ReaderName, string CardAtr) : base()
        {
            InitializeComponent();
            reader_name = ReaderName;
            card_atr = CardAtr;

            Text = "Card " + CardAtr;

            eAtr.Text = CardAtr;
            eAnalysis.Lines = (new CardAtrParser(CardAtr)).TextDescription();
            eDescription.Lines = CardAtrList.Descriptions(CardAtr);
            eTechnical.Lines = CardPixList.Descriptions(CardAtr);
        }

        void BtnCloseClick(object sender, EventArgs e)
        {
            Close();
        }
    }
}
