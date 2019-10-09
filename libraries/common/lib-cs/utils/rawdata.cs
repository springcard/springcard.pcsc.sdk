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
	public class RawData
	{
		public static void MemCopy(byte[] Target, byte[] Source)
		{
			MemCopy(Target, Source, Source.Length);
		}

		public static void MemCopy(byte[] Target, byte[] Source, int Length)
		{
			MemCopy(Target, 0, Source, 0, Length);
		}

		public static void MemCopy(byte[] Target, int offsetTarget, byte[] Source, int offsetSource, int Length)
		{
			int i;

			for (i = 0; i < Length; i++)
				Target[offsetTarget + i] = Source[offsetSource + i];
		}

		/**
		 * \brief Compare two arrays
		 * 
		 * \deprecated Use BinUtils.Equals()
		 */
		[Obsolete("RawData.MemEqual is deprecated, please use BinUtils.Equals instead.")]
		public static bool MemEqual(byte[] Buffer1, byte[] Buffer2)
		{
			if ((Buffer1 == null) && (Buffer2 == null))
				return true;
			if ((Buffer1 == null) || (Buffer2 == null))
				return false;
			if (Buffer1.Length != Buffer2.Length)
				return false;

			return MemEqual(Buffer1, Buffer2, Buffer2.Length);
		}

		/**
		 * \brief Compare two arrays
		 * 
		 * \deprecated Use BinUtils.Equals()
		 */
		[Obsolete("RawData.MemEqual is deprecated, please use BinUtils.Equals instead.")]
		public static bool MemEqual(byte[] Buffer1, byte[] Buffer2, int Length)
		{
			if ((Buffer1 == null) && (Buffer2 == null))
				return true;
			if ((Buffer1 == null) || (Buffer2 == null))
				return false;
			if (Buffer1.Length != Buffer2.Length)
				return false;
			if ((Buffer1.Length < Length) || (Buffer2.Length < Length))
				return false;

			return MemEqual(Buffer1, 0, Buffer2, 0, Length);
		}

		/**
		 * \brief Compare two arrays
		 * 
		 * \deprecated Use BinUtils.Equals()
		 */
		[Obsolete("RawData.MemEqual is deprecated, please use BinUtils.Equals instead.")]
		public static bool MemEqual(byte[] Buffer1, int offsetBuffer1, byte[] Buffer2, int offsetBuffer2, int Length)
		{
			if ((Buffer1 == null) && (Buffer2 == null))
				return true;
			if ((Buffer1 == null) || (Buffer2 == null))
				return false;
			if ((Buffer1.Length < offsetBuffer1 + Length) || (Buffer2.Length < offsetBuffer2 + Length))
				return false;

			int i;

			for (i = 0; i < Length; i++)
				if (Buffer1[offsetBuffer1 + i] != Buffer2[offsetBuffer2 + i])
					return false;

			return true;
		}

		/**
		 * \brief Compare two arrays
		 * 
		 * \deprecated Use BinUtils.Equals()
		 */
		[Obsolete("RawData.BufferEquals is deprecated, please use BinUtils.Equals instead.")]
		public static bool BufferEquals(byte[] buf1, byte[] buf2)
		{
			if ((buf1 == null) || (buf2 == null))
			{
				if ((buf1 == null) && (buf2 == null))
					return true;
				return false;
			}

			if (buf1.Length != buf2.Length)
				return false;

			int i;
			for (i = 0; i < buf1.Length; i++)
				if (buf1[i] != buf2[i])
					return false;

			return true;
		}

		/**
		 * \brief Copy an array
		 * 
		 * \deprecated Use BinUtils.Copy()
		 */
		[Obsolete("RawData.CopyBuffer is deprecated, please use BinUtils.Copy instead.")]
		public static byte[] CopyBuffer(byte[] buffer, int startIndex = 0, int copyLength = 0, bool ensureLength = false)
		{
			int bufferLength = 0;

			if (buffer != null)
				bufferLength = buffer.Length;

			if (startIndex < 0)
			{
				/* Count from the end */
				startIndex = bufferLength + startIndex;
			}

			int endIndex;
			if (copyLength == 0)
			{
				/* We go up to the last one */
				endIndex = bufferLength - 1;
			}
			else if (copyLength < 0)
			{
				/* Count from the end */
				endIndex = bufferLength + copyLength - 1;
			}
			else
			{
				/* Count from the start index */
				endIndex = startIndex + copyLength - 1;
			}

			int actualLength = endIndex - startIndex + 1;

			if (actualLength < 0)
				return null;

			byte[] result;

			if ((actualLength < copyLength) && ensureLength)
			{
				result = new byte[copyLength];
			}
			else
			{
				result = new byte[actualLength];
			}

			if ((actualLength > 0) && (buffer != null))
				Array.Copy(buffer, startIndex, result, 0, actualLength);

			return result;
		}
	}
}
