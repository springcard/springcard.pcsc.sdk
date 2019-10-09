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
using System;

namespace SpringCard.LibCs
{
	/**
	 * \brief Utilities to manipulate raw arrays and binary values
	 */
	public class BinUtils
	{
		/**
		 * \brief Create a copy of an array
		 */
		public static byte[] Copy(byte[] a, int offset = 0, int length = 0)
		{
			if (length == 0)
			{
				if (offset < 0)
				{
					length = Math.Abs(offset);
					offset = a.Length - length;
				}
				else if (offset > 0)
				{
					length = a.Length - offset;
				}
			}
            else if (length < 0)
            {
                length = Math.Abs(length);
                length = a.Length - length - offset;
            }

			byte[] r = new byte[length];
			Array.Copy(a, offset, r, 0, length);

			return r;
		}

		/**
		 * \brief Create a copy of an array. If ensureSize is true, resize the destination array if the source array was to small
		 */
		public static byte[] Copy(byte[] a, int offset, int length, bool ensureSize)
		{
			byte[] result = Copy(a, offset, length);

			if (ensureSize)
				result = EnsureSize(result, length, length);

			return result;
		}

		public static byte[] Copy(byte[] a, uint offset = 0, uint length = 0)
		{
			if (length == 0)
			{
                if (offset > a.Length)
                    length = 0;
                else
				    length = (uint)a.Length - offset;
			}

			byte[] r = new byte[length];
            if (length > 0)
			    Array.Copy(a, offset, r, 0, length);

			return r;
		}

        /**
		 * \brief Copy an array into another
		 */
        public static void CopyTo(byte[] dst, byte[] src)
        {
            Array.Copy(src, dst, src.Length);
        }

        /**
		 * \brief Copy an array into another
		 */
        public static void CopyTo(byte[] dst, byte[] src, int length)
        {
            Array.Copy(src, dst, length);
        }

        /**
		 * \brief Copy an array into another
		 */
        public static void CopyTo(byte[] dst, int dst_offset, byte[] src, int length)
        {
            Array.Copy(src, 0,  dst, dst_offset, length);
        }

        /**
		 * \brief Copy an array into another
		 */
        public static void CopyTo(byte[] dst, int dst_offset, byte[] src, int src_offset, int length)
        {
            Array.Copy(src, src_offset, dst, dst_offset, length);
        }

        /**
		 * \brief Concatenate two arrays
		 */
        public static byte[] Concat(byte[] a, byte[] b)
		{
			byte[] r = new byte[a.Length + b.Length];
			Array.Copy(a, 0, r, 0, a.Length);
			Array.Copy(b, 0, r, a.Length, b.Length);
			return r;
		}

        /**
		 * \brief Concatenate one byte after an array
		 */
        public static byte[] Concat(byte[] a, byte b)
        {
            byte[] r = new byte[a.Length + 1];
            Array.Copy(a, 0, r, 0, a.Length);
            r[a.Length] = b;
            return r;
        }

        /**
		 * \brief Concatenate one byte before an array
		 */
        public static byte[] Concat(byte a, byte[] b)
        {
            byte[] r = new byte[1 + b.Length];
            r[0] = a;
            Array.Copy(b, 0, r, 1, b.Length);
            return r;
        }

        /**
		 * \brief Check whether two arrays are equals
		 */
        public static bool Equals(byte[] a1, byte[] a2)
		{
			if ((a1 == null) && (a2 == null))
				return true;
			if ((a1 == null) || (a2 == null))
				return false;
			if (a1.Length != a2.Length)
				return false;
			for (int i = 0; i < a1.Length; i++)
			{
				if (a2[i] != a1[i])
					return false;
			}

			return true;
		}

		/**
		 * \brief Check whether two arrays are equals, at least until the specified length
		 */
		public static bool Equals(byte[] a1, byte[] a2, int length)
		{
			if ((a1 == null) && (a2 == null))
				return true;
			if ((a1 == null) || (a2 == null))
				return false;
			for (int i = 0; i < length; i++)
			{
				if (a1.Length < i)
					return false;
				if (a2.Length < i)
					return false;
				if (a2[i] != a1[i])
					return false;
			}
			return true;
		}

