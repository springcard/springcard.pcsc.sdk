using System;

namespace SpringCard.LibCs.Crypto
{
	public class MD5
	{
		public static byte[] Hash(byte[] buffer)
		{
			// Use input string to calculate MD5 hash
			using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
			{
				return md5.ComputeHash(buffer);
			}
		}

        private System.Security.Cryptography.MD5 md5;

        public MD5()
        {
            md5 = System.Security.Cryptography.MD5.Create();
        }

        public void Update(byte[] buffer)
        {
            md5.TransformBlock(buffer, 0, buffer.Length, buffer, 0);
        }

        public byte[] Final()
        {
            byte[] result = md5.TransformFinalBlock(null, 0, 0);
            return result;
        }
	}
}
