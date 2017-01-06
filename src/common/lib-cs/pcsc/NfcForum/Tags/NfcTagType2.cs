/**h* SpringCard.NFC/NfcTag
 *
 * NAME
 *   SpringCard API for NFC Forum :: NfcTagType2 class
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
using System.Threading;
using SpringCard.PCSC;

namespace SpringCard.NFC
{

	/**c* SpringCard.NFC/NfcTagType2
	 *
	 * NAME
	 *   NfcTagType2
	 * 
	 * DERIVED FROM
	 *   NfcTag
	 * 
	 * DESCRIPTION
	 *   Represents a Type 2 NFC Tag that has been discovered on a reader.
	 *
	 * SYNOPSIS
	 *   if (NfcTagType2.Recognize(channel))
	 *     NfcTag tag = NfcTagType2.Create(SCardChannel channel)
	 *
	 **/
	public class NfcTagType2 : NfcTag
	{
		private const string ATR_MIFARE_UL   = "3B8F8001804F0CA0000003060300030000000068";
		private const string ATR_MIFARE_UL_C = "3B8F8001804F0CA00000030603003A0000000051";
		
		private const byte OFFSET_USER_DATA = 16;
		private const byte READ_4_PAGES = 16;
		
		private List<NfcTlv> _tlvs = new List<NfcTlv>();
		
		private byte[] _raw_data = null;


		public NfcTagType2(SCardChannel Channel) : base(Channel)
		{
			/* A type 2 Tag can always be locked */
			_lockable = true;
		}
		
		protected override bool WriteContent(byte[] ndef_content)
		{
			Trace.WriteLine("Writing the NFC Forum type 2 Tag");
			
			if (ndef_content == null)
				return false;
			
			/* Get the new NDEF TLV to store into the Tag */
			NfcTlv ndef_tlv = new NfcTlv(NDEF_MESSAGE_TLV, ndef_content);
			
			/* Remove the Terminator TLV (if some) */
			while ((_tlvs.Count > 0) && (_tlvs[_tlvs.Count-1].T == TERMINATOR_TLV))
				_tlvs.RemoveAt(_tlvs.Count-1);
			
			/* Where shall I put the NDEF TLV in the Tag ? */
			if (_tlvs.Count == 0)
			{
				_tlvs.Add(ndef_tlv);
			} else
			{
				for (int i=0; i<_tlvs.Count; i++)
				{
					if (_tlvs[i].T == NDEF_MESSAGE_TLV)
					{
						/* Replace this one */
						_tlvs[i] = ndef_tlv;
						ndef_tlv = null;
						break;
					}
				}
				
				if (ndef_tlv != null)
				{
					/* No NDEF in the Tag beforehand? Let's add it the the end */
					_tlvs.Add(ndef_tlv);
				}
			}
			
			CardBuffer actual_content = new CardBuffer();
			
			for (int i=0; i<_tlvs.Count; i++)
			{
				actual_content.Append(_tlvs[i].Serialize());
			}
			
			if (actual_content.Length > Capacity())
			{
				Trace.WriteLine("The size of the content (with its TLVs) is bigger than the tag's capacity");
				return false;
			}
			
			if ((actual_content.Length + 2) < Capacity())
			{
				/* Add a Terminator at the end */
				Trace.WriteLine("We add a TERMINATOR TLV at the end of the Tag");
				actual_content.Append((new NfcTlv(TERMINATOR_TLV, null)).Serialize());
			}
			
			/* And now write */
			ushort page = 4;
			for (long i=0; i<actual_content.Length; i+=4)
			{
				byte[] buffer = new byte[4];
				
				for (long j=0; j<4; j++)
				{
					if ((i+j) < actual_content.Length)
						buffer[j] = actual_content.GetByte(i+j);
				}
				
				if (!WriteBinary(_channel, page, buffer))
					return false;
				page++;
			}

			return true;
		}

		public override bool Format()
		{
			Trace.WriteLine("Formatting the NFC Forum type 2 Tag");
			
			byte[] cc_block = new byte[4];
			byte[] user_data = new byte[4];
			
			long capacity;

			capacity  = Capacity();
			capacity /= 8;
			if (capacity > 255)
				capacity = 255;
			
			cc_block[0] = NFC_FORUM_MAGIC_NUMBER;
			cc_block[1] = NFC_FORUM_VERSION_NUMBER;
			cc_block[2] = (byte) (capacity);
			cc_block[3] = 0x00;

			if (!WriteBinary(_channel, 3, cc_block))
			{
				Trace.WriteLine("Can't write the CC bytes");
				return false;
			}
			
			
			user_data[0] = 0x00; // Erase first bytes
			user_data[1] = 0x00; // in order to avoid finding false
			user_data[2] = 0x00; // TLVs
			user_data[3] = 0x00;

			if (!WriteBinary(_channel, 4, user_data))
			{
				Trace.WriteLine("Can't write the 1st page of user data");
				return false;
			}
			
			
			/* The Tag is now formatted */
			_formatted = true;
			/* So it's not formattable anymore */
			_formattable = false;
			/* We consider it is empty */
			_is_empty = true;

			return true;
		}
		
		public override bool Lock()
		{
			Trace.WriteLine("Locking the NFC Forum type 2 Tag");
			
			byte[] cc_block = ReadBinary(_channel, 3, 4);
			if ((cc_block == null) || (cc_block.Length != 4))
				return false;
			
			/* No write access at all */
			cc_block[3] = 0x0F;
			
			if (!WriteBinary(_channel, 3, cc_block))
				return false;
			
			/* Write the LOCKs */
			byte[] lock_block = new byte[4];
			lock_block[0] = 0xFF;
			lock_block[1] = 0xFF;
			lock_block[2] = 0xFF;
			lock_block[3] = 0xFF;

			if (!WriteBinary(_channel, 2, lock_block))
				return false;
			
			/* OK! */
			_locked = true;
			_lockable = false;

			return true;
		}
		
		public bool WritePage(ushort address, byte[] data)
		{
			return WriteBinary(_channel, address, data);
		}
		
		protected static byte[] ReadBinary(SCardChannel channel, ushort address, byte length)
		{
			CAPDU capdu = new CAPDU(0xFF, 0xB0, (byte) (address / 0x0100), (byte) (address % 0x0100), length);

			Trace.WriteLine("< " + capdu.AsString(" "));
			
			RAPDU rapdu = null;
			
			for (int retry = 0; retry < 4; retry++)
			{
				rapdu = channel.Transmit(capdu);
				
				if (rapdu == null)
					break;
				if ((rapdu.SW != 0x6F01) && (rapdu.SW != 0x6F02) && (rapdu.SW != 0x6F0B))
					break;
				
				Thread.Sleep(10);
			}
			
			if (rapdu == null)
			{
				Trace.WriteLine("Error '" + channel.LastErrorAsString + "' while reading the card");
				return null;
			}

			Trace.WriteLine("> " + rapdu.AsString(" "));

			if (rapdu.SW != 0x9000)
			{
				Trace.WriteLine("Bad status word " + rapdu.SWString + " while reading the card");
				return null;
			}
			
			if (!rapdu.hasData)
			{
				Trace.WriteLine("Empty response");
				return null;
			}
			
			return rapdu.data.GetBytes();
		}
		
		protected static bool WriteBinary(SCardChannel channel, ushort address, byte[] data)
		{
			if (data == null)
				return false;
			
			if (data.Length != 4)
			{
				Trace.WriteLine("Type 2 Tag: Write Binary accepts only 4B");
				return false;
			}
			
			CAPDU capdu = new CAPDU(0xFF, 0xD6, (byte) (address / 0x0100), (byte) (address % 0x0100), data);
			
			Trace.WriteLine("< " + capdu.AsString(" "));
			
			
			RAPDU rapdu = null;
			
			for (int retry = 0; retry < 4; retry++)
			{
				rapdu = channel.Transmit(capdu);
				
				if (rapdu == null)
					break;
				if ((rapdu.SW != 0x6F01) && (rapdu.SW != 0x6F02) && (rapdu.SW != 0x6F0B))
					break;
				
				Thread.Sleep(15);
			}

			if (rapdu == null)
			{
				Trace.WriteLine("Error '" + channel.LastErrorAsString + "' while writing the card");
				return false;
			}
			
			Trace.WriteLine("> " + rapdu.AsString(" "));
			
			if (rapdu.SW != 0x9000)
			{
				Trace.WriteLine("Bad status word " + rapdu.SWString + " while writing the card");
				return false;
			}
			
			
			return true;
		}
		
		protected override bool Read()
		{
			Trace.WriteLine("Reading the NFC Forum type 2 Tag");
			
			ushort page = 0;
			
			if (!Recognize(_channel, ref _formatted, ref _formattable, ref _locked))
			{
				return false;
			}
			
			CardBuffer buffer = new CardBuffer();
			
			for (page = 0; page < 256; page+=4)
			{
				byte[] data = ReadBinary(_channel, page, READ_4_PAGES);
				
				if (data == null)
					break;
				
				if (page > 0)
				{
					bool same_as_header = true;
					for (int i=0; i<OFFSET_USER_DATA; i++)
					{
						if (data[i] != buffer.GetByte(i))
						{
							same_as_header = false;
							break;
						}
					}
					if (same_as_header)
						break;
				}
				
				buffer.Append(data);
			}
			
			Trace.WriteLine("Read " + buffer.Length + "B of data from the Tag");
			
			_raw_data = buffer.GetBytes();
			
			_capacity = _raw_data.Length;
			if (_capacity <= OFFSET_USER_DATA)
			{
				_capacity = 0;
				return false;
			}
			
			if (!_formatted)
			{
				/* Guess the capacity from the read area */
				if ((_capacity > 64) && !_formatted)
				{
					/* Drop the 16 last bytes if they are not empty (locks on Mifare UltraLight C) */
					bool locks_found = false;
					for (long i=_capacity-16; i<_capacity; i++)
					{
						if (_raw_data[i] != 0)
						{
							locks_found = true;
							break;
						}
					}
					if (locks_found)
					{
						Trace.WriteLine("Locks found at the end");
						_capacity -= 16;
					}
				}
				_capacity -= OFFSET_USER_DATA;
				Trace.WriteLine("The Tag is not formatted, capacity=" + _capacity + "B");
				
			} else
			{
				/* Read the capacity in the CC */
				_capacity = 8 * _raw_data[14];
				Trace.WriteLine("The Tag is formatted, capacity read from the CC=" + _capacity + "B");
			}

			/* Is the tag empty ? */
			_is_empty = true;
			for (long i=0; i<_capacity; i++)
			{
				if (_raw_data[OFFSET_USER_DATA+i] != 0)
				{
					_is_empty = false;
					break;
				}
			}
			
			if (_is_empty)
			{
				Trace.WriteLine("The Tag is empty");
				return true;
			}

			byte[] ndef_data = null;
			
			if (!ParseUserData(buffer.GetBytes(OFFSET_USER_DATA, -1), ref ndef_data))
			{
				Trace.WriteLine("The parsing of the Tag failed");
				return false;
			}
			
			if (ndef_data == null)
			{
				Trace.WriteLine("The Tag doesn't contain a NDEF");
				_is_empty = true;
				return true;
			}
			
			_is_empty = false;
			
			Ndef[] t = Ndef.Parse(ndef_data);
			if (t == null)
			{
				Trace.WriteLine("The NDEF is invalid or unsupported");
				return false;
			}
			
			Trace.WriteLine(t.Length + " NDEF record(s) found in the Tag");
			
			/* This NDEF is the new content of the tag */
			Content.Clear();
			for (int i=0; i<t.Length; i++)
				Content.Add(t[i]);
			
			return true;
		}
		
		private bool ParseUserData(byte[] user_data, ref byte[] ndef_data)
		{
			byte[] buffer = user_data;
			byte[] remaining_buffer = null;
			
			_tlvs.Clear();
			
			while (buffer != null)
			{
				NfcTlv tlv = NfcTlv.Unserialize(buffer, ref remaining_buffer);
				buffer = remaining_buffer;
				
				if (tlv == null)
				{
					Trace.WriteLine("An invalid content has been found (not a T,L,V)");
					break;
				}
				
				switch (tlv.T)
				{
					case NDEF_MESSAGE_TLV :
						Trace.WriteLine("Found a NDEF TLV");
						if (ndef_data != null)
						{
							Trace.WriteLine("The Tag has already a NDEF, ignoring this one");
						} else
						{
							ndef_data = tlv.V;
						}
						break;
					case LOCK_CONTROL_TLV :
						Trace.WriteLine("Found a LOCK CONTROL TLV");
						break;
					case MEMORY_CONTROL_TLV :
						Trace.WriteLine("Found a MEMORY CONTROL TLV");
						break;
					case PROPRIETARY_TLV :
						Trace.WriteLine("Found a PROPRIETARY TLV");
						break;
					case TERMINATOR_TLV :
						Trace.WriteLine("Found a TERMINATOR TLV");
						/* After a terminator... we terminate */
						buffer = null;
						break;
					case NULL_TLV :
						/* Terminate here */
						buffer = null;
						break;
						default :
							Trace.WriteLine("Found an unsupported TLV (T=" + tlv.T + ")");
						return false;
				}
				
				if (tlv.T != NULL_TLV)
					_tlvs.Add(tlv);
				
			}

			return true;
		}


		/**m* SpringCard.NFC/NfcTagType2.RecognizeAtr
		 *
		 * SYNOPSIS
		 *   public static bool RecognizeAtr(CardBuffer atr)
		 * 	 public static bool RecognizeAtr(SCardChannel channel)
		 * 
		 * 
		 * DESCRIPTION
		 *   Checks wether the ATR of the card corresponds to the ATR
		 * 	 of a Mifare Ultralight or a Mifare Ultralight C card.
		 *   Returns true on success.
		 *
		 **/
		public static bool RecognizeAtr(CardBuffer atr)
		{
			string s = atr.AsString("");
			if (s.Equals(ATR_MIFARE_UL))
			{
				Trace.WriteLine("ATR: Mifare UltraLight");
				return true;
			}
			if (s.Equals(ATR_MIFARE_UL_C))
			{
				Trace.WriteLine("ATR: Mifare UltraLight C");
				return true;
			}
			
			return false;
		}
		
		public static bool RecognizeAtr(SCardChannel channel)
		{
			CardBuffer atr = channel.CardAtr;
			
			return RecognizeAtr(atr);
		}

		
		
		/**m* SpringCard.NFC/NfcTagType2.Create
		 *
		 * NAME
		 *   NfcTagType2.Create
		 * 
		 * SYNOPSIS
		 *   public static NfcTagType2 Create(SCardChannel channel)
		 *
		 * DESCRIPTION
		 *   Instanciates a new NfcTagType2 object for this card
		 * 
		 * SEE ALSO
		 *   NfcTagType2.Recognize
		 *
		 **/
		public static NfcTagType2 Create(SCardChannel channel)
		{
			NfcTagType2 t = new NfcTagType2(channel);
			
			if (!t.Read()) return null;
			
			return t;
		}
		

		/**f* SpringCard.NFC/NfcTagType2.Recognize
		 *
		 * NAME
		 *   NfcTagType2.Recognize
		 * 
		 * SYNOPSIS
		 *   public static bool Recognize(SCardChannel channel)
		 *
		 * DESCRIPTION
		 *   Returns true if the card on the reader is a NFC Forum type 2 Tag
		 *
		 * SEE ALSO
		 *   NfcTagType2.Create
		 * 
		 **/
		public static bool Recognize(SCardChannel channel)
		{
			bool formatted = false;
			bool formattable = false;
			bool write_protected = false;
			return Recognize(channel, ref formatted, ref formattable, ref write_protected);
		}

		public static bool Recognize(SCardChannel channel, ref bool formatted, ref bool formattable, ref bool write_protected)
		{
			byte[] header = ReadBinary(channel, 0, READ_4_PAGES);
			
			if (header == null)
			{
				Trace.WriteLine("Failed to read pages 0-3");
				return false;
			}
			
			if ((header[12] == 0) && (header[13] == 0) && (header[14] == 0) && (header[15] == 0))
			{
				/* The OTP bits are blank, assume the card is an unformatted type 2 Tag */
				Trace.WriteLine("OTP are blank");
				formatted = false;
				formattable = true;
				write_protected = false;
				return true;
			}
			
			if (header[12] != NFC_FORUM_MAGIC_NUMBER)
			{
				/* The OTP bits contain something else */
				Trace.WriteLine("OTP are not blank");
				formatted = false;
				formattable = false;
				write_protected = true;
				return false;
			}
			
			/* The OTP bits contain the NFC NDEF MAGIC NUMBER, so this is a formatted type 2 Tag */
			Trace.WriteLine("OTP = NFC Forum magic number");
			formatted = true;
			formattable = false;
			write_protected = true;
			if ((header[13] & 0xF0) != (NFC_FORUM_VERSION_NUMBER & 0xF0))
			{
				Trace.WriteLine("Version mismatch in OTP");
				return false;
			}
			if ((header[15] & 0xF0) == 0)
			{
				Trace.WriteLine("Free read access");
			} else
			{
				Trace.WriteLine("No read access");
				return false;
			}
			if ((header[15] & 0x0F) == 0)
			{
				Trace.WriteLine("Free write access");
				write_protected = false;
			} else
			{
				Trace.WriteLine("No write access");
			}
			return true;
		}

	}
}
