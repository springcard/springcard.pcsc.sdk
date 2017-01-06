/**h* SpringCardApplication/RtdTextControl
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

	/**c* SpringCardApplication/RtdTextControl
	 *
	 * NAME
	 *   RtdTextControl
	 * 
	 * DESCRIPTION
	 *   Prints the content of an RTD Text NDEF
	 *
	 * SYNOPSIS
	 *   RtdTextControl control = new RtdTextControl()
	 * 
	 * DERIVED FROM
	 *   RtdControl
	 * 
	 * USED BY
	 * 		RtdSmartPosterControl
	 * 
	 **/
	public partial class RtdTextControl : RtdControl
	{
		public RtdTextControl()
		{
			InitializeComponent();
			
			eValue.Text = LoadString("Text.Value", "SpringCard: your first choice in NFC and smartcard readers");
			eLang.Text = LoadString("Text.Lang", "");
		}
		
		public override void SetEditable(bool yes)
		{
			eValue.ReadOnly = !yes;
			eLang.ReadOnly = !yes;
		}

		public override void ClearContent()
		{
			eValue.Text = "";
			eLang.Text = "";
		}
		
		public override bool ValidateUserContent()
		{
			if (eValue.Text.Equals(""))
			{
				MessageBox.Show("You must enter something in the Text field");
				return false;
			}
			return true;
		}
		
		/**m* SpringCardApplication/RtdTextControl.SetContent
		 *
		 * SYNOPSIS
		 *   public void SetContent(RtdText text)
		 * 
		 * DESCRIPTION
		 * 	 Only called by the "public override void SetContent(Ndef ndef)" method, if the ndef is an RtdTextControl object.
		 *   It prints on the form the content of the RtdTextControl object passed as a parameter.
		 *
		 **/
		public void SetContent(RtdText text)
		{
			ClearContent();
			eValue.Text = text.Value;
			eLang.Text = text.Lang;
		}
		
		
		/**m* SpringCardApplication/RtdTextControl.GetContentEx
		 *
		 * SYNOPSIS
		 *   public RtdTextControl GetContentEx()
		 * 
		 * DESCRIPTION
		 * 	 Constructs a RtdText object, using the values of the different fields in the form
		 * 	 and returns this object
		 *
		 **/
		public RtdText GetContentEx()
		{
			if (!eValue.Text.Equals(""))
			{
				SaveString("Text.Value", eValue.Text);
				SaveString("Text.Lang", eLang.Text);
			}

			RtdText t = new RtdText(eValue.Text, eLang.Text);
			return t;
		}

		public override void SetContent(Ndef ndef)
		{
			if (ndef is RtdText)
				SetContent((RtdText) ndef);
		}
		
		public override Ndef GetContent()
		{
			return GetContentEx();
		}
		
	}
}
