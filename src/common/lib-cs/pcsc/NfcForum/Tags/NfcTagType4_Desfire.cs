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

namespace SpringCard.NFC
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
				Trace.WriteLine("The card is not a Desfire EV1");
				return false;
			}
			
			if (!NfcTagType4.Recognize(channel, ref write_protected, ref max_le, ref max_lc, ref ndef_file_id, ref ndef_file_size))
			{
				/* Failed to recognize a Type 4 card, but anyway a Desfire EV1 may be formatted later on */
				Trace.WriteLine("The card is a Desfire EV1, it may become a type 4 Tag");
			} else
			{
				Trace.WriteLine("The card is a Desfire EV1, already formatted as type 4 Tag");
			}
			return true;
		}

		public static bool IsDesfireEV1(SCardChannel channel)
		{
			bool is_desfire_ev1 = false;
			
			CAPDU capdu = new CAPDU(0x90, 0x60, 0x00, 0x00, 0x00);
			
			Trace.WriteLine("< " + capdu.AsString(" "));

			RAPDU rapdu = channel.Transmit(capdu);
			Trace.WriteLine("> " + rapdu.AsString(" "));
			
			if (rapdu.SW != 0x91AF)
			{
				Trace.WriteLine("Desfire GetVersion function failed");
				return false;
			}
			
			if (rapdu.GetByte(3) > 0)
			{
				Trace.WriteLine("This is a Desfire EV1");
				is_desfire_ev1 = true;
			} else
			{
				Trace.WriteLine("This is a Desfire EV0");
			}
			
			capdu = new CAPDU(0x90, 0xAF, 0x00, 0x00, 0x00);
			
			Trace.WriteLine("< " + capdu.AsString(" "));

			rapdu = channel.Transmit(capdu);
			Trace.WriteLine("> " + rapdu.AsString(" "));
			
			if (rapdu.SW != 0x91AF)
			{
				Trace.WriteLine("Desfire GetVersion(2) function failed");
				return false;
			}

			capdu = new CAPDU(0x90, 0xAF, 0x00, 0x00, 0x00);
			
			Trace.WriteLine("< " + capdu.AsString(" "));

			rapdu = channel.Transmit(capdu);
			Trace.WriteLine("> " + rapdu.AsString(" "));
			
			if (rapdu.SW != 0x9100)
			{
				Trace.WriteLine("Desfire GetVersion(3) function failed");
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