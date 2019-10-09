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
using System.Threading;
using SpringCard.PCSC;

namespace SpringCard.NFC
{
	public class LLCP_Link
	{
		public class LLCP_InstanceData
		{
			public LLCP_InstanceData()
			{
				
			}
		}
		
		public const byte StateWaiting = 0;
		public const byte StateOpening = 1;
		public const byte StateActive = 2;
		public const byte StateClosing = 3;
		public const byte StateClosed = 4;
		
		private byte RemoteSequence = 0;
		private byte LocalSequence = 0;
		public byte RemotePort = 0;
		public byte LocalPort = 0;
		
		public int MaxPayload = 128;
		public byte State = 0;
		private bool _answered = false;
		
		private LLCP _llcp;
		private LLCP_Service _service;
		
		public LLCP_InstanceData InstanceData;
		
		public LLCP_Link(LLCP llcp, LLCP_Service service)
		{
			_llcp = llcp;
			_service = service;
		}

		public LLCP_Link(LLCP llcp, LLCP_Service service, LLCP_PDU recv_pdu)
		{
			_llcp = llcp;
			_service = service;
			RemotePort = recv_pdu.SSAP;
			LocalPort = recv_pdu.DSAP;
		}

		public LLCP_Link(LLCP llcp, LLCP_Client client, byte local_port)
		{
			_llcp = llcp;
			_service = client;
			RemotePort = client.ServerPort;
			LocalPort = local_port;
		}
		
		public bool Answered
		{
			get
			{
				return _answered;
			}
		}
		
		public void SetRemoteSequence(byte remote_sequence)
		{
			RemoteSequence = remote_sequence;
		}
		
		public bool ProcessPDU(LLCP_PDU recv_pdu)
		{
			_answered = false;
			
			switch (recv_pdu.PTYPE)
			{
				case LLCP_PDU.PTYPE_CONNECT :
				case LLCP_PDU.PTYPE_CC :
					RemoteSequence = 0;
					LocalSequence = 0;
					break;
					default :
						break;
			}
			
			return _service.ProcessPDU(this, recv_pdu);
		}
		
		public void Send_Disc()
		{
			LLCP_PDU send_pdu = new LLCP_DISC_PDU(RemotePort, LocalPort);
			
			_llcp.SendPDU_PUSH(send_pdu);

			_answered = true;
			if (State == StateActive)
				State = StateClosing;
		}

		public void Send_Connect(byte[] payload)
		{
			LLCP_PDU send_pdu = new LLCP_CONNECT_PDU(RemotePort, LocalPort, payload);

			_llcp.SendPDU_PUSH(send_pdu);
			
			State = StateOpening;
		}
		
		public void Send_Connect()
		{
			Send_Connect(null);
		}
		
		public void Send_DM(byte reason)
		{
			LLCP_PDU send_pdu = new LLCP_DM_PDU(RemotePort, LocalPort, reason);

			_llcp.SendPDU_PUSH(send_pdu);
			
			_answered = true;
			if (State > StateWaiting)
				State = StateClosed;
		}
		
		public void Send_FRMR(byte flags, byte ptype, byte sequence)
		{
			LLCP_PDU send_pdu = new LLCP_FRMR_PDU(RemotePort, LocalPort, flags, ptype, sequence);

			_llcp.SendPDU_PUSH(send_pdu);
			
			_answered = true;
			if (State > StateWaiting)
				State = StateClosed;
			
		}

		public void Send_CC(byte[] payload)
		{
			LLCP_PDU send_pdu = new LLCP_CC_PDU(RemotePort, LocalPort, payload);

			_llcp.SendPDU_PUSH(send_pdu);
			
			_answered = true;
			State = StateActive;
		}
		
		public void Send_CC()
		{
			Send_CC(null);
		}
		
		public void Send_RR()
		{
			byte nr = (byte) (RemoteSequence & 0x0F);
			
			LLCP_PDU send_pdu = new LLCP_RR_PDU(RemotePort, LocalPort, nr);

			_llcp.SendPDU_PUSH(send_pdu);
			
			_answered = true;
		}
		
		public void Send_I(byte[] payload)
		{
			byte ns = (byte) (LocalSequence & 0x0F);
			byte nr = (byte) (RemoteSequence & 0x0F);
			
			LLCP_PDU send_pdu = new LLCP_I_PDU(RemotePort, LocalPort, ns, nr, payload);
			
			_llcp.SendPDU_PUSH(send_pdu);
			
			LocalSequence += 1;
			LocalSequence &= 0x0F;
			
			_answered = true;
		}
		
	}
}
