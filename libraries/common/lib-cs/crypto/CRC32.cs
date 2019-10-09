using System;
using System.Collections.Generic;
using System.Linq;

namespace SpringCard.LibCs.Crypto
{
	public class CRC32
	{
		public const UInt32 DefaultPolynomial = 0xedb88320u;
		public const UInt32 DefaultSeed = 0xffffffffu;

		static UInt32[] defaultTable;

		
		public static byte[] Hash(byte[] buffer)
		{
			return Hash(DefaultSeed, buffer);
		}

		public static byte[] Hash(UInt32 seed, byte[] buffer)
		{
			return Hash(DefaultPolynomial, seed, buffer);
		}

		public static byte[] Hash(UInt32 polynomial, UInt32 seed, byte[] buffer)
		{
			return UInt32ToBigEndianBytes(~CalculateHash(InitializeTable(polynomial), seed, buffer, 0, buffer.Length));
		}

		static UInt32[] InitializeTable(UInt32 polynomial)
		{
			if (polynomial == DefaultPolynomial && defaultTable != null)
				return defaultTable;

			var createTable = new UInt32[256];
			for (var i = 0; i < 256; i++)
			{
				var entry = (UInt32)i;
				for (var j = 0; j < 8; j++)
					if ((entry & 1) == 1)
						entry = (entry >> 1) ^ polynomial;
					else
						entry = entry >> 1;
				createTable[i] = entry;
			}

			if (polynomial == DefaultPolynomial)
				defaultTable = createTable;

			return createTable;
		}

		static UInt32 CalculateHash(UInt32[] table, UInt32 seed, IList<byte> buffer, int start, int size)
		{
			var hash = seed;
			for (var i = start; i < start + size; i++)
				hash = (hash >> 8) ^ table[buffer[i] ^ hash & 0xff];
			return hash;
		}

		static byte[] UInt32ToBigEndianBytes(UInt32 uint32)
		{
			var result = BitConverter.GetBytes(uint32);

			if (BitConverter.IsLittleEndian)
				Array.Reverse(result);

			return result;
		}
		
		public static bool SelfTest()
		{
			string str = "Hello, world!";
			byte[] buffer = new byte[str.Length];
			for (int i=0; i<buffer.Length; i++)
				buffer[i] = (byte) str[i];
			byte[] crc32 = Hash(buffer);
			if (crc32[0] != 0xEB) return false;
			if (crc32[1] != 0xE6) return false;
			if (crc32[2] != 0xC6) return false;
			if (crc32[3] != 0xE6) return false;
			return true;
		}
	}
}
