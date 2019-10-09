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
using System.Linq;

namespace SpringCard.LibCs
{
	/**
	 * \brief Utilities to convert text into numbers or array, and reverse
	 */
	public class BinConvert
	{
		/**
		 * \brief Convert a string into an array of bytes
		 *
		 * \deprecated Use StrUtils.ToBytes_ASCII() or StrUtils.ToBytes_UTF8()
		 */
		[Obsolete("BinConvert.StringToByteArray is deprecated, please use StrUtils.ToBytes_ASCII or StrUtils.ToBytes_UTF8 instead.")]
		public static byte[] StringToByteArray(string str, bool includeEndZero = false)
		{
			char[] s_chars = str.ToCharArray();
			int plus = includeEndZero ? 1 : 0;
			byte[] s_bytes = new byte[s_chars.Length + plus];
			for (int i=0; i<s_chars.Length; i++)
				s_bytes[i] = (byte) s_chars[i];
			return s_bytes;
		}

		/**
		 * \brief Convert an array of bytes into a string
		 *
		 * \deprecated Use StrUtils.FromBytes_ASCII() or StrUtils.FromBytes_UTF8()
		 */
		[Obsolete("BinConvert.ByteArrayToString is deprecated, please use StrUtils.ToStr_ASCII or StrUtils.ToStr_UTF8 instead.")]
		public static string ByteArrayToString(byte[] val, bool stopOnZero = true, bool hideNonAscii = false)
		{
			string s = "";
			if (val != null)
			{
				for (int i=0; i<val.Length; i++)
				{
					if ((val[i] == 0) && stopOnZero)
						break;
					if ((val[i] <= ' ') && hideNonAscii)
					{
						s = s + '.';
					}
					else
					{
						s = s + (char) val[i];
					}
				}
			}
			return s;
		}

		/**
		 * \brief Convert an array of bytes into a string
		 */
		[Obsolete("BinConvert.ByteArrayToString is deprecated, please use StrUtils.ToStr_ASCII or StrUtils.ToStr_UTF8 instead.")]
		public static string ByteArrayToString(byte[] val, int offset, int length = -1, bool stopOnZero = true, bool hideNonAscii = false)
		{
			string s = "";
			if (val != null)
			{
				if ((length < 0) || (offset + length > val.Length))
				{
					length = val.Length - offset;
				}
				
				for (int i=offset; i<length; i++)
				{
					if ((val[i] == 0) && stopOnZero)
						break;
					if ((val[i] <= ' ') && hideNonAscii)
					{
						s = s + '.';
					}
					else
					{
						s = s + (char) val[i];
					}
				}
			}
			return s;
		}

		[Obsolete("BinConvert.ByteArrayToStringAscii is deprecated, please use StrUtils.ToStr_ASCII or StrUtils.ToStr_UTF8 instead.")]
		public static string ByteArrayToStringAscii(byte[] value)
		{
			return value != null ? string.Concat(value.Select(b => Convert.ToString(b, 2))) : "";
		}

		/**
		 * \brief Convert an decimal representation of an integer into its actual value
		 */
		public static int DecToInt(string dec)
		{
			int result;
			if (!int.TryParse(dec, out result))
				result = 0;
			return result;
		}

		/**
		 * \brief Convert an decimal representation of an integer into its actual value
		 * 
		 * \deprecated Use DecToInt()
		 */
		public static int ParseInt(string dec)
		{
			return DecToInt(dec);
		}

		/**
		 * \brief Convert an decimal representation of an unsigned integer into its actual value
		 */
		public static uint DecToUInt(string dec)
		{
			uint result;
			if (!uint.TryParse(dec, out result))
				result = 0;
			return result;
		}

        /**
		 * \brief Convert an decimal representation of an unsigned integer into its actual value
		 * 
		 * \deprecated Use DecToUInt()
		 */
        public static uint ParseUInt(string dec)
		{
			return DecToUInt(dec);
		}

		public static UInt32 ParseDW(string dec)
		{
			UInt32 result;
			if (!UInt32.TryParse(dec, out result))
				result = 0;
			return result;
		}

