using System;
using System.Windows.Forms;
using SpringCard.LibCs;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Microsoft.Win32;


namespace SpringCard.LibCs
{
	class AppConfig
	{
		public const int RememberFileCount = 5;
		public const int RememberKeyCount = 10;
		
		const string sMRUFiles = "MRUFiles";
		const string sAspect = "Aspect";
		const string sSettings = "Settings";
		
		public static void RememberFile(string FileName)
		{
			string[] recents = new string[RememberFileCount];
			
			for (int i=0; i<RememberFileCount; i++)
			{
				recents[i] = AppUtils.ReadSectionSettingString(sMRUFiles, i.ToString());
			}
			
			int pos = RememberFileCount - 1;
			for (int i=0 ; i<RememberFileCount ; i++)
			{
				if ((recents[i]!=null) && recents[i].Equals(FileName))
					pos = i;
			}
			
			AppUtils.WriteSectionSettingString(sMRUFiles, "0", FileName);
			for (int i=1; i<=pos; i++)
			{
				if (recents[i-1] != null)
					AppUtils.WriteSectionSettingString(sMRUFiles, i.ToString(), recents[i-1]);
			}
		}
		
		public static void RememberDirectory(string PathName)
		{
			AppUtils.WriteSectionSettingString(sMRUFiles, "path", PathName);
		}
		
		public static string LastDirectory()
		{
			return AppUtils.ReadSectionSettingString(sMRUFiles, "path");
		}
		
		public static string LastFile()
		{
			return AppUtils.ReadSectionSettingString(sMRUFiles, "0");
		}
		
		public static string[] LastFiles()
		{
			int i, c;
			
			for (c=0; c<RememberFileCount; c++)
			{
				string s = AppUtils.ReadSectionSettingString(sMRUFiles, c.ToString());
				if ((s == null) || s.Equals(""))
					break;
			}
			
			string[] r = new string[c];
			
			for (i=0; i<c; i++)
			{
				r[i] = AppUtils.ReadSectionSettingString(sMRUFiles, i.ToString());
			}

			return r;
		}
		
		public static void SaveFormAspect(Form f)
		{
			string section = f.Name + "." + sAspect;
			
			if (f.WindowState == FormWindowState.Maximized)
			{
				AppUtils.WriteSectionSettingBool(section, "Maximized", true);
			} else
			{
				AppUtils.WriteSectionSettingBool(section, "Maximized", false);
				AppUtils.WriteSectionSettingInt(section, "Left", f.Left);
				AppUtils.WriteSectionSettingInt(section, "Top", f.Top);
				AppUtils.WriteSectionSettingInt(section, "Height", f.Height);
				AppUtils.WriteSectionSettingInt(section, "Width", f.Width);
			}
		}
		
		public static void LoadFormAspect(Form f)
		{
			string section = f.Name + "." + sAspect;
			
			f.Left = AppUtils.ReadSectionSettingInt(section, "Left", f.Left);
			f.Top = AppUtils.ReadSectionSettingInt(section, "Top", f.Top);
			f.Height = AppUtils.ReadSectionSettingInt(section, "Height", f.Height);
			f.Width = AppUtils.ReadSectionSettingInt(section, "Width", f.Width);
			
			if (AppUtils.ReadSectionSettingBool(section, "Maximized"))
				f.WindowState = FormWindowState.Maximized;
			else
				f.WindowState = FormWindowState.Normal;
		}
		
		public static string ReadString(string name, string default_value = "")
		{
			return AppUtils.ReadSectionSettingString(sSettings, name, default_value);
		}
		
		public static void WriteString(string name, string value)
		{
			AppUtils.WriteSectionSettingString(sSettings, name, value);
		}

		public static bool ReadBoolean(string name, bool default_value = false)
		{
			return AppUtils.ReadSectionSettingBool(sSettings, name, default_value);
		}
		
		public static void WriteBoolean(string name, bool value)
		{
			AppUtils.WriteSectionSettingBool(sSettings, name, value);
		}

		
	}
}