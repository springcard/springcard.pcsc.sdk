/*
 * Created by SharpDevelop.
 * User: johann
 * Date: 15/05/2017
 * Time: 15:14
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Threading;
using SpringCard.LibCs;
using SpringCard.PCSC.CardHelpers;

namespace SpringCard.PCSC.SelfTest
{
	/// <summary>
	/// Description of Process.
	/// </summary>
	public class Process
	{
		Thread thread;
		string readerName;
		public delegate void ProcessDone(string readerName, bool success);
		ProcessDone processDone;
		
		public Process(string readerName, ProcessDone processDone)
		{
			this.readerName = readerName;
			this.processDone = processDone;
			
			thread = new Thread(ProcessCore);
		}
		
		public void Start()
		{
			thread.Start();
		}
		
		void ProcessCore()
		{
			bool success = ProcessCard(readerName);
			
			processDone(readerName, success);			
		}
		
		bool ProcessCard(string readerName)
		{
			Console.WriteLine("Connecting to card in " + readerName);
			
			SCardChannel cardChannel = new SCardChannel(readerName);
			
			if (!cardChannel.Connect())
			{
				Console.WriteLine("Failed: " + cardChannel.LastErrorAsString);
				return false;
			}
			
			Console.WriteLine("Connected!");
			
			CardHelper cardHelper = CardHelper.Instantiate(cardChannel);
			if (cardHelper == null)
            {
                Console.WriteLine("No card helper!");
                cardChannel.Disconnect();
                return false;
            }

			Console.WriteLine("Got an instance of " + cardHelper.GetType().ToString());

            cardHelper.SelfTest();
			
			cardChannel.Disconnect();
			
			return true;
		}		
	}
}