        /**
		 * \brief Convert an hexadecimal representation of an array of bytes into its actual value
		 */
        public static bool TryHexToBytes(string hex, out byte[] value, int minLength, int maxLength)
        {
            try
            {
                value = HexToBytes(hex);
                if ((minLength > 0) && (value.Length < minLength)) return false;
                if ((maxLength > 0) && (value.Length > maxLength)) return false;
                return true;
            }
            catch
            {
                value = null;
                return false;
            }
        }

        /**
		 * \brief Convert an hexadecimal representation of an array of bytes into its actual value
		 */
        public static bool TryHexToBytes(string hex, out byte[] value)
        {
            try
            {
                value = HexToBytes(hex);
                return true;
            }
            catch
            {
                value = null;
                return false;
            }
        }

        /**
		 * \brief Convert an hexadecimal representation of an array of bytes into its actual value
		 */
        public static byte[] HexToBytes(string hex)
		{
			hex = hex.Replace(" ", "");
            hex = hex.Replace("\t", "");
            hex = hex.Replace(":", "");
			hex = hex.Replace(",", "");
			hex = hex.Replace(";", "");

            hex = hex.Trim();

            int offset = hex.StartsWith("0x") ? 2 : 0;

			if ((hex.Length % 2) != 0)
				throw new ArgumentException("Invalid hex string: \"" + hex + "\"");

			byte[] ret = new byte[(hex.Length - offset) / 2];

			for (int i = 0; i < ret.Length; i++)
			{
				ret[i] = (byte)((ParseNibble(hex[offset]) << 4)
								 | ParseNibble(hex[offset + 1]));
				offset += 2;
			}
			return ret;
		}

		/**
		 * \brief Convert an hexadecimal representation of an array of bytes into its actual value
		 * 
		 * \deprecated Use HexToBytes()
		 */
		public static byte[] ParseHex(string hex)
		{
			return HexToBytes(hex);
		}

		/**
		 * \brief Convert an hexadecimal representation of one byte into its actual value
		 */
		public static byte HexToByte(string hex)
		{
			byte[] r = ParseHex(hex);
			if ((r == null) || (r.Length == 0))
				throw new ArgumentException("Invalid hex value: " + hex);

			return r[0];
		}

		/**
		 * \brief Convert an hexadecimal representation of one byte into its actual value
		 * 
		 * \deprecated Use HexToByte()
		 */
		public static byte ParseHexB(string hex)
		{
			return HexToByte(hex);
		}

		/**
		 * \brief Convert an hexadecimal representation of two bytes into their actual value
		 */
		public static UInt16 HexToWord(string hex)
		{
			byte[] r = ParseHex(hex);
			if ((r == null) || (r.Length == 0))
				throw new ArgumentException("Invalid hex value: " + hex);
			if (r.Length < 2)
				throw new ArgumentException("Hex value is too short for a WORD");

			UInt16 result = 0;
			for (int i=0; i<2; i++)
			{
				result <<= 8;
				result |= r[i];
			}
			return result;
		}

		/**
		 * \brief Convert an hexadecimal representation of two bytes into their actual value
		 * 
		 * \deprecated Use HexToWord()
		 */
		public static UInt16 ParseHexW(string hex)
		{
			return HexToWord(hex);
		}

		/**
		 * \brief Convert an hexadecimal representation of four bytes into their actual value
		 */
		public static UInt32 HexToDword(string hex)
		{
			byte[] r = ParseHex(hex);
			if ((r == null) || (r.Length == 0))
				throw new ArgumentException("Invalid hex value: " + hex);
			if (r.Length < 4)
				throw new ArgumentException("Hex value is too short for a DWORD");

			UInt32 result = 0;
			for (int i = 0; i < 4; i++)
			{
				result <<= 8;
				result |= r[i];
			}
			return result;
		}

		/**
		 * \brief Convert an hexadecimal representation of four bytes into their actual value
		 * 
		 * \deprecated Use HexToDword()
		 */
		public static UInt32 ParseHexDW(string hex)
		{
			return HexToDword(hex);
		}

		/**
		 * \brief Convert an hexadecimal representation of eight bytes into the actual value
		 */
		public static UInt64 HexToQword(string hex)
		{
			byte[] r = ParseHex(hex);
			if ((r == null) || (r.Length == 0))
				throw new ArgumentException("Invalid hex value: " + hex);
			if (r.Length < 8)
				throw new ArgumentException("Hex value is too short for a QWORD");

			UInt64 result = 0;
			for (int i = 0; i < 8; i++)
			{
				result <<= 8;
				result |= r[i];
			}
			return result;
		}

