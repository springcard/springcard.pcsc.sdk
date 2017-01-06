using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Microsoft.Win32;
using System.IO;

namespace SpringCard.LibCs
{
	class SystemConsole
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
			StreamWriter standardOutput = new StreamWriter(Console.OpenStandardOutput());
	        standardOutput.AutoFlush = true;
	        Console.SetOut(standardOutput);

			Console.WriteLine("\n" + Application.ProductName + " v."+Application.ProductVersion+"\n");			
			visible = true;
		}
		
		public static void Free()
		{
			visible = false;			
			FreeConsole();
		}
	}
}