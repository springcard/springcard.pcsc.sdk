using System;
using System.Collections.Generic;
using System.Management;
using System.Diagnostics;

namespace SpringCard.LibCs.Windows
{
    public static class WMI
    {
        public const string WmiDeviceTable = @"Win32_PnPEntity";

        public class DeviceInfo
        {
            ManagementObject device; /* Right-click References on the right and manually add System.Management. Even though I included it in the using statement I still had to do this. Once I did, all worked fine. */

            public string HardwareID { get; private set; } = "";
            private string OverrideName = "";
            public string FriendlyName { get; private set; } = "";
            public string Type { get; private set; } = "";
            public string ParentPnpDeviceId { get; private set; } = "";

            private void RecognizeDevice()
            {
                if (this.wVendorID == USB.SpringCard_VendorIDw)
                {
                    switch (this.wProductID & 0xFF00)
                    {
                        case 0x7000: /* CSB6 generation */
                        case 0x7100: /* CSB6 generation */
                        case 0x7200: /* CSB6 generation */
                        case 0x8000: /* H512 generation */
                        case 0x8100: /* H512 generation */
                        case 0x8200: /* H512 generation */
                        case 0x9000: /* H663 generaton */
                        case 0x9100: /* H663 generaton */
                        case 0x9200: /* H663 generaton */

                            switch (this.wProductID)
                            {
                                case 0x7241:
                                    FriendlyName = "SpringCard RFID Scanner";
                                    break;
                                case 0x9241:
                                    FriendlyName = "SpringCard RFID Scanner HSP";
                                    break;
                                default:
                                    break;
                            }
                            switch (this.wProductID & 0x0F00)
                            {
                                case 0x0000:
                                    Type = "VCP";
                                    break;
                                case 0x0100:
                                    Type = "PC/SC";
                                    break;
                                case 0x0200:
                                    Type = "RFID Scanner";
                                    break;
                                default:
                                    break;
                            }
                            break;

                        case 0x6100:
                            /* SpringCore'18 generation */
                            OverrideName = "SpringCore'18";
                            switch (this.wProductID)
                            {
                                case 0x6120:
                                case 0x6127:
                                    Type = "Direct";
                                    break;
                                case 0x6121:
                                case 0x6124:
                                    Type = "VCP";
                                    break;
                                case 0x6122:
                                    Type = "PC/SC";
                                    break;
                                case 0x6123:
                                    Type = "RFID Scanner";
                                    break;
                            }
                            break;

                        case 0xAF00:
                            /* AFCare device */
                            switch (this.wProductID)
                            {
                                case 0x61C1:
                                case 0x61C2:
                                    Type = "PC/SC";
                                    break;
                            }
                            break;

                        default:
                            break;
                    }
                }
                else if ((this.wVendorID == 0x03EB) && (this.wProductID == 0x2FF6))
                {
                    Type = "Atmel DFU";
                }
            }

            public DeviceInfo(ManagementObject device, string parent = null)
            {
                this.device = device;
                this.ParentPnpDeviceId = parent;

                try
                {
                    string[] s = (string[])device.GetPropertyValue("HardwareID");
                    HardwareID = s[0];
                } catch { }
                if (string.IsNullOrEmpty(HardwareID))
                {
                    try
                    {
                        HardwareID = (string)device.GetPropertyValue("HardwareID");
                    }
                    catch { }
                }

                if (HardwareID == null)
                {
                    HardwareID = device.Path.Path;
                    HardwareID = HardwareID.Replace("\\\\", "\\");
                    string C = "DeviceID=";
                    if (HardwareID.Contains(C))
                    {
                        HardwareID = HardwareID.Substring(HardwareID.IndexOf(C) + C.Length);
                        if (HardwareID.StartsWith("\"") && HardwareID.EndsWith("\"") && (HardwareID.Length >= 2))
                            HardwareID = HardwareID.Substring(1, HardwareID.Length - 2);
                    }
                }

                FriendlyName = (string)device.GetPropertyValue("Caption");

                RecognizeDevice();
            }

            private string GetProperty(string name)
            {
                try
                {
                    return (string)device.GetPropertyValue(name);
                }
                catch
                {
                    return "";
                }
            }

            public string DeviceID
            {
                get
                {
                    return GetProperty("DeviceID");
                }
            }

            public string PnpDeviceID
            {
                get
                {
                    return GetProperty("PNPDeviceID");
                }
            }

            public string Description
            {
                get
                {
                    return GetProperty("Description");
                }
            }

