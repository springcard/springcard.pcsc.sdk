/**h* SpringCard.NfcForum.Ndef/RtdSmartPoster
 *
 * NAME
 *   SpringCard API for NFC Forum :: SmartPoster class
 * 
 * COPYRIGHT
 *   Copyright (c) SpringCard SAS, 2012
 *   See LICENSE.TXT for information
 *
 **/
using System;
using System.Collections.Generic;
using SpringCard.LibCs;
using SpringCard.PCSC;

namespace SpringCard.NfcForum.Ndef
{

	/**c* SpringCard.NfcForum.Ndef/RtdSmartPosterAction
	 *
	 * NAME
	 *   RtdSmartPosterAction
	 * 
	 * DESCRIPTION
	 *   Represents a Ndef Record that stores a SmartPoster Action
	 *   (according to NFC Forum SmartPoster specification)
	 *
	 * SYNOPSIS
	 *   RtdSmartPosterAction smartPosterAction = new RtdSmartPosterAction(int Action)
	 * 	 RtdSmartPosterAction smartPosterAction = new RtdSmartPosterAction(byte[] Payload)
	 * 
	 * DERIVED FROM
	 *   Rtd
	 * 
	 * USED BY
	 *   RtdSmartPoster
	 *
	 **/
	public class RtdSmartPosterAction : Rtd
	{
		public RtdSmartPosterAction(int Action) : base(NdefObject.NdefTypeNameAndFormat.NFC_RTD_WKN, "act")
		{
			payload = new byte[1];
			payload[0] = (byte) Action;
		}
		
		public RtdSmartPosterAction(byte[] Payload) : base(NdefObject.NdefTypeNameAndFormat.NFC_RTD_WKN, "act")
		{
			if ((Payload == null) || (Payload.Length == 0))
			{
				payload = new byte[1];
			} else
			{
				payload = Payload;
			}
		}

		/**v* SpringCard.NfcForum.Ndef/RtdSmartPosterAction.Value
		 *
		 * SYNOPSIS
		 *   	public int Value
		 * 
		 * DESCRIPTION
		 *   Gets and sets the payload of the object
		 *
		 **/
		public int Value
		{
			get{ return payload[0]; }
			set{ payload[0] = (byte) Value;}
		}
	}

	
	/**c* SpringCard.NfcForum.Ndef/RtdSmartPosterTargetIcon
	 *
	 * NAME
	 *   RtdSmartPosterTargetIcon
	 * 
	 * DESCRIPTION
	 *   Represents a Ndef Record that stores a SmartPoster Icon
	 *   (according to NFC Forum SmartPoster specification)
	 *
	 * SYNOPSIS
	 *   RtdSmartPosterTargetIcon smartPosterTargetSize = new RtdSmartPosterTargetIcon(string IconMimeType, byte[] ImageBlob)
	 * 
	 * DERIVED FROM
	 *   Rtd
	 * 
	 * USED BY
	 *   RtdSmartPoster
	 *
	 **/
	public class RtdSmartPosterTargetIcon : Rtd
	{
		public RtdSmartPosterTargetIcon(string IconMimeType, byte[] ImageBlob) : base(NdefObject.NdefTypeNameAndFormat.Media_Type, IconMimeType)
		{
			payload = ImageBlob;
		}
	}

	
	/**c* SpringCard.NfcForum.Ndef/RtdSmartPosterTargetSize
	 *
	 * NAME
	 *   RtdSmartPosterTargetSize
	 * 
	 * DESCRIPTION
	 *   Represents a Ndef Record that stores a SmartPoster Size
	 *   (according to NFC Forum SmartPoster specification)
	 *
	 * SYNOPSIS
	 *   RtdSmartPosterTargetSize smartPosterTargetSize = new RtdSmartPosterTargetSize(int Size)
	 * 	 RtdSmartPosterTargetSize smartPosterTargetSize = new RtdSmartPosterTargetSize(byte[] Payload)
	 * 
	 * DERIVED FROM
	 *   Rtd
	 * 
	 * USED BY
	 *   RtdSmartPoster
	 *
	 **/
	public class RtdSmartPosterTargetSize : Rtd
	{

		public RtdSmartPosterTargetSize(int Size) : base(NdefTypeNameAndFormat.NFC_RTD_WKN, "s")
		{
			payload = new byte[4];
			payload[3] = (byte) (Size); Size /= 0x00000100;
			payload[2] = (byte) (Size); Size /= 0x00000100;
			payload[1] = (byte) (Size); Size /= 0x00000100;
			payload[0] = (byte) (Size);
		}
		
		public RtdSmartPosterTargetSize(byte[] Payload) : base(NdefTypeNameAndFormat.NFC_RTD_WKN, "s")
		{
			if ((Payload == null) || (Payload.Length == 0))
			{
				payload = new byte[4];
			} else
			{
				payload = Payload;
			}
		}
		
