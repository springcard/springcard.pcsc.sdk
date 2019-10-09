/*
 * Created by SharpDevelop.
 * User: johann
 * Date: 02/09/2014
 * Time: 11:46
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Windows.Forms;
using SpringCard.LibCs;
using SpringCard.LibCs.Windows;

namespace MultiTest
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
						
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new MainForm());
		}
		
	}
}
