/**h* SpringCard.NFC/Ndef
 *
 * NAME
 *   SpringCard API for NFC Forum :: NDEF class
 * 
 * COPYRIGHT
 *   Copyright (c) SpringCard SAS, 2012-2013
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

	/**c* SpringCard.NFC/Ndef
	 *
	 * NAME
	 *   Ndef
	 * 
	 * DESCRIPTION
	 *   Represents a NDEF message, or portion of message
	 *
	 * DERIVED BY
	 *   Rtd
	 *
	 * SYNOPSIS
	 *   Ndef ndef = new Ndef(bytes[])
	 *   Ndef ndef = new Ndef(byte TNF, string TYPE)
	 *   Ndef ndef = new Ndef(byte TNF, string TYPE, byte[] PAYLOAD)
	 *   Ndef ndef = new Ndef(Ndef ndef)
	 * 
	 **/

	public class Ndef
	{
		public const byte NDEF_HEADER_MESSAGE_BEGIN     = 0x80;
		public const byte NDEF_HEADER_MESSAGE_END       = 0x40;
		public const byte NDEF_HEADER_CHUNK_FLAG        = 0x20;
		public const byte NDEF_HEADER_SHORT_RECORD      = 0x10;
		public const byte NDEF_HEADER_ID_LENGTH_PRESENT = 0x08;
		public const byte NDEF_HEADER_TNF_MASK          = 0x07;
		public const byte NDEF_HEADER_TNF_EMPTY         = 0x00;
		public const byte NDEF_HEADER_TNF_NFC_RTD_WKN   = 0x01;
		public const byte NDEF_HEADER_TNF_MEDIA_TYPE    = 0x02;
		public const byte NDEF_HEADER_TNF_ABSOLUTE_URI  = 0x03;
		public const byte NDEF_HEADER_TNF_NFC_RTD_EXT   = 0x04;
		public const byte NDEF_HEADER_TNF_UNKNOWN       = 0x05;
		public const byte NDEF_HEADER_TNF_UNCHANGED     = 0x06;
		public const byte NDEF_HEADER_TNF_RESERVED      = 0x07;
		
		protected byte[] _payload = null;
		protected List<Ndef> _children = new List<Ndef>();

		private byte _header = 0;
		private byte[] _type = null;
		private byte[] _id = null;
		
		public Ndef(Ndef ndef)
		{
			TNF = ndef.TNF;
			TYPE = ndef.TYPE;
			PAYLOAD = ndef.PAYLOAD;
		}
		
		public Ndef(byte _TNF, string _TYPE)
		{
			_TNF &= 0x07;
			_header &= 0xF8;
			_header |= _TNF;
			_type = CardBuffer.BytesFromString(_TYPE);
		}

		public Ndef(byte _TNF, string _TYPE, byte[] _PAYLOAD)
		{
			_TNF &= 0x07;
			_header &= 0xF8;
			_header |= _TNF;
			_type = CardBuffer.BytesFromString(_TYPE);
			_payload = _PAYLOAD;
		}

		public Ndef(byte _TNF, string _TYPE, byte[] ID, byte[] _PAYLOAD)
		{
			_TNF &= 0x07;
			_header &= 0xF8;
			_header |= _TNF;
			_type = CardBuffer.BytesFromString(_TYPE);
			_id = ID;
			_payload = _PAYLOAD;
		}
		
		public Ndef(byte _TNF, byte[] _TYPE, byte[] ID, byte[] _PAYLOAD)
		{
			_TNF &= 0x07;
			_header &= 0xF8;
			_header |= _TNF;
			_type = _TYPE;
			_id = ID;
			_payload = _PAYLOAD;
		}
		
		/**m* SpringCard.NFC/Ndef.SetMessageBegin
		 *
		 * SYNOPSIS
		 *   public void SetMessageBegin(bool mb)
		 * 
		 * DESCRIPTION
		 *   Sets the "MESSAGE BEGIN" bit in header
		 *
		 * SEE ALSO
		 *   Ndef.SetMessageEnd
		 *
		 **/
		public void SetMessageBegin(bool mb)
		{
			if (mb)
				_header |= NDEF_HEADER_MESSAGE_BEGIN;
			else
				_header = (byte) (_header & ~NDEF_HEADER_MESSAGE_BEGIN);
		}

		
		/**m* SpringCard.NFC/Ndef.SetMessageEnd
		 *
		 * SYNOPSIS
		 *   public void SetMessageEnd(bool me)
		 * 
		 * DESCRIPTION
		 *   Sets the "MESSAGE END" bit in header
		 *
		 * SEE ALSO
		 *   Ndef.SetMessageBegin
		 *
		 **/
		public void SetMessageEnd(bool me)
		{
			if (me)
				_header |= NDEF_HEADER_MESSAGE_END;
			else
				_header = (byte) (_header & ~NDEF_HEADER_MESSAGE_END);
		}

		
		/**v* SpringCard.NFC/Ndef.TNF
		 *
		 * SYNOPSIS
		 *   public byte TNF
		 * 
		 * DESCRIPTION
		 *   Gets and sets the Type Name Format of the NDEF
		 *
		 *
		 **/
		public byte TNF
		{
			get
			{
				return (byte) (_header & 0x07);
			}
			set
			{
				value &= 0x07;
				_header &= 0xF8;
				_header |= value;
			}
		}

		
		/**v* SpringCard.NFC/Ndef.TYPE
		 *
		 * SYNOPSIS
		 *   public string TYPE
		 * 
		 * DESCRIPTION
		 *   Gets and sets the Type of the NDEF
		 *
		 *
		 **/
		public string TYPE
		{
			get
			{
				return CardBuffer.StringFromBytes(_type);
			}
			set
			{
				_type = CardBuffer.BytesFromString(value);
			}
		}
		
		
		/**v* SpringCard.NFC/Ndef.ID
		 *
		 * SYNOPSIS
		 *   public byte[] ID
		 * 
		 * DESCRIPTION
		 *   Gets and sets the Id of the NDEF
		 *
		 **/
		public byte[] ID
		{
			get
			{
				return _id;
			}
			set
			{
				_id = value;
			}
		}

		
		/**v* SpringCard.NFC/Ndef.PAYLOAD
		 *
		 * SYNOPSIS
		 *   public byte[] PAYLOAD
		 * 
		 * DESCRIPTION
		 *   Gets and sets the Payload of the NDEF
		 *
		 *
		 **/
		public byte[] PAYLOAD
		{
			get
			{
				return _payload;
			}
			set
			{
				_payload = value;
			}
		}

		
		/**m* SpringCard.NFC/Ndef.Size
		 *
		 * SYNOPSIS
		 *   public int Size(ref bool is_short_record)
		 * 
		 * DESCRIPTION
		 *   Calculates the size in bytes of the whole NDEF
		 *   The parameter is_short_record indicates if the NDEF is a short record
		 * 	 (length of payload < 256)
		 *
		 *
		 **/
		public int Size(ref bool is_short_record)
		{
			int l = 2; /* 1 byte for the header and 1 for type length			*/
			
			is_short_record = (_payload.Length < 256) ? true : false;

			if (is_short_record)
			{
				l += 1;
			} else
			{
				l += 4;
			}

			if (_id != null)
			{
				l += 1; 		/* ID_Length byte	*/
				l += _id.Length;
			}
			
			l += _type.Length;
			l += _payload.Length;
			
			return l;
		}
		
		
		/**m* SpringCard.NFC/Ndef.GetBytes
		 *
		 * SYNOPSIS
		 *   public byte[] GetBytes(bool isBegin, bool isEnd)
		 * 	 public byte[] GetBytes()
		 * 
		 * DESCRIPTION
		 *   Uses the "Encode" method to serialize the ndef and returns the byte array.
		 * 	 The two parameters isBegin and isEnd indicate if the NDEF is the first or the last
		 * 	 in a series of NDEF
		 *
		 * SEE ALSO
		 *   Ndef.Encode
		 * 
		 **/
		public byte[] GetBytes(bool isBegin, bool isEnd)
		{
			byte[] b = null;
			
			SetMessageBegin(isBegin);
			SetMessageEnd(isEnd);
			
			if (Encode(ref b))
				return b;
			
			return null;
		}

		public byte[] GetBytes()
		{
			return GetBytes(true, true);
		}

		
		/**m* SpringCard.NFC/Ndef.Encode
		 *
		 * SYNOPSIS
		 *   public virtual bool Encode(ref byte[] buffer)
		 * 
		 * DESCRIPTION
		 *   Serializes the NDEF and returns true if the operation succeeds
		 *
		 * 
		 **/
		public virtual bool Encode(ref byte[] buffer)
		{

			int offset;
			
			/* Serializes the children (if any), which will become the payload of the NDEF	*/
			if (_children.Count != 0)
			{
				Trace.WriteLine("Encoding children...");
				
				int payload_size = 0;
				bool child_is_short = false;
				
				for (int i=0; i<_children.Count; i++)
					payload_size += _children[i].Size(ref child_is_short);

				_payload = new byte[payload_size];
				
				offset = 0;
				for (int i=0; i<_children.Count; i++)
				{
					byte[] child_buffer = null;
					
					_children[i].SetMessageBegin((i == 0) ? true : false);
					_children[i].SetMessageEnd((i == _children.Count - 1) ? true : false);
					
					if (!_children[i].Encode(ref child_buffer))
						return false;
					
					for (int j=0; j<child_buffer.Length; j++)
						_payload[offset++] = child_buffer[j];
				}
			}
			
			/* Serializes the NDEF	*/
			Trace.WriteLine("Encoding NDEF");
			
			bool is_short_record = false;
			int record_size = Size(ref is_short_record);
			
			if (is_short_record)
				_header |= NDEF_HEADER_SHORT_RECORD;
			else
				_header = (byte) (_header & ~NDEF_HEADER_SHORT_RECORD);
			
			if (_id != null)
				_header |= NDEF_HEADER_ID_LENGTH_PRESENT;
			else
				_header = (byte) (_header & ~NDEF_HEADER_ID_LENGTH_PRESENT);

			buffer = new byte[record_size];
			offset = 0;
			
			Trace.WriteLine(String.Format("- Header : {0:X2}", _header));
			buffer[offset++] = _header;
			
			Trace.WriteLine(String.Format("- Type length : {0}", _type.Length));
			buffer[offset++] = (byte) _type.Length;
			
			Trace.WriteLine(String.Format("- Payload length : {0}", _payload.Length));
			if (is_short_record)
			{
				buffer[offset++] = (byte) _payload.Length;
			} else
			{
				int l = _payload.Length;
				buffer[offset + 3] = (byte) (l % 0x00000100); l /= 0x00000100;
				buffer[offset + 2] = (byte) (l % 0x00000100); l /= 0x00000100;
				buffer[offset + 1] = (byte) (l % 0x00000100); l /= 0x00000100;
				buffer[offset + 0] = (byte) (l % 0x00000100);
				offset += 4;
			}
			
			if (_id != null)
			{
				Trace.WriteLine(String.Format("- ID length : {0}", _id.Length));
				buffer[offset++] = (byte) _id.Length;
			}

			Trace.WriteLine("- Type : " + (new CardBuffer(_type)).AsString(" "));
			for (int i=0; i<_type.Length; i++)
				buffer[offset++] = _type[i];
			
			if (_id != null)
			{
				Trace.WriteLine("- ID : " + (new CardBuffer(_id)).AsString(" "));
				for (int i=0; i<_id.Length; i++)
					buffer[offset++] = _id[i];
			}
			
			Trace.WriteLine("- Payload : " + (new CardBuffer(_payload)).AsString(" "));
			for (int i=0; i<_payload.Length; i++)
				buffer[offset++] = _payload[i];


			return true;
		}

		
		/**m* SpringCard.NFC/Ndef.NdefFoundCallback
		 *
		 * SYNOPSIS
		 *   public delegate void NdefFoundCallback(Ndef ndef)
		 * 
		 * DESCRIPTION
		 *   Specifies the callback that will be called once a NDEF is found
		 *
		 * SEE ALSO
		 *   Ndef.Parse
		 * 
		 **/
		public delegate void NdefFoundCallback(Ndef ndef);


		/**m* SpringCard.NFC/Ndef.Parse
		 *
		 * SYNOPSIS
		 *   public static bool Parse(byte [] buffer, NdefFoundCallback callback)
		 * 	 public static bool Parse(byte[] buffer, ref int offset, ref Ndef ndef, ref bool terminated)
		 *	 public static Ndef[] Parse(byte[] buffer)
		 * 
		 * DESCRIPTION
		 *   Analyses a bytes array to retrieve one or several NDEFs in it
		 *
		 * SEE ALSO
		 *   Ndef.Parse
		 * 
		 **/
		public static bool Parse(byte[] buffer, NdefFoundCallback callback)
		{
			int offset = 0;
			Ndef ndef = null;
			bool terminated = true;
			
			while (Ndef.Parse(buffer, ref offset, ref ndef, ref terminated))
			{
				if (callback != null)
					callback(ndef);
				
				if (terminated)
					return true;
			}

			Trace.WriteLine("Parsing failed at offset " + offset);
			return false;
		}

		public static bool Parse(byte[] buffer, ref int offset, ref Ndef ndef, ref bool terminated)
		{
			if (offset > buffer.Length)
				return false;
			
			terminated = false;
			ndef = null;
			
			/*  Header */
			if (offset+1 > buffer.Length)
			{
				Trace.WriteLine("NDEF truncated after 'Header' byte");
				return false;
			}
			byte header = buffer[offset++];
			
			if (header == 0)
			{
				Trace.WriteLine("Empty byte?");
				return false;
			}
			
			/* Type length		*/
			if (offset+1 > buffer.Length)
			{
				Trace.WriteLine("NDEF truncated after 'Type Length' byte");
				return false;
			}
			int type_length = buffer[offset++];
			
			/* Payload length	*/
			int payload_length = 0;
			if ((header & NDEF_HEADER_SHORT_RECORD) != 0)
			{
				if (offset+1 > buffer.Length)
				{
					Trace.WriteLine("NDEF truncated after 'Payload Length' byte");
					return false;
				}
				payload_length = buffer[offset++];
			} else
			{
				if (offset+4 > buffer.Length)
				{
					Trace.WriteLine("NDEF truncated after 'Payload Length' dword");
					return false;
				}
				payload_length  = buffer[offset++]; payload_length *= 0x00000100;
				payload_length += buffer[offset++]; payload_length *= 0x00000100;
				payload_length += buffer[offset++]; payload_length *= 0x00000100;
				payload_length += buffer[offset++];
			}

			/* 	ID Length			*/
			int id_length = 0;
			if ((header & NDEF_HEADER_ID_LENGTH_PRESENT) != 0)
			{
				if (offset+1 > buffer.Length)
				{
					Trace.WriteLine("NDEF truncated after 'ID Length' byte");
					return false;
				}
				id_length = buffer[offset++];
			}
			
			/* Type */
			byte[] type = null;
			if (type_length > 0)
			{
				if (offset+type_length > buffer.Length)
				{
					Trace.WriteLine("NDEF truncated after 'Type' bytes");
					return false;
				}
				type = new byte[type_length];
				for (int i=0; i<type_length; i++)
					type[i] = buffer[offset++];
			}
			
			/* ID */
			byte[] id = null;
			if (id_length > 0)
			{
				if (offset+id_length > buffer.Length)
				{
					Trace.WriteLine("NDEF truncated after 'ID' bytes");
					return false;
				}
				id = new byte[id_length];
				for (int i=0; i<id_length; i++)
					id[i] = buffer[offset++];
			}
			
			/* Payload */
			byte[] payload = null;
			if (payload_length > 0)
			{
				if (offset+payload_length > buffer.Length)
				{
					Trace.WriteLine("NDEF truncated after 'Payload' bytes");
					return false;
				}
				payload = new byte[payload_length];
				for (int i=0; i<payload_length; i++)
					payload[i] = buffer[offset++];
			}
			
			/* OK */
			string type_s = CardBuffer.StringFromBytes(type);
			
			switch (header & NDEF_HEADER_TNF_MASK)
			{
				case NDEF_HEADER_TNF_EMPTY :
					break;
					
				case NDEF_HEADER_TNF_NFC_RTD_WKN :
					if (type_s.Equals("Sp"))
					{
						Trace.WriteLine("Found a SmartPoster");
						ndef = new RtdSmartPoster(payload);
					} else
						if (type_s.Equals("U"))
					{
						Trace.WriteLine("Found an URI");
						ndef = new RtdUri(payload);
					} else
						if (type_s.Equals("T"))
					{
						Trace.WriteLine("Found a Text");
						ndef = new RtdText(payload);
					} else
						if (type_s.Equals("act"))
					{
						Trace.WriteLine("Found an Action");
						ndef = new RtdSmartPosterAction(payload);
						
					} else
						if (type_s.Equals("s"))
					{
						Trace.WriteLine("Found a Size");
						ndef = new RtdSmartPosterTargetSize(payload);
						
					} else
						if (type_s.Equals("t"))
					{
						Trace.WriteLine("Found a MIME-Type");
						ndef = new RtdSmartPosterTargetType(payload);
					} else
						if (type_s.Equals("Hs"))
					{
						Trace.WriteLine("Found a Handover Selector");
						ndef = new RtdHandoverSelector(payload, ref buffer, ref offset);
					} else
						if (type_s.Equals("ac"))
					{
						Trace.WriteLine("Found a Alternative Carrier");
						ndef = new RtdAlternativeCarrier(payload);
					} else
					{
						Trace.WriteLine("Found an unknown RTD : " + type_s);
					}
					break;
					
				case NDEF_HEADER_TNF_MEDIA_TYPE :
					if (type_s.ToLower().Equals("text/x-vcard"))
					{
						Trace.WriteLine("Found a vCard");
						ndef = new RtdVCard(payload);
					} else
					{
						Trace.WriteLine("Found a MIME Media : " + type_s);
						ndef = new RtdMedia(type_s, payload);
					}
					break;
					
				case NDEF_HEADER_TNF_ABSOLUTE_URI :
					if (type_s.Equals("U"))
					{
						Trace.WriteLine("Found an absolute URI");
						ndef = new AbsoluteUri(id, payload);
					}
					break;
					
				case NDEF_HEADER_TNF_NFC_RTD_EXT :
					Trace.WriteLine("Found TNF urn:nfc:ext");
					break;
				case NDEF_HEADER_TNF_UNKNOWN :
					Trace.WriteLine("Found TNF unknown");
					break;
				case NDEF_HEADER_TNF_UNCHANGED :
					Trace.WriteLine("Found TNF unchanged");
					break;
				case NDEF_HEADER_TNF_RESERVED :
					Trace.WriteLine("Found TNF reserved");
					break;
					
				default :
					return false; // Invalid
			}
			
			if (ndef == null)
			{
				ndef = new Ndef(header, type, id, payload);
			}
			
			if (offset >= buffer.Length)
			{
				Trace.WriteLine("Done!");
				terminated = true;
			}

			return true;
		}
		
		public static Ndef[] Parse(byte[] buffer)
		{
			int offset = 0;
			List<Ndef> ndefs = new List<Ndef>();
			Ndef ndef = null;
			bool terminated = true;
			
			while (Ndef.Parse(buffer, ref offset, ref ndef, ref terminated))
			{
				ndefs.Add(ndef);
				
				if (terminated)
					break;
			}
			
			if (ndefs.Count == 0)
				return null;
			
			return ndefs.ToArray();
		}
		
	}
	
}
