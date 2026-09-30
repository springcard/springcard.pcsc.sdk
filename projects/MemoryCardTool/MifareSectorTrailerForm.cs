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
using SpringCard.LibCs;

namespace MemoryCardTool
{
	/// <summary>
	/// Description of Form1.
	/// </summary>
	public partial class MifareSectorTrailerForm : Form
	{
		MemoryCardMifareClassic Card;
		int SectorNumber;
		int BlockNumber;
		int[] lvOldItemsChecked = new int[4] {-1, -1, -1, -1};
		byte[] acValue = new byte[4] { 0x00, 0x00, 0x00, 0x00};
		byte[] readKeyA = null;
		byte[] readKeyB = null;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="sector"></param>
		public MifareSectorTrailerForm(MemoryCardMifareClassic Card, int SectorNumber)
		{
			InitializeComponent();
			this.Card = Card;
			this.SectorNumber = SectorNumber;
			this.BlockNumber = MemoryCardMifareClassic.SectorTrailerAddress(SectorNumber);

			lbTitle.Text = string.Format("Trailer of sector {0} (block {1})", SectorNumber, BlockNumber);

			loadPreviousKeys();
		}
		
		/// <summary>
		/// Load previous entered keys by user (A & B)
		/// </summary>
		private void loadPreviousKeys()
		{
			string[] lastKeyAs = Settings.LastKeysA();
			for (int i = 0; i < lastKeyAs.Length ; i++)
				cbNewKeyA.Items.Add(lastKeyAs[i]);

			if (cbNewKeyA.Items.Count > 0)
				cbNewKeyA.SelectedIndex = 0;

			string[] lastKeyBs = Settings.LastKeysB();
			for (int i = 0; i < lastKeyBs.Length ; i++)
				cbNewKeyB.Items.Add(lastKeyBs[i]);

			if (cbNewKeyB.Items.Count > 0)
				cbNewKeyB.SelectedIndex = 0;			
		}

		/// <summary>
		/// Set UI from sector's access bits
		/// </summary>
		private void showOldValues()
		{
			byte[] accessBits = sector.AccessBits;
			byte b6, b7, b8, c10, c20, c30, c11, c21, c31, c12, c22, c32, c13, c23, c33;
			string b0 = "", b1 = "", b2 = "", b3 = "";

			if (accessBits == null || accessBits.Length != 4)
				return;

			Logger.Trace("Access bits are {0}", BinConvert.ToHex(accessBits));

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

			/* Access bits for block 1	*/
			b1 += (c11 == 0x01) ? "1" : "0";
			b1 += (c21 == 0x01) ? "1" : "0";
			b1 += (c31 == 0x01) ? "1" : "0";

			/* Access bits for block 2	*/
			b2 += (c12 == 0x01) ? "1" : "0";
			b2 += (c22 == 0x01) ? "1" : "0";
			b2 += (c32 == 0x01) ? "1" : "0";

			/* Access bits for sector trailer	*/
			b3 += (c13 == 0x01) ? "1" : "0";
			b3 += (c23 == 0x01) ? "1" : "0";
			b3 += (c33 == 0x01) ? "1" : "0";

			Logger.Trace("Hence b0={0} b1={1} b2={2} b3={3}", b0, b1, b2, b3);

			checkListViewItemFromBinaryValue(b0, lvBlock0);
			checkListViewItemFromBinaryValue(b1, lvBlock1);
			checkListViewItemFromBinaryValue(b2, lvBlock2);
			checkListViewItemFromBinaryValue(b3, lvAccessConditions);
		}

		/// <summary>
		/// Check an item in a listview representing a block or an access condition
		/// </summary>
		/// <param name="binaryValue"></param>
		/// <param name="lv"></param>
		private void checkListViewItemFromBinaryValue(string binaryValue, ListView lv)
		{
			Logger.Debug("Showing value {0} on {1}", binaryValue, lv.Name);
			for(int i=0; i < lv.Items.Count; i++)
			{
				if (lv.Items[i].Text == binaryValue) {
					lv.Items[i].Checked = true;
					lv.Items[i].Selected = true;	// For a more visual "effect"
					return;
				}
			}
			Logger.Warning("Failed to show value {0} on {1}", binaryValue, lv.Name);
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
		private byte[] computeAccessBits()
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

			byte[] result = new byte[4];
			result[0] = (byte)b6;
			result[1] = (byte)b7;
			result[2] = (byte)b8;
			result[3] = 0;

			return result;
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
				acValue[index] = Convert.ToByte(lv.Items[currentItemIndex].Text, 2);
			}
			showAccessBits(computeAccessBits());
		}

