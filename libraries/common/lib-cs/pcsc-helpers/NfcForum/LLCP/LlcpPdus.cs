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

namespace SpringCard.NFC
{
	public class LLCP_PARAMETER
	{
		public const byte TYPE_VERSION = 0x01;
		public const byte TYPE_MIUX = 0x02;
		public const byte TYPE_WKS = 0x03;
		public const byte TYPE_LTO = 0x04;
		public const byte TYPE_RW = 0x05;
		public const byte TYPE_SN = 0x06;
		public const byte TYPE_OPT = 0x07;
		public const byte TYPE_SDREQ = 0x08;
		public const byte TYPE_SDRES = 0x09;
		
		protected byte _Type = 0;
		protected byte[] _Value = null;
		
		public LLCP_PARAMETER()
		{
			
		}
		
		public LLCP_PARAMETER(byte Type, byte[] Value)
		{
			_Type = Type;
			_Value = Value;
		}
		
		public LLCP_PARAMETER(byte Type, byte Value)
		{
			_Type = Type;
			_Value = new byte[1];
			_Value[0] = Value;
		}
		
		public int Length()
		{
			if ((_Value == null) || (_Value.Length > 255))
				return 0;
			return 2 + _Value.Length;
		}
		
		public byte[] GetBytes()
		{
			if ((_Value == null) || (_Value.Length > 255))
				return null;
			
			byte[] r = new byte[2 + _Value.Length];
			
			r[0] = _Type;
			r[1] = (byte) _Value.Length;
			
			for (int i=0; i<_Value.Length; i++)
				r[i+2] = _Value[i];

			return r;
		}

	}
	
	public class LLCP_PDU : CardBuffer
	{
		public const byte PTYPE_SYMM = 0x00;
		public const byte PTYPE_PAX = 0x01;
		public const byte PTYPE_AGF = 0x02;
		public const byte PTYPE_UI = 0x03;
		public const byte PTYPE_CONNECT = 0x04;
		public const byte PTYPE_DISC = 0x05;
		public const byte PTYPE_CC = 0x06;
		public const byte PTYPE_DM = 0x07;
		public const byte PTYPE_FRMR = 0x08;
		public const byte PTYPE_SNL = 0x09;
		public const byte PTYPE_I = 0x0C;
		public const byte PTYPE_RR = 0x0D;
		public const byte PTYPE_RNR = 0x0E;
		
		
		
		public override string AsString(string separator)
		{
			string s = "";
			long i;
			
			if ((Bytes == null) || (Bytes.Length < 2))
			{
				return "#ERROR";
			}
			
			s += String.Format("DSAP={0:X02} ", DSAP);
			s += String.Format("TYPE={0:X02} ", PTYPE);
			s += String.Format("SSAP={0:X02} ", SSAP);
			
			if (hasSequence())
			{
				if (Bytes.Length > 2)
					s += String.Format("SEQ={0:X02} ", Bytes[2]);
				
				for (i = 3; i < Bytes.Length; i++)
				{
					if (i > 3)
						s = s + separator;
					s = s + String.Format("{0:X02}", Bytes[i]);
				}
			} else
			{
				for (i = 2; i < Bytes.Length; i++)
				{
					if (i > 2)
						s = s + separator;
					s = s + String.Format("{0:X02}", Bytes[i]);
				}
			}
			
			return s;
		}

		
		private void ensureLength(int length)
		{
			if (length <= 0)
			{
                Bytes = null;
				return;
			}
			
			if (Bytes == null)
			{
                Bytes = new byte[length];
			} else
				if (Bytes.Length < length)
			{
				byte[] b = new byte[length];
				
				for (int i=0; i< Bytes.Length; i++)
					b[i] = Bytes[i];

                Bytes = b;
			}
		}
		
		private void addHeader(byte PTYPE, byte DSAP, byte SSAP)
		{
			ensureLength(2);
			
			PTYPE &= 0x0F;
			DSAP  &= 0x3F;
			SSAP  &= 0x3F;
			
			int h0 = ((DSAP << 2) | (PTYPE >> 2));
			int h1 = ((SSAP     ) | (PTYPE << 6));

            Bytes[0] = (byte) (h0 & 0xFF);
            Bytes[1] = (byte) (h1 & 0xFF);
		}
		
		private void addSequence(byte Sequence)
		{
			ensureLength(3);
            Bytes[2] = Sequence;
		}
		
		protected void addPayload(byte[] Payload)
		{
			if (Payload != null)
			{
				if (hasSequence())
				{
					ensureLength(3 + Payload.Length);
					for (int i=0; i<Payload.Length; i++)
                        Bytes[3+i] = Payload[i];
				} else
				{
					ensureLength(2 + Payload.Length);
					for (int i=0; i<Payload.Length; i++)
                        Bytes[2+i] = Payload[i];
				}
			}
		}
		
