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
using System.Text;

namespace SpringCard.LibCs
{
	/**
	 * \brief String manipulation utilities
	 */
	public class StrUtils
	{
		/**
		 * \brief Count the number of occurrences of a char in a string
		 */
		public static int CountTokens(string source, char searched)
		{
			int count = 0;
			foreach (char c in source)
			{
				if (c == searched)
					count++;
			}
			return count;
		}

		/**
		 * \brief Translate a base64-encoded string into its actual value
		 */
		public static byte[] Base64Decode(string message)
		{
			return Convert.FromBase64String(message);
		}

        /**
		 * \brief Translate a base64-encoded string into its actual value
		 */
        public static bool Base64TryDecode(string message, out byte[] value)
        {
            try
            {
                value = Base64Decode(message);
                return true;
            }
            catch
            {
                value = null;
                return false;
            }
        }

        /**
		 * \brief Translate a base64url-encoded string into its actual value
		 */
        public static byte[] Base64UrlDecode(string message)
		{
			message = message.Replace('_', '/');
			message = message.Replace('-', '+');
			switch (message.Length % 4)
			{
				case 2:
					message += "==";
					break;
				case 3:
					message += "=";
					break;
			}
			return Convert.FromBase64String(message);
		}

        /**
		 * \brief Translate a base64url-encoded string into its actual value
		 */
        public static bool Base64UrlTryDecode(string message, out byte[] value)
        {
            try
            {
                value = Base64UrlDecode(message);
                return true;
            }
            catch
            {
                value = null;
                return false;
            }
        }

        /**
		 * \brief Translate a base64url-encoded string into its actual value - expect the value is a string itself
		 */
        public static string Base64UrlDecodeString(string message, Encoding encoding)
		{
            return ToStr(Base64UrlDecode(message), encoding);
		}

        /**
		 * \brief Translate a base64url-encoded string into its actual value - expect the value is a string itself
		 */
        public static bool Base64UrlTryDecodeString(string message, Encoding encoding, out string value)
        {
            try
            {
                value = ToStr(Base64UrlDecode(message), encoding);
                return true;
            }
            catch
            {
                value = "";
                return false;
            }
        }

        /**
		 * \brief Translate a base64url-encoded string into its actual value - expect the value is a string itself
		 */
        public static string Base64UrlDecodeString(string message)
		{
			return Base64UrlDecodeString(message, Encoding.UTF8);
		}

        /**
		 * \brief Translate a base64url-encoded string into its actual value - expect the value is a string itself
		 */
        public static bool Base64UrlTryDecodeString(string message, out string value)
        {
            try
            {
                value = ToStr(Base64UrlDecode(message));
                return true;
            }
            catch
            {
                value = "";
                return false;
            }
        }

        /**
		 * \brief Translate an array of bytes into base64
		 */
        public static string Base64Encode(byte[] content, int lineLength = 0)
		{
            if (content == null)
                return "";

			string result = System.Convert.ToBase64String(content);
			if (lineLength > 0)
			{
				string t = "";
				while (result.Length > lineLength)
				{
					if (t.Length > 0) t = t + "\n";
					t = t + result.Substring(0, lineLength);
					result = result.Substring(lineLength);
				}
				if (result.Length > 0)
				{
					if (t.Length > 0) t = t + "\n";
					t = t + result;
				}
				result = t;
			}
			return result;
		}

		/**
		 * \brief Translate an array of bytes into base64url
		 */
		public static string Base64UrlEncode(byte[] content)
		{
            if (content == null)
                return "";

            string result = Convert.ToBase64String(content);
			result = result.TrimEnd('=');
			result = result.Replace('+', '-');
			result = result.Replace('/', '_');
			return result;
		}

		/**
		 * \brief Translate a string into base64url
		 */
		public static string Base64UrlEncode(string content, Encoding encoding)
		{
			return Base64UrlEncode(encoding.GetBytes(content));
		}

		/**
		 * \brief Translate a string into base64url
		 */
		public static string Base64UrlEncode(string content)
		{
			return Base64UrlEncode(content, Encoding.UTF8);
		}

		/**
		 * \brief Convert a string into an array of bytes. The '\0' terminator may be included if requested.
		 */
		public static byte[] ToBytes(string input, Encoding encoding, bool includeTerminator = false)
		{
			byte[] result = encoding.GetBytes(input);
			if (includeTerminator)
			{
				byte[] endMarker = new byte[1];
				result = BinUtils.Concat(result, endMarker);
			}
			return result;
		}

		/**
		 * \brief Convert a string into an array of bytes. The '\0' terminator may be included if requested.
		 */
		public static byte[] ToBytes(string input, bool includeTerminator = false)
		{
			return ToBytes(input, Encoding.UTF8, includeTerminator);
		}

		/**
		 * \brief Convert an array of bytes into a string
		 */
		public static string ToStr(byte[] buffer, Encoding encoding)
		{
            /* New JDA 2019/01/22: terminate the string correctly */
            int count;
            for (count=0; count<buffer.Length; count++)
                if (buffer[count] == 0)
                    break;
            return encoding.GetString(buffer, 0, count);
		}

