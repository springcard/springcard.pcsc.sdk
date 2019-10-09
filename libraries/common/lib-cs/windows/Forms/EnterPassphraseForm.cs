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
	/// Description of EnterPassphraseForm.
	/// </summary>
	public partial class EnterPassphraseForm : Form
	{
		public EnterPassphraseForm()
		{
			InitializeComponent();
			lbTitle.Text = T._("Encrypted content");
			lbSubTitle.Text = T._("This content is protected. Please enter the passphrase to decrypt it.");
			lbPassphrase.Text = T._("Passphrase:");
			cbShowPassphrase.Text = T._("Show passphrase");
			cbRememberPassphrase.Text = T._("Remember this passphrase until I close the application");
			ePassphrase.PasswordChar = '•';
			btnOK.Text = T._("OK");
			btnCancel.Text = T._("Cancel");
		}
		
		public static bool Display(out string passphrase, Form parent)
		{
			return Display(out passphrase, parent);
		}
		
		public static bool Display(out string passphrase, string title = null, string subtitle = null, Form parent = null)
		{
			bool dummy;
			return Display(out passphrase, out dummy, false, title, subtitle, parent);
		}
		
		public static bool Display(out string passphrase, out bool remember, bool rememberable = false, string title = null, string subtitle = null, Form parent = null)
		{			
			EnterPassphraseForm form;
			form = new EnterPassphraseForm();
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
			form.cbRememberPassphrase.Visible = rememberable;
			
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
				remember = rememberable && form.cbRememberPassphrase.Checked;
				return true;
			} else
			{			
				passphrase = null;
				remember = false;
				return false;
			}
		}
		
		void CbShowPassphraseCheckedChanged(object sender, EventArgs e)
		{
			if (cbShowPassphrase.Checked)
			{
				ePassphrase.PasswordChar = '\0';
			} else
			{
				ePassphrase.PasswordChar = '•';
			}
		}
		
		void BtnOKClick(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
			Close();	
		}		
	}
}
