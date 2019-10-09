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
using SpringCard.NfcForum.Ndef;
using SpringCard.LibCs;

namespace SpringCard.NFC
{
	public class SNEP_Server : LLCP_Server
	{
		private bool Fragments;
		private int Offset;
		private byte[] Buffer;
		
		public delegate void NdefReceivedCallback(NdefObject ndef);
		public NdefReceivedCallback OnNdefReceived = null;

		public delegate void MessageReceivedCallback(byte[] message);
		public MessageReceivedCallback OnMessageReceived = null;
		
		public SNEP_Server(NdefReceivedCallback NdefReceived) : base("SNEP Server", SNEP.SERVER_PORT, SNEP.SERVER_NAME) /* SNEP server address is 0x04 */
		{
			OnNdefReceived = NdefReceived;
			OnMessageReceived = MessageToNdef;
		}
		
		public SNEP_Server(MessageReceivedCallback MessageReceived) : base("SNEP Server", SNEP.SERVER_PORT, SNEP.SERVER_NAME) /* SNEP server address is 0x04 */
		{
			OnMessageReceived = MessageReceived;
			OnNdefReceived = null;
		}
		
		void NdefFound(NdefObject ndef)
		{
			if (ndef == null)
				Logger.Debug("SNEP Server: empty NDEF received");
			else
				Logger.Debug("SNEP Server: valid NDEF received");
			
			if (OnNdefReceived != null)
				OnNdefReceived(ndef);
		}
		
		void MessageToNdef(byte[] message)
		{
			/* Parse the received data - it must be an NDEF message */
			if (!NdefObject.Parse(message, NdefFound))
				Logger.Debug("SNEP Server: got an invalid message");
		}
		
		void ServerResponse(LLCP_Link link, byte response_code)
		{
			byte[] buffer = new byte[6];
			
			Logger.Debug("SNEP Server: say " + String.Format("{0:X02}", response_code));
			
			buffer[0] = 0x10; /* SNEP version */
			buffer[1] = response_code;
			
			link.Send_I(buffer);
		}
		
		public override bool OnConnect(LLCP_Link link, byte[] ConnectParams)
		{
			Fragments = false;
			Offset = 0;
			Buffer = null;
			
			Logger.Debug("SNEP Server: client connect");			
			
			byte[] param_miux = LLCP.FindParameter(ConnectParams, LLCP.PARAM_MAX_INF_UNIT_EXT);
			byte[] param_rw   = LLCP.FindParameter(ConnectParams, LLCP.PARAM_RECV_WINDOW_SIZE);

			/*
			byte[] cc_params = new byte[4];
			
			cc_params[0] = LLCP.PARAM_MAX_INF_UNIT_EXT;
			cc_params[1] = 2;
			cc_params[2] = (byte) ((SNEP.MAX_BUFFER_SIZE - 128) / 0x0100);
			cc_params[3] = (byte) ((SNEP.MAX_BUFFER_SIZE - 128) % 0x0100);
					
			link.Send_CC(cc_params);
			*/
			
			return true;
		}
		
		public override void OnDisconnect(LLCP_Link link)
		{
			Logger.Debug("SNEP Server: client disconnect");
		}
		
		public override void OnInformation(LLCP_Link link, byte[] ServiceDataUnit)
		{
			if (Fragments)
			{
				/* Is the length acceptable ? */
				if (Offset + ServiceDataUnit.Length > Buffer.Length)
				{
					/* Overflow */
					Logger.Debug("SNEP Server: bad request");
					ServerResponse(link, SNEP.RSP_BAD_REQUEST);
					return;
				}
				
				/* Get the NFC_DATA */
				for (int i=0; i<ServiceDataUnit.Length; i++)
					Buffer[Offset++] = ServiceDataUnit[i];
				
				/* Is the message complete ? */
				if (Offset >= Buffer.Length)
				{
					/* Yes ! Process the message */
					Logger.Debug("SNEP Server: got last fragment (" + ServiceDataUnit.Length + "B)");
					Fragments = false;
				} else
				{
					/* Do nothing */
					Logger.Debug("SNEP Server: got a fragment (" + ServiceDataUnit.Length + "B), more to come");
					link.Send_RR();
					return;
				}
			} else
			{
				if (ServiceDataUnit.Length < 6)
				{
					/* Frame is too short to have the REQUEST header */
					Logger.Debug("SNEP Server: bad request");
					ServerResponse(link, SNEP.RSP_BAD_REQUEST);
					return;
				}
				
				/* Get version */
				byte version = ServiceDataUnit[0];
				if ((version < 0x10) || (version >= 0x20))
				{
					/* Version not supported */
					Logger.Debug("SNEP Server: unsupported version");
					ServerResponse(link, SNEP.RSP_UNSUPPORTED_VERSION);
					return;
				}
				
				/* Request code */
				byte request = ServiceDataUnit[1];
				if (request != SNEP.REQ_PUT)
				{
					/* Only PUT REQUEST implemented */
					Logger.Debug("SNEP Server: not implemented");
					ServerResponse(link, SNEP.RSP_NOT_IMPLEMENTED);
					return;
				}
				
				/* Message length */
				uint total_length;
				total_length  = ServiceDataUnit[2]; total_length <<= 8;
				total_length |= ServiceDataUnit[3]; total_length <<= 8;
				total_length |= ServiceDataUnit[4]; total_length <<= 8;
				total_length |= ServiceDataUnit[5];
				
				/* Is the length acceptable ? */
				if (total_length > SNEP.MAX_BUFFER_SIZE)
				{
					/* Overflow */
					Logger.Debug("SNEP Server: reject");
					ServerResponse(link, SNEP.RSP_REJECT);
					return;
				}
				
				/* Allocate the buffer */
				Buffer = new byte[total_length];
				Offset = 0;
				
				/* Get the NFC_DATA */
				for (int i=6; i<ServiceDataUnit.Length; i++)
					Buffer[Offset++] = ServiceDataUnit[i];
				
				/* Is the message complete ? */
				if (Offset >= Buffer.Length)
				{
					/* Yes ! Process the message */
					Logger.Debug("SNEP Server: got single fragment (" + ServiceDataUnit.Length + "B)");
				} else
				{
					/* No ! The client may continue */
					Fragments = true;
					Logger.Debug("SNEP Server: got first fragment (" + ServiceDataUnit.Length + "B), more to come");
					Logger.Debug("SNEP Server: continue");
					ServerResponse(link, SNEP.RSP_CONTINUE);
					return;
				}
			}
			
			/* Say thank you */
			Logger.Debug("SNEP Server: thank you");
			ServerResponse(link, SNEP.RSP_SUCCESS);
			
			/* Process the message */
			if (OnMessageReceived != null)
				OnMessageReceived(Buffer);
		}
		
		public override void OnAcknowledge(LLCP_Link link)
		{
			Logger.Debug("SNEP Server: client aknowledge");
		}
	}
}