		/**
		 * \brief Convert an array of bytes into a string
		 */
		public static string ToStr(byte[] buffer)
		{
			return ToStr(buffer, Encoding.UTF8);
		}

		/**
		 * \brief Convert an array of bytes into a string
		 */
		public static string ToStr(byte[] buffer, int offset, Encoding encoding)
		{
			return encoding.GetString(buffer, offset, buffer.Length - offset);
		}

		/**
		 * \brief Convert an array of bytes into a string
		 */
		public static string ToStr(byte[] buffer, int offset)
		{
			return ToStr(buffer, offset, Encoding.UTF8);
		}

		/**
		 * \brief Convert an array of bytes into a string
		 */
		public static string ToStr(byte[] buffer, int offset, int length, Encoding encoding)
		{
			return encoding.GetString(buffer, offset, length);
		}

		/**
		 * \brief Convert an array of bytes into a string
		 */
		public static string ToStr(byte[] buffer, int offset, int length)
		{
			return ToStr(buffer, offset, length, Encoding.UTF8);
		}

		/**
		 * \brief Convert a string into an array of bytes, encoded in UTF8. The '\0' terminator may be included if requested.
		 */
		public static byte[] ToBytes_UTF8(string input, bool includeTerminator = false)
		{
			return ToBytes(input, Encoding.UTF8, includeTerminator);
		}

		/**
		 * \brief Convert an array of bytes, supposingly holding an UTF8 string, into a string
		 */
		public static string ToStr_UTF8(byte[] buffer)
		{
			return ToStr(buffer, Encoding.UTF8);
		}

		/**
		 * \brief Convert an array of bytes, supposingly holding an UTF8 string, into a string
		 */
		public static string ToStr_UTF8(byte[] buffer, int offset)
		{
			return ToStr(buffer, offset, Encoding.UTF8);
		}

		/**
		 * \brief Convert an array of bytes, supposingly holding an UTF8 string, into a string
		 */
		public static string ToStr_UTF8(byte[] buffer, int offset, int length)
		{
			return ToStr(buffer, offset, length, Encoding.UTF8);
		}

		/**
		 * \brief Convert a string into an array of bytes, encoded in ASCII. The '\0' terminator may be included if requested.
		 */
		public static byte[] ToBytes_ASCII(string input, bool includeTerminator = false)
		{
			return ToBytes(input, Encoding.ASCII, includeTerminator);
		}

		/**
		 * \brief Convert an array of bytes, supposingly holding an ASCII string, into a string
		 */
		public static string ToStr_ASCII(byte[] buffer)
		{
			return ToStr(buffer, Encoding.ASCII);
		}

		/**
		 * \brief Convert an array of bytes, supposingly holding an ASCII string, into a string
		 */
		public static string ToStr_ASCII(byte[] buffer, int offset)
		{
			return ToStr(buffer, offset, Encoding.ASCII);
		}

		/**
		 * \brief Convert an array of bytes, supposingly holding an ASCII string, into a string
		 */
		public static string ToStr_ASCII(byte[] buffer, int offset, int length)
		{
			return ToStr(buffer, offset, length, Encoding.ASCII);
		}

		/**
		 * \brief Convert an array of bytes, supposingly holding an ASCII string, into a string. Non ASCII chars are replaced by '.'.
		 */
		public static string ToStr_ASCII_nice(byte[] buffer, bool stopOnZero = true)
		{
			string s = "";
			if (buffer != null)
			{
				for (int i = 0; i < buffer.Length; i++)
				{
					if ((buffer[i] == 0) && stopOnZero)
						break;
					if ((buffer[i] <= ' ') || (buffer[i] >= 128))
					{
						s = s + '.';
					}
					else
					{
						s = s + (char)buffer[i];
					}
				}
			}
			return s;
		}

		/**
		 * \brief Return true if the string contains an integer
		 */
		public static bool IsValidInteger(string value)
		{
			int r;
			return int.TryParse(value, out r);
		}

		/**
		 * \brief Return true if the string contains a boolean ("true", "false", "yes", "no" or an integer)
		 */
		public static bool IsValidBoolean(string value)
		{
			bool valid;
			ReadBoolean(value, out valid);
			return valid;
		}

		/**
		 * \brief Read a boolean value from a string: return true for "true", "yes" or a non-zero integer, false otherwise
		 */
		public static bool ReadBoolean(string value)
		{
			bool dummy;
			return ReadBoolean(value, out dummy);
		}

		/**
		 * \brief Read a boolean value from a string; the valid out parameter takes the return of IsValidBoolean()
		 */
		public static bool ReadBoolean(string value, out bool valid)
		{
			valid = false;
			if (string.IsNullOrEmpty(value))
				return false;

			value = value.ToLower();

			if (value.Equals("false") || value.Equals("no"))
			{
				valid = true;
				return false;
			}
			if (value.Equals("true") || value.Equals("yes"))
			{
				valid = true;
				return true;
			}

			int r;
			if (!int.TryParse(value, out r))
				return false;

			valid = true;
			if (r != 0)
				return true;
			else
				return false;
		}
	}

}
