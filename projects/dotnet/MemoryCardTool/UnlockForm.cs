/*
 * Created by SharpDevelop.
 * User: jerome.i
 * Date: 21/06/2013
 * Time: 16:08
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Drawing;
using System.Windows.Forms;
using SpringCard.PCSC;
using SpringCardMemoryCard;

namespace MemoryCardTool
{
	public partial class UnlockForm : Form
	{
		MemoryCardMifareClassic.Sector sectorControl;
		
		private void addKeys(string[] keys, ComboBox cb)
		{
			for (int i = 0; i < keys.Length ; i++)
				cb.Items.Add(keys[i]);		
		}
		
		private void selectLastKey(ComboBox cb)
		{
			if (cb.Items.Count > 0)
				cb.SelectedIndex = 0;
		}
		
		private void loadPreviousKeys()
		{
			addKeys(Settings.LastReadKeys(), cbKeyA);
			addKeys(Settings.LastWriteKeys(), cbKeyA);
			addKeys(Settings.LastReadKeys(), cbKeyB);
			addKeys(Settings.LastWriteKeys(), cbKeyB);
			selectLastKey(cbKeyA);		
			selectLastKey(cbKeyB);
		}
		
		public UnlockForm(MemoryCardMifareClassic.Sector sectorControl)
		{
			InitializeComponent();
			this.sectorControl = sectorControl;
			loadPreviousKeys();
		}
		
		void BtOkClick(object sender, EventArgs e)
		{
			if (cbKeyA.Text.Equals("") && cbKeyB.Text.Equals(""))
			{
				MessageBox.Show(this, "Both Keys can't be null", "Invalid entry", MessageBoxButtons.OK, MessageBoxIcon.Error);
				this.DialogResult = DialogResult.None;
				return;
			}
			
			if (!cbKeyA.Text.Equals(""))
			{
				if (!MemoryCardMifareClassic.Sector.isKeyValid(cbKeyA.Text, 6))
				{
					MessageBox.Show(this, "Key A: please enter a 6-byte key, in hexadecimal (12 hex digits).", "Invalid entry", MessageBoxButtons.OK, MessageBoxIcon.Warning);
					this.DialogResult = DialogResult.None;
					return;
				}
				sectorControl.setKeyA(cbKeyA.Text);
				Settings.RememberReadKeys(cbKeyA.Text);
				Settings.RememberWriteKeys(cbKeyA.Text);
			}
					
			
			if (!cbKeyB.Text.Equals(""))
			{
				if (!MemoryCardMifareClassic.Sector.isKeyValid(cbKeyB.Text, 6))
				{
					MessageBox.Show(this, "Key B: please enter a 6-byte key, in hexadecimal (12 hex digits).", "Invalid entry", MessageBoxButtons.OK, MessageBoxIcon.Warning);
					this.DialogResult = DialogResult.None;
					return;
				}
				sectorControl.setKeyB(cbKeyB.Text);
				Settings.RememberReadKeys(cbKeyB.Text);
				Settings.RememberWriteKeys(cbKeyB.Text);				
			}	
		}
		
		void BtCancelClick(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
		}
		
		public MemoryCardMifareClassic.Sector getSectorControl()
		{
			return this.sectorControl;
		}		
	}
}
