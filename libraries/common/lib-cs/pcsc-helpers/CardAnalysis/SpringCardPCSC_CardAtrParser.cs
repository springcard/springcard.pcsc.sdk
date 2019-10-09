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
using System.Collections.Generic;
using SpringCard.PCSC;

namespace SpringCard.PCSC.CardAnalysis
{
	public class CardAtrParser
	{
		CardBuffer abAtr;
		List<string> slDescription;
		
		string[] BINQ = new string[16] {
			"0000", "0001", "0010", "0011", "0100", "0101", "0110", "0111",
			"1000", "1001", "1010", "1011", "1100", "1101", "1110", "1111"
		};

		string[] Fi = new string[16] {
			"372", "372", "558", "744", "1116", "1488", "1860", "RFU",
			"RFU", "512", "768", "1024", "1536", "2048", "RFU", "RFU"
		};

		string[] Di = new string[16] {
			"RFU", "1", "2", "4", "8", "16", "32", "RFU",
			"12", "20", "RFU", "RFU", "RFU", "RFU", "RFU", "RFU"
		};

		
		public CardAtrParser()
		{
			
		}
		
		public CardAtrParser(string sAtr)
		{
			Parse(sAtr);
		}
		
		public CardAtrParser(CardBuffer abAtr)
		{
			Parse(abAtr);
		}
		
		public void Parse(string sAtr)
		{
			abAtr = new CardBuffer(sAtr);
			Parse();
		}

		public void Parse(CardBuffer abAtr)
		{
			this.abAtr = abAtr;
			Parse();
		}
		
		public string[] TextDescription()
		{
			return slDescription.ToArray();
		}
		
		private void Parse()
		{
			if (abAtr == null)
				throw new NullReferenceException();
			
			byte[] atr = abAtr.GetBytes();
			int offset = 0;
			byte Y1;
			byte K;
			
			slDescription = new List<string>();
			
			string s = String.Format("+ TS  = {0:X2} --> ", atr[0]);
			switch (atr[0])
			{
				case 0x3B:
					s += "Direct Convention";
					break;
				case 0x3F:
					s += "Inverse Convention";
					break;
				default:
					s += "UNDEFINED";
					break;
			}
			slDescription.Add(s);
			
			/* T0 */
			/* -- */
			Y1 = (byte) (atr[1] / 0x10);
			K  = (byte) (atr[1] % 0x10);
			slDescription.Add(String.Format("+ T0  = {0:X02}, Y1={1}, K={2} (historical bytes)", atr[1], BINQ[Y1], K));
			
			/* TA, TB, TC, TD */
			/* -------------- */

			offset = 2;

			if ((Y1 & 0x01) != 0)
				ATR_analysis_TA(1, 0, ref offset, atr);

			if ((Y1 & 0x02) != 0)
				ATR_analysis_TB(1, 0, ref offset, atr);

			if ((Y1 & 0x04) != 0)
				ATR_analysis_TC(1, 0, ref offset, atr);

			if ((Y1 & 0x08) != 0)
				ATR_analysis_TD(1, ref offset, atr);

			/* Historical bytes */
			/* ---------------- */
			if (K != 0)
			{
				int i;

				slDescription.Add(String.Format("+ {0} historical byte{1}:", K, K > 1 ? "s" : ""));
				
				s = String.Format("  Hex:");
				for (i = 0; i < K; i++)
				{
					if (offset >= atr.Length)
					{
						slDescription.Add(String.Format("ERROR! ATR is truncated; {0} byte(s) missing", K - i));
						break;
					}
					s += String.Format(" {0:X02}", atr[offset + i]);
				}
				slDescription.Add(s);
				
				s = String.Format("  Asc: ");
				for (i = 0; i < K; i++)
				{
					if (offset >= atr.Length)
						break;
					char c = (char) atr[offset + i];
					if ((c <= ' ') || (c >= 0x80))
						s += ".";
					else
						s += c;
				}
				slDescription.Add(s);

				offset += K;
			}

			/* Check TCK */
			/* --------- */
			if (K != 0)
			{
				int i;
				byte TCK = 0;

				for (i = 1; i < (atr.Length - 1); i++)
				{
					TCK ^= atr[i];
				}

				s = String.Format("+ TCK = {0:X02} ", TCK);

				if (TCK == atr[atr.Length - 1])
				{
					s += String.Format("(correct checksum)");
					slDescription.Add(s);
				} else
				{
					s += String.Format("!= {0:X02}", atr[atr.Length - 1]);
					slDescription.Add(s);
					slDescription.Add(String.Format("ERROR! Checksum is invalid"));
				}
			}
		}
		
