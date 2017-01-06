using System;
using System.Threading;
using System.Diagnostics;
using Microsoft.Win32;
#if WINDOWS_GUI
using System.Windows.Forms;
#endif

using System.IO;
using System.Reflection;

namespace SpringCard.LibCs
{
	public class Logger
	{
		public enum Level
		{
			None = -1,
			Fatal,
			Error,
			Warning,
			Info,
			Trace,
			Debug,
			All
		};
		
		public static Level DebugLevel = Level.Debug;
		public static Level TraceLevel = Level.Trace;
		public static Level ConsoleLevel = Level.Info;

		public static Level EventLogLevel = Level.Trace;
		public static Level FileLogLevel = Level.Info;
		private static Level syslogLogLevel = Level.Info;

		public static bool useFatalWindow = true;
		
		private static EventLog eventLog = null;
		private static string logFileName = null;
		private static Mutex logFileMutex = null;
		
		private static ISyslogMessageSerializer syslogSerializer = null;
		private static ISyslogMessageSender syslogSender = null;
		private static SysLog.Facility syslogFacility = SysLog.Facility.LocalUse0;
		private static string syslogMachineName = null;
		private static string syslogApplicationName = null;

		private static object locker = new object();

		public static Level SyslogLogLevel {
			get {
				return syslogLogLevel;
			}
			set {
				syslogLogLevel = value;
			}
		}

#if WINDOWS_GUI
		public static void LoadConfigFromRegistry()
		{
			LoadConfigFromRegistry(Application.CompanyName, Application.ProductName);
		}
#endif

		public static Level IntToLevel(int intLevel)
		{
			switch (intLevel) {
				case -1:
					return Level.None;
				case 0:
					return Level.Fatal;
				case 1:
					return Level.Error;
				case 2:
					return Level.Warning;
				case 3:
					return Level.Info;
				case 4:
					return Level.Trace;
				case 5:
					return Level.Debug;
				default :
					if (intLevel >= 6)
						return Level.All;
					return Level.Warning;
			}			
		}
		
		public static void LoadConfigFromRegistry(string CompanyName, string ProductName)
		{
			try {
				Level verboseLevel = Level.None;
				
				RegistryKey k = Registry.CurrentUser.OpenSubKey("SOFTWARE\\" + CompanyName + "\\" + ProductName, false);
				string s = (string)k.GetValue("VerboseLevel", "");
				
				int i;
				if (int.TryParse(s, out i)) {
					if (i >= (int)Level.Debug)
						verboseLevel = Level.Debug;
					else if (i >= (int)Level.Trace)
						verboseLevel = Level.Trace;
					else if (i >= (int)Level.Info)
						verboseLevel = Level.Info;
					else if (i >= (int)Level.Warning)
						verboseLevel = Level.Warning;
					else if (i >= (int)Level.Error)
						verboseLevel = Level.Error;
					else if (i >= (int)Level.Fatal)
						verboseLevel = Level.Fatal;
				} else {
					for (i = (int)Level.Debug; i > (int)Level.None; i--) {
						if (s.ToLower() == ((Level)i).ToString().ToLower()) {
							verboseLevel = (Level)i;
							break;
						}
					}
				}
				
				s = (string)k.GetValue("VerboseFile");
				if (!string.IsNullOrEmpty(s)) {
					logFileName = s;
					if (logFileMutex == null)
						logFileMutex = new Mutex();
				}
				
				ConsoleLevel = verboseLevel;
			} catch {
			}
		}

		public static void OpenSysLog(SysLog.Facility facility, string applicationName, string serverAddr, int serverPort = 514, bool useRfc5424 = false)
		{
			try {
				syslogFacility = facility;
				syslogMachineName = Environment.MachineName;
				if (String.IsNullOrEmpty(applicationName))
					syslogApplicationName = System.AppDomain.CurrentDomain.FriendlyName;
				else
					syslogApplicationName = applicationName;

				syslogApplicationName += String.Format("[{0}]", Process.GetCurrentProcess().Id);

				if (useRfc5424)
					syslogSerializer = new SyslogRfc5424MessageSerializer();
				else
					syslogSerializer = new SyslogRfc3164MessageSerializer();
				syslogSender = new SyslogUdpSender(serverAddr, serverPort);
			} catch (Exception e) {
				syslogSender = null;
				syslogSerializer = null;
				Log(Level.Warning, String.Format("Failed to create Syslog sender ({0})", e.Message));
			}            
		}

		public static void OpenSysLog(SysLog.Facility facility, string ServerAddr, int ServerPort = 514, bool useRfc5424 = false)
		{
			OpenSysLog(facility, null, ServerAddr, ServerPort, useRfc5424);
		}
		
		public static void OpenSysLog(string ServerAddr, int ServerPort = 514, bool useRfc5424 = false)
		{
			OpenSysLog(syslogFacility, ServerAddr, ServerPort, useRfc5424);
		}
		
		public static void OpenLogFile(string fileName)
		{
			logFileName = fileName;
			
			string logFilePath = Path.GetDirectoryName(fileName);
			logFilePath = logFilePath.TrimEnd(Path.DirectorySeparatorChar);

			if (!String.IsNullOrEmpty(logFilePath))
			if (!Directory.Exists(logFilePath))
				Directory.CreateDirectory(logFilePath);
			if (logFileMutex == null)
				logFileMutex = new Mutex();
		}
		
