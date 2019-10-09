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
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SpringCard.LibCs
{
	/**
	 * \brief MD5 hash
	 */
	public class MD5
	{
		/**
		 * \brief Compute the MD5 hash of an array of bytes
		 */
		public static byte[] Hash(byte[] input)
		{
			// Use input string to calculate MD5 hash
			using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
			{
				return md5.ComputeHash(input);
			}
		}

		/**
		 * \brief Compute the MD5 hash of a string
		 */
		public static byte[] Hash(string input, Encoding encoding = null)
		{
			if (encoding == null)
				encoding = Encoding.UTF8;
			byte[] inputBytes = encoding.GetBytes(input);
			return Hash(inputBytes);
		}

		/**
		 * \brief Compute the MD5 hash of a string, and returns it as an hexadecimal string
		 */
		public static string HashToHex(string input, Encoding encoding = null)
		{
			return BinConvert.ToHex(Hash(input, encoding));
		}

		/**
		 * \brief Compute the MD5 hash of a file
		 */
		public static byte[] HashFile(string FileName)
		{
			using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
			{
				using (var stream = File.OpenRead(FileName))
				{
					return md5.ComputeHash(stream);
				}
			}
		}

		/**
		 * \brief Compute the MD5 hash of a file, and returns it as an hexadecimal string
		 */
		public static string HashFileToHex(string FileName)
		{
			return BinConvert.ToHex(HashFile(FileName));
		}

		/**
		 * \brief Read the MD5 of a file from a file created by md5sum
		 */
		public static string ReadMd5sumOutputHex(string Md5SumOutputFile, string FileName = null)
		{
			if (!string.IsNullOrEmpty(FileName))
				FileName = FileUtils.BaseName(FileName).ToLower();

			try
			{
				foreach (string line in File.ReadLines(Md5SumOutputFile))
				{
					string[] e = line.Split(new char[] { ' ' }, 2);
					if (e.Length == 2)
					{
						e[0] = e[0].Trim().ToUpper();

						if (string.IsNullOrEmpty(FileName))
							return e[0];

						e[1] = e[1].Trim().ToLower();

						if (e[1].StartsWith("*"))
							e[1] = e[1].Substring(1);

						if (e[1] == FileName)
							return e[0];
					}
				}
			}
			catch
			{
			}

			return "";
		}

        /**
		 * \brief Compute a HMAC-MD5
		 */
        public static byte[] HMAC(byte[] key, byte[] input)
        {
            using (System.Security.Cryptography.HMACMD5 hmacmd5 = new System.Security.Cryptography.HMACMD5(key))
            {
                return hmacmd5.ComputeHash(input);
            }
        }

        /**
		 * \brief Compute a HMAC-MD5 - input is a text
		 */
        public static byte[] HMAC(byte[] key, string input, Encoding encoding = null)
        {
            if (encoding == null)
                encoding = Encoding.UTF8;
            byte[] inputBytes = encoding.GetBytes(input);
            return HMAC(key, inputBytes);
        }

        /**
		 * \brief Compute a HMAC-MD5 - key and input are texts
		 */
        public static byte[] HMAC(string key, string input, Encoding encoding = null)
        {
            if (encoding == null)
                encoding = Encoding.UTF8;
            byte[] keyBytes = encoding.GetBytes(key);
            byte[] inputBytes = encoding.GetBytes(input);
            return HMAC(keyBytes, inputBytes);
        }

        /**
		 * \brief Compute a HMAC-MD5 - key is a text
		 */
        public static byte[] HMAC(string key, byte[] input, Encoding encoding = null)
        {
            if (encoding == null)
                encoding = Encoding.UTF8;
            byte[] keyBytes = encoding.GetBytes(key);
            return HMAC(keyBytes, input);
        }

        private System.Security.Cryptography.MD5 md5;

        /**
		 * \brief MD5 engine - Initialization
		 */
        public MD5()
        {
            md5 = System.Security.Cryptography.MD5.Create();
        }

        /**
		 * \brief MD5 engine - Update
		 */
        public void Update(byte[] buffer)
        {
            if (buffer != null)
                md5.TransformBlock(buffer, 0, buffer.Length, buffer, 0);
        }

        /**
		 * \brief MD5 engine - Final
		 */
        public byte[] Final()
        {
            md5.TransformFinalBlock(new byte[0], 0, 0);
            byte[] result = md5.Hash;
            md5.Dispose();
            md5 = null;
            return result;
        }
    }

    /**
	 * \brief SHA256 hash
	 */
    public class SHA256
    {
        /**
		 * \brief Compute the SHA256 hash of an array of bytes
		 */
        public static byte[] Hash(byte[] input)
        {
            // Use input string to calculate MD5 hash
            using (System.Security.Cryptography.SHA256 sha256 = System.Security.Cryptography.SHA256.Create())
            {
                return sha256.ComputeHash(input);
            }
        }

        /**
		 * \brief Compute the SHA256 hash of a string
		 */
        public static byte[] Hash(string input, Encoding encoding = null)
        {
            if (encoding == null)
                encoding = Encoding.UTF8;
            byte[] inputBytes = encoding.GetBytes(input);
            return Hash(inputBytes);
        }

        /**
		 * \brief Compute the SHA256 hash of a string, and returns it as an hexadecimal string
		 */
        public static string HashToHex(string input, Encoding encoding = null)
        {
            return BinConvert.ToHex(Hash(input, encoding));
        }

        /**
		 * \brief Compute the SHA256 hash of a file
		 */
        public static byte[] HashFile(string FileName)
        {
            using (System.Security.Cryptography.SHA256 sha256 = System.Security.Cryptography.SHA256.Create())
            {
                using (var stream = File.OpenRead(FileName))
                {
                    return sha256.ComputeHash(stream);
                }
            }
        }

        /**
		 * \brief Compute a HMAC-SHA256
		 */
        public static byte[] HMAC(byte[] key, byte[] input)
        {
            using (System.Security.Cryptography.HMACSHA256 hmacsha256 = new System.Security.Cryptography.HMACSHA256(key))
            {
                return hmacsha256.ComputeHash(input);
            }
        }

        /**
		 * \brief Compute a HMAC-SHA256 - input is a text
		 */
        public static byte[] HMAC(byte[] key, string input, Encoding encoding = null)
        {
            if (encoding == null)
                encoding = Encoding.UTF8;
            byte[] inputBytes = encoding.GetBytes(input);
            return HMAC(key, inputBytes);
        }

        /**
		 * \brief Compute a HMAC-SHA256 - key and input are texts
		 */
        public static byte[] HMAC(string key, string input, Encoding encoding = null)
        {
            if (encoding == null)
                encoding = Encoding.UTF8;
            byte[] keyBytes = encoding.GetBytes(key);
            byte[] inputBytes = encoding.GetBytes(input);
            return HMAC(keyBytes, inputBytes);
        }

        /**
		 * \brief Compute a HMAC-SHA256 - key is a text
		 */
        public static byte[] HMAC(string key, byte[] input, Encoding encoding = null)
        {
            if (encoding == null)
                encoding = Encoding.UTF8;
            byte[] keyBytes = encoding.GetBytes(key);
            return HMAC(keyBytes, input);
        }

        private System.Security.Cryptography.SHA256 sha256;

        /**
		 * \brief SHA256 engine - Initialization
		 */
        public SHA256()
        {
            sha256 = System.Security.Cryptography.SHA256.Create();
        }

        /**
		 * \brief SHA256 engine - Udpate
		 */
        public void Update(byte[] buffer)
        {
            if (buffer != null)
                sha256.TransformBlock(buffer, 0, buffer.Length, buffer, 0);
        }

        /**
		 * \brief SHA256 engine - Final
		 */
        public byte[] Final()
        {
            sha256.TransformFinalBlock(new byte[0], 0, 0);
            byte[] result = sha256.Hash;
            sha256.Dispose();
            sha256 = null;
            return result;
        }
    }
}
