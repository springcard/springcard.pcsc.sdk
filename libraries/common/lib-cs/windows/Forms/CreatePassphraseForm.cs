/*
 * Created by SharpDevelop.
 * User: johann
 * Date: 30/11/2017
 * Time: 09:04
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Drawing;
using System.Windows.Forms;
using SpringCard.LibCs;

namespace SpringCard.LibCs.Windows
{
	/// <summary>
	/// Description of CreatePassphraseForm.
	/// </summary>
	public partial class CreatePassphraseForm : Form
	{
		public CreatePassphraseForm()
		{
			InitializeComponent();
			lbTitle.Text = T._("Encrypt content");
			lbSubTitle.Text = T._("This content will be protected by a passphrase. It is important that you keep this passphrase secret, and do not forget it.");
			lbPassphrase.Text = T._("Passphrase:");
			ePassphrase.PasswordChar = '•';
			eConfirmation.PasswordChar = '•';
			btnOK.Text = T._("OK");
			btnCancel.Text = T._("Cancel");
		}
		
		public static bool Display(out string passphrase, Form parent)
		{
			return Display(out passphrase, parent);
		}
		
		public static bool Display(out string passphrase, string title = null, string subtitle = null, Form parent = null)
		{			
			CreatePassphraseForm form;
			form = new CreatePassphraseForm();
			DialogResult result;

			if (parent != null)
			{
				form.StartPosition = FormStartPosition.CenterParent;
			} else
			{
				form.StartPosition = FormStartPosition.CenterScreen;
			}
			
			if (title != null)
				form.lbTitle.Text = title;
			if (subtitle != null)
				form.lbSubTitle.Text = subtitle;			
			
			if (parent != null)
			{
				result = form.ShowDialog(parent);
			} else
			{
				result = form.ShowDialog();
			}
			
			if (result == DialogResult.OK)
			{
				passphrase = form.ePassphrase.Text;
				return true;
			} else
			{			
				passphrase = null;
				return false;
			}
		}
		
		void EPassphraseKeyUp(object sender, KeyEventArgs e)
		{
			btnOK.Enabled = (ePassphrase.Text == eConfirmation.Text);
		}
		
		void EConfirmationKeyUp(object sender, KeyEventArgs e)
		{
			btnOK.Enabled = (ePassphrase.Text == eConfirmation.Text);
		}
		
		void BtnOKClick(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
			Close();
		}
	}
}
