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
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace SpringCard.LibCs
{
	/**
	 * \brief INI file object (pure .NET implementation, no dependency to Windows' libraries)
	 */
	public class IniFile
	{
		/**
		 * \brief Default text encoding, when not specified explicitely in the constructor. Default is ASCII.
		 */
		public static Encoding DefaultEncoding = Encoding.ASCII;

		private struct Entry
		{
			public string Section;
			public string Name;
			public string Value;
			public bool EqualSign;
		}

		/**
		 * \brief The list of sections in the INI file
		 */
		public List<string> Sections { get; private set; }
		private List<Entry> Entries;

		private bool _ReadOnly = false;
		private bool _AutoSave = true;
		private string _FileName;
		private Encoding _FileEncoding = DefaultEncoding;

		/**
		 * \brief Create an instance of the IniFile object for the given INI file. The instance is read only.
		 */
		public static IniFile OpenReadOnly(string FileName)
		{
			IniFile f = new IniFile();
			f._ReadOnly = true;
			f._AutoSave = false;
			f._FileName = FileName;
			f.Load();
			return f;
		}

		/**
		 * \brief Create an instance of the IniFile object for the given INI file. The instance is read only.
		 */
		public static IniFile OpenReadOnly(string FileName, Encoding FileEncoding)
		{
			IniFile f = new IniFile();
			f._ReadOnly = true;
			f._AutoSave = false;
			f._FileName = FileName;
			f._FileEncoding = FileEncoding;
			f.Load();
			return f;
		}

		/**
		 * \brief Create an instance of the IniFile object for the given INI file. The instance is read/write.
		 *
		 * If the AutoSave parameter is set to false, Save() must be called explicitly, otherwise the write operations are lost.
		 */
		public static IniFile OpenReadWrite(string FileName, bool AutoSave = false)
		{
			IniFile f = new IniFile();
			f._ReadOnly = false;
			f._AutoSave = AutoSave;
			f._FileName = FileName;
			f.Load();
			return f;
		}

		/**
		 * \brief Create an instance of the IniFile object for the given INI file. The instance is read/write.
		 *
		 * If the AutoSave parameter is set to false, Save() must be called explicitly, otherwise the write operations are lost.
		 */
		public static IniFile OpenReadWrite(string FileName, Encoding FileEncoding, bool AutoSave = false)
		{
			IniFile f = new IniFile();
			f._ReadOnly = false;
			f._AutoSave = AutoSave;
			f._FileName = FileName;
			f._FileEncoding = FileEncoding;
			f.Load();
			return f;
		}

		/**
		 * \brief Create an instance of the IniFile object from a content already loaded in memory
		 */
		public static IniFile CreateFromText(string Text)
		{
			string[] Lines = Text.Split('\n');
			return CreateFromLines(Lines);
		}

		/**
		 * \brief Create an instance of the IniFile object from a content already loaded in memory
		 */		
		public static IniFile CreateFromLines(string[] Lines)
		{
			IniFile f = new IniFile();
			f._ReadOnly = false;
			f._AutoSave = false;
			f._FileName = null;
			f.Populate(Lines);
			return f;
		}

		private IniFile()
		{

		}

		/**
		 * \brief Create an instance of the IniFile object from the given INI file. The instance is read/write, with AutoSave set to true. 
		 *
		 * \deprecated Use either OpenReadOnly() or OpenReadWrite()
		 */		
		public IniFile(string FileName)
		{
			_ReadOnly = false;
			_AutoSave = true;
			_FileName = FileName;
			Load();
		}

		/**
		 * \brief Create an instance of the IniFile object from the given INI file. The instance is read/write, using the specified AutoSave value. 
		 *
		 * \deprecated Use OpenReadWrite()
		 */				
		public IniFile(string FileName, bool AutoSave)
		{
			_ReadOnly = false;
			_AutoSave = AutoSave;
			_FileName = FileName;
			Load();
		}

		/**
		 * \brief Create an instance of the IniFile object from the given INI file. The type of the instance depends on the parameters.
		 *
		 * \deprecated Use either OpenReadOnly() or OpenReadWrite()
		 */		
		public IniFile(string FileName, bool AutoSave, bool ReadOnly)
		{
			_ReadOnly = ReadOnly;
			_AutoSave = AutoSave;
			_FileName = FileName;
			Load();
		}

		private bool Populate(string[] Lines)
		{
			Sections = new List<string>();
			Entries = new List<Entry>();

			string strSection = null;
			foreach (string Line in Lines)
			{
				string strLine = Line.Trim();

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
					}
					else
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
			}
			return true;
		}

		/**
		 * \brief (Re)load the content from the file
		 */		
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
					iniFile = new StreamReader(_FileName, _FileEncoding);

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
							}
							else
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

			}
			else
			{
				Result = false;
			}

			return Result;
		}

		/**
		 * \brief Save the content to another INI file
		 */
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

		/**
		 * \brief Save the content to another INI file
		 */
		public bool SaveTo(string FileName, Encoding FileEncoding)
		{
			string oldFileName = _FileName;
			Encoding oldEncoding = _FileEncoding;
			_FileName = FileName;
			_FileEncoding = FileEncoding;
			if (!Save())
			{
				_FileName = oldFileName;
				_FileEncoding = oldEncoding;
				return false;
			}
			return true;
		}

		/**
		 * \brief Save the file. Return false in case of error
		 */
		public bool Save()
		{
			try
			{
				SaveEx();
				return true;
			}
			catch (Exception e)
			{
				Logger.Trace(e.Message);
				return false;
			}
		}

		/**
		 * \brief Save the file. Throw an exception in case of error
		 */
		public void SaveEx()
		{
			if (_ReadOnly)
				throw new UnauthorizedAccessException();

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

			TextWriter iniFile = new StreamWriter(_FileName, false, _FileEncoding);
			iniFile.Write(strToSave);
			iniFile.Close();
			iniFile.Dispose();
		}

		/**
		 * \brief Get the list of sections in the INI file
		 *
		 * \deprecated See Sections
		 */		
		public List<string> GetSections()
		{
			return Sections;
		}

		/**
		 * \brief Read a string entry from the INI file
		 */		
		public string ReadString(string Section, string Name, string Default = "")
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

		/**
		 * \brief Read an integer entry from the INI file
		 */		
		public int ReadInteger(string Section, string Name, int Default = 0)
		{
			string s = ReadString(Section, Name);
			int r;
			if (int.TryParse(s, out r))
				return r;
			return Default;
		}

		/**
		 * \brief Read a boolean entry from the INI file
		 */		
		public bool ReadBoolean(string Section, string Name, bool Default = false)
		{
			bool valid;
			bool result = StrUtils.ReadBoolean(ReadString(Section, Name, null), out valid);
			if (!valid)
				return Default;
			return result;
		}

		/**
		 * \brief Remove an entry from the INI file
		 *
		 * \deprecated See Remove()
		 */				
		public bool Erase(string Section, string Name)
		{
			return Remove(Section, Name);
		}
		
		/**
		 * \brief Remove an entry from the INI file
		 */				
		public bool Remove(string Section, string Name)
		{
			if (_ReadOnly)
				return false;

			for (int i = 0; i < Entries.Count; i++)
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
				for (int i = 0; i < Sections.Count; i++)
				{
					if (Sections[i] == newEntry.Section)
					{
						isNewSection = false;
						break;
					}
				}
			}
			else
			{
				isNewSection = false;
			}

			for (int i = 0; i < Entries.Count; i++)
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

		/**
		 * \brief Write an empty entry into the INI file
		 */						
		public bool WriteName(string Section, string Name)
		{
			Entry newEntry = new Entry();
			newEntry.Section = Section;
			newEntry.Name = Name;
			newEntry.Value = null;
			newEntry.EqualSign = false;

			return WriteEntry(newEntry);
		}

		/**
		 * \brief Write a string entry into the INI file
		 */		
		public bool WriteString(string Section, string Name, string Value)
		{
			Entry newEntry = new Entry();
			newEntry.Section = Section;
			newEntry.Name = Name;
			newEntry.Value = Value;
			newEntry.EqualSign = true;

			return WriteEntry(newEntry);
		}

		/**
		 * \brief Write an integer entry into the INI file
		 */		
		public bool WriteInteger(string Section, string Name, int Value)
		{
			return WriteString(Section, Name, String.Format("{0}", Value));
		}

		/**
		 * \brief Write a boolean entry into the INI file
		 */		
		public bool WriteBoolean(string Section, string Name, bool value)
		{
			int i = value ? 1 : 0;
			return WriteInteger(Section, Name, i);
		}


		/**
		 * \brief Return all the entries from a given section
		 *
		 * If withAssociatedValues is set to true, the entries are returned as name=value, otherwise, only the name is returned
		 */		
		public string[] ReadSection(string Section, bool withAssociatedValues = false)
		{
			List<string> Names = new List<string>();

			foreach (Entry entry in Entries)
			{
				if (entry.Section == Section)
				{
					if (withAssociatedValues)
					{
						Names.Add(entry.Name + "=" + entry.Value);

					}
					else
					{
						Names.Add(entry.Name);
					}
				}
			}

			return Names.ToArray();
		}

	}

}