		public static bool hasSequence(byte PTYPE)
		{
			PTYPE &= 0x0F;
			
			switch (PTYPE)
			{
				case PTYPE_I :
				case PTYPE_RR :
				case PTYPE_RNR :
					return true;
					
					default :
						return false;
			}
		}
		
		private bool hasSequence()
		{
			return hasSequence(PTYPE);
		}
		
		public LLCP_PDU(CardBuffer Buffer)
		{
            Bytes = Buffer.GetBytes();
		}
		
		public LLCP_PDU(LLCP_PDU LLCP_PDU)
		{
            Bytes = LLCP_PDU.GetBytes();
		}
		
		public LLCP_PDU(byte PTYPE, byte DSAP, byte SSAP, byte Sequence, byte[] Payload) : this(PTYPE, DSAP, SSAP)
		{
			addSequence(Sequence);
			addPayload(Payload);
		}
		
		public LLCP_PDU(byte PTYPE, byte DSAP, byte SSAP, byte[] Payload) : this(PTYPE, DSAP, SSAP)
		{
			addPayload(Payload);
		}

		public LLCP_PDU(byte PTYPE, byte DSAP, byte SSAP, LLCP_PARAMETER[] Parameters) : this(PTYPE, DSAP, SSAP)
		{
			if (Parameters != null)
			{
				int TotalLength = 0;
				
				for (int i=0; i<Parameters.Length; i++)
					TotalLength += Parameters[i].Length();
				
				byte[] Payload = new byte[TotalLength];
				
				int Offset = 0;
				
				for (int i=0; i<Parameters.Length; i++)
				{
					byte[] ParameterBytes = Parameters[i].GetBytes();
					for (int j=0; j<ParameterBytes.Length; j++)
						Payload[Offset++] = ParameterBytes[j];
				}
				
				addPayload(Payload);
			}
		}

		public LLCP_PDU(byte PTYPE, byte DSAP, byte SSAP, byte Sequence) : this(PTYPE, DSAP, SSAP)
		{
			addSequence(Sequence);
		}
		
		public LLCP_PDU(byte PTYPE, byte DSAP, byte SSAP)
		{
			addHeader(PTYPE, DSAP, SSAP);
		}

        public bool Valid
        {
            get
            {
                if ((Bytes != null) && (Bytes.Length >= 2))
                    return true;
                return false;
            }
        }
		
		public byte PTYPE
		{
			get
			{
				byte r;
				
				r = (byte) (((Bytes[0] << 2) & 0x0C) | ((Bytes[1] >> 6) & 0x03));

				return r;
			}
		}

		public byte DSAP
		{
			get
			{
				byte r;
				
				r = (byte) ((Bytes[0] >> 2) & 0x3F);

				return r;
			}
		}
		
		public byte SSAP
		{
			get
			{
				byte r;
				
				r = (byte) (Bytes[1] & 0x3F);

				return r;
			}
		}

		public byte[] Header
		{
			get
			{
				if ((Bytes == null) || (Bytes.Length < 2))
					return null;
				
				byte[] b = new byte[2];
				b[0] = Bytes[0];
				b[1] = Bytes[1];
				return b;
			}
		}

		public byte Sequence
		{
			get
			{
				if ((Bytes == null) || (Bytes.Length < 3) || !hasSequence())
					return 0;
				
				return Bytes[2];
			}
		}

		public byte[] Payload
		{
			get
			{
				if ((Bytes == null) || (Bytes.Length <= 2))
					return null;

				if (hasSequence())
				{
					if (Bytes.Length <= 2)
						return null;
					
					byte[] b = new byte[Bytes.Length - 3];
					for (int i=0; i<b.Length; i++)
						b[i] = Bytes[i+3];
					return b;
					
				} else
				{
					byte[] b = new byte[Bytes.Length - 2];
					for (int i=0; i<b.Length; i++)
						b[i] = Bytes[i+2];
					return b;
				}
			}
		}
	}
	
	public class LLCP_SYMM_PDU : LLCP_PDU
	{
		public LLCP_SYMM_PDU() : base(PTYPE_SYMM, 0, 0)
		{

		}
	}

	public class LLCP_PAX_PDU : LLCP_PDU
	{
		public LLCP_PAX_PDU(LLCP_PARAMETER[] Parameters) : base(PTYPE_PAX, 0, 0, Parameters)
		{

		}
		
		public LLCP_PAX_PDU(LLCP_PDU LLCP_PDU) : base(LLCP_PDU)
		{

		}
	}

	public class LLCP_SYMM_AGF : LLCP_PDU
	{
		public LLCP_SYMM_AGF(LLCP_PDU[] Pdus) : base(PTYPE_AGF, 0, 0)
		{
			int TotalLength = 0;
			
			for (int i=0; i<Pdus.Length; i++)
				TotalLength += Pdus[i].Length;
			
			byte[] Payload = new byte[TotalLength];
			
			int Offset = 0;
			
			for (int i=0; i<Pdus.Length; i++)
			{
				byte[] PduBytes = Pdus[i].GetBytes();
				for (int j=0; j<PduBytes.Length; j++)
					Payload[Offset++] = PduBytes[j];
			}
			
			addPayload(Payload);
		}
	}
	