		/**
		 * \brief Convert an hexadecimal representation of eight bytes into the actual value
		 * 
		 * \deprecated Use HexToQword()
		 */
		public static UInt64 ParseHexUInt64(string hex)
		{
			return HexToQword(hex);
		}



		/**
		 * \brief Express one byte in hexadecimal. Digits 0xA to 0xF use uppercase ('A' to 'F'). 
		 */
		public static string ToHex(byte val)
		{
			string s = "";
			s += String.Format("{0:X2}", val);
			return s;
		}

		/**
		 * \brief Express one byte in hexadecimal. Digits 0xA to 0xF use lowercase ('a' to 'f'). 
		 */
		public static string tohex(byte val)
		{
			string s = "";
			s += String.Format("{0:x2}", val);
			return s;
		}

		/**
		 * \brief Express two bytes in hexadecimal. Digits 0xA to 0xF use uppercase ('A' to 'F'). 
		 */
		public static string ToHex(UInt16 val)
		{
			string s = "";
			s += String.Format("{0:X4}", val);
			return s;
		}

		/**
		 * \brief Express two bytes in hexadecimal. Digits 0xA to 0xF use lowercase ('a' to 'f'). 
		 */
		public static string tohex(UInt16 val)
		{
			string s = "";
			s += String.Format("{0:x4}", val);
			return s;
		}

		/**
		 * \brief Express four bytes in hexadecimal. Digits 0xA to 0xF use uppercase ('A' to 'F'). 
		 */
		public static string ToHex(UInt32 val)
		{
			string s = "";
			s += String.Format("{0:X8}", val);
			return s;
		}

		/**
		 * \brief Express four bytes in hexadecimal. Digits 0xA to 0xF use lowercase ('a' to 'f'). 
		 */
		public static string tohex(UInt32 val)
		{
			string s = "";
			s += String.Format("{0:x8}", val);
			return s;
		}

		/**
		 * \brief Express eight bytes in hexadecimal. Digits 0xA to 0xF use uppercase ('A' to 'F'). 
		 */
		public static string ToHex(UInt64 val)
		{
			string s = "";
			s += String.Format("{0:X16}", val);
			return s;
		}

		/**
		 * \brief Express eight bytes in hexadecimal. Digits 0xA to 0xF use lowercase ('a' to 'f'). 
		 */
		public static string tohex(UInt64 val)
		{
			string s = "";
			s += String.Format("{0:x16}", val);
			return s;
		}

		/**
		 * \brief Express an array of bytes into hexadecimal. Digits 0xA to 0xF use uppercase ('A' to 'F'). 
		 */
		public static string ToHex(byte[] val)
		{
			string s = "";
			if (val != null)
				for (int i=0; i<val.Length; i++)
					s += String.Format("{0:X2}", val[i]);
			return s;
		}

		/**
		 * \brief Express an array of bytes into hexadecimal, truncating at specified length. Digits 0xA to 0xF use uppercase ('A' to 'F'). 
		 */
		public static string ToHex(byte[] val, int length)
		{
			string s = "";
			for (int i = 0; (i < val.Length) && (i < length); i++)
				s += String.Format("{0:X2}", val[i]);
			return s;
		}

		/**
		 * \brief Express an array of bytes into hexadecimal, specifying offset and length. Digits 0xA to 0xF use uppercase ('A' to 'F'). 
		 */
		public static string ToHex(byte[] val, int offset, int length)
		{
			string s = "";
			for (int i = 0, j = offset; (i < length) && (j < val.Length); i++, j++)
				s += String.Format("{0:X2}", val[j]);
			return s;
		}

		/**
		 * \brief Express an array of bytes into hexadecimal. Digits 0xA to 0xF use lowercase ('a' to 'f'). 
		 */
		public static string tohex(byte[] val)
		{
			string s = "";
			if (val != null)
				for (int i = 0; i < val.Length; i++)
					s += String.Format("{0:x2}", val[i]);
			return s;
		}

