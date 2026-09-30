/*
 * Crée par SharpDevelop.
 * Utilisateur: johann
 * Date: 01/10/2012
 * Heure: 15:46
 * 
 * Pour changer ce modèle utiliser Outils | Options | Codage | Editer les en-têtes standards.
 */
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using SpringCard.NfcForum;
using Be.Windows.Forms;
using SpringCard.NfcForum.Ndef;
using SpringCardApplication.Controls;
using SpringCard.LibCs;

namespace SpringCardApplication
{
	/// <summary>
	/// Description of NdefDisplayForm.
	/// </summary>
	public partial class NdefDisplayForm : Form
	{
		public NdefDisplayForm()
		{
			InitializeComponent();
		}
		
		public void SetNdef(NdefObject ndef)
		{
			/* Display the NDEF as raw data in the hexBox */
			/* ------------------------------------------ */
			
			Logger.Trace("SetNdef");

			byte[] b = ndef.Serialize();
			DynamicByteProvider p = new DynamicByteProvider(b);
			hexBox.ByteProvider = p;
			
			/* Try to recognize the NDEF, to display it in a user-friendly control */
			/* ------------------------------------------------------------------- */
			
			RtdControl control = null;

			if (ndef is RtdSmartPoster)
			{
				lbDecodedType.Text = "SmartPoster";
				control = new RtdSmartPosterControl();
				control.SetContent(ndef);
				
			}
			else if (ndef is RtdUri)
			{
				lbDecodedType.Text = "URI";
				control = new RtdUriControl();
				control.SetContent(ndef);
				
			}
			else if (ndef is RtdText)
			{
				lbDecodedType.Text = "Text";
				control = new RtdControl();
				control.SetContent(ndef);
				
			}
			else if (ndef is RtdVCard)
			{
				lbDecodedType.Text = "VCard";
				control = new RtdVCardControl();
				control.SetContent(ndef);

			}
			else if (ndef is RtdMedia)
			{
				lbDecodedType.Text = "Media (MIME)";
				control = new RtdMediaControl();
				control.SetContent(ndef);
			}
			
			if (control != null)
			{
				pDecoded.Controls.Clear();
				control.Dock = DockStyle.Fill;
				pDecoded.Controls.Add(control);
			}
		}
		
		void BtnCloseClick(object sender, EventArgs e)
		{
			Close();
		}
	}
}
