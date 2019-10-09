/*
 * Created by SharpDevelop.
 * User: herve.t
 * Date: 19/01/2016
 * Time: 10:33
 *
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Windows.Forms;
using SpringCardMemoryCard;
using SpringCard.PCSC;

namespace MemoryCardTool
{
	/// <summary>
	/// Description of Form1.
	/// </summary>
	public partial class MifareSectorTrailerDefinitionForm : Form
	{
		private MemoryCardMifareClassic.Sector sector;
		private int[] lvOldItemsChecked = new int[4] {-1, -1, -1, -1};
		private byte[] acValue = new byte[4] { 0x00, 0x00, 0x00, 0x00};

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="sector"></param>
		public MifareSectorTrailerDefinitionForm(MemoryCardMifareClassic.Sector sector)
		{
			InitializeComponent();
			this.sector = sector;
			loadPreviousKeys();
			this.Text += " - Sector " + sector.Number;
			//setUiFromSectorTrailerDefinition();
		}
		
		/// <summary>
		/// Load previous entered keys by user (A & B)
		/// </summary>
		private void loadPreviousKeys()
		{
			string[] lastKeyAs = Settings.LastKeysA();
			for (int i = 0; i < lastKeyAs.Length ; i++)
				cbKeyA.Items.Add(lastKeyAs[i]);

			if (cbKeyA.Items.Count > 0)
				cbKeyA.SelectedIndex = 0;

			string[] lastKeyBs = Settings.LastKeysB();
			for (int i = 0; i < lastKeyBs.Length ; i++)
				cbKeyB.Items.Add(lastKeyBs[i]);

			if (cbKeyB.Items.Count > 0)
				cbKeyB.SelectedIndex = 0;			
		}

		/// <summary>
		/// Set UI from sector's access bits
		/// </summary>
		private void setUiFromSectorTrailerDefinition()
		{
			byte[] accessBits = sector.AccessBits;
			byte b6, b7, b8, c10, c20, c30, c11, c21, c31, c12, c22, c32, c13, c23, c33;
			string b0 = "", b1 = "", b2 = "", bst = "";

			if (accessBits == null || accessBits.Length != 4)
				return;

			b6 = accessBits[0];
			b7 = accessBits[1];
			b8 = accessBits[2];

			c10 = (byte) ((b7 & 0x10) >> 4);
			c20 = (byte) (b8 & 0x01);
			c30 = (byte) ((b8 & 0x10) >> 4);

			c11 = (byte) ((b7 & 0x20) >> 5);
			c21 = (byte) ((b8 & 0x02) >> 1);
			c31 = (byte) ((b8 & 0x20) >> 5);

			c12 = (byte) ((b7 & 0x40) >> 6);
			c22 = (byte) ((b8 & 0x04) >> 2);
			c32 = (byte) ((b8 & 0x40) >> 6);

			c13 = (byte) ((b7 & 0x80) >> 7);
			c23 = (byte) ((b8 & 0x08) >> 3);
			c33 = (byte) ((b8 & 0x80) >> 7);

			/* Access bits for block 0	*/
			b0 += (c10 == 0x01) ? "1" : "0";
			b0 += (c20 == 0x01) ? "1" : "0";
			b0 += (c30 == 0x01) ? "1" : "0";
			checkListViewItemFromBinaryValue(b0, lvBlock0);

			/* Access bits for block 1	*/
			b1 += (c11 == 0x01) ? "1" : "0";
			b1 += (c21 == 0x01) ? "1" : "0";
			b1 += (c31 == 0x01) ? "1" : "0";
			checkListViewItemFromBinaryValue(b1, lvBlock1);

			/* Access bits for block 2	*/
			b2 += (c12 == 0x01) ? "1" : "0";
			b2 += (c22 == 0x01) ? "1" : "0";
			b2 += (c32 == 0x01) ? "1" : "0";
			checkListViewItemFromBinaryValue(b2, lvBlock2);

			/* Access bits for sector trailer	*/
			bst += (c13 == 0x01) ? "1" : "0";
			bst += (c23 == 0x01) ? "1" : "0";
			bst += (c33 == 0x01) ? "1" : "0";
			checkListViewItemFromBinaryValue(bst, lvAccessConditions);
		}

		/// <summary>
		/// Check an item in a listview representing a block or an access condition
		/// </summary>
		/// <param name="binaryValue"></param>
		/// <param name="lv"></param>
		private void checkListViewItemFromBinaryValue(string binaryValue, ListView lv)
		{
			for(int i=0; i < lv.Items.Count; i++)
			{
				if(lv.Items[i].Tag.ToString().Trim().Equals(binaryValue.Trim())) {
					lv.Items[i].Checked = true;
					lv.Items[i].Selected = true;	// For a more visual "effect"
					break;
				}
			}
		}

		void BtnCancelClick(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.None;
			Close();
		}

		/// <summary>
		/// Converts a decimal value to its hexadecimal value BUT returns only the 2 right characters 
		/// </summary>
		/// <param name="d"></param>
		/// <returns></returns>
		private string decimalToHex(int d)
		{
			try {
				string val = "00" + d.ToString("X");
				return val.Substring(val.Length-2, 2);
			} catch (Exception) {
				return "";
			}
		}

		/// <summary>
		/// Compute the sector trailer according to the user's choices
		/// </summary>
		private byte[] computeSectorTrailer()
		{
			byte[] c1 = new byte[4] {0, 0, 0, 0};
			byte[] c2 = new byte[4] {0, 0, 0, 0};
			byte[] c3 = new byte[4] {0, 0, 0, 0};
			string s = "";
			for (byte i=0; i<4; i++)
			{
				byte ac = acValue[i];
				c1[i] = Convert.ToBoolean(ac & 0x04) ? (byte) 1 : (byte) 0;
				c2[i] = Convert.ToBoolean(ac & 0x02) ? (byte) 1 : (byte) 0;
				c3[i] = Convert.ToBoolean(ac & 0x01) ? (byte) 1 : (byte) 0;
				s = s + '(' + c1[i] + c2[i] + c3[i] + ')';
			}

			int b6 = 0;
			if (!Convert.ToBoolean(c1[0])) b6 |= 0x01;
			if (!Convert.ToBoolean(c1[1])) b6 |= 0x02;
			if (!Convert.ToBoolean(c1[2])) b6 |= 0x04;
			if (!Convert.ToBoolean(c1[3])) b6 |= 0x08;
			if (!Convert.ToBoolean(c2[0])) b6 |= 0x10;
			if (!Convert.ToBoolean(c2[1])) b6 |= 0x20;
			if (!Convert.ToBoolean(c2[2])) b6 |= 0x40;
			if (!Convert.ToBoolean(c2[3])) b6 |= 0x80;

			int b7 = 0;
			if (!Convert.ToBoolean(c3[0])) b7 |= 0x01;
			if (!Convert.ToBoolean(c3[1])) b7 |= 0x02;
			if (!Convert.ToBoolean(c3[2])) b7 |= 0x04;
			if (!Convert.ToBoolean(c3[3])) b7 |= 0x08;
			if (Convert.ToBoolean(c1[0])) b7 |= 0x10;
			if (Convert.ToBoolean(c1[1])) b7 |= 0x20;
			if (Convert.ToBoolean(c1[2])) b7 |= 0x40;
			if (Convert.ToBoolean(c1[3])) b7 |= 0x80;

			int b8 = 0;
			if (Convert.ToBoolean(c2[0])) b8 |= 0x01;
			if (Convert.ToBoolean(c2[1])) b8 |= 0x02;
			if (Convert.ToBoolean(c2[2])) b8 |= 0x04;
			if (Convert.ToBoolean(c2[3])) b8 |= 0x08;
			if (Convert.ToBoolean(c3[0])) b8 |= 0x10;
			if (Convert.ToBoolean(c3[1])) b8 |= 0x20;
			if (Convert.ToBoolean(c3[2])) b8 |= 0x40;
			if (Convert.ToBoolean(c3[3])) b8 |= 0x80;

			s = decimalToHex(b6) + decimalToHex(b7) + decimalToHex(b8);
			txtAccessBits.Text = s;
			s = cbKeyA.Text + s + "00" + cbKeyB.Text;
			txtSectorTrailer.Text = s;

			int b9 = (sector.AccessBits != null && sector.AccessBits.Length == 4) ? sector.AccessBits[3] : 0;

			try {
				byte[] ret = new byte[4] { (byte) b6, (byte) b7, (byte) b8, (byte) b9};
				return ret;
			} catch (Exception) {
				return null;
			}
		}

		private void manageClickAndValues(ListView lv, ItemCheckedEventArgs e, int index)
		{
			acValue[index] = 0;
			int currentItemIndex = e.Item.Index;
			if(lvOldItemsChecked[index] != -1) {	// Uncheck previous one
				lv.Items[lvOldItemsChecked[index]].Checked = false;	// Uncheck old selected element
			}
			if(lvOldItemsChecked[index] == currentItemIndex) {	// Nothing selected
				lvOldItemsChecked[index] = -1;
			} else {									// Something is selected
				lvOldItemsChecked[index] = currentItemIndex;
				acValue[index] = Convert.ToByte(lv.Items[currentItemIndex].Tag.ToString(), 2);
			}
			computeSectorTrailer();
		}

		void LvBlock0ItemChecked(object sender, ItemCheckedEventArgs e)
		{
			manageClickAndValues(lvBlock0, e, 0);
		}

		private void lvBlock1_ItemChecked(object sender, ItemCheckedEventArgs e)
		{
			manageClickAndValues(lvBlock1, e, 1);
		}

		private void lvBlock2_ItemChecked(object sender, ItemCheckedEventArgs e)
		{
			manageClickAndValues(lvBlock2, e, 2);
		}

		private void lvAccessConditions_ItemChecked(object sender, ItemCheckedEventArgs e)
		{
			manageClickAndValues(lvAccessConditions, e, 3);
		}

		void CbKeyAKeyUp(object sender, KeyEventArgs e)
		{
			computeSectorTrailer();
		}

		void CbKeyBKeyUp(object sender, KeyEventArgs e)
		{
			computeSectorTrailer();
		}
		
		void CbKeyASelectedIndexChanged(object sender, EventArgs e)
		{
			computeSectorTrailer();
		}

		void CbKeyBSelectedIndexChanged(object sender, EventArgs e)
		{
			computeSectorTrailer();
		}

		private void txtKeyA_KeyUp(object sender, KeyEventArgs e)
		{
			computeSectorTrailer();
		}

		private void txtKeyB_KeyUp(object sender, KeyEventArgs e)
		{
			computeSectorTrailer();
		}		

		/// <summary>
		/// Save sector
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void BtnOkClick(object sender, EventArgs e)
		{
			byte[] AccessBits;
			byte[] SectorTrailer;
			byte[] KeyA;
			byte[] KeyB;
			CardBuffer cardbufA;
			CardBuffer cardbufB;

			if (!MemoryCardMifareClassic.Sector.isKeyValid(cbKeyA.Text))
			{
				MessageBox.Show(this, "Key A: please supply a valid 6-byte value, in hexadecimal (12 hex digits).", "Invalid entry", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				this.DialogResult = DialogResult.None;
				return;
			}

			if (!MemoryCardMifareClassic.Sector.isKeyValid(cbKeyB.Text))
			{
				MessageBox.Show(this, "Key B: please supply a valid 6-byte value, in hexadecimal (12 hex digits).", "Invalid entry", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				this.DialogResult = DialogResult.None;
				return;
			}

			AccessBits = computeSectorTrailer();
			if ((AccessBits == null) || (AccessBits.Length != 4))
			{
				MessageBox.Show(this, "Failed to compute the access bits.", "Internal error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				this.DialogResult = DialogResult.Cancel;
				return;
			}

			SectorTrailer = new byte[16];

			cardbufA = new CardBuffer(cbKeyA.Text);
			cardbufB = new CardBuffer(cbKeyB.Text);
			KeyA = cardbufA.GetBytes();
			KeyB = cardbufB.GetBytes();

			Array.ConstrainedCopy(KeyA, 0, SectorTrailer, 0, KeyA.Length);
			Array.ConstrainedCopy(AccessBits, 0, SectorTrailer, KeyA.Length, AccessBits.Length);
			Array.ConstrainedCopy(KeyB, 0, SectorTrailer, KeyA.Length + AccessBits.Length, KeyB.Length);

			// Save keyA and keyB
			Settings.RememberKeyA(cbKeyA.Text);
			Settings.RememberKeyB(cbKeyB.Text);
			
			if(!sector.setKeyA(KeyA) || !sector.setKeyB(KeyB))
			{
				MessageBox.Show(this, "Failed to define the new keys.", "Internal error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				this.DialogResult = DialogResult.Cancel;
				return;
			}
			this.DialogResult = DialogResult.OK;
		}
		
		/// <summary>
		/// Returns the sector 
		/// </summary>
		/// <returns></returns>
		public MemoryCardMifareClassic.Sector getSector()
		{
			return sector;
		}
		void MifareSectorTrailerDefinitionFormShown(object sender, EventArgs e)
		{
			setUiFromSectorTrailerDefinition();	
		}
	}
}
