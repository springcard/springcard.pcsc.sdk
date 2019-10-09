/**
 * \cond
 */
using System;
using System.Collections.Generic;

namespace SpringCard.LibCs
{	
	public class KeyValueEntry
	{
		public enum ValueType
		{
			String,
			SignedInteger,
			UnsignedInteger,
			DateTime
		}
		
		public ValueType Type;
		public string Name;
		public object Value;
	}
	
	public class KeyValueList
	{
		private List<KeyValueEntry> entries;
		
		public int Count { get { return entries.Count; } }
		
		
		public KeyValueList()
		{
			entries = new List<KeyValueEntry>();
		}
		
		public List<KeyValueEntry> Entries
		{
			get
			{
				return entries;
			}
		}
		
		public void Add(string Name, string Value)
		{
			KeyValueEntry e = new KeyValueEntry();
			e.Type = KeyValueEntry.ValueType.String;
			e.Name = Name;
			e.Value = Value;
			entries.Add(e);
		}

		public void Add(string Name, ulong Value)
		{
			KeyValueEntry e = new KeyValueEntry();
			e.Type = KeyValueEntry.ValueType.UnsignedInteger;
			e.Name = Name;
			e.Value = Value;
			entries.Add(e);
		}

		public void Add(string Name, long Value)
		{
			KeyValueEntry e = new KeyValueEntry();
			e.Type = KeyValueEntry.ValueType.SignedInteger;
			e.Name = Name;
			e.Value = Value;
			entries.Add(e);
		}

		public void Add(string Name, DateTime Value)
		{
			KeyValueEntry e = new KeyValueEntry();
			e.Type = KeyValueEntry.ValueType.DateTime;
			e.Name = Name;
			e.Value = Value;
			entries.Add(e);
		}
		
		public void Add(KeyValueList SubList)
		{
			for (int i=0; i<SubList.entries.Count; i++)
				entries.Add(SubList.entries[i]);
		}

		public void Add(string PrefixName, KeyValueList SubList)
		{
			for (int i=0; i<SubList.entries.Count; i++)
			{
				KeyValueEntry e = new KeyValueEntry();
				e.Type = SubList.entries[i].Type;
				e.Name = PrefixName + SubList.entries[i].Name;
				e.Value = SubList.entries[i].Value;
				entries.Add(e);
			}
		}
		
		public string JoinNames(string separator = ", ", string prefix = "", string suffix ="")
		{
			string result = "";
			
			for (int i=0; i<entries.Count; i++)
			{
				if (i>0)
					result += separator;
				result += prefix + entries[i].Name + suffix;
			}
			
			return result;
		}
		
		public string JoinValues(string separator = ", ", string prefix = "", string suffix ="")
		{
			string result = "";
			
			for (int i=0; i<entries.Count; i++)
			{
				if (i>0)
					result += separator;
				result += prefix + entries[i].Value + suffix;
			}
			
			return result;
		}

		public string JoinNamesNames(string separator, string prefix1, string prefix2)
		{
			string result = "";
			
			for (int i=0; i<entries.Count; i++)
			{
				if (i>0)
					result += separator;
				result += prefix1 + entries[i].Name;
				result += prefix2 + entries[i].Name;
			}
			
			return result;
		}
		
		public string JoinNamesNames(string mainSeparator, string subSeparator, string prefix1, string suffix1, string prefix2, string suffix2)
		{
			string result = "";
			
			for (int i=0; i<entries.Count; i++)
			{
				if (i>0)
					result += mainSeparator;
				result += prefix1 + entries[i].Name + suffix1;
				result += subSeparator;
				result += prefix2 + entries[i].Name + suffix2;
			}
			
			return result;
		}
		
		public string JoinNamesValues(string mainSeparator, string subSeparator, string prefix1, string suffix1, string prefix2, string suffix2)
		{
			string result = "";
			
			for (int i=0; i<entries.Count; i++)
			{
				if (i>0)
					result += mainSeparator;
				result += prefix1 + entries[i].Name + suffix1;
				result += subSeparator;
				result += prefix2 + entries[i].Value + suffix2;
			}
			
			return result;
		}
		
		public List<KeyValueEntry> AsList()
		{
			return entries;
		}
		
		public string Find (string key)
		{
			string result = "";
			
			foreach(KeyValueEntry entry in entries)
			{
				if(entry.Name.Equals(key))
					result = (string)entry.Value;
			}
			return result;
		}
		
		public bool TryGetUshort(string key, out ushort result)
		{
			return ushort.TryParse(Find(key), out result);
		}
		
		public bool TryGetUInt16(string key, out UInt16 result)
		{
			return UInt16.TryParse(Find(key), out result);
		}
		
		public bool TryGetUInt32(string key, out UInt32 result)
		{
			return UInt32.TryParse(Find(key), out result);
		}
		
		public bool TryGetInt(string key, out int result)
		{
			return int.TryParse(Find(key), out result);
		}
		
		public bool TryGetByte(string key, out byte result)
		{
			return byte.TryParse(Find(key), out result);
		}
				
		/* Used only to get UID for now, so dots are erased */
		public bool TryGetByteArray(string key, out byte[] result)
		{
			try
			{
				result = BinConvert.ParseHex(Find(key).Replace(".", string.Empty));
			}
			catch
			{
				result = null;
				return false;
			}
			return true;
		}
		
		public bool TryGetDateTime(string key, out DateTime result)
		{
			return DateTime.TryParse(Find(key), out result);
		}
	}
}
/**
 * \endcond
 */
