using System;
using System.Windows.Forms;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Win32;
using SpringCard.PCSC;

namespace PcscDiag2
{
  public class History
  {
    private List<string> l = new List<string>();
    private int c = 10;
    
    public History(int MaxCount)
    {
      c = MaxCount;
    }
    
    public void Clear()
    {
      l.Clear();
    }
    
    public int Length()
    {
      return l.Count;
    }
    
    public void Add(string s)
    {
      if (s.Equals("")) return;
      
      int i = l.IndexOf(s);
      
      if (i == 0) return;
      if (i > 0)
        l.RemoveAt(i);

      l.Insert(0, s);
      
      while (l.Count > c)
        l.RemoveAt(c - 1);
    }
    
    public string Get(int i)
    {      
      if ((i < 0) || (i >= l.Count)) return "";
      return l[i];
    }
  }
  
  public static class Settings
  {  
    public static uint ContextScope = SCARD.SCOPE_SYSTEM;
    public static string ListGroup = SCARD.ALL_READERS;
      
    public static uint DefaultConnectShare = SCARD.SHARE_EXCLUSIVE;
    public static uint DefaultConnectProtocol = (SCARD.PROTOCOL_T0|SCARD.PROTOCOL_T1);
    public static uint DefaultReconnectShare = SCARD.SHARE_EXCLUSIVE;
    public static uint DefaultReconnectProtocol = (SCARD.PROTOCOL_T0|SCARD.PROTOCOL_T1);
    public static uint DefaultReconnectDisposition = SCARD.RESET_CARD;
    public static uint DefaultDisconnectDisposition = SCARD.UNPOWER_CARD;
    public static uint DefaultEndTransactionDisposition = SCARD.RESET_CARD;
    
    public static int HistoryLength = 10;
    
    public static History HistoryControl = new History(HistoryLength);
    public static History HistoryTransmit = new History(HistoryLength);
    
    private static int GetIntValue(RegistryKey Key, string Name, int DefaultValue)
    {
      int r = DefaultValue;
      string s = (string) Key.GetValue(Name, String.Format("{0}", r));
      int.TryParse(s, out r);
      return r;
    }

    private static uint GetUintValue(RegistryKey Key, string Name, uint DefaultValue)
    {
      uint r = DefaultValue;
      string s = (string) Key.GetValue(Name, String.Format("{0}", r));
      uint.TryParse(s, out r);
      return r;
    }

    public static void Load()
    {
      try
      {
        RegistryKey k = Registry.CurrentUser.OpenSubKey("SOFTWARE\\SpringCard\\" + Application.ProductName, false);
        
        ContextScope = GetUintValue(k, "ContextScope", ContextScope);
        ListGroup = (string) k.GetValue("ListGroup", ListGroup);
        
        DefaultConnectShare = GetUintValue(k, "DefaultConnectShare", DefaultConnectShare);
        DefaultConnectProtocol = GetUintValue(k, "DefaultConnectProtocol", DefaultConnectProtocol);
        DefaultReconnectShare = GetUintValue(k, "DefaultReconnectShare", DefaultReconnectShare);
        DefaultReconnectProtocol = GetUintValue(k, "DefaultReconnectProtocol", DefaultReconnectProtocol);
        DefaultReconnectDisposition = GetUintValue(k, "DefaultReconnectDisposition", DefaultReconnectDisposition);
        DefaultDisconnectDisposition = GetUintValue(k, "DefaultDisconnectDisposition", DefaultDisconnectDisposition);
        DefaultEndTransactionDisposition = GetUintValue(k, "DefaultEndTransactionDisposition", DefaultEndTransactionDisposition);
        HistoryLength = GetIntValue(k, "HistoryLength", HistoryLength);
        
        HistoryTransmit.Clear();
        for (int i=HistoryLength-1; i>=0; i--)
        {
          string s = (string) k.GetValue("HistoryTransmit" + i, "");
          if (!s.Equals(""))
            HistoryTransmit.Add(s);
        }

        HistoryControl.Clear();
        for (int i=HistoryLength-1; i>=0; i--)
        {
          string s = (string) k.GetValue("HistoryControl" + i, "");
          if (!s.Equals(""))
            HistoryControl.Add(s);
        }
       
      }
      catch (Exception)
      {

      }
    }
    
    public static void Save()
    {
      try
      {
        RegistryKey k = Registry.CurrentUser.CreateSubKey("SOFTWARE\\SpringCard\\" + Application.ProductName);
        
        k.SetValue("ContextScope", ContextScope.ToString());
        k.SetValue("ListGroup", ListGroup);

        k.SetValue("DefaultConnectShare", DefaultConnectShare.ToString());
        k.SetValue("DefaultConnectProtocol", DefaultConnectProtocol.ToString());
        k.SetValue("DefaultReconnectShare", DefaultReconnectShare.ToString());
        k.SetValue("DefaultReconnectProtocol", DefaultReconnectProtocol.ToString());
        k.SetValue("DefaultReconnectDisposition", DefaultReconnectDisposition.ToString());
        k.SetValue("DefaultDisconnectDisposition", DefaultDisconnectDisposition.ToString());
        k.SetValue("DefaultEndTransactionDisposition", DefaultEndTransactionDisposition.ToString());
        k.SetValue("HistoryLength", HistoryLength.ToString());

        for (int i=0; i<HistoryTransmit.Length(); i++)
          k.SetValue("HistoryTransmit" + i, HistoryTransmit.Get(i));

        for (int i=0; i<HistoryControl.Length(); i++)
          k.SetValue("HistoryControl" + i, HistoryControl.Get(i));

      }
      catch (Exception)
      {

      }
    }
    
  }
}
