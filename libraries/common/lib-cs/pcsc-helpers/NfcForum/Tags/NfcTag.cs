﻿/**h* SpringCard.NfcForum.Tags/NfcTag
 *
 * NAME
 *   SpringCard API for NFC Forum :: NfcTag class
 * 
 * COPYRIGHT
 *   Copyright (c) SpringCard SAS, 2012
 *   See LICENSE.TXT for information
 *
 **/
using System;
using System.Collections.Generic;
using SpringCard.NfcForum.Ndef;
using SpringCard.LibCs;
using SpringCard.PCSC;

/**c* SpringCard.NfcForum.Tags/NfcTag
 *
 * NAME
 *   NfcTag
 * 
 * DESCRIPTION
 *   Represents an NFC Tag that has been discovered on a reader
 *
 * DERIVED BY
 *   NfcTagType2
 *   NfcTagType4
 *
 * SYNOPSIS
 *   NfcTag tag = new NfcTag(SCardChannel channel)
 *
 **/
namespace SpringCard.NfcForum.Tags
{
	public abstract class NfcTag
	{
		public const byte NFC_FORUM_MAGIC_NUMBER	= 0xE1;
		public const byte NFC_FORUM_VERSION_NUMBER	= 0x10;
		
		public const byte LOCK_CONTROL_TLV 		= 0x01;
		public const byte MEMORY_CONTROL_TLV 	= 0x02;
		public const byte NDEF_MESSAGE_TLV 		= 0x03;
		public const byte NDEF_FILE_CONTROL_TLV = 0x04;
		public const byte PROPRIETARY_TLV 		= 0xFD;
		public const byte TERMINATOR_TLV 		= 0xFE;
		public const byte NULL_TLV				= 0x00;
		
		protected ICardApduTransmitter _channel = null;
		
		protected long _capacity = 0;
		protected bool _is_empty = false;
		protected bool _formatted = false;
		protected bool _formattable = false;
		protected bool _locked = false;
		protected bool _lockable = false;

		public NfcTag(ICardApduTransmitter Channel)
		{
			_channel = Channel;
		}
		
		/**m* SpringCard.NfcForum.Tags/NfcTag.Content
		 *
		 * SYNOPSIS
		 *   public List<NDEF> Content
		 * 
		 * DESCRIPTION
		 *   The list of NDEF objects found on the Tag.
		 *   To change the Tag's content, update this list before
		 *   calling NfcTag.Write
		 *
		 * SEE ALSO
		 *   NfcTag.ContentSize
		 *
		 **/
		public List<NdefObject> Content = new List<NdefObject>();
		

