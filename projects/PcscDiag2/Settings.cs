using Microsoft.Win32;
using SpringCard.LibCs;
using SpringCard.PCSC;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace PcscDiag2
{
    public class Settings
    {
        public bool Maximized = false;

        public uint ContextScope = SCARD.SCOPE_SYSTEM;
        public string ListGroup = SCARD.ALL_READERS;

        public uint DefaultConnectShare = SCARD.SHARE_EXCLUSIVE;
        public uint DefaultConnectProtocol = (SCARD.PROTOCOL_T0 | SCARD.PROTOCOL_T1);
        public uint DefaultReconnectShare = SCARD.SHARE_EXCLUSIVE;
        public uint DefaultReconnectProtocol = (SCARD.PROTOCOL_T0 | SCARD.PROTOCOL_T1);
        public uint DefaultReconnectDisposition = SCARD.RESET_CARD;
        public uint DefaultDisconnectDisposition = SCARD.UNPOWER_CARD;
        public uint DefaultEndTransactionDisposition = SCARD.RESET_CARD;

        public int HistoryLength = 10;

        private List<string> HistoryControl = new List<string>();
        private List<string> HistoryTransmit = new List<string>();


        private int GetIntValue(RegistryKey Key, string Name, int DefaultValue)
        {
            int r = DefaultValue;
            try
            {
                string s = (string)Key.GetValue(Name, string.Format("{0}", r));
                r = int.Parse(s);
            }
            catch { }
            return r;
        }

        private uint GetUintValue(RegistryKey Key, string Name, uint DefaultValue)
        {
            uint r = DefaultValue;
            try
            {
                string s = (string)Key.GetValue(Name, string.Format("{0}", r));
                r = uint.Parse(s);
            }
            catch { }
            return r;
        }

        public void TransmitHistoryAdd(string s)
        {
            if (HistoryTransmit.Contains(s))
                HistoryTransmit.Remove(s);
            while (HistoryTransmit.Count > HistoryLength)
                HistoryTransmit.RemoveAt(0);
            HistoryTransmit.Add(s);
        }

        public string TransmitHistoryGet(int index)
        {
            if (index < 0)
                return null;
            if ((index >= HistoryTransmit.Count) || (index >= HistoryLength))
                return null;
            return HistoryTransmit[HistoryTransmit.Count - index - 1];
        }

        public int TransmitHistoryCount
        {
            get
            {
                return HistoryTransmit.Count;
            }
        }

        public void ControlHistoryAdd(string s)
        {
            if (HistoryControl.Contains(s))
                HistoryControl.Remove(s);
            while (HistoryControl.Count > HistoryLength)
                HistoryControl.RemoveAt(0);
            HistoryControl.Add(s);
        }

        public string ControlHistoryGet(int index)
        {
            if (index < 0)
                return null;
            if ((index >= HistoryControl.Count) || (index >= HistoryLength))
                return null;
            return HistoryControl[HistoryControl.Count - index - 1];
        }

        public int ControlHistoryCount
        {
            get
            {
                return HistoryControl.Count;
            }
        }

        public void Load()
        {
            try
            {
                RegistryKey k = Registry.CurrentUser.OpenSubKey("SOFTWARE\\SpringCard\\" + Application.ProductName, false);

                Logger.Trace("Loading settings to registry");

                Maximized = (GetIntValue(k, "Maximized", Maximized ? 1 : 0) != 0);

                ContextScope = GetUintValue(k, "ContextScope", ContextScope);
                ListGroup = (string)k.GetValue("ListGroup", ListGroup);

                DefaultConnectShare = GetUintValue(k, "DefaultConnectShare", DefaultConnectShare);
                DefaultConnectProtocol = GetUintValue(k, "DefaultConnectProtocol", DefaultConnectProtocol);
                DefaultReconnectShare = GetUintValue(k, "DefaultReconnectShare", DefaultReconnectShare);
                DefaultReconnectProtocol = GetUintValue(k, "DefaultReconnectProtocol", DefaultReconnectProtocol);
                DefaultReconnectDisposition = GetUintValue(k, "DefaultReconnectDisposition", DefaultReconnectDisposition);
                DefaultDisconnectDisposition = GetUintValue(k, "DefaultDisconnectDisposition", DefaultDisconnectDisposition);
                DefaultEndTransactionDisposition = GetUintValue(k, "DefaultEndTransactionDisposition", DefaultEndTransactionDisposition);
                HistoryLength = GetIntValue(k, "HistoryLength", HistoryLength);

                HistoryTransmit.Clear();
                for (int i = HistoryLength - 1; i >= 0; i--)
                {
                    string v = (string)k.GetValue("HistoryTransmit" + i, "");
                    if (!string.IsNullOrEmpty(v))
                    {
                        Logger.Trace("TransmitHistory{0}={1}", i, v);
                        HistoryTransmit.Add(v);
                    }
                }

                HistoryControl.Clear();
                for (int i = HistoryLength - 1; i >= 0; i--)
                {
                    string v = (string)k.GetValue("HistoryControl" + i, "");
                    if (!string.IsNullOrEmpty(v))
                    {
                        Logger.Trace("ControlHistory{0}={1}", i, v);
                        HistoryControl.Add(v);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Warning("Exception {0} while loading settings", e.Message);
            }
        }

        public void Save()
        {
            try
            {
                RegistryKey k = Registry.CurrentUser.CreateSubKey("SOFTWARE\\SpringCard\\" + Application.ProductName);

                Logger.Trace("Saving settings to registry");

                k.SetValue("Maximized", Maximized ? "1" : "0");

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

                for (int i = 0; i < HistoryLength; i++)
                {
                    string n = "HistoryTransmit" + i;
                    string v = TransmitHistoryGet(i);
                    if (string.IsNullOrEmpty(v))
                    {
                        k.DeleteValue(n, false);
                    }
                    else
                    {
                        Logger.Trace("TransmitHistory{0}={1}", i, v);
                        k.SetValue(n, v);
                    }
                }

                for (int i = 0; i < HistoryLength; i++)
                {
                    string n = "HistoryControl" + i;
                    string v = ControlHistoryGet(i);
                    if (string.IsNullOrEmpty(v))
                    {
                        k.DeleteValue(n, false);
                    }
                    else
                    {
                        Logger.Trace("ControlHistory{0}={1}", i, v);
                        k.SetValue(n, v);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Warning("Exception {0} while loading settings", e.Message);
            }
        }
    }
}
