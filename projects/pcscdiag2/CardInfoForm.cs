/*
 * Crée par SharpDevelop.
 * Utilisateur: johann
 * Date: 20/03/2012
 * Heure: 12:15
 * 
 * Pour changer ce modèle utiliser Outils | Options | Codage | Editer les en-têtes standards.
 */
using System;
using System.Drawing;
using System.Windows.Forms;
using SpringCard.PCSC;

namespace PcscDiag2
{
	/// <summary>
	/// Description of CardInfoForm.
	/// </summary>
	public partial class CardInfoForm : Form
	{
		string reader_name;
		string card_atr;
		
		public CardInfoForm(CardAtrList smartcardList, string ReaderName, string CardAtr) : base()
		{
			InitializeComponent();
			reader_name = ReaderName;
			card_atr = CardAtr;
			
			Text = "Card " + CardAtr;
			
			eAtr.Text = CardAtr;
			eDescription.Lines = smartcardList.Descriptions(CardAtr);
			eAnalysis.Lines = (new CardAtrParser(CardAtr)).TextDescription();
		}
		
		void BtnCloseClick(object sender, EventArgs e)
		{
			Close();
		}		
	}
}
