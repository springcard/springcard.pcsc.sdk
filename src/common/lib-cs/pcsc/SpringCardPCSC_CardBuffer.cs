/**h* SpringCard/PCSC_CardBuffer
 *
 * NAME
 *   PCSC : PCSC_CardBuffer
 * 
 * DESCRIPTION
 *   SpringCard's wrapper for PC/SC API
 *
 * COPYRIGHT
 *   Copyright (c) 2010-2015 SpringCard - www.springcard.com
 *
 * AUTHOR
 *   Johann.D and Emilie.C / SpringCard
 *
 **/
using System;
using System.Collections.ObjectModel;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;

namespace SpringCard.PCSC
{
	

	#region CardBuffer class

	/**c* SpringCardPCSC/CardBuffer
	 *
	 * NAME
	 *   CardBuffer
	 * 
	 * DESCRIPTION
	 *   The CardBuffer provides convenient access to byte-arrays
	 * 
	 * DERIVED BY
	 *   CAPDU
	 *   RAPDU
	 *
	 **/

	public class CardBuffer
	{
		protected byte[] _bytes = null;

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
			_bytes = new byte[1];
			_bytes[0] = b;
		}

		public CardBuffer(ushort w)
		{
			_bytes = new byte[2];
			_bytes[0] = (byte) (w / 0x0100);
			_bytes[1] = (byte) (w % 0x0100);
		}

		public CardBuffer(byte[]bytes)
		{
			_bytes = bytes;
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
			if (_bytes == null)
				return 0;
			
			if (offset >= _bytes.Length)
				offset = 0;

			return _bytes[offset];
		}

		public byte[] GetBytes()
		{
			return _bytes;
		}

		public byte[] GetBytes(long length)
		{
			if (_bytes == null)
				return null;
			
			if (length < 0)
				length = _bytes.Length - length;
			
			if (length > _bytes.Length)
				length = _bytes.Length;
			
			byte[] r = new byte[length];
			for (long i=0; i<length; i++)
				r[i] = _bytes[i];
			
			return r;
		}

		public byte[] GetBytes(long offset, long length)
		{
			if (_bytes == null)
				return null;
			
			if (offset < 0)
				offset = _bytes.Length - offset;
			
			if (length < 0)
				length = _bytes.Length - length;

			if (offset >= _bytes.Length)
				return null;

			if (length > (_bytes.Length - offset))
				length = _bytes.Length - offset;
			
			byte[] r = new byte[length];
			for (long i=0; i<length; i++)
				r[i] = _bytes[offset+i];
			
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
			_bytes = bytes;
		}

		public void SetBytes(byte[]bytes, long length)
		{
			_bytes = new byte[length];

			long i;

			for (i = 0; i < length; i++)
				_bytes[i] = bytes[i];
		}

		public void SetBytes(byte[]bytes, long offset, long length)
		{
			_bytes = new byte[length];

			long i;

			for (i = 0; i < length; i++)
				_bytes[i] = bytes[offset + i];
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
			
			_bytes = new byte[old_bytes.Length + bytes.Length];
			
			for (long i=0; i<old_bytes.Length; i++)
				_bytes[i] = old_bytes[i];
			for (long i=0; i<bytes.Length; i++)
				_bytes[old_bytes.Length+i] = bytes[i];

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
			_bytes = new Byte[l / 2];

			for (i = 0; i < l; i += 2)
			{
				_bytes[i / 2] = htob(s[i]);
				_bytes[i / 2] *= 0x10;
				_bytes[i / 2] += htob(s[i + 1]);
			}
		}
		
		public string GetString()
		{
			string s = "";
			long i;

			if (_bytes != null)
			{
				for (i = 0; i < _bytes.Length; i++)
				{
					s = s + (char) _bytes[i];
				}
			}

			return s;			
		}

		public virtual string AsString(string separator)
		{
			string s = "";
			long i;

			if (_bytes != null)
			{
				for (i = 0; i < _bytes.Length; i++)
				{
					if (i > 0)
						s = s + separator;
					s = s + String.Format("{0:X02}", _bytes[i]);
				}
			}

			return s;
		}

