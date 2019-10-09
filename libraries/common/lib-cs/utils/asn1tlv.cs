/**
 *
 * \ingroup LibCs
 *
 * \copyright
 *   Copyright (c) 2008-2018 SpringCard - www.springcard.com
 *   All right reserved
 *
 * \author
 *   Johann.D et al. / SpringCard
 *
 */
/*
 * Read LICENSE.txt for license details and restrictions.
 */

namespace SpringCard.LibCs
{
	/**
	 * \brief ASN.1 Tag,Length,Value (TLV) object
	 **/
	public class Asn1Tlv
	{
		private byte[] _tag;
		private byte[] _value;

		/**
		 * \brief Create an empty TLV
		 **/
		public Asn1Tlv()
		{
			_tag = null;
			_value = null;
		}

		private void set_tag(byte[] tag)
		{
			_tag = tag;
		}

		private void set_tag(ushort tag)
		{
			if ((tag & 0xFF00) == 0x0000)
			{
				_tag = new byte[1];
				_tag[0] = (byte)tag;
			}
			else
			{
				_tag = new byte[2];
				_tag[0] = (byte)(tag >> 8);
				_tag[1] = (byte)(tag);
			}
		}

		/**
		 * \brief Create a TLV with given Tag and Value (Length set to value.Length)
		 **/
		public Asn1Tlv(byte[] tag, byte[] value)
		{
			set_tag(tag);
			_value = value;
		}

		/**
		 * \brief Create a TLV with given Tag and Value (Length set to value.Length)
		 **/
		public Asn1Tlv(ushort tag, byte[] value)
		{
			set_tag(tag);
			_value = value;
		}

		public Asn1Tlv(ushort tag, params Asn1Tlv[] children)
		{
			set_tag(tag);
			byte[] value = new byte[0];
			for (int i=0; i<children.Length; i++)
			{
				value = BinUtils.Concat(value, children[i].Serialize());
			}
			_value = value;
		}

		private Asn1Tlv[] child = new Asn1Tlv[0];

		/**
		 * \brief Retrieve the Tag (T) field as an array of bytes
		 **/
		public byte[] T
		{
			get
			{
				return _tag;
			}
		}

		/**
		 * \brief Retrieve the length (in bytes) of the Tag (T) field
		 **/
		public int T_Length
		{
			get
			{
				if ((_tag == null) || (_tag.Length == 0)) return 1;
				if ((_tag[0] & 0x1F) != 0x1F) return 1;

				int result = 2;
				int i = 1;
				while ((i < _tag.Length) && ((_tag[i] & 0x80) != 0))
				{
					result++;
					i++;
				}

				return result;
			}
		}

		/**
		 * \brief Return true for a one-byte Tag (field T fits in 1 byte)
		 **/
		public bool T_IsByte
		{
			get
			{
				return (T_Length == 1);
			}
		}

		/**
		 * \brief Get/Set the Tag (T) field in case it fits in 1 byte
		 **/
		public byte T_AsByte
		{
			get
			{
				if ((_tag == null) || (_tag.Length == 0)) return 0x00;
				return _tag[0];
			}
			set
			{
				_tag = new byte[1];
				_tag[0] = value;
			}
		}

		/**
		 * \brief Return true for a two-byte Tag (field T fits in 2 bytes)
		 **/
		public bool T_IsWord
		{
			get
			{
				return (T_Length == 2);
			}
		}

		/**
		 * \brief Get/Set the Tag (T) field in case it fits in 2 bytes
		 **/
		public ushort T_AsWord
		{
			get
			{
				if ((_tag == null) || (_tag.Length == 0)) return 0x0000;
				if (_tag.Length == 1) return _tag[0];
				return (ushort)(_tag[0] << 8 | _tag[1]);
			}
			set
			{
				if ((value & 0xFF00) == 0x0000)
				{
					T_AsByte = (byte)value;
				}
				else
				{
					_tag = new byte[2];
					_tag[0] = (byte)(value >> 8);
					_tag[1] = (byte)(value);
				}
			}
		}

		/**
		 * \brief Retrieve the Length (L) field, i.e. the length (in bytes) of the Value (V) field
		 **/
		public long L
		{
			get
			{
				if (_value == null)
					return 0;
				return _value.Length;
			}
		}

		/**
		 * \brief Retrieve the length (in bytes) of the Length (L) field
		 **/
		public int L_Length
		{
			get
			{
				long l = L;

				if (l <= 0x7F) return 1;
				if (l <= 0xFF) return 2;
				if (l <= 0xFFFF) return 3;
				if (l <= 0xFFFFFF) return 4;
				return 5;
			}
		}

