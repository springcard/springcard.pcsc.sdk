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
	public class SNEP_Client : LLCP_Client
	{
		public class SNEP_ClientInstanceData : LLCP_Link.LLCP_InstanceData
		{
			public int FragmentSize = 0;
			public int Offset = 0;
			public bool SomethingSent = false;
			public bool MayContinue = false;
			public bool MessageSentSent = false;
		}
		
		private byte[] SnepDataToSend = null;
		
		public SNEP_Client(NdefObject _NdefDataToSend) : base("SNEP Client", SNEP.SERVER_PORT)
		{
			if (_NdefDataToSend != null)
				SnepDataToSend = _NdefDataToSend.Serialize();
			
			Logger.Debug("SNEP Client: creation with NDEF=" + (new CardBuffer(SnepDataToSend)).AsString());
		}
		
		public delegate void SNEPMessageSentCallback();
		public bool MessageSentCallbackOnAcknowledge = false;
		public SNEPMessageSentCallback OnMessageSent = null;
		
		private void ClientPUT(LLCP_Link link, SNEP_ClientInstanceData instance)
		{
			int length = 6 + SnepDataToSend.Length;

			if (length > instance.FragmentSize)
			{
				instance.Offset = instance.FragmentSize - 6;
				length = instance.FragmentSize;
			} else
			{
				instance.Offset = length - 6;
			}
			
			byte[] buffer = new byte[length];
			
			buffer[0] = 0x10; /* SNEP version */
			buffer[1] = SNEP.REQ_PUT;
			buffer[4] = (byte) (SnepDataToSend.Length / 0x0100);
			buffer[5] = (byte) (SnepDataToSend.Length % 0x0100);
			
			for (int i=0; i<instance.Offset; i++)
				buffer[i+6] = SnepDataToSend[i];
			
			Logger.Debug("SNEP Client: sending (" + instance.Offset + "/" + SnepDataToSend.Length + "): " + (new CardBuffer(buffer)).AsString());			
			instance.SomethingSent = true;
			link.Send_I(buffer);
		}

		private void ClientNEXT(LLCP_Link link, SNEP_ClientInstanceData instance)
		{
			if (SnepDataToSend == null)
				return;
			
			int length = SnepDataToSend.Length - instance.Offset;
			
			if (length <= 0)
				return;
			
			if (length > instance.FragmentSize)
				length = instance.FragmentSize;
			
			byte[] buffer = new byte[length];
			for (int i=0; i<length; i++)
				buffer[i] = SnepDataToSend[instance.Offset++];

			if (instance.Offset >= SnepDataToSend.Length)
				instance.MayContinue = false;
			
			Logger.Debug("SNEP Client: sending (" + instance.Offset + "/" + SnepDataToSend.Length + "): " + (new CardBuffer(buffer)).AsString());
			instance.SomethingSent = true;
			link.Send_I(buffer);
		}
		
		public override void OnConnect(LLCP_Link link, byte[] payload)
		{
			Logger.Debug("SNEP Client: connected to server");
			
			if (SnepDataToSend == null)
				return;

			SNEP_ClientInstanceData instance = new SNEP_ClientInstanceData();
			link.InstanceData = instance;
			instance.FragmentSize = link.MaxPayload;
			
			ClientPUT(link, instance);
		}
		
		public override void OnDisconnect(LLCP_Link link)
		{
			Logger.Debug("SNEP Client: disconnected from server");
		}
		
		public override void OnInformation(LLCP_Link link, byte[] ServiceDataUnit)
		{
			SNEP_ClientInstanceData instance = (SNEP_ClientInstanceData) link.InstanceData;
			
			Logger.Debug("SNEP Client: I...");
			instance.SomethingSent = false;
			
			if (ServiceDataUnit.Length < 2)
			{
				/* Frame is too short to have the RESPONSE header */
				Logger.Debug("SNEP Client: I.Bad length");
				goto close;
			}
			
			/* Get version */
			byte version = ServiceDataUnit[0];
			if ((version < 0x10) || (version >= 0x20))
			{
				/* Version not supported */
				Logger.Debug("SNEP Client: I.Bad version");
				goto close;
			}
			
			/* Response code */
			byte response = ServiceDataUnit[1];
			switch (response)
			{
				case SNEP.RSP_CONTINUE :
					/* Message successfully transmited, let's continue */
					instance.MayContinue = true;
					Logger.Debug("SNEP Client: I.Continue -> send RR, then second I");
					link.Send_RR();
					ClientNEXT(link, instance);
					break;
					
				case SNEP.RSP_SUCCESS :
					/* Message successfully transmited, let's disconnect */
					Logger.Debug("SNEP Client: I.Success -> send RR");
					/* Acknowledge */
					link.Send_RR();
					if ((OnMessageSent != null) && (!instance.MessageSentSent))
					{
						OnMessageSent();
						instance.MessageSentSent = true;
					}
					break;
					
				default :
				    /* An error has occured */
					Logger.Debug("SNEP Client: I.Bad Opcode");
					goto close;
			}
			
			/*  */
			return;
			
		close:
			
			Logger.Debug("SNEP Client: Force exit");
			link.Send_Disc();
		}
				
		public override void OnAcknowledge(LLCP_Link link)
		{
			SNEP_ClientInstanceData instance = (SNEP_ClientInstanceData) link.InstanceData;

			if (instance.MayContinue)
			{
				Logger.Debug("SNEP Client: RR -> Next I");
				ClientNEXT(link, instance);
			}
			else if (instance.SomethingSent)
			{
				Logger.Debug("SNEP Client: RR (thank you)");
				instance.SomethingSent = false;					
			}			
			else
			{
				Logger.Debug("SNEP Client: RR unexpected");
				if (MessageSentCallbackOnAcknowledge)
				{
					if ((OnMessageSent != null) && (!instance.MessageSentSent))
					{
						OnMessageSent();
						instance.MessageSentSent = true;
					}
				}
			}			
		}
	}
}