		protected abstract bool WriteContent(byte[] content);
		
		
		/**m* SpringCard.NfcForum.Tags/NfcTag.Recognize
		 *
		 * SYNOPSIS
		 *   public static bool Recognize(SCardChannel cardchannel, out NfcTag tag, out string msg)
		 * 
		 * DESCRIPTION
		 *	 Determines if the card on the reader is a NFC Forum compliant tag
		 *	 It first checks the ATR to determine if the card can be type 2 or
		 * 	 a type 4 and tries to recognize the content of the tag.
		 * 	 It creates either a NfcTagType2 or a NfcTagType4 and returns true if the tag is recognized.
		 * 	 It returns false if the card is not a NFC Forum tag
		 *
		 * SEE ALSO
		 *   NfcTagType2.RecognizeAtr
		 *   NfcTagType2.Recognize
		 * 	 NfcTagType2.Create
		 *   NfcTagType4.Recognize
		 * 	 NfcTagType4.Create
		 * 
		 *
		 **/
		public static bool Recognize(SCardChannel cardchannel, out NfcTag tag, out string msg, out bool Desfire_formatable)
		{
			bool res = false;
			msg = "";
			tag = null;
			Desfire_formatable = false;
			
			if (NfcTagType2.RecognizeAtr(cardchannel))
			{
				Logger.Trace("Based on the ATR, this card is likely to be a NFC type 2 Tag");

				if (NfcTagType2.Recognize(cardchannel))
				{
					Logger.Trace("This card is actually a NFC type 2 Tag");
					tag = NfcTagType2.Create(cardchannel);
					if (tag == null)
						msg = "An error has occured while reading the Tag's content";
					res = true;
				} else
				{
					Logger.Trace("Based on its content, this card is not a NFC type 2 Tag, sorry");
					msg = "From the ATR it may be a NFC type 2 Tag, but the content is invalid";
				}
			} else
			if (NfcTagType4.Recognize(cardchannel))
			{
				Logger.Trace("This card is a NFC type 4 Tag");
				tag = NfcTagType4.Create(cardchannel);
				if (tag == null)
					msg = "An error has occured while reading the Tag's content";
				
				res = true;
				
			} else
#if (NFC_TAGS_DESFIRE)
			if (NfcTagType4Desfire.Recognize(cardchannel))
			{
				msg = "A DESFire EV1 card has been detected.\nIt may be formatted into a type 4 Tag.";
				Desfire_formatable = true;
				res = false;			
			} else
#endif
			{
				msg = "Unrecognized or unsupported card";
				tag = null;
			}
			return res;
		}
		
		
		private byte[] SerializeContent()
		{
			if ((Content == null) || (Content.Count == 0))
			{
				Logger.Trace("Nothing to serialize");
				return null;
			}

            byte[] result = new byte[0];
			
			for (int i=0; i<Content.Count; i++)
			{
                Content[i].SetMessageBegin((i == 0));
                Content[i].SetMessageEnd((i == (Content.Count - 1)));
                result = BinUtils.Concat(result, Content[i].Serialize());
			}
			
			return result;
		}

		
		/**m* SpringCard.NfcForum.Tags/NfcTag.IsEmpty
		 *
		 * SYNOPSIS
		 *   public bool IsEmpty()
		 * 
		 * DESCRIPTION
		 *   Returns true if the NfcTag doesn't contain any data.
		 *
		 * SEE ALSO
		 *   NfcTag.Write
		 *
		 **/
		public bool IsEmpty()
		{
			return _is_empty;
		}

		
		/**m* SpringCard.NfcForum.Tags/NfcTag.IsFormatted
		 *
		 * SYNOPSIS
		 *   public bool IsFormatted()
		 * 
		 * DESCRIPTION
		 *   Returns true if the NfcTag is ready to store a NDEF content.
		 *
		 * SEE ALSO
		 *   NfcTag.IsFormattable
		 *   NfcTag.Format
		 *
		 **/
		public bool IsFormatted()
		{
			return _formatted;
		}

		
		/**m* SpringCard.NfcForum.Tags/NfcTag.IsFormattable
		 *
		 * SYNOPSIS
		 *   public bool IsFormattable()
		 * 
		 * DESCRIPTION
		 *   Returns true if the NfcTag be formatted to store a NDEF content.
		 *
		 * SEE ALSO
		 *   NfcTag.IsFormatted
		 *   NfcTag.Format
		 *
		 **/
		public bool IsFormattable()
		{
			return (!_locked && !_formatted && _formattable);
		}

		
		/**m* SpringCard.NfcForum.Tags/NfcTag.IsLocked
		 *
		 * SYNOPSIS
		 *   public bool IsLocked()
		 * 
		 * DESCRIPTION
		 *   Returns true if the NfcTag is locked in read-only state
		 *
		 * SEE ALSO
		 *   NfcTag.IsLockable
		 *   NfcTag.Lock
		 *
		 **/
		public bool IsLocked()
		{
			return _locked;
		}

		/**m* SpringCard.NfcForum.Tags/NfcTag.IsLockable
		 *
		 * SYNOPSIS
		 *   public bool IsLockable()
		 * 
		 * DESCRIPTION
		 *   Returns true if the NfcTag could be locked in read-only state.
		 *
		 * SEE ALSO
		 *   NfcTag.IsLocked
		 *   NfcTag.Lock
		 *
		 **/
		public bool IsLockable()
		{
			return (!_locked && _lockable);
		}
		
		/**m* SpringCard.NfcForum.Tags/NfcTag.Capacity
		 *
		 * SYNOPSIS
		 *   public long Capacity()
		 * 
		 * DESCRIPTION
		 *   Returns the size of the Tag's NDEF storage container.
		 * 
		 * SEE ALSO
		 *   NfcTag.ContentSize
		 *
		 **/
		public long Capacity()
		{
			return _capacity;
		}

