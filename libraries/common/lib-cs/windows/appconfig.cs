/**
 *
 * \ingroup Windows
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
using Microsoft.Win32;
using System.Windows.Forms;

namespace SpringCard.LibCs.Windows
{
	/**
	 * \brief Utility to store application's configuration in registry
	 */
	public class AppConfig
	{
		public const int RememberFileCount = 5;
		public const int RememberKeyCount = 10;

		const string sMRUFiles = "MRUFiles";
		const string sAspect = "Aspect";
		const string sSettings = "Settings";

		/**
		 * \brief Add a new filename to the list of MRU (most recently used)
		 */
		public static void RememberFile(string FileName)
		{
			string[] recents = new string[RememberFileCount];

			for (int i=0; i<RememberFileCount; i++)
			{
				recents[i] = ReadSectionSettingString(sMRUFiles, i.ToString());
			}

			int pos = RememberFileCount - 1;
			for (int i=0 ; i<RememberFileCount ; i++)
			{
				if ((recents[i]!=null) && recents[i].Equals(FileName))
					pos = i;
			}

			WriteSectionSettingString(sMRUFiles, "0", FileName);
			for (int i=1; i<=pos; i++)
			{
				if (recents[i-1] != null)
					WriteSectionSettingString(sMRUFiles, i.ToString(), recents[i-1]);
			}
		}

		/**
		 * \brief Remember the last directory used to open or save a file
		 */
		public static void RememberDirectory(string PathName)
		{
			WriteSectionSettingString(sMRUFiles, "path", PathName);
		}

		/**
		 * \brief Retrieve the last directory used to open or save a file
		 */
		public static string LastDirectory()
		{
			return ReadSectionSettingString(sMRUFiles, "path");
		}

		/**
		 * \brief Retrieve the last file used
		 */
		public static string LastFile()
		{
			return ReadSectionSettingString(sMRUFiles, "0");
		}

		/**
		 * \brief Retrieve the history of the last files (MRU)
		 */
		public static string[] LastFiles()
		{
			int i, c;

			for (c=0; c<RememberFileCount; c++)
			{
				string s = ReadSectionSettingString(sMRUFiles, c.ToString());
				if ((s == null) || s.Equals(""))
					break;
			}

			string[] r = new string[c];

			for (i=0; i<c; i++)
			{
				r[i] = ReadSectionSettingString(sMRUFiles, i.ToString());
			}

			return r;
		}

		/**
		 * \brief Save the geometry of the form
		 */
		public static void SaveFormAspect(Form f)
		{
			string section = sAspect + "." + f.Name;
			
			if (f.WindowState == FormWindowState.Maximized)
			{
                if (WinUtils.Verbose)
                    Logger.Debug("Save form {0} aspect: maximized", f.Name);
				WriteSectionSettingBool(section, "Maximized", true);
			} else
			{
                if (WinUtils.Verbose)
                    Logger.Debug("Save form {0} aspect: left={1}, top={2}, height={3}, width={4}", f.Name, f.Left, f.Top, f.Height, f.Width);
				WriteSectionSettingBool(section, "Maximized", false);
				WriteSectionSettingInt(section, "Left", f.Left);
				WriteSectionSettingInt(section, "Top", f.Top);
				WriteSectionSettingInt(section, "Height", f.Height);
				WriteSectionSettingInt(section, "Width", f.Width);
			}
		}

		/**
		 * \brief Load the geometry of the form
		 */
		public static void LoadFormAspect(Form f)
		{
			string section = sAspect + "." + f.Name;

			f.StartPosition = FormStartPosition.Manual;
			
			f.Left = ReadSectionSettingInt(section, "Left", f.Left);
			f.Top = ReadSectionSettingInt(section, "Top", f.Top);
			f.Height = ReadSectionSettingInt(section, "Height", f.Height);
			f.Width = ReadSectionSettingInt(section, "Width", f.Width);

			if (ReadSectionSettingBool(section, "Maximized"))
			{
                if (WinUtils.Verbose)
                    Logger.Debug("Load form {0} aspect: maximized", f.Name);
				f.WindowState = FormWindowState.Maximized;
			}
			else
			{
                if (WinUtils.Verbose)
                    Logger.Debug("Load form {0} aspect: left={1}, top={2}, height={3}, width={4}", f.Name, f.Left, f.Top, f.Height, f.Width);
				f.WindowState = FormWindowState.Normal;
			}
		}

		/**
		 * \brief Read a string from registry
		 */
		public static string ReadString(string name, string default_value = "")
		{
			return ReadSectionSettingString(sSettings, name, default_value);
		}

		/**
		 * \brief Write a string to registry
		 */
		public static void WriteString(string name, string value)
		{
			WriteSectionSettingString(sSettings, name, value);
		}

		/**
		 * \brief Read an integer from registry
		 */
		public static int ReadInteger(string name, int default_value = 0)
		{
			return ReadSectionSettingInt(sSettings, name, default_value);
		}

		/**
		 * \brief Write an integer to registry
		 */
		public static void WriteInteger(string name, int value)
		{
			WriteSectionSettingInt(sSettings, name, value);
		}

		/**
		 * \brief Read a a boolean from registry
		 */
		public static bool ReadBoolean(string name, bool default_value = false)
		{
			return ReadSectionSettingBool(sSettings, name, default_value);
		}

		/**
		 * \brief Write a boolean to registry
		 */
		public static void WriteBoolean(string name, bool value)
		{
			WriteSectionSettingBool(sSettings, name, value);
		}

		public static string ReadSettingString(string name, string default_value = "")
		{
			string r = default_value;

			try
			{
				RegistryKey k = Registry.CurrentUser.OpenSubKey("SOFTWARE\\" + AppUtils.CompanyName + "\\" + AppUtils.ApplicationName, false);
				r = (string)k.GetValue(name, default_value);
			}
			catch
			{

			}

			return r;
		}

		public static int ReadSettingInt(string name, int default_value = 0)
		{
			string s = ReadSettingString(name, default_value.ToString());
			int r;
			int.TryParse(s, out r);
			return r;
		}

		public static bool ReadSettingBool(string name, bool default_value = false)
		{
			return (ReadSettingInt(name, default_value ? 1 : 0) != 0);
		}

		public static void WriteSettingString(string name, string value)
		{
			try
			{
				RegistryKey k = Registry.CurrentUser.CreateSubKey("SOFTWARE\\" + AppUtils.CompanyName + "\\" + AppUtils.ApplicationName);
				k.SetValue(name, value);
			}
			catch
			{

			}
		}

		public static void WriteSettingInt(string name, int value)
		{
			WriteSettingString(name, value.ToString());
		}

		public static void WriteSettingBool(string name, bool value)
		{
			WriteSettingInt(name, value ? 1 : 0);
		}

		public static string ReadSectionSettingString(string section, string name, string default_value = "")
		{
			string r = default_value;

			try
			{
				RegistryKey k = Registry.CurrentUser.OpenSubKey("SOFTWARE\\" + AppUtils.CompanyName + "\\" + AppUtils.ApplicationName + "\\" + section, false);
				r = (string)k.GetValue(name, default_value);
			}
			catch
			{

			}

			return r;
		}

		public static int ReadSectionSettingInt(string section, string name, int default_value = 0)
		{
			string s = ReadSectionSettingString(section, name, default_value.ToString());
			int r;
			int.TryParse(s, out r);
			return r;
		}

		public static bool ReadSectionSettingBool(string section, string name, bool default_value = false)
		{
			return (ReadSectionSettingInt(section, name, default_value ? 1 : 0) != 0);
		}

		public static void WriteSectionSettingString(string section, string name, string value)
		{
			try
			{
				RegistryKey k = Registry.CurrentUser.CreateSubKey("SOFTWARE\\" + AppUtils.CompanyName + "\\" + AppUtils.ApplicationName + "\\" + section);
				k.SetValue(name, value);
			}
			catch
			{

			}
		}

		public static void WriteSectionSettingInt(string section, string name, int value)
		{
			WriteSectionSettingString(section, name, value.ToString());
		}

		public static void WriteSectionSettingBool(string section, string name, bool value)
		{
			WriteSectionSettingInt(section, name, value ? 1 : 0);
		}

	}
}