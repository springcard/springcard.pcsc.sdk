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
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using SpringCard.LibCs;

namespace SpringCard.PCSC.CardAnalysis
{
	public static class CardAtrList
	{
		private static bool inited = false;
		private static Dictionary<string, List<string>> AtrDescriptionList = new Dictionary<string, List<string>>();
		
		private static void Init()
		{
			if (inited)
				return;
			
			inited = true;
			
			string FileName = "smartcard_list.txt";
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
			
			if (!LoadFromFile(FullFileName))
			{
				Logger.Trace("Failed to load " + FullFileName);
				return;				
			}
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
			
			string sAtr = abAtr.AsString(" ");
			
			foreach (KeyValuePair<string, List<string>> entry in AtrDescriptionList)
			{
				if (Regex.IsMatch(sAtr, entry.Key))
				{
					return entry.Value.ToArray();
				}
			}
			
			return null;
		}
		
		private static void AddDescription(string sAtr, string sDescription)
		{
			if (!AtrDescriptionList.ContainsKey(sAtr))
				AtrDescriptionList[sAtr] = new List<string>();
			
			AtrDescriptionList[sAtr].Add(sDescription);
			
			// Logger.Trace(sAtr + " = " + sDescription);
		}
		
		private static bool LoadFromFile(string SmartcardListFileName)
		{
			if (!File.Exists(SmartcardListFileName))
				return false;
			
			try
			{
				System.IO.StreamReader file = new System.IO.StreamReader(SmartcardListFileName);
				
				string atr_line = null;
				string line = file.ReadLine();
				
				while (line != null)
				{
					if (line.Length > 0)
					{
						if (!line.StartsWith("#"))
						{
							if (line.StartsWith("\t"))
							{
								AddDescription(atr_line, line.Trim());
							}
							else
							{
								atr_line = line.Trim();
							}
						}
					}
					
					line = file.ReadLine();
				}
			}
			catch (Exception)
			{
				return false;
			}
			
			return true;
		}
	}
}
