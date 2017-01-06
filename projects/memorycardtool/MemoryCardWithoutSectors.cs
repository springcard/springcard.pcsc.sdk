/*
 * Created by SharpDevelop.
 * User: jerome.i
 * Date: 17/06/2013
 * Time: 09:43
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
	/// Description of MemoryCardWithoutSectors.
	/// </summary>
	public class MemoryCardWithoutSectors : MemoryCard
	{
		
		private const string PIX_NN_MIFARE_UL								= "0003";
		private const string PIX_NN_ST_MICRO_ST176					= "0006";
		private const string PIX_NN_ST_MICRO_OTHER_SR				=	"0007";
		//private const string PIX_NN_ATMEL_AT88SC0808CRF		= "000A";
		//private const string PIX_NN_ATMEL_AT88SC1616CRF		= "000B";
		//private const string PIX_NN_ATMEL_AT88SC3216CRF		= "000C";
		//private const string PIX_NN_ATMEL_AT88SC6416CRF		= "000D";
		private const string PIX_NN_TEXAS_TAGIT							= "0012";
		//private const string PIX_NN_ST_MICRO_LRI512				= "0013";
		private const string PIX_NN_NXP_ICODE_SLI						= "0014";
		private const string PIX_NN_NXP_ICODE1							= "0016";
		private const string PIX_NN_ST_MICRO_LRI64					= "0021";
		//private const string PIX_NN_ST_MICRO_LR12					= "0024";
		//private const string PIX_NN_ST_MICRO_LRI128				= "0025";
		//private const string PIX_NN_MIFARE_MINI						= "0026";
		private const string PIX_NN_INNOVISION_JEWEL				= "002F";
		private const string PIX_NN_INNOVISION_TOPAZ				= "0030";
		//private const string PIX_NN_ATMEL_AT88RF04C				= "0034";
		//private const string PIX_NN_NXP_ICODE_SL2					= "0035";
		private const string PIX_NN_MIFARE_UL_C							= "003A";		
		//private const string PIX_NN_GENERIC_14443_A				= "FFA0";
		//private const string PIX_NN_GENERIC_14443_B				= "FFB0";
		private const string PIX_NN_ASK_CTS_256B						= "FFB1";
		private const string PIX_NN_ASK_CTS_512B						= "FFB2";
		private const string PIX_NN_INSIDE_CONTACTLESS			= "FFB7";
		//private const string PIX_NN_UNIDENTIFIED_ATMEL		= "FFB8";
		//private const string PIX_NN_CALYPSO_INNOVATRON		= "FFC0";
		//private const string PIX_NN_UNIDENTIFIED_ISO15693	= "FFD0";
		private const string PIX_NN_UNIDENTIFIED_EMMARIN		= "FFD1";
		//private const string PIX_NN_UNIDENTIFIED_ST_MICRO	= "FFD2";
		
		private const byte UNKNOWN								= 0x00;
		private const byte MIFARE_UL							= 0x01;
		private const byte ST_MICRO_ST176					= 0x02;
		private const byte ST_MICRO_OTHER_SR			=	0x03;
		//private const byte ATMEL_AT88SC0808CRF	= 0x04;
		//private const byte ATMEL_AT88SC1616CRF	= 0x05;
		//private const byte ATMEL_AT88SC3216CRF	= 0x06;
		//private const byte ATMEL_AT88SC6416CRF	= 0x07;
		private const byte TEXAS_TAGIT						= 0x08;
		//private const byte ST_MICRO_LRI512			= 0x09;
		private const byte NXP_ICODE_SLI					= 0x0A;
		private const byte NXP_ICODE1							= 0x0B;
		private const byte ST_MICRO_LRI64					= 0x0C;
		//private const byte ST_MICRO_LR12				= 0x0D;
		//private const byte ST_MICRO_LRI128			= 0x0E;
		//private const byte MIFARE_MINI					= 0x0F;
		private const byte INNOVISION_JEWEL				= 0x10;
		private const byte INNOVISION_TOPAZ				= 0x11;
		//private const byte ATMEL_AT88RF04C			= 0x12;
		//private const byte NXP_ICODE_SL2				= 0x13;
		private const byte MIFARE_UL_C						= 0x14;
		//private const byte GENERIC_14443_A			= 0x15;
		//private const byte GENERIC_14443_B			= 0x16;
		private const byte ASK_CTS_256B						= 0x17;
		private const byte ASK_CTS_512B						= 0x18;
		private const byte INSIDE_CONTACTLESS			= 0x19;
		//private const byte UNIDENTIFIED_ATMEL		= 0x1A;
		//private const byte CALYPSO_INNOVATRON		= 0x1B;
		//private const byte UNIDENTIFIED_ISO15693	= 0x1C;;
		private const byte UNIDENTIFIED_EMMARIN		= 0x1D;
		//private const byte UNIDENTIFIED_ST_MICRO	= 0x1E;			
		
		private int nbBytesPerPage;
		protected int _nbPages;	
		private List<byte[]> pages;
		//private int readingSizeInBytes;
		//private int writingSizeInBytes;
		private List<byte[]> p1p2;
		
		private List<byte[]> new_pages;
		
		private byte maxP1 = 0;
		private byte maxP2 = 0;
		
		public MemoryCardWithoutSectors(MainForm _MF, SCardChannel Channel) : base(_MF, Channel)
		{	
			Recognize_with_ATR();
			if (!Read(false))
			{
				throw new System.ApplicationException();
			}
		}
		
		public List<byte[]> Pages
		{
			get
			{
				return pages;	
			}
		}
		
		public List<byte[]> P1P2
		{
			get
			{
				return p1p2;
			}
		}
			
		public byte MaxP1
		{
			get
			{
				return maxP1;
			}
		}
		
		public byte MaxP2
		{
			get 
			{
				return maxP2;	
			}
		}
		private void Recognize_with_ATR()
		{
			string atr 		= _channel.CardAtr.AsString("");
			string pix_nn = "";
			
			log("Recognizing card with its ATR ...");
			
			type = UNKNOWN;
			nbBytesPerPage = 0;
			_nbPages = 0;
			_cardType = "Unknown memory card";
			
			if (atr.Length >= 30)
			{ 
				pix_nn = atr.Substring(26, 4);
				
				if (pix_nn.Equals(PIX_NN_MIFARE_UL))
				{
					log("Mifare UltraLight recognized");
					type = MIFARE_UL;
					_cardType = "Mifare Ultralight";
					nbBytesPerPage = 4;					
					
				} else
				if (pix_nn.Equals(PIX_NN_ST_MICRO_ST176))
				{
					log("ST MicroElectronics SR176 recognized");
					type = ST_MICRO_ST176;
					_cardType = "ST MicroElectronics SR176";
					nbBytesPerPage = 2;
					_nbPages = 16;
					
				} else
				if (pix_nn.Equals(PIX_NN_ST_MICRO_OTHER_SR))
				{
					log("ST MicroElectronics SRI4K, SRIX4K, SRIX512, SRI512 or SRT512 recognized");
					type = ST_MICRO_OTHER_SR;
					_cardType = "ST MicroElectronics SRI4K, SRIX4K, SRIX512, SRI512 or SRT512";
					nbBytesPerPage = 4;
					
				} else
				if (pix_nn.Equals(PIX_NN_TEXAS_TAGIT))
				{
					log("Texas Instruments TAG IT recognized");
					_cardType = "Texas Instruments TAG IT ";
					type = TEXAS_TAGIT;
					nbBytesPerPage = 4;
					
				} else
				if (pix_nn.Equals(PIX_NN_NXP_ICODE_SLI))
				{
					log("NXP ICODE SLI recognized");
					_cardType = "NXP ICODE SLI";
					type = NXP_ICODE_SLI;
					nbBytesPerPage = 4;
					
				} else
				if (pix_nn.Equals(PIX_NN_NXP_ICODE1))
				{
					log("NXP ICODE1 recognized");
					type = NXP_ICODE1;
					_cardType = "NXP ICODE1";
					nbBytesPerPage = 4;
					_nbPages = 16;
					
				} else
				if (pix_nn.Equals(PIX_NN_ST_MICRO_LRI64))
				{
					log("ST MicroElectronics LRI64 recognized");
					_cardType = "ST MicroElectronics LRI64";
					type = ST_MICRO_LRI64;
					nbBytesPerPage = 1;
					
				} else
				if (pix_nn.Equals(PIX_NN_INNOVISION_JEWEL))
				{
					log("Innovision Jewel recognized");
					type = INNOVISION_JEWEL;
					_cardType = "Innovision Jewel";
					nbBytesPerPage = 128;
					_nbPages = 1;
					
				} else
				if (pix_nn.Equals(PIX_NN_INNOVISION_TOPAZ))
				{
					log("Innovision Topaz recognized");
					type = INNOVISION_TOPAZ;
					_cardType = "Innovision Topaz";
					nbBytesPerPage = 128;
					_nbPages = 1;		
					
				} else
				if (pix_nn.Equals(PIX_NN_MIFARE_UL_C))
				{
					log("Mifare UltraLight C recognized");
					type = MIFARE_UL_C;
					_cardType = "Mifare UltraLight C";
					nbBytesPerPage = 4;
					
				} else
				if (pix_nn.Equals(PIX_NN_ASK_CTS_256B))
				{
					log("ASK CTS 256B recognized");
					type = ASK_CTS_256B;
					_cardType = "ASK CTS 256B";
					nbBytesPerPage = 2;
					_nbPages = 16;
					
				} else
				if (pix_nn.Equals(PIX_NN_ASK_CTS_512B))
				{
					log("ASK CTS 512B recognized");
					type = ASK_CTS_512B;
					_cardType = "ASK CTS 512B";
					nbBytesPerPage = 2;
					_nbPages = 32;
					
				} else
				if (pix_nn.Equals(PIX_NN_INSIDE_CONTACTLESS))
				{
					log("Inside Contacless PICOTAG/PICOPASS recognized");
					type = INSIDE_CONTACTLESS;
					_cardType = "Inside Contacless PICOTAG/PICOPASS";
					nbBytesPerPage = 8;
					
				} else
				if (pix_nn.Equals(PIX_NN_UNIDENTIFIED_EMMARIN))
				{
					log("Unidentified ISO 15693 from EMMarin (or Legic)");
					type = UNIDENTIFIED_EMMARIN;
					_cardType = "Unidentified ISO 15693 from EMMarin (or Legic)";
					nbBytesPerPage = 8;
				} else
				{
					log("Non-recognized memory card");
				}
				
				
			}

				

						
		}
		
    protected byte[] ReadBinary(ushort address, byte length)
		{
			CAPDU capdu = new CAPDU(0xFF, 0xB0, (byte) (address / 0x0100), (byte) (address % 0x0100), length);

			log("< " + capdu.AsString(" "));
			
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
		
    private bool Read(bool max_address_known)
    {
    	byte[] tmp;
    	byte[] read_bytes;
    	byte[] pages_read;
    	byte[] addr;
    	ushort address;
    	Int32 offset;
    		
    	/* Mifare cards can be special: Kovio, Infineon for exemples use their ATR	*/
    	if ((type == MIFARE_UL) || (type == MIFARE_UL_C))
    		return Read_Mifare_UL_ULC(max_address_known);

    	/* Inside Contactless PicoTAG or PicoPASS are special: 								*/
 			/* Either 32 or 256 pages, but no error when reading incorect address	*/
    	if (type == INSIDE_CONTACTLESS)
    		return Read_Inside_Contactless(max_address_known);
    	
    	/* Innovision Topaz and Jewel are special:																	*/
    	/* They contain only one page. But we need to have P1 and P2 for each byte 	*/
    	/* So we represent card's content as pages of 16 bytes											*/
    	if ((type == INNOVISION_JEWEL) || (type == INNOVISION_TOPAZ))
    		return Read_Innovision(max_address_known);
    	
    	log("Reading the memory card ...");
    	pages = new List<byte[]>();
    	p1p2 = new List<byte[]>();
    	
    	if (nbBytesPerPage == 0)
    	{
    		/* We don't know the number of bytes in a page	*/
    		read_bytes = ReadBinary(0, 0);
    		if (read_bytes == null)
    			return false;
    		
    		nbBytesPerPage = read_bytes.Length;
    	}
    		
    	/*--*/

    	if (max_address_known)
    	{
    		ushort pg;
    		/* Loop until everything is read	*/
				for (byte p1=0; p1 <= maxP1 ; p1++)
				{
					for (byte p2=0; p2 <= maxP2; p2++)
					{
						pg = (ushort)(p1*0x100 + p2);
	
						pages_read = ReadBinary(pg, (byte) nbBytesPerPage);
		    		if (pages_read == null)
		    		{
		    			log("Can't read page " + pg);
		    		} else
		    		{		
							tmp = new byte[nbBytesPerPage];
							
							addr = new byte[2];
							addr[0] = p1;
							addr[1] = p2;
							p1p2.Add(addr);
							
		    			Array.ConstrainedCopy(pages_read, 0, tmp, 0, nbBytesPerPage);
				  		pages.Add(tmp);
	
		    		}

					}
					
				}

    	}	else
    	{
    		if (_nbPages == 0)
	    	{
	    		/* We don't know the number of pages in the detected card	*/
	    		address = 0;	 
					offset = 0;	    		
	    		for (;;)
	    		{
	    			tmp = new byte[nbBytesPerPage];

						read_bytes = ReadBinary(address, (byte) nbBytesPerPage);
	    			
	    			if (read_bytes == null)
	    			{
	    				if (address == 0)
	    					return false;
	    				
	    				break;
	    				
	    			}
	    			
	    			if (read_bytes.Length != nbBytesPerPage)
	    				break;
	    			
		    		addr = new byte[2];
						addr[0] = (byte)(offset >> 8);
						addr[1] = (byte)(offset & 0x00FF);
						offset ++;
						p1p2.Add(addr);					
	    			
						Array.ConstrainedCopy(read_bytes, 0, tmp, 0, nbBytesPerPage);
	    			pages.Add(tmp);
	    			address += 1;
	    		}
	    			
	    	} else
	    	{
	    		offset = 0;	   
    			/* We know the number of pages */
	    		for (byte i = 0; i<_nbPages; i += 1)
	    		{
	    			tmp = new byte[nbBytesPerPage];
	    			read_bytes = ReadBinary(i, (byte) nbBytesPerPage);
	    			
	    			if (read_bytes == null)
	    			{
	    				if (i==0)
	    					return false;
	    				
	    				break;
	    			}
	    			
	    			if (read_bytes.Length != nbBytesPerPage)
	    				break;
	    			
	    			addr = new byte[2];
						addr[0] = (byte)(offset >> 8);
						addr[1] = (byte)(offset & 0x00FF);
						offset ++;
						p1p2.Add(addr);		
						
	    			Array.ConstrainedCopy(read_bytes, 0, tmp, 0, nbBytesPerPage);
	    			pages.Add(tmp);
	    			 
	    		}
	    			
	    	}

    	}
    	
    	if (p1p2 == null)
    	{
    		maxP1 = 0;
    		maxP2 = 0;
    		return false;
    	}
    	if (p1p2.Count == 0)
    	{
    		maxP1 = 0;
    		maxP2 = 0;
    		return false;
    	}
    	
  		maxP1 = p1p2[p1p2.Count-1][0];
			maxP2 = p1p2[p1p2.Count-1][1];	    	
    	return true;
    	
    }
    
    private bool ByteArraysEqual(byte[] b1, byte[] b2)
		{
	    if (b1 == b2) 
	    		return true;
	    
	    if (b1 == null || b2 == null) 
	    		return false;
	    
	    if (b1.Length != b2.Length) 
	    		return false;
	    
	    for (int i=0; i < b1.Length; i++)
      	if (b1[i] != b2[i])
	    		return false;
	    
	    return true;
		}
       
    private bool Read_Mifare_UL_ULC(bool max_address_known)
    {
    	
    	byte[] tmp;
    	byte[] addr;
    	byte[] pages_read;
    	byte[] next_pages;
			
    	log("Reading the Mifare UL or ULC ...");
			
    	if (max_address_known)
    	{
    		pages = new List<byte[]>();
 	    	p1p2 = new List<byte[]>();
 	    	ushort pg;
    		
 	    	/* Loop until everything is read	*/
    		for (byte p1 = 0; p1 <= maxP1 ; p1++)
    		{
    			for (byte p2=0; p2 <= maxP2; p2++)
    			{
    				pg = (ushort)(p1*0x100 + p2);
    				pages_read = ReadBinary(pg, 4);
    				
    				if (pages_read == null)
		    		{
		    			log("Can't read page " + pg);
		    		} else
		    		{		
							tmp = new byte[4];
							
							addr = new byte[2];
							addr[0] = p1;
							addr[1] = p2;
							p1p2.Add(addr);
							
		    			Array.ConstrainedCopy(pages_read, 0, tmp, 0, 4);
				  		pages.Add(tmp);
	
		    		}
    				
    			}
    		}

    	} else   	 	
    	{
	    	/* read page 0	*/
	    	byte[] first_four_pages = ReadBinary(0, 16);
	
	    	if (first_four_pages == null)
	    	{
	    		log("Can't read the first four pages");
	    		return false;
	    	}
	  	
	    	pages = new List<byte[]>();
	    	p1p2 = new List<byte[]>();
	    	UInt32 offset = 0;
	    	for (int i = 0; i<16; i+=4)
	    	{
					tmp = new byte[4];
					
					addr = new byte[2];
					addr[0] = (byte)(offset >> 8);
					addr[1] = (byte)(offset & 0x00FF);
					offset ++;
					p1p2.Add(addr);
					
					Array.ConstrainedCopy(first_four_pages, i, tmp, 0, 4);
	  			pages.Add(tmp);
	  		}
	    	
	    	/* read pages 4 - 8 - 12	*/
	    	for (ushort pg=4; pg<16; pg +=4)
	    	{
	    		next_pages = ReadBinary(pg, 16);
	    		if (next_pages == null)
	    		{
	    			log("Can't read page " + pg);
	    			return false;
	    		} else
	    		{		
	    			for (int i = 0; i<16; i+=4)
			    	{
							tmp = new byte[4];
							
							addr = new byte[2];
							addr[0] = (byte)(offset >> 8);
							addr[1] = (byte)(offset & 0x00FF);
							offset ++;
							p1p2.Add(addr);
					
	    				Array.ConstrainedCopy(next_pages, i, tmp, 0, 4);
			  			pages.Add(tmp);
			  		}	
	    		}
	    		
	    	}
	    	
	    	/* read page 16 */
	    	next_pages = ReadBinary(16, 16);
	    	if (next_pages == null)
	    	{
	    		/* A "REAL" Mifare Ultralight card !	*/
	    		_nbPages = 16;
	    		log("Mifare UL: 16 pages of data");
	    		
	    	} else
	    	if (ByteArraysEqual(first_four_pages, next_pages))
	    	{
	    		/* Pages 0-3 = pages 16-19 : stay with 16 pages	*/
	    		_nbPages = 16;
	    		log("Mifare UL: 16 pages of data");
	    		
	    	} else
	    	{
	    		for (int i = 0; i<16; i+=4)
		    	{
						tmp = new byte[4];
						
						addr = new byte[2];
						addr[0] = (byte)(offset >> 8);
						addr[1] = (byte)(offset & 0x00FF);
						offset ++;
						p1p2.Add(addr);
						
						Array.ConstrainedCopy(next_pages, i, tmp, 0, 4);
		  			pages.Add(tmp);
		  		}
	    		
	    		ushort address = 20;
	    		for (;;)
	    		{
	    			next_pages = ReadBinary(address, 4);
	    			if (next_pages == null)
	    			{
	    				_nbPages = address;
	    				break;
	    			}

						tmp = new byte[4];
						
						addr = new byte[2];
						addr[0] = (byte)(offset >> 8);
						addr[1] = (byte)(offset & 0x00FF);
						offset ++;
						p1p2.Add(addr);
						
						Array.ConstrainedCopy(next_pages, 0, tmp, 0, 4);
		  			pages.Add(tmp);	    			

	    			address += 1;
	    			
	    		}
	    		
	    		if (_nbPages == 44)
	    		{
	    			log("Mifare UL C: 44 pages of data");
	    		} else
	    		if (_nbPages == 64)
	    		{
	    			log("Kovio 2KB: 64 pages of data");
	    			_cardType = "Kovio 2KB";
	    		} else
	    		{
	    			log("Unknown card: "+ _nbPages + " pages of data");
	    		}
	    			
	    	}
    	
   		}
    	
    	if (p1p2 == null)
    	{
    		maxP1 = 0;
    		maxP2 = 0;
    		return false;
    	}
    	if (p1p2.Count == 0)
    	{
    		maxP1 = 0;
    		maxP2 = 0;
    		return false;
    	}
    	maxP1 = p1p2[p1p2.Count-1][0];
			maxP2 = p1p2[p1p2.Count-1][1];	
			return true;   	
    	
    }
    
    private bool Read_Inside_Contactless(bool max_address_known)
    {
    	byte[] first_four_pages;
    	byte[] next_pages;
    	byte[] pages_read;
    	byte[] tmp;
    	byte[] addr;
    	
    	log("Reading the Inside Contactless card ...");
    	
    	if (nbBytesPerPage == 0)
    	{
    		log("Determine the size of a page");
    		first_four_pages = ReadBinary(0, 0);
    		if (first_four_pages == null)
    		{
    			log("Can't read the first page");
    			return false;
    		}
    		
    		nbBytesPerPage = first_four_pages.Length;
    	}
    	
    	
    	pages = new List<byte[]>();
 	    p1p2 = new List<byte[]>();
    	ushort pg;

    	if (max_address_known)
    	{
	    	/* Loop until everything is read	*/
				for (byte p1=0; p1 <= maxP1 ; p1++)
				{
					for (byte p2=0; p2 <= maxP2; p2++)
					{
						pg = (ushort)(p1*0x100 + p2);
	
						pages_read = ReadBinary(pg, (byte) nbBytesPerPage);
		    		if (pages_read == null)
		    		{
		    			log("Can't read page " + pg);
		    		} else
		    		{		
							tmp = new byte[nbBytesPerPage];
							
							addr = new byte[2];
							addr[0] = p1;
							addr[1] = p2;
							p1p2.Add(addr);
							
		    			Array.ConstrainedCopy(pages_read, 0, tmp, 0, nbBytesPerPage);
				  		pages.Add(tmp);
	
		    		}

					}
					
				}
    		
    	} else
    	{
	    	/* Read the first four pages */
	    	pages = new List<byte[]>();
	    	p1p2 = new List<byte[]>();
	    	UInt32 offset = 0;
	    	first_four_pages = ReadBinary(0, (byte) (4*nbBytesPerPage));
	    	if (first_four_pages == null)
	    	{
	    		log("Can't read the first four pages");
	    		return false;
	    	}

	    	for (int i = 0; i<4*nbBytesPerPage; i+=nbBytesPerPage)
	    	{
					tmp = new byte[nbBytesPerPage];
	    		
					addr = new byte[2];
					addr[0] = (byte)(offset >> 8);
					addr[1] = (byte)(offset & 0x00FF);
					offset ++;
					p1p2.Add(addr);
					
					Array.ConstrainedCopy(first_four_pages, i, tmp, 0, nbBytesPerPage);
	  			pages.Add(tmp);
	  		}
	    	
	    	/* Read pages 4 to 31 */
	    	for (ushort p=4 ; p<32 ; p+=4)
	    	{
	    		next_pages = ReadBinary(p, (byte) (4*nbBytesPerPage));
	    		if (next_pages == null)
	    		{
	    			log("Can't read page " + p);
	    			return false;
	    		}
	    		
		    	for (int i = 0; i<4*nbBytesPerPage; i+=nbBytesPerPage)
		    	{
						tmp = new byte[nbBytesPerPage];
						
						addr = new byte[2];
						addr[0] = (byte)(offset >> 8);
						addr[1] = (byte)(offset & 0x00FF);
						offset ++;
						p1p2.Add(addr);
						
		    		Array.ConstrainedCopy(next_pages, i, tmp, 0, nbBytesPerPage);  			
		  			pages.Add(tmp);
		  		}
		    	
	    	}
	    	
	    	/* Read pages 32->35 : are they the same as 0->3 ?	*/
	    	next_pages = ReadBinary(32, (byte) (4*nbBytesPerPage));
	    	if (next_pages == null)
	    	{
	    		log("Can't read pages 32 -> 35: Inside ContactLess card detected !");
	    		
 		    	if (p1p2 == null)
		    	{
		    		maxP1 = 0;
		    		maxP2 = 0;
		    		return false;
		    	}
		    	if (p1p2.Count == 0)
		    	{
		    		maxP1 = 0;
		    		maxP2 = 0;
		    		return false;
		    	}
		    	maxP1 = p1p2[p1p2.Count-1][0];
					maxP2 = p1p2[p1p2.Count-1][1];	
			    		
	    		return true;
	    	}
	    	
	    	if (ByteArraysEqual(first_four_pages, next_pages))
	    	{
	    		log("pages 0->4 = pages 32->35: Inside ContactLess card detected !");
	    		
	    	 	if (p1p2 == null)
		    	{
		    		maxP1 = 0;
		    		maxP2 = 0;
		    		return false;
		    	}
		    	if (p1p2.Count == 0)
		    	{
		    		maxP1 = 0;
		    		maxP2 = 0;
		    		return false;
		    	}
		    	maxP1 = p1p2[p1p2.Count-1][0];
					maxP2 = p1p2[p1p2.Count-1][1];	
	    		
	    		
	    		return true;
	    	} else
	    	{
		    
	    	 	for (int i = 0; i<4*nbBytesPerPage; i+=nbBytesPerPage)
		    	{
						tmp = new byte[nbBytesPerPage+2];
						
						addr = new byte[2];
						addr[0] = (byte)(offset >> 8);
						addr[1] = (byte)(offset & 0x00FF);
						offset ++;
						p1p2.Add(addr);
						
		    		Array.ConstrainedCopy(next_pages, i, tmp, 0, nbBytesPerPage);  			
		  			pages.Add(tmp);
		  		}
	    		
	    		/* Read pages 36 to 255 */
		    	for (ushort p=36 ; p<256 ; p+=4)
		    	{
		    		next_pages = ReadBinary(p, (byte) (4*nbBytesPerPage));
		    		if (next_pages == null)
		    		{
		    			log("Can't read address " + p);
		    			return false;
		    		}
		    		
			    	for (int i = 0; i<4*nbBytesPerPage; i+=nbBytesPerPage)
			    	{
							tmp = new byte[nbBytesPerPage];
							
							addr = new byte[2];
							addr[0] = (byte)(offset >> 8);
							addr[1] = (byte)(offset & 0x00FF);
							offset ++;
							p1p2.Add(addr);
							
			    		Array.ConstrainedCopy(next_pages, i, tmp, 0, nbBytesPerPage);  			
			  			pages.Add(tmp);
			  		}
			    	
		    	}    			
	    	}
    	
    	}
       
    	if (p1p2 == null)
    	{
    		maxP1 = 0;
    		maxP2 = 0;
    		return false;
    	}
    	if (p1p2.Count == 0)
    	{
    		maxP1 = 0;
    		maxP2 = 0;
    		return false;
    	}
    	maxP1 = p1p2[p1p2.Count-1][0];
			maxP2 = p1p2[p1p2.Count-1][1];	
    	return true;
    }
    
    private bool Read_Innovision(bool max_address_known)
    {
    	byte[] pages_read;
    	ushort pg;
    	byte[] tmp;
    	int offset;
    	byte[] addr;
    	byte[] read_bytes;
    	
	    pages = new List<byte[]>();
	    p1p2 = new List<byte[]>();    	
    	
	    if (max_address_known)
    	{
	    	/* Loop until everything is read	*/
	    	/* We read byte after after byte	*/
	    	tmp = new byte[16];
	    	offset = 0;

	    	for (byte p1=0; p1 <= maxP1 ; p1++)
				{
					for (byte p2=0; p2 <= maxP2; p2++)
					{
						pg = (ushort)(p1*0x100 + p2);

						pages_read = ReadBinary(pg, 1);
		    		if (pages_read == null)
		    		{
		    			log("Can't read page " + pg);
		    		} else
		    		{		
		    			if (offset == 0)
		    			{
								addr = new byte[2];
								addr[0] = p1;
								addr[1] = p2;	
								p1p2.Add(addr);
		    			}

		    			tmp[offset] = pages_read[0];
	    				offset ++;
	    				
		    			if (offset == 16)
		    			{
		    				pages.Add(tmp);
		    				
		    				tmp = new byte[16];
		    				offset = 0;
		    			}
		    		}

					}
  				if (offset != 0)
  				{
  					pages.Add(tmp);
						tmp = new byte[16];
						offset = 0;
  				}
					
				}
    		
    	} else
    	{
    		offset = 0;	   
  			
    		/* We know the number of pages - It is exactly 1 */

  			read_bytes = ReadBinary(0, (byte) nbBytesPerPage);
  			
  			if (read_bytes == null)
  				return false;
  			
  			if (read_bytes.Length != nbBytesPerPage)
  				return false;
	
  			tmp = new byte[16];
  			for (int i=0; i<read_bytes.Length; i++)
  			{

  				if (offset == 0)
  				{
  					addr = new byte[2];
  					addr[0] = (byte)((i >> 8) & 0x00FF);;
  					addr[1] = (byte)(i & 0x00FF);
  					p1p2.Add(addr);
  				}
  				tmp[offset] = read_bytes[i];
  				offset ++;
  				
   				if (offset==16)
  				{
  					pages.Add(tmp);
  					tmp = new byte[16];
  					offset = 0;
  				} 					
  				
  			}
  			
  			if (offset != 0)
  				pages.Add(tmp);
	   
	    }
	    
	    if (p1p2 == null)
    	{
    		maxP1 = 0;
    		maxP2 = 0;
    		return false;
    	}
    	if (p1p2.Count == 0)
    	{
    		maxP1 = 0;
    		maxP2 = 0;
    		return false;
    	}
	    
	    maxP1 = p1p2[p1p2.Count-1][0];
	    maxP2 = (byte) (p1p2[p1p2.Count-1][1] + pages[pages.Count-1].Length);
    	return true;	
    	
    }
    
    public void SetNewPages(List<byte[]> _new_pages)
    {
    	log("Setting new pages ...");
    	new_pages = new List<byte[]>();
    	new_pages = _new_pages;
    }
    
    private bool WriteBinary(ushort address, byte[] data)
		{
		  if (data == null)
		    return false;
		  		  
		  CAPDU capdu = new CAPDU(0xFF, 0xD6, (byte) (address / 0x0100), (byte) (address % 0x0100), data);		
		  
			log("< " + capdu.AsString(" "));
			
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
  
		public bool SetMaxAddress(byte p1, byte p2)
		{
			if ((p1 == 0) && (p2 == 0))
			{
				log("Error : maxP1=0 and maxP2=0");
				return false;
			}
			
			maxP1 = p1;
			maxP2 = p2;
			return true;
		
		}
    
    public override bool ReadAgain()
    {
    	return Read(true);
    }
    
    public bool WriteDeltaPages()
    {
    	log("Writing all the differences ...");
    	
    	/* Mandatory checks	*/
    	if (pages == null)
    	{
    		log("No content read yet !");
    		return false;
    	}
    	
    	if (new_pages == null)
    	{
    		log("New pages not set !");
    		return false;
    	}
    	
    	if (pages.Count != new_pages.Count)
    	{
    		log("The number of new pages doesn't correspond with the number of read pages");
    		return false;
    	}
    	
    	if (pages[0].Length != new_pages[0].Length)
    	{
    		log("The size of the new pages doesn't correspond with the size of read pages");
    		return false;
    	}
    	
    	/* Special case for Innovision cards: they only have one page	*/
    	/* and each of their byte must be written separately					*/
    	if ((type == INNOVISION_JEWEL) || (type == INNOVISION_TOPAZ))
    	{
    		
    		byte[] new_byte = new byte[1];
    		for (ushort i = 0; i<new_pages.Count; i++)
    		{
    			for (int j = 0; j<new_pages[i].Length; j++)
    			{
    				if (new_pages[i][j] != pages[i][j])
		    		{
		    			
    					//System.Windows.Forms.MessageBox.Show("page " + i + " - byte " + j + " - P1=" + p1p2[i][0]+ " - P2=" +p1p2[i][1]);
    					if (p1p2 == null)
    						return false;
    					
    					if (p1p2.Count <= i)
    						return false;
    					
    					if (p1p2[0].Length < 2)
    						return false;
    					
    					ushort address = (ushort) (p1p2[i][0] * 0x100 + p1p2[i][1] + j);
    					new_byte[0] = new_pages[i][j];
    					
    					if (!WriteBinary(address, new_byte))
		    				return false;
		    			
    					pages[i][j] = new_pages[i][j];
		    			continue;
		    		}
    			}
    		}
    		
    	} else
    	{
    		for (ushort i = 0; i<new_pages.Count; i++)
    		{
    			for (int j = 0; j<new_pages[i].Length; j++)
    			{
    				if (new_pages[i][j] != pages[i][j])
		    		{
		    			
    					//System.Windows.Forms.MessageBox.Show("page " + i + " - byte " + j + " - P1=" + p1p2[i][0]+ " - P2=" +p1p2[i][1]);
    					if (p1p2 == null)
    						return false;
    					
    					if (p1p2.Count <= i)
    						return false;
    					
    					if (p1p2[0].Length < 2)
    						return false;
    					
    					ushort address = (ushort) (p1p2[i][0] * 0x100 + p1p2[i][1]);
    					
    					if (!WriteBinary(address, new_pages[i]))
		    				return false;
		    			
		    			pages[i] = new_pages[i];
		    			continue;
		    		}
    			}
    		}
    	}
    	
    	
    	pages = new_pages;
    	return true;
    }
	
	
	}
}
