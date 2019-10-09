/*
 * Crée par SharpDevelop.
 * Utilisateur: johann
 * Date: 05/01/2012
 * Heure: 12:03
 * 
 * Pour changer ce modèle utiliser Outils | Options | Codage | Editer les en-têtes standards.
 */
using System;
using System.Diagnostics;
using SpringCard.LibCs;
using SpringCard.PCSC;

namespace SpringCard.NFC
{
	public abstract class LLCP_Server : LLCP_Service
	{
		private string _name;
		private byte _server_port = 0;
		private string _server_name = "";
		
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
				return (byte) (_server_port & 0x3F);
			}
		}
		
		public LLCP_Server(string _Name, byte _ServerPort, string _ServerName)
		{
			_name = _Name;
			_server_port = _ServerPort;
			_server_name = _ServerName;
		}
		
		public abstract bool OnConnect(LLCP_Link link, byte[] ConnectParams);
		
		public override bool ProcessPDU(LLCP_Link link, LLCP_PDU recv_pdu)
		{
			/* Server action */
			/* ------------- */

			link.RemotePort = recv_pdu.SSAP;
			link.LocalPort = (byte) (_server_port & 0x3F);

			switch (recv_pdu.PTYPE)
			{
				case LLCP_PDU.PTYPE_CONNECT :
					Logger.Debug("> CONNECT");

					/* The client wants to connect us */
					link.State = LLCP_Link.StateActive;
					
					if (!OnConnect(link, recv_pdu.Payload))
					{
						/* Failed */
						link.Send_DM(LLCP_DM_PDU.REASON_REJECTED); /* 03 */
						/* Drop the link */
						return false;
					} else
						if (!link.Answered)
					{
						/* Confirm we agree (CC) */
						link.Send_CC();
					}
					break;
					
				case LLCP_PDU.PTYPE_DISC :
					Logger.Debug("> DISC");

					/* The client is leaving us */
					if (link.State == LLCP_Link.StateActive)
					{
						OnDisconnect(link);
						link.Send_DM(LLCP_DM_PDU.REASON_DISC_OK); /* 00 */
					} else
					{
						link.Send_DM(LLCP_DM_PDU.REASON_NO_CONNECTION); /* 01 */
					}
					/* Drop the link */
					return false;
					
				case LLCP_PDU.PTYPE_I :
					Logger.Debug("> I");

					/* The client is talking to us */
					if (link.State == LLCP_Link.StateActive)
					{
						byte nr = (byte) (recv_pdu.Sequence & 0x0F);
						byte ns = (byte) ((recv_pdu.Sequence >> 4) & 0x0F);
						
						link.SetRemoteSequence((byte) ((ns + 1) & 0x0F));
						
						OnInformation(link, recv_pdu.Payload);

						if (!link.Answered)
						{
							/* No frame -> ACK */
							link.Send_RR();
						}
					} else
					{
						link.Send_DM(LLCP_DM_PDU.REASON_NO_CONNECTION); /* 01 */
					}
					break;
					
				case LLCP_PDU.PTYPE_RR :
					Logger.Debug("> RR");
					
					/* The client is acknowledging */
					if (link.State == LLCP_Link.StateActive)
					{
						byte nr = (byte) (recv_pdu.Sequence & 0x0F);
						
						OnAcknowledge(link);
					} else
					{
						link.Send_DM(LLCP_DM_PDU.REASON_NO_CONNECTION); /* 01 */
					}
					break;					
					
				default :
					Logger.Debug("Unsupported LLCP frame client->server");
					link.Send_FRMR(0x80, recv_pdu.PTYPE, 0x00);
					/* Drop the link */
					return false;
			}

			/* Keep the link OK */
			return true;
		}
	}
}

