using System;
using System.IO;
using System.Linq;
using Microsoft.Win32;
using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SpringCard.LibCs
{	
	class AppUtils
	{
		static string baseDirectory = null;		

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

				string t = System.Reflection.Assembly.GetExecutingAssembly().Location;

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
			return Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetExecutingAssembly().Location);
		}
		
		public static string ReadSettingString(string name, string default_value = "")
		{
			string r = default_value;
			
			try
			{
				RegistryKey k = Registry.CurrentUser.OpenSubKey("SOFTWARE\\SpringCard\\" + ApplicationName, false);
				r = (string) k.GetValue(name, default_value);
			}
			catch
			{

			}
			
			return r;
		}
		
		public static int ReadSettingInt(string name, int default_value = 0)
		{
			string s = ReadSettingString(name, default_value.ToString());
			int r;
			int.TryParse(s, out r);
			return r;
		}

		public static bool ReadSettingBool(string name, bool default_value = false)
		{
			return (ReadSettingInt(name, default_value ? 1 : 0) != 0);
		}
		
		public static void WriteSettingString(string name, string value)
		{
			try
			{
				RegistryKey k = Registry.CurrentUser.CreateSubKey("SOFTWARE\\SpringCard\\" + ApplicationName);
				k.SetValue(name, value);
			}
			catch 
			{
			
			}
		}
		
		public static void WriteSettingInt(string name, int value)
		{
			WriteSettingString(name, value.ToString());
		}	

		public static void WriteSettingBool(string name, bool value)
		{
			WriteSettingInt(name, value ? 1 : 0);
		}	
		
		public static string ReadSectionSettingString(string section, string name, string default_value = "")
		{
			string r = default_value;
			
			try
			{
				RegistryKey k = Registry.CurrentUser.OpenSubKey("SOFTWARE\\SpringCard\\" + ApplicationName + "\\" + section, false);
				r = (string) k.GetValue(name, default_value);
			}
			catch
			{

			}
			
			return r;
		}
		
		public static int ReadSectionSettingInt(string section, string name, int default_value = 0)
		{
			string s = ReadSectionSettingString(section, name, default_value.ToString());
			int r;
			int.TryParse(s, out r);
			return r;
		}		
			
		public static bool ReadSectionSettingBool(string section, string name, bool default_value = false)
		{
			return (ReadSectionSettingInt(section, name, default_value ? 1 : 0) != 0);
		}

		public static void WriteSectionSettingString(string section, string name, string value)
		{
			try
			{
				RegistryKey k = Registry.CurrentUser.CreateSubKey("SOFTWARE\\SpringCard\\" + ApplicationName + "\\" + section);
				k.SetValue(name, value);
			}
			catch 
			{
			
			}
		}
		
		public static void WriteSectionSettingInt(string section, string name, int value)
		{
			WriteSectionSettingString(section, name, value.ToString());
		}	

		public static void WriteSectionSettingBool(string section, string name, bool value)
		{
			WriteSectionSettingInt(section, name, value ? 1 : 0);
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
