/**h* SpringCard.NFC/NfcTag
 *
 * NAME
 *   SpringCard API for NFC Forum :: NfcTagType4 class
 * 
 * COPYRIGHT
 *   Copyright (c) SpringCard SAS, 2012
 *   See LICENSE.TXT for information
 *
 **/
using System;
using System.Diagnostics;
using SpringCard.PCSC;

namespace SpringCard.NFC
{
	/**c* SpringCard.NFC/NfcTagType4
	 *
	 * NAME
	 *   NfcTagType4
	 * 
	 * DERIVED FROM
	 *   NfcTag
	 * 
	 * DERIVED BY
	 *   NfcTagType4_Desfire
	 * 
	 * DESCRIPTION
	 *   Represents a Type 4 NFC Tag that has been discovered on a reader.
	 *
	 * SYNOPSIS
	 *   if (NfcTagType4.Recognize(channel))
	 *     NfcTag tag = NfcTagType4.Create(SCardChannel channel)
	 *
	 **/
	public class NfcTagType4 : NfcTag
	{
		private const string NDEF_APPLICATION_ID = "D2760000850101";
		private const ushort NDEF_CC_FILE_ID = 0xE103;
		
		private bool _application_selected = false;
		private ushort _ndef_file_id = 0;
		private ushort _file_selected = 0;
		private ushort _max_le = 0;
		private ushort _max_lc = 0;

		public override bool Format()
		{
			return false;
		}
		
		public override bool Lock()
		{
			if (!SelectNfcApplication(_channel))
				return false;
			if (!SelectFile(NDEF_CC_FILE_ID))
				return false;

			byte[] cc_write_control = ReadBinary(14, 1);
			if (cc_write_control == null)
				return false;
			
			cc_write_control[0] = 0xFF;
			return WriteBinary(14, cc_write_control);
		}
		
		protected bool SelectNfcApplication()
		{
			if (!_application_selected)
			{
				if (!SelectNfcApplication(_channel))
				{
					SelectRootApplication(_channel);
					if (!SelectNfcApplication(_channel))
						return false;
				}
				_application_selected = true;
				_file_selected = 0;
			}
			return true;
		}

		protected bool SelectFile(ushort file_id)
		{
			if (_file_selected != file_id)
			{
				if (!SelectFile(_channel, file_id))
					return false;
				_file_selected = file_id;
			}
			return true;
		}
		
		protected byte[] ReadBinary(ushort offset, ushort length)
		{
			byte[] r = ReadBinary(_channel, offset, length);
			if (r == null)
			{
				_application_selected = false;
				_file_selected = 0;
			}
			return r;
		}

		protected bool WriteBinary(ushort offset, byte[] buffer)
		{
			bool r = WriteBinary(_channel, offset, buffer);
			if (!r)
			{
				_application_selected = false;
				_file_selected = 0;
			}
			return r;
		}

		protected override bool WriteContent(byte[] content)
		{
			long offset_in_content = 0;
			ushort offset_in_file = 2;
			byte[] buffer;
			
			/* Write the content */
			while (offset_in_content < content.Length)
			{
				if ((content.Length - offset_in_content) > _max_lc)
				{
					buffer = new byte[_max_lc];
				} else
					if ((content.Length - offset_in_content) > 254)
				{
					buffer = new byte[254];
				} else
				{
					buffer = new byte[content.Length - offset_in_content];
				}
				
				for (int i=0; i<buffer.Length; i++)
					buffer[i] = content[offset_in_content++];
				
				if (!WriteBinary(offset_in_file, buffer))
				{
					Trace.WriteLine("Failed to write the NDEF file at offset " + offset_in_file);
					return false;
				}
				
				offset_in_file += (ushort) buffer.Length;
			}
			
			/* Write the length as header */
			buffer = new byte[2];
			
			buffer[0] = (byte) (content.Length / 0x0100);
			buffer[1] = (byte) (content.Length % 0x0100);

			if (!WriteBinary(0, buffer))
			{
				Trace.WriteLine("Failed to write the header in the NDEF file");
				return false;
			}

			return true;
		}

