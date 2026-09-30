/**h* SpringCardApplication/DesfireFormatForm
 *
 * NAME
 *   SpringCard API for NFC Forum :: Desfire as type 4
 * 
 * COPYRIGHT
 *   Copyright (c) Pro Active SAS, 2012-2013
 *   See LICENSE.TXT for information
 *
 **/

using System;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using SpringCard.LibCs;
using SpringCard.PCSC;

namespace SpringCardApplication
{
	
/**c* SpringCardApplication/DesfireFormatForm
 *
 * NAME
 *   DesfireFormatForm
 * 
 * DESCRIPTION
 *   Enables to format a DESFire EV1 card into a type 4 Tag
 * 
 * 
 * USED BY
 *   NfcTagType4
 *
 **/
	public partial class DesfireFormatForm : Form
	{
		SCardChannel cardchannel;

		public DesfireFormatForm(SCardChannel cardchannel)
		{
			InitializeComponent();
			this.cardchannel = cardchannel;
		}
		
		void BtnCancelClick(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}
		
		void BtnFormatClick(object sender, EventArgs e)
		{
			byte[] MasterKey = null;

			if (cbUseCustomRootKey.Checked)
            {
				if (!BinConvert.TryHexToBytes(eCustomRootKey.Text, out MasterKey, 16, 16))
                {
					MessageBox.Show(this, "Please provide a valid AES key (16 bytes, i.e. 32 hexadecimal characters.", "Invalid key", MessageBoxButtons.OK, MessageBoxIcon.Warning);
					return;
                }
            }

			DesfireFormatClass df = new DesfireFormatClass(cardchannel);

			if (!df.Verify())
            {
				MessageBox.Show(this, "Please make sure the card is a Desfire EV1 (or later).", "The card is not supported", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			if (!df.Format(MasterKey))
			{
				MessageBox.Show(this, "An error has occured. Please try again or use a different card.", "Failed to format the Desfire card", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			DialogResult = DialogResult.OK;
			Close();
		}

		private void cbUseCustomRootKey_CheckedChanged(object sender, EventArgs e)
        {
			eCustomRootKey.Enabled = cbUseCustomRootKey.Checked;
        }
    }

}
