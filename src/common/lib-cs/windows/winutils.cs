using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Xml;
using System.Threading;
using System.Windows;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SpringCard.LibCs
{
	class WinUtils
	{		
		public static void FatalError(string message, string title = "Internal error")
		{
            System.Windows.Forms.MessageBox.Show(message + "\n\n" + Translatable.GetTranslation("FatalError", "This is a fatal error. The application will now terminate."), title);
			System.Environment.Exit(0);			
		}
		
		public static void ShowMessage(string message)
		{
            System.Windows.Forms.MessageBox.Show(message);
		}
		
		[DllImport("user32.dll")] private static extern bool IsIconic(IntPtr hWnd);
		[DllImport("user32.dll")] private static extern bool ShowWindowAsync(IntPtr hWnd, int cmdShow);
		[DllImport("user32.dll")] private static extern bool SetForegroundWindow(IntPtr hWnd);
		
		private const int SW_HIDE = 0;
		private const int SW_SHOWNORMAL = 1;
		private const int SW_SHOWMINIMIZED = 2;
		private const int SW_SHOWMAXIMIZED = 3;
		private const int SW_SHOWNOACTIVATE = 4;
		private const int SW_RESTORE = 9;
		private const int SW_SHOWDEFAULT = 10;
		
		private static Mutex singleInstanceMutex = null;
		private static bool singleInstanceOwner = false;
		
		public static bool IsSingleInstance(string UniqueName = null)
		{
			if (singleInstanceMutex == null)
				DeclareInstance(UniqueName);
			
			return singleInstanceOwner;
		}

		public static void DeclareInstance(string UniqueName = null)
		{
			string processName = Process.GetCurrentProcess().ProcessName;
			
			if (singleInstanceMutex == null)
			{
				if (UniqueName == null)
					UniqueName = processName;
	
				string mutexName = "Local\\SingleInstance_SpringCard_" + UniqueName;
				
				singleInstanceMutex = new Mutex(false, mutexName);
			}
			
			if (!singleInstanceOwner)
				singleInstanceOwner = singleInstanceMutex.WaitOne(0);			
		}
		
		public static void ReleaseInstance()
		{
			if (singleInstanceMutex != null)
			{
				if (singleInstanceOwner)
					singleInstanceMutex.ReleaseMutex();
				
				singleInstanceMutex.Dispose();
				singleInstanceMutex = null;
			}
		}

		public static bool RestorePreviousInstance()
		{
			Process thisProcess = Process.GetCurrentProcess();
			string processName = thisProcess.ProcessName;			
			Process[] processes = Process.GetProcessesByName(processName);
			
			if (processes.Length > 1)
			{
				for (int i=0; i<processes.Length; i++)
				{
					if (processes[i].Id != thisProcess.Id)
					{
						/* Restore this instance */
						IntPtr hWnd = processes[i].MainWindowHandle;
						if (IsIconic(hWnd))
							ShowWindowAsync(hWnd, SW_RESTORE);
						if (SetForegroundWindow(hWnd))
							return true;
					}
				}
			}
			
			return false;
		}		
	}		
}