		public virtual string AsString()
		{
			return AsString("");
		}

		protected byte[] Bytes
		{
			get
			{
				return _bytes;
			}
			set
			{
				_bytes = value;
			}
		}

		public int Length
		{
			get
			{
				if (_bytes == null)
					return 0;
				return _bytes.Length;
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

	/**c* SpringCardPCSC/CAPDU
	 *
	 * NAME
	 *   CAPDU
	 * 
	 * DESCRIPTION
	 *   The CAPDU object is used to format and send COMMAND APDUs (according to ISO 7816-4) to the smartcard
	 * 
	 * DERIVED FROM
	 *   CardBuffer
	 *
	 **/

	public class CAPDU:CardBuffer
	{
		public CAPDU()
		{

		}

		public CAPDU(byte[]bytes)
		{
			_bytes = bytes;
		}

		public CAPDU(byte CLA, byte INS, byte P1, byte P2)
		{
			_bytes = new byte[4];
			_bytes[0] = CLA;
			_bytes[1] = INS;
			_bytes[2] = P1;
			_bytes[3] = P2;
		}

		public CAPDU(byte CLA, byte INS, byte P1, byte P2, byte P3)
		{
			_bytes = new byte[5];
			_bytes[0] = CLA;
			_bytes[1] = INS;
			_bytes[2] = P1;
			_bytes[3] = P2;
			_bytes[4] = P3;
		}

		public CAPDU(byte CLA, byte INS, byte P1, byte P2, byte[] data)
		{
			int i;
			_bytes = new byte[5 + data.Length];
			_bytes[0] = CLA;
			_bytes[1] = INS;
			_bytes[2] = P1;
			_bytes[3] = P2;
			_bytes[4] = (byte) data.Length;
			for (i = 0; i < data.Length; i++)
				_bytes[5 + i] = data[i];
		}

		public CAPDU(byte CLA, byte INS, byte P1, byte P2, string data)
		{
			int i;
			byte[] _data = (new CardBuffer(data)).GetBytes();
			_bytes = new byte[5 + _data.Length];
			_bytes[0] = CLA;
			_bytes[1] = INS;
			_bytes[2] = P1;
			_bytes[3] = P2;
			_bytes[4] = (byte)_data.Length;
			for (i = 0; i < _data.Length; i++)
				_bytes[5 + i] = _data[i];
		}

		public CAPDU(byte CLA, byte INS, byte P1, byte P2, byte[] data, byte LE)
		{
			int i;
			_bytes = new byte[6 + data.Length];
			_bytes[0] = CLA;
			_bytes[1] = INS;
			_bytes[2] = P1;
			_bytes[3] = P2;
			_bytes[4] = (byte) data.Length;
			for (i = 0; i < data.Length; i++)
				_bytes[5 + i] = data[i];
			_bytes[5 + data.Length] = LE;
		}

		public CAPDU(byte CLA, byte INS, byte P1, byte P2, string data, byte LE)
		{
			int i;
			byte[] _data = (new CardBuffer(data)).GetBytes();
			_bytes = new byte[6 + _data.Length];
			_bytes[0] = CLA;
			_bytes[1] = INS;
			_bytes[2] = P1;
			_bytes[3] = P2;
			_bytes[4] = (byte)_data.Length;
			for (i = 0; i < _data.Length; i++)
				_bytes[5 + i] = _data[i];
			_bytes[5 + _data.Length] = LE;
		}

		public CAPDU(string str)
		{
			SetString(str);
		}

		public byte CLA
		{
			get
			{
				if (_bytes == null)
					return 0xFF;
				return _bytes[0];
			}
			set
			{
				if (_bytes == null)
					_bytes = new byte[4];
				_bytes[0] = value;
			}
		}

		public byte INS
		{
			get
			{
				if (_bytes == null)
					return 0xFF;
				return _bytes[1];
			}
			set
			{
				if (_bytes == null)
					_bytes = new byte[4];
				_bytes[1] = value;
			}
		}

		public byte P1
		{
			get
			{
				if (_bytes == null)
					return 0xFF;
				return _bytes[2];
			}
			set
			{
				if (_bytes == null)
					_bytes = new byte[4];
				_bytes[2] = value;
			}
		}

		public byte P2
		{
			get
			{
				if (_bytes == null)
					return 0xFF;
				return _bytes[3];
			}
			set
			{
				if (_bytes == null)
					_bytes = new byte[4];
				_bytes[3] = value;
			}
		}
		
		private bool hasLc()
		{
			if (!Valid())
				return false;
			if (_bytes.Length <= 5)
				return false;
			return true;
		}

		private bool hasLe()
		{
			if (!Valid())
				return false;
			if (_bytes.Length == 6 + _bytes[4])
				return false;
			return true;
		}
		
		public bool Valid()
		{
			if (_bytes == null)
				return false;
			if (_bytes.Length <= 4)
				return false;
			if (_bytes.Length == 5)
				return true;
			if (_bytes.Length == 5 + _bytes[4])
				return true;
			if (_bytes.Length == 6 + _bytes[4])
				return true;
			return false;
		}

		public byte Lc
		{
			get
			{
				if (!hasLc())
					return 0x00;

				return _bytes[4];
			}
		}

		public byte Le
		{
			get
			{
				if (!hasLe())
					return 0x00;
				return _bytes[_bytes.Length - 1];
			}

			set
			{
				if (_bytes == null)
					_bytes = new byte[5];
				if (!hasLe())
				{
					byte[] t = new byte[_bytes.Length+1];
					for (int i=0; i<_bytes.Length; i++)
						t[i] = _bytes[i];
					_bytes = t;
				}
				_bytes[_bytes.Length-1] = value;
			}
		}

		public CardBuffer data
		{
			get
			{
				if (!hasLc())
					return null;
				
				byte[] t = new byte[Lc];
				for (int i=0; i<t.Length; i++)
					t[i] = _bytes[5+i];
				
				return new CardBuffer(t);
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
							t[i] = _bytes[i];
						if (hasLe())
							t[t.Length-1] = _bytes[_bytes.Length-1];
					}
					
					for (int i=0; i<length; i++)
						t[5+i] = value.GetByte(i);
					
					t[4] = (byte) length;
					
					_bytes = t;
				} else
				{
					/* Oups ? */
				}
			}
		}
	}
	#endregion

