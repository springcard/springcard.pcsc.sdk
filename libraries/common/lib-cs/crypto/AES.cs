using System;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Engines;

namespace SpringCard.LibCs.Crypto
{
	public static class AES
	{
		public static byte[] Encrypt(byte[] key, byte[] block)
		{
			AesEngine engine = new AesEngine();
			engine.Init(true, new KeyParameter(key));
			byte[] result = new byte[16];
			engine.ProcessBlock(block, 0, result, 0);
			return result;
		}
	}
}

