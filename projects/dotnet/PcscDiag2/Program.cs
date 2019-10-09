/*
 * Created by SharpDevelop.
 * User: johann
 * Date: 02/03/2012
 * Time: 17:56
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Windows.Forms;
using SpringCard.LibCs;
using SpringCard.LibCs.Windows;

namespace PcscDiag2
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
