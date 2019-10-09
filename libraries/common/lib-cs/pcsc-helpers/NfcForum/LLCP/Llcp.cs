/*
 * Crée par SharpDevelop.
 * Utilisateur: johann
 * Date: 05/01/2012
 * Heure: 12:03
 * 
 * Pour changer ce modèle utiliser Outils | Options | Codage | Editer les en-têtes standards.
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;
using SpringCard.PCSC;
using SpringCard.LibCs;

namespace SpringCard.NFC
{
	public abstract class LLCP
	{
		public const byte ADDR_NAME_SERVER = 0x01;
		
		public const byte PARAM_VERSION = 0x01;
		public const byte PARAM_MAX_INF_UNIT_EXT = 0x02;
		public const byte PARAM_WKS = 0x03;
		public const byte PARAM_LTO = 0x04;
		public const byte PARAM_RECV_WINDOW_SIZE = 0x05;
		public const byte PARAM_SERVICE_NAME = 0x06;
		public const byte PARAMETER_OPT = 0x07;
		public const byte PARAMETER_SDREQ = 0x08;
		public const byte PARAMETER_SDRES = 0x09;
		
		private uint SymmCount = 0;
		
		public uint StartClientsAfterSymms = 1;
		
		protected ICardApduTransmitter _channel;
		
		private List<LLCP_PDU> _recv_queue = new List<LLCP_PDU>();
		private List<LLCP_PDU> _send_queue = new List<LLCP_PDU>();
		
		private List<LLCP_Link> _links = new List<LLCP_Link>();
		
		private List<LLCP_Server> _servers = new List<LLCP_Server>();
		private List<LLCP_Client> _clients = new List<LLCP_Client>();

        public delegate void LinkOpenedCallback();
        public LinkOpenedCallback OnLinkOpened = null;
        public delegate void LinkClosedCallback();
        public LinkClosedCallback OnLinkClosed = null;

        public LLCP(ICardApduTransmitter Channel)
		{
			_channel = Channel;
		}
		
		private bool HandleManagementPDU(LLCP_PDU recv_pdu)
		{
			switch (recv_pdu.PTYPE)
			{
				case LLCP_PDU.PTYPE_SYMM :
					/* This is a SYMM PDU - do nothing */
					Logger.Debug("> SYMM");
					SymmCount++;
					
					/* Check whether we have clients waiting to come to life */
					if (SymmCount > StartClientsAfterSymms)
					{
						for (int i=0; i<_clients.Count; i++)
						{
							if (_clients[i].Mode == LLCP_Client.MODE_READY)
							{
								Logger.Debug("Launching client '" + _clients[i].Name + "'");
								
								_clients[i].Mode = LLCP_Client.MODE_STARTING;
								
								LLCP_Link link = new LLCP_Link(this, _clients[i], (byte) (0x20 + i));
								link.Send_Connect();
								_links.Add(link);
								break;
							}
						}
					}

					/* Nothing done */
					break;
					
				case LLCP_PDU.PTYPE_DISC :
					/* Link deactivation */
					Logger.Debug("LLCP deactivation");
					return false;
					
					default :
						Logger.Debug("BAD PTYPE with DSAP=SSAP=0\n");
					break;
			}
			
			return true;
		}
		
		protected bool HandleRecvPDU(LLCP_PDU recv_pdu)
		{
			LLCP_Link link;
			
			if ((recv_pdu.DSAP == 0) && (recv_pdu.SSAP == 0))
			{
				/* Management PDUs */
				/* --------------- */
				
				return HandleManagementPDU(recv_pdu);
			}
			
			SymmCount = 0;
			
			if (recv_pdu.PTYPE == LLCP_PDU.PTYPE_CONNECT)
			{
				/* Handle CONNECT */
				/* -------------- */
				
				Logger.Debug("> CONNECT");
				
				/* Name server is listening on address 01 */
				if (recv_pdu.DSAP == ADDR_NAME_SERVER)
				{
					/* Using name server */
					string server_name = FindParameterStr(recv_pdu.Payload, LLCP.PARAM_SERVICE_NAME);
					
					if (server_name != null)
					{
						Logger.Debug("Looking for server '" + server_name + "'");
						
						/* Looking for a server by its name */
						for (int i=0; i<_servers.Count; i++)
						{
							if (_servers[i].ServerName == server_name)
							{
								link = new LLCP_Link(this, _servers[i], recv_pdu);
								link.LocalPort = _servers[i].ServerPort;
								
								if (link.ProcessPDU(recv_pdu))
									_links.Add(link);
								
								return true;
							}
						}
					}
					
				} else
				{
					/* Using server address */
					
					Logger.Debug("Looking for server " + recv_pdu.DSAP);
					
					for (int i=0; i<_servers.Count; i++)
					{
						if (_servers[i].ServerPort == recv_pdu.DSAP)
						{
							link = new LLCP_Link(this, _servers[i], recv_pdu);
							
							if (link.ProcessPDU(recv_pdu))
								_links.Add(link);
							
							return true;
						}
					}
				}
			}
			
			/* Looking for an already-connected Server, or already instancied Client */
			/* --------------------------------------------------------------------- */
			
			for (int i=0; i<_links.Count; i++)
			{
				if (_links[i].LocalPort == recv_pdu.DSAP)
				{
					if (!_links[i].ProcessPDU(recv_pdu))
					{
						/* Drop the link */
						_links.RemoveAt(i);
					}
					
					return true;
				}
			}
			
			/* No client, no server, and DSAP or SSAP != 0 */
			link = new LLCP_Link(this, null, recv_pdu);
			
			switch (recv_pdu.PTYPE)
			{
				case LLCP_PDU.PTYPE_CONNECT :
					/* Peer is trying to connect to a non-existing service */
					Logger.Debug("CONNECT to non existing server");
					link.Send_DM(LLCP_DM_PDU.REASON_NO_SERVICE);
					break;
					
				case LLCP_PDU.PTYPE_RR :
				case LLCP_PDU.PTYPE_I :
					Logger.Debug("I or RR to non active service");
					link.Send_DM(LLCP_DM_PDU.REASON_NO_CONNECTION); /* 01 */
					break;
					
				default :
					Logger.Debug("No client, no server, bad PTYPE");
					break;
			}
			
			return true;
		}
		
		
		public void RegisterServer(LLCP_Server server)
		{
			_servers.Add(server);
		}
		
		public void RegisterClient(LLCP_Client client)
		{
			_clients.Add(client);
		}
		
		private void StartClients()
		{
			Logger.Debug("Starting clients");
			
			if (_clients != null)
			{
				for (int i=0; i<_clients.Count; i++)
				{
					Logger.Debug("Client '" + _clients[i].Name + "' is ready");
					_clients[i].Mode = LLCP_Client.MODE_READY;
				}
			}
		}

		protected void OnLinkEstablished()
		{
			Logger.Debug("Link established");
			
//			if (!HasServers)
			StartClients();
		}
		
