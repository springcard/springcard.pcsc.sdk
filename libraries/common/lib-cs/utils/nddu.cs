/**
 * \cond
 */
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

namespace SpringCard.LibCs
{
	public class Nddu
	{
		public const string MessageMarker = "com.springcard.nddu.1";
		public const string MessageLookup = "lookup";
		public const string MessageDevice = "device";
		public const string MessageMessage = "message";
		public const string MessageSetAddress = "set_address";
		public const int DefaultUdpPort = 3999;

		public class Device
		{
			public string MacAddress = null;
			public string Type = null;
			public string Info = null;
			public bool SupportsDhcp = false;
			public bool DhcpEnable = false;
			public string Ipv4Address = null;
			public string Ipv4Mask = null;
			public string Ipv4Gateway = null;
			public string ConfigGotoPage = null;
			public bool ConfigLocked = false;

			public string SetStaticNetworkConfigMessage(string currentPassword, string newPassword = null)
			{
				DhcpEnable = false;
				string s = MessageMarker + ":" + MessageSetAddress + ":";
				s += "mac=" + MacAddress + ";";
				if (Ipv4Address != null)
					s += "addr=" + Ipv4Address + ";";
				if (Ipv4Mask != null)
					s += "mask=" + Ipv4Mask + ";";
				if (Ipv4Gateway != null)
					s += "gway=" + Ipv4Gateway + ";";
				if (Info != null)
					s += "info=" + Info + ";";
				if (currentPassword != null)
					s += "pass=" + currentPassword + ";";
				if (newPassword != null)
					s += "set_pass=" + newPassword + ";";
				return s;
			}

			public string SetDhcpNetworkConfigMessage(string currentPassword, string newPassword = null)
			{
				DhcpEnable = true;
				string s = MessageMarker + ":" + MessageSetAddress + ":";
				s += "mac=" + MacAddress + ";";
				s += "dhcp=1;";
				if (Info != null)
					s += "info=" + Info + ";";
				if (currentPassword != null)
					s += "pass=" + currentPassword + ";";
				if (newPassword != null)
					s += "set_pass=" + newPassword + ";";
				return s;
			}

			public string SetNetworkConfigMessage(string currentPassword, string newPassword = null)
			{
				if (SupportsDhcp && DhcpEnable)
				{
					return SetDhcpNetworkConfigMessage(currentPassword, newPassword);
				}
				else
				{
					return SetStaticNetworkConfigMessage(currentPassword, newPassword);
				}
			}

			public string AnnounceMessage()
			{
				string s;

				s = MessageMarker + ":" + MessageDevice + ":";
				s += "mac=" + MacAddress + ";";
				s += "type=" + Type + ";";

				if (ConfigLocked)
					s += "lock=1;";

				if (ConfigGotoPage != null)
					s += "goto=" + ConfigGotoPage + ";";

				if (SupportsDhcp)
				{
					s += "dhcp=";
					if (DhcpEnable)
						s += "1";
					else
						s += "0";
					s += ";";
				}

				if (Ipv4Address != null)
					s += "addr=" + Ipv4Address + ";";
				if (Ipv4Mask != null)
					s += "mask=" + Ipv4Mask + ";";
				if (Ipv4Gateway != null)
					s += "gway=" + Ipv4Gateway + ";";

				if (Info != null)
					s += "info=" + Info + ";";

				return s;
			}
		}

		private class NdduUdpSender : UdpClient
		{
			public NdduUdpSender() : base()
			{
				//Calls the protected Client property belonging to the UdpClient base class.
				Socket s = this.Client;
				//Uses the Socket returned by Client to set an option that is not available using UdpClient.
				s.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
				s.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontRoute, 1);
			}