		/**
		 * \brief Check whether two arrays are equals, at least from the specified offsets and until the specified length
		 */
		public static bool Equals(byte[] a1, int offset1, byte[] a2, int offset2, int length)
		{
			if ((a1 == null) && (a2 == null))
				return true;
			if ((a1 == null) || (a2 == null))
				return false;
			for (int i = 0; i < length; i++)
			{
				if (a1.Length + offset1 < i)
					return false;
				if (a2.Length + offset2 < i)
					return false;
				if (a2[offset2 + i] != a1[offset1 + i])
					return false;
			}
			return true;
		}

        /**
		 * \brief Rotate an array by one byte to the left
		 */
        public static byte[] RotateLeftOneByte(byte[] buffer)
        {
            byte[] result = new byte[buffer.Length];

            for (int i = 1; i < buffer.Length; i++)
                result[i - 1] = buffer[i];
            result[buffer.Length - 1] = buffer[0];

            return result;
        }

        /**
		 * \brief Rotate an array by one byte to the right
		 */
        public static byte[] RotateRightOneByte(byte[] buffer)
        {
            byte[] result = new byte[buffer.Length];

            for (int i = buffer.Length - 1; i >= 1; i--)
                result[i] = buffer[i - 1];
            result[0] = buffer[buffer.Length - 1];

            return result;
        }

        /**
		 * \brief Select the padding method for EnsureSize
		 */
        public enum PaddMethod
		{
			After,
			Before
		}

		/**
		 * \brief Provide an array with the given size (truncate / padd with 00)
		 */
		public static byte[] EnsureSize(byte[] buffer, int minsize, int maxsize)
		{
			return EnsureSize(buffer, minsize, maxsize, PaddMethod.After);
		}

		/**
		 * \brief Provide an array with the given size (truncate / padd with 00)
		 */
		public static byte[] EnsureSize(byte[] buffer, int minsize, int maxsize, PaddMethod padd)
		{
			int resultSize;

            if (buffer == null)
                buffer = new byte[0];

            resultSize = buffer.Length;

			if (resultSize < minsize)
				resultSize = minsize;

			if ((maxsize >= minsize) && (resultSize > maxsize))
				resultSize = maxsize;

			if (resultSize == buffer.Length)
				return buffer;

			byte[] result = new byte[resultSize];

			int copySize = buffer.Length;
			if (copySize > resultSize)
				copySize = resultSize;

			if (padd == PaddMethod.After)
			{
				Array.Copy(buffer, 0, result, 0, copySize);
			}
			if (padd == PaddMethod.Before)
			{
				Array.Copy(buffer, 0, result, resultSize - copySize, copySize);
			}

			return result;
		}

		/**
		 * \brief Select the endianness for convertion functions
		 */
		public enum Endianness
		{
			BigEndian,
			LittleEndian
		}

		/**
		 * \brief Convert an array of 2 bytes to the corresponding WORD
		 */
		public static UInt16 ToWord(byte[] value, Endianness endianness = Endianness.BigEndian)
		{
			UInt16 result = 0;

			if (endianness == Endianness.BigEndian)
			{
				for (int i = 0; i <= 1; i++)
				{
					result <<= 8;
					result |= value[i];
				}
			}
			else
			{
				for (int i = 1; i >= 0; i--)
				{
					result <<= 8;
					result |= value[i];
				}
			}

			return result;
		}

		/**
		 * \brief Convert an array of 2 bytes to the corresponding DWORD
		 * 
		 * \deprecated Use ToWord()
		 */
		public static UInt16 BytesToUInt16(byte[] value)
		{
			return ToWord(value);
		}

		/**
		 * \brief Convert a WORD into an array of 2 bytes
		 */
		public static byte[] FromWord(UInt16 value, Endianness endianness = Endianness.BigEndian)
		{
			byte[] result = new byte[2];

			if (endianness == Endianness.BigEndian)
			{
				for (int i = 1; i >= 0; i--)
				{
					result[i] = (byte)(value & 0x0FF);
					value >>= 8;
				}
			}
			else
			{
				for (int i = 0; i <= 1; i++)
				{
					result[i] = (byte)(value & 0x0FF);
					value >>= 8;
				}
			}

			return result;
		}