            public string Name
            {
                get
                {
                    if (!string.IsNullOrEmpty(OverrideName))
                        return OverrideName;
                    return GetProperty("Name");
                }
            }

            public string Manufacturer
            {
                get
                {
                    return GetProperty("Manufacturer");
                }
            }

            public string Service
            {
                get
                {
                    return GetProperty("Service");
                }
            }

            public string Status
            {
                get
                {
                    return GetProperty("Status");
                }
            }

            public string Caption
            {
                get
                {
                    return GetProperty("Caption");
                }
            }

            public string HidInstance
            {
                get
                {
                    string result = PnpDeviceID;
                    result = result.ToLower();
                    result = result.Replace(@"\", @"#");
                    return result;
                }
            }


            public string VendorID
            {
                get
                {
                    string s = PnpDeviceID.ToUpper();
                    if (s.Contains("USB") && s.Contains("VID_"))
                    {
                        s = s.Substring(s.IndexOf("VID_") + 4, 4);
                        return s;
                    }
                    s = HardwareID.ToUpper();
                    if (s.Contains("USB") && s.Contains("VID_"))
                    {
                        s = s.Substring(s.IndexOf("VID_") + 4, 4);
                        return s;
                    }
                    return "0000";
                }
            }

            public ushort wProductID
            {
                get
                {
                    return BinConvert.ParseHexW(ProductID);
                }
            }

            public string ProductID
            {
                get
                {
                    string s = PnpDeviceID.ToUpper();
                    if (s.Contains("USB") && s.Contains("PID_"))
                    {
                        s = s.Substring(s.IndexOf("PID_") + 4, 4);
                        return s;
                    }
                    s = HardwareID.ToUpper();
                    if (s.Contains("USB") && s.Contains("PID_"))
                    {
                        s = s.Substring(s.IndexOf("PID_") + 4, 4);
                        return s;
                    }
                    return "0000";
                }
            }

            public string VersionID
            {
                get
                {
                    string s = PnpDeviceID.ToUpper();
                    if (s.Contains("USB") && s.Contains("REV_"))
                    {
                        s = s.Substring(s.IndexOf("REV_") + 4, 4);
                        return s;
                    }
                    s = HardwareID.ToUpper();
                    if (s.Contains("USB") && s.Contains("REV_"))
                    {
                        s = s.Substring(s.IndexOf("REV_") + 4, 4);
                        return s;
                    }
                    return "0000";
                }
            }

            public string Version
            {
                get
                {
                    string s = VersionID;
                    s = s.Substring(0, 2) + "." + s.Substring(2, 2);
                    if (s.StartsWith("0")) s = s.Substring(1);
                    return s;
                }
            }

            public ushort wVendorID
            {
                get
                {
                    return BinConvert.ParseHexW(VendorID);
                }
            }

            public string SerialNumber
            {
                get
                {
                    string[] s = PnpDeviceID.ToUpper().Split('\\');
                    if (s.Length > 2)
                    {
                        return s[s.Length - 1];
                    }
                    return "";
                }
            }

            public byte[] abSerialNumber
            {
                get
                {
                    return BinConvert.ParseHex(SerialNumber);
                }
            }
        }

        public static List<DeviceInfo> EnumDevices()
		{
			return EnumDevices(WmiDeviceTable, null, null);
		}
		
		public delegate bool EnumDeviceFilter(DeviceInfo device);
		public static List<DeviceInfo> EnumDevices(string table, string where, EnumDeviceFilter filter)
		{
			List<DeviceInfo> devices = new List<DeviceInfo>();
			
			try
			{
				ManagementObjectCollection collection = null;
				
				string selectString = string.Format(@"SELECT * FROM {0}", table);
                if (!string.IsNullOrEmpty(where))
                    selectString += string.Format(" WHERE ({0})", where);

                if (WinUtils.Verbose)
                    Logger.Debug("WMI:{0}", selectString);

                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(selectString))
					collection = searcher.Get();
				
				if (collection != null)
				{
					foreach (ManagementObject md in collection)
					{
						try
						{						
							DeviceInfo ud = new DeviceInfo(md);

                            if (filter != null)
                            {
                                if (!filter(ud))
                                {
                                    if (WinUtils.Verbose)
                                        Logger.Debug("WMI:Discarding {0}", ud.FriendlyName);
                                    continue;
                                }
                            }

                            if (WinUtils.Verbose)
                                Logger.Debug("WMI:Adding {0}", ud.FriendlyName);
                            devices.Add(ud);
						}
						catch {}						
					}
					
					collection.Dispose();
				}
			}
			catch {}
			
			return devices;			
		}
		
	}
}
