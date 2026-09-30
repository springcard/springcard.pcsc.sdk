/*
 * Created by SharpDevelop.
 * User: jerome.i
 * Date: 20/06/2013
 * Time: 14:30
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using SpringCardMemoryCard;
using SpringCard.LibCs;
using System.Collections.Generic;

namespace MemoryCardTool
{
	/// <summary>
	/// Description of MemoryCardWithSectorsControl.
	/// </summary>
	public partial class MemoryCardMifareClassicControl : UserControl
	{
		public delegate void DataChangeHandler();
		public event DataChangeHandler DataChanged;
		
		public MemoryCardMifareClassicControl(MemoryCardMifareClassic card)
		{
			InitializeComponent();
			MemoryCardMifareClassicSectorControl sector_control;
		
			foreach ( KeyValuePair<int, MemoryCardMifareClassic.Sector> pair in card.Sectors)
			{
				Logger.Trace("Display sector " + pair.Key);
				if(pair.Value == null) 
					sector_control = new MemoryCardMifareClassicSectorControl(card, pair.Key);
				else
					sector_control = new MemoryCardMifareClassicSectorControl(card, pair.Value);
				sector_control.Dock = DockStyle.Top;
				this.Controls.Add(sector_control);
				sector_control.BringToFront();
			}

		}
		
		public virtual void WriteToCard()
		{
			// TODO
		}
	}
}
