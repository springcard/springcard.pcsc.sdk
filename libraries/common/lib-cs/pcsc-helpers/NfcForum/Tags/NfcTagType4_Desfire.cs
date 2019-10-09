/*
 * Crée par SharpDevelop.
 * Utilisateur: johann
 * Date: 12/03/2012
 * Heure: 09:22
 * 
 * Pour changer ce modèle utiliser Outils | Options | Codage | Editer les en-têtes standards.
 */
using System;
using System.Diagnostics;
using SpringCard.PCSC;

#if (NFC_TAGS_DESFIRE)

namespace SpringCard.NfcForum.Tags
{
	/// <summary>
	/// Description of NfcTagType4Desfire.
	/// </summary>
	public class NfcTagType4Desfire : NfcTagType4
	{
		public static new bool Recognize(SCardChannel channel)
		{
			bool write_protected = false;
			ushort max_le = 0;
			ushort max_lc = 0;
			ushort ndef_file_id = 0;
			long ndef_file_size = 0;
			
			if (!IsDesfireEV1(channel))
			{
				Logger.Trace("The card is not a Desfire EV1");
				return false;
			}
			
			if (!NfcTagType4.Recognize(channel, ref write_protected, ref max_le, ref max_lc, ref ndef_file_id, ref ndef_file_size))
			{
				/* Failed to recognize a Type 4 card, but anyway a Desfire EV1 may be formatted later on */
				Logger.Trace("The card is a Desfire EV1, it may become a type 4 Tag");
			} else
			{
				Logger.Trace("The card is a Desfire EV1, already formatted as type 4 Tag");
			}
			return true;
		}

		public static bool IsDesfireEV1(SCardChannel channel)
		{
			bool is_desfire_ev1 = false;
			
			CAPDU capdu = new CAPDU(0x90, 0x60, 0x00, 0x00, 0x00);
			
			Logger.Trace("< " + capdu.AsString(" "));

			RAPDU rapdu = channel.Transmit(capdu);
			Logger.Trace("> " + rapdu.AsString(" "));
			
			if (rapdu.SW != 0x91AF)
			{
				Logger.Trace("Desfire GetVersion function failed");
				return false;
			}
			
			if (rapdu.GetByte(3) > 0)
			{
				Logger.Trace("This is a Desfire EV1");
				is_desfire_ev1 = true;
			} else
			{
				Logger.Trace("This is a Desfire EV0");
			}
			
			capdu = new CAPDU(0x90, 0xAF, 0x00, 0x00, 0x00);
			
			Logger.Trace("< " + capdu.AsString(" "));

			rapdu = channel.Transmit(capdu);
			Logger.Trace("> " + rapdu.AsString(" "));
			
			if (rapdu.SW != 0x91AF)
			{
				Logger.Trace("Desfire GetVersion(2) function failed");
				return false;
			}

			capdu = new CAPDU(0x90, 0xAF, 0x00, 0x00, 0x00);
			
			Logger.Trace("< " + capdu.AsString(" "));

			rapdu = channel.Transmit(capdu);
			Logger.Trace("> " + rapdu.AsString(" "));
			
			if (rapdu.SW != 0x9100)
			{
				Logger.Trace("Desfire GetVersion(3) function failed");
				return false;
			}

			return is_desfire_ev1;
		}
		
		public NfcTagType4Desfire(SCardChannel Channel) : base(Channel)
		{
			_formattable = true;
		}

		public static NfcTagType4Desfire Read(SCardChannel channel)
		{
			NfcTagType4Desfire t = new NfcTagType4Desfire(channel);
			
			t.Read();
			
			return t;
		}
	}
}

#endif