using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace SpringCard.LibCs
{
	public static class SysLog
	{
		public enum Facility
		{
			KernelMessages = 0,
			UserLevelMessages = 1,
			MailSystem = 2,
			SystemDaemons = 3,
			SecurityOrAuthorizationMessages1 = 4,
			InternalMessages = 5,
			LinePrinterSubsystem = 6,
			NetworkNewsSubsystem = 7,
			UUCPSubsystem = 8,
			ClockDaemon1 = 9,
			SecurityOrAuthorizationMessages2 = 10,
			FTPDaemon = 11,
			NTPSubsystem = 12,
			LogAudit = 13,
			LogAlert = 14,
			ClockDaemon2 = 15,
			LocalUse0 = 16,
			LocalUse1 = 17,
			LocalUse2 = 18,
			LocalUse3 = 19,
			LocalUse4 = 20,
			LocalUse5 = 21,
			LocalUse6 = 22,
			LocalUse7 = 23
		}
		
		public enum Severity
		{
			/// <summary>
			/// System is unusable
			/// </summary>
			Emergency = 0,
			/// <summary>
			/// Action must be taken immediately
			/// </summary>
			Alert = 1,
			/// <summary>
			/// Critical conditions
			/// </summary>
			Critical = 2,
			/// <summary>
			/// Error conditions
			/// </summary>
			Error = 3,
			/// <summary>
			/// Warning conditions
			/// </summary>
			Warning = 4,
			/// <summary>
			/// Normal but significant condition
			/// </summary>
			Notice = 5,
			/// <summary>
			/// Informational messages
			/// </summary>
			Informational = 6,
			/// <summary>
			/// Debug-level messages
			/// </summary>
			Debug = 7
		}
		
		public static Facility IntToFacility(int i)
		{
			try
			{
				return (Facility) i;
			}
			catch
			{
				return Facility.LocalUse0;
			}
		}
		
		public class StructuredDataElement
		{
			// RFC 5424 specifies that you must provide a private enterprise number. If none specified, using example number reserved for documentation (see RFC)
			public const string DefaultPrivateEnterpriseNumber = "32473";
			
			private readonly string sdId;
			private readonly Dictionary<string, string> parameters;
			
			public StructuredDataElement(string sdId, Dictionary<string, string> parameters)
			{
				this.sdId = sdId.Contains("@") ? sdId : sdId + "@" + DefaultPrivateEnterpriseNumber;
				this.parameters = parameters;
			}
			
			public string SdId
			{
				get { return sdId; }
			}
			
			public Dictionary<string, string> Parameters
			{
				get { return parameters; }
			}
		}
		
		public class Message
		{
			private readonly Facility facility;
			private readonly Severity severity;
			private readonly string hostName;
			private readonly string appName;
			private readonly string procId;
			private readonly string msgId;
			private readonly string data;
			private readonly IEnumerable<StructuredDataElement> structuredDataElements;
			private readonly DateTimeOffset? dateTimeOffset;
			
			/// <summary>
			/// Constructor for use when sending RFC 3164 messages
			/// </summary>
			public Message(
				DateTimeOffset? dateTimeOffset,
				Facility facility,
				Severity severity,
				string hostName,
				string appName,
				string data)
			{
				this.dateTimeOffset = dateTimeOffset;
				this.facility = facility;
				this.severity = severity;
				this.hostName = hostName;
				this.appName = appName;
				this.data = data;
			}

			/// <summary>
			/// Constructor for use when sending RFC 5424 messages
			/// </summary>
			public Message(
				DateTimeOffset? dateTimeOffset,
				Facility facility,
				Severity severity,
				string hostName,
				string appName,
				string procId,
				string msgId,
				string data,
				params StructuredDataElement[] structuredDataElements)
				: this(dateTimeOffset, facility, severity, hostName, appName, data)
			{
				this.procId = procId;
				this.msgId = msgId;
				this.structuredDataElements = structuredDataElements;
			}

			public int Version
			{
				get { return 1; }
			}

			public Facility Facility
			{
				get { return facility; }
			}

			public Severity Severity
			{
				get { return severity; }
			}

			public DateTimeOffset? DateTimeOffset
			{
				get { return dateTimeOffset; }
			}

			public string HostName
			{
				get { return hostName; }
			}

			public string AppName
			{
				get { return appName; }
			}

			public string ProcId
			{
				get { return procId; }
			}

			public string MsgId
			{
				get { return msgId; }
			}

			public string Data
			{
				get { return data; }
			}

			public IEnumerable<StructuredDataElement> StructuredDataElements
			{
				get { return structuredDataElements; }
			}
		}	
	}
	
	public interface ISyslogMessageSerializer
	{
		void Serialize(SysLog.Message message, Stream stream);
	}
	
	public abstract class SyslogMessageSerializerBase
	{
		protected static int CalculatePriorityValue(SysLog.Facility facility, SysLog.Severity severity)
		{
			return ((int)facility * 8) + (int)severity;
		}
	}
	
	public static class SyslogMessageSerializerExtensions
	{
		public static byte[] Serialize(this ISyslogMessageSerializer serializer, SysLog.Message message)
		{
			byte[] datagramBytes;
			using (var stream = new MemoryStream())
			{
				serializer.Serialize(message, stream);

				stream.Position = 0;

				datagramBytes = new byte[stream.Length];
				stream.Read(datagramBytes, 0, (int)stream.Length);
			}
			return datagramBytes;
		}		
	}
	
	public class SyslogRfc3164MessageSerializer : SyslogMessageSerializerBase, ISyslogMessageSerializer
	{
		private static string[] Months = new string[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
		
		public void Serialize(SysLog.Message message, Stream stream)
		{
			var priorityValue = CalculatePriorityValue(message.Facility, message.Severity);

			string timestamp = null;

			if (message.DateTimeOffset.HasValue)
			{
				DateTimeOffset dt = message.DateTimeOffset.Value;
				timestamp = String.Format("{0} {1} {2}", Months[dt.Month-1], dt.Day < 10 ? " " + dt.Day : dt.Day.ToString(), dt.ToString("HH:mm:ss"));
			}

			var headerBuilder = new StringBuilder();
			headerBuilder.Append("<").Append(priorityValue).Append(">");
			headerBuilder.Append(timestamp).Append(" ");
			headerBuilder.Append(message.HostName).Append(" ");
			headerBuilder.Append(message.AppName).Append(": ");
			headerBuilder.Append(message.Data ?? "");
			
			byte[] asciiBytes = Encoding.ASCII.GetBytes(headerBuilder.ToString());
			stream.Write(asciiBytes, 0, asciiBytes.Length);
		}
	}
	
	public class SyslogRfc5424MessageSerializer : SyslogMessageSerializerBase, ISyslogMessageSerializer
	{
		public const string NilValue = "-";
		public static readonly HashSet<char> sdNameDisallowedChars = new HashSet<char>() {' ', '=', ']', '"' };

		private readonly char[] asciiCharsBuffer = new char[255];

		public void Serialize(SysLog.Message message, Stream stream)
		{
			var priorityValue = CalculatePriorityValue(message.Facility, message.Severity);

			// Note: The .Net ISO 8601 "o" format string uses 7 decimal places for fractional second. Syslog spec only allows 6, hence the custom format string
			var timestamp = message.DateTimeOffset.HasValue
				? message.DateTimeOffset.Value.ToString("yyyy-MM-ddTHH:mm:ss.ffffffK")
				: null;

			var messageBuilder = new StringBuilder();
			messageBuilder.Append("<").Append(priorityValue).Append(">");
			messageBuilder.Append(message.Version);
			messageBuilder.Append(" ").Append(timestamp.FormatSyslogField(NilValue));
			messageBuilder.Append(" ").Append(message.HostName.FormatSyslogAsciiField(NilValue, 255, asciiCharsBuffer));
			messageBuilder.Append(" ").Append(message.AppName.FormatSyslogAsciiField(NilValue, 48, asciiCharsBuffer));
			messageBuilder.Append(" ").Append(message.ProcId.FormatSyslogAsciiField(NilValue, 128, asciiCharsBuffer));
			messageBuilder.Append(" ").Append(message.MsgId.FormatSyslogAsciiField(NilValue, 32, asciiCharsBuffer));
			
			writeStream(stream, Encoding.ASCII, messageBuilder.ToString());

			// Structured data
			foreach(SysLog.StructuredDataElement sdElement in message.StructuredDataElements)
			{
				messageBuilder.Clear()
					.Append(" ")
					.Append("[")
					.Append(sdElement.SdId.FormatSyslogSdnameField(asciiCharsBuffer));

				writeStream(stream, Encoding.ASCII, messageBuilder.ToString());

				foreach(System.Collections.Generic.KeyValuePair<string, string> sdParam in sdElement.Parameters)
				{
					messageBuilder.Clear()
						.Append(" ")
						.Append(sdParam.Key.FormatSyslogSdnameField(asciiCharsBuffer))
						.Append("=")
						.Append("\"")
						.Append(
							sdParam.Value != null ?
							sdParam.Value
							.Replace("\\", "\\\\")
							.Replace("\"", "\\\"")
							.Replace("]", "\\]")
							:
							String.Empty
						)
						.Append("\"");

					writeStream(stream, Encoding.UTF8, messageBuilder.ToString());
				}

				// ]
				stream.WriteByte(93);
			}

			if (!String.IsNullOrWhiteSpace(message.Data))
			{
				// Space
				stream.WriteByte(32);

				stream.Write(Encoding.UTF8.GetPreamble(), 0, Encoding.UTF8.GetPreamble().Length);
				writeStream(stream, Encoding.UTF8, message.Data);
			}
		}

		private void writeStream(Stream stream, Encoding encoding, String data)
		{
			byte[] streamBytes = encoding.GetBytes(data);
			stream.Write(streamBytes, 0, streamBytes.Length);
		}
	}

	internal static class StringExtensions
	{
		public static string IfNotNullOrWhitespace(this string s, Func<string, string> action)
		{
			return String.IsNullOrWhiteSpace(s) ? s : action(s);
		}

		public static string FormatSyslogField(this string s, string replacementValue, int? maxLength = null)
		{
			return String.IsNullOrWhiteSpace(s)
				? replacementValue
				: maxLength.HasValue ? EnsureMaxLength(s, maxLength.Value) : s;
		}

		public static string EnsureMaxLength(this string s, int maxLength)
		{
			return String.IsNullOrWhiteSpace(s)
				? s
				: s.Length > maxLength ? s.Substring(0, maxLength) : s;
		}

		public static string FormatSyslogAsciiField(this string s, string replacementValue, int maxLength, char[] charBuffer, Boolean sdName = false)
		{
			s = FormatSyslogField(s, replacementValue, maxLength);

			int bufferIndex = 0;
			for (int i = 0; i < s.Length; i++)
			{
				char c = s[i];
				if (c >= 33 && c <= 126)
				{
					if (!sdName || !SyslogRfc5424MessageSerializer.sdNameDisallowedChars.Contains(c))
					{
						charBuffer[bufferIndex++] = c;
					}
				}
			}

			return new string(charBuffer, 0, bufferIndex);
		}

		public static string FormatSyslogSdnameField(this string s, char[] charBuffer)
		{
			return FormatSyslogAsciiField(s, SyslogRfc5424MessageSerializer.NilValue, 32, charBuffer, true);
		}
	}
	
	public interface ISyslogMessageSender : IDisposable
	{
		void Reconnect();
		void Send(SysLog.Message message, ISyslogMessageSerializer serializer);
		void Send(IEnumerable<SysLog.Message> messages, ISyslogMessageSerializer serializer);
	}

	public class SyslogUdpSender : ISyslogMessageSender, IDisposable
	{
		private readonly UdpClient udpClient;

		public SyslogUdpSender(string hostname, int port)
		{
			udpClient = new UdpClient(hostname, port);
		}

		public void Send(SysLog.Message message, ISyslogMessageSerializer serializer)
		{
			byte[] datagramBytes = serializer.Serialize(message);
			udpClient.Send(datagramBytes, datagramBytes.Length);
		}

		public void Send(IEnumerable<SysLog.Message> messages, ISyslogMessageSerializer serializer)
		{
			foreach(SysLog.Message message in messages)
			{
				Send(message, serializer);
			}
		}

		public void Reconnect() { /* UDP is connectionless */ }

		public void Dispose()
		{
			udpClient.Close();
		}
	}
}
