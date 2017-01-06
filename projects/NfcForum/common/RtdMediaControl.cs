/**h* SpringCardApplication/RtdMediaControl
 *
 * NAME
 *   SpringCard API for NFC Forum :: RTD Media Control class
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

	/**c* SpringCardApplication/RtdMediaControl
	 *
	 * NAME
	 *   RtdMediaControl
	 * 
	 * DESCRIPTION
	 *   Prints the content of an RTD media NDEF
	 *
	 * SYNOPSIS
	 *   RtdMediaControl control = new RtdMediaControl()
	 * 
	 * DERIVED FROM
	 *   RtdControl
	 * 
	 **/
	public partial class RtdMediaControl : RtdControl
	{
		public RtdMediaControl()
		{
			InitializeComponent();
			
			cbType.Items.Clear();
			string[] s = GetMimeTypes();
			for (int i=0; i<s.Length; i++)
				cbType.Items.Add(s[i]);
			cbType.Text = LoadString("Media.Type", "text/plain");
			eContent.Text = LoadString("Media.Text", "SpringCard sample text for NFC Beam");
		}
		
		
		public override void SetEditable(bool yes)
		{
			cbType.Enabled = yes;
			eContent.ReadOnly = !yes;
		}

		public override void ClearContent()
		{
			cbType.Text = "";
			eContent.Text = "";
		}
		
		public override bool ValidateUserContent()
		{
			if (cbType.Text.Equals(""))
			{
				MessageBox.Show("The MIME type can't be empty");
				return false;
			}
			return true;
		}
		
		/**m* SpringCardApplication/RtdMediaControl.SetContent
		 *
		 * SYNOPSIS
		 *   public void SetContent(RtdMedia media)
		 * 
		 * DESCRIPTION
		 * 	 Only called by the "public override void SetContent(Ndef ndef)" method, if the ndef is an Rtdmedia object.
		 *   It prints on the form the content of the RtdMedia object passed as a parameter.
		 *
		 **/
		public void SetContent(RtdMedia media)
		{
			ClearContent();
			cbType.Text = media.TYPE;
			eContent.Text = media.TextContent;
		}
		
		
		/**m* SpringCardApplication/RtdMediaControl.GetContentEx
		 *
		 * SYNOPSIS
		 *   public RtdMedia GetContentEx()
		 * 
		 * DESCRIPTION
		 * 	 Constructs a RtdMedia object, using the values of the different fields in the form
		 * 	 and returns this object
		 *
		 **/
		public RtdMedia GetContentEx()
		{
			SaveString("Media.Type", cbType.Text);
			SaveString("Media.Text", eContent.Text);

			RtdMedia t = new RtdMedia(cbType.Text, eContent.Text);
			return t;
		}

		public override void SetContent(Ndef ndef)
		{
			if (ndef is RtdMedia)
				SetContent((RtdMedia) ndef);
		}
		
		public override Ndef GetContent()
		{
			return GetContentEx();
		}
		
	}
}
