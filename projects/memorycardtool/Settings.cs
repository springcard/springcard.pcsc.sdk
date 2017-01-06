using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Microsoft.Win32;
using SpringCard.LibCs;

namespace MemoryCardTool
{
	public static class Settings
	{
		const int RememberKeyCount = 8;
		
		public static void RememberKeyA(string KeyValue)
		{			
			RememberKey("A", KeyValue);
		}

		public static void RememberKeyB(string KeyValue)
		{			
			RememberKey("B", KeyValue);
		}

		public static void RememberReadKeys(string KeyValue)
		{			
			RememberKey("Reading", KeyValue);
		}

		public static void RememberWriteKeys(string KeyValue)
		{			
			RememberKey("Writing", KeyValue);
		}		
		
		public static string[] LastKeysA()
		{			
			return LastKeys("A");
		}

		public static string[] LastReadKeys()
		{			
			return LastKeys("Reading");
		}

		public static string[] LastWriteKeys()
		{			
			return LastKeys("Writing");
		}
		
		public static string[] LastKeysB()
		{			
			return LastKeys("B");
		}
		
		private static void RememberKey(string KeyType, string KeyValue)
		{
			string section = "MifareClassic.Keys." + KeyType;
			string[] recents = new string[RememberKeyCount];
			
			for (int i=0; i<RememberKeyCount; i++)
				recents[i] = AppUtils.ReadSectionSettingString(section, i.ToString());
			
			int pos = RememberKeyCount - 1;
			for (int i=0 ; i<RememberKeyCount ; i++)
			{
				if ((recents[i]!=null) && recents[i].Equals(KeyValue))
					pos = i;
			}
			
			AppUtils.WriteSectionSettingString(section, "0", KeyValue);
			for (int i=1; i<=pos; i++)
			{
				if (recents[i-1] != null)
					AppUtils.WriteSectionSettingString(section, i.ToString(), recents[i-1]);
			}
		}

		
		private static string[] LastKeys(string KeyType)
		{
			string section = "MifareClassic.Keys." + KeyType;
			int i, c;
			
			for (c=0; c<RememberKeyCount; c++)
			{				
				string s = AppUtils.ReadSectionSettingString(section, c.ToString());
				if ((s == null) || s.Equals(""))
					break;
			}
			
			string[] r = new string[c];
			
			for (i=0; i<c; i++)
			{
				r[i] = AppUtils.ReadSectionSettingString(section, i.ToString());
			}

			return r;
		}
	}
}