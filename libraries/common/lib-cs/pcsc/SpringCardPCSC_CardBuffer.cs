/**
 *
 * \ingroup PCSC 
 *
 * \copyright
 *   Copyright (c) 2010-2018 SpringCard - www.springcard.com
 *   All right reserved
 *
 * \author
 *   Johann.D and Emilie.C / SpringCard 
 *
 */
/*
 *	This software is part of the SPRINGCARD SDK FOR PC/SC
 *
 *   Redistribution and use in source (source code) and binary
 *   (object code) forms, with or without modification, are
 *   permitted provided that the following conditions are met :
 *
 *   1. Redistributed source code or object code shall be used
 *   only in conjunction with products (hardware devices) either
 *   manufactured, distributed or developed by SPRINGCARD,
 *
 *   2. Redistributed source code, either modified or
 *   un-modified, must retain the above copyright notice,
 *   this list of conditions and the disclaimer below,
 *
 *   3. Redistribution of any modified code must be clearly
 *   identified "Code derived from original SPRINGCARD 
 *   copyrighted source code", with a description of the
 *   modification and the name of its author,
 *
 *   4. Redistributed object code must reproduce the above
 *   copyright notice, this list of conditions and the
 *   disclaimer below in the documentation and/or other
 *   materials provided with the distribution,
 *
 *   5. The name of SPRINGCARD may not be used to endorse
 *   or promote products derived from this software or in any
 *   other form without specific prior written permission from
 *   SPRINGCARD.
 *
 *   THIS SOFTWARE IS PROVIDED BY SPRINGCARD "AS IS".
 *   SPRINGCARD SHALL NOT BE LIABLE FOR INFRINGEMENTS OF THIRD
 *   PARTIES RIGHTS BASED ON THIS SOFTWARE.
 *
 *   ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
 *   LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
 *   FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
 *
 *   SPRINGCARD DOES NOT WARRANT THAT THE FUNCTIONS CONTAINED IN
 *   THIS SOFTWARE WILL MEET THE USER'S REQUIREMENTS OR THAT THE
 *   OPERATION OF IT WILL BE UNINTERRUPTED OR ERROR-FREE.
 *
 *   IN NO EVENT SHALL SPRINGCARD BE LIABLE FOR ANY DIRECT,
 *   INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
 *   DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
 *   SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
 *   OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
 *   LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 *   (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF
 *   THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY
 *   OF SUCH DAMAGE. 
 *
 **/
using System;
using SpringCard.LibCs;

namespace SpringCard.PCSC
{
	#region CardBuffer class

	/**
	 *
	 * \brief The CardBuffer object eases the manipulation of byte arrays
	 *
	 **/
	public class CardBuffer
	{
		public byte[] Bytes;

		private bool isb(char c)
		{
			bool r = false;

			if ((c >= '0') && (c <= '9'))
			{
				r = true;
			} else if ((c >= 'A') && (c <= 'F'))
			{
				r = true;
			} else if ((c >= 'a') && (c <= 'f'))
			{
				r = true;
			}

			return r;
		}

		private byte htob(char c)
		{
			int r = 0;

			if ((c >= '0') && (c <= '9'))
			{
				r = c - '0';
			} else if ((c >= 'A') && (c <= 'F'))
			{
				r = c - 'A';
				r += 10;
			} else if ((c >= 'a') && (c <= 'f'))
			{
				r = c - 'a';
				r += 10;
			}

			return (byte) r;
		}

		public CardBuffer()
		{

		}


		public CardBuffer(byte b)
		{
			Bytes = new byte[1];
			Bytes[0] = b;
		}

		public CardBuffer(ushort w)
		{
			Bytes = new byte[2];
			Bytes[0] = (byte) (w / 0x0100);
			Bytes[1] = (byte) (w % 0x0100);
		}

		public CardBuffer(byte[]bytes)
		{
			Bytes = bytes;
		}

		public CardBuffer(byte[]bytes, long length)
		{
			SetBytes(bytes, length);
		}

