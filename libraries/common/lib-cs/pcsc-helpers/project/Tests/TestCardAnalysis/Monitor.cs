/*
 * Created by SharpDevelop.
 * User: johann
 * Date: 15/05/2017
 * Time: 14:21
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using SpringCard.PCSC;
using SpringCard.PCSC.CardAnalysis;

namespace SpringCard.PCSC.SelfTest
{
	/// <summary>
	/// Description of Monitor.
	/// </summary>
	public class Monitor
	{
		SCardReaderList readers;
		List<string> readersAlreadyAnalyzed = new List<string>();
		Dictionary<string, Process> readersProcessing = new Dictionary<string, Process>();
		private Object mutex = new Object();
			
		public Monitor()
		{
		}
		
		public void Start()
		{
			readers = new SCardReaderList();
			readers.StartMonitor(new SCardReaderList.StatusChangeCallback(ReaderStatusChangeCallback));
		}
		
		public void Stop()
		{
			readers.StopMonitor();
		}
		
		public void ReaderProcessDone(string readerName, bool success)
		{			
			if (!success)
			{
				lock(mutex)
				{
					readersProcessing.Remove(readerName);
				}
			}
		}
		
		public void ReaderStatusChangeCallback(string readerName, uint readerState, CardBuffer cardAtr)
		{
			Console.WriteLine("ReaderStatusChangeCallback");
			if (readerName != null)
				Console.WriteLine(String.Format("\tReader={0}", readerName));
			Console.WriteLine(String.Format("\tState={0:X08} ({1})", readerState, SCARD.ReaderStatusToString(readerState)));
			if (cardAtr != null)
				Console.WriteLine(String.Format("\tCard's ATR={0}", cardAtr.AsString(" ")));			
						
			if ((cardAtr != null) && ((readerState & SCARD.STATE_PRESENT) != 0) && ((readerState & SCARD.STATE_INUSE) == 0))
			{
				bool f;
				
				lock (mutex)
				{
					f = readersAlreadyAnalyzed.Contains(readerName);
					if (!f)
						readersAlreadyAnalyzed.Add(readerName);
				}
				
				if (!f)
				{					
					AnalyzeCard(readerName, cardAtr);
				}
				
				Process p = null;
				
				lock (mutex)					
				{
					f = readersProcessing.ContainsKey(readerName);
					if (!f)
					{
						p = new Process(readerName, new Process.ProcessDone(ReaderProcessDone));
						readersProcessing.Add(readerName, p);
					}
				}
				
				if (p != null)
				{
					p.Start();
				}				
				
				
			} else
			if ((readerState & SCARD.STATE_EMPTY) != 0)
			{
				lock (mutex)
				{
					readersAlreadyAnalyzed.Remove(readerName);
					readersProcessing.Remove(readerName);
				}
			}
			
		}
		
		public void AnalyzeCard(string readerName, CardBuffer cardAtr)
		{
			string[] description;
			
			CardAtrParser cardAtrParser = new CardAtrParser(cardAtr);
			description = cardAtrParser.TextDescription();
			if (description != null)
			{
				Console.WriteLine("\tATR explanation:");
				foreach (string s in description)
				{
					Console.WriteLine(String.Format("\t\t{0}", s));
				}
			}
			
			description = CardAtrList.Descriptions(cardAtr);
			if (description != null)
			{
				Console.WriteLine("\tPossibly identified card:");
				foreach (string s in description)
				{
					Console.WriteLine(String.Format("\t\t{0}", s));
				}
			}
			
			if (CardPixList.IsPcscWiredLogicCard(cardAtr))
			{
				description = CardPixList.Descriptions(cardAtr);
				if (description != null)
				{
					Console.WriteLine("\tPC/SC v2 data for wired-logic cards:");				
					foreach (string s in description)
					{
						Console.WriteLine(String.Format("\t\t{0}", s));
					}				
				}				
			}
		}
	}
}