		/**
		 * \brief Express an array of bytes into hexadecimal, truncating at specified length. Digits 0xA to 0xF use lowercase ('a' to 'f'). 
		 */
		public static string tohex(byte[] val, int length)
		{
			string s = "";
			for (int i = 0; (i < val.Length) && (i < length); i++)
				s += String.Format("{0:x2}", val[i]);
			return s;
		}

		/**
		 * \brief Express an array of bytes into hexadecimal, specifying offset and length. Digits 0xA to 0xF use lowercase ('a' to 'f'). 
		 */
		public static string tohex(byte[] val, int offset, int length)
		{
			string s = "";
			for (int i = 0, j = offset; (i < length) && (j < val.Length); i++, j++)
				s += String.Format("{0:x2}", val[j]);
			return s;
		}



		public static string ToHex_delim(byte[] val, string delim = ", ", string first_delim = "", string last_delim = "")
		{
			string s = first_delim;
			if (val != null)
				for (int i = 0; i < val.Length; i++)
				{
					if (s != first_delim) s += delim;
					s += String.Format("{0:X2}", val[i]);
				}
			s += last_delim;
			return s;
		}

		public static string ToHex_C(byte[] val)
		{
			return ToHex_delim(val, ", 0x", "0x");
		}

        public static string ToHex_C(byte[] val, int bytesPerLine)
        {
            string s = ToHex_delim(val, ", 0x", "0x");
            string[] e = s.Split(',');
            string r = "";
            for (int i=0; i<e.Length; i++)
            {
                if (i % bytesPerLine == 0)
                {
                    if (r != "")
                        r += "\n";
                }
                else
                    r += " ";
                r += e[i].Trim();
                if (i < e.Length - 1)
                    r += ",";
            }
            return r;
        }


        public static string ToHex_nice(byte[] val, string delim, string linePrefix, int bytesPerLine)
        {
            string s = "";
            int count = 0;
            if (val != null)
            {
                for (int i = 0; i < val.Length; i++)
                {
                    if (count == 0)
                        s += linePrefix;
                    s += String.Format("{0:X2}", val[i]);
                    count++;
                    if (count >= bytesPerLine)
                    {
                        count = 0;
                        s += "\n";
                    }
                    else if (i < val.Length - 1)
                    {
                        s += delim;
                    }
                }
            }
            return s;
        }

        public static string ToHex_nice(byte[] val)
        {
            return ToHex_nice(val, ":", "\t", 30);
        }



        public static string tohex_delim(byte[] val, string delim = ", ", string first_delim = "", string last_delim = "")
        {
            string s = first_delim;
            if (val != null)
                for (int i = 0; i < val.Length; i++)
                {
                    if (s != first_delim) s += delim;
                    s += String.Format("{0:x2}", val[i]);
                }
            s += last_delim;
            return s;
        }

        public static string tohex_C(byte[] val)
        {
            return tohex_delim(val, ", 0x", "0x");
        }

        public static string ToHex(byte[] val, uint length)
        {
            string s = "";
            for (uint i = 0; (i < val.Length) && (i < length); i++)
                s += String.Format("{0:X2}", val[i]);
            return s;
        }

        public static string tohex(byte[] val, uint length)
        {
            string s = "";
            for (uint i = 0; (i < val.Length) && (i < length); i++)
                s += String.Format("{0:x2}", val[i]);
            return s;
        }



        public static string ToHex(byte[] val, uint offset, uint length)
        {
            string s = "";
            for (uint i = 0, j = offset; (i < length) && (j < val.Length); i++, j++)
                s += String.Format("{0:X2}", val[j]);
            return s;
        }

        public static string tohex(byte[] val, uint offset, uint length)
        {
            string s = "";
            for (uint i = 0, j = offset; (i < length) && (j < val.Length); i++, j++)
                s += String.Format("{0:x2}", val[j]);
            return s;
        }





		
		

		public static Int32 ParseHexInt32(string hex)
		{
			byte[] b = HexToBytes(hex);
			Int32 r = 0;
			
			if (b != null)
			{
				for (int i=0; (i<b.Length) && (i<4); i++)
				{
					r <<= 8;
					r += b[i];
				}
			}
			
			return r;
		}

