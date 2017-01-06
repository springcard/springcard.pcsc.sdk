using System;
using System.Windows.Forms;
using Microsoft.Win32;

namespace SpringCardApplication
{
  public class Settings
  {
    public string Reader;
    public bool AutoConnect;   
    public bool ShowConsole;
    public bool EnableLock;
    
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
        EnableLock = ((int) k.GetValue("EnableLock", 0) != 0);
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
        k.SetValue("EnableLock", (int) (EnableLock ? 1 : 0));
      }
      catch (Exception)
      {

      }
    }
  }
}