//		public void ClientConnectToServer(LlcpClient client, LLCP_PARAMETER[] Parameters)
//		{
		/* The client wants to connect to the server -> Push a connect PDU */
//			EnqueueSendPdu(new LLCP_CONNECT_PDU(client.ServerAddress(), client.LocalAddress(), Parameters));
//		}
		
		public void SendPDU_PUSH(LLCP_PDU send_pdu)
		{
			_send_queue.Add(send_pdu);
		}
		
		protected LLCP_PDU SendPDU_POP()
		{
			LLCP_PDU r = null;
			
			if (_send_queue.Count > 0)
			{
				r = _send_queue[0];
				_send_queue.RemoveAt(0);
			}
			
			return r;
		}
		
		public static byte[] FindParameter(byte[] buffer, byte ParamId)
		{
			if (buffer == null)
				return null;
			
			int offset = 0;
			while (offset < buffer.Length)
			{
				byte t = buffer[offset++];
				if (offset >= buffer.Length) break;
				
				byte l = buffer[offset++];
				if (offset >= buffer.Length) break;
				
				if (offset + l > buffer.Length) break;

				if (t == ParamId)
				{
					byte[] r = new byte[l];
					for (int i=0; i<l; i++)
						r[i] = buffer[offset++];

					return r;
				}
				
				offset += l;
			}
			
			return null;
		}
		
		public static string FindParameterStr(byte[] buffer, byte ParamId)
		{
			byte[] b = FindParameter(buffer, ParamId);
			
			if (b == null)
				return null;
			
			string r = "";
			
			for (int i=0; i<b.Length; i++)
				r += (char) b[i];

			return r;
		}

		public abstract bool Start();
		public abstract void Stop();
		public abstract void StopAndResetPeer();
		public abstract void StopAndSuspend();
		protected abstract LLCP_PDU Exchange(LLCP_PDU myPdu);
	}
}
