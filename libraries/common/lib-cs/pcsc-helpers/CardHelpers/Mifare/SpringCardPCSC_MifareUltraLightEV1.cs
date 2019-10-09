/**
 *
 * \defgroup MifareUltraLightEV1
 *
 * \brief Mifare UltraLight EV1 library (.NET only, no native depedency)
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
	public class MifareUltraLightEV1 : MifareUltraLight
	{
		/**
		 * \brief Instanciate a Mifare UltraLight EV1 card object over a card channel. The channel must already be connected.
		 */
		public MifareUltraLightEV1(ICardApduTransmitter Channel) : base(Channel)
		{
		}
		
		public byte[] Read(byte first_block, byte count)
		{
			CAPDU capdu = new CAPDU(
				0xFF, // Embedded command interpreter
				0xFE, // Encapsulate
				0x01, // ISO 14443-3 A
				0x04, // wait 848 ETU,
				new byte[] { 0x3A, first_block, (byte) (first_block + count - 1) } // NXP FAST_READ
			);
			
			RAPDU rapdu = Channel.Transmit(capdu);
			
			if ((rapdu != null) && (rapdu.SW == 0x9000) && (rapdu.hasData))
				return rapdu.data.GetBytes();
			
			return null;			
		}

		public byte[] ReadCounter(byte counter)
		{
			CAPDU capdu = new CAPDU(
				0xFF, // Embedded command interpreter
				0xFE, // Encapsulate
				0x01, // ISO 14443-3 A
				0x04, // wait 848 ETU,
				new byte[] { 0x39, counter } // NXP READ_CNT
			);
			
			RAPDU rapdu = Channel.Transmit(capdu);
			
			if ((rapdu != null) && (rapdu.SW == 0x9000) && (rapdu.hasData) && (rapdu.data.Length == 3))
				return rapdu.data.GetBytes();
			
			return null;
		}

		public uint ReadCounterI(byte counter)
		{
			byte[] value = ReadCounter(counter);
			
			if (value == null)
				return 0xFFFFFFFF; /* Invalid value */
			
			uint result = 0;
			
			result += value[0]; result *= 0x00000100;
			result += value[1]; result *= 0x00000100;
			result += value[2];
			
			return result;
		}

		public bool IsCounterValid(byte counter)
		{
			CAPDU capdu = new CAPDU(
				0xFF, // Embedded command interpreter
				0xFE, // Encapsulate
				0x01, // ISO 14443-3 A
				0x04, // wait 848 ETU,
				new byte[] { 0x3E, counter } // NXP CHECK_TEARING_EVENT
			);
			
			RAPDU rapdu = Channel.Transmit(capdu);
			
			if ((rapdu != null) && (rapdu.SW == 0x9000) && (rapdu.hasData) && (rapdu.data.Length == 1))
				return (rapdu.data.GetByte(0) == 0xBD);
			
			return false;
		}
		
		public bool IncrementCounter(byte counter, byte[] increment)
		{
			if ((increment == null) || (increment.Length != 3))
				throw new ArgumentException("INCR_CNT data must be on 3 bytes");
			
			CAPDU capdu = new CAPDU(
				0xFF, // Embedded command interpreter
				0xFE, // Encapsulate
				0x01, // ISO 14443-3 A
				0x04, // wait 848 ETU,
				new byte[] { 0xA5, counter, increment[0], increment[1], increment[2], 0x00 } // NXP INCR_CNT
			);
			
			RAPDU rapdu = Channel.Transmit(capdu);
			
			if ((rapdu != null) && (rapdu.SW == 0x9000))
				return true;
			
			return false;
		}
		
		public bool IncrementCounter(byte counter, uint value)
		{
			byte[] increment = new byte[3];
			
			increment[2] = (byte) (value % 0x00000100); value /= 0x00000100;
			increment[1] = (byte) (value % 0x00000100); value /= 0x00000100;
			increment[0] = (byte) (value % 0x00000100); value /= 0x00000100;
			
			if (value != 0)
				throw new ArgumentException("INCR_CNT data size > 24 bits");
			
			return IncrementCounter(counter, increment);
		}
		
		public byte[] VerifyPassword(byte[] password)
		{
			if ((password == null) || (password.Length != 4))
				throw new ArgumentException("PASSWORD data must be on 4 bytes");
			
			CAPDU capdu = new CAPDU(
				0xFF, // Embedded command interpreter
				0xFE, // Encapsulate
				0x01, // ISO 14443-3 A
				0x04, // wait 848 ETU,
				new byte[] { 0x1B, password[0], password[1], password[2], password[3] } // NXP PWD_AUTH
			);
			
			RAPDU rapdu = Channel.Transmit(capdu);
			
			if ((rapdu != null) && (rapdu.SW == 0x9000) && (rapdu.hasData) && (rapdu.data.Length == 3))
				return rapdu.data.GetBytes();
			
			return null;
		}
		
		public override bool SelfTest()
		{
			if (!base.SelfTest())
				return false;

//			Console.WriteLine("VerifyPassword({0,0,0,0})");
//			Console.WriteLine(new CardBuffer(VerifyPassword(new byte[] {0,0,0,0})).AsString(" "));

			Console.WriteLine("Read(0, 4)");
			Console.WriteLine(new CardBuffer(Read(0, 4)).AsString(" "));
			Console.WriteLine("Read(0, 8)");
			Console.WriteLine(new CardBuffer(Read(0, 8)).AsString(" "));
			Console.WriteLine("Read(0, 12)");
			Console.WriteLine(new CardBuffer(Read(0, 12)).AsString(" "));
			Console.WriteLine("Read(0, 16)");
			Console.WriteLine(new CardBuffer(Read(0, 16)).AsString(" "));
			Console.WriteLine("Read(0, 1)");
			Console.WriteLine(new CardBuffer(Read(0, 1)).AsString(" "));
			Console.WriteLine("Read(1, 3)");
			Console.WriteLine(new CardBuffer(Read(1, 3)).AsString(" "));
			
			Console.WriteLine("ReadCounter(0)");
			Console.WriteLine(new CardBuffer(ReadCounter(0)).AsString(" "));
			Console.WriteLine("ReadCounter(1)");
			Console.WriteLine(new CardBuffer(ReadCounter(1)).AsString(" "));
			Console.WriteLine("ReadCounter(2)");
			Console.WriteLine(new CardBuffer(ReadCounter(2)).AsString(" "));

			Console.WriteLine("IncrementCounter(0, 10)");
			IncrementCounter(0, 10);
			Console.WriteLine("IncrementCounter(1, 1)");
			IncrementCounter(1, 1);
			Console.WriteLine("IncrementCounter(2, 0)");
			IncrementCounter(2, 0);

			Console.WriteLine("ReadCounter(0)");
			Console.WriteLine(new CardBuffer(ReadCounter(0)).AsString(" "));
			Console.WriteLine("ReadCounter(1)");
			Console.WriteLine(new CardBuffer(ReadCounter(1)).AsString(" "));
			Console.WriteLine("ReadCounter(2)");
			Console.WriteLine(new CardBuffer(ReadCounter(2)).AsString(" "));
			
			Console.WriteLine("ReadCounter(0)");
			Console.WriteLine(ReadCounterI(0));
			Console.WriteLine("ReadCounter(1)");
			Console.WriteLine(ReadCounterI(1));
			Console.WriteLine("ReadCounter(2)");
			Console.WriteLine(ReadCounterI(2));

			Console.WriteLine("IsCounterValid(0)");
			Console.WriteLine(IsCounterValid(0));
			Console.WriteLine("IsCounterValid(1)");
			Console.WriteLine(IsCounterValid(1));
			Console.WriteLine("IsCounterValid(2)");
			Console.WriteLine(IsCounterValid(2));
			
			return true;
		}
		
	}
}