		public CardBuffer(byte[]bytes, long offset, long length)
		{
			SetBytes(bytes, offset, length);
		}

		public CardBuffer(string str)
		{
			SetString(str);
		}

		public byte this[long offset]
		{
			get
			{
				return GetByte(offset);
			}
		}

		public byte GetByte(long offset)
		{
			if (Bytes == null)
				return 0;

			if (offset >= Bytes.Length)
				offset = 0;

			return Bytes[offset];
		}

		public byte[] GetBytes()
		{
			return Bytes;
		}

		public byte[] GetBytes(long length)
		{
			if (Bytes == null)
				return null;

			if (length < 0)
				length = Bytes.Length - length;

			if (length > Bytes.Length)
				length = Bytes.Length;

			byte[] r = new byte[length];
			for (long i=0; i<length; i++)
				r[i] = Bytes[i];

			return r;
		}

		public byte[] GetBytes(long offset, long length)
		{
			if (Bytes == null)
				return null;

			if (offset < 0)
				offset = Bytes.Length - offset;

			if (offset >= Bytes.Length)
				return null;
			
			if (length < 0)
				length = Bytes.Length - length;
			
			byte[] r = new byte[length];
			for (long i=0; i<length; i++)
			{
				if (offset >= Bytes.Length) break;
				r[i] = Bytes[offset++];
			}

			return r;
		}

		public char[] GetChars(long offset, long length)
		{
			byte[] b = GetBytes(offset, length);

			if (b == null) return null;

			char[] c = new char[b.Length];
			for (long i=0; i<b.Length; i++)
				c[i] = (char) b[i];

			return c;
		}

		public void SetBytes(byte[]bytes)
		{
			Bytes = bytes;
		}

		public void SetBytes(byte[]bytes, long length)
		{
			Bytes = new byte[length];

			long i;

			for (i = 0; i < length; i++)
				Bytes[i] = bytes[i];
		}

		public void SetBytes(byte[]bytes, long offset, long length)
		{
			Bytes = new byte[length];

			long i;

			for (i = 0; i < length; i++)
				Bytes[i] = bytes[offset + i];
		}

		public void Append(byte[]bytes)
		{
			if ((bytes == null) || (bytes.Length == 0))
				return;

			byte[] old_bytes = GetBytes();

			if ((old_bytes == null) || (old_bytes.Length == 0))
			{
				SetBytes(bytes);
				return;
			}

			Bytes = new byte[old_bytes.Length + bytes.Length];

			for (long i=0; i<old_bytes.Length; i++)
				Bytes[i] = old_bytes[i];
			for (long i=0; i<bytes.Length; i++)
				Bytes[old_bytes.Length+i] = bytes[i];

		}

		public void AppendOne(byte b)
		{			
			byte[] old_bytes = GetBytes();

			if ((old_bytes == null) || (old_bytes.Length == 0))
			{
				Bytes = new byte[1];
				Bytes[0] = b;
				return;
			}

			Bytes = new byte[old_bytes.Length + 1];

			for (long i=0; i<old_bytes.Length; i++)
				Bytes[i] = old_bytes[i];

			Bytes[old_bytes.Length] = b;
		}

		public bool StartsWith(byte[] value)
		{
			for (int i=0; i<value.Length; i++)
			{
				if (i >= Bytes.Length)
					break;
				if (value[i] != Bytes[i])
					return false;
			}
			return true;
		}

		public bool StartsWith(CardBuffer value)
		{
			return StartsWith(value.Bytes);
		}

		public void SetString(string str)
		{
			string s = "";
			int i, l;

			if (str == null)
			  l = 0;
			else
			  l = str.Length;

			for (i = 0; i < l; i++)
			{
				char c = str[i];

				if (isb(c))
					s = s + c;
			}

			l = s.Length;
			Bytes = new Byte[l / 2];

			for (i = 0; i < l; i += 2)
			{
				Bytes[i / 2] = htob(s[i]);
				Bytes[i / 2] *= 0x10;
				Bytes[i / 2] += htob(s[i + 1]);
			}
		}

