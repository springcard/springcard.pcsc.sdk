/**h* SpringCardApplication/DesfireFormatForm
 *
 * NAME
 *   SpringCard API for NFC Forum :: Desfire as type 4
 * 
 * COPYRIGHT
 *   Copyright (c) Pro Active SAS, 2012-2013
 *   See LICENSE.TXT for information
 *
 **/

using System;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using SpringCard.PCSC;

namespace SpringCardApplication
{
	
/**c* SpringCardApplication/DesfireFormatForm
 *
 * NAME
 *   DesfireFormatForm
 * 
 * DESCRIPTION
 *   Enables to format a DESFire EV1 card into a type 4 Tag
 * 
 * 
 * USED BY
 *   NfcTagType4
 *
 **/
	public partial class DesfireFormatForm : Form
	{
	
		int _reader;
		public bool format_ok = false;
		public DesfireFormatForm(int reader)
		{
			InitializeComponent();
			_reader = reader;
		}
		
		void Button2Click(object sender, EventArgs e)
		{
			this.Close();
		}

		void CheckBox1CheckedChanged(object sender, EventArgs e)
		{
			if (cbAllMemory.Checked)
			{
				tSize.Enabled = false;
			} else
			{
				tSize.Enabled = true;
			}
		}
		
		void Button1Click(object sender, EventArgs e)
		{
			
			if (tNewRootKey.Equals(""))
			{
				MessageBox.Show("New Key is empty !");
				return;
			}
			
			if (tOldRootKey.Equals(""))
			{
				MessageBox.Show("Old Key is empty !");
				return;
			}
			Process p;
			ProcessStartInfo info;
			string s="";
			int size = 0;
			format_ok = false;
			Trace.WriteLine("Formating DESFire EV1 card into type 4 tag");
			if (!cbAllMemory.Checked)
			{
				try
				{
					size =Int32.Parse(tSize.Text);
				}
				catch
				{
					MessageBox.Show("Size invalid");
					return;
				}
			}
			
			/* First: erase and set new key	*/
			string parameters = "-r " + _reader + " -e -q";

			if (!tOldRootKey.Text.Equals(""))
				parameters += " -k " + tOldRootKey.Text;
			
			if (!tNewRootKey.Text.Equals(""))
				parameters += " -s " + tNewRootKey.Text;

			info = new ProcessStartInfo("NfcToolDesfire.exe", parameters);
			info.RedirectStandardOutput = true;
			info.UseShellExecute = false;
			p = Process.Start(info);
			s = p.StandardOutput.ReadToEnd();
			p.WaitForExit();

			if (s.Equals(""))
			{
				/* Second: format, using the new key	*/
				parameters = "-r " + _reader + " -p -q";
			
				if (!tNewRootKey.Text.Equals(""))
					parameters += " -k " + tNewRootKey.Text;
				
				if (!cbAllMemory.Checked)
					parameters += " -z " + size;
					
				info = new ProcessStartInfo("NfcToolDesfire.exe", parameters);
				info.RedirectStandardOutput = true;
				info.UseShellExecute = false;
				p = Process.Start(info);
				string message = p.StandardOutput.ReadToEnd();
				p.WaitForExit();			
				
				format_ok = true;
				if (!message.Equals(""))
				{
					MessageBox.Show(message, "Error while formating", MessageBoxButtons.OK, MessageBoxIcon.Error);			
					format_ok = false;
				}
				
				MessageBox.Show("The DESFire EV1 has been successfully formatted !", "Formating ok", MessageBoxButtons.OK, MessageBoxIcon.None);
				
			} else
			{
				MessageBox.Show(s, "Error while formating", MessageBoxButtons.OK, MessageBoxIcon.Error);
				format_ok = false;
			}
			
		}
	}

}