		/**
		 * \brief Convert a WORD into an array of 2 bytes
		 * 
		 * \deprecated Use FromWord()
		 */
		public static byte[] UInt16ToBytes(UInt16 value)
		{
			return FromWord(value);
		}

		/**
		 * \brief Convert an array of 4 bytes to the corresponding DWORD
		 */
		public static UInt32 ToDword(byte[] value, Endianness endianness = Endianness.BigEndian)
		{
			UInt32 result = 0;

			if (endianness == Endianness.BigEndian)
			{
				for (int i = 0; i <= 3; i++)
				{
					result <<= 8;
					result |= value[i];
				}
			}
			else
			{
				for (int i = 3; i >= 0; i--)
				{
					result <<= 8;
					result |= value[i];
				}
			}

			return result;
		}

		/**
		 * \brief Convert an array of 4 bytes to the corresponding DWORD
		 * 
		 * \deprecated Use ToDword()
		 */
		public static UInt32 BytesToUInt32(byte[] value)
		{
			return ToDword(value);
		}

		/**
		 * \brief Convert a DWORD into an array of 4 bytes
		 */
		public static byte[] FromDword(UInt32 value, Endianness endianness = Endianness.BigEndian)
		{
			byte[] result = new byte[4];

			if (endianness == Endianness.BigEndian)
			{
				for (int i = 3; i >= 0; i--)
				{
					result[i] = (byte)(value & 0x0FF);
					value >>= 8;
				}
			}
			else
			{
				for (int i = 0; i <= 3; i++)
				{
					result[i] = (byte)(value & 0x0FF);
					value >>= 8;
				}
			}

			return result;
		}

		/**
		 * \brief Convert a DWORD into an array of 4 bytes
		 * 
		 * \deprecated Use FromDword()
		 */
		public static byte[] UInt32ToBytes(UInt32 value)
		{
			return FromDword(value);
		}

		/**
		 * \brief Convert an array of 8 bytes to the corresponding QWORD
		 */
		public static UInt64 ToQword(byte[] value, Endianness endianness = Endianness.BigEndian)
		{
			UInt64 result = 0;

			if (endianness == Endianness.BigEndian)
			{
				for (int i = 0; i <= 7; i++)
				{
					result <<= 8;
					result |= value[i];
				}
			}
			else
			{
				for (int i = 7; i >= 0; i--)
				{
					result <<= 8;
					result |= value[i];
				}
			}

			return result;
		}

		/**
		 * \brief Convert an array of 8 bytes to the corresponding QWORD
		 * 
		 * \deprecated Use ToQword()
		 */
		public static UInt64 BytesToUInt64(byte[] value)
		{
			return ToQword(value);
		}

		/**
		 * \brief Convert a QWORD into an array of 8 bytes
		 */
		public static byte[] FromQword(UInt64 value, Endianness endianness = Endianness.BigEndian)
		{
			byte[] result = new byte[8];

			if (endianness == Endianness.BigEndian)
			{
				for (int i = 7; i >= 0; i--)
				{
					result[i] = (byte)(value & 0x0FF);
					value >>= 8;
				}
			}
			else
			{
				for (int i = 0; i <= 7; i++)
				{
					result[i] = (byte)(value & 0x0FF);
					value >>= 8;
				}
			}

			return result;
		}

		/**
		 * \brief Convert a QWORD into an array of 4 bytes
		 * 
		 * \deprecated Use FromQword()
		 */
		public static byte[] UInt64ToBytes(UInt64 value)
		{
			return FromQword(value);
		}
		/**
		 * \brief Count the number of bits set to 1 in the buffer
		 */
		public static int CountOnes(byte[] buffer)
		{
			int result = 0;

			if (buffer == null)
				return 0;

			for (int i = 0; i < buffer.Length; i++)
			{
				for (int j = 7; j >= 0; j--)
				{
					if ((buffer[i] & (1 << j)) != 0)
						result++;
				}
			}

			return result;
		}

		/**
		 * \brief Get the index of the first 1 bit, starting from the right
		 */
		public static int GetOnePositionRight(byte[] buffer)
		{
			if (buffer == null)
				return -1;

			int result = 0;
			for (int i = buffer.Length - 1; i >= 0; i--)
			{
				for (int j = 0; j < 8; j++)
				{
					if ((buffer[i] & (1 << j)) != 0)
						return result;

					result++;
				}
			}

			return -1;
		}

