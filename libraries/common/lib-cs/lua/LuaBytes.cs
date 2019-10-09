/*
 * Created by SharpDevelop.
 * User: johann
 * Date: 28/03/2017
 * Time: 10:21
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Security.Cryptography;
using SpringCard.LibCs;
using SpringCard.LibCs.Crypto;
using SpringCard.PCSC;
using MoonSharp.Interpreter;

namespace PcscHce
{
	/// <summary>
	/// Description of LuaBytes.
	/// </summary>
	public class LuaBytes
	{
		CardBuffer buffer;
		bool Debug = true;
		
		public LuaBytes()
		{
			buffer = new CardBuffer();
		}

		public LuaBytes(byte[] value)
		{
			buffer = new CardBuffer(value);
		}
		
		public LuaBytes(string hexvalue)
		{
			buffer = new CardBuffer(hexvalue);
		}
		
		private DynValue ThisValue()
		{
			if (Debug)
				Logger.Debug(string.Format("LuaBytes:returning {0}", tostring()));
			return DynValue.NewString(buffer.AsString());
		}
		
		public string tostring()
		{
			if (Debug)
				Logger.Debug("LuaBytes:to_string");
			return buffer.AsString();
		}		
		
		public DynValue get_one(double offset = 0)
		{
			if (Debug)
				Logger.Debug("LuaBytes:get_one");
			byte result = buffer.GetByte((long) offset);
			return DynValue.NewNumber(result);
		}

		public DynValue get(double length = 0)
		{
			if (Debug)
				Logger.Debug("LuaBytes:get");
			return get(0, length);
		}
		
		public DynValue get(double offset, double length)
		{
			if (Debug)
				Logger.Debug("LuaBytes:get");
			LuaBytes result = new LuaBytes(buffer.GetBytes((long) offset, (long) length));			
			return result.ThisValue();
		}
		
		public DynValue after(double length = 0)
		{
			if (Debug)
				Logger.Debug("LuaBytes:after");
			long result_length = buffer.Length - (long) length;
			buffer = new CardBuffer(buffer.GetBytes((long) length, result_length));
			return ThisValue();
		}
		
		public DynValue empty()
		{
			if (Debug)
				Logger.Debug("LuaBytes:empty");
			return DynValue.NewBoolean((buffer.Length == 0) ? true : false);
		}
		
		public DynValue length()
		{
			if (Debug)
				Logger.Debug("LuaBytes:length");
			return DynValue.NewNumber(buffer.Length);
		}

		public DynValue get_length()
		{
			if (Debug)
				Logger.Debug("LuaBytes:get_length");
			return DynValue.NewNumber(buffer.Length);
		}
		
		public DynValue append(string hexvalue)
		{
			if (Debug)
				Logger.Debug(string.Format("LuaBytes:append {0} as hex", hexvalue));
			buffer.Append(new CardBuffer(hexvalue).GetBytes());
			return ThisValue();
		}		

		public DynValue append(LuaBytes value)
		{
			if (Debug)
				Logger.Debug(string.Format("LuaBytes:append {0} as bytes", value.tostring()));
			buffer.Append(value.buffer.GetBytes());
			return ThisValue();
		}

		public DynValue append(double value)
		{
			if (Debug)
				Logger.Debug(string.Format("LuaBytes:append {0:X02} as double", (byte) value));
			buffer.Append(new byte[] { (byte) value });
			return ThisValue();
		}
		
		public DynValue append_one(double value)
		{
			if (Debug)
				Logger.Debug(string.Format("LuaBytes:append_one {0:X02} as double", (byte) value));
			buffer.AppendOne((byte) value);
			return ThisValue();
		}
		
		public DynValue equal(LuaBytes value)
		{
			if (Debug)
				Logger.Debug("LuaBytes:is " + value.tostring() + " equal to " + tostring() + " ?");
			byte[] value1 = value.buffer.GetBytes();
			byte[] value2 = buffer.GetBytes();
			bool result = true;
			if (value1.Length != value2.Length)
			{
				result = false;
			} else
			{
				for (int i=0; i<value1.Length; i++)
				{
					if (value1[i] != value2[i])
					{
						result = false;
						break;
					}
				}
			}			
			return DynValue.NewBoolean(result);
		}

		public DynValue equal(string hexvalue)
		{
			if (Debug)
				Logger.Debug("LuaBytes:is " + hexvalue + " equal to " + tostring() + " ?");
			byte[] value1 = new CardBuffer(hexvalue).GetBytes();
			byte[] value2 = buffer.GetBytes();
			bool result = true;
			if (value1.Length != value2.Length)
			{
				result = false;
			} else
			{
				for (int i=0; i<value1.Length; i++)
				{
					if (value1[i] != value2[i])
					{
						result = false;
						break;
					}
				}
			}			
			return DynValue.NewBoolean(result);
		}
		
		public DynValue xor(LuaBytes value)
		{
			if (Debug)
				Logger.Debug("LuaBytes:xor " + tostring() + " with " + value.tostring());
			byte[] value1 = value.buffer.GetBytes();
			byte[] value2 = buffer.GetBytes();
			
			int length = value1.Length;
			if (value2.Length > length) length = value2.Length;
			
			byte[] result = new byte[length];
			
			for (int i=0; i<length; i++)
			{
				result[i] = 0;
				
				if (i < value1.Length)
					result[i] ^= value1[i];
				if (i < value2.Length)
					result[i] ^= value2[i];				
			}
			
			return create(result);
		}
		
		public DynValue sha256()
		{
			if (Debug)
				Logger.Debug("LuaBytes:sha256");
			
			byte[] result = SpringCard.LibCs.Crypto.SHA256.Hash(buffer.GetBytes());
			
			return create(result);
		}

		public DynValue sha256_128_even()
		{
			if (Debug)
				Logger.Debug("LuaBytes:sha256_128_even");
			
			byte[] result = SpringCard.LibCs.Crypto.SHA256.Hash128(buffer.GetBytes(), false);
			
			return create(result);
		}

		public DynValue sha256_128_odd()
		{
			if (Debug)
				Logger.Debug("LuaBytes:sha256_128_odd");
			
			byte[] result = SpringCard.LibCs.Crypto.SHA256.Hash128(buffer.GetBytes(), true);
			
			return create(result);
		}
		
		public DynValue eccdsa_sign(LuaBytes key)
		{
			if (Debug)
				Logger.Debug("LuaBytes:sign, key=" + key.tostring() + ", block=" + tostring());
			byte[] key_value = key.buffer.GetBytes();
			byte[] data_value = buffer.GetBytes();
			
			ECC ecc = new ECC();
			byte[] result = ecc.Sign(data_value, key_value);
			
			return create(result);
		}

		public DynValue eccdsa_verify(LuaBytes key, LuaBytes signature)
		{
			if (Debug)
				Logger.Debug("LuaBytes:verify, key=" + key.tostring() + ", signature=" + signature.tostring() + ", block=" + tostring());
			byte[] key_value = key.buffer.GetBytes();
			byte[] signature_value = signature.buffer.GetBytes();
			byte[] data_value = buffer.GetBytes();
			
			ECC ecc = new ECC();
			bool result = ecc.Verify(data_value, signature_value, key_value);
			
			return DynValue.NewBoolean(result);
		}
		
		public DynValue aes_encrypt(LuaBytes key)
		{
			if (Debug)
				Logger.Debug("LuaBytes:aes_encrypt, key=" + key.tostring() + ", block=" + tostring());
			byte[] key_value = key.buffer.GetBytes();
			byte[] data_value = buffer.GetBytes(16);
			
			RijndaelManaged aes = new RijndaelManaged();
			aes.Key = key_value;
			aes.Mode = CipherMode.ECB;
			aes.Padding = PaddingMode.None;
			ICryptoTransform aes_t = aes.CreateEncryptor();
			byte[] result = aes_t.TransformFinalBlock(data_value, 0, 16);
			
			return create(result); 
		}

		public DynValue aes_decrypt(LuaBytes key)
		{
			if (Debug)
				Logger.Debug("LuaBytes:aes_decrypt, key=" + key.tostring() + ", block=" + tostring());
			byte[] key_value = key.buffer.GetBytes();
			byte[] data_value = buffer.GetBytes(16);
			
			RijndaelManaged aes = new RijndaelManaged();
			aes.Key = key_value;
			aes.Mode = CipherMode.ECB;
			aes.Padding = PaddingMode.None;
			ICryptoTransform aes_t = aes.CreateDecryptor();
			byte[] result = aes_t.TransformFinalBlock(data_value, 0, 16);
			
			return create(result); 
		}
		
		public static DynValue create()
		{
			LuaBytes luabytes = new LuaBytes();
			return UserData.Create(luabytes, UserData.GetDescriptorForObject(luabytes));
		}

		private static DynValue create(byte[] value)
		{
			LuaBytes luabytes = new LuaBytes(value);
			return UserData.Create(luabytes, UserData.GetDescriptorForObject(luabytes));
		}
		
		public static DynValue create(string hexvalue)
		{
			LuaBytes luabytes = new LuaBytes(hexvalue);
			return UserData.Create(luabytes, UserData.GetDescriptorForObject(luabytes));
		}
		
		public static DynValue random(double length)
		{
			Random r = new Random();
			byte[] result = new byte[(int) length];
			r.NextBytes(result);
			LuaBytes luabytes = new LuaBytes(result);
			return UserData.Create(luabytes, UserData.GetDescriptorForObject(luabytes));
		}
		
		public static DynValue get_one(string hexvalue)
		{
			LuaBytes luabytes = new LuaBytes(hexvalue);
			
			return DynValue.NewTuple(
				luabytes.get_one(0),
				luabytes.after(1)
			);
		}

		public static DynValue get(string hexvalue, double count)
		{
			LuaBytes luabytes = new LuaBytes(hexvalue);
			return luabytes.get(0, count);
		}
		
		public static DynValue after(string hexvalue, double count)
		{
			LuaBytes luabytes = new LuaBytes(hexvalue);
			return luabytes.after(count);
		}		

		public static DynValue empty(string hexvalue)
		{
			LuaBytes luabytes = new LuaBytes(hexvalue);
			return luabytes.empty();
		}

		public static DynValue length(string hexvalue)
		{
			LuaBytes luabytes = new LuaBytes(hexvalue);
			return luabytes.length();
		}

		public static void InjectIntoScript(ref Script script)
		{
			UserData.RegisterType<LuaBytes>();
			DynValue obj = UserData.Create(new LuaBytes());
			script.Globals.Set("bytes", obj);
			script.DoString("log.print(log.DEBUG,\"Bytes library loaded\")");
		}		
	}
}
