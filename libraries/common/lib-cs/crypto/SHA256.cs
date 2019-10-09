using System;
using System.IO;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.Encoders;
using Org.BouncyCastle.Crypto.Digests;

namespace SpringCard.LibCs.Crypto
{
	public static class SHA256
	{
		public static byte[] Hash(byte[] buffer)
		{
			Sha256Digest digest = new Sha256Digest();
			digest.BlockUpdate(buffer, 0, buffer.Length);
			byte[] result = new byte[32];
			digest.DoFinal(result, 0);
			return result;
		}
		
		public static byte[] Hash128(byte[] buffer, bool odd = false)
		{
			byte[] hash = Hash(buffer);
			byte[] result = new byte[16];
			if (odd)
			{
				for (int i=0; i<16; i++)
					result[i] = hash[2 * i + 1];
			}
			else
			{
				for (int i=0; i<16; i++)
					result[i] = hash[2 * i];				
			}
			return result;
		}
	}
}
