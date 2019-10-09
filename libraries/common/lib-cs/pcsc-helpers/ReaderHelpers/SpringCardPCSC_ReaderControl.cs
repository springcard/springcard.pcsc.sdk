/**h* SpringCard/PCSC_Utils
 *
 * NAME
 *   PCSC : PCSC_Utils
 * 
 * DESCRIPTION
 *   SpringCard's misc utilities for the PC/SC API
 *
 * COPYRIGHT
 *   Copyright (c) 2010-2015 SpringCard - www.springcard.com
 *
 * AUTHOR
 *   Johann.D / SpringCard
 *
 **/
using System;
using System.Drawing;
using System.Collections.Generic;
using SpringCard.PCSC;
using SpringCard.LibCs;

namespace SpringCard.PCSC.ReaderHelper
{
	public static class ReaderCommands
	{
		public const byte ControlCode = 0x58;
		
		public enum Opcode : byte
		{
            WriteRegister = 0x0D,
            ReadRegister = 0x0E,
            ReadI2c = 0x12,
            WriteI2c = 0x13,
            SetBuzzerOld = 0x1C,
            SetLedsOld = 0x1E,
            ReadProductString = 0x20,
            ReadSlotString = 0x21,
            SlotStop = 0x22,
            SlotStart = 0x23,
            NfcPoller = 0x62,
            NfcListener = 0x63,
            WriteRegisterTemp = 0x8D,
            Reset = 0x8E,
            PlaySequence = 0x90,
			SetRgbLed = 0x91,
			SetLeds = 0x92,
			SetBuzzer = 0x93,
			SetSound = 0x94,
			SetVibrator = 0x95,
			PlaySequenceBuffer = 0x98,
		};
		
		public enum LedPattern : byte
		{
			Off = 0x00,
			On = 0x01,
			Blink__0_25Hz = 0x02,
			Blink__0_50Hz = 0x03,
			Blink__1Hz = 0x04,
			Blink__2Hz = 0x05,
			Blink__4Hz = 0x06,
			Blink__5Hz = 0x07,
			Blink__6_66Hz = 0x08,
			Blink__10Hz = 0x09,
			Blink__20Hz = 0x0A,
			Breath = 0x0F,
			
			Flash__0_25Hz__1_outof_4 = 0x12,
			Flash__0_50Hz__1_outof_4 = 0x13,
			Flash__1Hz__1_outof_4 = 0x14,
			Flash__2Hz__1_outof_4 = 0x15,
			Flash__4Hz__1_outof_4 = 0x16,
			Flash__5Hz__1_outof_4 = 0x17,
			Flash__6_66Hz__1_outof_4 = 0x18,
			Flash__10Hz__1_outof_4 = 0x19,
			Flash__20Hz__1_outof_4 = 0x1A,
			
			Flash__0_25Hz__1_outof_8 = 0x22,
			Flash__0_50Hz__1_outof_8 = 0x23,
			Flash__1Hz__1_outof_8 = 0x24,
			Flash__2Hz__1_outof_8 = 0x25,
			Flash__4Hz__1_outof_8 = 0x26,
			Flash__5Hz__1_outof_8 = 0x27,
			Flash__6_66Hz__1_outof_8 = 0x28,
			Flash__10Hz__1_outof_8 = 0x29,
			Flash__20Hz__1_outof_8 = 0x2A,
			
			Flash__0_25Hz__1_outof_16 = 0x32,
			Flash__0_50Hz__1_outof_16 = 0x33,
			Flash__1Hz__1_outof_16 = 0x34,
			Flash__2Hz__1_outof_16 = 0x35,
			Flash__4Hz__1_outof_16 = 0x36,
			Flash__5Hz__1_outof_16 = 0x37,
			Flash__6_66Hz__1_outof_16 = 0x38,
			Flash__10Hz__1_outof_16 = 0x39,
			Flash__20Hz__1_outof_16 = 0x3A,

			Flash__0_25Hz__1_outof_32 = 0x42,
			Flash__0_50Hz__1_outof_32 = 0x43,
			Flash__1Hz__1_outof_32 = 0x44,
			Flash__2Hz__1_outof_32 = 0x45,
			Flash__4Hz__1_outof_32 = 0x46,
			Flash__5Hz__1_outof_32 = 0x47,
			Flash__6_66Hz__1_outof_32 = 0x48,
			Flash__10Hz__1_outof_32 = 0x49,
			Flash__20Hz__1_outof_32 = 0x4A,

