/*
 * Created by SharpDevelop.
 * User: johann
 * Date: 15/05/2017
 * Time: 14:20
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace SpringCard.PCSC.SelfTest
{
	class Program
	{
		public static void Main(string[] args)
		{
			Monitor monitor = new Monitor();
			monitor.Start();
			
			Console.ReadKey(true);			
			monitor.Stop();
		}
	}
}