/**h* SpringCard.NfcForum.Ndef/RtdText
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

namespace SpringCard.NfcForum.Ndef
{
	/**c* SpringCard.NfcForum.Ndef/RtdText
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
		
		public RtdText(string Text) : base(NdefTypeNameAndFormat.NFC_RTD_WKN, "T")
		{
			_lang = "en";
			_text = Text;
			EncodePayload();
		}
		
		public RtdText(string Text, string Lang) : base(NdefTypeNameAndFormat.NFC_RTD_WKN, "T")
		{
			_lang = Lang;
			_text = Text;
			EncodePayload();
		}

        public RtdText(string Text, string Lang, string ID) : base(NdefTypeNameAndFormat.NFC_RTD_WKN, "T", ID)
        {
            _lang = Lang;
            _text = Text;
            EncodePayload();
        }

        public RtdText(byte[] Payload) : base(NdefTypeNameAndFormat.NFC_RTD_WKN, "T")
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
			payload = new byte[1 + _lang.Length + _text.Length];
			
			payload[0] = 0;
			
			payload[0] |= (byte) (_lang.Length & 0x3F);
			
			int offset = 1;
			
			for (int i=0; i<_lang.Length; i++)
				payload[offset++] = (byte) _lang[i];
			for (int i=0; i<_text.Length; i++)
				payload[offset++] = (byte) _text[i];
		}

		/**v* SpringCard.NfcForum.Ndef/RtdText.Value
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

		/**v* SpringCard.NfcForum.Ndef/RtdText.Lang
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
