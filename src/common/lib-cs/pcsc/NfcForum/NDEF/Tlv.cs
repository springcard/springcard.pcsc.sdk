/**h* SpringCard.NFC/NfcTlv
 *
 * NAME
 *   SpringCard API for NFC Forum :: NFC Tlv class
 * 
 * COPYRIGHT
 *   Copyright (c) SpringCard SAS, 2012
 *   See LICENSE.TXT for information
 *
 **/
using System;
using System.Collections;
using System.Collections.Generic;
using SpringCard.PCSC;

namespace SpringCard.NFC
{

	/**c* SpringCard.NFC/NfcTlv
	 *
	 * NAME
	 *   NfcTlv
	 * 
	 * DESCRIPTION
	 *   Represents a TLV that has been found on a NFC Type 2 Tag, found on a reader
	 *
	 * SYNOPSIS
	 *   NfcTlv tlv = new NfcTlv()
	 *   NfcTlv tlv = new NfcTlv(byte t, byte[] v)
	 * 
	 * USED BY
	 * 	 NfcTagType2
	 *
	 **/
	public class NfcTlv
	{
		private const byte FLAG_L_3_BYTES = 0xFF;
		private byte _t;
		private byte[] _v;
		
		public NfcTlv()
		{
			_t = 0;
			_v = null;
		}
		
		public NfcTlv(byte t, byte[] v)
		{
			_t = t;
			_v = v;
		}
		
		private Ndef[] child = new Ndef[0];
		
		
		/**v* SpringCard.NFC/NfcTlv.T
		 *
		 * SYNOPSIS
		 *   public byte T
		 * 
		 * DESCRIPTION
		 *   Gets the Type of the TLV
		 *
		 *
		 **/
		public byte T
		{
			get
			{
				return _t;
			}
		}

		/**v* SpringCard.NFC/NfcTlv.L
		 *
		 * SYNOPSIS
		 *   public long L
		 * 
		 * DESCRIPTION
		 *   Gets the Length of the TLV
		 *
		 *
		 **/
		public long L
		{
			get
			{
				if (_v == null)
					return 0;
				return _v.Length;
			}
		}
		
		/**v* SpringCard.NFC/NfcTlv.V
		 *
		 * SYNOPSIS
		 *   public byte[] V
		 * 
		 * DESCRIPTION
		 *   Gets the Value of the TLV
		 *
		 *
		 **/
		public byte[] V
		{
			get
			{
				return _v;
			}
		}
		
		/**m* SpringCard.NFC/NfcTlv.Serialize
		 *
		 * SYNOPSIS
		 *   public byte[] Serialize()
		 * 
		 * DESCRIPTION
		 *   Serializes the TLV and returns the corresponding byte array
		 *
		 **/
		public byte[] Serialize()
		{
			byte[] r = null;
			
			if (L <= 254)
			{
				/* L is on 1 byte only */
				r = new byte[1 + 1 + L];
				r[0] = T;
				r[1] = (byte) L;
				for (long i=0; i<L; i++)
					r[2+i] = V[i];
				
			} else
				if (L < 65535)
			{
				/* L is on 3 bytes */
				r = new byte[1 + 3 + L];
				r[0] = T;
				r[1] = 0xFF;
				r[2] = (byte) (L / 0x0100);
				r[3] = (byte) (L % 0x0100);
				for (long i=0; i<L; i++)
					r[4+i] = V[i];
				
			}
			
			return r;
		}
		
		/**m* SpringCard.NFC/NfcTlv.Unserialize
		 *
		 * SYNOPSIS
		 *   public static NfcTlv Unserialize(byte[] buffer, ref byte[] remaining_buffer)
		 * 
		 * DESCRIPTION
		 *   Constructs and returns a NfcTlv object from the "buffer" byte array.
		 * 	 The "remaining_buffer" array contains the bytes that follow the ones which belong to the constructed TLV.
		 *
		 **/
		public static NfcTlv Unserialize(byte[] buffer, ref byte[] remaining_buffer)
		{
			byte t;
			long l, o = 0;
			byte[] v = null;
			
			remaining_buffer = null;
			
			if (buffer == null)
				return null;
			if (buffer.Length < 2)
				return null;
			
			t = buffer[0];
			
			if (buffer[1] == 0xFF)
			{
				if (buffer.Length < 4)
					return null;
				l = buffer[2] * 0x0100 + buffer[3];
				o = 4;
			} else
			{
				l = buffer[1];
				o = 2;
			}
			
			if ((o + l) > buffer.Length)
				return null;
			
			if (l > 0)
			{
				v = new byte[l];
				for (long i=0; i<l; i++)
					v[i] = buffer[o + i];
			}
			
			o += l;
			
			if (o < buffer.Length)
			{
				remaining_buffer = new byte[buffer.Length - o];
				for (long i=0; i<remaining_buffer.Length; i++)
					remaining_buffer[i] = buffer[o + i];
			}
			
			return new NfcTlv(t, v);
		}
		
		/**m* SpringCard.NFC/NfcTlv.add_child
		 *
		 * SYNOPSIS
		 *   public void add_child(Ndef ndef)
		 * 
		 * DESCRIPTION
		 *   Adds a child (ndef) to the NfcTlv object.
		 *
		 **/
		public void add_child(Ndef ndef)
		{
			Ndef[] tmp = new Ndef[child.Length + 1];
			Array.Copy(child, tmp, child.Length);
			tmp[tmp.Length -1 ] = ndef;
			child = new Ndef[tmp.Length];
			Array.Copy(tmp, child, child.Length);
		}

		/**m* SpringCard.NFC/NfcTlv.get_child
		 *
		 * SYNOPSIS
		 *   public Ndef get_child(int  i)
		 * 
		 * DESCRIPTION
		 *   Returns the ndef child, with index i.
		 * 	 In case the child doesn't exist, null is returned.
		 *
		 **/
		public Ndef get_child(int  i)
		{
			if ((i <= this.count_children()) && (i>=0))
			{
				return child[i];
			} else
			{
				return null;
			}
		}
		
		/**m* SpringCard.NFC/NfcTlv.count_children
		 *
		 * SYNOPSIS
		 *   public int count_children()
		 * 
		 * DESCRIPTION
		 *   Returns the number of children that the NfcTlv has.
		 *
		 **/
		public int count_children()
		{
			return child.Length;
		}

	}
	
}
