/*
 * Created by SharpDevelop.
 * User: jerome.i
 * Date: 20/06/2013
 * Time: 10:28
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using SpringCard.PCSC;
using MemoryCardTool;
using SpringCard.LibCs;

namespace SpringCardMemoryCard
{
	/// <summary>
	/// Description of MifareSector.
	/// </summary>
	public class MifareClassicSector___DEPRECATED
	{
		private List<byte[]> blocks;
		private List<byte[]> new_blocks;
		private byte[] keyA;
		private byte[] keyB;
		private byte[] access_bits;
		private byte[] sector_trailer;

		public int Sector { get; private set; }
		private MemoryCardMifareClassic Card;
		private byte[] Data;

		public MifareClassicSector___DEPRECATED(MemoryCardMifareClassic Card, int Sector)
		{
			this.Card = Card;			
			this.Sector = Sector;
			
			if (Sector < 32)
			{
				Data =  new byte[48];
			} else
			{
				Data =  new byte[240];
			}			
		}
	
				
		private void PopulateBlocks()
		{			
			if (Data != null) {
				blocks = new List<byte[]>();
				byte[] bloc;
				if (Data.Length == 0x40) {
					for (int i = 0; i < 0x30; i += 0x10) {
						bloc = new byte[16];
						Array.ConstrainedCopy(Data, i, bloc, 0, 16);
						blocks.Add(bloc);
					}
					access_bits = new byte[4];
					Array.ConstrainedCopy(Data, 3 * 16 + 6, access_bits, 0, 4);
					
				} else if (Data.Length == 0x100) {
					for (int i = 0; i < 0xF0; i += 0x10) {
						bloc = new byte[16];
						Array.ConstrainedCopy(Data, i, bloc, 0, 16);
						blocks.Add(bloc);						
					}
					access_bits = new byte[4];
					Array.ConstrainedCopy(Data, 15 * 16 + 6, access_bits, 0, 4);
				}
			}
		}
		
		public List<byte[]> Blocks {
			get {
				return blocks;
			}
		}
	
		public byte[] AccessBits {
			get {
				return access_bits;	
			}
		}
	
		public void SetNewBlocks(List<byte[]> _new_blocks)
		{
			Logger.Trace("Setting new blocks ...");
			new_blocks = new List<byte[]>();
			new_blocks = _new_blocks;
		}
		
		public bool SetSectorTrailer(byte[] _sector_trailer)
		{
			Logger.Trace("Setting sector trailer ...");
			if (_sector_trailer.Length == 16) {
				sector_trailer = _sector_trailer;
				return true;
			} else {
				Logger.Trace("Can't set Sector Trailer: invalid length");
				return false;
			}
			
		}
		
		public bool WriteSectorTrailer()
		{
			Logger.Trace("Writing sector trailer ...");
			if (sector_trailer == null) {
				Logger.Trace("Sector trailer is null");
				return false;
			}
			
			if (sector_trailer.Length != 16) {
				Logger.Trace("Sector trailer length != 16");
				return false;
			}
			
			ushort address;		
			byte[] _keyA = new byte[6];
			byte[] _keyB = new byte[6];
			byte[] _access_bits = new Byte[4];
			
			if (Sector < 32) {
				address = (ushort)(Sector * 4 + 3);
			} else {
				address = (ushort)(32 * 4 + (Sector - 32) * 16 + 15);
			}
			
			for (int i = 0; i < 6; i++) {
				if (i < 4)
					_access_bits[i] = sector_trailer[i + 6];
				
				_keyA[i] = sector_trailer[i];
				_keyB[i] = sector_trailer[i + 10];
			}		
			/*
			 * @todo remettre en place et corriger
			if (WriteBinary(address, sector_trailer)) {
				setKeyA(_keyA);
				setKeyB(_keyB);
				setAccessBits(_access_bits);
				return true;
				
			} else if (WriteBinaryWithKey(address, sector_trailer, keyB)) {
				setKeyA(_keyA);
				setKeyB(_keyB);
				setAccessBits(_access_bits);
				return true;
				
			} else if (WriteBinaryWithKey(address, sector_trailer, keyA)) {
				setKeyA(_keyA);
				setKeyB(_keyB);
				setAccessBits(_access_bits);
				return true;
			}
			*/

			return false;
		}
		
		private void GetSectorData(out int address, out byte length)
		{
			if (Sector < 32)
			{
				address = 4 * Sector;
				length = 3 * 16;
			} else
			{
				address = 16 * (Sector - 32) + 128;
				length = 15 * 16;
			}			
		}
		
		public bool ReadBinaryWithoutKey()
		{
			int address;
			byte length;
			GetSectorData(out address, out length);
			
			CAPDU capdu = new CAPDU(0xFF, 0xF3, (byte)(address / 0x0100), (byte)(address % 0x0100), length);
			
			Logger.Trace("< " + capdu.AsString(" ") + " (reading without specified key)");
			
			RAPDU rapdu = null;
			
			for (int retry = 0; retry < 4; retry++) {
				rapdu = Card.Channel.Transmit(capdu);
  			
				if (rapdu == null)
					break;
				if ((rapdu.SW != 0x6F01) && (rapdu.SW != 0x6F02) && (rapdu.SW != 0x6F0B))
					break;
  			
				Thread.Sleep(10);
			}
			
			if (rapdu == null) {
				Logger.Trace("Error '" + Card.Channel.LastErrorAsString + "' while reading the card");
				return false;
			}		

			Logger.Trace("> " + rapdu.AsString(" "));
			
			if (rapdu.SW != 0x9000) {
				Logger.Trace("Bad status word " + rapdu.SWString + " while reading the card");
				return false;
			}
			
			if (!rapdu.hasData) {
				Logger.Trace("Empty response");
				return false;
			}
  			
			Data = rapdu.data.GetBytes();
			PopulateBlocks();
			return true;
		}
		
		private bool setAccessBits(byte[] _access_bits)
		{
			Logger.Trace("Setting access bits ...");
			if (_access_bits.Length != 4) {
				Logger.Trace("Incorrect length for Access Bits");
				return false;
			}
			
			access_bits = _access_bits;
			return true;
		}
