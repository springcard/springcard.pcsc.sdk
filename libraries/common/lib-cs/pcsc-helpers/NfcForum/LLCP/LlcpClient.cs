/*
 * Crée par SharpDevelop.
 * Utilisateur: johann
 * Date: 05/01/2012
 * Heure: 12:03
 * 
 * Pour changer ce modèle utiliser Outils | Options | Codage | Editer les en-têtes standards.
 */
using System;
using System.Threading;
using System.Diagnostics;
using SpringCard.PCSC;
using SpringCard.LibCs;

namespace SpringCard.NFC
{
	public abstract class LLCP_Client : LLCP_Service
	{
		public const int MODE_DISABLED = 0;
		public const int MODE_READY = 1;
		public const int MODE_STARTING = 2;
		public const int MODE_STARTED = 3;
		public const int MODE_TERMINATED = 3;
		
		private string _name = null;
		private byte _server_port = 0;
		private string _server_name = null;
		
		public int Mode = MODE_DISABLED;

		public string Name
		{
			get
			{
				return _name;
			}
		}
		
		public string ServerName
		{
			get
			{
				return _server_name;
			}
		}
		
		public byte ServerPort
		{
			get
			{
				return _server_port;
			}
		}
		
		public LLCP_Client(string _Name, byte _ServerPort)
		{
			_name = _Name;
			_server_port = (byte) (_ServerPort & 0x3F);
		}
		
		public abstract void OnConnect(LLCP_Link link, byte[] ConnectParams);
		
		public override bool ProcessPDU(LLCP_Link link, LLCP_PDU recv_pdu)
		{
			switch (recv_pdu.PTYPE)
			{
				case LLCP_PDU.PTYPE_CC :
					Logger.Debug("> CC");

					if (link.State == LLCP_Link.StateOpening)
					{
						/* Connection accepted */
						link.State = LLCP_Link.StateActive;
						OnConnect(link, recv_pdu.Payload);
					}
					break;
					
				case LLCP_PDU.PTYPE_DM :
					Logger.Debug("> DM");

					/* Disconnect OK */
					if (link.State == LLCP_Link.StateActive)
					{
						/* Peer is disconnecting */
						link.State = LLCP_Link.StateClosed;
						OnDisconnect(link);
					} else
					{
						/* We were disconnecting - no need to invoke the callback */
						link.State = LLCP_Link.StateClosed;
					}
					/* Drop the link */
					return false;
					
				case LLCP_PDU.PTYPE_I :
					Logger.Debug("> I");

					/* The server is talking to us */
					if (link.State == LLCP_Link.StateActive)
					{
						byte nr = (byte) (recv_pdu.Sequence & 0x0F);
						byte ns = (byte) ((recv_pdu.Sequence >> 4) & 0x0F);
						
						OnInformation(link, recv_pdu.Payload);
					}
					break;
					
				case LLCP_PDU.PTYPE_RR :
					Logger.Debug("> RR");
					/* Acknowledge */
					if (link.State == LLCP_Link.StateActive)
					{
						byte nr = (byte) (recv_pdu.Sequence & 0x0F);
						
						OnAcknowledge(link);
					}
					break;

				case LLCP_PDU.PTYPE_RNR :
					Logger.Debug("> RNR");
					break;					
					
				default :
					Logger.Debug("Unsupported LLCP frame server->client");
					link.Send_FRMR(0x80, recv_pdu.PTYPE, 0x00);
					/* Drop the link */
					return false;
			}

			/* Keep the link ON */
			return true;
		}

		/*
    public void Connect(LLCP llcp, byte Address)
    {
      _llcp = llcp;
      _client_address = Address;
      
      Logger.Debug("Adding new client for server address " + _server_address + " from local address " + _client_address);
      
      llcp.ClientConnectToServer(this, null);
    }

		 */
	}
}
