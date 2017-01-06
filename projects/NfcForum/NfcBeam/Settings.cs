using System;
using System.Windows.Forms;
using Microsoft.Win32;

namespace SpringCardApplication
{
	public class Settings
	{
		public const int MODE_SEND_AND_RECV = 0;
		public const int MODE_SEND_ONLY = 1;
		public const int MODE_RECV_ONLY = 2;
		public const int MODE_SEND_THEN_RECV = 3;
		public const int MODE_RECV_THEN_SEND = 4;
		
		public string Reader = null;
		public bool AutoConnect = false;
		public bool ShowConsole = false;
		public int SelectedType = 1;
		public int SelectedMode = MODE_SEND_AND_RECV;
		
		public Settings()
		{
			Load();
		}
		
		public void Load()
		{
			try
			{
				RegistryKey k = Registry.CurrentUser.OpenSubKey("SOFTWARE\\SpringCard\\" + Application.ProductName, false);
				
				Reader = (string) k.GetValue("Reader", "");
				AutoConnect = ((int) k.GetValue("AutoConnect", 1) != 0);
				ShowConsole = ((int) k.GetValue("ShowConsole", 0) != 0);
				SelectedType = (int) k.GetValue("Type", 1);
				
				bool SNEPClient = ((int) k.GetValue("SNEPClient", 1) != 0);
				bool SNEPServer = ((int) k.GetValue("SNEPServer", 1) != 0);
				
				int tmpMode = ((int) k.GetValue("Mode", -1));
				
				if ((tmpMode >= MODE_SEND_AND_RECV) && (tmpMode <= MODE_RECV_THEN_SEND))
				{
					SelectedMode = tmpMode;
				} else
				{
					SelectedMode = MODE_SEND_AND_RECV;
					if (SNEPClient && !SNEPServer) SelectedMode = MODE_SEND_ONLY; else
						if (!SNEPClient && SNEPServer) SelectedMode = MODE_RECV_ONLY;
				}

			}
			catch (Exception)
			{

			}
		}
		
		public void Save()
		{
			try
			{
				RegistryKey k = Registry.CurrentUser.CreateSubKey("SOFTWARE\\SpringCard\\" + Application.ProductName);

				k.SetValue("Reader", (Reader != null) ? Reader : "");
				k.SetValue("AutoConnect", (int) (AutoConnect ? 1 : 0));
				k.SetValue("ShowConsole", (int) (ShowConsole ? 1 : 0));
				k.SetValue("Type", SelectedType);
				k.SetValue("Mode", SelectedMode);
			}
			catch (Exception)
			{

			}
		}
	}
}
