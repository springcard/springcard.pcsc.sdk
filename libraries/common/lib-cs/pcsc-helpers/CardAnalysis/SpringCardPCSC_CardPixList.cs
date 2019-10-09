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
using SpringCard.LibCs;

namespace SpringCard.PCSC.CardAnalysis
{
	public static class CardPixList
	{
		private static readonly byte[] PcscContactlessCardAtrBegin = {
			0x3B, 0x8F, 0x80, 0x01, 0x80, 0x4F, 0x0C, 0xA0, 0x00, 0x00, 0x03, 0x06
		};
		private static bool inited = false;		
		private static IniFile inifile;
		
		private static void Init()
		{
			if (inited)
				return;
			
			inited = true;
			
			string FileName = "pix_values.ini";
			string[] SearchDirectories = {
				@".",
				@".\conf",
				@"..\conf",
				@".."				
			};
			
			string FullFileName = FileUtils.LocateFile(FileName, SearchDirectories);
			
			if (FullFileName == null)
			{
				Logger.Trace("File " + FileName + " not found");
				return;
			}
			
			inifile = new IniFile(FullFileName, false, true);
		}
		
		public static string SS_String(byte ss)
		{
			Init();
			
			if (inifile == null)
				return "";
			
			string ss_string = String.Format("{0:X02}", ss);
			return inifile.ReadString("PIXSS", ss_string, "");
		}
		
		public static string NN_String(ushort nn)
		{
			Init();
			
			if (inifile == null)
				return "";			

			string nn_string = String.Format("{0:X04}", nn);
			return inifile.ReadString("PIXNN", nn_string, "");			
		}
		
		public static string[] Descriptions(string sAtr)
		{
			Init();
			
			CardBuffer abAtr = new CardBuffer(sAtr);
			if ((abAtr == null) || (abAtr.Length == 0))
				return null;
			
			return Descriptions(abAtr);
		}
		
		public static string[] Descriptions(CardBuffer abAtr)
		{
			Init();
			
			List<string> result = new List<string>();
			
			if (!abAtr.StartsWith(PcscContactlessCardAtrBegin))
			{
				result.Add("This ATR does not belong to a contactless wired-logic card");
			}
			else
			{
				result.Add("This is the ATR of a contactless wired-logic card");
				
				string s;
				byte[] data = BinUtils.Copy(abAtr.Bytes, PcscContactlessCardAtrBegin.Length, 3, true);
				byte ss = data[0];
				s = SS_String(ss);
				if (!string.IsNullOrEmpty(s))
					result.Add("Contactless protocol: " + s);				
				ushort nn = (ushort) ((data[1] * 0x0100) + data[2]);
				s = NN_String(nn);
				if (!string.IsNullOrEmpty(s))
					result.Add("Card family: " + s);
			}
			
			return result.ToArray();
		}
		
		public static bool IsPcscWiredLogicCard(CardBuffer abAtr)
		{
			Init();
			
			return abAtr.StartsWith(PcscContactlessCardAtrBegin);
		}

		public static byte SS(CardBuffer abAtr)
		{
			if (!abAtr.StartsWith(PcscContactlessCardAtrBegin)) return 0x00;
			byte[] data = BinUtils.Copy(abAtr.Bytes, PcscContactlessCardAtrBegin.Length, 3, true);
			return data[0];
		}
		
		public static ushort NN(CardBuffer abAtr)
		{
			if (!abAtr.StartsWith(PcscContactlessCardAtrBegin)) return 0x00;
			byte[] data = BinUtils.Copy(abAtr.Bytes, PcscContactlessCardAtrBegin.Length, 3, true);
			return (ushort) ((data[1] * 0x0100) + data[2]);
		}
	}
}