			Flash__0_25Hz__2_outof_8 = 0x52,
			Flash__0_50Hz__2_outof_8 = 0x53,
			Flash__1Hz__2_outof_8 = 0x54,
			Flash__2Hz__2_outof_8 = 0x55,
			Flash__4Hz__2_outof_8 = 0x56,
			Flash__5Hz__2_outof_8 = 0x57,
			Flash__6_66Hz__2_outof_8 = 0x58,
			Flash__10Hz__2_outof_8 = 0x59,
			Flash__20Hz__2_outof_8 = 0x5A,
			
			Flash__0_25Hz__3_outof_16 = 0x62,
			Flash__0_50Hz__3_outof_16 = 0x63,
			Flash__1Hz__3_outof_16 = 0x64,
			Flash__2Hz__3_outof_16 = 0x65,
			Flash__4Hz__3_outof_16 = 0x66,
			Flash__5Hz__3_outof_16 = 0x67,
			Flash__6_66Hz__3_outof_16 = 0x68,
			Flash__10Hz__3_outof_16 = 0x69,
			Flash__20Hz__3_outof_16 = 0x6A,
			
			Flash__0_25Hz__4_outof_32 = 0x72,
			Flash__0_50Hz__4_outof_32 = 0x73,
			Flash__1Hz__4_outof_32 = 0x74,
			Flash__2Hz__4_outof_32 = 0x75,
			Flash__4Hz__4_outof_32 = 0x76,
			Flash__5Hz__4_outof_32 = 0x77,
			Flash__6_66Hz__4_outof_32 = 0x78,
			Flash__10Hz__4_outof_32 = 0x79,
			Flash__20Hz__4_outof_32 = 0x7A,			
		}
		
		private static string BeautifyName(string name)
		{
			name = name.Replace("__", " ");
			name = name.Replace("_outof_", "/");
			name = name.Replace("_", ".");
			return name;
		}
		
		public static string[] LedPatterns
		{
			get
			{
				Array patterns = Enum.GetValues(typeof(LedPattern));
				
				string[] result = new string[patterns.Length];
				
				int i = 0;
				foreach (LedPattern pattern in patterns)
					result[i++] = BeautifyName(pattern.ToString());
				
				return result;
			}
		}
		
		public static LedPattern GetPatternByName(string patternName)
		{
			Array patterns = Enum.GetValues(typeof(LedPattern));
			foreach (LedPattern pattern in patterns)
				if (BeautifyName(pattern.ToString()).Equals(patternName))
					return pattern;
			return LedPattern.Off;
		}
		
		public static class Sequencer
		{
			public static CardBuffer PlaySequence(byte[] sequence)
			{
				CardBuffer c = new CardBuffer();
				
				c.AppendOne(ControlCode);
				c.AppendOne((byte) Opcode.PlaySequenceBuffer);
				c.Append(sequence);
				
				return c;				
			}
		}
		
		public static class Leds
		{			
			public static CardBuffer Set(LedPattern led1, LedPattern led2, LedPattern led3, LedPattern led4)
			{
				CardBuffer c = new CardBuffer();
				
				c.AppendOne(ControlCode);
				c.AppendOne((byte) Opcode.SetLeds);
				c.AppendOne((byte) led1);
				c.AppendOne((byte) led2);
				c.AppendOne((byte) led3);
				c.AppendOne((byte) led4);
				
				return c;
			}

			public static CardBuffer Set(string led1, string led2, string led3, string led4)
			{
				return Set(GetPatternByName(led1), GetPatternByName(led2), GetPatternByName(led3), GetPatternByName(led4));
			}
			
			public static CardBuffer Set(byte led1, byte led2, byte led3, byte led4)
			{
				return Set((byte) led1, (byte) led2, (byte) led3, (byte) led4);
			}
		}
		
		public static class RgbLed
		{		
			public static CardBuffer SetPattern(LedPattern pattern)
			{
				CardBuffer c = new CardBuffer();
				
				c.AppendOne(ControlCode);
				c.AppendOne((byte) Opcode.SetRgbLed);
				c.AppendOne((byte) pattern);
				
				return c;
			}
			
			public static CardBuffer SetPattern(string patternName)
			{
				return SetPattern(GetPatternByName(patternName));
			}
			
			public static CardBuffer SetColor(Color color)
			{
				CardBuffer c = new CardBuffer();
				
				c.AppendOne(ControlCode);
				c.AppendOne((byte) Opcode.SetRgbLed);
				c.AppendOne(color.R);
				c.AppendOne(color.G);
				c.AppendOne(color.B);
				
				return c;
			}
		}
	}

    public partial class ReaderHelper
    {
        SCardChannel channel;

