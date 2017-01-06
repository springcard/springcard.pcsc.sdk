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

namespace SpringCardMemoryCard
{
	/// <summary>
	/// Description of MifareSector.
	/// </summary>
	public class MifareSector
	{
		
		private List<byte[]> blocks;
		private List<byte[]> new_blocks;
		private byte[] keyA;
		private byte[] keyB;
		private byte[] access_bits;
		private byte[] sector_trailer;
		private int sector;
		private SCardChannel _channel;
		private MainForm MF;
		
		public MifareSector(MainForm _MF, int _sector, byte[] data, SCardChannel channel)
		{
			MF = _MF;
			sector = _sector;
			_channel = channel;

			PopulateBlocks(data);
		}
	
		private void log(string s)
		{
			if (MF != null)
				MF.add_log_text(s);			
		}
		
				
		private void PopulateBlocks(byte[] data)
		{			
			if (data != null)
			{
				blocks = new List<byte[]>();
				byte[] bloc;
				if (data.Length == 0x40)
				{
					for (int i=0; i<0x30; i+=0x10)
					{
						bloc = new byte[16];
						Array.ConstrainedCopy(data, i, bloc, 0, 16);
						blocks.Add(bloc);
					}
					access_bits = new byte[4];
					Array.ConstrainedCopy(data, 3*16+6, access_bits, 0, 4);
					
				} else
				if (data.Length == 0x100)
				{
					for (int i=0; i<0xF0; i+=0x10)
					{
						bloc = new byte[16];
						Array.ConstrainedCopy(data, i, bloc, 0, 16);
						blocks.Add(bloc);						
					}
					access_bits = new byte[4];
					Array.ConstrainedCopy(data, 15*16+6, access_bits, 0, 4);
				}
				
				
			}
			
		}
		
		public List<byte[]> Blocks
		{
			get
			{
				return blocks;
			}
		}
	
		public byte[] AccessBits
		{
			get
			{
				return access_bits;	
			}
		}
	
		public int Sector
		{
			get
			{
				return sector;
			}
		}
		
		public void SetNewBlocks(List<byte[]> _new_blocks)
		{
			log("Setting new blocks ...");
			new_blocks = new List<byte[]>();
			new_blocks = _new_blocks;
		}			
		
		public bool SetSectorTrailer(byte[] _sector_trailer)
		{
			log("Setting sector trailer ...");
			if (_sector_trailer.Length == 16)
			{
				sector_trailer = _sector_trailer;
				return true;
			} else
			{
				log("Can't set Sector Trailer: invalid length");
				return false;
			}
			
		}
		
		public bool WriteSectorTrailer()
		{
			log("Writing sector trailer ...");
			if (sector_trailer == null)
			{
				log("Sector trailer is null");
				return false;
			}
			
			if (sector_trailer.Length != 16)
			{
				log("Sector trailer length != 16");
				return false;
			}
			
			ushort address;		
			byte[] _keyA = new byte[6];
			byte[] _keyB = new byte[6];
			byte[] _access_bits = new Byte[4];
			
			if (sector < 32)
			{
				address = (ushort) (sector*4+3);
			} else
			{
				address = (ushort) (32*4 + (sector - 32)*16 + 15);
			}
			
			for (int i=0; i<6; i++)
			{
				if (i<4)
					_access_bits[i] = sector_trailer[i+6];
				
				_keyA[i] = sector_trailer[i];
				_keyB[i] = sector_trailer[i+10];
			}		
			
			if (WriteBinary(address, sector_trailer))
			{
				setKeyA(_keyA);
				setKeyB(_keyB);
				setAccessBits(_access_bits);
				return true;
				
			} else
			if (WriteBinaryWithKey(address, sector_trailer, keyB))
			{
				setKeyA(_keyA);
				setKeyB(_keyB);
				setAccessBits(_access_bits);
				return true;
				
			} else
			if (WriteBinaryWithKey(address, sector_trailer, keyA))
			{
				setKeyA(_keyA);
				setKeyB(_keyB);
				setAccessBits(_access_bits);
				return true;
			}

			return false;
		}
		
		public static bool isByte(char c)
    {
      bool r = false;

      if ((c >= '0') && (c <= '9'))
      {
        r = true;
      } else 
      if ((c >= 'A') && (c <= 'F'))
      {
        r = true;
      } else 
      if ((c >= 'a') && (c <= 'f'))
      {
        r = true;
      }

      return r;
    }
		