		/**
		 * \brief Get the index of the first 1 bit, starting from the left
		 */
		public static int GetOnePositionLeft(byte[] buffer)
		{
			if (buffer == null)
				return -1;

			int result = 0;
			for (int i = 0; i < buffer.Length; i++)
			{
				for (int j = 7; j >= 0; j--)
				{
					if ((buffer[i] & (1 << j)) != 0)
						return result;

					result++;
				}
			}

			return -1;
		}

		public static int CountConsecutiveOnes(byte[] buffer)
		{
			int result = 0;
			bool one_seen = false;
			bool zero_after_one_seen = false;

			if (buffer == null)
				return 0;

			for (int i = 0; i < buffer.Length; i++)
			{
				for (int j = 7; j >= 0; j--)
				{
					if ((buffer[i] & (1 << j)) != 0)
					{
						if (zero_after_one_seen)
							return 0;

						one_seen = true;
						result++;
					}
					else
					{
						if (one_seen)
							zero_after_one_seen = true;
					}
				}
			}

			return result;
		}

		/**
		 * \brief Logical INVERT of one array: result = ~buffer.
		 */
		public static byte[] INVERT(byte[] buffer)
		{
			byte[] result = new byte[buffer.Length];
			for (int i = 0; i < buffer.Length; i++)
				result[i] = (byte)(~buffer[i] & 0x0FF);
			return result;
		}

		/**
		 * \brief Logical AND of two arrays: result = buffer1 AND buffer2. The length of the resulting array is set to the shortest of both.
		 */
		public static byte[] AND(byte[] buffer1, byte[] buffer2)
		{
			return AND(buffer1, buffer2, false);
		}

		/**
		 * \brief Logical AND of two arrays: result = buffer1 AND buffer2. If expand is false, the length of the resulting array is set to the shortest of both. If expand is true, to the longest.
		 */
		public static byte[] AND(byte[] buffer1, byte[] buffer2, bool expand)
		{
			return BufferOperation(buffer1, buffer2, expand, BufferOperationType.AND);
		}

		/**
		 * \brief Logical OR of two arrays: result = buffer1 OR buffer2. The length of the resulting array is set to the shortest of both.
		 */
		public static byte[] OR(byte[] buffer1, byte[] buffer2)
		{
			return OR(buffer1, buffer2, false);
		}

		/**
		 * \brief Logical OR of two arrays: result = buffer1 OR buffer2. If expand is false, the length of the resulting array is set to the shortest of both. If expand is true, to the longest.
		 */
		public static byte[] OR(byte[] buffer1, byte[] buffer2, bool expand)
		{
			return BufferOperation(buffer1, buffer2, expand, BufferOperationType.OR);
		}

		/**
		 * \brief Logical XOR of two arrays: result = buffer1 XOR buffer2. The length of the resulting array is set to the shortest of both.
		 */
		public static byte[] XOR(byte[] buffer1, byte[] buffer2)
		{
			return XOR(buffer1, buffer2, false);
		}

		/**
		 * \brief Logical XOR of two arrays: result = buffer1 XOR buffer2. If expand is false, the length of the resulting array is set to the shortest of both. If expand is true, to the longest.
		 */
		public static byte[] XOR(byte[] buffer1, byte[] buffer2, bool expand)
		{
			return BufferOperation(buffer1, buffer2, expand, BufferOperationType.XOR);
		}

		/**
		 * \brief Clear bits from one array with the other: result = buffer1 AND ~buffer2. The length of the resulting array is set to the shortest of both.
		 */
		public static byte[] CLEAR(byte[] buffer1, byte[] buffer2)
		{
			return CLEAR(buffer1, buffer2, false);
		}

		/**
		 * \brief Clear bits from one array with the other: result = buffer1 AND ~buffer2. If expand is false, the length of the resulting array is set to the shortest of both. If expand is true, to the longest.
		 */
		public static byte[] CLEAR(byte[] buffer1, byte[] buffer2, bool expand)
		{
			return AND(buffer1, INVERT(buffer2), expand);
		}

		private enum BufferOperationType { AND, OR, XOR };