        public ReaderHelper(SCardChannel channel)
        {
            this.channel = channel;
        }

        public ReaderHelper(SCardReader reader)
        {
            channel = new SCardChannel(reader);
            channel.ShareMode = SCARD.SHARE_DIRECT;
            channel.Protocol = 0;
            channel.Connect();
        }

        public void Disconnect()
        {
            channel.DisconnectLeave();
        }

        public bool ReadRegister(byte address, out byte[] data)
        {
            data = null;

            CardBuffer c = new CardBuffer();

            c.AppendOne(ReaderCommands.ControlCode);
            c.AppendOne((byte)ReaderCommands.Opcode.ReadRegister);
            c.AppendOne(address);

            CardBuffer r = channel.Control(c);

            if ((r == null) || (r.Length == 0))
                return false;

            if (r.GetByte(0) != 0x00)
                return false;

            if (r.Length > 1)
                data = r.GetBytes(1, r.Length - 1);

            return true;
        }

        public bool WriteRegister(byte address, byte[] data)
        {
            data = null;

            CardBuffer c = new CardBuffer();

            c.AppendOne(ReaderCommands.ControlCode);
            c.AppendOne((byte)ReaderCommands.Opcode.WriteRegister);
            c.AppendOne(address);
            if (data != null)
                c.Append(data);

            CardBuffer r = channel.Control(c);

            if ((r == null) || (r.Length == 0))
                return false;

            if (r.GetByte(0) != 0x00)
                return false;

            return true;
        }

        public bool EraseRegisters(bool full = false)
        {
            CardBuffer c = new CardBuffer();

            c.AppendOne(ReaderCommands.ControlCode);
            c.AppendOne((byte)ReaderCommands.Opcode.WriteRegister);
            c.AppendOne(0xFF);
            c.AppendOne(0xDE);
            c.AppendOne(0xAD);
            c.AppendOne(0xDE);
            c.AppendOne(0xAD);

            CardBuffer r = channel.Control(c);

            if ((r == null) || (r.Length == 0))
                return false;

            if (r.GetByte(0) != 0x00)
                return false;

            return true;
        }

        public bool Buzzer(int time_ms)
        {
            CardBuffer c = new CardBuffer();

            c.AppendOne(ReaderCommands.ControlCode);
            c.AppendOne((byte)ReaderCommands.Opcode.SetBuzzerOld);
            c.AppendOne((byte)(time_ms / 0x0100));
            c.AppendOne((byte)(time_ms % 0x0100));

            CardBuffer r = channel.Control(c);

            if ((r == null) || (r.Length == 0))
                return false;

            if (r.GetByte(0) != 0x00)
                return false;

            return true;
        }

        public bool SetLeds(byte[] led_patterns)
        {
            CardBuffer c = new CardBuffer();

            c.AppendOne(ReaderCommands.ControlCode);
            c.AppendOne((byte)ReaderCommands.Opcode.SetLedsOld);
            if (led_patterns != null)
                for (int i=0; i<led_patterns.Length; i++)
                    c.AppendOne(led_patterns[i]);

            CardBuffer r = channel.Control(c);

            if ((r == null) || (r.Length == 0))
                return false;

            if (r.GetByte(0) != 0x00)
                return false;

            return true;
        }

        public bool PlaySequence(byte sequenceIndex)
        {
            CardBuffer c = new CardBuffer();

            c.AppendOne(ReaderCommands.ControlCode);
            c.AppendOne((byte)ReaderCommands.Opcode.PlaySequence);
            c.AppendOne(sequenceIndex);

            CardBuffer r = channel.Control(c);

            if ((r == null) || (r.Length == 0))
                return false;

            if (r.GetByte(0) != 0x00)
                return false;

            return true;
        }

        public bool NfcListener(byte listenerParams)
        {
            CardBuffer c = new CardBuffer();

            c.AppendOne(ReaderCommands.ControlCode);
            c.AppendOne((byte)ReaderCommands.Opcode.NfcListener);
            c.AppendOne(listenerParams);

            CardBuffer r = channel.Control(c);

            if ((r == null) || (r.Length == 0))
                return false;

            if (r.GetByte(0) != 0x00)
                return false;

            return true;
        }

        public bool NfcPoller()
        {
            CardBuffer c = new CardBuffer();

            c.AppendOne(ReaderCommands.ControlCode);
            c.AppendOne((byte)ReaderCommands.Opcode.NfcPoller);

            CardBuffer r = channel.Control(c);

            if ((r == null) || (r.Length == 0))
                return false;

            if (r.GetByte(0) != 0x00)
                return false;

            return true;
        }
    }
}
