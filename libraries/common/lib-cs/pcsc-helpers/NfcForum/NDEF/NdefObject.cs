/**h* SpringCard.NfcForum.Ndef/Ndef
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
using System.Collections.Generic;
using SpringCard.LibCs;
using SpringCard.PCSC;

namespace SpringCard.NfcForum.Ndef
{
    /**c* SpringCard.NfcForum.Ndef/NdefObject
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

    public class NdefObject
    {
        public enum NdefTypeNameAndFormat : byte
        {
            Empty = 0x00,
            NFC_RTD_WKN = 0x01,
            Media_Type = 0x02,
            Absolute_URI = 0x03,
            NFC_RTD_EXT = 0x04,
            Unknown = 0x05,
            Unchanged = 0x06,
            Reserved = 0x07
        }

        public const byte HEADER_FLAG_MESSAGE_BEGIN = 0x80;
        public const byte HEADER_FLAG_MESSAGE_END = 0x40;
        public const byte HEADER_FLAG_CHUNK = 0x20;
        public const byte HEADER_FLAG_SHORT_RECORD = 0x10;
        public const byte HEADER_FLAG_ID_LENGTH_PRESENT = 0x08;
        public const byte HEADER_TNF_MASK = 0x07;

        private NdefTypeNameAndFormat tnf;
        private bool messageBegin;
        private bool messageEnd;
        private bool chunk = false;
        private bool shortRecord = false;

        private byte[] type = null;
        private byte[] id = null;
        protected byte[] payload = null;
        protected List<NdefObject> children = new List<NdefObject>();

        public NdefObject(NdefObject ndef)
        {
            TNF = ndef.TNF;
            TYPE = ndef.TYPE;
            PAYLOAD = ndef.PAYLOAD;
        }

        public NdefObject(NdefTypeNameAndFormat tnf, string type)
        {
            this.tnf = tnf;
            this.type = StrUtils.ToBytes(type);
        }

        public NdefObject(NdefTypeNameAndFormat tnf, string type, byte[] payload)
        {
            this.tnf = tnf;
            this.type = StrUtils.ToBytes(type);
            this.payload = payload;
        }

        public NdefObject(NdefTypeNameAndFormat tnf, string type, byte[] id, byte[] payload)
        {
            this.tnf = tnf;
            this.type = StrUtils.ToBytes(type);
            this.id = id;
            this.payload = payload;
        }

        public NdefObject(NdefTypeNameAndFormat tnf, string type, string id, byte[] payload)
        {
            this.tnf = tnf;
            this.type = StrUtils.ToBytes(type);
            this.id = StrUtils.ToBytes(id);
            this.payload = payload;
        }

        public NdefObject(NdefTypeNameAndFormat tnf, byte[] type, byte[] id, byte[] payload)
        {
            this.tnf = tnf;
            this.type = type;
            this.id = id;
            this.payload = payload;
        }

        public static NdefTypeNameAndFormat TranslateTNF(byte header)
        {
            return (NdefTypeNameAndFormat)(header & HEADER_TNF_MASK);
        }

        public bool Is(NdefTypeNameAndFormat tnf, string type)
        {
            if (tnf != this.tnf) return false;
            if (type != StrUtils.ToStr(this.type)) return false;
            return true;
        }

        public byte GetHeader()
        {
            byte result = (byte)tnf;
            if (id != null) result |= HEADER_FLAG_ID_LENGTH_PRESENT;
            if (shortRecord) result |= HEADER_FLAG_SHORT_RECORD;
            if (chunk) result |= HEADER_FLAG_CHUNK;
            if (messageEnd) result |= HEADER_FLAG_MESSAGE_END;
            if (messageBegin) result |= HEADER_FLAG_MESSAGE_BEGIN;
            return result;
        }

        public int GetChildCount()
        {
            if (children == null)
                return 0;
            return children.Count;
        }

        public NdefObject GetChild(int Index)
        {
            if (children == null)
                return null;
            if (Index >= children.Count)
                return null;
            return children[Index];
        }

        protected void SetChildren(NdefObject[] children)
        {
            this.children = new List<NdefObject>();
            foreach (NdefObject child in children)
                this.children.Add(child);
        }

        /**m* SpringCard.NfcForum.Ndef/NdefObject.SetMessageBegin
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
            this.messageBegin = mb;
		}

        public bool MB
        {
            get
            {
                return this.messageBegin;
            }
            set
            {
                this.messageBegin = value;
            }
        }
		
		/**m* SpringCard.NfcForum.Ndef/NdefObject.SetMessageEnd
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
            this.messageEnd = me;
		}

        public bool ME
        {
            get
            {
                return this.messageEnd;
            }
            set
            {
                this.messageEnd = value;
            }
        }

        /**v* SpringCard.NfcForum.Ndef/NdefObject.TNF
		 *
		 * SYNOPSIS
		 *   public NdefTypeNameAndFormat TNF
		 * 
		 * DESCRIPTION
		 *   Gets and sets the Type Name Format of the NDEF
		 *
		 *
		 **/
        public NdefTypeNameAndFormat TNF
		{
			get
			{
				return this.tnf;
			}
			set
			{
                this.tnf = value;
			}
		}
		
		/**v* SpringCard.NfcForum.Ndef/NdefObject.TYPE
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
				return CardBuffer.StringFromBytes(type);
			}
			set
			{
				type = CardBuffer.BytesFromString(value);
			}
		}
		
		
		/**v* SpringCard.NfcForum.Ndef/NdefObject.ID
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
				return id;
			}
			set
			{
				id = value;
			}
		}

		
		/**v* SpringCard.NfcForum.Ndef/NdefObject.PAYLOAD
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
				return payload;
			}
			set
			{
				payload = value;
			}
		}

		
		/**m* SpringCard.NfcForum.Ndef/NdefObject.Size
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
		public int Size(ref bool shortRecord)
		{
			int l = 2; /* 1 byte for the header and 1 for type length			*/
			
			if (payload != null)
			{
				shortRecord = (payload.Length < 256) ? true : false;
			}
			else
			{
				shortRecord = true;
			}

			if (shortRecord)
			{
				l += 1;
			} else
			{
				l += 4;
			}

			if (id != null)
			{
				l += 1; 		/* ID_Length byte	*/
				l += id.Length;
			}
			
			l += type.Length;
			
			if (payload != null)
				l += payload.Length;
			
			return l;
		}

        public int Size()
        {
            return Size(ref shortRecord);
        }

        public void ClearChildren()
        {
            children = new List<NdefObject>();
        }

        public void AddChild(NdefObject child)
        {
            children.Add(child);
        }

        /**m* SpringCard.NfcForum.Ndef/NdefObject.Serialize
		 *
		 * SYNOPSIS
		 * 	 public byte[] Serialize()
		 * 
		 * DESCRIPTION
		 *   Serializes the NDEF and returns the result
		 *
		 **/
        public byte[] Serialize()
		{
			byte[] b = null;
			
			if (Serialize(ref b))
				return b;
			
			return null;
		}


        /**m* SpringCard.NfcForum.Ndef/NdefObject.Serialize
		 *
		 * SYNOPSIS
		 *   public virtual bool Serialize(ref byte[] buffer)
		 * 
		 * DESCRIPTION
		 *   Serializes the NDEF and returns true if the operation succeeds
		 *
		 **/
        public virtual bool Serialize(ref byte[] buffer)
		{

			int offset;
			
			/* Serializes the children (if any), which will become the payload of the NDEF	*/
			if (children.Count != 0)
			{
				int payload_size = 0;
				bool child_is_short = false;
				
				for (int i=0; i<children.Count; i++)
					payload_size += children[i].Size(ref child_is_short);

				payload = new byte[payload_size];
				
				offset = 0;
				for (int i=0; i<children.Count; i++)
				{
					byte[] child_buffer = null;
					
					children[i].SetMessageBegin((i == 0) ? true : false);
					children[i].SetMessageEnd((i == children.Count - 1) ? true : false);
					
					if (!children[i].Serialize(ref child_buffer))
						return false;
					
					for (int j=0; j<child_buffer.Length; j++)
						payload[offset++] = child_buffer[j];
				}
			}

            /* Serializes the NDEF	*/
            int size = Size();

			buffer = new byte[size];
			offset = 0;

            byte header = GetHeader();
			buffer[offset++] = header;
			buffer[offset++] = (byte) type.Length;
			
			if (payload != null)
			{
				if (shortRecord)
				{
					buffer[offset++] = (byte) payload.Length;
				}
                else
				{
					int l = payload.Length;
					buffer[offset + 3] = (byte) (l % 0x00000100); l /= 0x00000100;
					buffer[offset + 2] = (byte) (l % 0x00000100); l /= 0x00000100;
					buffer[offset + 1] = (byte) (l % 0x00000100); l /= 0x00000100;
					buffer[offset + 0] = (byte) (l % 0x00000100);
					offset += 4;
				}
			}
			else
			{
				if (shortRecord)
				{
					offset++;
				}
                else
				{
					offset += 4;
				}				
			}
			
			if (id != null)
			{
				buffer[offset++] = (byte) id.Length;
			}

			for (int i=0; i<type.Length; i++)
				buffer[offset++] = type[i];
			
			if (id != null)
			{
				for (int i=0; i<id.Length; i++)
					buffer[offset++] = id[i];
			}
			
			if (payload != null)
			{
				for (int i=0; i<payload.Length; i++)
					buffer[offset++] = payload[i];
			}

			return true;
		}

		
		/**m* SpringCard.NfcForum.Ndef/NdefObject.NdefFoundCallback
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
		public delegate void NdefFoundCallback(NdefObject ndef);


		/**m* SpringCard.NfcForum.Ndef/NdefObject.Parse
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
			NdefObject ndef = null;
			bool terminated = true;
			
			while (NdefObject.Parse(buffer, ref offset, ref ndef, ref terminated))
			{
				if (callback != null)
					callback(ndef);
				
				if (terminated)
					return true;
			}

            Logger.Debug("Parsing failed at offset " + offset);
			return false;
		}

		public static bool Parse(byte[] buffer, ref int offset, ref NdefObject ndef, ref bool terminated)
		{
			if (offset > buffer.Length)
				return false;
			
			terminated = false;
			ndef = null;
			
			/*  Header */
			if (offset+1 > buffer.Length)
			{
                Logger.Debug("NDEF truncated after 'Header' byte");
				return false;
			}
			byte header = buffer[offset++];
			
			if (header == 0)
			{
                Logger.Debug("Empty byte?");
				return false;
			}
		
			/* Type length		*/
			if (offset+1 > buffer.Length)
			{
                Logger.Debug("NDEF truncated after 'Type Length' byte");
				return false;
			}
			uint type_length = buffer[offset++];
					
			/* Payload length	*/
			uint payload_length = 0;
			if ((header & HEADER_FLAG_SHORT_RECORD) != 0)
			{
				if (offset+1 > buffer.Length)
				{
                    Logger.Debug("NDEF truncated after 'Payload Length' byte");
					return false;
				}
				payload_length = buffer[offset++];
			} else
			{
				if (offset+4 > buffer.Length)
				{
                    Logger.Debug("NDEF truncated after 'Payload Length' dword");
					return false;
				}
				payload_length  = buffer[offset++]; payload_length *= 0x00000100;
				payload_length += buffer[offset++]; payload_length *= 0x00000100;
				payload_length += buffer[offset++]; payload_length *= 0x00000100;
				payload_length += buffer[offset++];
			}
			
			/* 	ID Length			*/
			uint id_length = 0;
			if ((header & HEADER_FLAG_ID_LENGTH_PRESENT) != 0)
			{
				if (offset+1 > buffer.Length)
				{
                    Logger.Debug("NDEF truncated after 'ID Length' byte");
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
                    Logger.Debug("NDEF truncated after 'Type' bytes");
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
                    Logger.Debug("NDEF truncated after 'ID' bytes");
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
                    Logger.Debug("NDEF truncated after 'Payload' bytes");
					return false;
				}
				payload = new byte[payload_length];
				for (int i=0; i<payload_length; i++)
					payload[i] = buffer[offset++];
			}
			
			/* OK */
			string type_s = CardBuffer.StringFromBytes(type);


            NdefTypeNameAndFormat tnf = TranslateTNF(header);

            switch (tnf)
			{
				case NdefTypeNameAndFormat.Empty :
					break;
					
				case NdefTypeNameAndFormat.NFC_RTD_WKN :
					if (type_s.Equals("Sp"))
					{
						ndef = new RtdSmartPoster(payload);
					}
                    else if (type_s.Equals("U"))
					{
						ndef = new RtdUri(payload);
					}
                    else if (type_s.Equals("T"))
					{
						ndef = new RtdText(payload);
					}
                    else if (type_s.Equals("act"))
					{
						ndef = new RtdSmartPosterAction(payload);						
					}
                    else if (type_s.Equals("s"))
					{
						ndef = new RtdSmartPosterTargetSize(payload);						
					}
                    else if (type_s.Equals("t"))
					{
						ndef = new RtdSmartPosterTargetType(payload);
					}
                    else if (type_s.Equals("Hs"))
					{
						ndef = new RtdHandoverSelector(payload, ref buffer, ref offset);
					}
                    else if (type_s.Equals("ac"))
					{
						ndef = new RtdAlternativeCarrier(payload);
					}
                    else
					{
						Logger.Debug("Unknown WKN RTD: " + type_s);
					}
					break;
					
				case NdefTypeNameAndFormat.Media_Type :
					if (type_s.ToLower().Equals("text/x-vcard"))
					{
						ndef = new RtdVCard(payload);
					}
                    else
					{
						ndef = new RtdMedia(type_s, payload);
					}
					break;
					
				case NdefTypeNameAndFormat.Absolute_URI :
					if (type_s.Equals("U"))
					{
						ndef = new AbsoluteUri(id, payload);
					}
					break;
					
				case NdefTypeNameAndFormat.NFC_RTD_EXT :
                    ndef = new RtdExternalType(type_s, payload);
                    break;

				case NdefTypeNameAndFormat.Unknown :
					break;

				case NdefTypeNameAndFormat.Unchanged :
					break;

				case NdefTypeNameAndFormat.Reserved :
					break;
					
				default :
					return false; // Invalid
			}
			
			if (ndef == null)
			{
				ndef = new NdefObject(tnf, type, id, payload);
			}
			
			if (offset >= buffer.Length)
			{
				terminated = true;
			}

			return true;
		}
		
		public static NdefObject[] Deserialize(byte[] buffer, bool lookForChildren = false)
		{
			int offset = 0;
			List<NdefObject> ndefs = new List<NdefObject>();
			NdefObject ndef = null;
			bool terminated = true;
			
			while (NdefObject.Parse(buffer, ref offset, ref ndef, ref terminated))
			{
                if (lookForChildren && (ndef.PAYLOAD != null) && (ndef.PAYLOAD.Length >= 3))
                {
                    NdefObject[] children = Deserialize(ndef.PAYLOAD, false);
                    ndef.SetChildren(children);
                }

				ndefs.Add(ndef);
				
				if (terminated)
					break;
			}
			
			if (ndefs.Count == 0)
				return null;
			
			return ndefs.ToArray();
		}
		

        public static NdefObject DeserializeOne(byte[] buffer, bool lookForChildren = false)
        {
            NdefObject[] ndefObjects = Deserialize(buffer, lookForChildren);

            if (ndefObjects == null)
                throw new Exception("Invalid NDEF message");
            if (ndefObjects.Length == 0)
                throw new Exception("Empty NDEF message");
            if (ndefObjects.Length != 1)
                throw new Exception("Found an array of NDEF messages, expecting only one");

            return ndefObjects[0];
        }
    }
	
}
