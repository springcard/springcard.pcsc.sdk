using System;
using System.Collections.Generic;
using System.Management;

namespace SpringCard.LibCs
{
	public static class USB
	{
		public const ushort SpringCard_VendorIDw = 0x1C34;

		public static bool IsSpringCardRFIDScanner(ushort vid, ushort pid)
		{
			if (vid == SpringCard_VendorIDw)
			{
				Logger.Trace("A SpringCard device, mask=" + BinConvert.ToHex((ushort) (pid & 0x0F0F)));
				if ((pid & 0x0F0F) == 0x0201)
					return true;
			}
			return false;
		}
		
		public class DeviceInfo
		{
			ManagementObject device; /* Right-click References on the right and manually add System.Management. Even though I included it in the using statement I still had to do this. Once I did, all worked fine. */
			
			string _HardwareID;
			string _FriendlyName;
			string _Type;

			
			public DeviceInfo(ManagementObject device)
			{
				this.device = device;

				_HardwareID = null;
				
				if (_HardwareID == null)
				{
					try
					{
						string[] s = (string[]) device.GetPropertyValue("HardwareID");
						_HardwareID = s[0];
					}
					catch
					{
	
					}
				}
				if (_HardwareID == null)
				{
					try
					{
						_HardwareID = (string) device.GetPropertyValue("HardwareID");
					}
					catch
					{
							
					}
				}
				
				if (_HardwareID == null)
				{
					_HardwareID = device.Path.Path;
					_HardwareID = _HardwareID.Replace("\\\\", "\\");					
					string C = "DeviceID=";
					if (_HardwareID.Contains(C))
					{
						_HardwareID = _HardwareID.Substring(_HardwareID.IndexOf(C) + C.Length);
						if (_HardwareID.StartsWith("\"") && _HardwareID.EndsWith("\"") && (_HardwareID.Length >= 2))
							_HardwareID = _HardwareID.Substring(1, _HardwareID.Length - 2);
					}					
				}
				
				_FriendlyName = (string) device.GetPropertyValue("Caption");
				
				_Type = "";
				
				if (this.wVendorID == SpringCard_VendorIDw)
				{
					switch (this.wProductID)
					{
						case 0x7241 :
							_FriendlyName = "SpringCard RFID Scanner";
							break;
						case 0x9241 :
							_FriendlyName = "SpringCard RFID Scanner HSP";
							break;
						default :
							break;
					}
					switch (this.wProductID & 0x0F00)
					{
						case 0x0000 :
							_Type = "VCP";
							break;
						case 0x0100 :
							_Type = "PC/SC";
							break;
						case 0x0200 :
							_Type = "RFID Scanner";					
							break;								
						default :
							break;
					}
				}
				else if ((this.wVendorID == 0x03EB) && (this.wProductID == 0x2FF6))
				{
					_Type = "Atmel DFU";
				}
			}
			
			public string Type
			{
				get
				{
					return _Type;
				}
			}
			
			private string GetProperty(string name)
			{
				try
				{
					return (string) device.GetPropertyValue(name);
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
			
			public string HardwareID
			{
				get
				{
					return _HardwareID;
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
			
			public string FriendlyName
			{
				get
				{
					return _FriendlyName;
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
			
			public string Version
			{
				get
				{
					string s = HardwareID.ToUpper();
					if (s.StartsWith("USB") && s.Contains("REV_"))
					{
						s = s.Substring(s.IndexOf("REV_") + 4, 4);
						s = s.Substring(0, 2) + "." + s.Substring(2, 2);
						if (s.StartsWith("0")) s = s.Substring(1);
						return s;
					}
					return "0.00";
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
		 
		public static List<DeviceInfo> EnumDevices_RFIDScanners()
		{
			return EnumDevices(delegate(DeviceInfo device)
			                   {
			                   		return device.Type == "RFID Scanner";
			                   });
		}
		
		public static List<DeviceInfo> EnumDevices_DFU()
		{
			return EnumDevices(delegate(DeviceInfo device)
			                   {
			                   		return (device.Type == "Atmel DFU");
			                   });			
		}

		public static List<DeviceInfo> EnumDevices_H_Group(bool IncludeDFU = false)
		{
			return EnumDevices(delegate(DeviceInfo device)
			                   {
			                   		if (IncludeDFU && (device.Type == "Atmel DFU")) return true;
			                   		return ((device.wVendorID == SpringCard_VendorIDw) && ((device.wProductID & 0xF000) == 0x9000));
			                   });
		}
		
		public static List<DeviceInfo> EnumDevices()
		{
			return EnumDevices(null);
		}
		
		public delegate bool EnumDeviceFilter(DeviceInfo device);
		public static List<DeviceInfo> EnumDevices(EnumDeviceFilter filter)
		{
			List<DeviceInfo> devices = new List<DeviceInfo>();
			
			try
			{
				ManagementObjectCollection collection = null;
				
				string selectString = @"SELECT * From Win32_PnPEntity WHERE (PNPDeviceID LIKE '%USB%VID%&PID%')";
				
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
								if (!filter(ud))
									continue;
							
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