		public static void OpenEventLog(string log, string source)
		{
			try {
				if (!EventLog.SourceExists(source)) {
					EventLog.CreateEventSource(source, log);
				}
				
				eventLog = new EventLog();
				eventLog.Log = log;
				eventLog.Source = source;
				
				useFatalWindow = false;
			} catch (Exception e) {				
				eventLog = null;
				Log(Level.Warning, String.Format("Failed to open Event Log ({0})", e.Message));
			}
		}
		
		public static void Debug(string message)
		{
			Log(Level.Debug, message);
		}

		public static void Trace(string message)
		{
			Log(Level.Trace, message);
		}

		public static void Info(string message)
		{
			Log(Level.Info, message);
		}
		
		public static void Warning(string message)
		{
			Log(Level.Warning, message);
		}

		public static void Error(string message)
		{
			Log(Level.Error, message);
		}

		public static void Fatal(string message)
		{
			Log(Level.Fatal, message);
		}
		
		public static void Log(Level level, string message)
		{
#if DEBUG
			if (level <= DebugLevel)
				System.Diagnostics.Debug.WriteLine(message);
#endif			
#if TRACE
			if (level <= TraceLevel)
				System.Diagnostics.Trace.WriteLine(message);
#endif
			
			if (level <= EventLogLevel)
				LogToEvent(level, message);
			if (level <= SyslogLogLevel)
				LogToSyslog(level, message);
			if (level <= FileLogLevel)
				LogToFile(level, message);
			if (level <= ConsoleLevel)
				LogToConsole(level, message);
			
			if (useFatalWindow && (level <= Level.Fatal)) {
				#if WINDOWS_GUI
				MessageBox.Show(message, "A fatal error has occured", MessageBoxButtons.OK, MessageBoxIcon.Stop);
				Application.Exit();
				#endif
			}
		}
		
		private static void LogToConsole(Level level, string message)
		{
			if (level <= ConsoleLevel) {
				Console.WriteLine(message);
			}			
		}
		
		/// <summary>
		/// Add event trace into SpringCard windows event log viewer
		/// </summary>
		/// <param name="level">Level of debug</param>
		/// <param name="message">message to add</param>
		/// Add Call logClear for Windows XP because crash application when log is full.
		/// Add locker for multithread application.
		private static void LogToEvent(Level level, string message)
		{
			lock (locker) {
				if (eventLog != null) {
					try {
						if (level <= EventLogLevel) {
							switch (level) {
								case Level.Fatal:
								case Level.Error:
									eventLog.WriteEntry(message, EventLogEntryType.Error);
									break;
								case Level.Warning:
									eventLog.WriteEntry(message, EventLogEntryType.Warning);
									break;
								case Level.Info:
									eventLog.WriteEntry(message, EventLogEntryType.Information);
									break;
								case Level.Trace:
									eventLog.WriteEntry("[Trace] " + message, EventLogEntryType.Information);
									break;
								case Level.Debug:
									eventLog.WriteEntry("[Debug] " + message, EventLogEntryType.Information);
									break;
								default:
									break;
							}
						}
					} catch (Exception e) {
						LogToConsole(Level.Warning, String.Format("Failed to write into Event Log ({0})", e.Message));
						ClearEventLog();
						eventLog.WriteEntry("[Clear log]", EventLogEntryType.Error);
					}

				}
			}
		}
		/// <summary>
		/// Clear the event window viewer for SpringCard input
		/// </summary>
		private static void ClearEventLog()
		{
			lock (locker) {
				try {
					if (eventLog != null) {
						
						string log = eventLog.Log;
						string source = eventLog.Source;						
						eventLog.Clear();
						eventLog = null;
						OpenEventLog(log, source);
						eventLog.Clear();
					}
				} catch {

				}

			}
		}
		
		private static void LogToFile(Level level, String message)
		{
			if (logFileName == null)
				return;
			
			string[] aMessage = message.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
			
			bool fMutexOwner = false;
			
			try {
				if (logFileMutex != null) {
					fMutexOwner = logFileMutex.WaitOne(500);
					if (!fMutexOwner)
						return;
				}
				
				foreach (string m in aMessage) {
					File.AppendAllText(logFileName,
						DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff") + "\t" +
						level + "\t" +
						m + Environment.NewLine);
				}
			} catch {
				
			} finally {
				if (fMutexOwner)
					logFileMutex.ReleaseMutex();				
			}			
		}
		
		private static string RemoveDiacritics(string text)
		{
			byte[] b = System.Text.Encoding.GetEncoding("ISO-8859-8").GetBytes(text);
			string r = System.Text.Encoding.UTF8.GetString(b);
			return r;
		}
		
		private static void LogToSyslog(Level level, String message)
		{
			if ((syslogSender == null) || (syslogSerializer == null))
				return;
		
			SysLog.Severity severity;
			
			switch (level) {
				case Level.All:
				case Level.Debug:
					severity = SysLog.Severity.Debug;
					break;
				case Level.Trace:
					severity = SysLog.Severity.Informational;
					break;					
				case Level.Info:
					severity = SysLog.Severity.Notice;
					break;
				case Level.Warning:
					severity = SysLog.Severity.Warning;
					break;
				case Level.Error:
					severity = SysLog.Severity.Error;
					break;
				case Level.Fatal:
					severity = SysLog.Severity.Critical;
					break;
				default :
					return;
			}		
			
			SysLog.Message syslogMessage = new SysLog.Message(DateTimeOffset.Now, syslogFacility, severity, syslogMachineName, syslogApplicationName, RemoveDiacritics(message));
			
			try {
				syslogSender.Send(syslogMessage, syslogSerializer);
			} catch (Exception e) {
				LogToConsole(Level.Warning, String.Format("Failed to send to remote Syslog ({0})", e.Message));
			}            
		}
	}
}
