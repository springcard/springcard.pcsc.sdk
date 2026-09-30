/*
 * Created by SharpDevelop.
 * User: jerome.i
 * Date: 20/06/2013
 * Time: 14:36
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using SpringCardMemoryCard;
using Be.Windows.Forms;
using SpringCard.LibCs;

namespace MemoryCardTool
{
	/// <summary>
	/// Description of SectorControl.
	/// </summary>
	public partial class MemoryCardMifareClassicSectorControl : UserControl
	{
		private MemoryCardMifareClassic Card;
		private MemoryCardMifareClassic.Sector Sector;
		private int SectorNumber = 0;
		private ToolTip toolTip;
		
		public MemoryCardMifareClassicSectorControl(MemoryCardMifareClassic Card, MemoryCardMifareClassic.Sector Sector)
		{
			InitializeComponent();
			this.Card = Card;
			this.Sector = Sector;
			PrintData();
		}

		public MemoryCardMifareClassicSectorControl(MemoryCardMifareClassic Card, int sectorCounter)
		{
			SectorNumber = sectorCounter;
			InitializeComponent();
			this.Card = Card;
			this.Sector = null;
			lbSector.Text = sectorCounter.ToString();
			PrintData();
		}

		void PrintData()
		{
			if (this.Sector == null) {
				btChangeAccesConditions.Enabled = false;
				btChangeAccesConditions.Visible = false;
				HexBoxContent.Enabled = false;
				btWriteToCard.Enabled = false;
				btnUnlock.Location = new Point(650, 27);
				btnUnlock.Visible = true;
				btnUnlock.Enabled = true;
				return;
			} else {
				btnUnlock.Visible = false;
				btnUnlock.Enabled = false;
				btChangeAccesConditions.Enabled = true;
				btChangeAccesConditions.Visible = true;
			}
			btWriteToCard.Enabled = false;
			btWriteToCard.Visible = true;
			HexBoxContent._mifare_sector = true;
			lbSector.Text = Sector.Number.ToString();
			btTryDifferentKeys.Enabled = false;
			btTryDifferentKeys.Visible = false;
			HexBoxContent.Enabled = true;
			
			btChangeAccesConditions.Visible = true;
			
			toolTip = new ToolTip();
			toolTip.SetToolTip(btChangeAccesConditions, "Change keys and access conditions");
			toolTip.SetToolTip(btTryDifferentKeys, "Unlock sector with specific keys");
			toolTip.SetToolTip(btWriteToCard, "Write current modifications to the card");
			
			if (Sector.Blocks != null) {
				if (Sector.Blocks.Count > 10)
					HexBoxContent.Size = new Size(HexBoxContent.Size.Width, 228);
				
				HexBoxContent.ByteProvider = new DynamicByteProvider(Sector.GetBytes());
			} else {
				HexBoxContent.Enabled = false;
				btWriteToCard.Visible = false;
				btTryDifferentKeys.Enabled = true;
				btTryDifferentKeys.Visible = true;
				btChangeAccesConditions.Visible = false;
			}
		}
		
		void HexBoxContentKeyDown(object sender, KeyEventArgs e)
		{
			btWriteToCard.Enabled = true;
			if (Sector.Number == 0)
				toolTip.SetToolTip(btWriteToCard, "Write current modifications to the card - block 0 can't be changed");
		}
		
		/// <summary>
		/// Returns data entered by the user in the HexBox
		/// </summary>
		/// <returns></returns>
		private byte[] getDataFromHexbox()
		{
			byte[] b;
			DynamicByteProvider p;
			
			if (Sector == null) {
				/* TODO: on ne sait pas ecrire sans avoir lu au prealable */
				return null;
			}
			
			/* Get the bytes entered by the user */
			p = (DynamicByteProvider)HexBoxContent.ByteProvider;
			b = new byte[p.Length];
			
			for (int i = 0; i < p.Length; i++)
				b[i] = p.ReadByte(i);
			
			return b;
		}
		
		/// <summary>
		/// Write changes to card
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void BtWriteToCardClick(object sender, EventArgs e)
		{
			// @TODO: Interdire la modification du block 0
			byte[] b = getDataFromHexbox();
			if (b == null) {
				return;
			}
			
			MemoryCardMifareClassic.Sector newSector = new MemoryCardMifareClassic.Sector(Sector.Number);
			
			/* Fill the sector data */
			int firstBlockAddress = MemoryCardMifareClassic.SectorFirstBlockAddress(Sector.Number);
			int lastBlockAddress = MemoryCardMifareClassic.SectorTrailerAddress(Sector.Number) - 1;	// Excluded trailer def.
			int startingAddress = 0;
			
			for (int address = 0; address < 3; address++) {
				byte[] data = new byte[16];
				Array.Copy(b, startingAddress, data, 0, 16);
				startingAddress += 16;
				Logger.Trace("Content " + BinConvert.ToHex(data));
				newSector.SetBlock(address, data);
			}

			bool success = false;
			
			/* Write data */
			if (Card.WriteSector(newSector)) {
				success = true;
			} else {
				// Can't write, we are going to try with a key
				string message = String.Format("Please enter the key to write sector {0}:", SectorNumber);
				KeyForm keyForm = new KeyForm(message, KeyForm.KeyType.writing);
				
				if (keyForm.ShowDialog() != DialogResult.Cancel) {
					if (newSector.setWritingKeyFromString(keyForm.getUserKey())) {
						if (Card.WriteSector(newSector)) {
							success = true;
						} else {
							MessageBox.Show(this.Parent,
							                "There has been an error while writing the sector.",
							                "Writing failed",
							                MessageBoxButtons.OK,
							                MessageBoxIcon.Error);
						}
					} else {
						MessageBox.Show(this.Parent,
						                "The application is unable to use the specified key.",
						                "Internal error",
						                MessageBoxButtons.OK,
						                MessageBoxIcon.Error);
					}
				}
			}
			
			if (success) {
				MessageBox.Show(this.Parent,
				                "The sectors's content has been successfully updated !",
				                "Writing succeeded",
				                MessageBoxButtons.OK,
				                MessageBoxIcon.Information);
			}

			btWriteToCard.Enabled = false;
		}
		
		/// <summary>
		/// Change sector's access conditions
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void BtChangeAccesConditionsClick(object sender, EventArgs e)
		{
			if(this.Sector == null)
				return;
			
			bool proposeToTypeKeys = (!Sector.isSetKeyA() && !Sector.isSetKeyB()) || !Sector.isSetKeyA() || !Sector.isSetKeyB();
			if(proposeToTypeKeys) {
				DialogResult AccessResult = MessageBox.Show(this.Parent,
				                                            "Do you know the keys to unlock this sector?",
				                                            "Authentication required",
				                                            MessageBoxButtons.YesNo,
				                                            MessageBoxIcon.Question);
				if(AccessResult == DialogResult.Yes) {
					UnlockForm unlockForm = new UnlockForm(this.Sector);
					DialogResult unlockFormResult = unlockForm.ShowDialog();
					if (unlockFormResult != DialogResult.Cancel) {
						this.Sector = unlockForm.getSectorControl();	// Keys are set
						Card.ReadSectorTrailer(ref this.Sector);
					}
				}
			}
			
			MifareSectorTrailerDefinitionForm accessConditions = new MifareSectorTrailerDefinitionForm(this.Sector);
			DialogResult r = accessConditions.ShowDialog();
			if (r == DialogResult.Cancel)
				return;
			
			bool success = false;
			
			if (Card.WriteSectorTrailer(this.Sector)) {
				success = true;
			} else
			{
				DialogResult result = MessageBox.Show(this.Parent,
				                                      "Failed to change the access conditions.\nDo you know the key to write this sector?",
				                                      "Authentication required",
				                                      MessageBoxButtons.YesNo,
				                                      MessageBoxIcon.Question);
				if(result == DialogResult.Yes) {

					string message = String.Format("Please enter the key to write sector {0}:", Sector.Number);
					KeyForm keyForm = new KeyForm(message, KeyForm.KeyType.writing);
					
					DialogResult resdial = keyForm.ShowDialog();
					if (resdial != DialogResult.Cancel) {
						if(this.Sector.setWritingKeyFromString(keyForm.getUserKey())) {
							if(Card.WriteSectorTrailer(this.Sector)) {
								success = true;
							} else {
								MessageBox.Show(this.Parent,
								                "There has been an error while writing the sector's trailer.",
								                "Writing failed",
								                MessageBoxButtons.OK,
								                MessageBoxIcon.Error);
							}
						} else {
							MessageBox.Show(this.Parent,
							                "The application is unable to use the specified key.",
							                "Internal error",
							                MessageBoxButtons.OK,
							                MessageBoxIcon.Error);
						}
					}
				}
			}
			
			if (success) {
				MessageBox.Show(this.Parent,
				                "The sectors's trailer has been successfully updated !",
				                "Writing succeeded",
				                MessageBoxButtons.OK,
				                MessageBoxIcon.Information);
			}

			return;
		}

		/// <summary>
		/// Ask user for keys
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void BtnUnlockClick(object sender, EventArgs e)
		{
			string message = String.Format("Please enter the key to read sector {0}:", SectorNumber);
			KeyForm keyForm = new KeyForm(message, KeyForm.KeyType.reading);
			
			DialogResult r = keyForm.ShowDialog();
			if (r == DialogResult.Cancel)
				return;
			
			MemoryCardMifareClassic.Sector newSector = new MemoryCardMifareClassic.Sector(SectorNumber);

			if (!newSector.setReadingKeyFromString(keyForm.getUserKey()))
			{
				MessageBox.Show(this.Parent,
				                "The application is unable to use the specified key.",
				                "Internal error",
				                MessageBoxButtons.OK,
				                MessageBoxIcon.Error);
				return;
			}
			
			MemoryCardMifareClassic.Sector readedSector = Card.ReadSectorWithKey(newSector);
			
			if (readedSector == null) {				
				MessageBox.Show(this.Parent,
				                "There has been an error while reading the sector.",
				                "Reading failed",
				                MessageBoxButtons.OK,
				                MessageBoxIcon.Error);
				return;
			}
				
			this.Sector = readedSector;
			PrintData();
		}
	}
}
