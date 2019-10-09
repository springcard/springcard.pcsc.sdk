/**h* SpringCard.NfcForum.Ndef/Ndef
 *
 * NAME
 *   SpringCard API for NFC Forum :: Absolute URI class
 * 
 * COPYRIGHT
 *   Copyright (c) SpringCard SAS, 2012-2013
 *   See LICENSE.TXT for information
 *
 **/
using System;

namespace SpringCard.NfcForum.Ndef
{

	/**c* SpringCard.NfcForum.Ndef/AbsoluteUri
	 *
	 * NAME
	 *   AbsoluteUri
	 * 
	 * DESCRIPTION
	 *   Represents a Ndef Record that stores an absolute URI
	 *
	 * DERIVED FROM
	 *   Ndef
	 * 
	 * 
	 * SYNOPSIS
	 *   AbsoluteUri absoluteUri = new AbsoluteUri(byte[] Id, byte[] Payload)
	 *   AbsoluteUri absoluteUri = new AbsoluteUri(string Id, string uri)
	 * 
	 * 
	 * USED BY
	 *   RtdHandoverSelector
	 *
	 **/
	
	public class AbsoluteUri : NdefObject
	{
		public AbsoluteUri(byte[] Id, byte[] Payload) : base(NdefTypeNameAndFormat.Absolute_URI, "U")
		{
			ID = Id;
			PAYLOAD = Payload;
		}
		
		public AbsoluteUri(string Id, string uri) :  base(NdefTypeNameAndFormat.Absolute_URI, "U")
		{
			ID = System.Text.Encoding.ASCII.GetBytes(Id);
			PAYLOAD = System.Text.Encoding.ASCII.GetBytes(uri);
		}

		
	}
	
}