		private static byte[] BufferOperation(byte[] buffer1, byte[] buffer2, bool expand_instead_of_truncate, BufferOperationType operation)
		{
			if ((buffer1 == null) && (buffer2 == null))
				return null;

			int length1 = 0;
			if (buffer1 != null)
				length1 = buffer1.Length;

			int length2 = 0;
			if (buffer2 != null)
				length2 = buffer2.Length;

			int length = 0;

			if (length1 == length2)
			{
				length = length1;
			}
			else if (length1 > length2)
			{
				if (expand_instead_of_truncate)
					length = length1;
				else
					length = length2;
			}
			else if (length2 > length1)
			{
				if (expand_instead_of_truncate)
					length = length2;
				else
					length = length1;
			}

			byte[] result = new byte[length];

			for (int i = 0; i < length; i++)
			{
				byte b1 = 0, b2 = 0;

				if (i < length1)
					b1 = buffer1[i];
				if (i < length2)
					b2 = buffer2[i];

				switch (operation)
				{
					case BufferOperationType.AND:
						result[i] = (byte)(b1 & b2);
						break;
					case BufferOperationType.OR:
						result[i] = (byte)(b1 | b2);
						break;
					case BufferOperationType.XOR:
						result[i] = (byte)(b1 ^ b2);
						break;
				}
			}

			return result;
		}

		/**
		 * \brief Replace a bit pattern, looking up from the right
		 */
		public static byte[] ReplaceFromRight(byte[] buffer, byte[] replace, int offset_from_right, int length_bits)
		{
			if ((buffer == null) || (replace == null))
				return null;

			byte[] result = new byte[buffer.Length];

			for (int i = 0; i < buffer.Length; i++)
				result[i] = buffer[i];

			int offset_replace = 8 * replace.Length - 1;
			int offset_buffer = 8 * buffer.Length - offset_from_right - 1;

			for (int i = 0; i < length_bits; i++)
			{
				result = SetBitAt(result, offset_buffer--, GetBitAt(replace, offset_replace--));
			}

			return result;
		}

		/**
		 * \brief Extract some bits, starting from the right
		 */
		public static byte[] ExtractBitsRight(byte[] buffer, int offset_from_right, int length_bits)
		{
			if (buffer == null)
				return null;

			int byte_length = (length_bits + 7) / 8;
			byte[] result = new byte[byte_length];

			int offset_dst = 8 * byte_length - 1;
			int offset_src = 8 * buffer.Length - offset_from_right - 1;

			for (int i = 0; i < length_bits; i++)
				result = SetBitAt(result, offset_dst--, GetBitAt(buffer, offset_src--));

			return result;
		}

		/**
		 * \brief Extract some bits, starting from the left
		 */
		public static byte[] ExtractBitsLeft(byte[] buffer, int offset_from_left, int length_bits)
		{
			if (buffer == null)
				return null;

			int byte_length = (length_bits + 7) / 8;
			byte[] result = new byte[byte_length];

			int offset_dst = 7 - (length_bits % 8);
			int offset_src = offset_from_left;

			for (int i = 0; i < length_bits; i++)
				result = SetBitAt(result, offset_dst++, GetBitAt(buffer, offset_src++));

			return result;
		}

		/**
		 * \brief Set a bit to 1 in the buffer
		 */
		public static byte[] SetBitAt(byte[] buffer, int offset, bool value)
		{
			int byte_offset = offset / 8;
			int bit_offset = 7 - (offset % 8);

			int bit_mask = 1 << bit_offset;

			if (value)
			{
				buffer[byte_offset] |= (byte)bit_mask;
			}
			else
			{
				bit_mask = bit_mask ^ 0x0FF;
				buffer[byte_offset] &= (byte)bit_mask;
			}

			return buffer;
		}

		/**
		 * \brief Get the value of one bit from the buffer
		 */
		public static bool GetBitAt(byte[] buffer, int offset)
		{
			if ((buffer == null) || (offset < 0))
				return false;

			int byte_offset = offset / 8;

			if (byte_offset >= buffer.Length)
				return false;

			int bit_offset = 7 - (offset % 8);

			if ((buffer[byte_offset] & (1 << bit_offset)) != 0)
				return true;

			return false;
		}
	}
}
