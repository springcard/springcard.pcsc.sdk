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
	public partial class MemoryCardControl : UserControl
	{
		MemoryCard Card;
		
		public delegate void DataChangeHandler();
		public event DataChangeHandler DataChanged;
		
		public MemoryCardControl(MemoryCard Card)
		{
			InitializeComponent();

			this.Card = Card;

			HexBoxContent._useOffsetInsteadOfPage = false;

			if (Card.Pages[0].Length > 16)
			{
				HexBoxContent.BytesPerLine = 16;
			} else
			{
				HexBoxContent.BytesPerLine = Card.Pages[0].Length;
			}
			
			int x = lbHex.Location.X + 16 + (HexBoxContent.BytesPerLine * 21);
			lbASCII.Location = new Point(x, lbASCII.Location.Y);
			
			/* The content	*/
			List<byte> content = new List<byte>();
			
			/* P1 and P2	*/
			List<byte> addresses = new List<byte>();
			
			for (int address=0; address<Card.LastAddressRead; address++)
			{				
				addresses.Add((byte) (address / 0x0100));
				addresses.Add((byte) (address % 0x0100));
				
				byte[] data = Card.GetData(address);				
				
				for (int i=0; i<data.Length; i++)
					content.Add(data[i]);
			}
			
			HexBoxContent.ByteProviderP1P2 = new DynamicByteProvider(addresses.ToArray());
			HexBoxContent.ByteProvider = new DynamicByteProvider(content.ToArray());
		}
		
		void HexBoxContentKeyDown(object sender, KeyEventArgs e)
		{
			DataChanged();
		}
		
		public virtual void WriteToCard()
		{			
			DynamicByteProvider dataProvider;			
			dataProvider = (DynamicByteProvider) HexBoxContent.ByteProvider;
			
			byte[] cardData = new byte[dataProvider.Length];
			for (int i=0; i<dataProvider.Length; i++)
				cardData[i] = dataProvider.ReadByte(i);
			
			if (Card.SetData(cardData))
			{
				if (Card.WriteUpdates())
				{
					MessageBox.Show(this.Parent,
					                "The card's content has been successfully updated !",
					                "Writing succeeded",
					                MessageBoxButtons.OK,
					                MessageBoxIcon.Information);
				}
				else
				{
					MessageBox.Show(this.Parent,
					                "There has been an error while writing the card.",
					                "Writing failed",
					                MessageBoxButtons.OK,
					                MessageBoxIcon.Error);
				}
			}
			else
			{
				/* Nothing to write */
				MessageBox.Show(this.Parent,
				                "There's nothing to write!",
				                "Writing canceled",
				               	MessageBoxButtons.OK,
					            MessageBoxIcon.Stop);
			}
		}
	}
}
