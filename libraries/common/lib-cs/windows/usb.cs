using System;
using System.Collections.Generic;
using System.Management;
using System.Diagnostics;

namespace SpringCard.LibCs.Windows
{
	public static class USB
	{
        public const ushort SpringCard_VendorIDw = 0x1C34;
        public const string WmiWhereUsb = @"PNPDeviceID LIKE '%USB%VID%&PID%'";

        private static string FindParent(List<string> deviceTree, string deviceId)
        {
            if (!deviceTree.Contains(deviceId))
                return null;

            int childPos = deviceTree.IndexOf(deviceId);

            string[] childChain = deviceId.Replace('&', '\\').Split('\\');

            if ((childChain.Length < 4) || (!childChain[3].StartsWith("MI_")))
                return null; /* Failed to parse, or not a child device */

            int parentPos = childPos;

            while (--parentPos >= 0)
            {
                string[] parentChain = deviceTree[parentPos].Replace('&', '\\').Split('\\');

                if (parentChain.Length >= 3)
                {
                    if ((parentChain[0] == childChain[0]) && (parentChain[1] == childChain[1]) && (parentChain[2] == childChain[2]))
                    {
                        if (!parentChain[3].StartsWith("MI_"))
                        {
                            return deviceTree[parentPos];
                        }
                    }
                    else
                    {
                        /* Other device */
                        break;
                    }
                }
            }

            return null;
        }

        public static List<string> GetDevicesTree()
        {
            List<string> deviceTree = new List<string>();

            try
            {
                ManagementObjectCollection collection = null;

                string selectString = @"SELECT * From Win32_USBControllerDevice";

                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(selectString))
                    collection = searcher.Get();

                if (collection != null)
                {
                    foreach (ManagementObject mo in collection)
                    {
                        string deviceId = (string)mo.GetPropertyValue("Dependent");

                        string[] e;
                        e = deviceId.Split('=');
                        deviceId = e[1];
                        deviceId = deviceId.Replace("\\\\", "\\");
                        deviceId = deviceId.Replace("\"", "");

                        deviceTree.Add(deviceId);
                    }
                    collection.Dispose();
                }
            }
            catch { }

            /* The devices must be listed in the right order to be able to create the tree... */
            deviceTree.Sort();
            deviceTree.Reverse();

            return deviceTree;
        }

        public static List<WMI.DeviceInfo> EnumDevices()
        {
            List<WMI.DeviceInfo> devices = new List<WMI.DeviceInfo>();

            List<string> deviceTree = GetDevicesTree();

            try
            {
                ManagementObjectCollection collection = null;

                string selectString = @"SELECT * From Win32_PnPEntity WHERE PNPDeviceID LIKE '%USB%VID%&PID%'";

                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(selectString))
                    collection = searcher.Get();

                if (collection != null)
                {
                    foreach (ManagementObject mo in collection)
                    {
                        string deviceId = (string)mo.GetPropertyValue("PNPDeviceID");

                        if (!deviceTree.Contains(deviceId))
                            continue;

                        string parentDeviceId = FindParent(deviceTree, deviceId);


                        WMI.DeviceInfo di = new WMI.DeviceInfo(mo, parentDeviceId);

                        devices.Add(di);
                    }
                    collection.Dispose();
                }
            }
            catch { }

            return devices;
        }

        public static bool IsSpringCardRFIDScanner(ushort vid, ushort pid)
        {
            if (vid == SpringCard_VendorIDw)
            {
                switch (pid)
                {
                    case 0x7241:
                    case 0x9241:
                        return true;
                    default:
                        break;
                }
            }
            return false;
        }

        public static List<WMI.DeviceInfo> EnumDevices_RFIDScanners()
		{
			return WMI.EnumDevices(WMI.WmiDeviceTable, WmiWhereUsb, delegate(WMI.DeviceInfo device) {
			    return device.Type.Contains("RFID Scanner");
			});
		}
		
		public static List<WMI.DeviceInfo> EnumDevices_DFU()
		{
            return WMI.EnumDevices(WMI.WmiDeviceTable,  WmiWhereUsb, delegate(WMI.DeviceInfo device) {
			    return (device.Type == "Atmel DFU");
			});			
		}

		public static List<WMI.DeviceInfo> EnumDevices_H_Group(bool IncludeDFU = false, bool IncludeCSB6 = false)
		{
			return WMI.EnumDevices(WMI.WmiDeviceTable, WmiWhereUsb, delegate(WMI.DeviceInfo device) {
                if (IncludeDFU && (device.Type == "Atmel DFU"))
                {
                    if (WinUtils.Verbose)
                        Logger.Debug("Atmel DFU");
                    return true;
                }
                if (device.wVendorID != SpringCard_VendorIDw)
                {
                    if (WinUtils.Verbose)
                        Logger.Debug("Not a SpringCard USB device");
                    return false;
                }
                if ((device.wProductID & 0xF000) == 0x9000)
                {
                    if (WinUtils.Verbose)
                        Logger.Debug("H663 family");
                    return true; /* H663 */
                }
                if ((device.wProductID & 0xF000) == 0xA000)
                {
                    if (WinUtils.Verbose)
                        Logger.Debug("H512 family");
                    return true; /* H512 */
                }
                if (IncludeCSB6 && ((device.wProductID & 0xF000) == 0x7000))
                {
                    if (WinUtils.Verbose)
                        Logger.Debug("CSB6 family");
                    return true; /* CSB6 */
                }
                if (WinUtils.Verbose)
                    Logger.Debug("ProductID={0:X04} - not an H663/H512 device", device.wProductID);
                return false;
		    });
		}		
	}
}
