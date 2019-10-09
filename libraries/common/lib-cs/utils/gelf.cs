/**
 * \cond
 */
using System;
using System.Text;
using System.Net.Sockets;

namespace SpringCard.LibCs
{
    /// <summary>
    /// UDP publisher to send Gelf messages to a Graylog server
    /// </summary>
    public class GelfTcpSender
    {
        private TcpClient client;

        public GelfTcpSender(string remoteHostname, int remoteHostPort)
        {
            client = new TcpClient();
            client.Connect(remoteHostname, remoteHostPort);
        }

        /// <summary>
        /// Publishes a Gelf message to the specified Graylog server.
        /// </summary>
        public void Send(string message)
        {
        	if (!client.Connected)
        		return;
        	
            byte[] bytes = StrUtils.ToBytes(message, true);
            
            NetworkStream stream = client.GetStream();
            stream.Write(bytes, 0, bytes.Length);
        }
    }
        
	internal static class Extensions
	{
		public static double ToUnixTimestamp(this DateTime d)
		{
			var duration = d.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc));
			return duration.TotalSeconds;
		}

		public static DateTime FromUnixTimestamp(this double d)
		{
			var datetime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(d);
			return datetime;
		}
	}
	
	internal static class Constants
	{
		public static readonly Encoding Encoding = Encoding.UTF8;
		public const int MaxHeaderSize = 8;
		public const int MaxChunkSize = 1024;
	}
}
/**
 * \endcond
 */
