/**h* SpringCard.NFC/RtdText
 *
 * NAME
 *   SpringCard API for NFC Forum :: RTD Text class
 * 
 * COPYRIGHT
 *   Copyright (c) SpringCard SAS, 2012
 *   See LICENSE.TXT for information
 *
 **/
using System;

namespace SpringCard.NFC
{
	/**c* SpringCard.NFC/RtdText
	 *
	 * NAME
	 *   RtdText
	 * 
	 * DESCRIPTION
	 *   Represents a Ndef Record that stores a Text
	 *   (according to NFC Forum RTD Text specification)
	 *
	 * DERIVED FROM
	 *   Rtd
	 * 
	 * SYNOPSIS
	 *   RtdText text = new RtdText(string Text)
	 *   RtdText text = new RtdText(string Text, string Lang)
	 *   RtdText text = new RtdText(byte[] Payload)
	 * 
	 * USED BY
	 *   RtdSmartPoster
	 *
	 **/
	public class RtdText : Rtd
	{
		private string _lang = "";
		private string _text = "";
		
		public RtdText(string Text) : base(Ndef.NDEF_HEADER_TNF_NFC_RTD_WKN, "T")
		{
			_lang = "en";
			_text = Text;
			EncodePayload();
		}
		
		public RtdText(string Text, string Lang) : base(Ndef.NDEF_HEADER_TNF_NFC_RTD_WKN, "T")
		{
			_lang = Lang;
			_text = Text;
			EncodePayload();
		}

		public RtdText(byte[] Payload) : base(Ndef.NDEF_HEADER_TNF_NFC_RTD_WKN, "T")
		{
			_lang = "";
			_text = "";
			
			int offset = 1;
			
			if ((Payload[0] & 0x3F) != 0)
			{
				for (int i=0; i<(Payload[0] & 0x3F); i++)
					_lang = _lang + (char) Payload[offset++];
			}

			while (offset < Payload.Length)
				_text = _text + (char) Payload[offset++];
			
			EncodePayload();
		}
		
		private void EncodePayload()
		{
			_payload = new byte[1 + _lang.Length + _text.Length];
			
			_payload[0] = 0;
			
			_payload[0] |= (byte) (_lang.Length & 0x3F);
			
			int offset = 1;
			
			for (int i=0; i<_lang.Length; i++)
				_payload[offset++] = (byte) _lang[i];
			for (int i=0; i<_text.Length; i++)
				_payload[offset++] = (byte) _text[i];
		}

		/**v* SpringCard.NFC/RtdText.Value
		 *
		 * SYNOPSIS
		 *   public string Value
		 * 
		 * DESCRIPTION
		 *  Gets and sets the text of the object
		 *
		 **/
		public string Value
		{
			get
			{
				return _text;
			}
			set
			{
				_text = value;
				EncodePayload();
			}
		}

		/**v* SpringCard.NFC/RtdText.Lang
		 *
		 * SYNOPSIS
		 *   public string TextContent
		 * 
		 * DESCRIPTION
		 *  Gets and sets the lang of the object
		 *
		 **/
		public string Lang
		{
			get
			{
				return _lang;
			}
			set
			{
				_lang = value;
				EncodePayload();
			}
		}
	}
}
