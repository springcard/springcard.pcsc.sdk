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
	public class IniFile
	{
		private struct Entry
		{
			public string Section;
			public string Name;
			public string Value;
			public bool EqualSign;
		}
		
		private List<string> Sections;
		private List<Entry> Entries;
		
		private bool _ReadOnly = false;
		private bool _AutoSave = true;
		private string _FileName;
		
		public static IniFile OpenReadOnly(string FileName)
		{
			IniFile f = new IniFile();
			f._ReadOnly = true;
			f._AutoSave = false;
			f._FileName = FileName;
			f.Load();
			return f;
		}
				
		public static IniFile OpenReadWrite(string FileName, bool AutoSave = false)
		{
			IniFile f = new IniFile();
			f._ReadOnly = false;
			f._AutoSave = AutoSave;
			f._FileName = FileName;
			f.Load();
			return f;
		}		
		
		private IniFile()
		{
			
		}
		
		public IniFile(string FileName)
		{
			_ReadOnly = false;
			_AutoSave = true;
			_FileName = FileName;
			Load();			
		}

		public IniFile(string FileName, bool AutoSave)
		{
			_ReadOnly = false;
			_AutoSave = AutoSave;
			_FileName = FileName;
			Load();			
		}

		public IniFile(string FileName, bool AutoSave, bool ReadOnly)
		{
			_ReadOnly = ReadOnly;
			_AutoSave = AutoSave;
			_FileName = FileName;
			Load();
		}
		
		public bool Load()
		{
			bool Result = true;
			TextReader iniFile = null;
			string strLine = null;
			string strSection = null;
			
			Sections = new List<string>();
			Entries = new List<Entry>();
			
			if (File.Exists(_FileName))
			{
				try
				{
					iniFile = new StreamReader(_FileName, Encoding.Default);
					
					strLine = iniFile.ReadLine();
					
					while (strLine != null)
					{
						strLine = strLine.Trim();
						
						if (strLine.Contains(";"))
							strLine = strLine.Substring(0, strLine.IndexOf(';'));
						
						if (strLine != "")
						{
							if (strLine.StartsWith("[") && strLine.EndsWith("]"))
							{
								/* Start of section */
								strSection = strLine.Substring(1, strLine.Length - 2);
								
								/* Add to sections */
								Sections.Add(strSection);
							}	else
							{
								Entry e = new Entry();
								e.Section = strSection;
								
								string[] t = strLine.Split(new char[] { '=' }, 2);
								e.Name = t[0];
								e.Value = null;
								if (t.Length > 1)
									e.Value = t[1];
								
								Entries.Add(e);
							}
						}
						
						strLine = iniFile.ReadLine();
					}
					iniFile.Close();
					iniFile.Dispose();
				}
				catch
				{
					Result = false;
				}
				finally
				{
					if (iniFile != null)
					{
						iniFile.Close();
						iniFile.Dispose();
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
				if ((entry.Section == null) || (entry.Section == ""))
				{
					if (entry.Value == null)
						strToSave += entry.Name;
					else
						strToSave += entry.Name + "=" + entry.Value;
					
					strToSave += "\r\n";
				}
			}
			
			foreach (string section in Sections)
			{
				strToSave += "[" + section + "]" + "\r\n";
				
				foreach (Entry entry in Entries)
				{
					if (entry.Section == section)
					{
						if (entry.Value == null)
							strToSave += entry.Name;
						else
							strToSave += entry.Name + "=" + entry.Value;
						
						strToSave += "\r\n";
					}
				}
				
				strToSave += "\r\n";
			}
			
			try
			{
				TextWriter iniFile = new StreamWriter(_FileName);
				iniFile.Write(strToSave);
				iniFile.Close();
				iniFile.Dispose();
				ErrorMsg = "";
				return true;
			}
			catch (Exception e)
			{
				ErrorMsg = e.Message;
				return false;
			}
		}

		public string ReadString(string Section, string Name, string Default)
		{
			foreach (Entry entry in Entries)
			{
				if (entry.Section == Section)
				{
					if (entry.Name == Name)
					{
						return entry.Value;
					}
				}
			}
			
			return Default;
		}
		
		public string ReadString(string Section, string Name)
		{
			return ReadString(Section, Name, "");
		}
		
		public int ReadInteger(string Section, string Name, int Default)
		{
			string s = ReadString(Section, Name);
			int r;
			if (int.TryParse(s, out r))
				return r;
			return Default;
		}
		
		public int ReadInteger(string Section, string Name)
		{
			return ReadInteger(Section, Name, 0);
		}
		
		public bool ReadBoolean(string Section, string Name, Boolean Default)
		{
			int i;
			if (Default)
				i = ReadInteger(Section, Name, 1);
			else
				i = ReadInteger(Section, Name, 0);
			return (i != 0) ? true : false;
		}

		public bool ReadBoolean(string Section, string Name)
		{
			return ReadBoolean(Section, Name, false);
		}
		
		public bool Erase(string Section, string Name)
		{
			if (_ReadOnly)
				return false;
			
			for (int i=0; i<Entries.Count; i++)
			{
				Entry oldEntry = Entries[i];
				if (oldEntry.Section == Section)
				{
					if (oldEntry.Name == Name)
					{
						Entries.Remove(oldEntry);
						break;
					}
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
			bool isNewSection = true;
		
			if (!String.IsNullOrEmpty(newEntry.Section))
			{
				for (int i=0; i<Sections.Count; i++)
				{
					if (Sections[i] == newEntry.Section)
					{
						isNewSection = false;
						break;
					}
				}
			} else
			{
				isNewSection = false;
			}
			
			for (int i=0; i<Entries.Count; i++)
			{
				Entry oldEntry = Entries[i];
				if (oldEntry.Section == newEntry.Section)
				{
					if (oldEntry.Name == newEntry.Name)
					{
						Entries[i] = newEntry;
						isNewEntry = false;
						break;
					}
				}
			}

			if (isNewSection)
				Sections.Add(newEntry.Section);
			
			if (isNewEntry)
				Entries.Add(newEntry);
			
			if (_AutoSave)
				return Save();
			
			return true;			
		}
		
		public bool WriteName(string Section, string Name)
		{
			Entry newEntry = new Entry();
			newEntry.Section = Section;
			newEntry.Name = Name;
			newEntry.Value = null;
			newEntry.EqualSign = false; 
			
			return WriteEntry(newEntry);
		}
		
		public bool WriteString(string Section, string Name, string Value)
		{
			Entry newEntry = new Entry();
			newEntry.Section = Section;
			newEntry.Name = Name;
			newEntry.Value = Value;
			newEntry.EqualSign = true;
			
			return WriteEntry(newEntry);
		}
		
		public bool WriteInteger(string Section, string Name, int Value)
		{
			return WriteString(Section, Name, String.Format("{0}", Value));
		}
		
		public bool WriteBoolean(string Section, string Name, bool value)
		{
			int i = value ? 1 : 0;
			return WriteInteger(Section, Name, i);
		}
		
		public string[] ReadSection(string Section)
		{
			return ReadSection(Section, false);
		}
		
		public string[] ReadSection(string Section, bool withAssociatedValues)
		{
			List<string> Names = new List<string>();
			
			foreach (Entry entry in Entries)
			{
				if (entry.Section == Section)
				{
					if (withAssociatedValues)
					{
						Names.Add(entry.Name + "=" + entry.Value);
						
					} else
					{
						Names.Add(entry.Name);
					}
				}
			}
			
			return Names.ToArray();
		}
		
	}
	
}