		public string GetString()
		{
			string s = "";
			long i;

			if (Bytes != null)
			{
				for (i = 0; i < Bytes.Length; i++)
				{
					s = s + (char) Bytes[i];
				}
			}

			return s;
		}

		public virtual string AsString(string separator)
		{
			string s = "";
			long i;

			if (Bytes != null)
			{
				for (i = 0; i < Bytes.Length; i++)
				{
					if (i > 0)
						s = s + separator;
					s = s + String.Format("{0:X02}", Bytes[i]);
				}
			}

			return s;
		}

		public virtual string AsString()
		{
			return AsString("");
		}

        public override string ToString()
        {
            return AsString("");
        }

        public int Length
		{
			get
			{
				if (Bytes == null)
					return 0;
				return Bytes.Length;
			}
		}

		public static byte[] BytesFromString(string s)
		{
			byte[] r = new byte[s.Length];
			for (int i=0; i<r.Length; i++)
			{
				char c = s[i];
				r[i] = (byte) (c & 0x7F);
			}
			return r;
		}

		public static string StringFromBytes(byte[] a)
		{
			string r = "";
			if (a != null)
			{
				for (int i=0; i<a.Length; i++)
				{
					char c = (char) a[i];
					r += c;
				}
			}
			return r;
		}
	}
	#endregion

	#region CAPDU class

	/**
	 *
	 * \brief The CAPDU object is used to format and send COMMAND APDUs (according to ISO 7816-4) to the smartcard
	 *
	 **/
	public class CAPDU : CardBuffer
	{
		public CAPDU()
		{

		}

		/**
		 * Create a CAPDU from the given arry of bytes
		 */
		public CAPDU(byte[]bytes)
		{
			Bytes = bytes;
		}

		/**
		 * Create a CAPDU with only the CLA, INS, P1, P2 header
		 */

		public CAPDU(byte CLA, byte INS, byte P1, byte P2)
		{
			Bytes = new byte[4];
			Bytes[0] = CLA;
			Bytes[1] = INS;
			Bytes[2] = P1;
			Bytes[3] = P2;
		}

		/**
		 * Create a CAPDU with only the CLA, INS, P1, P2 header and a P3 (Le) entry
		 */

		public CAPDU(byte CLA, byte INS, byte P1, byte P2, byte P3)
		{
			Bytes = new byte[5];
			Bytes[0] = CLA;
			Bytes[1] = INS;
			Bytes[2] = P1;
			Bytes[3] = P2;
			Bytes[4] = P3;
		}

		/**
		 * Create a CAPDU with the CLA, INS, P1, P2 header and some data
		 */

		public CAPDU(byte CLA, byte INS, byte P1, byte P2, byte[] data)
		{
			int i;
			Bytes = new byte[5 + data.Length];
			Bytes[0] = CLA;
			Bytes[1] = INS;
			Bytes[2] = P1;
			Bytes[3] = P2;
			Bytes[4] = (byte) data.Length;
			for (i = 0; i < data.Length; i++)
				Bytes[5 + i] = data[i];
		}

		/**
		 * Create a CAPDU with the CLA, INS, P1, P2 header and some data. The data field is taken from a string.
		 */
		public CAPDU(byte CLA, byte INS, byte P1, byte P2, string data)
        {
            int i;
            byte[] _data = StrUtils.ToBytes(data);
			Bytes = new byte[5 + _data.Length];
			Bytes[0] = CLA;
			Bytes[1] = INS;
			Bytes[2] = P1;
			Bytes[3] = P2;
			Bytes[4] = (byte)_data.Length;
			for (i = 0; i < _data.Length; i++)
				Bytes[5 + i] = _data[i];
		}