		public NfcTagType4(SCardChannel Channel) : base(Channel) {}
		
		protected override bool Read()
		{
			long ndef_file_size = 0;
			byte[] buffer;
			
			if (!Recognize(_channel, ref _locked, ref _max_le, ref _max_lc, ref _ndef_file_id, ref ndef_file_size))
				return false;
			
			if (ndef_file_size > 2)
				_capacity = ndef_file_size - 2;
			else
				_capacity = 0;
			
			_formatted = true;
			
			_application_selected = true;
			_file_selected = NDEF_CC_FILE_ID;
			
			if (!SelectFile(_ndef_file_id))
			{
				Trace.WriteLine("Failed to select the NDEF file");
				return false;
			}
			
			buffer = ReadBinary(0, 2);
			if (buffer == null)
			{
				Trace.WriteLine("Failed to read from the NDEF file");
				return false;
			}
			
			if ((buffer[0] == 0) && (buffer[1] == 0))
			{
				Trace.WriteLine("Tag is empty");
				_is_empty = true;
				return true;
			}
			
			_is_empty = false;
			
			long ndef_announced_size = (long) (buffer[0] * 0x0100 + buffer[1]);
			
			if ((ndef_announced_size > (ndef_file_size - 2)) || (ndef_announced_size > 0xFFFF))
			{
				Trace.WriteLine("The NDEF file contains an invalid length");
				return false;
			}
			
			byte[] content = new byte[ndef_announced_size];
			
			ushort offset_in_file = 2;
			long offset_in_content = 0;
			
			while (offset_in_content < content.Length)
			{
				if (_max_le > 254)
					buffer = ReadBinary(offset_in_file, 254);
				else
					buffer = ReadBinary(offset_in_file, _max_le);
				
				if ((buffer == null) || (buffer.Length == 0))
				{
					buffer = ReadBinary(offset_in_file, 0);
					if ((buffer == null) || (buffer.Length == 0))
					{
						Trace.WriteLine("Failed to read the NDEF file at offset " + offset_in_file);
						return false;
					}
				}
				
				
				
				for (int i=0; i<buffer.Length; i++)
				{
					if (offset_in_content >= content.Length)
						break;
					content[offset_in_content++] = buffer[i];
				}
				
				offset_in_file += (ushort) buffer.Length;

				if ((offset_in_content >= content.Length) || (offset_in_file >= ndef_file_size))
					break;
			}
			
			Ndef[] ndefs = Ndef.Parse(content);
			
			if (ndefs == null)
			{
				Trace.WriteLine("The NDEF is invalid or unsupported");
				return true;
			}
			
			Trace.WriteLine(ndefs.Length + " NDEF record(s) found in the tag");
			
			/* This NDEF is the new content of the tag */
			Content.Clear();
			for (int i=0; i<ndefs.Length; i++)
				Content.Add(ndefs[i]);
			
			return true;
		}
		
		private static bool SelectFile(SCardChannel channel, ushort file_id)
		{
			CAPDU capdu = new CAPDU(0x00, 0xA4, 0x00, 0x0C, (new CardBuffer(file_id)).GetBytes());
			
			Trace.WriteLine("< " + capdu.AsString(" "));
			
			RAPDU rapdu = channel.Transmit(capdu);
			
			if (rapdu == null)
			{
				Trace.WriteLine("SelectFile " + String.Format("{0:X4}", file_id) + " error " + channel.LastError + " (" + channel.LastErrorAsString + ")");
				return false;
			}
			
			Trace.WriteLine("> " + rapdu.AsString(" "));
			
			if (rapdu.SW != 0x9000)
			{
				Trace.WriteLine("SelectFile " + String.Format("{0:X4}", file_id) + " failed " + rapdu.SWString + " (" + SCARD.CardStatusWordsToString(rapdu.SW) + ")");
				return false;
			}
			
			return true;
		}
		
