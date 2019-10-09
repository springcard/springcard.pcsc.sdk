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
using SpringCard.PCSC;

namespace SpringCard.NFC
{
	public abstract class LLCP_Service
	{
		public abstract void OnDisconnect(LLCP_Link link);
		
		public abstract void OnAcknowledge(LLCP_Link link);
		public abstract void OnInformation(LLCP_Link link, byte[] ServiceDataUnit);
		
		public abstract bool ProcessPDU(LLCP_Link link, LLCP_PDU recv_pdu);
	}
}
