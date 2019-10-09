/**
 *
 * \ingroup Windows
 *
 * \copyright
 *   Copyright (c) 2008-2018 SpringCard - www.springcard.com
 *   All right reserved
 *
 * \author
 *   Johann.D et al. / SpringCard
 *
 */
/*
 * Read LICENSE.txt for license details and restrictions.
 */
using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO;

namespace SpringCard.LibCs.Windows
{
	/**
	 * \brief Utility to manipulate the console that is attached to the process
	 */
	public class SystemConsole
	{
		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool AllocConsole();

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool FreeConsole();

		[DllImport("kernel32", SetLastError = true)]
		public static extern bool AttachConsole(int dwProcessId);

		[DllImport("user32.dll")]
		public static extern IntPtr GetForegroundWindow();

		[DllImport("user32.dll", SetLastError = true)]
		public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

		private static bool visible = false;

		/**
		 * \brief Show the process' console
		 */
		public static void Show()
		{
			if (visible)
				return;

			IntPtr ptr = GetForegroundWindow();
			int  u;
			GetWindowThreadProcessId(ptr, out u);
			Process process = Process.GetProcessById(u);
			if (process.ProcessName.Equals("cmd"))    //Is the uppermost window a cmd process?
			{
				AttachConsole(process.Id);
			}
			else
			{
				AllocConsole();
			}

			// rebind standart output to the current console
			StreamWriter standardOutput = new StreamWriter(System.Console.OpenStandardOutput());
	        standardOutput.AutoFlush = true;
	        Console.SetOut(standardOutput);

			ConsoleColor oldBackgroundColor = Console.BackgroundColor;
			ConsoleColor oldForegroundColor = Console.ForegroundColor;
			
			Console.OutputEncoding = System.Text.Encoding.UTF8;

			Console.WriteLine();

			Console.BackgroundColor = ConsoleColor.White;
			Console.ForegroundColor = ConsoleColor.Black;

			Console.Write(Application.CompanyName);

			Console.BackgroundColor = oldBackgroundColor;
			Console.ForegroundColor = oldForegroundColor;

			Console.WriteLine();

			Console.BackgroundColor = ConsoleColor.White;
			Console.ForegroundColor = ConsoleColor.Black;

			Console.Write(Application.ProductName + " v."+Application.ProductVersion);

			Console.BackgroundColor = oldBackgroundColor;
			Console.ForegroundColor = oldForegroundColor;

			Console.WriteLine();

			visible = true;
		}

		/**
		 * \brief Hide the process' console
		 */
		public static void Hide()
		{
			visible = false;
			FreeConsole();
		}

		/**
		 * \brief Read the program's arguments and show the console if required
		 */
		public static void ReadArgs(string[] args)
		{
			try
			{
				bool show_console = false;
				if (args != null)
				{
					for (int i=0; i<args.Length; i++)
					{
						string s = args[i].ToLower();
						if ((s == "--console"))
						{
							show_console = true;
						}
						else if (s.StartsWith("--verbose") || s.StartsWith("-v"))
						{
							show_console = true;
						}
					}
				}

				if (show_console)
					Show();
			}
			catch {}
		}
	}
}