		public static UInt32 ParseHexUInt32(string hex)
		{
			byte[] b = HexToBytes(hex);
			UInt32 r = 0;
			
			if (b != null)
			{
				for (int i=0; (i<b.Length) && (i<4); i++)
				{
					r <<= 8;
					r += b[i];
				}
			}
			
			return r;
		}
		

		

		
		public static bool IsHex(string s)
		{
			if ((s.Length % 2) != 0)
				return false;
			
			for (int i=0; i<s.Length; i++)
				if (!IsHex(s[i]))
					return false;

			return true;
		}

		public static bool IsHex(string s, int wantLength)
		{
			if (s.Length != 2*wantLength)
				return false;
			
			return IsHex(s);
		}

		public static bool IsHex(char c)
		{
			if (c >= '0' && c <= '9')
			{
				return true;
			}
			if (c >= 'A' && c <= 'F')
			{
				return true;
			}
			if (c >= 'a' && c <= 'f')
			{
				return true;
			}
			return false;
		}
		
		static int ParseNibble(char c)
		{
			if (c >= '0' && c <= '9')
			{
				return c-'0';
			}
			if (c >= 'A' && c <= 'F')
			{
				return c-'A'+10;
			}
			if (c >= 'a' && c <= 'f')
			{
				return c-'a'+10;
			}
			throw new ArgumentException("Invalid hex digit: " + c);
		}
		
		
		static bool is_number(char c)
		{
			if ((c >= '0') && (c <= '9'))
			{
				return true;
			} else
			{
				return false;
			}
			
		}
		
		static byte hex_to_byte(char c)
		{
			if ((c >= '0') && (c <= '9')) return (byte) (c - '0');
			if ((c >= 'A') && (c <= 'F')) return (byte) (c - 'A' + 10);
			if ((c >= 'a') && (c <= 'f')) return (byte) (c - 'a' + 10);
			return 0xFF;
		}
		
		public static  byte[]  DecimalASCII_to_BYTEArray(string ascii_number)
		{
			UInt64 number = 0, result = 0;
			int i=0, offset = 0 ;
			byte[] tmp = new byte[255];
			
			for (i=0; i<ascii_number.Length - 1; i++)
			{
				if (is_number(ascii_number[i]))
				{
					number += hex_to_byte(ascii_number[i]);
					number *= 10;
				}  else
				{
					return null;
				}
			}
			number += hex_to_byte(ascii_number[ascii_number.Length-1]);
			
			while(true)
			{
				result = number / 256;
				if ( result > 0)
				{
					if (offset < 255)
					{
						tmp[offset++] = (byte) (number - (result * 256));
						number = result;
					} else
					{
						return null;
					}
					
				} else
				{
					if (offset < 255)
					{
						tmp[offset++] = (byte) number;
						break;
					} else
					{
						return null;
					}
				}
			}
			byte[] res_bytes = new byte[offset];
			for (i=0; i<offset ; i++)
				res_bytes[offset - 1 - i] = tmp[i];
			
			return res_bytes;
			
		}
		
		public static bool isValidHexString(string s)
		{
			return System.Text.RegularExpressions.Regex.IsMatch(s, @"\A\b[0-9a-fA-F]+\b\Z");
		}
		
		public static bool isByte(char c)
		{
			bool r = false;

			if ((c >= '0') && (c <= '9')) {
				r = true;
			} else if ((c >= 'A') && (c <= 'F')) {
				r = true;
			} else if ((c >= 'a') && (c <= 'f')) {
				r = true;
			}
			return r;
		}		
		
    	public static int SparseBitcount(int n)
    	{
			int count = 0;
			while (n != 0)
			{
				count++;
				n &= (n - 1);
			}
			return count;
		}

    	/// <summary>
    	/// "Converts" "HERVE" to "4845525645" 
    	/// </summary>
    	/// <param name="input"></param>
    	/// <returns></returns>
    	public static string AsciiToHex(string input)
    	{
    		input = input.Trim();
    		if(input.Length == 0)
    			return "";
			char[] charValues = input.ToCharArray();
			string hexOutput="";
			foreach (char _eachChar in charValues)
			{
				int value = Convert.ToInt32(_eachChar);
				hexOutput += String.Format("{0:X}", value);
			}
			return hexOutput;
    	}
	}	
}
