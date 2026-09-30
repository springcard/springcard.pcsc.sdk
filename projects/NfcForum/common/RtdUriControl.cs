/**h* SpringCardApplication/RtdTextControl
 *
 * NAME
 *   SpringCard API for NFC Forum :: RTD Uri Control class
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
using SpringCard.NfcForum.Ndef;

namespace SpringCardApplication.Controls
{
	/**c* SpringCardApplication/RtdUriControl
	 *
	 * NAME
	 *   RtdTextControl
	 * 
	 * DESCRIPTION
	 *   Prints the content of an RTD Uri NDEF
	 *
	 * SYNOPSIS
	 *   RtdUriControl control = new RtdUriControl()
	 * 
	 * DERIVED FROM
	 *   RtdControl
	 * 
	 * USED BY
	 * 		RtdSmartPosterControl
	 * 
	 **/
	public partial class RtdUriControl : RtdControl
	{
		public RtdUriControl()
		{
			InitializeComponent();
			
			eValue.Text = LoadString("Uri.Value", "http://www.springcard.com");
		}
		
		public override void SetEditable(bool yes)
		{
			eValue.ReadOnly = !yes;
		}

		public override void ClearContent()
		{
			eValue.Text = "";
		}
		
		public override bool ValidateUserContent()
		{
			if (eValue.Text.Equals(""))
			{
				MessageBox.Show("You must enter something in the URL box");
				return false;
			}
			return true;
		}
		
		/**m* SpringCardApplication/RtdUri.SetContent
		 *
		 * SYNOPSIS
		 *   public void SetContent(RtdUri uri)
		 * 
		 * DESCRIPTION
		 * 	 Only called by the "public override void SetContent(Ndef ndef)" method, if the ndef is an RtdUri object.
		 *   It prints on the form the content of the RtdUri object passed as a parameter.
		 *
		 **/
		public void SetContent(RtdUri uri)
		{
			ClearContent();
			eValue.Text 	= uri.Value;
		}
		
		/**m* SpringCardApplication/RtdUri.GetContentEx
		 *
		 * SYNOPSIS
		 *   public RtdUri GetContentEx()
		 * 
		 * DESCRIPTION
		 * 	 Constructs a RtdUri object, using the values of the different fields in the form
		 * 	 and returns this object
		 *
		 **/
		public RtdUri GetContentEx()
		{
			if (!eValue.Text.Equals(""))
			{
				SaveString("Uri.Value", eValue.Text);
			}

			RtdUri t = new RtdUri(eValue.Text);
			return t;
		}

		public override void SetContent(NdefObject ndef)
		{
			if (ndef is RtdUri)
				SetContent((RtdUri) ndef);
		}
		
		public override NdefObject GetContent()
		{
			return GetContentEx();
		}
		
	}
	
}
