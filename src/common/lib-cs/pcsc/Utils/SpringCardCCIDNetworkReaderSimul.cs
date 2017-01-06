/*
 * Created by SharpDevelop.
 * User: johann
 * Date: 12/05/2015
 * Time: 17:16
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using SpringCard.LibCs;

namespace SpringCard.PCSC
{
	/// <summary>
	/// Description of scCcidNetworkRegistry.
	/// </summary>
	public class CcidNetworkReaderSimul
	{
		TcpListener server;
		ushort port;
		
		const string sDeviceDescriptor = "1201000200000040341CC191030201020301";
		const string sConfigurationDescriptor = "09025D0001010480C809040000030B000000362110010003030000000A0F00000A0F000000002A0000900D030000FE00000000000000000000007E0402000F010000FFFF00000001070581024000000705020240000007058303080001";
		const string sVendorName = "160353007000720069006E0067004300610072006400";
		const string sProductName = "0A034500360036003300";
		const string sSerialNumber = "1A03300030003500300043003200300042004100300030003200";
		
		public CcidNetworkReaderSimul(ushort port)
		{
			this.port = port;
		}
		
		public void Start()
		{
			server = new TcpListener(IPAddress.Any, port);			
			server.Start();
			WaitForClient();			
		}
		
		void WaitForClient()
		{
			Trace.WriteLine("Server: Waiting for clients");
			server.BeginAcceptTcpClient(OnClientConnect, server);
		}
		
		void OnClientConnect(IAsyncResult ar)
		{
			try
			{
				Trace.WriteLine("Server: Client connected");
				TcpListener l_server = (TcpListener) ar.AsyncState;
				TcpClient client = l_server.EndAcceptTcpClient(ar);
				ClientHandler handler = new ClientHandler(client);
				handler.Start();
			}
			catch (Exception)
			{
				throw;
			}
			WaitForClient();
		}
		
		class ClientHandler
		{
			TcpClient client;
			NetworkStream stream = null;
			
			public ClientHandler(TcpClient client)
			{
				Trace.WriteLine("Client: accepted");
				this.client = client;
			}
			
			public void Start()
			{
				stream = client.GetStream();
				WaitForRequest();
			}
			
			void WaitForRequest()
			{
				Trace.WriteLine("Client: Waiting for request");
				byte[] buffer = new byte[client.ReceiveBufferSize];
				stream.BeginRead(buffer, 0, buffer.Length, OnRequest, buffer);
			}
			
			void OnRequest(IAsyncResult ar)
			{
				try
				{

					int length = stream.EndRead(ar);
					
					if (length == 0)
					{
						Trace.WriteLine("Client: Connexion close");
						stream.Close();
						client.Close();
						return;
					}
					
					Trace.WriteLine("Client: Got a request");					
					
					byte[] frame = RawData.CopyBuffer(ar.AsyncState as byte[], 0, length);
					Trace.WriteLine(">" + BinConvert.ToHex(frame));
					
					Recv(frame[0], RawData.CopyBuffer(frame, 1, 0));
					
				}
				catch (Exception)
				{
					throw;
				}
				WaitForRequest();
			}
			
			public bool Send(byte endpoint, byte[] buffer)
			{
				byte[] frame = new byte[1 + buffer.Length];
				
				frame[0] = endpoint;
				
				Array.Copy(buffer, 0, frame, 1, buffer.Length);
				
				Trace.WriteLine("Client: Sending");
				Trace.WriteLine("<" + BinConvert.ToHex(frame));
				
				stream.Write(frame, 0, frame.Length);
				stream.Flush();
				
				return true;
			}
			
			protected void Recv(byte endpoint, byte[] buffer)
			{
				switch (endpoint) {
					case CCID.EP_Control_To_RDR:
						recvControl(buffer);
						break;
						
					case CCID.EP_Bulk_PC_To_RDR:
						recvBulk(buffer);
						break;
						
					default :
						break;
				}
			}
			
			protected bool sendControl(byte bRequest, ushort wValue = 0, ushort wIndex = 0, byte bStatus = 0, byte[] abData = null)
			{				
				int length = 0;
				
				if (abData != null)
					length = abData.Length;
				
				byte[] buffer = new byte[10 + length];
				
				buffer[0] = bRequest;
				buffer[1] = (byte) (length % 256); length /= 256;
				buffer[2] = (byte) (length % 256); length /= 256;
				buffer[3] = (byte) (length % 256); length /= 256;
				buffer[4] = (byte) (length % 256);
				buffer[5] = (byte) (wValue % 256);
				buffer[6] = (byte) (wValue / 256);
				buffer[7] = (byte) (wIndex % 256);
				buffer[8] = (byte) (wIndex / 256);
				buffer[9] = bStatus;
				
				if (abData != null)
					Array.Copy(abData, 0, buffer, 10, abData.Length);
				
				return Send(CCID.EP_Control_To_PC, buffer);
			}

			protected bool sendControlString(byte bRequest, ushort wValue, ushort wIndex, byte bStatus, string abData)
			{				
				return sendControl(bRequest, wValue, wIndex, bStatus, BinConvert.HexToBytes(abData));
			}
			
			void recvControl(byte[] buffer)
			{
				byte bRequest = buffer[0];
				ushort wValue = (ushort)(buffer[5] + 256 * buffer[6]);
				ushort wIndex = (ushort)(buffer[7] + 256 * buffer[8]);
				byte bStatus = buffer[9];
				byte[] abData = RawData.CopyBuffer(buffer, 10);
	
				recvControl(bRequest, wValue, wIndex, bStatus, abData);
			}

			void recvControl(byte bRequest, ushort wValue = 0, ushort wIndex = 0, byte bStatus = 0, byte[] abData = null)
			{
				switch (bRequest) {
					case CCID.GET_DESCRIPTOR:
						switch (wValue) {
							case 0x0001:
								/* Send Device Descriptor */
								sendControlString(bRequest, wValue, wIndex, bStatus, sDeviceDescriptor);
								break;
							case 0x0002:
								/* Send Configuration Descriptor */
								sendControlString(bRequest, wValue, wIndex, bStatus, sConfigurationDescriptor);
								break;
							case 0x0103:
								/* Send Vendor Name */
								sendControlString(bRequest, wValue, wIndex, bStatus, sVendorName);
								break;
							case 0x0203:
								/* Send Product Name */
								sendControlString(bRequest, wValue, wIndex, bStatus, sProductName);
								break;
							case 0x0303:
								/* Send Serial Number */
								sendControlString(bRequest, wValue, wIndex, bStatus, sSerialNumber);
								break;
							default :
								/* Not supported */
								sendControl(bRequest, wValue, wIndex, bStatus, null);
								break;
						}
						break;
						
					case CCID.SET_CONFIGURATION:
						/* Set configuration */
						if (wValue == 0)
						{
							/* Stop the coupler */
							sendControl(bRequest, 0, 0, 0, null);
						}
						else
						{
							/* Start the coupler */
							sendControl(bRequest, 0, 0, 1, null);
						}
						break;
						
					case CCID.GET_STATE:
						/* Get state */
						sendControl(CCID.GET_STATE, 0, 0, 0, null);
						break;
						
					default :
						/* Unsupported message */
						sendControl(CCID.GET_STATE, 0, 0, 0xFF, null);
						break;
				}
			}

			void recvBulk(byte[] buffer)
			{
				CCID.PC_to_RDR_Block block = new CCID.PC_to_RDR_Block(buffer);				
				
				// TODO : process the block
			}
			
		}
	}
}
