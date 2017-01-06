using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Microsoft.Win32;

namespace SpringCard.LibCs
{
	public class CfgFile
	{
		private struct Entry
		{
			public string Name;
			public string Value;
			public bool EqualSign;
		}
		
		private List<Entry> Entries;
		
		private bool _ReadOnly = false;
		private bool _AutoSave = true;
		private string _FileName;
		
		public static CfgFile OpenReadOnly(string FileName)
		{
			CfgFile f = new CfgFile();
			f._ReadOnly = true;
			f._AutoSave = false;
			f._FileName = FileName;
			f.Load();
			return f;
		}
				
		public static CfgFile OpenReadWrite(string FileName, bool AutoSave = false)
		{
			CfgFile f = new CfgFile();
			f._ReadOnly = false;
			f._AutoSave = AutoSave;
			f._FileName = FileName;
			f.Load();
			return f;
		}
		
		private CfgFile()
		{
			
		}
		
		public CfgFile(string FileName)
		{
			this._ReadOnly = true;
			this._AutoSave = false;
			this._FileName = FileName;
			this.Load();			
		}
		
		public CfgFile(string FileName, bool AutoSave)
		{
			_ReadOnly = false;
			_AutoSave = AutoSave;
			_FileName = FileName;
			Load();			
		}

		public CfgFile(string FileName, bool AutoSave, bool ReadOnly)
		{
			_ReadOnly = ReadOnly;
			_AutoSave = AutoSave;
			_FileName = FileName;
			Load();
		}		
				
		public bool Load()
		{
			bool Result = true;
			TextReader cfgFile = null;
			string strLine = null;
			
			Entries = new List<Entry>();
			
			if (File.Exists(_FileName))
			{
				try
				{
					cfgFile = new StreamReader(_FileName, Encoding.Default);
					
					strLine = cfgFile.ReadLine();
					
					while (strLine != null)
					{
						strLine = strLine.Trim();
						
						if (strLine.Contains(";"))
							strLine = strLine.Substring(0, strLine.IndexOf(';'));
						
						Entry e = new Entry();
						
						string[] t = strLine.Split(new char[] { '=' }, 2);
						e.Name = t[0];
						e.Value = null;
						if (t.Length > 1)
							e.Value = t[1];
						
						Entries.Add(e);
						
						strLine = cfgFile.ReadLine();
					}
					cfgFile.Close();
					cfgFile.Dispose();
				}
				catch
				{
					Result = false;
				}
				finally
				{
					if (cfgFile != null)
					{
						cfgFile.Close();
						cfgFile.Dispose();
					}
				}
				
			}	else
			{
				Result = false;
			}
			
			return Result;
		}

		public bool SaveTo(string FileName)
		{
			string oldFileName = _FileName;
			_FileName = FileName;
			if (!Save())
			{
				_FileName = oldFileName;
				return false;
			}
			return true;
		}
		
		public bool Save()
		{
			string e;
			if (!Save(out e))
			{
				Logger.Trace(e);
				return false;
			}
			return true;
		}
		
		public bool Save(out string ErrorMsg)
		{
			if (_ReadOnly)
			{
				ErrorMsg = Translatable.GetTranslation("FileOpenReadOnlyMode", "the file has been opened in read-only mode");
				return false;
			}
			
			string strToSave = "";
			
			foreach (Entry entry in Entries)
			{
				if (entry.Value == null)
					strToSave += entry.Name;
				else
					strToSave += entry.Name + "=" + entry.Value;
				
				strToSave += "\r\n";
			}
					
			try
			{
				TextWriter cfgFile = new StreamWriter(_FileName);
				cfgFile.Write(strToSave);
				cfgFile.Close();
				cfgFile.Dispose();
				ErrorMsg = "";
				return true;
			}
			catch (Exception e)
			{
				ErrorMsg = e.Message;
				return false;
			}
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
		
		public bool Erase(string Name)
		{
			if (_ReadOnly)
				return false;
			
			for (int i=0; i<Entries.Count; i++)
			{
				Entry oldEntry = Entries[i];
				if (oldEntry.Name == Name)
				{
					Entries.Remove(oldEntry);
					break;
				}
			}
			
			if (_AutoSave)
				return Save();
			
			return true;
			
		}
		
		private bool WriteEntry(Entry newEntry)
		{
			if (_ReadOnly)
				return false;
		
			bool isNewEntry = true;
		
			for (int i=0; i<Entries.Count; i++)
			{
				Entry oldEntry = Entries[i];
				if (oldEntry.Name == newEntry.Name)
				{
					Entries[i] = newEntry;
					isNewEntry = false;
					break;
				}
			}

			if (isNewEntry)
				Entries.Add(newEntry);
			
			if (_AutoSave)
				return Save();
			
			return true;			
		}
		
		public bool WriteName(string Name)
		{
			Entry newEntry = new Entry();
			newEntry.Name = Name;
			newEntry.Value = null;
			newEntry.EqualSign = false; 
			
			return WriteEntry(newEntry);
		}
		
		public bool WriteString(string Name, string Value)
		{
			Entry newEntry = new Entry();
			newEntry.Name = Name;
			newEntry.Value = Value;
			newEntry.EqualSign = true;
			
			return WriteEntry(newEntry);
		}
		
		public bool WriteInteger(string Name, int Value)
		{
			return WriteString(Name, String.Format("{0}", Value));
		}
		
		public bool WriteBoolean(string Name, bool value)
		{
			int i = value ? 1 : 0;
			return WriteInteger(Name, i);
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