		private static bool SelectRootApplication(SCardChannel channel)
		{
			CAPDU capdu = new CAPDU(0x00, 0xA4, 0x00, 0x00, "3F00");
			
			Trace.WriteLine("< " + capdu.AsString(" "));
			
			RAPDU rapdu = channel.Transmit(capdu);
			
			if (rapdu == null)
			{
				Trace.WriteLine("SelectRootApplication error " + channel.LastError + " (" + channel.LastErrorAsString + ")");
				return false;
			}
			
			Trace.WriteLine("> " + rapdu.AsString(" "));

			if (rapdu.SW != 0x9000)
			{
				Trace.WriteLine("SelectRootApplication failed " + rapdu.SWString + " (" + SCARD.CardStatusWordsToString(rapdu.SW) + ")");
				return false;
			}

			return true;
		}
		
		private static bool SelectNfcApplication(SCardChannel channel)
		{
			CAPDU capdu = new CAPDU(0x00, 0xA4, 0x04, 0x00, (new CardBuffer(NDEF_APPLICATION_ID)).GetBytes(), 0x00);
			
			Trace.WriteLine("< " + capdu.AsString(" "));
			
			RAPDU rapdu = channel.Transmit(capdu);
			
			if (rapdu == null)
			{
				Trace.WriteLine("SelectNfcApplication error " + channel.LastError + " (" + channel.LastErrorAsString + ")");
				return false;
			}
			
			Trace.WriteLine("> " + rapdu.AsString(" "));

			if (rapdu.SW != 0x9000)
			{
				Trace.WriteLine("SelectNfcApplication failed " + rapdu.SWString + " (" + SCARD.CardStatusWordsToString(rapdu.SW) + ")");
				return false;
			}

			return true;
		}

		/**f* SpringCard.NFC/NfcTagType4.Create
		 *
		 * NAME
		 *   NfcTagType4.Create
		 * 
		 * SYNOPSIS
		 *   public static NfcTagType4 Create(SCardChannel channel)
		 *
		 * DESCRIPTION
		 *   Instanciates a new NfcTagType4 object for this card
		 *
		 * SEE ALSO
		 *   NfcTagType4.Recognize
		 * 
		 **/
		public static NfcTagType4 Create(SCardChannel channel)
		{
			NfcTagType4 t = new NfcTagType4(channel);
			
			if (!t.Read()) return null;
			
			return t;
		}

		
		/**f* SpringCard.NFC/NfcTagType4.Recognize
		 *
		 * NAME
		 *   NfcTagType4.Recognize
		 * 
		 * SYNOPSIS
		 *   public static bool Recognize(SCardChannel channel)
		 *
		 * DESCRIPTION
		 *   Return true if the card on the reader is a NFC Forum type 4 Tag
		 *
		 * SEE ALSO
		 *   NfcTagType4.Create
		 * 
		 **/
		public static bool Recognize(SCardChannel channel)
		{
			bool write_protected = false;
			ushort max_le = 0;
			ushort max_lc = 0;
			ushort ndef_file_id = 0;
			long ndef_file_size = 0;
			return Recognize(channel, ref write_protected, ref max_le, ref max_lc, ref ndef_file_id, ref ndef_file_size);
		}

