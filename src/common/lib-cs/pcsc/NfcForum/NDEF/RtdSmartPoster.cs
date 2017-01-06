/**h* SpringCard.NFC/RtdSmartPoster
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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using SpringCard.PCSC;

namespace SpringCard.NFC
{

	/**c* SpringCard.NFC/RtdSmartPosterAction
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
		public RtdSmartPosterAction(int Action) : base(Ndef.NDEF_HEADER_TNF_NFC_RTD_WKN, "act")
		{
			_payload = new byte[1];
			_payload[0] = (byte) Action;
		}
		
		public RtdSmartPosterAction(byte[] Payload) : base(Ndef.NDEF_HEADER_TNF_NFC_RTD_WKN, "act")
		{
			if ((Payload == null) || (Payload.Length == 0))
			{
				_payload = new byte[1];
			} else
			{
				_payload = Payload;
			}
		}

		/**v* SpringCard.NFC/RtdSmartPosterAction.Value
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
			get{ return _payload[0]; }
			set{ _payload[0] = (byte) Value;}
		}
	}

	
	/**c* SpringCard.NFC/RtdSmartPosterTargetIcon
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
		public RtdSmartPosterTargetIcon(string IconMimeType, byte[] ImageBlob) : base(Ndef.NDEF_HEADER_TNF_MEDIA_TYPE, IconMimeType)
		{
			_payload = ImageBlob;
		}
	}

	
	/**c* SpringCard.NFC/RtdSmartPosterTargetSize
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

		public RtdSmartPosterTargetSize(int Size) : base(NDEF_HEADER_TNF_NFC_RTD_WKN, "s")
		{
			_payload = new byte[4];
			_payload[3] = (byte) (Size); Size /= 0x00000100;
			_payload[2] = (byte) (Size); Size /= 0x00000100;
			_payload[1] = (byte) (Size); Size /= 0x00000100;
			_payload[0] = (byte) (Size);
		}
		
		public RtdSmartPosterTargetSize(byte[] Payload) : base(NDEF_HEADER_TNF_NFC_RTD_WKN, "s")
		{
			if ((Payload == null) || (Payload.Length == 0))
			{
				_payload = new byte[4];
			} else
			{
				_payload = Payload;
			}
		}
		
		/**v* SpringCard.NFC/RtdSmartPosterTargetSize.Value
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
				for (int i = 0 ; i< (_payload.Length -1) ; i++)
				{
					size += _payload[i];
					size <<= 8;
				}
				size += _payload[_payload.Length -1] ;
				return size;
			}
			set
			{
				_payload = new byte[4];
				_payload[3] = (byte) (Value); Value /= 0x00000100;
				_payload[2] = (byte) (Value); Value /= 0x00000100;
				_payload[1] = (byte) (Value); Value /= 0x00000100;
				_payload[0] = (byte) (Value);

			}
		}
		
	}

	
	/**c* SpringCard.NFC/RtdSmartPosterTargetType
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
		public RtdSmartPosterTargetType(string MimeType) : base(Ndef.NDEF_HEADER_TNF_NFC_RTD_WKN, "t")
		{
			_payload = CardBuffer.BytesFromString(MimeType);
		}
		
		public RtdSmartPosterTargetType(byte[] Payload) : base(Ndef.NDEF_HEADER_TNF_NFC_RTD_WKN, "t")
		{
			if ((Payload == null) || (Payload.Length == 0))
			{
				
			} else
			{
				_payload = Payload;
			}
		}
		
		/**v* SpringCard.NFC/RtdSmartPosterTargetType.Value
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
				if (_payload != null)
				{
					return CardBuffer.StringFromBytes(_payload);
				} else
				{
					return "";
				}
			}
			set{ _payload = CardBuffer.BytesFromString(Value);}
		}
		
	}

	
	/**c* SpringCard.NFC/RtdSmartPoster
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
		
		public RtdSmartPoster() : base(Ndef.NDEF_HEADER_TNF_NFC_RTD_WKN, "Sp")
		{
			
		}

		public RtdSmartPoster(byte[] payload) : base(Ndef.NDEF_HEADER_TNF_NFC_RTD_WKN, "Sp")
		{
			int offset = 0;
			Ndef ndef = null;
			bool terminated = true;
			
			while (Ndef.Parse(payload, ref offset, ref ndef, ref terminated))
			{
				if (ndef is RtdUri)
				{
					Trace.WriteLine("Got a new URI");
					Uri = (RtdUri) ndef;
				} else
					if (ndef is RtdText)
				{
					Trace.WriteLine("Got a new Text");
					Title.Add((RtdText) ndef);
				} else
					if (ndef is RtdSmartPosterAction)
				{
					Trace.WriteLine("Got a new SmartPoster Action");
					Action = (RtdSmartPosterAction) ndef;
				} else
					if (ndef is RtdSmartPosterTargetIcon)
				{
					Trace.WriteLine("Got a new SmartPoster Icon");
					TargetIcon = (RtdSmartPosterTargetIcon) ndef;
				} else
					if (ndef is RtdSmartPosterTargetType)
				{
					Trace.WriteLine("Got a new SmartPoster MIME type");
					TargetType = (RtdSmartPosterTargetType) ndef;
				} else
					if (ndef is RtdSmartPosterTargetSize)
				{
					Trace.WriteLine("Got a new SmartPoster Size");
					TargetSize = (RtdSmartPosterTargetSize) ndef;
				} else
				{
					Trace.WriteLine("Got an unknown child in the SmartPoster");
				}
				
				if (terminated)
					break;
			}
		}

		public override bool Encode(ref byte[] buffer)
		{
			_children.Clear();
			
			if (Uri != null)
				_children.Add(Uri);
			for (int i=0; i<Title.Count; i++)
				_children.Add(Title[i]);
			if (Action != null)
				_children.Add(Action);
			if (TargetIcon != null)
				_children.Add(TargetIcon);
			if (TargetType != null)
				_children.Add(TargetType);
			if (TargetSize != null)
				_children.Add(TargetSize);
			
			return base.Encode(ref buffer);
		}
	}
}