/*
 * @todo HTH		
		private bool WriteBinary(ushort address, byte[] data)
		{
			if (data == null)
				return false;
		  		  
			CAPDU capdu = new CAPDU(0xFF, 0xF4, (byte)(address / 0x0100), (byte)(address % 0x0100), data);		
		  
			Logger.Trace("< " + capdu.AsString(" ") + " (writing without specified key)");
			
			RAPDU rapdu = null;
			
			for (int retry = 0; retry < 4; retry++) {
				rapdu = Card.Channel.Transmit(capdu);
  			
				if (rapdu == null)
					break;
				if ((rapdu.SW != 0x6F01) && (rapdu.SW != 0x6F02) && (rapdu.SW != 0x6F0B))
					break;
  			
				Thread.Sleep(15);
			}

			if (rapdu == null) {
				Logger.Trace("Error '" + Card.Channel.LastErrorAsString + "' while writing the card");
				return false;
			}
  		
			Logger.Trace("> " + rapdu.AsString(" "));
			
			if (rapdu.SW != 0x9000) {
				Logger.Trace("Bad status word " + rapdu.SWString + " while writing the card");
				return false;
			}
			
			return true;
		}
*/    
		private bool WriteBinaryWithKey(ushort address, byte[] data, byte[] key)
		{
			if (data == null)
				return false;
		
			if (key == null)
				return false;
 		  
			byte[] dataAndKey = new byte[data.Length + key.Length];
			Array.ConstrainedCopy(data, 0, dataAndKey, 0, data.Length);
			Array.ConstrainedCopy(key, 0, dataAndKey, data.Length, key.Length);
 		  
			CAPDU capdu = new CAPDU(0xFF, 0xF4, (byte)(address / 0x0100), (byte)(address % 0x0100), dataAndKey);		
		  
			Logger.Trace("< " + capdu.AsString(" ") + " (writing with specified key)");
			
			RAPDU rapdu = null;
			
			for (int retry = 0; retry < 4; retry++) {
				rapdu = Card.Channel.Transmit(capdu);
  			
				if (rapdu == null)
					break;
				if ((rapdu.SW != 0x6F01) && (rapdu.SW != 0x6F02) && (rapdu.SW != 0x6F0B))
					break;
  			
				Thread.Sleep(15);
			}

			if (rapdu == null) {
				Logger.Trace("Error '" + Card.Channel.LastErrorAsString + "' while writing the card");
				return false;
			}
  		
			Logger.Trace("> " + rapdu.AsString(" "));		  
			
			if (rapdu.SW != 0x9000) {
				Logger.Trace("Bad status word " + rapdu.SWString + " while writing the card");
				return false;
			}
			
			return true;   	
		}
    
		public bool WriteNewBlocks()
		{
			Logger.Trace("Writing new blocks in sector " + Sector + " ...");
			if (new_blocks[0].Length != 16) {
				Logger.Trace("New blocks: invalid length (16 bytes awaited)");
				return false;
			}
			
			if (new_blocks.Count != blocks.Count) {
				Logger.Trace("The number of blocks in this sector doesn't correspond to the number of read blocks");
				return false;
			}

			byte[] data_to_write;
			ushort address = 0x00;	
			
			if (Sector == 0) {
				data_to_write = new byte[16 * 2];
				Array.ConstrainedCopy(new_blocks[1], 0, data_to_write, 0, 16);
				Array.ConstrainedCopy(new_blocks[2], 0, data_to_write, 16, 16);
				address = 0x01;

			} else {
				data_to_write = new Byte[new_blocks.Count * new_blocks[0].Length];
				
				for (int i = 0; i < new_blocks.Count; i++) {
					if ((i + 16) <= data_to_write.Length) {
						Array.ConstrainedCopy(new_blocks[i], 0, data_to_write, 16 * i, 16);
					} else {
						Logger.Trace("Overflow");
						return false;
					}
				}
				
				if (Sector < 32) {
					address = (ushort)(4 * Sector);
				} else {
					address = (ushort)(4 * 32 + (Sector - 32) * 16);
				}
			}

			/*
			 * @tood, remettre en place et corriger
			if (WriteBinary(address, data_to_write)) {
				return true;
			} else {
				if (keyB != null)
				if (WriteBinaryWithKey(address, data_to_write, keyB))
					return true;

				if (keyA != null)
				if (WriteBinaryWithKey(address, data_to_write, keyA))
					return true;
			}
			*/
			
			return false;
			
		}
		
		private bool ReadBinaryWithKey(byte[] key)
		{
			int address;
			byte length;
			GetSectorData(out address, out length);
			
			CAPDU capdu = new CAPDU(0xFF, 0xF3, (byte)(address / 0x0100), (byte)(address % 0x0100), key, length);

			Logger.Trace("< " + capdu.AsString(" ") + " (reading with specified key)");
			
			RAPDU rapdu = null;
			
			for (int retry = 0; retry < 4; retry++) {
				rapdu = Card.Channel.Transmit(capdu);
  			
				if (rapdu == null)
					break;
				if ((rapdu.SW != 0x6F01) && (rapdu.SW != 0x6F02) && (rapdu.SW != 0x6F0B))
					break;
  			
				Thread.Sleep(10);
			}
			
			if (rapdu == null) {
				Logger.Trace("Error '" + Card.Channel.LastErrorAsString + "' while reading the card");
				return false;
			}		

			Logger.Trace("> " + rapdu.AsString(" "));

			if (rapdu.SW != 0x9000) {
				Logger.Trace("Bad status word " + rapdu.SWString + " while reading the card");
				return false;
			}
			
			if (!rapdu.hasData) {
				Logger.Trace("Empty response");
				return false;
			}
  			
			Data = rapdu.data.GetBytes();
			PopulateBlocks();
			return true;
		}
				
		public bool ReadWithGivenKeys()
		{
			Logger.Trace("Reading sector " + Sector + " with specified key(s) ...");
			
			if (keyA != null)
				if (ReadBinaryWithKey(keyA))
					return true;

			if (keyB != null)
				if (ReadBinaryWithKey(keyB))
					return true;

			return false;
		}
	}	
}
