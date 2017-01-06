using System;
using System.Linq;


namespace SpringCard.LibCs
{
	public class BinConvert
	{
		public static byte[] StringToByteArray(string str)
		{
			char[] s_chars = str.ToCharArray();
			byte[] s_bytes = new byte[s_chars.Length];
			for (int i=0; i<s_chars.Length; i++)
				s_bytes[i] = (byte) s_chars[i];
			return s_bytes;
		}
		
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
		
		public static string ByteArrayToStringAscii(byte[] value)
		{
			return value != null ? string.Concat(value.Select(b => Convert.ToString(b, 2))) : "";
		}
		
		
		public static string ToHex(byte[] val)
		{
			string s = "";
			if (val != null)
				for (int i=0; i<val.Length; i++)
					s += String.Format("{0:X2}", val[i]);
			return s;
		}

		public static string ToHex(byte[] val, int length)
		{
			string s = "";				
			for (int i=0; (i<val.Length) && (i<length); i++)
				s += String.Format("{0:X2}", val[i]);
			return s;
		}
		
		public static string ToHex(byte[] val, int offset, int length)
		{
			string s = "";				
			for (int i=0, j=offset; (i<length) && (j<val.Length); i++, j++)
				s += String.Format("{0:X2}", val[j]);
			return s;				
		}
		
		public static string ToHex(byte val)
		{
			string s = "";
			s += String.Format("{0:X2}", val);
			return s;
		}
		
		public static string ToHex(ushort val)
		{
			string s = "";
			s += String.Format("{0:X4}", val);
			return s;
		}
		
		public static string ToHex(uint val)
		{
			string s = "";
			s += String.Format("{0:X8}", val);
			return s;
		}

		public static string ToHex(ulong val)
		{
			string s = "";
			s += String.Format("{0:X16}", val);
			return s;
		}
		
		public static byte ParseHexB(string hex)
		{
			return HexToByte(hex);
		}
		
		public static ushort ParseHexW(string hex)
		{
			byte[] b = HexToBytes(hex);
			ushort r = 0;
			
			if (b != null)
			{
				for (int i=0; (i<b.Length) && (i<2); i++)
				{
					r <<= 8;
					r += b[i];
				}
			}
			
			return r;
		}
		
		public static uint ParseHexDW(string hex)
		{
			byte[] b = HexToBytes(hex);
			uint r = 0;
			
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
		
		/* SHU : Test - start */
		public static UInt64 ParseHexUInt64(string hex)
		{
			byte[] b = HexToBytes(hex);
			UInt64 r = 0;
			
			if (b != null)
			{
				for (int i=0; (i<b.Length) && (i<8); i++)
				{
					r <<= 8;
					r += b[i];
				}
			}
			
			return r;
		}
		/* SHU : Test - end */
		
		public static byte[] ParseHex(string hex)
		{
			return HexToBytes(hex);
		}

		public static int ParseInt(string dec)
		{
			int result;
			if (!int.TryParse(dec, out result))
				result = 0;
			return result;
		}

		public static UInt32 ParseDW(string dec)
		{
			UInt32 result;
			if (!UInt32.TryParse(dec, out result))
				result = 0;
			return result;
		}
		
		public static byte[] HexToBytes(string hex)
		{
			int offset = hex.StartsWith("0x") ? 2 : 0;
			if ((hex.Length % 2) != 0)
				throw new ArgumentException(Translatable.GetTranslation("InvalidLength", "Invalid length: ") + hex.Length);
			
			byte[] ret = new byte[(hex.Length-offset)/2];
			
			for (int i=0; i < ret.Length; i++)
			{
				ret[i] = (byte) ((ParseNibble(hex[offset]) << 4)
				                 | ParseNibble(hex[offset+1]));
				offset += 2;
			}
			return ret;
		}

		public static byte HexToByte(string hex)
		{
			byte[] r = ParseHex(hex);
			if ((r == null) || (r.Length == 0))
				throw new ArgumentException(Translatable.GetTranslation("InvalidHexByte", "Invalid hex byte: ") + hex);

			return r[0];
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
			throw new ArgumentException(Translatable.GetTranslation("InvalidHexDigit", "Invalid hex digit: ") + c);
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
	}
	
}
