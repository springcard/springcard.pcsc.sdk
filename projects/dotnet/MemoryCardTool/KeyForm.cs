/*
 * Created by SharpDevelop.
 * User: herve.t
 * Date: 29/01/2016
 * Time: 10:23
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Windows.Forms;
using SpringCardMemoryCard;

namespace MemoryCardTool
{
	/// <summary>
	/// Description of KeyForm.
	/// </summary>
	public partial class KeyForm : Form
	{
		public enum KeyType {reading, writing};
		private KeyType currentKeyType = KeyType.reading;
		
		public string userKey
		{
			get; set;
		}
		
		public KeyForm(string textForLabel, KeyType keytype)
		{
			InitializeComponent();
			currentKeyType = keytype;
			lblWhat.Text = textForLabel;			
			loadPreviousKeys();			
			cbKey.Focus();
		}
		
		public string getUserKey()
		{
			return userKey;
		}
		
		private void loadPreviousKeys()
		{
			string[] lastKeys = null;
			if(currentKeyType == KeyType.reading) {
				lastKeys = Settings.LastReadKeys();
			} else {
				lastKeys = Settings.LastWriteKeys();
			}
			for (int i = 0; i < lastKeys.Length ; i++)
				cbKey.Items.Add(lastKeys[i]);
			if (cbKey.Items.Count > 0)
				cbKey.SelectedIndex = 0;			
		}
		
		private void saveKey()
		{
			if(currentKeyType == KeyType.reading) {
				Settings.RememberReadKeys(cbKey.Text);
			} else {
				Settings.RememberWriteKeys(cbKey.Text);
			}			
		}
		
		void BtnOkClick(object sender, EventArgs e)
		{
			if (!MemoryCardMifareClassic.Sector.isKeyValid(cbKey.Text))
			{
				MessageBox.Show(this, "Please enter a 6-byte key, in hexadecimal (12 hex digits).", "Invalid entry", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				this.DialogResult = DialogResult.None;
				return;
			}
			saveKey();
			userKey = cbKey.Text.Trim();
			DialogResult = DialogResult.OK;
			
		}
		
		void BtnCancelClick(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
		}
	}
}
