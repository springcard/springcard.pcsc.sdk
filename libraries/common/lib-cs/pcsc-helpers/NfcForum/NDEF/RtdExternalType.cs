/**h* SpringCard.NfcForum.Ndef/RtdUri
 *
 * NAME
 *   SpringCard API for NFC Forum :: RTD URI class
 * 
 * COPYRIGHT
 *   Copyright (c) SpringCard SAS, 2012
 *   See LICENSE.TXT for information
 *
 **/
using System;
using System.Collections;
using System.Collections.Generic;
using SpringCard.PCSC;
using SpringCard.LibCs;

namespace SpringCard.NfcForum.Ndef
{
	public class RtdExternalType : Rtd
	{
		
		public RtdExternalType(string Type) : base(NdefObject.NdefTypeNameAndFormat.NFC_RTD_EXT, Type)
		{

		}
		
		public RtdExternalType(string Type, string Payload) : base(NdefObject.NdefTypeNameAndFormat.NFC_RTD_EXT, Type)
		{
            payload = StrUtils.ToBytes_UTF8(Payload);
        }

		public RtdExternalType(string Type, byte[] Payload) : base(NdefObject.NdefTypeNameAndFormat.NFC_RTD_EXT, Type)
		{
            payload = Payload;
		}

		public RtdExternalType(NdefObject record) : base(record)
		{

		}

        public string PAYLOAD_Str
        {
            get
            {
                return StrUtils.ToStr_UTF8(payload);
            }
            set
            {
                payload = StrUtils.ToBytes_UTF8(value);
            }
        }
	}	
}
