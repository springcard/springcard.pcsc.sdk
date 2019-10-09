/*
 * Created by SharpDevelop.
 * User: jerome.i
 * Date: 14/06/2013
 * Time: 16:20
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using SpringCard.PCSC;
using SpringCard.LibCs;
using MemoryCardTool;
using System.Windows.Forms;

namespace SpringCardMemoryCard
{
	/// <summary>
	/// Description of MemoryCard.
	/// </summary>
	public class MemoryCard
	{
		public static readonly byte[] MemoryCardAtrPrefix = new byte[] {
			0x3B,
			0x8F,
			0x80,
			0x01,
			0x80,
			0x4F,
			0x0C,
			0xA0,
			0x00,
			0x00,
			0x03,
			0x06
		};
		
		public enum MemoryCardType
		{
			MIFARE_CLASSIC_1K = 0x0001,
			MIFARE_CLASSIC_4K = 0x0002,
			MIFARE_UL = 0x0003,
			ST_MICRO_ST176 = 0x0006,
			ST_MICRO_OTHER_SR =	0x0007,
			ATMEL_AT88SC0808CRF = 0x000A,
			ATMEL_AT88SC1616CRF = 0x000B,
			ATMEL_AT88SC3216CRF = 0x000C,
			ATMEL_AT88SC6416CRF = 0x000D,
			TEXAS_TAGIT = 0x0012,
			ST_MICRO_LRI512 = 0x0013,
			NXP_ICODE_SLI = 0x0014,
			NXP_ICODE1 = 0x0016,
			ST_MICRO_LRI64 = 0x0021,
			ST_MICRO_LR12 = 0x0024,
			ST_MICRO_LRI128 = 0x0025,
			MIFARE_MINI = 0x0026,
			INNOVISION_JEWEL = 0x002F,
			INNOVISION_TOPAZ = 0x0030,
			ATMEL_AT88RF04C = 0x0034,
			NXP_ICODE_SL2 = 0x0035,
			MIFARE_UL_C = 0x003A,
			GENERIC_14443_A = 0xFFA0,
			GENERIC_14443_B = 0xFFB0,
			ASK_CTS_256B = 0xFFB1,
			ASK_CTS_512B = 0xFFB2,
			INSIDE_CONTACTLESS = 0xFFB7,
			UNIDENTIFIED_ATMEL = 0xFFB8,
			CALYPSO_INNOVATRON = 0xFFC0,
			UNIDENTIFIED_ISO15693 = 0xFFD0,
			UNIDENTIFIED_EMMARIN = 0xFFD1,
			UNIDENTIFIED_ST_MICRO = 0xFFD2,
			UNKNOWN = 0xFFFF
		}

		public SCardChannel Channel { get; protected set; }
		public byte[] Atr { get; protected set; }
		public MemoryCardType CardType { get; protected set; }
		public byte PixSS { get; protected set; }
		public byte[] PixNN { get; protected set; }

		public string CardName { get; private set; }
		public string CardUID { get; private set; }
		
		public int BytesPerRead { get; private set; }
		public int BytesPerWrite { get; private set; }
		public int MaxByteCount { get; private set; }
		
		public int LastAddressRead { get; private set; }
		public int PageSize { get; private set; }
		
		public Dictionary<int, byte[]> Pages { get; protected set; }
		public List<int> UpdatedAddresses { get; protected set; }
		
		public int ReadByteCount { get; protected set; }		
		
		protected int max_address = 0xFFFF;
		
			
		private static MemoryCard Create(SCardChannel Channel, byte[] Atr)
		{
			if (Atr == null)
				return null; /* ATR error */
			if (Atr.Length < MemoryCardAtrPrefix.Length + 3) 
				return null; /* ATR too short for a memory card */
			
			for (int i = 0; i < MemoryCardAtrPrefix.Length; i++)
				if (Atr[i] != MemoryCardAtrPrefix[i])
					return null; /* ATR doesn't denote a memory card */
			
			Logger.Trace("From the ATR, this seems to be a Memory Card");
			
			MemoryCard Card = new MemoryCard();
			
			Card.Channel = Channel;
			
			Card.Atr = Atr;
			
			Card.PixSS = Card.Atr[MemoryCardAtrPrefix.Length];
			
			Card.PixNN = new byte[2];
			Card.PixNN[0] = Card.Atr[MemoryCardAtrPrefix.Length + 1];
			Card.PixNN[1] = Card.Atr[MemoryCardAtrPrefix.Length + 2];
			
			ushort wPixNN = (ushort)((Card.PixNN[0] * 0x0100) | Card.PixNN[1]);
			
			Logger.Trace("PIX.SS=" + BinConvert.ToHex(Card.PixSS) + ", PIX.NN=" + BinConvert.ToHex(Card.PixNN));
			try {
				Card.CardType = (MemoryCardType) wPixNN;
			} catch {
				Card.CardType = MemoryCardType.UNKNOWN;
				Logger.Trace("This card is not listed");
			}
			
			switch (Card.CardType) {
				case MemoryCardType.MIFARE_CLASSIC_1K:
				case MemoryCardType.MIFARE_CLASSIC_4K:
				case MemoryCardType.MIFARE_MINI:
					Logger.Trace("Creating a Mifare Classic child object");
					Card = new MemoryCardMifareClassic(Card);
					break;
				default :
					Logger.Trace("(This is not Mifare Classic type)");
					break;
			}
			
			switch (Card.CardType) {
				case MemoryCardType.MIFARE_CLASSIC_1K:
					Card.CardName = "Mifare Classic 1K";
					Card.BytesPerWrite = Card.BytesPerRead = 16;
					Card.MaxByteCount = 16 * 64;
					break;
				case MemoryCardType.MIFARE_CLASSIC_4K:
					Card.CardName = "Mifare Classic 4K";
					Card.BytesPerWrite = Card.BytesPerRead = 16;
					Card.MaxByteCount = 16 * 256;
					break;
				case MemoryCardType.MIFARE_UL:
					Card.CardName = "Mifare UltraLight";
					Card.BytesPerRead = 16;
					Card.BytesPerWrite = 4;
					break;
				case MemoryCardType.ST_MICRO_ST176:
					Card.CardName = "ST MicroElectronics SR176";
					Card.BytesPerWrite = Card.BytesPerRead = 2;
					Card.MaxByteCount = 16 * 2;
					break;
				case MemoryCardType.ST_MICRO_OTHER_SR:
					Card.CardName = "ST MicroElectronics SRI4K, SRIX4K, SRIX512, SRI512 or SRT512";
					Card.BytesPerWrite = Card.BytesPerRead = 4;
					break;
					
				case MemoryCardType.ATMEL_AT88SC0808CRF:
				case MemoryCardType.ATMEL_AT88SC1616CRF:
				case MemoryCardType.ATMEL_AT88SC3216CRF:
				case MemoryCardType.ATMEL_AT88SC6416CRF:
					Card.CardName = "Atmel AT88 'CryptoRF'";
					break;
					
				case MemoryCardType.TEXAS_TAGIT:
					Card.CardName = "Texas Instruments TAG IT ";
					Card.BytesPerWrite = Card.BytesPerRead = 4;
					break;
					
				case MemoryCardType.ST_MICRO_LRI512:
					Card.CardName = "ST MicroElectronics LRI512";
					break;
					
				case MemoryCardType.NXP_ICODE_SLI:
					Card.CardName = "NXP ICODE SLI";
					Card.BytesPerWrite = Card.BytesPerRead = 4;
					break;
					
				case MemoryCardType.NXP_ICODE1:
					Card.CardName = "NXP ICODE1";
					Card.BytesPerWrite = Card.BytesPerRead = 4;
					Card.MaxByteCount = 16 * 4;
					break;
					
				case MemoryCardType.ST_MICRO_LRI64:
					Card.CardName = "ST MicroElectronics LRI64";
					break;
					
				case MemoryCardType.ST_MICRO_LR12:
					Card.CardName = "ST MicroElectronics LR12";
					break;
					
				case MemoryCardType.ST_MICRO_LRI128:
					Card.CardName = "ST MicroElectronics LRI128";
					break;
					
				case MemoryCardType.MIFARE_MINI:
					Card.CardName = "Mifare Classic Mini";
					Card.BytesPerWrite = Card.BytesPerRead = 16;
					Card.MaxByteCount = 24 * 16;
					break;
					
				case MemoryCardType.INNOVISION_JEWEL:
					Card.CardName = "Broadcom Jewel";
					Card.BytesPerRead = 128;
					Card.BytesPerWrite = 1;
					Card.MaxByteCount = 128;
					break;
					
				case MemoryCardType.INNOVISION_TOPAZ:
					Card.CardName = "Broadcom Topaz";
					Card.BytesPerRead = 128;
					Card.BytesPerWrite = 1;
					Card.MaxByteCount = 128;
					break;
					
				case MemoryCardType.ATMEL_AT88RF04C:
					Card.CardName = "Atmel AT88RF04C";
					break;
					
				case MemoryCardType.NXP_ICODE_SL2:
					Card.CardName = "NXP ICODE SL2";
					break;
					
				case MemoryCardType.MIFARE_UL_C:
					Card.CardName = "Mifare UltraLight C";
					Card.BytesPerRead = 16;
					Card.BytesPerWrite = 4;
					break;
					
				case MemoryCardType.ASK_CTS_256B:
					Card.CardName = "ASK CTS 256B";
					Card.BytesPerRead = Card.BytesPerWrite = 2;
					Card.MaxByteCount = 16 * 2;
					break;

				case MemoryCardType.ASK_CTS_512B:
					Card.CardName = "ASK CTS 512B";
					Card.BytesPerRead = Card.BytesPerWrite = 2;
					Card.MaxByteCount = 32 * 2;
					break;
					
				case MemoryCardType.INSIDE_CONTACTLESS:
					Card.CardName = "Inside PicoTag / HID iClass";
					Card.BytesPerRead = Card.BytesPerWrite = 8;
					Card.max_address = 0x21;
					break;
					
				case MemoryCardType.UNIDENTIFIED_EMMARIN:
					Card.CardName = "EM-Marin / Legic ISO 15693";
					Card.BytesPerRead = Card.BytesPerWrite = 8;
					break;
					
					default :
						break;
			}
			
			if (Card.CardName != null)
				Logger.Trace("Card name: " + Card.CardName);

			Card.CardUID = BinConvert.ToHex(Card.ReadUID());
			
			if (Card.CardUID != null)
				Logger.Trace("Card UID: " + Card.CardUID);
			
			return Card;
		}
		
		public static MemoryCard Create(SCardChannel Channel)
		{
			return Create(Channel, Channel.CardAtr.GetBytes());
		}
		
		protected byte[] ReadUID()
		{
			CAPDU capdu = new CAPDU(0xFF, 0xCA, 0x00, 0x00, 0x00);

			Logger.Trace("< " + capdu.AsString(" "));
			
			RAPDU rapdu = null;
			
			rapdu = Channel.Transmit(capdu);
			
			if (rapdu == null)
			{
				Logger.Trace("Error '" + Channel.LastErrorAsString + "' while reading the UID");
				return null;
			}

			Logger.Trace("> " + rapdu.AsString(" "));
			
			if (rapdu.SW != 0x9000)
			{
				Logger.Trace("Bad status word " + rapdu.SWString + " while reading the UID");
				return null;
			}
			
			if (!rapdu.hasData)
			{
				Logger.Trace("Empty response");
				return null;
			}
			
			return rapdu.data.GetBytes();			
		}

		protected byte[] ReadBinary(ushort P1P2, byte Le = 0)
		{
			CAPDU capdu = new CAPDU(0xFF, 0xB0, (byte) (P1P2 / 0x0100), (byte) (P1P2 % 0x0100), Le);

			Logger.Trace("< " + capdu.AsString(" "));
			
			RAPDU rapdu = null;
			
			for (int retry = 0; retry < 4; retry++)
			{
				rapdu = Channel.Transmit(capdu);
				
				if (rapdu == null)
					break;
				if ((rapdu.SW != 0x6F01) && (rapdu.SW != 0x6F02) && (rapdu.SW != 0x6F0B))
					break;
			}
			
			if (rapdu == null)
			{
				Logger.Trace("Error '" + Channel.LastErrorAsString + "' while reading the card");
				return null;
			}

			Logger.Trace("> " + rapdu.AsString(" "));
			
			if (rapdu.SW != 0x9000)
			{
				Logger.Trace("Bad status word " + rapdu.SWString + " while reading the card");
				return null;
			}
			
			if (!rapdu.hasData)
			{
				Logger.Trace("Empty response");
				return null;
			}
			
			return rapdu.data.GetBytes();
		}
		
		protected bool WriteBinary(ushort P1P1, byte[] Data)
		{
			if (Data == null)
				return false;
			
			CAPDU capdu = new CAPDU(0xFF, 0xD6, (byte) (P1P1 / 0x0100), (byte) (P1P1 % 0x0100), Data);
			
			Logger.Trace("< " + capdu.AsString(" "));
			
			RAPDU rapdu = null;
			
			for (int retry = 0; retry < 4; retry++)
			{
				rapdu = Channel.Transmit(capdu);
				
				if (rapdu == null)
					break;
				if ((rapdu.SW != 0x6F01) && (rapdu.SW != 0x6F02) && (rapdu.SW != 0x6F0B))
					break;
			}

			if (rapdu == null)
			{
				Logger.Trace("Error '" + Channel.LastErrorAsString + "' while writing the card");
				return false;
			}
			
			Logger.Trace("> " + rapdu.AsString(" "));
			
			if (rapdu.SW != 0x9000)
			{
				Logger.Trace("Bad status word " + rapdu.SWString + " while writing the card");				
				return false;
			}
			
			return true;
		}
				
		public virtual bool Read()
		{
			LastAddressRead = 0;
			PageSize = 0;
			ReadByteCount = 0;
			Pages = new Dictionary<int, byte[]>();
			
			//int max_address = 0xFFFF;
			
			for (int address=0; address < this.max_address; address++)
			{
				byte[] data = ReadBinary((ushort) address, (byte) PageSize);
				
				if (data == null)
					break;
				
				LastAddressRead = address;				
				
				if (PageSize == 0)
				{
					PageSize = data.Length;
				}
				else if (PageSize != data.Length)
				{
					Logger.Trace("The page size should be a constant among all the card's pages");
					return false;
				}
				
				Pages.Add(address, data);
				
				ReadByteCount += data.Length;
				if ((MaxByteCount > 0) && (ReadByteCount >= MaxByteCount))
					break;
			}
			
			return (ReadByteCount > 0);
		}
		
		public virtual bool WriteUpdates()
		{
			if (UpdatedAddresses == null)
				return true;
			
			List<int> failedUpdates = new List<int>();
			
			/* Write all the pages that have been modified */
			foreach (int address in UpdatedAddresses)
			{
				if (Pages.ContainsKey(address))
				{
					byte[] data = Pages[address];
					if (!WriteBinary((ushort) address, data))
						failedUpdates.Add(address);
				}
			}
			
			if (failedUpdates.Count > 0)
			{
				/* Remember the updates that are still to be done */
				UpdatedAddresses = failedUpdates;
				return false;
			}
			
			/* No more updates */
			UpdatedAddresses = null;
			return true;
		}
		
		public virtual void CancelUpdates()
		{
			UpdatedAddresses = null;
		}
		
		public virtual bool SetData(int address, byte[] data)
		{
			if ((data == null) || (address < 0) || (PageSize <= 0))
				return false;
			
			bool updated = false;
			
			if (!Pages.ContainsKey(address))
			{
				updated = true;
			}
			else
			{
				byte[] old_data = Pages[address];
				
				Logger.Trace("Page " + address + ": current value is " +  BinConvert.ToHex(old_data));
				
				if (old_data.Length != data.Length)
					return false;
				for (int i=0; i<old_data.Length; i++)
					if (old_data[i] != data[i])
						updated = true;
			}
			
			if (updated)
			{
				Logger.Trace("New content for page " + address + ": " + BinConvert.ToHex(data));
				
				Pages[address] = data;
				if (UpdatedAddresses == null)
					UpdatedAddresses = new List<int>();
				UpdatedAddresses.Add(address);
			}
			
			return true;
		}
		
		public virtual bool SetData(byte[] cardData)
		{
			if ((cardData == null) || (PageSize <= 0))
				return false;
			
			Logger.Trace("New data: " + BinConvert.ToHex(cardData));
			
			int address = 0;
			
			for (int i=0; i<cardData.Length; i+= PageSize)
			{
				byte[] pageData = new byte[PageSize];
				
				Array.Copy(cardData, i, pageData, 0, PageSize);
				
				Logger.Trace("Page " + address + ": new value is " +  BinConvert.ToHex(pageData));
				
				if (!SetData(address, pageData))
					return false;
				
				address++;
			}
			
			return true;
		}		 
		
		public virtual byte[] GetData(int address)
		{
			if (Pages.ContainsKey(address))
				return Pages[address];
				
			if (address <= LastAddressRead)
				return new byte[PageSize];
			
			return null;
		}
	}
}
