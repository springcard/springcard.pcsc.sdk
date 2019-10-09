using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace SpringCard.LibCs
{
	public class PhpDefineFile
	{
		private string FileName;
		
		private struct Entry
		{
			public string Name;
			public string Value;
			public char EnclosedBy;
		}

		private List<Entry> Entries;		
		
		public static PhpDefineFile OpenReadOnly(string FileName)
		{
			PhpDefineFile f = new PhpDefineFile();
			f.FileName = FileName;
			f.Load();
			return f;
		}

		private PhpDefineFile()
		{
			
		}
		
		public PhpDefineFile(string FileName)
		{
			this.FileName = FileName;
			this.Load();			
		}
		
		private int CountOccurrences(string s, char c)
		{
			return s.Split(c).Length - 1;
		}
		
		private void LoadLine(string DefineLine)
		{
			string[] kp = DefineLine.Split(new char[] { ',' }, 2);
			if (kp.Length != 2)
				return;
			Entry e = new Entry();
			/* Cleanup the name */
			e.Name = kp[0].Trim();
			if (!e.Name.StartsWith("define")) return;
			e.Name = e.Name.Substring(6);
			e.Name = e.Name.Trim();
			if (!e.Name.StartsWith("(")) return;
			e.Name = e.Name.Substring(1);
			e.Name = e.Name.Trim();
			if ((e.Name.StartsWith("'") && e.Name.EndsWith("'")) || (e.Name.StartsWith("\"") && e.Name.EndsWith("\""))) e.Name = e.Name.Substring(1, e.Name.Length - 2); else return;
			/* Cleanup the value */
			e.Value = kp[1].Trim();
			while (e.Value.EndsWith(")") && (CountOccurrences(e.Value, ')') > CountOccurrences(e.Value, '('))) e.Value = e.Value.Substring(0, e.Value.Length - 1);
			if (e.Value.StartsWith("'") && e.Value.EndsWith("'"))
			{
				e.EnclosedBy = '\'';
				e.Value = e.Value.Substring(1, e.Value.Length - 2);
			}
			else if (e.Value.StartsWith("\"") && e.Value.EndsWith("\""))
			{
				e.EnclosedBy = '\"';
				e.Value = e.Value.Substring(1, e.Value.Length - 2);
			}
			else
			{
				e.EnclosedBy = '\0';
			}
			Entries.Add(e);
		}
		
		public bool Load()
		{
			bool result = false;
			
			Entries = new List<Entry>();
			
			if (File.Exists(FileName))
			{
				try
				{
					string pattern = @"define\((.*?)\)";
					string cfgText = File.ReadAllText(FileName, Encoding.Default);          
          
					MatchCollection mc = Regex.Matches(cfgText, pattern);
					foreach (Match m in mc)
						LoadLine(m.Value);

					result = true;
				}
				catch
				{
				}				
			}
			
			return result;
		}

		public string ReadString(string Name, string Default)
		{
			foreach (Entry entry in Entries)
			{
				if (entry.Name == Name)
				{
					return entry.Value;
				}
			}
			
			return Default;
		}
		
		public string ReadString(string Name)
		{
			return ReadString(Name, "");
		}
		
		public int ReadInteger(string Name, int Default)
		{
			string s = ReadString(Name);
			int r;
			if (int.TryParse(s, out r))
				return r;
			return Default;
		}
		
		public int ReadInteger(string Name)
		{
			return ReadInteger(Name, 0);
		}
		
		public bool ReadBoolean(string Name, Boolean Default)
		{
			string s = ReadString(Name, null);
			if (string.IsNullOrEmpty(s))
				return Default;
			s = s.ToLower();
			if (s.Equals("false") || s.Equals("0"))
				return false;
			if (s.Equals("true"))
				return true;			
			int r;
			int.TryParse(s, out r);
			return (r != 0) ? true : false;
		}

		public bool ReadBoolean(string Name)
		{
			return ReadBoolean(Name, false);
		}
		
		public string[] Read(bool withAssociatedValues)
		{
			List<string> Names = new List<string>();
			
			foreach (Entry entry in Entries)
			{
				if (withAssociatedValues)
				{
					Names.Add(entry.Name + "=" + entry.Value);
					
				} else
				{
					Names.Add(entry.Name);
				}
			}
			
			return Names.ToArray();
		}
		
	}
	
}