		public static bool checkCorrectFormat(string data, int nbBytes)
		{
			if (data.Length != (nbBytes * 2))
				return false;
			
			for (int i = 0; i<data.Length; i++)
				if (!isByte(data[i]))
					return false;
			
			return true;
		}	
		
		public bool setKeyA(byte[] _keyA)
		{
			log("Setting Key A ...");
			if (_keyA.Length != 6)
			{
				log("Incorrect length for Key A");
				return false;
			}
			
			keyA = _keyA;
			return true;
		}
		
		public bool setKeyB(byte[] _keyB)
		{
			log("Setting key B ...");
			if (_keyB.Length != 6)
			{
				log("Incorrect length for Key B");
				return false;
			}
			
			keyB = _keyB;
			return true;
		}
		
		private bool setAccessBits(byte[] _access_bits)
		{
			log("Setting access bits ...");
			if (_access_bits.Length != 4)
			{
				log("Incorrect length for Access Bits");
				return false;
			}
			
			access_bits = _access_bits;
			return true;
		}
		
    private bool WriteBinary(ushort address, byte[] data)
		{
		  if (data == null)
		    return false;
		  		  
		  CAPDU capdu = new CAPDU(0xFF, 0xF4, (byte) (address / 0x0100), (byte) (address % 0x0100), data);		
		  
			log("< " + capdu.AsString(" ") + " (writing without specified key)");
			
			RAPDU rapdu = null;
			
		  for (int retry = 0; retry < 4; retry++)
		  {
  			rapdu = _channel.Transmit(capdu);
  			
  			if (rapdu == null)
  			  break;
  			if ((rapdu.SW != 0x6F01) && (rapdu.SW != 0x6F02) && (rapdu.SW != 0x6F0B))
  			  break;
  			
  			Thread.Sleep(15);
		  }

		  if (rapdu == null)
			{
				log("Error '" + _channel.LastErrorAsString + "' while writing the card");
				return false;
			}
  		
		  log("> " + rapdu.AsString(" "));
			
			if (rapdu.SW != 0x9000)
			{
				log("Bad status word " + rapdu.SWString + " while writing the card");
				return false;
			}
			
			return true;
		}
    
    private bool WriteBinaryWithKey(ushort address, byte[] data, byte[] key)
    {
 		  if (data == null)
		    return false;
		
 		  if (key == null)
 		  	return false;
 		  
 		  byte[] dataAndKey = new byte[data.Length + key.Length];
 		  Array.ConstrainedCopy(data, 0, dataAndKey, 0, data.Length);
 		  Array.ConstrainedCopy(key, 0, dataAndKey, data.Length, key.Length);
 		  
		  CAPDU capdu = new CAPDU(0xFF, 0xF4, (byte) (address / 0x0100), (byte) (address % 0x0100), dataAndKey);		
		  
		  log("< " + capdu.AsString(" ") + " (writing with specified key)");
			
			RAPDU rapdu = null;
			
		  for (int retry = 0; retry < 4; retry++)
		  {
  			rapdu = _channel.Transmit(capdu);
  			
  			if (rapdu == null)
  			  break;
  			if ((rapdu.SW != 0x6F01) && (rapdu.SW != 0x6F02) && (rapdu.SW != 0x6F0B))
  			  break;
  			
  			Thread.Sleep(15);
		  }

		  if (rapdu == null)
			{
		  	log("Error '" + _channel.LastErrorAsString + "' while writing the card");
				return false;
			}
  		
			log("> " + rapdu.AsString(" "));		  
			
			if (rapdu.SW != 0x9000)
			{
				log("Bad status word " + rapdu.SWString + " while writing the card");
				return false;
			}
			
			return true;   	
    }
    