	#region RAPDU class

	/**c* SpringCardPCSC/RAPDU
	 *
	 * NAME
	 *   RAPDU
	 * 
	 * DESCRIPTION
	 *   The RAPDU object is used to receive and decode RESPONSE APDUs (according to ISO 7816-4) from the smartcard
	 * 
	 * DERIVED FROM
	 *   CardBuffer
	 *
	 **/

	public class RAPDU:CardBuffer
	{
		public bool isValid
		{
			get
			{
				return (_bytes.Length >= 2);
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
				if ((_bytes == null) || (_bytes.Length < 2))
					return false;
				
				return true;
			}
		}

		public CardBuffer data
		{
			get
			{
				if ((_bytes == null) || (_bytes.Length < 2))
					return null;

				return new CardBuffer(_bytes, _bytes.Length - 2);
			}
		}

		public byte SW1
		{
			get
			{
				if ((_bytes == null) || (_bytes.Length < 2))
					return 0xCC;
				return _bytes[_bytes.Length - 2];
			}
		}

		public byte SW2
		{
			get
			{
				if ((_bytes == null) || (_bytes.Length < 2))
					return 0xCC;
				return _bytes[_bytes.Length - 1];
			}
		}

		public ushort SW
		{
			get
			{
				if ((_bytes == null) || (_bytes.Length < 2))
					return 0xCCCC;

				ushort r;

				r = _bytes[_bytes.Length - 2];
				r *= 0x0100;
				r += _bytes[_bytes.Length - 1];

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
	}
	#endregion
}