		/*  _____  _
		 * |_   _|/ \
		 *   | | / _ \
		 *   | |/ ___ \
		 *   |_/_/   \_\
		 */
		void ATR_analysis_TA(byte counter, byte proto, ref int offset, byte[] atr)
		{
			byte value;

			value = atr[offset];
			offset = offset + 1;

			string s = String.Format("+ TA{0} = {1:X02} --> ", counter, value);
			
			if (counter == 1)
			{
				/* TA1 */
				byte F = (byte) (value / 0x10);
				byte D = (byte) (value % 0x10);

				s += String.Format("Fi=%s, Di=%s", Fi[F], Di[D]);

			} else if (counter == 2)
			{
				/* TA2 */
				byte F = (byte) (value / 0x10);
				byte D = (byte) (value % 0x10);

				s += String.Format("Protocol to be used in spec mode: T={0}", D);
				if ((F & 0x08) != 0)
				{
					s += String.Format(" - unable to change");
				} else
				{
					s += String.Format(" - capable to change");
				}
				if ((F & 0x01) != 0)
				{
					s += String.Format(" - implicity defined");
				} else
				{
					s += String.Format(" - defined by interface bytes");
				}
			} else if (counter >= 3)
			{
				/* TA3 (and other) */
				if (proto == 1)
				{
					/* T=1 protocol */
					s += String.Format("IFSC: {0}", value);
				} else
				{
					/* Other protocol than T=1 */
					byte F = (byte) (value / 0x40);
					byte D = (byte) (value % 0x40);

					s += String.Format("Class: ");

					if ((D & 0x01) != 0)
						s += String.Format("A 5V ");
					if ((D & 0x02) != 0)
						s += String.Format("B 3V ");
					if ((D & 0x04) != 0)
						s += String.Format("C 1.8V ");
					if ((D & 0x08) != 0)
						s += String.Format("D RFU ");
					if ((D & 0x10) != 0)
						s += String.Format("E RFU ");

					s += String.Format("Clock stop: ");
					switch (F)
					{
						case 0:
							s += String.Format("not supported");
							break;
						case 1:
							s += String.Format("state L");
							break;
						case 2:
							s += String.Format("state H");
							break;
						case 3:
						default:
							s += String.Format("no preference");
							break;
					}
				}
			}
			
			slDescription.Add(s);
		}

		/*  _____ ____
		 * |_   _| __ )
		 *   | | |  _ \
		 *   | | | |_) |
		 *   |_| |____/
		 */
		void ATR_analysis_TB(byte counter, byte T, ref int offset, byte[] atr)
		{
			byte value;

			value = atr[offset];
			offset = offset + 1;

			string s = String.Format("+ TB{0} = {1:X02} --> ", counter, value);

			if (counter == 1)
			{
				/* TB1 */
				byte I = (byte) (value / 0x20);
				byte PI = (byte) (value % 0x20);

				if (PI == 0)
				{
					s += String.Format("Vpp not connected");
				} else
				{
					s += String.Format("Programming params P:{0}V, I:{1}mA", PI, I);
				}
			} else if (counter == 2)
			{
				/* TB2 */
				s += String.Format("Programming param PI2 (PI1 should be ignored): ");
				if ((value > 49) || (value < 251))
				{
					s += String.Format("{0}(dV)", value);
				} else
				{
					s += String.Format("RFU value {0}", value);
				}
			} else if (counter >= 3)
			{
				if (T == 1)
				{
					byte BWI = (byte) (value / 0x10);
					byte CWI = (byte) (value % 0x10);

					s += String.Format("BWI={0} - CWI={1}", BWI, CWI);
				}
			}

			slDescription.Add(s);
		}

		/*
		 *  _____ ____
		 * |_   _/ ___|
		 *   | || |
		 *   | || |___
		 *   |_| \____|
		 */
		void ATR_analysis_TC(byte counter, byte T, ref int offset, byte[] atr)
		{
			byte value;

			value = atr[offset];
			offset = offset + 1;

			string s = String.Format("+ TC{0} = {1:X02} --> ", counter, value);

			if (counter == 1)
			{
				/* TC1 */
				s += String.Format("EGT={0}", value);
				if (value == 0xFF)
					s += String.Format(" (special value)");
			} else if (counter == 2)
			{
				/* TC2 */
				s += String.Format("Work waiting time= 960 x {0} x (Fi/F)", value);
			} else if (counter == 3)
			{
				/* TC3 */
				if (T == 1)
				{
					s += String.Format("Error dectection code: ");
					if (value == 1)
					{
						s += String.Format("CRC");
					} else if (value == 0)
					{
						s += String.Format("LRC");
					} else
					{
						s += String.Format("RFU");
					}
				}
			}

			slDescription.Add(s);
		}

		/*  _____ ____
		 * |_   _|  _ \
		 *   | | | | | |
		 *   | | | |_| |
		 *   |_| |____/
		 */
		void ATR_analysis_TD(byte counter, ref int offset, byte[] atr)
		{
			byte value, Y, T;

			value = atr[offset];
			offset = offset + 1;

			Y = (byte) (value / 0x10);
			T = (byte) (value % 0x10);

			if (T == 15)
			{
				slDescription.Add(String.Format("Global interface bytes following"));
			}

			slDescription.Add(String.Format("+ TD{0} = {1:X02} --> Y{2}={3}, protocol T={4}\n", counter, value, counter + 1, BINQ[Y], T));
			slDescription.Add(String.Format("-----------"));

			counter++;

			if ((Y & 0x01) != 0)
				ATR_analysis_TA(counter, T, ref offset, atr);

			if ((Y & 0x02) != 0)
				ATR_analysis_TB(counter, T, ref offset, atr);

			if ((Y & 0x04) != 0)
				ATR_analysis_TC(counter, T, ref offset, atr);

			if ((Y & 0x08) != 0)
				ATR_analysis_TD(counter, ref offset, atr);

		}

	}
}