		public bool WriteNewBlocks()
		{
			log("Writing new blocks in sector " + sector + " ...");
			if (new_blocks[0].Length != 16)
			{
				log("New blocks: invalid length (16 bytes awaited)");
				return false;
			}
			
			if (new_blocks.Count != blocks.Count)
			{
				log("The number of blocks in this sector doesn't correspond to the number of read blocks");
				return false;
			}

			byte[] data_to_write;
			ushort address = 0x00;	
			
			if (sector == 0)
			{
				data_to_write = new byte[16*2];
				Array.ConstrainedCopy(new_blocks[1], 0, data_to_write, 0, 16);
				Array.ConstrainedCopy(new_blocks[2], 0, data_to_write, 16, 16);
				address = 0x01;

			} else
			{
				data_to_write = new Byte[new_blocks.Count * new_blocks[0].Length];
				
				for (int i=0; i<new_blocks.Count; i++)
				{
					if ( (i+16) <= data_to_write.Length)
					{
						Array.ConstrainedCopy(new_blocks[i], 0, data_to_write, 16*i, 16);
					} else
					{
						log("Overflow");
						return false;
					}
				}
				
				if (sector < 32)
				{
					address = (ushort) (4*sector);
				} else
				{
					address = (ushort) (4*32 + (sector-32)*16);
				}
			}

			if (WriteBinary(address, data_to_write))
			{
				return true;
			} else
			{
				if (keyB != null)
					if (WriteBinaryWithKey(address, data_to_write, keyB))
						return true;

				if (keyA != null)
					if (WriteBinaryWithKey(address, data_to_write, keyA))
						return true;
			}
			
			return false;
			
		}
		
		private byte[] ReadBinaryWithKey(ushort address, byte length, byte[] key)
		{
			CAPDU capdu = new CAPDU(0xFF, 0xF3, (byte) (address / 0x0100), (byte) (address % 0x0100), key, length);

			log("< " + capdu.AsString(" ") + " (reading with specified key)");
			
			RAPDU rapdu = null;
			
		  for (int retry = 0; retry < 4; retry++)
		  {
  			rapdu = _channel.Transmit(capdu);
  			
  			if (rapdu == null)
  			  break;
  			if ((rapdu.SW != 0x6F01) && (rapdu.SW != 0x6F02) && (rapdu.SW != 0x6F0B))
  			  break;
  			
  			Thread.Sleep(10);
		  }
			
			if (rapdu == null)
			{
				log("Error '" + _channel.LastErrorAsString + "' while reading the card");
				return null;
			}		

			log("> " + rapdu.AsString(" "));

			if (rapdu.SW != 0x9000)
			{
				log("Bad status word " + rapdu.SWString + " while reading the card");
				return null;
			}
			
			if (!rapdu.hasData)
			{
				log("Empty response");
				return null;
			}
  			
  	  return rapdu.data.GetBytes();
  	  
		}
				
		public bool ReadWithGivenKeys()
		{
			byte[] data = null;
			byte[] data_long_sectors = null;
			ushort address = 0;
			byte length = 0x00;
			log("Reading sector " + Sector + " with specified key(s) ...");
			
			if (sector < 32)
			{
				address = (ushort) (4*sector);
				length = 0x40;
				
				if (keyA != null)
				{
					data = ReadBinaryWithKey(address, length, keyA);
					if (data != null)
					{
						PopulateBlocks(data);
						return true;
					}
				}
					
				if (keyB != null)
				{
					data = ReadBinaryWithKey(address, length, keyB);
					if (data != null)
					{
						PopulateBlocks(data);
						return true;
					}
				}
				
			} else
			{
				length = 0x80;
				address = (ushort) (4*32 + (sector-32)*16);
				
				if (keyA != null)
				{
					data = ReadBinaryWithKey(address, length, keyA);
					if (data != null)
					{
						data_long_sectors = new byte[2*data.Length];
						Array.ConstrainedCopy(data, 0, data_long_sectors, 0, data.Length);
						
						address += (ushort) (length / 0x10);
						
						data = ReadBinaryWithKey(address, length, keyA);
						if (data != null)
						{
							Array.ConstrainedCopy(data, 0, data_long_sectors, length, data.Length);
							PopulateBlocks(data_long_sectors);
							return true;
						} else
						{
							return false;
						}

					}

			  }
				
				if (keyB != null)
				{
					data = ReadBinaryWithKey(address, length, keyB);
					if (data != null)
					{
						data_long_sectors = new byte[2*data.Length];
						Array.ConstrainedCopy(data, 0, data_long_sectors, 0, data.Length);
						
						address += (ushort) (length / 0x10);
						
						data = ReadBinaryWithKey(address, length, keyB);
						if (data != null)
						{
							Array.ConstrainedCopy(data, 0, data_long_sectors, length, data.Length);
							PopulateBlocks(data_long_sectors);
							return true;
						} else
						{
							return false;		
						}

					}

				}
			
			}
			
			return false;
		}
		
	}
	
}