		/**
		 * Create a CAPDU with the CLA, INS, P1, P2 header, some data and a LE.
		 */
		public CAPDU(byte CLA, byte INS, byte P1, byte P2, byte[] data, byte LE)
		{
			int i;
			Bytes = new byte[6 + data.Length];
			Bytes[0] = CLA;
			Bytes[1] = INS;
			Bytes[2] = P1;
			Bytes[3] = P2;
			Bytes[4] = (byte) data.Length;
			for (i = 0; i < data.Length; i++)
				Bytes[5 + i] = data[i];
			Bytes[5 + data.Length] = LE;
		}

		/**
		 * Create a CAPDU with the CLA, INS, P1, P2 header, some data and a LE. The data field is taken from a string.
		 */
		public CAPDU(byte CLA, byte INS, byte P1, byte P2, string data, byte LE)
		{
			int i;
            byte[] _data = StrUtils.ToBytes(data);
            Bytes = new byte[6 + _data.Length];
			Bytes[0] = CLA;
			Bytes[1] = INS;
			Bytes[2] = P1;
			Bytes[3] = P2;
			Bytes[4] = (byte)_data.Length;
			for (i = 0; i < _data.Length; i++)
				Bytes[5 + i] = _data[i];
			Bytes[5 + _data.Length] = LE;
		}

		public CAPDU(string str)
		{
			SetString(str);
		}

		public byte CLA
		{
			get
			{
				if (Bytes == null)
					return 0xFF;
				return Bytes[0];
			}
			set
			{
				if (Bytes == null)
					Bytes = new byte[4];
				Bytes[0] = value;
			}
		}

		public byte INS
		{
			get
			{
				if (Bytes == null)
					return 0xFF;
				return Bytes[1];
			}
			set
			{
				if (Bytes == null)
					Bytes = new byte[4];
				Bytes[1] = value;
			}
		}

		public byte P1
		{
			get
			{
				if (Bytes == null)
					return 0xFF;
				return Bytes[2];
			}
			set
			{
				if (Bytes == null)
					Bytes = new byte[4];
				Bytes[2] = value;
			}
		}

		public byte P2
		{
			get
			{
				if (Bytes == null)
					return 0xFF;
				return Bytes[3];
			}
			set
			{
				if (Bytes == null)
					Bytes = new byte[4];
				Bytes[3] = value;
			}
		}

		private bool hasLc()
		{
			if (!Valid())
				return false;
			if (Bytes.Length <= 5)
				return false;
			return true;
		}

		private bool hasLe()
		{
			if (!Valid())
				return false;
			if (Bytes.Length == 6 + Bytes[4])
				return false;
			return true;
		}

		public bool Valid()
		{
			if (Bytes == null)
				return false;
			if (Bytes.Length <= 4)
				return false;
			if (Bytes.Length == 5)
				return true;
			if (Bytes.Length == 5 + Bytes[4])
				return true;
			if (Bytes.Length == 6 + Bytes[4])
				return true;
			return false;
		}

		public byte Lc
		{
			get
			{
				if (!hasLc())
					return 0x00;

				return Bytes[4];
			}
		}

		public byte Le
		{
			get
			{
				if (!hasLe())
					return 0x00;
				return Bytes[Bytes.Length - 1];
			}

			set
			{
				if (Bytes == null)
					Bytes = new byte[5];
				if (!hasLe())
				{
					byte[] t = new byte[Bytes.Length+1];
					for (int i=0; i<Bytes.Length; i++)
						t[i] = Bytes[i];
					Bytes = t;
				}
				Bytes[Bytes.Length-1] = value;
			}
		}

		public CardBuffer data
		{
			get
			{
				byte[] t = DataBytes;

				if (t == null)
				{
					return null;
				}
				else
				{
					return new CardBuffer(t);
				}
			}

			set
			{
				if (value == null)
				{
					DataBytes = null;
				}
				else
				{
					DataBytes = value.GetBytes();
				}
			}
		}