		/**m* SpringCard.NfcForum.Tags/NfcTag.ContentSize
		 *
		 * SYNOPSIS
		 *   public long ContentSize()
		 * 
		 * DESCRIPTION
		 *   Returns the actual size of the Tag's NDEF content.
		 * 
		 * SEE ALSO
		 *   NfcTag.Capacity
		 *
		 **/
		public long ContentSize()
		{
			byte[] bytes = SerializeContent();
			
			if ((bytes == null) || (bytes.Length == 0))
				return 0;
			
			return bytes.Length;
		}


		/**m* SpringCard.NfcForum.Tags/NfcTag.Format
		 *
		 * SYNOPSIS
		 *   public bool Format()
		 * 
		 * DESCRIPTION
		 *   Formats the physical Tag currently on the reader.
		 *   This is only possible if IsFormattable returns true.
		 *   Return true on success.
		 * 
		 * SEE ALSO
		 *   NfcTag.IsFormatted
		 *   NfcTag.IsFormatable
		 *   NfcTag.Write
		 *
		 **/
		public abstract bool Format();

		/**m* SpringCard.NfcForum.Tags/NfcTag.Write
		 *
		 * SYNOPSIS
		 *   public bool Write(bool skip_checks = false)
		 * 
		 * DESCRIPTION
		 *   Writes the new content of the NfcTag object to the physical
		 *   Tag currently on the reader.
		 * 
		 *   If parameter skip_checks is true, the function doesn't verify whether
		 *   the Tag is writable or not before trying to write.
		 *   Returns true on success.
		 * 
		 * NOTES
		 *   The Tag must already be formatted. See NfcTag.IsFormatted and NfcTag.Format.
		 *
		 **/
		public bool Write()
		{
			return Write(false);
		}

		public bool Write(bool skip_checks)
		{
			if (!IsFormatted() && !skip_checks)
			{
				Logger.Trace("The Tag is not formatted");
				return false;
			}
			
			if (IsLocked() && !skip_checks)
			{
				Logger.Trace("The Tag is not writable");
				return false;
			}

			byte[] bytes = SerializeContent();
			
			if ((bytes == null) || (bytes.Length == 0))
			{
				Logger.Trace("Nothing to write on the Tag");
				return false;
			}
			
			if (bytes.Length > Capacity())
			{
				Logger.Trace("The size of the content is bigger than the Tag's capacity");
				return false;
			}
			
			Logger.Trace("Writing the Tag...");

			if (!WriteContent(bytes))
			{
				Logger.Trace("Write failed!");
				return false;
			}
			
			Logger.Trace("Write success!");
			return true;
		}
		
		/**m* SpringCard.NfcForum.Tags/NfcTag.Lock
		 *
		 * SYNOPSIS
		 *   public bool Lock()
		 * 
		 * DESCRIPTION
		 *   Sets physical Tag currently on the reader in read-only (locked) state.
		 *   This is only possible if IsLockable returns true.
		 *   Returns true on success.
		 *
		 **/
		public abstract bool Lock();

		protected abstract bool Read();
		
		public byte[] GetUid()
		{
			return GetUid(_channel);
		}
		
		protected static byte[] GetUid(ICardApduTransmitter channel)
		{
			CAPDU capdu = new CAPDU(0xFF, 0xCA, 0x00, 0x00, 0x00);

			Logger.Trace("< " + capdu.AsString(" "));
			
			RAPDU rapdu = null;
			
			rapdu = channel.Transmit(capdu);				
			if (rapdu == null)
			{
				Logger.Trace("Error while getting the card's UID");
				return null;
			}

			Logger.Trace("> " + rapdu.AsString(" "));

			if (rapdu.SW != 0x9000)
			{
				Logger.Trace("Bad status word " + SCARD.CardStatusWordsToString(rapdu.SW) + " while getting the card's UID");
				return null;
			}
			
			if (!rapdu.hasData)
			{
				Logger.Trace("Empty response");
				return null;
			}
			
			return rapdu.data.GetBytes();
		}		
	}
	
}
