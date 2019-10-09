using System;
using System.Collections.Generic;
using System.Management;
using System.IO.Ports;

namespace SpringCard.LibCs.Windows
{
	public static class Serial
	{
		public class CommPortInfo
		{
			ManagementObject device;
			
			public CommPortInfo(ManagementObject device)
			{
				this.device = device;
			}
			public string Name
			{
				get
				{
					return (string)device.GetPropertyValue("Name");
				}				
			}			
			public string PortName
			{
				get
				{
					string name = Name;
					
					string[] exploded_name = name.Split('(');
					
					foreach (string subname in exploded_name)
					{
						if (subname.StartsWith("COM") || subname.StartsWith("LPT"))
						{
							string[] exploded_subname = subname.Split(')');
							return exploded_subname[0];
						}
					}
					
					return name;
				}				
			}						
			public string DeviceID
			{
				get
				{
					return (string)device.GetPropertyValue("DeviceID");
				}				
			}						
			public string PNPDeviceID
			{
				get
				{
					return (string)device.GetPropertyValue("PNPDeviceID");
				}				
			}			
			public string[] HardwareIDs
			{
				get
				{
					return (string[])device.GetPropertyValue("HardwareID");
				}				
			}			
			
		}
    
		public static List<CommPortInfo> EnumCommPorts()
		{
			List<CommPortInfo> commPorts = new List<CommPortInfo>();
			
			try
			{
				ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_PnPEntity WHERE ClassGuid='{4d36e978-e325-11ce-bfc1-08002be10318}'");
				
				foreach (ManagementObject queryObj in searcher.Get())
				{
					CommPortInfo deviceObj = new CommPortInfo(queryObj);					
					if (deviceObj.PortName.StartsWith("LPT")) continue;
                    if (WinUtils.Verbose)
                        Logger.Debug("Found a comm. port: " + deviceObj.PortName);
					commPorts.Add(deviceObj);
				}
			}
			catch (Exception e)
			{
				Logger.Trace("Exception while listing the comm. ports");
				Logger.Trace(e.Message);
			}
			
			return commPorts;
		}
		
		public static string[] GetCommPortNames()
		{
			return SerialPort.GetPortNames();
		}

        public static List<WMI.DeviceInfo> EnumDevices()
        {
            return WMI.EnumDevices("Win32_SerialPort", null, null);
        }
    }
}