		public byte[] DataBytes
		{
			get
			{
				if (!hasLc())
					return null;

				byte[] t = new byte[Lc];
				for (int i=0; i<t.Length; i++)
					t[i] = Bytes[5+i];

				return t;
			}

			set
			{
				int length;
				uint apdu_size;

				if (value == null)
					length = 0;
				else
					length = value.Length;

				if (length == 0)
				{

				} else
					if (length < 256)
				{
					if (hasLe())
						apdu_size = (uint) (6 + length);
					else
						apdu_size = (uint) (5 + length);

					byte[] t = new byte[apdu_size];

					if (Valid())
					{
						for (int i=0; i<4; i++)
							t[i] = Bytes[i];
						if (hasLe())
							t[t.Length-1] = Bytes[Bytes.Length-1];
					}

					for (int i=0; i<length; i++)
						t[5+i] = value[i];

					t[4] = (byte) length;

					Bytes = t;
				} else
				{
					/* Oups ? */
				}
			}
		}

        public override string AsString()
        {
            return AsString("");
        }

        public override string ToString()
        {
            return AsString();
        }

    }
    #endregion

    #region RAPDU class

    /**
	 *
	 * \brief The RAPDU object is used to receive and decode RESPONSE APDUs (according to ISO 7816-4) from the smartcard
	 *
	 **/
    public class RAPDU : CardBuffer
	{
		public bool isValid
		{
			get
			{
				return (Bytes.Length >= 2);
			}
		}

		public RAPDU(CardBuffer buffer)
		{
			SetBytes(buffer.GetBytes());
		}

		public RAPDU(byte[]bytes, int length)
		{
			SetBytes(bytes, length);
		}

		public RAPDU(byte[]bytes)
		{
			SetBytes(bytes);
		}

		public RAPDU(byte[]bytes, byte SW1, byte SW2)
		{
			byte[] t;
			if (bytes == null)
			{
				t = new byte[2];
				t[0] = SW1;
				t[1] = SW2;
			} else
			{
				t = new byte[bytes.Length + 2];
				for (int i=0; i<bytes.Length; i++)
					t[i] = bytes[i];
				t[bytes.Length] = SW1;
				t[bytes.Length+1] = SW2;
			}
			SetBytes(t);
		}

		public RAPDU(string data, byte sw1, byte sw2)
		{
			SetString(data);
			AppendOne(sw1);
			AppendOne(sw2);
		}

		public RAPDU(byte sw1, byte sw2)
		{
			byte[] t = new byte[2];
			t[0] = sw1;
			t[1] = sw2;
			SetBytes(t);
		}

		public RAPDU(ushort sw)
		{
			byte[] t = new byte[2];
			t[0] = (byte) (sw / 0x0100);
			t[1] = (byte) (sw % 0x0100);
			SetBytes(t);
		}

		public bool hasData
		{
			get
			{
				if ((Bytes == null) || (Bytes.Length < 2))
					return false;

				return true;
			}
		}

		public CardBuffer data
		{
			get
			{
				if ((Bytes == null) || (Bytes.Length < 2))
					return null;

				return new CardBuffer(Bytes, Bytes.Length - 2);
			}
		}

        public byte[] DataBytes
        {
            get
            {
                if ((Bytes == null) || (Bytes.Length < 2))
                    return new byte[0];

                return new CardBuffer(Bytes, Bytes.Length - 2).Bytes;
            }
        }

        public byte SW1
		{
			get
			{
				if ((Bytes == null) || (Bytes.Length < 2))
					return 0xCC;
				return Bytes[Bytes.Length - 2];
			}
		}

		public byte SW2
		{
			get
			{
				if ((Bytes == null) || (Bytes.Length < 2))
					return 0xCC;
				return Bytes[Bytes.Length - 1];
			}
		}

		public ushort SW
		{
			get
			{
				if ((Bytes == null) || (Bytes.Length < 2))
					return 0xCCCC;

				ushort r;

				r = Bytes[Bytes.Length - 2];
				r *= 0x0100;
				r += Bytes[Bytes.Length - 1];

				return r;
			}
		}

		public string SWString
		{
			get
			{
				return SCARD.CardStatusWordsToString(SW1, SW2);
			}
		}

        public override string AsString()
        {
            return AsString("");
        }

        public override string ToString()
        {
            return AsString();
        }

    }
    #endregion
}
