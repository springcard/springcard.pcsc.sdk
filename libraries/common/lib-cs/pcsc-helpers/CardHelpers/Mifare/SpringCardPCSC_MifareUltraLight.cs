/**
 *
 * \defgroup MifareUltraLight
 *
 * \brief Mifare UltraLight library (.NET only, no native depedency)
 *
 * \author
 *   Johann.D et al. / SpringCard
 */
/*
 * Read LICENSE.txt for license details and restrictions.
 */
using System;
using System.Runtime.InteropServices;
using SpringCard.PCSC;

namespace SpringCard.PCSC.CardHelpers.Mifare
{
	public class MifareUltraLight : CardHelperMifare
    {
		/**
		 * \brief Instanciate a Mifare UltraLight card object over a card channel. The channel must already be connected.
		 */
		public MifareUltraLight(ICardApduTransmitter Channel) : base(Channel)
		{

		}

		public byte[] Read16(byte first_block)
		{
			CAPDU capdu = new CAPDU(
				0xFF, // Embedded command interpreter
				0xB0, // Read
				0x00,
				first_block,
				16 // Le = 16
			);
			
			RAPDU rapdu = Channel.Transmit(capdu);
			
			if ((rapdu != null) && (rapdu.SW == 0x9000) && (rapdu.hasData) && (rapdu.data.Length == 16))
				return rapdu.data.GetBytes();
			
			return null;
		}

		public byte[] Read4(byte block)
		{
			CAPDU capdu = new CAPDU(
				0xFF, // Embedded command interpreter
				0xB0, // Read
				0x00,
				block,
				4 // Le = 4
			);
			
			RAPDU rapdu = Channel.Transmit(capdu);
			
			if ((rapdu != null) && (rapdu.SW == 0x9000) && (rapdu.hasData) && (rapdu.data.Length == 4))
				return rapdu.data.GetBytes();
			
			return null;
		}

		public bool Write4(byte block, byte[] data)
		{
			if ((data == null) || (data.Length != 4))
				throw new ArgumentException("WRITE4 data must be on 4 bytes");
			
			CAPDU capdu = new CAPDU(
				0xFF, // Embedded command interpreter
				0xD6, // Write
				0x00,
				block,
				data
			);
			
			RAPDU rapdu = Channel.Transmit(capdu);
			
			if ((rapdu != null) && (rapdu.SW == 0x9000))
				return true;
			
			return false;
		}

		public bool Write4(byte block, byte[] data, int offset)
		{
			if ((data == null) || (data.Length < offset + 4))
				throw new ArgumentException("WRITE4 data must be on 4 bytes");
			
			int blockCopy = block + offset / 4;

			if (blockCopy >= 256)
				throw new ArgumentException("WRITE4 block must be <= 255");
			
			byte[] dataCopy = new byte[4];
			Array.Copy(data, offset, dataCopy, 0, 4);
			
			return Write4((byte) blockCopy, dataCopy);
		}
		
		public byte[] GetVersion()
		{
			CAPDU capdu = new CAPDU(
				0xFF, // Embedded command interpreter
				0xFE, // Encapsulate
				0x01, // ISO 14443-3 A
				0x04, // wait 848 ETU,
				new byte[] { 0x60 } // NXP GET_VERSION
			);
			
			RAPDU rapdu = Channel.Transmit(capdu);
			
			if ((rapdu != null) && (rapdu.hasData))
				return rapdu.data.GetBytes();
			
			return null;
		}
		
		public byte[] GetSignature()
		{
			CAPDU capdu = new CAPDU(
				0xFF, // Embedded command interpreter
				0xFE, // Encapsulate
				0x01, // ISO 14443-3 A
				0x04, // wait 848 ETU,
				new byte[] { 0x3C, 0x00 } // NXP READ_SIG
			);
			
			RAPDU rapdu = Channel.Transmit(capdu);
			
			if ((rapdu != null) && (rapdu.hasData))
				return rapdu.data.GetBytes();
			
			return null;
		}
		
		public override bool SelfTest()
		{
			Console.WriteLine("Read16(0)");
			Console.WriteLine(new CardBuffer(Read16(0)).AsString(" "));
			Console.WriteLine("Read16(4)");
			Console.WriteLine(new CardBuffer(Read16(4)).AsString(" "));
			Console.WriteLine("Read4(4)");
			Console.WriteLine(new CardBuffer(Read4(4)).AsString(" "));
			Console.WriteLine("Read4(5)");
			Console.WriteLine(new CardBuffer(Read4(5)).AsString(" "));
			Console.WriteLine("Read4(6)");
			Console.WriteLine(new CardBuffer(Read4(6)).AsString(" "));
			Console.WriteLine("Read4(7)");
			Console.WriteLine(new CardBuffer(Read4(7)).AsString(" "));
			Console.WriteLine("GetVersion");
			Console.WriteLine(new CardBuffer(GetVersion()).AsString(" "));
			Console.WriteLine("GetSignature");
			Console.WriteLine(new CardBuffer(GetSignature()).AsString(" "));
			return true;
		}
	}
}

