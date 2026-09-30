/**h* SpringCardApplication/RtdControl
 *
 * NAME
 *   SpringCard API for NFC Forum :: RTD Control class
 * 
 * COPYRIGHT
 *   Copyright (c) Pro Active SAS, 2012
 *   See LICENSE.TXT for information
 *
 **/
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using Microsoft.Win32;
using SpringCard.NfcForum.Ndef;

namespace SpringCardApplication.Controls
{

	/**c* SpringCardApplication/RtdControl
	 *
	 * NAME
	 *   RtdControl
	 * 
	 * DESCRIPTION
	 *   Prints the content of an RTD NDEF message
	 *
	 * SYNOPSIS
	 *   RtdControl control = new RtdControl()
	 * 
	 * 
	 * DERIVED FROM
	 *   UserControl
	 * 
	 * DERIVED BY
	 *   RtdMediaControl
	 *   RtdSmartPosterControl
	 *   RtdTextControl
	 *   RtdUriControl
	 * 	 RtdVCardControl
	 * 	 RtdWifiHandoverControl
	 *
	 **/
	public partial class RtdControl : UserControl
	{
		public RtdControl()
		{
			InitializeComponent();
		}
		
		/**m* SpringCardApplication/RtdControl.SetEditable
		 *
		 * SYNOPSIS
		 *   public virtual void SetEditable(bool yes)
		 * 
		 * DESCRIPTION
		 *   Enables the form to become editable (the content of fields
		 *   can be changed) if the parameter is "true". If the parameter
		 * 	 is "false", the content of the fields cannot be changed
		 *
		 **/
		public virtual void SetEditable(bool yes)
		{
			
		}
		
		
		/**m* SpringCardApplication/RtdControl.ClearContent
		 *
		 * SYNOPSIS
		 *   public virtual void ClearContent()
		 * 
		 * DESCRIPTION
		 *   Erases all the values of the fields in this form
		 *
		 **/
		public virtual void ClearContent()
		{
			
		}
		
		
		/**m* SpringCardApplication/RtdControl.ValidateUserContent
		 *
		 * SYNOPSIS
		 *   public virtual bool ValidateUserContent()
		 * 
		 * DESCRIPTION
		 *   Checks if all the values entered on the form are valid.
		 * 	 Returns true if they are valid, false if not.
		 *
		 **/
		public virtual bool ValidateUserContent()
		{
			return false;
		}
		
		/**m* SpringCardApplication/RtdControl.SetContent
		 *
		 * SYNOPSIS
		 *   public virtual void SetContent(Ndef ndef)
		 * 
		 * DESCRIPTION
		 * 	 Prints on the form the content of the NDEF message,
		 *   passed as a parameter
		 *
		 **/
		public virtual void SetContent(NdefObject ndef)
		{
			
		}
		
		
		/**m* SpringCardApplication/RtdControl.GetContent
		 *
		 * SYNOPSIS
		 *   public virtual Ndef GetContent()
		 * 
		 * DESCRIPTION
		 * 	 Constructs a NDEF object, using the values of the different fields in the form
		 * 	 and returns the NDEF object
		 *
		 **/
		public virtual NdefObject GetContent()
		{
			return null;
		}
		
		protected void SaveString(string Name, string Value)
		{
			try
			{
				RegistryKey k = Registry.CurrentUser.CreateSubKey("SOFTWARE\\SpringCard\\" + Application.ProductName);
				k.SetValue(Name, Value);
			}
			catch (Exception)
			{

			}
		}
		
		protected string LoadString(string Name, string DefaultValue)
		{
			try
			{
				RegistryKey k = Registry.CurrentUser.OpenSubKey("SOFTWARE\\SpringCard\\" + Application.ProductName, false);
				return (string) k.GetValue(Name, DefaultValue);
			}
			catch (Exception)
			{
				return DefaultValue;
			}
		}
		
		protected string LoadString(string Name)
		{
			return LoadString(Name, "");
		}
		
		protected string[] GetMimeTypes()
		{
			List<string> r = new List<string>();
			
			char[] separators = new char[2];
			separators[0] = ' ';
			separators[1] = '\t';
			
			try
			{
				TextReader tr = new StreamReader("mime.types");
				
				for (;;)
				{
					string s = tr.ReadLine();
					if (s == null)
						break;
					
					if (s.StartsWith("#"))
						continue;
					
					s = s.Trim();
					
					string[] t = s.Split(separators);
					if (t.Length > 0)
					{
						if (t[0].Length > 0)
						{
							r.Add(t[0]);
						}
					}
				}
				
				tr.Close();
			}
			catch (Exception)
			{

			}
			
			return r.ToArray();
		}

	}
}