		/**v* SpringCard.NfcForum.Ndef/RtdSmartPosterTargetSize.Value
		 *
		 * SYNOPSIS
		 *   	public int Value
		 * 
		 * DESCRIPTION
		 *   Gets and sets the payload of the object
		 *
		 **/
		public int Value
		{
			get
			{
				int size = 0;
				for (int i = 0 ; i< (payload.Length -1) ; i++)
				{
					size += payload[i];
					size <<= 8;
				}
				size += payload[payload.Length -1] ;
				return size;
			}
			set
			{
				payload = new byte[4];
				payload[3] = (byte) (Value); Value /= 0x00000100;
				payload[2] = (byte) (Value); Value /= 0x00000100;
				payload[1] = (byte) (Value); Value /= 0x00000100;
				payload[0] = (byte) (Value);

			}
		}
		
	}

	
	/**c* SpringCard.NfcForum.Ndef/RtdSmartPosterTargetType
	 *
	 * NAME
	 *   RtdSmartPosterTargetType
	 * 
	 * DESCRIPTION
	 *   Represents a Ndef Record that stores a SmartPoster Type
	 *   (according to NFC Forum SmartPoster specification)
	 *
	 * SYNOPSIS
	 *   RtdSmartPosterTargetType smartPosterTargetType = new RtdSmartPosterTargetType(string MimeType)
	 * 	 RtdSmartPosterTargetType smartPosterTargetType = new RtdSmartPosterTargetType(byte[] Payload)
	 * 
	 * DERIVED FROM
	 *   Rtd
	 * 
	 * USED BY
	 *   RtdSmartPoster
	 *
	 **/
	public class RtdSmartPosterTargetType : Rtd
	{
		public RtdSmartPosterTargetType(string MimeType) : base(NdefObject.NdefTypeNameAndFormat.NFC_RTD_WKN, "t")
		{
			payload = CardBuffer.BytesFromString(MimeType);
		}
		
		public RtdSmartPosterTargetType(byte[] Payload) : base(NdefObject.NdefTypeNameAndFormat.NFC_RTD_WKN, "t")
		{
			if ((Payload == null) || (Payload.Length == 0))
			{
				
			} else
			{
				payload = Payload;
			}
		}
		
		/**v* SpringCard.NfcForum.Ndef/RtdSmartPosterTargetType.Value
		 *
		 * SYNOPSIS
		 *   	public string Value
		 * 
		 * DESCRIPTION
		 *   Gets and sets the payload of the object, as an ASCII string
		 *
		 *
		 **/
		public string Value
		{
			get
			{
				if (payload != null)
				{
					return CardBuffer.StringFromBytes(payload);
				} else
				{
					return "";
				}
			}
			set{ payload = CardBuffer.BytesFromString(Value);}
		}
		
	}

	
	/**c* SpringCard.NfcForum.Ndef/RtdSmartPoster
	 *
	 * NAME
	 *   RtdSmartPoster
	 * 
	 * DESCRIPTION
	 *   Represents a Ndef Record that stores a SmartPoster
	 *   (according to NFC Forum SmartPoster specification)
	 *
	 * SYNOPSIS
	 *   RtdSmartPoster smartPoster = new RtdSmartPoster()
	 * 	 RtdSmartPoster smartPoster = new RtdSmartPoster(byte[] payload)
	 * 
	 * 
	 * DERIVED FROM
	 *   Rtd
	 * 
	 * USES
	 *   RtdUri
	 *   RtdText
	 * 	 RtdSmartPosterAction
	 * 	 RtdSmartPosterTargetIcon
	 * 	 RtdSmartPosterTargetType
	 * 	 RtdSmartPosterTargetSize
	 *
	 **/
	public class RtdSmartPoster : Rtd
	{
		public RtdUri Uri = null;
		public List<RtdText> Title = new List<RtdText>();
		public RtdSmartPosterAction Action = null;
		public RtdSmartPosterTargetIcon TargetIcon = null;
		public RtdSmartPosterTargetType TargetType = null;
		public RtdSmartPosterTargetSize TargetSize = null;
		
		public RtdSmartPoster() : base(NdefObject.NdefTypeNameAndFormat.NFC_RTD_WKN, "Sp")
		{
			
		}

		public RtdSmartPoster(byte[] payload) : base(NdefObject.NdefTypeNameAndFormat.NFC_RTD_WKN, "Sp")
		{
			int offset = 0;
			NdefObject ndef = null;
			bool terminated = true;
			
			while (NdefObject.Parse(payload, ref offset, ref ndef, ref terminated))
			{
				if (ndef is RtdUri)
				{
					Logger.Trace("Got a new URI");
					Uri = (RtdUri) ndef;
				} else
					if (ndef is RtdText)
				{
					Logger.Trace("Got a new Text");
					Title.Add((RtdText) ndef);
				} else
					if (ndef is RtdSmartPosterAction)
				{
					Logger.Trace("Got a new SmartPoster Action");
					Action = (RtdSmartPosterAction) ndef;
				} else
					if (ndef is RtdSmartPosterTargetIcon)
				{
					Logger.Trace("Got a new SmartPoster Icon");
					TargetIcon = (RtdSmartPosterTargetIcon) ndef;
				} else
					if (ndef is RtdSmartPosterTargetType)
				{
					Logger.Trace("Got a new SmartPoster MIME type");
					TargetType = (RtdSmartPosterTargetType) ndef;
				} else
					if (ndef is RtdSmartPosterTargetSize)
				{
					Logger.Trace("Got a new SmartPoster Size");
					TargetSize = (RtdSmartPosterTargetSize) ndef;
				} else
				{
					Logger.Trace("Got an unknown child in the SmartPoster");
				}
				
				if (terminated)
					break;
			}
		}

		public override bool Serialize(ref byte[] buffer)
		{
			children.Clear();
			
			if (Uri != null)
				children.Add(Uri);
			for (int i=0; i<Title.Count; i++)
				children.Add(Title[i]);
			if (Action != null)
				children.Add(Action);
			if (TargetIcon != null)
				children.Add(TargetIcon);
			if (TargetType != null)
				children.Add(TargetType);
			if (TargetSize != null)
				children.Add(TargetSize);
			
			return base.Serialize(ref buffer);
		}
	}
}
