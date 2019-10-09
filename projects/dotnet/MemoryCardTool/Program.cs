/*
 * Created by SharpDevelop.
 * User: jerome.i
 * Date: 14/06/2013
 * Time: 13:47
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using SpringCard.LibCs;
using SpringCard.LibCs.Windows;
using System;
using System.Windows.Forms;

namespace MemoryCardTool
{
	/// <summary>
	/// Class with program entry point.
	/// </summary>
	internal sealed class Program
	{
		/// <summary>
		/// Program entry point.
		/// </summary>
		[STAThread]
		private static void Main(string[] args)
		{
			SystemConsole.ReadArgs(args);
			Logger.ReadArgs(args);			
            SpringCard.PCSC.SCARD.UseLogger = true;
			
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new MainForm(args));
		}
		
	}
}