	public class LLCP_UI_PDU : LLCP_PDU
	{
		public LLCP_UI_PDU(byte DSAP, byte SSAP, byte[] ServiceData) : base(PTYPE_UI, DSAP, SSAP, ServiceData)
		{
			
		}
	}
	
	public class LLCP_CONNECT_PDU : LLCP_PDU
	{
		public LLCP_CONNECT_PDU(byte DSAP, byte SSAP, LLCP_PARAMETER[] Parameters) : base(PTYPE_CONNECT, DSAP, SSAP, Parameters)
		{
			
		}
		
		public LLCP_CONNECT_PDU(byte DSAP, byte SSAP, byte[] Parameters) : base(PTYPE_CONNECT, DSAP, SSAP, Parameters)
		{
			
		}
	}

	public class LLCP_DISC_PDU : LLCP_PDU
	{
		public LLCP_DISC_PDU(byte DSAP, byte SSAP) : base(PTYPE_DISC, DSAP, SSAP)
		{
			
		}
	}

	public class LLCP_CC_PDU : LLCP_PDU
	{
		public LLCP_CC_PDU(byte DSAP, byte SSAP, LLCP_PARAMETER[] Parameters) : base(PTYPE_CC, DSAP, SSAP, Parameters)
		{
			
		}
		
		public LLCP_CC_PDU(byte DSAP, byte SSAP, byte[] Parameters) : base(PTYPE_CC, DSAP, SSAP, Parameters)
		{
			
		}
	}

	public class LLCP_DM_PDU : LLCP_PDU
	{
		public const byte REASON_DISC_OK = 0x00;
		public const byte REASON_NO_CONNECTION = 0x01;
		public const byte REASON_NO_SERVICE = 0x10;
		public const byte REASON_REJECTED = 0x03;
		
		public LLCP_DM_PDU(byte DSAP, byte SSAP, byte Reason) : base(PTYPE_DM, DSAP, SSAP)
		{
			byte[] Payload = new Byte[1];
			Payload[0] = Reason;
			addPayload(Payload);
		}
	}

	public class LLCP_FRMR_PDU : LLCP_PDU
	{
		public LLCP_FRMR_PDU(byte DSAP, byte SSAP, bool Wf, bool If, bool Rf, bool Sf, byte PTYPE, byte Sequence, byte Vs, byte Vr, byte Vsa, byte Vra) : base(PTYPE_FRMR, DSAP, SSAP)
		{
			byte[] Payload = new Byte[4];

			Payload[0] = 0;
			if (Wf)
				Payload[0] |= 0x80;
			if (If)
				Payload[0] |= 0x40;
			if (Rf)
				Payload[0] |= 0x20;
			if (Sf)
				Payload[0] |= 0x10;
			Payload[0] |= (byte) (PTYPE & 0x0F);
			
			Payload[1] = Sequence;
			
			Payload[2] = (byte) (((Vs << 4) & 0xF0) | (Vr & 0x0F));
			
			Payload[3] = (byte) (((Vsa << 4) & 0xF0) | (Vra & 0x0F));
			
			addPayload(Payload);
		}
		
		public LLCP_FRMR_PDU(byte DSAP, byte SSAP, byte Flags, byte PTYPE, byte Sequence) : base(PTYPE_FRMR, DSAP, SSAP)
		{
			byte[] Payload = new Byte[4];

			Payload[0] = (byte) ((Flags << 4) & 0xF0);
			Payload[0] |= (byte) (PTYPE & 0x0F);
			Payload[1] = Sequence;
			
			addPayload(Payload);
		}
	}

	public class LLCP_I_PDU : LLCP_PDU
	{
		public LLCP_I_PDU(byte DSAP, byte SSAP, byte Ns, byte Nr, byte[] Data) : base(PTYPE_I, DSAP, SSAP, (byte) (((Ns << 4) & 0xF0) | (Nr & 0x0F)), Data)
		{
			
		}
	}

	public class LLCP_RR_PDU : LLCP_PDU
	{
		public LLCP_RR_PDU(byte DSAP, byte SSAP, byte Nr) : base(PTYPE_RR, DSAP, SSAP, (byte) (Nr & 0x0F))
		{
			
		}
	}

	public class LLCP_RNR_PDU : LLCP_PDU
	{
		public LLCP_RNR_PDU(byte DSAP, byte SSAP, byte Nr) : base(PTYPE_RNR, DSAP, SSAP, (byte) (Nr & 0x0F))
		{
			
		}
	}
	
	public class LLCP_SNL_PDU : LLCP_PDU
	{
		public LLCP_SNL_PDU(LLCP_PARAMETER[] Parameters) : base(PTYPE_SNL, 0x01, 0x01, Parameters)
		{
			
		}
	}
	
}