			public NdduUdpSender(IPEndPoint ipLocalEndPoint) : base(ipLocalEndPoint)
			{
				//Calls the protected Client property belonging to the UdpClient base class.
				Socket s = this.Client;
				//Uses the Socket returned by Client to set an option that is not available using UdpClient.
				s.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
				s.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontRoute, 1);
			}
		}

		public abstract class Listener
		{
			private int udpPort;
			private UdpClient udpListener;
			private volatile bool running = false;

			protected Listener(int udpPort)
			{
				this.udpPort = udpPort;
				udpListener = new UdpClient(this.udpPort);
			}

			~Listener()
			{
				Close();
			}

			public void Close()
			{
				if (udpListener != null)
				{
					udpListener.Close();
					udpListener = null;
				}
			}

			protected void Send(IPEndPoint ip, string message)
			{
				byte[] bytes = Encoding.ASCII.GetBytes(message);

				udpListener.Send(bytes, bytes.Length, ip);
			}

			protected void Send(IPAddress address, string message)
			{
				byte[] bytes = Encoding.ASCII.GetBytes(message);
				IPEndPoint ipTarget = new IPEndPoint(IPAddress.Broadcast, udpPort);

				try
				{
					IPEndPoint ipLocal = new IPEndPoint(address, udpPort);
					NdduUdpSender udpSender = new NdduUdpSender(ipLocal);
					udpSender.Send(bytes, bytes.Length, ipTarget);
					udpSender.Close();
				}
				catch
				{
					udpListener.Send(bytes, bytes.Length, ipTarget);
				}
			}

			public void Send(string message)
			{
				NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
				if (interfaces != null)
				{
					for (int i = 0; i < interfaces.Length; i++)
					{
						if (interfaces[i].OperationalStatus == OperationalStatus.Up)
						{
							IPInterfaceProperties properties = interfaces[i].GetIPProperties();
							for (int j = 0; j < properties.UnicastAddresses.Count; j++)
							{
								IPAddress address = properties.UnicastAddresses[j].Address;
								if ((!address.Equals(IPAddress.Loopback)) && (address.AddressFamily == AddressFamily.InterNetwork))
								{
									Send(address, message);
								}
							}
						}
					}
				}
			}


			protected virtual void HandleLookup(IPEndPoint ip, string message)
			{
				Logger.Debug("LOOKUP message from " + ip.Address + " : " + message);
			}

			protected virtual void HandleDevice(IPEndPoint ip, string message)
			{
				Logger.Debug("DEVICE message from " + ip.Address + " : " + message);
			}

			protected virtual void HandleMessage(IPEndPoint ip, string message)
			{
				Logger.Debug("MESSAGE message from " + ip.Address + " : " + message);
			}

			protected void HandleListenerData(IPEndPoint ip, byte[] data)
			{
                string message = StrUtils.ToStr_ASCII(data);

				string[] tokens = message.Split(new char[] { ':' }, 3);

				if (tokens.Length >= 3)
				{
					if (tokens[0] == MessageMarker)
					{
						switch (tokens[1])
						{
							case MessageLookup:
								HandleLookup(ip, tokens[2]);
								break;
							case MessageDevice:
								HandleDevice(ip, tokens[2]);
								break;
							case MessageMessage:
								HandleMessage(ip, tokens[2]);
								break;
							default:
								Logger.Debug("Unsupported message from " + ip.Address + " : token=" + tokens[1]);
								break;
						}
					}
					else
					{
						Logger.Debug("Unsupported message from " + ip.Address + " : marker=" + tokens[0]);
					}
				}
				else
				{
					Logger.Debug("Unsupported message from " + ip.Address + " : not NDDU");
				}
			}

			private void onListenerDataArrived(IAsyncResult ar)
			{
				if (running)
				{
					IPEndPoint ip = new IPEndPoint(IPAddress.Any, udpPort);
					byte[] data = udpListener.EndReceive(ar, ref ip);

					HandleListenerData(ip, data);

					udpListener.BeginReceive(onListenerDataArrived, null);
				}
			}

			public void Start()
			{
				running = true;
				udpListener.BeginReceive(onListenerDataArrived, null);
			}

			public void Stop()
			{
				running = false;
				udpListener.Close();
			}
		}

		public class Manager : Listener
		{
			private Dictionary<string, Device> FoundDevices = new Dictionary<string, Device>();

			public Device GetDevice(string MacAddress)
			{
				return FoundDevices[MacAddress];
			}

			public delegate void OnDeviceDataCallback(Device device);
			public OnDeviceDataCallback OnDeviceData = null;

			public delegate void OnDeviceMessageCallback(Device device, int code, string text);
			public OnDeviceMessageCallback OnDeviceMessage = null;

			public delegate void OnLookupCallback(string info);
			public OnLookupCallback OnLookup = null;

			public Manager() : base(DefaultUdpPort)
			{

			}

			public Manager(int udpPort) : base(udpPort)
			{

			}

			private string LookupString
			{
				get
				{
					return MessageMarker + ":" + MessageLookup + ":info=SpringCard NDDU running on " + System.Environment.MachineName;
				}
			}

			protected override void HandleDevice(IPEndPoint ip, string message)
			{
				string[] tokens = message.Split(new char[] { ';' });
				Device device = new Device();

				for (int i = 0; i < tokens.Length; i++)
				{
					string[] sub_tokens = tokens[i].Split(new char[] { '=' }, 2);

					if (sub_tokens.Length >= 2)
					{
						Console.WriteLine("\t" + sub_tokens[0] + "=" + sub_tokens[1]);

						switch (sub_tokens[0])
						{
							case "mac":
								device.MacAddress = sub_tokens[1];
								break;
							case "type":
								device.Type = sub_tokens[1];
								break;
							case "lock":
								if (sub_tokens[1] == "0")
									device.ConfigLocked = false;
								else
									device.ConfigLocked = true;
								break;
							case "dhcp":
								device.SupportsDhcp = true;
								if (sub_tokens[1] == "0")
									device.DhcpEnable = false;
								else
									device.DhcpEnable = true;
								break;
							case "goto":
								device.ConfigGotoPage = sub_tokens[1];
								break;
							case "addr":
								device.Ipv4Address = sub_tokens[1];
								break;
							case "mask":
								device.Ipv4Mask = sub_tokens[1];
								break;
							case "gway":
								device.Ipv4Gateway = sub_tokens[1];
								break;
							case "info":
								device.Info = sub_tokens[1];
								break;
							default:
								break;

						}
					}
				}

				FoundDevices[device.MacAddress] = device;

				if (OnDeviceData != null)
					OnDeviceData(device);
			}

			protected override void HandleMessage(IPEndPoint ip, string message)
			{
				string[] tokens = message.Split(new char[] { ';' });

				string macAddress = null;
				string s_code = null;
				int i_code = -1;
				string text = null;

				for (int i = 0; i < tokens.Length; i++)
				{
					string[] sub_tokens = tokens[i].Split(new char[] { '=' }, 2);

					if (tokens.Length >= 2)
					{
						switch (sub_tokens[0])
						{
							case "mac":
								macAddress = sub_tokens[1];
								break;
							case "code":
								s_code = sub_tokens[1];
								int.TryParse(s_code, out i_code);
								break;
							case "text":
								text = sub_tokens[1];
								break;
							default:
								break;
						}
					}
				}

				Device device = null;

				if (macAddress != null)
				{
					try
					{
						device = FoundDevices[macAddress];
					}
					catch
					{
						macAddress = null;
					}
				}

				if (OnDeviceMessage != null)
					OnDeviceMessage(device, i_code, text);
			}

			protected override void HandleLookup(IPEndPoint ip, string message)
			{
				if (OnLookup != null)
					OnLookup(message);
			}

			public void Lookup()
			{
				Send(LookupString);
			}
		}

		public class Client : Listener
		{
			public Device device;

			public Client(Device device, int udpPort = DefaultUdpPort) : base(udpPort)
			{
				this.device = device;
			}

			public Client(string MacAddress, string Type, int udpPort = DefaultUdpPort) : base(udpPort)
			{
				this.device = new Device();
				this.device.MacAddress = MacAddress;
				this.device.Type = Type;
			}

			public Client(string MacAddress, string Type, string Info, int udpPort = DefaultUdpPort) : base(udpPort)
			{
				this.device = new Device();
				this.device.MacAddress = MacAddress;
				this.device.Type = Type;
				this.device.Info = Type;
			}

			private void SendAnnounce(IPEndPoint ip)
			{
				string message = device.AnnounceMessage();
				Logger.Debug("ANNOUNCE to " + ip.Address + " : " + message);
				Send(ip, message);
			}

			protected override void HandleLookup(IPEndPoint ip, string message)
			{
				Logger.Debug("LOOKUP to " + ip.Address + " : " + message);
				SendAnnounce(ip);
			}
		}
	}
}
/**
 * \endcond
 */
