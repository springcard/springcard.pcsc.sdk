/*
 * Created by SharpDevelop.
 * User: johann
 * Date: 02/03/2012
 * Time: 17:56
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using SpringCard.LibCs;
using SpringCard.LibCs.Windows;
using System;
using System.Windows.Forms;

namespace SpringCardApplication
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
            Logger.Debug("Ready");

            Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new MainForm());
		}
		
	}
}
