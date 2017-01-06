/**h* SpringCardApplication/RtdWifiHandoverControl
 *
 * NAME
 *   SpringCard API for NFC Forum :: RTD Wifi Handover Control class
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

	/**c* SpringCardApplication/RtdWifiHandoverControl
	 *
	 * NAME
	 *   RtdWifiHandoverControl
	 * 
	 * DESCRIPTION
	 *   Prints the content of an RTD Wifi Handover
	 *
	 * SYNOPSIS
	 *   RtdWifiHandoverControl control = new RtdWifiHandoverControl()
	 * 
	 * DERIVED FROM
	 *   RtdWifiHandoverControl
	 * 
	 * 
	 **/
	public partial class RtdWifiHandoverControl : RtdControl
	{
		
		public RtdWifiHandoverControl()
		{
			InitializeComponent();
			
			cbPowerState.Text = LoadString("Wifi.PowerState", "Active");
			tbSSID.Text = LoadString("Wifi.SSID", "Wifi Network");
			cbKeyType.Text = LoadString("Wifi.KeyType", "WPA/WPA2");
			tbKey.Text = LoadString("Wifi.Key", "Wifi Key");
			
		}
		
		public override void ClearContent()
		{
			cbKeyType.Text = "";
			cbPowerState.Text = "";
			tbSSID.Text = "";
			tbKey.Text = "";
			
		}
		
		
		public override void SetEditable(bool yes)
		{
			cbPowerState.Enabled = yes;
			tbSSID.ReadOnly = !yes;
			cbKeyType.Enabled = yes;
			tbKey.ReadOnly = !yes;
		}
		
		
		/**m* SpringCardApplication/RtdWifiHandoverControl.SetContent
		 *
		 * SYNOPSIS
		 *   public void SetContent(RtdHandoverSelector handoverSelector)
		 * 
		 * DESCRIPTION
		 * 	 Only called by the "public override void SetContent(Ndef ndef)" method, if the ndef is an RtdHandoverSelector object.
		 *   It prints on the form the content of the RtdHandoverSelector object passed as a parameter, only if it is a wifi handover.
		 * 	 If the content is not a wifi handover (a bluetooth handover for example), nothing is printed.
		 *
		 **/
		public void SetContent(RtdHandoverSelector handoverSelector)
		{
			ClearContent();
			string carrierDataReference = "";
			
			/* We try to print the first wifi handover found on the handoverSelector	*/
			for (int i = 0; i<handoverSelector.AlternativeCarriers.Length ; i++)
			{
				carrierDataReference = handoverSelector.AlternativeCarriers[i].CarrierDataReference;
				for (int j=0; j<handoverSelector.RelatedAbsoluteUris.Length ; j++)
				{
					if  (carrierDataReference.Equals(System.Text.Encoding.ASCII.GetString(handoverSelector.RelatedAbsoluteUris[j].ID)))
					{
						string content_string =  System.Text.Encoding.ASCII.GetString(handoverSelector.RelatedAbsoluteUris[j].PAYLOAD);
						
						switch (handoverSelector.AlternativeCarriers[i].CarrierPowerState)
						{
							case 0x00 :
								cbPowerState.Text = "Inactive";
								break;
								
							case 0x01 :
								cbPowerState.Text = "Active";
								break;
								
							case 0x02 :
								cbPowerState.Text = "Activating";
								break;
								
							case 0x03 :
								cbPowerState.Text = "Unknown";
								break;
						}
						
						if (!content_string.StartsWith("wifi://"))
						{
							MessageBox.Show("This Tag contains a valid Handover Selector, but this application doesn't know how to display it, as it only displays wifi connexions.", "This Handover Selector is not supported");
							return;
						}
						
						char[] delimiterChar = { '/' };
						string[] wifi_parameters = content_string.Split(delimiterChar);
						tbSSID.Text = wifi_parameters[2];
						if (wifi_parameters[3].Equals("open"))
						{
							cbKeyType.Text = "None";
						} else
							if (wifi_parameters[3].Equals("wep"))
						{
							cbKeyType.Text = "WEP";
						} else
							if  (wifi_parameters[3].Equals("wpa"))
						{
							cbKeyType.Text = "WPA/WPA2";
						}
						
						tbKey.Text = wifi_parameters[4];

					}
				}
				
			}
		}
		
		public override void SetContent(Ndef ndef)
		{
			if (ndef is RtdHandoverSelector)
				SetContent((RtdHandoverSelector) ndef);
		}
		
		public override bool ValidateUserContent()
		{
			if ((cbKeyType.SelectedIndex != 0) && (cbKeyType.SelectedIndex != 1)  && (cbKeyType.SelectedIndex != 02) )
				return false;
			
			if ((cbPowerState.SelectedIndex!=0) && (cbPowerState.SelectedIndex!=1) && (cbPowerState.SelectedIndex!=2) && (cbPowerState.SelectedIndex!=3))
				return false;
			
			return true;
		}
		
		/**m* SpringCardApplication/RtdWifiHandoverControl.GetContentEx
		 *
		 * SYNOPSIS
		 *   public RtdHandoverSelector GetContentEx()
		 * 
		 * DESCRIPTION
		 * 	 Constructs a RtdHandoverSelector object, containing  a wifi handover, using the values of the different fields in the form
		 * 	 and returns this object
		 *
		 **/
		public RtdHandoverSelector GetContentEx()
		{
			byte power_status = 0x01;
			switch (cbPowerState.SelectedIndex)
			{
				case 0:
					power_status = 0x01;
					break;
					
				case 1:
					power_status = 0x00;
					break;
					
				case 2:
					power_status = 0x02;
					break;
					
				case 3:
					power_status = 0x03;
					break;
					
					default :
						break;
					
			}
			
			
			string wifi_parameters = "";
			switch (cbKeyType.SelectedIndex)
			{
				case 0:
					wifi_parameters = "wifi://" + tbSSID.Text + "/open/";
					break;
					
				case 1:
					wifi_parameters = "wifi://" + tbSSID.Text + "/wep/" + tbKey.Text;
					break;
					
				case 2:
					wifi_parameters = "wifi://" + tbSSID.Text + "/wpa/" + tbKey.Text;
					break;
					
					default :
						break;
			}

			SaveString("Wifi.PowerState", cbPowerState.Text);
			SaveString("Wifi.SSID", tbSSID.Text);
			SaveString("Wifi.KeyType", cbKeyType.Text);
			SaveString("Wifi.Key", tbKey.Text);
			
			RtdAlternativeCarrier alternative_carrier = new RtdAlternativeCarrier("0", power_status);
			AbsoluteUri related_absolute_uri = new AbsoluteUri("0", wifi_parameters);

			RtdHandoverSelector HandoverSelector = new RtdHandoverSelector(alternative_carrier, related_absolute_uri);

			return HandoverSelector;
		}
		
		public override Ndef GetContent()
		{
			return GetContentEx();
		}
	}
	
}
