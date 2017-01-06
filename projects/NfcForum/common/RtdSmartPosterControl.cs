/**h* SpringCardApplication/RtdMediaControl
 *
 * NAME
 *   SpringCard API for NFC Forum :: RTD Smart Poster Control class
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
using SpringCard.NFC;

namespace SpringCardApplication
{

	/**c* SpringCardApplication/RtdSmartPosterControl
	 *
	 * NAME
	 *   RtdSmartPosterControl
	 * 
	 * DESCRIPTION
	 *   Prints the content of an RTD Smart Poster NDEF
	 *
	 * SYNOPSIS
	 *   RtdSmartPosterControl control = new RtdSmartPosterControl()
	 * 
	 * DERIVED FROM
	 *   RtdControl
	 * 
	 **/
	public partial class RtdSmartPosterControl : RtdControl
	{
		public RtdSmartPosterControl()
		{
			InitializeComponent();
			
			cbMime.Items.Clear();
			string[] s = GetMimeTypes();
			for (int i=0; i<s.Length; i++)
				cbMime.Items.Add(s[i]);
		}
		
		public override void SetEditable(bool yes)
		{
			nfcRtdUriControl1.SetEditable(yes);
			nfcRtdTextControl1.SetEditable(yes);
			foreach (Control c in panel1.Controls)
			{
				if (c is TextBox)
					(c as TextBox).ReadOnly = !yes;
				if (c is ComboBox)
					(c as ComboBox).Enabled = yes;
			}
		}

		public override void ClearContent()
		{
			nfcRtdUriControl1.ClearContent();
			nfcRtdTextControl1.ClearContent();
			foreach (Control c in panel1.Controls)
			{
				if ((c is TextBox) || (c is ComboBox))
					c.Text = "";
			}
		}
		
		public override bool ValidateUserContent()
		{
			if (!nfcRtdUriControl1.ValidateUserContent())
				return false;
			return true;
		}
		
		/**m* SpringCardApplication/RtdSmartPoster.SetContent
		 *
		 * SYNOPSIS
		 *   public void SetContent(RtdSmartPoster smartPoster)
		 * 
		 * DESCRIPTION
		 * 	 Only called by the "public override void SetContent(Ndef ndef)" method, if the ndef is an RtdSmartPoster object.
		 *   It prints on the form the content of the RtdSmartPoster object passed as a parameter.
		 *
		 **/
		public void SetContent(RtdSmartPoster smartPoster)
		{
			ClearContent();
			
			if (smartPoster.Uri != null)
				nfcRtdUriControl1.SetContent(smartPoster.Uri);
			if ((smartPoster.Title != null) && (smartPoster.Title.Count > 0))
				nfcRtdTextControl1.SetContent(smartPoster.Title[0]);
			
			if (smartPoster.Action != null)
			{
				switch(smartPoster.Action.Value)
				{
					case 0	:
						ACT_Combo.Text = "Do the action";
						break;
						
					case 1	:
						ACT_Combo.Text = "Save for later";
						break;
						
					case 2	:
						ACT_Combo.Text = "Open for editing";
						break;
						
						default	:
							ACT_Combo.Text = "";
						break;
				}
			}
			
			if (smartPoster.TargetType != null)
				cbMime.Text = smartPoster.TargetType.Value;
			
			if (smartPoster.TargetSize != null)
				SIZE_txt.Text 	= smartPoster.TargetSize.Value.ToString();

		}
		
		/**m* SpringCardApplication/RtdSmartPoster.GetContentEx
		 *
		 * SYNOPSIS
		 *   public RtdSmartPoster GetContentEx()
		 * 
		 * DESCRIPTION
		 * 	 Constructs a RtdSmartPoster object, using the values of the different fields in the form
		 * 	 and returns this object
		 *
		 **/
		public RtdSmartPoster GetContentEx()
		{
			RtdSmartPoster smartPoster = new RtdSmartPoster();
			
			smartPoster.Uri = nfcRtdUriControl1.GetContentEx();
			
			RtdText t = nfcRtdTextControl1.GetContentEx();
			if (t != null)
				smartPoster.Title.Add(t);
			
			switch (ACT_Combo.SelectedIndex)
			{
					case 1	:	/*	do	*/
					smartPoster.Action = new RtdSmartPosterAction(0x00);
					break;
					
					case 2	:	/*	save	*/
					smartPoster.Action = new RtdSmartPosterAction(0x01);
					break;
					
					case 3	:	/*	open	*/
					smartPoster.Action = new RtdSmartPosterAction(0x02);
					break;
					
					default	:
						break;
					
			}

			if (!SIZE_txt.Text.Equals(""))
			{
				try
				{
					smartPoster.TargetSize = new RtdSmartPosterTargetSize(Int32.Parse(SIZE_txt.Text));
				}
				catch
				{

				}
			}
			
			if (!cbMime.Text.Equals(""))
				smartPoster.TargetType = new RtdSmartPosterTargetType(cbMime.Text);
			
			return smartPoster;
		}
		
		public override void SetContent(Ndef ndef)
		{
			if (ndef is RtdSmartPoster)
				SetContent((RtdSmartPoster) ndef);
		}
		
		public override Ndef GetContent()
		{
			return GetContentEx();
		}
		
	}
	
}