		public static bool Recognize(SCardChannel channel, ref bool write_protected, ref ushort max_le, ref ushort max_lc, ref ushort ndef_file_id, ref long ndef_file_size)
		{
			if (!SelectNfcApplication(channel))
			{
				SelectRootApplication(channel);
				if (!SelectNfcApplication(channel))
					return false;
			}
			if (!SelectFile(channel, NDEF_CC_FILE_ID))
				return false;
			
			byte[] cc_file_content = ReadBinary(channel, 0, 15);
			
			if ((cc_file_content == null) || (cc_file_content.Length < 15))
			{
				Trace.WriteLine("Failed to read the CC file");
				return false;
			}
			
			long l = cc_file_content[0] * 0x0100 + cc_file_content[1];
			if (l < 15)
			{
				Trace.WriteLine("Bad length in the CC file");
				return false;
			}
			
			if ((cc_file_content[2] & 0xF0) != 0x20)
			{
				Trace.WriteLine("Bad version in the CC file");
				return false;
			}
			
			max_le = (ushort) (cc_file_content[3] * 0x0100 + cc_file_content[4]);
			max_lc = (ushort) (cc_file_content[5] * 0x0100 + cc_file_content[6]);
			
			if (cc_file_content[7] != NDEF_FILE_CONTROL_TLV)
			{
				Trace.WriteLine("Bad TLV's Tag in the CC file");
				return false;
			}
			
			if (cc_file_content[8] < 6)
			{
				Trace.WriteLine("Bad TLV's Length in the CC file");
				return false;
			}
			
			ndef_file_id = (ushort) (cc_file_content[9] * 0x0100 + cc_file_content[10]);
			ndef_file_size = (long) (cc_file_content[11] * 0x0100 + cc_file_content[12]);
			
			if (cc_file_content[13] != 0x00)
			{
				Trace.WriteLine("No read access");
				return false;
			}
			
			if (cc_file_content[14] != 0x00)
			{
				Trace.WriteLine("No write access");
				write_protected = true;
			} else
				write_protected = false;
			
			return true;
		}
		
		protected static byte[] ReadBinary(SCardChannel channel, ushort offset, ushort length)
		{
			CAPDU capdu = new CAPDU(0x00, 0xB0, (byte) (offset / 0x0100), (byte) (offset % 0x0100), (byte) length);
			
			Trace.WriteLine("< " + capdu.AsString(" "));

			RAPDU rapdu = channel.Transmit(capdu);

			if (rapdu == null)
			{
				Trace.WriteLine("ReadBinary " + String.Format("{0:X4}", offset) + "," + String.Format("{0:X2}", (byte) length) + " error " + channel.LastError + " (" + channel.LastErrorAsString + ")");
				return null;
			}
			
			Trace.WriteLine("> " + rapdu.AsString(" "));
			
			if (rapdu.SW != 0x9000)
			{
				Trace.WriteLine("ReadBinary " + String.Format("{0:X4}", offset) + "," + String.Format("{0:X2}", (byte) length) + " failed " + rapdu.SWString + " (" + SCARD.CardStatusWordsToString(rapdu.SW) + ")");
				return null;
			}
			
			if (rapdu.hasData)
				return rapdu.data.GetBytes();
			
			return null;
		}

		protected static bool WriteBinary(SCardChannel channel, ushort offset, byte[] buffer)
		{
			CAPDU capdu = new CAPDU(0x00, 0xD6, (byte) (offset / 0x0100), (byte) (offset % 0x0100), buffer);
			
			Trace.WriteLine("< " + capdu.AsString(" "));

			RAPDU rapdu = channel.Transmit(capdu);
			
			if (rapdu == null)
			{
				Trace.WriteLine("WriteBinary " + String.Format("{0:X4}", offset) + "," + String.Format("{0:X2}", (byte) buffer.Length) + " error " + channel.LastError + " (" + channel.LastErrorAsString + ")");
				return false;
			}

			Trace.WriteLine("> " + rapdu.AsString(" "));

			if (rapdu.SW != 0x9000)
			{
				Trace.WriteLine("WriteBinary " + String.Format("{0:X4}", offset) + "," + String.Format("{0:X2}", (byte) buffer.Length) + " failed " + rapdu.SWString + " (" + SCARD.CardStatusWordsToString(rapdu.SW) + ")");
				return false;
			}
			return true;
		}

	}
}