		private void showAccessBits(byte[] accessBits)
        {
			txtNewAC.Text = BinConvert.ToHex(accessBits);
			showNewContent();
        }

		private void showNewContent()
        {
			txtNewContent.Text = cbNewKeyA.Text + txtNewAC.Text + cbNewKeyB.Text;
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
			showNewContent();
		}

		void CbKeyBKeyUp(object sender, KeyEventArgs e)
		{
			showNewContent();
		}
		
		void CbKeyASelectedIndexChanged(object sender, EventArgs e)
		{
			showNewContent();
		}

		void CbKeyBSelectedIndexChanged(object sender, EventArgs e)
		{
			showNewContent();
		}

		private void txtKeyA_KeyUp(object sender, KeyEventArgs e)
		{
			showNewContent();
		}

		private void txtKeyB_KeyUp(object sender, KeyEventArgs e)
		{
			showNewContent();
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

			if (!MemoryCardMifareClassic.Sector.isKeyValid(cbNewKeyA.Text))
			{
				MessageBox.Show(this, "Key A: please supply a valid 6-byte value, in hexadecimal (12 hex digits).", "Invalid entry", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				this.DialogResult = DialogResult.None;
				return;
			}

			if (!MemoryCardMifareClassic.Sector.isKeyValid(cbNewKeyB.Text))
			{
				MessageBox.Show(this, "Key B: please supply a valid 6-byte value, in hexadecimal (12 hex digits).", "Invalid entry", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				this.DialogResult = DialogResult.None;
				return;
			}

			AccessBits = computeAccessBits();
			if ((AccessBits == null) || (AccessBits.Length != 4))
			{
				MessageBox.Show(this, "Failed to compute the access bits.", "Internal error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				this.DialogResult = DialogResult.Cancel;
				return;
			}

			KeyA = (new CardBuffer(cbNewKeyA.Text)).GetBytes();
			KeyB = (new CardBuffer(cbNewKeyB.Text)).GetBytes();

			if (!sector.setKeyA(KeyA) || !sector.setKeyB(KeyB))
			{
				MessageBox.Show(this, "Failed to define the new keys.", "Internal error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				this.DialogResult = DialogResult.Cancel;
				return;
			}

			Logger.Trace("Building new sector trailer...");

			Logger.Trace("\tKeyA={0}", BinConvert.ToHex(KeyA));
			Logger.Trace("\tKeyB={0}", BinConvert.ToHex(KeyB));
			Logger.Trace("\tAccess bits={0}", BinConvert.ToHex(AccessBits));

			SectorTrailer = new byte[0];
			SectorTrailer = BinUtils.Concat(SectorTrailer, KeyA);
			SectorTrailer = BinUtils.Concat(SectorTrailer, AccessBits);
			SectorTrailer = BinUtils.Concat(SectorTrailer, KeyB);

			Logger.Trace("New sector trailer: {0}", BinConvert.ToHex(SectorTrailer));

			// Save keyA and keyB
			Settings.RememberKeyA(cbNewKeyA.Text);
			Settings.RememberKeyB(cbNewKeyB.Text);
			
			this.DialogResult = DialogResult.OK;
		}
		
		void MifareSectorTrailerDefinitionFormShown(object sender, EventArgs e)
		{
			btnRead_Click(sender, e);
		}

        private void btnRead_Click(object sender, EventArgs e)
        {
			showOldValues();
		}

        private void btnWrite_Click(object sender, EventArgs e)
        {

        }

        private void btnClose_Click(object sender, EventArgs e)
        {
			Close();
        }
    }
}
