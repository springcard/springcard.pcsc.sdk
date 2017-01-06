/**h* SpringCard.NFC/RtdMedia
 *
 * NAME
 *   SpringCard API for NFC Forum :: RTD Media class
 * 
 * COPYRIGHT
 *   Copyright (c) SpringCard SAS, 2012
 *   See LICENSE.TXT for information
 *
 **/
using System;
using SpringCard.PCSC;

namespace SpringCard.NFC
{
	/**c* SpringCard.NFC/RtdMedia
	 *
	 * NAME
	 *   RtdMedia
	 * 
	 * DESCRIPTION
	 *   Represents a Ndef Record that stores an arbitrary MIME Media
	 *
	 * SYNOPSIS
	 *   RtdMedia media = new RtdMedia(string MimeType)
	 *   RtdMedia media = new RtdMedia(string MimeType, string TextContent)
	 *   RtdMedia media = new RtdMedia(string MimeType, byte[] RawContent)
	 * 
	 * DERIVED FROM
	 *   Rtd
	 * 
	 * DERIVED BY
	 *   RtdVCard
	 *
	 **/
	public class RtdMedia : Rtd
	{
		public RtdMedia(string MimeType) : base(NDEF_HEADER_TNF_MEDIA_TYPE, MimeType)
		{
			
		}
		
		public RtdMedia(string MimeType, string TextContent) : base(NDEF_HEADER_TNF_MEDIA_TYPE, MimeType)
		{
			_payload = CardBuffer.BytesFromString(TextContent);
		}

		public RtdMedia(string MimeType, byte[] RawContent) : base(NDEF_HEADER_TNF_MEDIA_TYPE, MimeType)
		{
			_payload = RawContent;
		}
		
		/**v* SpringCard.NFC/RtdMedia.TextContent
		 *
		 * SYNOPSIS
		 *   public string TextContent
		 * 
		 * DESCRIPTION
		 *  Returns the payload of the RtdMedia object, as an ASCII string
		 *
		 **/
		public string TextContent
		{
			get
			{
				if (_payload == null)
					return "";
				return CardBuffer.StringFromBytes(_payload);
			}
		}
		
		/**v* SpringCard.NFC/RtdMedia.RawContent
		 *
		 * SYNOPSIS
		 *   public byte[] RawContent
		 * 
		 * DESCRIPTION
		 *  Returns the payload of the RtdMedia object, as a byte array
		 *
		 **/
		public byte[] RawContent
		{
			get
			{
				return _payload;
			}
		}

	}
	
}
