/**h* SpringCardApplication/TyoeSelectButton
 *
 * NAME
 *   SpringCard API for NFC Forum :: Type Select Button class
 * 
 * COPYRIGHT
 *   Copyright (c) Pro Active SAS, 2012
 *   See LICENSE.TXT for information
 *
 **/
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace SpringCardApplication
{
	
	/**c* SpringCardApplication/TypeSelectButton
	 *
	 * NAME
	 *   DesfireFormatForm
	 * 
	 * DESCRIPTION
	 *   The button that enables to select the NDEF to encode on the tag
	 * 
	 *
	 **/
	public partial class TypeSelectButton : UserControl
	{
		private bool _selected;

		public System.EventHandler OnSelected = null;
		public System.EventHandler OnDeselected = null;

		public TypeSelectButton(string text) : base()
		{
			InitializeComponent();
			lbText.Text = text;
			SetSelected(false);
		}
		
		public void SetSelected(bool selected)
		{
			_selected = selected;
			if (_selected)
			{
				this.BackColor = System.Drawing.Color.Brown;
				this.Cursor = Cursors.Arrow;
				lbText.Font = new Font(this.Font, FontStyle.Bold);
			} else
			{
				this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(174)))), ((int)(((byte)(141)))), ((int)(((byte)(128)))));
				this.Cursor = Cursors.Hand;
				lbText.Font = new Font(this.Font, FontStyle.Regular);
			}
		}
		
		void LbTextMouseEnter(object sender, System.EventArgs e)
		{
			if (!_selected)
				lbText.Font = new Font(this.Font, FontStyle.Underline);
		}
		
		void LbTextMouseLeave(object sender, EventArgs e)
		{
			if (!_selected)
				lbText.Font = new Font(this.Font, FontStyle.Regular);
		}
		
		void LbTextClick(object sender, EventArgs e)
		{
			if (!_selected)
			{
				SetSelected(true);
				if (OnSelected != null)
					OnSelected(this, null);
			}
		}
	}
}
