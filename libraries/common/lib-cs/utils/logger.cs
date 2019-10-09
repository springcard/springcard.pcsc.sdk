/**
 *
 * \ingroup LibCs
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
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Win32;
using System.IO;

namespace SpringCard.LibCs
{
	/**
	 * \brief Utility to log execution information on the console, the trace or debug output, the system event collector (Windows or Syslog) or a remote GrayLog server
	 */
	public class Logger
	{
		/**
		 * \brief The level of details in the output or log
		 *
		 * \warning Debug level may leak sensitive information!
		 */
		public enum Level
		{
			None = -1, /*!< Completely disable the log */
			Fatal, /*!< Log Fatal messages only, i.e. unrecoverable errors or exceptions that leads to the termination of the program */
			Error, /*!< Same as Fatal + also log Error messages, for instance errors or exceptions in the program itself */
			Warning, /*!< Same as Error + also log Warning messages, for instance recoverable errors in system calls or user-related errors in the program */
			Info, /*!< Same as Warning + also log Info messages, to see how the program behaves */
			Trace, /*!< Same as Info + also log Trace messages to follow the execution flow */
			Debug, /*!< Same as Trace + also log Debug messages, with detailed information regarding the execution flow and parameters */
			All /*!< Log all messages (same as Debug) */
		};

		/**
		 * \brief Level of details directed to Visual Studio's debug window if the program (or the library) is compiled with DEBUG active, and is launched from the IDE
		 */
		public static Level DebugLevel = Level.Trace;
		/**
		 * \brief Level of details directed to Visual Studio's output window if the program (or the library) is compiled with TRACE active, and is launched from the IDE
		 */
		public static Level TraceLevel = Level.Trace;

		
		public delegate void LogCallbackDelegate(Level level, string message);
		public static LogCallbackDelegate LogCallback = null;

		private static object locker = new object();

		public class LoggerTraceListener : TraceListener
		{
			public override void Write(string s)
			{
				Log(Level.Trace, s);
			}
			public override void WriteLine(string s)
			{
				Log(Level.Trace, s);
			}
		}

		public static void CaptureTrace()
		{
			System.Diagnostics.Trace.Listeners.Add(new LoggerTraceListener());
		}

		public class LoggerDebugListener : TraceListener
		{
			public override void Write(string s)
			{
				Log(Level.Debug, s);
			}
			public override void WriteLine(string s)
			{
				Log(Level.Debug, s);
			}
		}

		public static void CaptureDebug()
		{
			System.Diagnostics.Debug.Listeners.Add(new LoggerDebugListener());
		}

		#region Config


		/**
		 * \brief Translate a numeric value into a valid Level enum value
		 */
		public static Level IntToLevel(int intLevel)
		{
			switch (intLevel)
			{
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
				default:
					if (intLevel >= 6)
						return Level.All;
					return Level.Warning;
			}
		}

		/**
		 * \brief Translate a text value into a valid Level enum value
		 */		
		public static Level StringToLevel(string strLevel)
		{
			if (strLevel == null)
				return Level.Info;

			if (strLevel.StartsWith("=") || strLevel.StartsWith(":"))
				strLevel = strLevel.Substring(1);

			int intLevel;
			if (int.TryParse(strLevel, out intLevel))
				return IntToLevel(intLevel);

			strLevel = strLevel.ToLower();
			switch (strLevel)
			{
				case "e":
				case "err":
				case "error":
					return Level.Error;
				case "w":
				case "warn":
				case "warning":
					return Level.Warning;
				case "i":
				case "info":
					return Level.Info;
				case "t":
				case "tr":
				case "trace":
					return Level.Trace;
				case "d":
				case "debug":
					return Level.Debug;
				case "a":
				case "all":
					return Level.All;
			}

			return Level.Info;
		}

		/**
		 * \brief Load the Logger settings from the application's registry branch.
		 */
		public static void LoadConfigFromRegistry(string CompanyName, string ProductName)
		{
			try
			{
				Level verboseLevel = Level.None;

				RegistryKey k = Registry.CurrentUser.OpenSubKey("SOFTWARE\\" + CompanyName + "\\" + ProductName, false);
				string s = (string)k.GetValue("VerboseLevel", "");

				int i;
				if (int.TryParse(s, out i))
				{
					verboseLevel = IntToLevel(i);
				}
				else
				{
					for (i = (int)Level.Debug; i > (int)Level.None; i--)
					{
						if (s.ToLower() == ((Level)i).ToString().ToLower())
						{
							verboseLevel = (Level)i;
							break;
						}
					}
				}

				ConsoleLevel = verboseLevel;

				s = (string)k.GetValue("VerboseFile");
				if (!string.IsNullOrEmpty(s))
				{
					logFileName = s;
					if (logFileMutex == null)
						logFileMutex = new Mutex();
				}

				s = (string)k.GetValue("VerboseFileDate", "0");
				if (int.TryParse(s, out i))
				{
					if (i != 0)
					{
						logFileNameWithDate = true;
					}
				}

			}
			catch
			{
			}
		}

		/**
		 * \brief Load the Logger settings from the program's command line
		 */		
		public static void ReadArgs(string[] args)
		{
			try
			{
				if (args != null)
				{
					for (int i = 0; i < args.Length; i++)
					{
						string s = args[i].ToLower();
						if (s.Equals("--console"))
						{
							if (ConsoleLevel < Level.Info)
								ConsoleLevel = Level.Info;
						}
						else if (s.Equals("--debug"))
						{
							if (ConsoleLevel < Level.Debug)
								ConsoleLevel = Level.Debug;
						}
						else if (s.StartsWith("--verbose") || (s.StartsWith("-v")))
						{
							if (ConsoleLevel < Level.Trace)
								ConsoleLevel = Level.Trace;
							s = s.Substring(s.Length - 1);
							if (s.StartsWith("=")) s = s.Substring(1);
							if (s.Length > 0)
							{
								int v;
								if (int.TryParse(s, out v))
									ConsoleLevel = IntToLevel(v);
							}
						}
                        else if (s.StartsWith("--logfile="))
                        {
                            s = s.Substring(10);
                            if (s.Length > 0)
                            {
                                OpenLogFile(s, true);
                            }
                        }
                        else if (s.StartsWith("--loglevel="))
                        {
                            s = s.Substring(11);
                            if (s.Length > 0)
                            {
                                int v;
                                if (int.TryParse(s, out v))
                                    FileLogLevel = IntToLevel(v);
                            }
                        }
                    }
                }
			}
			catch { }
		}

		#endregion

		#region Console		

		/**
		 * \brief Level of details directed the console (stdout)
		 */
		public static Level ConsoleLevel = Level.None;		
		/**
		 * \brief Include the time of execution in the console output
		 */		
		public static bool ConsoleShowTime = true;
		/**
		 * \brief Include the class and method (if this information is available) in the console output
		 */
		public static bool ConsoleShowContext = true;
		/**
		 * \brief Use colors to decorate the console output
		 */		
		public static bool ConsoleUseColors = true;

		/**
		 * \brief Short cut to disable all log outputs -but console- if the program is undergoing unit tests
		 */
		public static bool ConsoleOnly
		{
			set
			{
				if (value)
				{
					DebugLevel = Level.None;
					TraceLevel = Level.None;
					eventLog = null;
					EventLogLevel = Level.None;
					syslogSender = null;
					SyslogLogLevel = Level.None;
					gelfSender = null;
					GelfLogLevel = Level.None;
					logFileName = null;
					FileLogLevel = Level.None;
					LogCallback = null;
				}
			}
		}

		


		private static void LogToConsole(Level level, string context, string message)
		{
            if (level <= ConsoleLevel)
            {
                try
                {
                    if (message.Contains("\b"))
                    {
                        Console.SetCursorPosition(0, Console.CursorTop - 1);
                    }

                    if (ConsoleShowTime)
                    {
                        Console.Write("{0:D02}.{1:D02}.{2:D03} ", DateTime.UtcNow.Minute, DateTime.UtcNow.Second, DateTime.UtcNow.Millisecond);
                    }
                    if (ConsoleShowContext && (context != null))
                    {
                        Console.Write("{0} ", context);
                    }

                    if (ConsoleUseColors)
                    {
                        ConsoleColor oldBackgroundColor = Console.BackgroundColor;
                        ConsoleColor oldForeroundColor = Console.ForegroundColor;
                        switch (level)
                        {
                            case Level.Fatal:
                                Console.BackgroundColor = ConsoleColor.Red;
                                Console.ForegroundColor = ConsoleColor.White;
                                break;
                            case Level.Error:
                                Console.ForegroundColor = ConsoleColor.Red;
                                break;
                            case Level.Warning:
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                break;
                            case Level.Info:
                                Console.ForegroundColor = ConsoleColor.Cyan;
                                break;
                            case Level.Trace:
                                Console.ForegroundColor = ConsoleColor.White;
                                break;
                            case Level.Debug:
                                Console.ForegroundColor = ConsoleColor.Gray;
                                break;
                        }
                        Console.Write(message);
                        Console.ForegroundColor = oldForeroundColor;
                        Console.BackgroundColor = oldBackgroundColor;
                    }
                    else
                    {
                        Console.Write(message);
                    }

                    Console.WriteLine();
                }
                catch { }
			}
		}

		#endregion

		#region SysLog		

		public static Level SyslogLogLevel = Level.Info;
		private static ISyslogMessageSerializer syslogSerializer = null;
		private static ISyslogMessageSender syslogSender = null;
		private static SysLog.Facility syslogFacility = SysLog.Facility.LocalUse0;
		private static string syslogMachineName = null;
		private static string syslogApplicationName = null;

		private static SysLog.Severity LevelToSeverity(Level level)
		{
			SysLog.Severity result;

			switch (level)
			{
				case Level.Trace:
					result = SysLog.Severity.Informational;
					break;
				case Level.Info:
					result = SysLog.Severity.Notice;
					break;
				case Level.Warning:
					result = SysLog.Severity.Warning;
					break;
				case Level.Error:
					result = SysLog.Severity.Error;
					break;
				case Level.Fatal:
					result = SysLog.Severity.Critical;
					break;
				case Level.All:
				case Level.Debug:
				default:
					result = SysLog.Severity.Debug;
					break;
			}

			return result;
		}

		/**
		 * \brief Configure the Logger to send its messages to a SysLog server
		 */				
		public static void OpenSysLog(SysLog.Facility facility, string hostName, string applicationName, string serverAddr, int serverPort = 514, bool useRfc5424 = false)
		{
			try
			{
				syslogFacility = facility;
				if (string.IsNullOrEmpty(hostName))
					syslogMachineName = Environment.MachineName;
				else
					syslogMachineName = hostName;
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
			}
			catch (Exception e)
			{
				syslogSender = null;
				syslogSerializer = null;
				Log(Level.Warning, String.Format("Failed to create Syslog sender ({0})", e.Message));
			}
		}

		/**
		 * \brief Configure the Logger to send its messages to a SysLog server
		 */						
		public static void OpenSysLog(SysLog.Facility facility, Level level, string applicationName, string serverAddr, int serverPort = 514, bool useRfc5424 = false)
		{
			SyslogLogLevel = level;
			OpenSysLog(facility, null, applicationName, serverAddr, serverPort, useRfc5424);
		}

		/**
		 * \brief Configure the Logger to send its messages to a SysLog server
		 */		
		public static void OpenSysLog(SysLog.Facility facility, string ServerAddr, int ServerPort = 514, bool useRfc5424 = false)
		{
			OpenSysLog(facility, null, null, ServerAddr, ServerPort, useRfc5424);
		}

		/**
		 * \brief Configure the Logger to send its messages to a SysLog server
		 */		
		public static void OpenSysLog(SysLog.Facility facility, Level level, string ServerAddr, int ServerPort = 514, bool useRfc5424 = false)
		{
			SyslogLogLevel = level;
			OpenSysLog(facility, null, null, ServerAddr, ServerPort, useRfc5424);
		}

		/**
		 * \brief Configure the Logger to send messages to a SysLog server
		 */		
		public static void OpenSysLog(string ServerAddr, int ServerPort = 514, bool useRfc5424 = false)
		{
			OpenSysLog(syslogFacility, ServerAddr, ServerPort, useRfc5424);
		}

		/**
		 * \brief Configure the Logger to send its messages to a SysLog server
		 */		
		public static void OpenSysLog(Level level, string ServerAddr, int ServerPort = 514, bool useRfc5424 = false)
		{
			SyslogLogLevel = level;
			OpenSysLog(syslogFacility, ServerAddr, ServerPort, useRfc5424);
		}

		private static void SendToSyslog(Level level, string context, string message)
		{
			if ((syslogSender == null) || (syslogSerializer == null))
				return;

			SysLog.Severity severity = LevelToSeverity(level);

			if (context == null)
			{
				context = syslogApplicationName;
			}
			else
			{
				context = syslogApplicationName + ":" + context;
			}

			SysLog.Message syslogMessage = new SysLog.Message(DateTimeOffset.Now, syslogFacility, severity, syslogMachineName, RemoveDiacritics(context), RemoveDiacritics(message));

			try
			{
				syslogSender.Send(syslogMessage, syslogSerializer);
			}
			catch (Exception e)
			{
				LogToConsole(Level.Warning, "SysLog", String.Format("Failed to send to remote server ({0})", e.Message));
			}
		}

		#endregion

		#region Gelf

		public static Level GelfLogLevel = Level.Warning;
		private static Dictionary<string, string> gelfConstants;
		private static GelfTcpSender gelfSender = null;

		/**
		 * \brief Configure the Logger to send its messages to SpringCard's GrayLog server
		 *
		 * \warning Do not use this feature in your own application!
		 */
		public static void OpenGelf_SpringCardNet(Level level, string hostName, string applicationName)
		{
			OpenGelf(level, hostName, applicationName, "discover.logs.ovh.com", 2202);
			SetGelfConstant("X-OVH-TOKEN", "03154cce-3aa3-480e-96d0-893d51ffabdb");
		}

		/**
		 * \brief Configure the Logger to send its messages to SpringCard's GrayLog server
		 *
		 * \warning Do not use this feature in your own application!
		 */		
		public static void OpenGelf_SpringCardNet(Level level, string applicationName)
		{
			OpenGelf_SpringCardNet(level, null, applicationName);
		}

		/**
		 * \brief Configure the Logger to send its messages to a GrayLog server
		 */		
		public static void OpenGelf(Level level, string hostName, string applicationName, string serverName, int serverPort = 2202)
		{
			try
			{
				LogToConsole(Level.Debug, "Gelf", "Sending to server " + serverName + ":" + serverPort);
				GelfLogLevel = level;
				gelfSender = new GelfTcpSender(serverName, serverPort);
				gelfConstants = new Dictionary<string, string>();
				gelfConstants["version"] = "1.1";
				if (String.IsNullOrEmpty(hostName))
					hostName = Environment.MachineName;
				gelfConstants["host"] = hostName;
				gelfConstants["_machine"] = Environment.MachineName;
				gelfConstants["_user"] = Environment.UserName + "@" + Environment.UserDomainName;
				if (String.IsNullOrEmpty(applicationName))
					applicationName = System.AppDomain.CurrentDomain.FriendlyName;
				gelfConstants["_application"] = applicationName;
				gelfConstants["_process_id"] = String.Format("{0}", Process.GetCurrentProcess().Id);
			}
			catch (Exception e)
			{
				gelfSender = null;
				Log(Level.Warning, String.Format("Failed to create Gelf sender ({0})", e.Message));
			}
		}

		/**
		 * \brief Configure the Logger to send its messages to a GrayLog server
		 */		
		public static void OpenGelf(Level level, string applicationName, string serverAddr, int serverPort = 2202)
		{
			OpenGelf(level, null, applicationName, serverAddr, serverPort);
		}

		/**
		 * \brief Configure the Logger to send its messages to a GrayLog server
		 */
		public static void OpenGelf(Level level, string serverAddr, int serverPort = 2202)
		{
			OpenGelf(level, null, null, serverAddr, serverPort);
		}

		/**
		 * \brief Set a GrayLog constant parameter
		 */
		public static void SetGelfConstant(string Name, string Value)
		{
			gelfConstants[Name] = Value;
		}

		private static void SendToGelf(Level level, string context, string message)
		{
			if (gelfSender == null)
				return;

			SysLog.Severity severity = LevelToSeverity(level);

			JSON json = new JSON();

			foreach (KeyValuePair<string, string> constant in gelfConstants)
				json.Add(constant.Key, constant.Value);

			if (context != null)
				message = context + ":" + message;

			json.Add("level", (int)severity);
			json.Add("short_message", message);

			try
			{
				gelfSender.Send(json.AsString());
			}
			catch (Exception e)
			{
				LogToConsole(Level.Warning, "Gelf", String.Format("Failed to send to remote server ({0})", e.Message));
			}
		}

		#endregion

		#region LogFile

		public static Level FileLogLevel = Level.Info;
		private static string logFileName = null;
		private static bool logFileNameWithDate = false;
		private static Mutex logFileMutex = null;

		/**
		 * \brief Configure the Logger to send its messages to a file
		 */		
		public static void OpenLogFile(string fileName, bool useDate = false)
		{
			logFileName = fileName;
			logFileNameWithDate = useDate;

			string logFilePath = Path.GetDirectoryName(fileName);
			logFilePath = logFilePath.TrimEnd(Path.DirectorySeparatorChar);

			if (!String.IsNullOrEmpty(logFilePath))
				if (!Directory.Exists(logFilePath))
					Directory.CreateDirectory(logFilePath);
			if (logFileMutex == null)
				logFileMutex = new Mutex();
		}

		/**
		 * \brief Configure the Logger to send its messages to a file
		 */		
		public static void OpenLogFile(Level level, string fileName, bool useDate = false)
		{
			FileLogLevel = level;
			OpenLogFile(fileName, useDate);
		}

		private static string FileNameWithDate(string fileName)
		{
			DateTime now = DateTime.Now;
			string str_now = String.Format("{0:D04}{1:D02}{2:D02}", now.Year, now.Month, now.Day);

			string[] parts = fileName.Split('.');

			string result = "";
			for (int i = 0; i < parts.Length; i++)
			{
				if (i > 0)
					result += ".";
				result += parts[i];
				if (i == parts.Length - 2)
					result += "-" + str_now;
			}

			return result;
		}

		private static void LogToFile(Level level, string context, string message)
		{
			if (logFileName == null)
				return;

			string aFileName = logFileName;
			if (logFileNameWithDate)
				aFileName = FileNameWithDate(aFileName);

			string[] aMessage = message.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

			bool fMutexOwner = false;

			try
			{
				if (logFileMutex != null)
				{
					fMutexOwner = logFileMutex.WaitOne(500);
					if (!fMutexOwner)
						return;
				}

				string s = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff") + "\t";
				if (context != null)
					s = s + context + "\t";
				s = s + level.ToString() + "\t";

				foreach (string m in aMessage)
				{
					File.AppendAllText(aFileName, s + m + Environment.NewLine);
				}
			}
			catch
			{

			}
			finally
			{
				if (fMutexOwner)
					logFileMutex.ReleaseMutex();
			}
		}

		#endregion

		#region EventLog

		public static Level EventLogLevel = Level.Info;
		private static EventLog eventLog = null;

		/**
		 * \brief Configure the Logger to send its messages to Windows' event log
		 */				
		public static void OpenEventLog(string log, string source)
		{
			try
			{
				if (!EventLog.SourceExists(source))
				{
					EventLog.CreateEventSource(source, log);
				}

				eventLog = new EventLog();
				eventLog.Log = log;
				eventLog.Source = source;

			}
			catch (Exception e)
			{
				eventLog = null;
				Log(Level.Warning, String.Format("Failed to open Event Log ({0})", e.Message));
			}
		}

		/**
		 * \brief Configure the Logger to send its messages to Windows' event log
		 */		
		public static void OpenEventLog(Level level, string log, string source)
		{
			EventLogLevel = level;
			OpenEventLog(log, source);
		}

		private static void LogToEvent(Level level, string context, string message)
		{
			lock (locker)
			{
				if (eventLog != null)
				{
					try
					{
						if (level <= EventLogLevel)
						{
							string s = "";

							if (context != null)
								s = s + context + "\n";
							s = s + message;

							switch (level)
							{
								case Level.Fatal:
								case Level.Error:
									eventLog.WriteEntry(s, EventLogEntryType.Error);
									break;
								case Level.Warning:
									eventLog.WriteEntry(s, EventLogEntryType.Warning);
									break;
								case Level.Info:
									eventLog.WriteEntry(s, EventLogEntryType.Information);
									break;
								case Level.Trace:
									eventLog.WriteEntry(s + "\n(Trace)", EventLogEntryType.Information);
									break;
								case Level.Debug:
									eventLog.WriteEntry(s + "\n(Debug)", EventLogEntryType.Information);
									break;
								default:
									break;
							}
						}
					}
					catch (Exception e)
					{
						LogToConsole(Level.Warning, "EventLog", String.Format("Failed to write into Event Log ({0})", e.Message));
						ClearEventLog();
						eventLog.WriteEntry("(Clear log)", EventLogEntryType.Error);
					}

				}
			}
		}

		private static void ClearEventLog()
		{
			lock (locker)
			{
				try
				{
					if (eventLog != null)
					{

						string log = eventLog.Log;
						string source = eventLog.Source;
						eventLog.Clear();
						eventLog = null;
						OpenEventLog(log, source);
						eventLog.Clear();
					}
				}
				catch
				{

				}

			}
		}

		#endregion

		/**
		 * \brief Log a Debug-level message
		 */
		public static void Debug(string message)
		{
			Log(Level.Debug, message);
		}
		/**
		 * \brief Log a Debug-level message
		 */
		public static void Debug(string message, params object[] args)
		{
			Log(Level.Debug, message, args);
		}

		/**
		 * \brief Log a Trace-level message
		 */
		public static void Trace(string message)
		{
			Log(Level.Trace, message);
		}
		/**
		 * \brief Log a Trace-level message
		 */
		public static void Trace(string message, params object[] args)
		{
			Log(Level.Trace, message, args);
		}

		/**
		 * \brief Log an Info-level message
		 */		
		public static void Info(string message)
		{
			Log(Level.Info, message);
		}
		/**
		 * \brief Log an Info-level message
		 */		
		public static void Info(string message, params object[] args)
		{
			Log(Level.Info, message, args);
		}

		/**
		 * \brief Log a Warning-level message
		 */		
		public static void Warning(string message)
		{
			Log(Level.Warning, message);
		}
		/**
		 * \brief Log a Warning-level message
		 */
		public static void Warning(string message, params object[] args)
		{
			Log(Level.Warning, message, args);
		}

		/**
		 * \brief Log an Error-level message
		 */		
		public static void Error(string message)
		{
			Log(Level.Error, message);
		}
		/**
		 * \brief Log an Error-level message
		 */		
		public static void Error(string message, params object[] args)
		{
			Log(Level.Error, message, args);
		}

		/**
		 * \brief Log a Fatal-level message
		 */		
		public static void Fatal(string message)
		{
			Log(Level.Fatal, message);
		}
		/**
		 * \brief Log a Fatal-level message
		 */		
		public static void Fatal(string message, params object[] args)
		{
			Log(Level.Fatal, message, args);
		}

		/**
		 * \brief Log a message
		 */				
		public static void Log(Level level, string message)
		{
			Log(level, message, null);
		}
		/**
		 * \brief Log a message
		 */		
		public static void Log(Level level, string message, params object[] args)
		{
			if (level == Level.None)
				return;
			if (message == null)
				return;

			string context = SmartContext();

			if (args != null)
				message = string.Format(message, args);

			//lock (locker)
			{
				bool shownInTraceOrDebug = false;
#if DEBUG
				if (level <= DebugLevel)
				{
					if (System.Diagnostics.Debug.Listeners.Count > 0)
					{
						System.Diagnostics.Debug.WriteLine(message);
						shownInTraceOrDebug = true;
					}
				}
#endif
#if TRACE
				if (level <= TraceLevel)
				{
					if (System.Diagnostics.Trace.Listeners.Count > 0)
					{
						System.Diagnostics.Trace.WriteLine(message);
						shownInTraceOrDebug = true;
					}
				}
#endif

				if ((eventLog != null) && (level <= EventLogLevel))
					LogToEvent(level, context, message);

				if ((syslogSender != null) && (level <= SyslogLogLevel))
					SendToSyslog(level, context, message);

				if ((gelfSender != null) && (level <= GelfLogLevel))
					SendToGelf(level, context, message);

				if ((logFileName != null) && (level <= FileLogLevel))
					LogToFile(level, context, message);

				if (level <= ConsoleLevel)
					LogToConsole(level, context, message);
			}

			if (LogCallback != null)
			{
				if (context == null)
				{
					LogCallback(level, message);
				}
				else
				{
					LogCallback(level, context + ": " + message);
				}
			}
		}


		private static string SmartContext()
		{
			StackFrame[] frames = new StackTrace().GetFrames();
			string thisAssembly = frames[0].GetMethod().Module.Assembly.FullName;
			foreach (StackFrame frame in frames)
			{
				string otherAssembly = frame.GetMethod().ReflectedType.Assembly.FullName;
				if (otherAssembly != thisAssembly)
				{
					string[] t = otherAssembly.Split(',');
					return t[0];
				}
			}

			return null;
		}


		private static string RemoveDiacritics(string text)
		{
			byte[] b = System.Text.Encoding.GetEncoding("ISO-8859-8").GetBytes(text);
			string r = System.Text.Encoding.UTF8.GetString(b);
			return r;
		}


	}
}
