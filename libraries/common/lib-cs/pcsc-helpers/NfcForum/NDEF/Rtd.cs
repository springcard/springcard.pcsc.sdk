/**h* SpringCard.NfcForum.Ndef/Rtd
 *
 * NAME
 *   SpringCard API for NFC Forum :: RTD class
 * 
 * COPYRIGHT
 *   Copyright (c) SpringCard SAS, 2012
 *   See LICENSE.TXT for information
 *
 **/
using System;

namespace SpringCard.NfcForum.Ndef
{
	/**c* SpringCard.NfcForum.Ndef/Rtd
	 *
	 * NAME
	 *   Rtd
	 * 
	 * DESCRIPTION
	 *   Represents a Record in a NDEF message
	 *
	 * DERIVED FROM
	 *   Ndef
	 * 
	 * DERIVED BY
	 *   RtdText
	 *   RtdUri
	 *   RtdMedia
	 *   RtdSmartPoster
	 * 
	 * SYNOPSIS
	 *   Rtd rtd = new Rtd(byte TNF, string Type)
	 *   Rtd rtd = new Rtd(byte TNF, string Type, byte[] id)
	 *   Rtd rtd = new Rtd(byte TNF, string Type, byte[] id, byte[] payload)
	 *   Rtd rtd = new Rtd(Ndef record)
	 *
	 **/
	public class Rtd : NdefObject
	{
		public Rtd(NdefTypeNameAndFormat TNF, string Type) : base(TNF, Type)
		{
			
		}
		
		public Rtd(NdefTypeNameAndFormat TNF, string Type, byte[] Id) : base(TNF, Type, Id, null)
		{
			
		}

        public Rtd(NdefTypeNameAndFormat TNF, string Type, string Id) : base(TNF, Type, Id, null)
        {

        }

        public Rtd(NdefTypeNameAndFormat TNF, string Type, byte[] Id, byte[] Payload) : base(TNF, Type, Id, Payload)
		{
			
		}
		
        public Rtd(NdefTypeNameAndFormat TNF, string Type, string Id, byte[] Payload) : base(TNF, Type, Id, Payload)
        {

        }

        public Rtd(NdefObject record) : base(record)
		{
			
		}
	}
}
