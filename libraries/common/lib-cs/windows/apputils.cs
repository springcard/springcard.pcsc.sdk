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
using System.IO;
using Microsoft.Win32;
using System.Threading;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace SpringCard.LibCs.Windows
{
	public class AppUtils
	{
		static string baseDirectory = null;
		public static string RunAsSID = null;
		public static string CompanyName = "SpringCard";

		public static RegistryKey ApplicationUserRegistryKey(string ApplicationRelativePath, bool OpenReadWrite = false)
		{
			RegistryKey key;

			if (RunAsSID == null)
			{
				key = Registry.CurrentUser;
			}
			else
			{
				key = Registry.Users.OpenSubKey(RunAsSID, OpenReadWrite);
			}

			if (key != null)
			{
				if (OpenReadWrite)
				{
					key = key.CreateSubKey(ApplicationRelativePath);
				}
				else
				{
					key = key.OpenSubKey(ApplicationRelativePath);
				}
			}

			return key;
		}

		/// <summary>
		/// Used to know if the current assembly is running from an install folder, we hope...
		/// </summary>
		/// <param name="gestfileReference"></param>
		/// <returns></returns>
		public static bool isRunningFromInstallationFolder(string gestfileReference)
		{
			string t = System.Reflection.Assembly.GetEntryAssembly().Location;
			t = Path.GetDirectoryName(t);
			return t.ToLower().Trim().Contains(gestfileReference.Trim().ToLower());
		}

		static class NativeMethods
		{
			[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
			public static extern IntPtr GetCurrentProcess();

			[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
			public static extern IntPtr GetModuleHandle([MarshalAs(UnmanagedType.LPWStr)]string moduleName);

			[DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
			public static extern IntPtr GetProcAddress(IntPtr hModule, [MarshalAs(UnmanagedType.LPWStr)]string procName);

			[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
			[return: MarshalAs(UnmanagedType.Bool)]
			public static extern bool IsWow64Process(IntPtr hProcess, out bool wow64Process);
		}

		/// <summary>
		/// The function determines whether the current operating system is a
		/// 64-bit operating system.
		/// </summary>
		/// <returns>
		/// The function returns true if the operating system is 64-bit;
		/// otherwise, it returns false.
		/// </returns>
		public static bool Is64BitOperatingSystem()
		{
			if (IntPtr.Size == 8) {  // 64-bit programs run only on Win64
				return true;
			} else {  // 32-bit programs run on both 32-bit and 64-bit Windows
				// Detect whether the current process is a 32-bit process
				// running on a 64-bit system.
				bool flag;
				return ((DoesWin32MethodExist("kernel32.dll", "IsWow64Process") &&
				         NativeMethods.IsWow64Process(NativeMethods.GetCurrentProcess(), out flag)) && flag);
			}
		}


		/// <summary>
		/// The function determins whether a method exists in the export
		/// table of a certain module.
		/// </summary>
		/// <param name="moduleName">The name of the module</param>
		/// <param name="methodName">The name of the method</param>
		/// <returns>
		/// The function returns true if the method specified by methodName
		/// exists in the export table of the module specified by moduleName.
		/// </returns>
		static bool DoesWin32MethodExist(string moduleName, string methodName)
		{
			IntPtr moduleHandle = NativeMethods.GetModuleHandle(moduleName);
			if (moduleHandle == IntPtr.Zero) {
				return false;
			}
			return (NativeMethods.GetProcAddress(moduleHandle, methodName) != IntPtr.Zero);
		}

		public static string BaseDirectory
		{
			get
			{
				if (baseDirectory != null)
					return baseDirectory;

				string t = System.Reflection.Assembly.GetEntryAssembly().Location;

				t = Path.GetDirectoryName(t);

				if (t.ToLower().EndsWith("_output")) {
					t = Path.GetDirectoryName(t);
					t = t.Replace("_output", "");
				}

				if (t.ToLower().EndsWith("release") || t.ToLower().EndsWith("debug"))
					t = Path.GetDirectoryName(t);

				if (t.ToLower().EndsWith("bin"))
					t = Path.GetDirectoryName(t);

				return t;
			}
			set
			{
				baseDirectory = value;
			}
		}

		private static string applicationName = null;

		public static string ApplicationName
		{
			get
			{
				if (applicationName == null)
					applicationName = ApplicationNameAuto();
				return applicationName;
			}
			set
			{
				applicationName = value;
			}
		}

		public static string ApplicationNameAuto()
		{
			return Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location);
		}
		
		public static string ApplicationTitle(bool withVersion = false)
		{
			FileVersionInfo i = FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location);
			string result = i.CompanyName + " " + i.ProductName;
			if (withVersion)
			{
				string[] e = i.ProductVersion.Split('.');
				if (e.Length < 4)
					result += i.ProductVersion;
				else
					result += " v." + string.Format("{0}.{1} [{2}.{3}]", e[0], e[1], e[2], e[3]);
			}
			return result;
		}
		
		public static string ApplicationExeName
		{
			get
			{
				return Assembly.GetEntryAssembly().Location;
			}
		}


        public static bool RegisterForAutoStart(bool Globals = false)
        {
            try
            {
                RegistryKey k;

                if (Globals)
                    k = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                else
                    k = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

                k.SetValue(ApplicationName, Assembly.GetEntryAssembly().Location);

                if (Globals)
                    UnregisterForAutoStart(false);

                return IsRegisteredForAutoStart(Globals);
            }
            catch
            {
                return false;
            }
        }

        public static bool UnregisterForAutoStart(bool Globals = false)
        {
            try
            {
                RegistryKey k;

                if (Globals)
                    k = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                else
                    k = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

                k.DeleteValue(ApplicationName);

                if (Globals)
                    UnregisterForAutoStart(false);

                return !IsRegisteredForAutoStart(Globals);
            }
            catch
            {
                return false;
            }
        }

        public static bool IsRegisteredForAutoStart(bool Globals = false)
        {
            try
            {
                RegistryKey k;

                if (Globals)
                    k = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", false);
                else
                    k = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", false);

                string r = (string)k.GetValue(ApplicationName);
                if (string.IsNullOrEmpty(r)) return false;
                if (r != Assembly.GetEntryAssembly().Location) return false;
                return true;
            }
            catch
            {
                return false;
            }
        }

        [DllImport("user32.dll")] private static extern bool IsIconic(IntPtr hWnd);
		[DllImport("user32.dll")] private static extern bool ShowWindowAsync(IntPtr hWnd, int cmdShow);
		[DllImport("user32.dll")] private static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")] private static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

        private const int SW_HIDE = 0;
		private const int SW_SHOWNORMAL = 1;
		private const int SW_SHOWMINIMIZED = 2;
		private const int SW_SHOWMAXIMIZED = 3;
		private const int SW_SHOWNOACTIVATE = 4;
		private const int SW_RESTORE = 9;
		private const int SW_SHOWDEFAULT = 10;

        private const UInt32 WM_CLOSE = 0x0010;

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

				string mutexName = "Local\\SingleInstance_" + CompanyName + "_" + UniqueName;

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

        public static string ApplicationGuid
		{
			get
			{
				Assembly assembly = Assembly.GetEntryAssembly();
				try
				{
					GuidAttribute attribute = (GuidAttribute) assembly.GetCustomAttributes(typeof(GuidAttribute), true)[0];
					return attribute.Value;
				}
				catch
				{
					/* The application does not have a GUID */
					return null;
				}
			}
		}
		

		/// <summary>
		/// Validate a (auth) key
		/// </summary>
		/// <param name="key">The key to validate</param>
		/// <param name="maxLength">The awaited key size</param>
		/// <returns>True if the key is valid, else false</returns>
		/*public static bool isKeyValid(string key, int maxLength = 16)
		{
			if (key.Trim().Equals(""))
				return false;

			if (key.Length != maxLength)
				return false;

			for (int i = 0; i < key.Length; i++)
				if (!BinConvert.IsHex(key[i]))
					return false;
			return true;
		}*/

	}

}
