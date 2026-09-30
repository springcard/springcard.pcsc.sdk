/*
 * Created by SharpDevelop.
 * User: jerome.i
 * Date: 19/06/2013
 * Time: 11:48
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using Be.Windows.Forms;
using SpringCardMemoryCard;

namespace MemoryCardTool
{
	/// <summary>
	/// Description of MemoryCardWithoutSectorsControl.
	/// </summary>
	public partial class MemoryCardWithoutSectorsControl : UserControl
	{
		MemoryCardWithoutSectors card;
		
		public MemoryCardWithoutSectorsControl(MemoryCardWithoutSectors _card)
		{

			InitializeComponent();
			btWriteToCard.Enabled = false;
			
			card = _card;

			HexBoxContent._useOffsetInsteadOfPage = false;

			if (card.Pages[0].Length > 16)
			{
				HexBoxContent.BytesPerLine = 16;
			} else
			{
				HexBoxContent.BytesPerLine = card.Pages[0].Length;
			}
			lbASCII.Location = new Point(lbHex.Location.X + 16 + (HexBoxContent.BytesPerLine * 24), lbASCII.Location.Y);
			
			/* The content	*/
			List<byte> content = new List<byte>();
			for (int i=0; i<card.Pages.Count; i++)
				for (int j=0; j<card.Pages[i].Length; j++)
					content.Add(card.Pages[i][j]);
			
			byte[] content_bytes = content.ToArray();
			
			DynamicByteProvider p;
			p = new DynamicByteProvider(content_bytes);
			HexBoxContent.ByteProvider = p;
			
			/* P1 and P2	*/
			List<byte> address = new List<byte>();

			for (int i=0; i<card.P1P2.Count; i++)
				for (int j=0; j<card.P1P2[i].Length; j++)
					address.Add(card.P1P2[i][j]);

			p = new DynamicByteProvider(address.ToArray());
			HexBoxContent.ByteProviderP1P2 = p;

		}
		
		void HexBoxContentKeyDown(object sender, KeyEventArgs e)
		{
			btWriteToCard.Enabled = true;
		}
		
		void BtWriteToCardClick(object sender, EventArgs e)
		{
			DynamicByteProvider p;
			byte[] b;
			List<byte[]> new_pages;
			
			/* First: get the bytes	*/
			p = (DynamicByteProvider) HexBoxContent.ByteProvider;
			
			b = new byte[p.Length];
			
			for (int i=0; i<p.Length; i++)
				b[i] = p.ReadByte(i);
			
			/* Then: re-create the pages of the new modified card	*/
			new_pages = new List<byte[]>();
			
			byte[] tmp;
			
			for(int i = 0; i<b.Length; i+=HexBoxContent.BytesPerLine)
			{
				tmp = new byte[HexBoxContent.BytesPerLine];
				if ((i+HexBoxContent.BytesPerLine) <= b.Length)
				{
					Array.ConstrainedCopy(b, i, tmp, 0, HexBoxContent.BytesPerLine);
					new_pages.Add(tmp);
				} else
				{
					MessageBox.Show("The number of bytes in the table doesn't correspond with the capacity of the card.", "Error: wrong number of bytes", MessageBoxButtons.OK, MessageBoxIcon.Error);
					btWriteToCard.Enabled = false;
					return;
				}
				
			}

			
			/* Check number of pages and number of bytes per page	*/
			if (new_pages.Count >	0)
			{
				if ((new_pages[0].Length != card.Pages[0].Length) || (new_pages.Count != card.Pages.Count))
				{
					MessageBox.Show("The number of bytes in the table doesn't correspond with the capacity of the card.", "Error: wrong number of bytes", MessageBoxButtons.OK, MessageBoxIcon.Error);
					btWriteToCard.Enabled = false;
					return;
				}
				
			}
			
			
			card.SetNewPages(new_pages);

			if (!card.WriteDeltaPages())
			{
				MessageBox.Show("The writing has failed", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			} else
			{
				MessageBox.Show("The card's content has been successfully updated !", "Writing OK", MessageBoxButtons.OK);
			}
			
			btWriteToCard.Enabled = false;
		}

	}
}
