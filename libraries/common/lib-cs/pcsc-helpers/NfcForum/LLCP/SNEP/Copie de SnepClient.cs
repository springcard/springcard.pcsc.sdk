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

namespace SpringCard.NFC
{
	public class SNEP_Client : LLCP_Client
	{
		public class SNEP_ClientInstanceData : LLCP_Link.LLCP_InstanceData
		{
			public int Offset = 0;
			public bool InHandshaking = true;
			public bool MayContinue = false;
			public int DummyInfSent = 0;
			public bool MessageSentSent = false;
		}
		
		private byte[] SnepDataToSend = null;
		
		public SNEP_Client(Ndef _NdefDataToSend) : base("SNEP Client", SNEP.SERVER_PORT)
		{
			if (_NdefDataToSend != null)
				SnepDataToSend = _NdefDataToSend.GetBytes();
			
			Trace.WriteLine("SNEP Client: creation with NDEF=" + (new CardBuffer(SnepDataToSend)).AsString());
		}
		
		public delegate void SNEPMessageSentCallback();
		public bool MessageSentCallbackOnAcknowledge = false;
		public SNEPMessageSentCallback OnMessageSent = null;
		
		private void ClientPUT(LLCP_Link link, SNEP_ClientInstanceData instance)
		{
			int length = 6 + SnepDataToSend.Length;

			if (length > link.MaxPayload)
			{
				instance.Offset = link.MaxPayload - 6;
				length = link.MaxPayload;
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
			
			Trace.WriteLine("SNEP Client: sending " + (new CardBuffer(buffer)).AsString());
			
			link.Send_I(buffer);
		}

		private void ClientNEXT(LLCP_Link link, SNEP_ClientInstanceData instance)
		{
			if (SnepDataToSend == null)
				return;
			
			int length = SnepDataToSend.Length - instance.Offset;
			
			if (length <= 0)
				return;
			
			if (length > link.MaxPayload)
				length = link.MaxPayload;
			
			byte[] buffer = new byte[length];
			for (int i=0; i<length; i++)
				buffer[i] = SnepDataToSend[instance.Offset++];

			if (instance.Offset >= SnepDataToSend.Length)
				instance.MayContinue = false;
			
			Trace.WriteLine("SNEP Client: sending " + (new CardBuffer(buffer)).AsString());
			link.Send_I(buffer);
		}
		
		public override void OnConnect(LLCP_Link link, byte[] payload)
		{
			Trace.WriteLine("SNEP Client: connected to server");
			
			if (SnepDataToSend == null)
				return;

			SNEP_ClientInstanceData instance = new SNEP_ClientInstanceData();
			link.InstanceData = instance;
			
			ClientPUT(link, instance);
		}
		
		public override void OnDisconnect(LLCP_Link link)
		{
			Trace.WriteLine("SNEP Client: disconnected from server");
		}
		
		public override void OnInformation(LLCP_Link link, byte[] ServiceDataUnit)
		{
			Trace.WriteLine("SNEP Client: information");
			
			SNEP_ClientInstanceData instance = (SNEP_ClientInstanceData) link.InstanceData;
			
			if (ServiceDataUnit.Length < 2)
			{
				/* Frame is too short to have the RESPONSE header */
				goto close;
			}
			
			/* Get version */
			byte version = ServiceDataUnit[0];
			if ((version < 0x10) || (version >= 0x20))
			{
				/* Version not supported */
				Trace.WriteLine("SNEP Client: bad version");
				goto close;
			}
			
			/* Response code */
			byte response = ServiceDataUnit[1];
			switch (response)
			{
				case SNEP.RSP_CONTINUE :
					/* Message successfully transmited, let's disconnect */
					instance.MayContinue = true;
					ClientNEXT(link, instance);
					return; /* No close here - this is the only case */
					
				case SNEP.RSP_SUCCESS :
					/* Message successfully transmited, let's disconnect */
					Trace.WriteLine("SNEP Client: success");
					if ((OnMessageSent != null) && (!instance.MessageSentSent))
					{
						OnMessageSent();
						instance.MessageSentSent = true;
					}
					return; //
					break;
					
				default :
					break;
			}
			
		close:
			
			link.Send_Disc();
		}
		
		void MessageSent()
		{
			
		}
		
		public override void OnAcknowledge(LLCP_Link link)
		{
			Trace.WriteLine("SNEP Client: acknowledge");
			
			SNEP_ClientInstanceData instance = (SNEP_ClientInstanceData) link.InstanceData;

			if (instance.InHandshaking)
			{
				Trace.WriteLine("First RR -> dummy INF");
				link.Send_I(null);
				instance.InHandshaking = false;
			}
			else if (instance.MayContinue)
			{
				Trace.WriteLine("Next INF");
				ClientNEXT(link, instance);
			}
			else if (instance.DummyInfSent < 1)
			{
				Trace.WriteLine("Dummy INF");
				instance.DummyInfSent++;
				link.Send_I(null);
			}
			else
			{
				Trace.WriteLine("Final INF");
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