		/**
		 * \brief Retrieve the Value (V) field
		 **/
		public byte[] V
		{
			get
			{
				return _value;
			}
		}

		/**
		 * \brief In case the Value (V) field is a TLV itself, retrieve the i-th child
		 **/
		public Asn1Tlv GetChild(int i)
		{
			if ((i <= this.count_children()) && (i >= 0))
			{
				return child[i];
			}
			else
			{
				return null;
			}
		}

		public Asn1Tlv get_child(int i)
		{
			return GetChild(i);
		}

		/**
		 * \brief In case the Value (V) field is a TLV itself, retrieve the number of childre
		 **/
		public int GetChildrenCount()
		{
			return child.Length;
		}

		public int count_children()
		{
			return GetChildrenCount();
		}

		/**
		 * \brief Serialize the TLV into an array of bytes
		 **/
		public byte[] Serialize()
		{
			byte[] result = new byte[T_Length + L_Length + L];

			long offset = 0;

			for (int i = 0; i < T_Length; i++)
			{
				if ((_tag != null) && (i < _tag.Length))
					result[offset++] = _tag[i];
				else
					result[offset++] = 0;
			}

			long length = L;
			int length_of_length = L_Length;
			switch (length_of_length)
			{
				case 1:
					result[offset++] = (byte)length;
					break;
				case 2:
					result[offset++] = 0x81;
					result[offset++] = (byte)length;
					break;
				case 3:
					result[offset++] = 0x82;
					result[offset++] = (byte)(length >> 8);
					result[offset++] = (byte)length;
					break;
				case 4:
					result[offset++] = 0x83;
					result[offset++] = (byte)(length >> 16);
					result[offset++] = (byte)(length >> 8);
					result[offset++] = (byte)length;
					break;
				case 5:
					result[offset++] = 0x84;
					result[offset++] = (byte)(length >> 24);
					result[offset++] = (byte)(length >> 16);
					result[offset++] = (byte)(length >> 8);
					result[offset++] = (byte)length;
					break;
			}

			for (long i = 0; i < length; i++)
			{
				result[offset++] = _value[i];
			}

			return result;
		}

		/**
		 * \brief De-serialize an array of bytes into a TLV
		 **/
		public static Asn1Tlv Deserialize(byte[] buffer)
		{
			byte[] dummy;
			return Deserialize(buffer, out dummy);
		}
        public static Asn1Tlv Unserialize(byte[] buffer)
        {
            return Deserialize(buffer);
        }

        /**
		 * \brief De-serialize an array of bytes into a TLV
		 **/
        public static Asn1Tlv Deserialize(byte[] buffer, out byte[] remaining)
		{
			long offset = 0;

			remaining = null;

			if (buffer == null)
				return null;
			if (buffer.Length < 1)
				return null;

			int length_of_tag = 1;
			if ((buffer[0] & 0x1F) == 0x1F)
			{
				/* Long tag */
				while ((offset < buffer.Length) && ((buffer[offset] & 0x80) != 0x00))
				{
					offset++;
					length_of_tag++;
				}
			}
			offset++;

			if (offset >= buffer.Length)
				return null;

			byte[] tag = new byte[length_of_tag];
			for (int i = 0; i < length_of_tag; i++)
				tag[i] = buffer[i];

			int length_of_length = 1;
			long length = 0;
			if ((buffer[offset] & 0x80) != 0x00)
			{
				/* Long length */
				length_of_length = buffer[offset] & 0x7F;
				if ((length_of_length == 0) || (length_of_length > 5))
					return null;
				offset++;
				length_of_length--;

				for (int i = 0; i < length_of_length; i++)
				{
					if (offset >= buffer.Length)
						return null;
					length <<= 8;
					length |= buffer[offset++];
				}
			}
			else
			{
				length = buffer[offset++];
			}

			if ((offset + length) > buffer.Length)
			{
				return null;
			}

			byte[] value = null;
			if (length > 0)
			{
				value = new byte[length];
				for (long i = 0; i < length; i++)
					value[i] = buffer[offset + i];
			}

			offset += length;

			if (offset < buffer.Length)
			{
				remaining = new byte[buffer.Length - offset];
				for (long i = 0; i < remaining.Length; i++)
					remaining[i] = buffer[offset + i];
			}

			return new Asn1Tlv(tag, value);
		}
        public static Asn1Tlv Unserialize(byte[] buffer, out byte[] remaining)
        {
            return Deserialize(buffer, out remaining);
        }


    }

}
