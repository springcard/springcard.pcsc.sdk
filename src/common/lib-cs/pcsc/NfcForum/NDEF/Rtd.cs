/**h* SpringCard.NFC/Rtd
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

namespace SpringCard.NFC
{
	/**c* SpringCard.NFC/Rtd
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
	public class Rtd : Ndef
	{
		public Rtd(byte TNF, string Type) : base(TNF, Type)
		{
			
		}
		
		public Rtd(byte TNF, string Type, byte[] id) : base(TNF, Type, id, null)
		{
			
		}
		
		public Rtd(byte TNF, string Type, byte[] id, byte[] payload) : base(TNF, Type, id, payload)
		{
			
		}
		
		public Rtd(Ndef record) : base(record)
		{
			
		}
	}
}
