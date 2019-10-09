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
	public abstract class SNEP
	{
		public const string SERVER_NAME = "urn:nfc:sn:snep";
		public const byte SERVER_PORT = 0x04;
		
		public const byte VERSION = 0x10;
		public const byte REQ_CONTINUE = 0x00;
		public const byte REQ_GET = 0x01;
		public const byte REQ_PUT = 0x02;
		public const byte REQ_REJECT = 0x7F;
		public const byte RSP_CONTINUE = 0x80;
		public const byte RSP_SUCCESS = 0x81;
		public const byte RSP_NOT_FOUND = 0xC0;
		public const byte RSP_EXCESS_DATA = 0xC1;
		public const byte RSP_BAD_REQUEST = 0xC2;
		public const byte RSP_NOT_IMPLEMENTED = 0xE0;
		public const byte RSP_UNSUPPORTED_VERSION = 0xE1;
		public const byte RSP_REJECT = 0xFF;
		
		public const uint MAX_BUFFER_SIZE = 4096;
		
		public static byte[] CreateFrame(byte Request, byte[] Information)
		{
			byte[] r;
			int l = 0;
			
			if (Information != null)
				l = Information.Length;

			r = new byte[6 + l];
			
			r[0] = VERSION;
			r[1] = Request;
			r[2] = (byte) ((l >> 24) & 0x000000FF);
			r[3] = (byte) ((l >> 16) & 0x000000FF);
			r[4] = (byte) ((l >> 8) & 0x000000FF);
			r[5] = (byte) (l & 0x000000FF);
			
			if (Information != null)
			{
				for (int i=0; i<Information.Length; i++)
					r[6+i] = Information[i];
			}
			
			return r;
		}
		
		public static bool CheckFrame(byte[] ServiceDataUnit, ref byte Version, ref byte Request, ref int Length, ref byte[] Information)
		{
			if ((ServiceDataUnit == null) || (ServiceDataUnit.Length < 6))
			{
				/* Too short */
				return false;
			}
			
			Version = ServiceDataUnit[0];
			Request = ServiceDataUnit[1];
			Length  = ServiceDataUnit[2]; Length <<= 8;
			Length |= ServiceDataUnit[3]; Length <<= 8;
			Length |= ServiceDataUnit[4]; Length <<= 8;
			Length |= ServiceDataUnit[5];
			
			if (ServiceDataUnit.Length > (Length + 6))
			{
				/* Bad length */
				return false;
			}
			
			if (ServiceDataUnit.Length > 6)
			{
				Information = new byte[Length];
				for (int i=6; i<ServiceDataUnit.Length; i++)
					Information[i-6] = ServiceDataUnit[i];
			} else
			{
				Information = null;
			}
			
			return true;
		}
	}
}
