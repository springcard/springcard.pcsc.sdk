/**
 * \cond
 */
using System.Net;
using System.Net.NetworkInformation;
using System.Text;

namespace SpringCard.LibCs
{
	public partial class Network
	{
		public static bool Ping(string host)
		{
			IPAddress address;

			try
			{
				address = IPAddress.Parse(host);
			}
			catch
			{
				try
				{
					IPHostEntry hostInfo = Dns.GetHostEntry(host);
					address = hostInfo.AddressList[0];
				}
				catch
				{
					return false;
				}
			}

			try
			{
				PingOptions options = new PingOptions();
				options.DontFragment = true;

				Ping pingSender = new Ping();
				byte[] buffer = Encoding.ASCII.GetBytes("0123456789ABCDEF0123456789ABCDEF");
				PingReply reply = pingSender.Send(host, 120, buffer, options);

				bool success = (reply.Status == IPStatus.Success);
				pingSender.Dispose();

				return success;
			}
			catch
			{
				return false;
			}
		}
	}
}
/**
 * \endcond
 */
