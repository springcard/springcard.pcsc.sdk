/*
 * Created by SharpDevelop.
 * User: jerome.i
 * Date: 17/06/2013
 * Time: 09:43
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Threading;
using SpringCard.LibCs;
using SpringCard.PCSC;
using MemoryCardTool;
using System.Windows.Forms;

namespace SpringCardMemoryCard
{
	/// <summary>
	/// Description of MemoryCard_Mifare1k.
	/// </summary>
	public class MemoryCardMifareClassic : MemoryCard
	{
		public class Sector
		{
			/// <summary>
			/// Key A
			/// </summary>
			private byte[] KeyA;
			
			/// <summary>
			/// Key B
			/// </summary>
			private byte[] KeyB;
			
			private byte[] sectorTrailer;
			private byte[] accessBytes;

			private byte[] readingKey;
			private byte[] writingKey;
			
			/// <summary>
			/// Sector's number
			/// </summary>
			public int Number { get; private set; }
			
			/// <summary>
			/// Blocks inside the sector
			/// </summary>
			public Dictionary<int, byte[]> Blocks;
			
			/// <summary>
			/// Create a new empty block from its number
			/// </summary>
			/// <param name="Number"></param>
			public Sector(int Number)
			{
				this.Number = Number;
				Blocks = new Dictionary<int, byte[]>();
			}
			
			public byte[] ReadingKey
			{
				get
				{
					return readingKey;
				}
				set
				{
					if(!isValidKeyLength(value))
						throw new ArgumentException("Can't set reading key invalid length");
					this.readingKey = value;
				}
			}
			
			public byte[] WritingKey
			{
				get
				{
					return writingKey;
				}
				set
				{
					if(!isValidKeyLength(value))
						throw new ArgumentException("Can't set writing key invalid length");
					this.writingKey= value;
				}
			}
			
			public bool isSetReadingKey()
			{
				return ReadingKey != null;
			}
			
			public bool isSetWritingKey()
			{
				return WritingKey != null;
			}
			
			public bool setReadingKeyFromString(string key)
			{
				if(!isKeyValid(key)) 
					return false;
				
				CardBuffer bkey = new CardBuffer(key.Trim());
				readingKey = bkey.GetBytes();
				return true;
			}
			
			public bool setWritingKeyFromString(string key)
			{
				if(!isKeyValid(key))
					return false;
				CardBuffer bkey = new CardBuffer(key.Trim());
				writingKey = bkey.GetBytes();
				return true;				
			}
		
			/// <summary>
			/// Get length of all sector's blocks according to the block's number
			/// </summary>
			public int Length
			{
				get
				{
					return (Number < 32) ? 64 : 256;
				}
			}
			
			/// <summary>
			/// Returns blocks count per sector according to the current sector's number
			/// </summary>
			public int BlockCount
			{
				get
				{
					return (Number < 32) ? 4 : 16;
				}
			}			
			
			public byte[] AccessBits 
			{
				set
				{
					if(value.Length != 4) {
						throw new ArgumentException("Can't set Sector Trailer: invalid length");
					}
					this.accessBytes = value;
				}
				get
				{
					return this.accessBytes;
				}
			}
			
			public byte[] SectorTrailer 
			{
				set 
				{
					if (value.Length != 16) {
						throw new ArgumentException("Can't set Sector Trailer: invalid length");
					}
					this.sectorTrailer = value;
				}
				get
				{
					return this.sectorTrailer;
				}
			}
			
			public bool SetSectorTrailer(byte[] data) 
			{
				if ((data == null) || (data.Length != 16))
					return false;
				this.sectorTrailer = data;
				return true;
			}

			/// <summary>
			/// Set ALL block's data
			/// </summary>
			/// <param name="address"></param>
			/// <param name="data"></param>
			public void SetBlock(int address, byte[] data)
			{
				int block;
				block = (address < 128) ? (address % 4) : ((address - 128) % 16);
				Logger.Trace("Block = " + block);
				Blocks.Add(block, data);
			}
			
			/// <summary>
			/// Returns content of all blocks of a sector (i.e 48 bytes)
			/// </summary>
			/// <returns></returns>
			public byte[] GetBytes()
			{
				int data_block_count = BlockCount - 1;	// 3
				byte[] result = new byte[16 * data_block_count];
				for (int i=0; i<data_block_count; i++)
				{
					if (Blocks.Count > i)
					{
						if (Blocks[i] != null)
						{
							Array.Copy(Blocks[i], 0, result, 16 * i, 16);
						}
					}
				}				
				return result;
			}
			
			/// <summary>
			/// Validate the form and content of a Key
			/// </summary>
			/// <param name="data"></param>
			/// <param name="nbBytes"></param>
			/// <returns></returns>
			public static bool isKeyValid(string data, int nbBytes = 6)
			{
				if (data.Length != (nbBytes * 2))
					return false;
				
				for (int i = 0; i < data.Length; i++)
					if (!BinConvert.isByte(data[i]))
						return false;
				
				return true;
			}
			
			/// <summary>
			/// Validate the length of a key
			/// </summary>
			/// <param name="key"></param>
			/// <returns></returns>
			private bool isValidKeyLength(byte[] key)
			{
				return (key.Length != 6) ? false : true;
			}
			
			/// <summary>
			/// Set sector's A key
			/// </summary>
			/// <param name="key"></param>
			/// <returns></returns>
			public bool setKeyA(byte[] key)
			{
				Logger.Trace("Setting Key A");
				if(!isValidKeyLength(key)) {
					Logger.Trace("Incorrect length for Key A");
					return false;
				}
				KeyA = key;
				return true;
			}
			
			public bool setKeyA(string skey)
			{
				Logger.Trace("Setting Key A");
				CardBuffer bkey = new CardBuffer(skey.Trim());
				return setKeyA(bkey.GetBytes());
			}			
			
			public byte[] getKeyA()
			{
				return KeyA;
			}
			
			public bool isSetKeyA()
			{
				return KeyA != null;
			}
			
			public byte[] getKeyB()
			{
				return KeyB;
			}
			
			public bool isSetKeyB()
			{
				return KeyB != null;
			}			
		
			/// <summary>
			/// Set sector's B key
			/// </summary>
			/// <param name="key"></param>
			/// <returns></returns>
			public bool setKeyB(byte[] key)
			{
				Logger.Trace("Setting Key B");
				if(!isValidKeyLength(key)) {
					Logger.Trace("Incorrect length for Key B");
					return false;
				}
				KeyB = key;
				return true;
			}
			
			public bool setKeyB(string skey)
			{
				Logger.Trace("Setting Key B");
				CardBuffer bkey = new CardBuffer(skey.Trim());
				return setKeyB(bkey.GetBytes());				
			}
		}	/* Sector */

		
		public Dictionary<int, Sector> Sectors { get; private set; }
		
		public int SectorCount
		{
			get
			{
				if (MaxByteCount < 128)
				{
					return MaxByteCount / 64;
				}
				else
				{
					return 32 + ((MaxByteCount - 128) / 256);
				}
			}				
		}
		
		public MemoryCardMifareClassic(MemoryCard InheritFrom)
		{
			this.Channel = InheritFrom.Channel;
			this.Atr = InheritFrom.Atr;
			this.CardType = InheritFrom.CardType;
			this.PixSS = InheritFrom.PixSS;
			this.PixNN = InheritFrom.PixNN;
		}

		protected byte[] MifareClassicRead(ushort blockNumber, byte dataLength = 0, byte[] keyValue = null)
		{
			CAPDU capdu = null;
			if(keyValue == null) {	// Reading without key
				capdu = new CAPDU(0xFF, 0xF3, (byte) (blockNumber / 0x0100), (byte) (blockNumber % 0x0100), dataLength);	
			} else {				// Reading with key
				capdu = new CAPDU(0xFF, 0xF3, (byte) (blockNumber / 0x0100), (byte) (blockNumber % 0x0100), keyValue, dataLength);
			}
			
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
		
		/// <summary>
		/// Write all sectors to the card
		/// </summary>
		/// <param name="address"></param>
		/// <param name="data"></param>
		/// <returns></returns>
		private bool MifareClassicWrite(ushort address, byte[] data, byte[] key)
		{
			if (data == null)
				return false;
			
			CAPDU capdu = null;
			if(key == null) {
				capdu = new CAPDU(0xFF, 0xF4, (byte)(address / 0x0100), (byte)(address % 0x0100), data);		
			} else {
				// Concat data and key
				byte[] sentData = new byte[data.Length + key.Length];
				System.Buffer.BlockCopy(data, 0, sentData, 0, data.Length);
				System.Buffer.BlockCopy( key, 0, sentData, data.Length, key.Length);
				capdu = new CAPDU(0xFF, 0xF4, (byte)(address / 0x0100), (byte)(address % 0x0100), sentData);
			}
		  
			Logger.Trace("< " + capdu.AsString(" ") + " (writing without specified key)");
			
			RAPDU rapdu = null;
			
			for (int retry = 0; retry < 4; retry++) {
				rapdu = Channel.Transmit(capdu);
				if (rapdu == null)
					break;
				if ((rapdu.SW != 0x6F01) && (rapdu.SW != 0x6F02) && (rapdu.SW != 0x6F0B))
					break;
  			
				Thread.Sleep(15);
			}

			if (rapdu == null) {
				Logger.Trace("Error '" + Channel.LastErrorAsString + "' while writing the card");
				return false;
			}
  		
			Logger.Trace("> " + rapdu.AsString(" "));
			
			if (rapdu.SW != 0x9000) {
				Logger.Trace("Bad status word " + rapdu.SWString + " while writing the card");
				return false;
			}
			
			return true;
		}
		
		
		public static int AddressToSector(int address)
		{
			if (address < 128)
			{
				return address / 4;
			}
			else
			{
				return 32 + (address - 128) / 16;
			}
		}
		
		public static int SectorFirstBlockAddress(int sectorNumber)
		{
			if (sectorNumber < 32)
				return sectorNumber * 4;
			else
				return (sectorNumber * 16) + 128;
		}
		
		public static int SectorTrailerAddress(int sectorNumber)
		{
			if (sectorNumber < 32)
				return SectorFirstBlockAddress(sectorNumber) + 3;
			else
				return SectorFirstBlockAddress(sectorNumber) + 15;
		}
		
		/// <summary>
		/// Read all card's content
		/// </summary>
		/// <returns></returns>
		public override bool Read()
		{
			Sectors = new Dictionary<int, Sector>();
			Pages = new Dictionary<int, byte[]>();
			ReadByteCount = 0;
			
			int max_address = MaxByteCount / 16;
			
			for (int address=0; address<max_address; address++)
			{
				byte[] data = MifareClassicRead((ushort) address, 16);
				if (data != null)
				{
					Logger.Trace("Adding block " + address);
					Pages.Add(address, data);
					
					int sector = AddressToSector(address);
					if (!Sectors.ContainsKey(sector))
					{
						Logger.Trace("Adding new sector " + sector);
						Sectors[sector] = new Sector(sector);
					}
					Logger.Trace("Adding block to sector " + sector);
					Sectors[sector].SetBlock(address, data);
					ReadByteCount += data.Length;
				} else {
					Pages.Add(address, null);
					int sector = AddressToSector(address);
					Sectors[sector] = null;
				}
			}			
			return (ReadByteCount > 0);
		}

		/// <summary>
		/// Write a sector (excluding the trailer sector)
		/// </summary>
		/// <param name="sector"></param>
		/// <returns></returns>
		public bool WriteSector(Sector sector)
		{
			int firstBlockAddress = SectorFirstBlockAddress(sector.Number);
			int lastBlockAddress = SectorTrailerAddress(sector.Number) - 1;
			
			int startingAddress = 0;
			byte[] sectorData = sector.GetBytes();
			byte[] key = null;
			
			Logger.Trace("Content to be written: " + BinConvert.ToHex(sectorData) + ", sector number: "+sector.Number);
			for (int address = firstBlockAddress; address <= lastBlockAddress; address++)
			{
				byte[] data = new byte[16];
				Array.Copy(sectorData, startingAddress, data, 0, 16);
				startingAddress += 16;
				Logger.Trace("Written content: " + BinConvert.ToHex(data));
				
				if(sector.isSetWritingKey())
					key = sector.WritingKey;
				
				if(!MifareClassicWrite((ushort) address, data, key)) {
					return false;
				}
			}
			return true;
		}
		
		/// <summary>
		/// Try to read a sector with a specified key
		/// </summary>
		/// <param name="sectorNumber">Sector's number</param>
		/// <param name="key">Key used to read</param>
		/// <returns></returns>
		private MemoryCardMifareClassic.Sector ReadSectorWithKey(int sectorNumber, byte[] key)
		{
			MemoryCardMifareClassic.Sector sector = new Sector(sectorNumber);
			int firstBlockAddress = MemoryCardMifareClassic.SectorFirstBlockAddress(sectorNumber);
			int lastBlockAddress = MemoryCardMifareClassic.SectorTrailerAddress(sectorNumber);
			int counter = 0;
			
			for(ushort blockAddress = (ushort) firstBlockAddress; blockAddress < (ushort) lastBlockAddress; blockAddress++) 
			{
				byte[] data = MifareClassicRead(blockAddress, 16, key);
				if (data != null)
				{
					sector.SetBlock(counter, data);
					counter++;
				} else {
					return null;
				}
			}
			return sector;
		}
		
		/// <summary>
		/// Try to read a specified sector with keys
		/// </summary>
		/// <param name="sectorToRead"></param>
		/// <returns></returns>
		public MemoryCardMifareClassic.Sector ReadSectorWithKey(MemoryCardMifareClassic.Sector sectorToRead)
		{
			MemoryCardMifareClassic.Sector newSector = null;
			
			if(sectorToRead.isSetReadingKey()) {
				newSector = ReadSectorWithKey(sectorToRead.Number, sectorToRead.ReadingKey);
				if(newSector != null) {
					newSector.ReadingKey = sectorToRead.ReadingKey;
				}
			}
			return newSector;
		}


		/// <summary>
		/// Write a sector's trailer
		/// </summary>
		/// <param name="sector"></param>
		/// <returns></returns>
		public bool WriteSectorTrailer(Sector sector)
		{
			if(sector == null)
				return false;
			
			int blockAddress = SectorTrailerAddress(sector.Number);
			byte[] sectorTrailerData = sector.SectorTrailer;
			byte[] key = null;
			
			Logger.Trace("WriteSectorTrailer: " + BinConvert.ToHex(sectorTrailerData) + ", sector number: "+sector.Number);
			if(sector.isSetWritingKey())
				key = sector.WritingKey;
				
			return MifareClassicWrite((ushort) blockAddress, sectorTrailerData, key);
		}
		
		/// <summary>
		/// Read a sector trailer with provided keys
		/// </summary>
		/// <param name="sector"></param>
		/// <returns></returns>
		public bool ReadSectorTrailer(ref Sector sector)
		{
			byte[] data = null;
			ushort blockAddress = (ushort) MemoryCardMifareClassic.SectorTrailerAddress(sector.Number);
			if(sector.isSetKeyA()) {
				data = MifareClassicRead(blockAddress, 16, sector.getKeyA());
				if (data == null) {
					if(sector.isSetKeyB())
						data = MifareClassicRead(blockAddress, 16, sector.getKeyB());
				}
			} else {
				if(sector.isSetKeyB())
					data = MifareClassicRead(blockAddress, 16, sector.getKeyB());
			}
			
			if(data == null)
				return false;
			
			// Set access bits
			byte[] accessBytes = new byte[4];
			Array.ConstrainedCopy(data,6, accessBytes, 0, 4);
			sector.AccessBits = accessBytes;
			return true;
		}		
	}
}
