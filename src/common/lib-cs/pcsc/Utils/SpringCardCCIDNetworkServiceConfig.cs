/*
 * Created by SharpDevelop.
 * User: johann
 * Date: 12/05/2015
 * Time: 17:16
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using Microsoft.Win32;
using System.Security.AccessControl;
using System.Security.Principal;
using System.IO;
using System.Security;
using System.Runtime.InteropServices;
using SpringCard.LibCs;

namespace SpringCard.PCSC
{
	/// <summary>
	/// Description of scCcidNetworkRegistry.
	/// </summary>
	public class CcidNetworkServiceConfig
	{
		public const string _ServiceConfigKey = @"Software\\SpringCard\\CcidNetwork";
        public const string _ServiceConfigDevicesSubKey = "Devices";
		public const string _ServiceConfigRuntimeSubKey = "Runtime";
		public const string _ServiceConfigDevicesSubKey64 = @"Software\\SpringCard\\CcidNetwork\\Devices";
		public const string _ServiceConfigRuntimeSubKey64 = @"Software\\SpringCard\\CcidNetwork\\Runtime";
        public const string _ServiceConfigDebugSubKey64 = @"Software\\SpringCard\\CcidNetwork";

        public enum Mode
		{
			ReadOnly,
			ReadWrite}

		;
		private Mode mode;
        private bool is64bit = false;
        RegistryKey baseKey64 = null;
        RegistryKey baseKey32 = null;
        
        RegistryKey serviceKey;

        public CcidNetworkServiceConfig(Mode mode)
		{
			this.mode = mode;        	
			this.is64bit = AppUtils.Is64BitOperatingSystem();
            baseKey32 = null;
            baseKey64 = null;
        }

        public bool Create()
        {
            try
            {
                if (is64bit)
                {
                    baseKey64 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                    if (mode == Mode.ReadOnly) {
                        serviceKey = Registry.LocalMachine.CreateSubKey(_ServiceConfigKey, RegistryKeyPermissionCheck.ReadSubTree);
                    } else {
                        serviceKey = Registry.LocalMachine.CreateSubKey(_ServiceConfigKey, RegistryKeyPermissionCheck.ReadWriteSubTree);

                        RegistryKey k = null;
                        k = baseKey64.CreateSubKey(_ServiceConfigDevicesSubKey64, RegistryKeyPermissionCheck.ReadSubTree);
                        k.Close();
                        k = null;

                        k = baseKey64.CreateSubKey(_ServiceConfigRuntimeSubKey64, RegistryKeyPermissionCheck.ReadSubTree);
                        k.Close();
                        k = null;
                    }                    
                }
                else
                {
                    baseKey32 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
                    if (mode == Mode.ReadOnly) {
                        serviceKey = baseKey32.CreateSubKey(_ServiceConfigKey, RegistryKeyPermissionCheck.ReadSubTree);
                    } else {
                        serviceKey = baseKey32.CreateSubKey(_ServiceConfigKey, RegistryKeyPermissionCheck.ReadWriteSubTree);

                        RegistryKey k = null;
                        k = baseKey32.CreateSubKey(_ServiceConfigDevicesSubKey64, RegistryKeyPermissionCheck.ReadSubTree);
                        k.Close();
                        k = null;

                        k = baseKey32.CreateSubKey(_ServiceConfigRuntimeSubKey64, RegistryKeyPermissionCheck.ReadSubTree);
                        k.Close();
                        k = null;
                    }                    
                }
                return true;
            }
            catch (Exception ex)
            {
                serviceKey = null;
                Console.WriteLine("Exception : fails to create LOCAL_MACHINE KEY {0}", ex.Message);
                baseKey32 = null;
                baseKey64 = null;
                return false;
            }            
        }
        
		public string[] DeviceNames {
			get {
				RegistryKey k = null;

                try
                {
                    if (baseKey64 != null)
                    {
                        k = baseKey64.OpenSubKey(_ServiceConfigDevicesSubKey64, RegistryKeyPermissionCheck.ReadSubTree);
                    }
                    else if (baseKey32 != null)
                    {
                        //k = serviceKey.OpenSubKey(_ServiceConfigDevicesSubKey, RegistryKeyPermissionCheck.ReadSubTree);
                        k = baseKey32.OpenSubKey(_ServiceConfigDevicesSubKey64, RegistryKeyPermissionCheck.ReadSubTree, RegistryRights.FullControl);
                    }
                    return k.GetSubKeyNames();
                }
                catch (Exception ex)
                {
                    k = null;
                    Console.WriteLine("Exception : fails to open LOCAL_MACHINE DeviceNames {0}", ex.Message);
                    return null;
                }
            }
		}

        public RegistryKey DeviceKey(string deviceName, Mode mode)
		{
			RegistryKey k = null;

            try
            {
                if (baseKey64 != null)
                {
                    k = baseKey64.OpenSubKey(_ServiceConfigDevicesSubKey64, RegistryKeyPermissionCheck.ReadSubTree, RegistryRights.FullControl);
                }
                else if (baseKey32 != null)
                {
                    k = baseKey32.OpenSubKey(_ServiceConfigDevicesSubKey64, RegistryKeyPermissionCheck.ReadSubTree, RegistryRights.FullControl);
                }

                if ( (mode == Mode.ReadOnly) && (k != null))
                {
                    k = k.OpenSubKey(deviceName, RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryRights.ReadKey);
                }
                else if (k != null)
                {
                    k = k.OpenSubKey(deviceName, RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryRights.FullControl);
                }

                return k;
            }
            catch (Exception ex)
            {
                k = null;
                Console.WriteLine("Exception : fails to open LOCAL_MACHINE DeviceKey {0} {1}", deviceName, ex.Message);
                return null;
            }
        }
        
		public RegistryKey DeviceKey(string deviceName)
		{
			return DeviceKey(deviceName, this.mode);
		}

		public RegistryKey RuntimeKey(string deviceName, Mode mode)
		{
			RegistryKey k = null;

            try
            {
                if (baseKey64 != null)
                {
                    k = baseKey64.OpenSubKey(_ServiceConfigRuntimeSubKey64, RegistryKeyPermissionCheck.ReadSubTree, RegistryRights.FullControl);
                }
                else if (baseKey32 != null)
                {
                    k = baseKey32.OpenSubKey(_ServiceConfigDevicesSubKey64, RegistryKeyPermissionCheck.ReadSubTree, RegistryRights.FullControl);
                }

                if ( (mode == Mode.ReadOnly) && (k != null))
                {
                    k = k.OpenSubKey(deviceName, RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryRights.ReadKey);
                }
                else if (k != null)
                {
                    k = k.OpenSubKey(deviceName, RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryRights.FullControl);
                }

                return k;
            }
            catch (Exception ex)
            {
                k = null;
                Console.WriteLine("Exception : fails to open LOCAL_MACHINE RuntimeKey {0}", ex.Message);
                return null;
            }
        }
        
		public RegistryKey RuntimeKey(string deviceName)
		{
			return RuntimeKey(deviceName, this.mode);
		}

        public RegistryKey DebugKey(Mode mode)
        {
            RegistryKey k = null;

            try
            {
                if (baseKey64 != null)
                {
                    k = baseKey64.OpenSubKey(_ServiceConfigDebugSubKey64, RegistryKeyPermissionCheck.ReadSubTree, RegistryRights.FullControl);
                }
                else if (baseKey32 != null)
                {
                    k = baseKey32.OpenSubKey(_ServiceConfigDebugSubKey64, RegistryKeyPermissionCheck.ReadSubTree, RegistryRights.FullControl);
                }

                return k;
            }
            catch (Exception ex)
            {
                k = null;
                Console.WriteLine("Exception : fails to open LOCAL_MACHINE DebugKey {0}", ex.Message);
                return null;
            }
        }

        public RegistryKey DebugKey()
        {
            return DebugKey(this.mode);
        }

        public bool Valid
        {
			get {
                if (baseKey64 != null)
                {
                    return true;
                }
                if (baseKey32 != null)
                {
                    return true;
                }
                return false;
			}
		}

		public void InstallDevice(string DeviceName, string HostName, string FriendlyName)
		{
			RegistryKey k = null;
            //RegistryKey key = null;
            try
            {
                if (baseKey64 != null)
                {
                    k = baseKey64.OpenSubKey(_ServiceConfigDevicesSubKey64, RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryRights.FullControl);
                }
                else if (baseKey32 != null)
                {
                    k = baseKey32.OpenSubKey(_ServiceConfigDevicesSubKey64, RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryRights.FullControl);
                }
                if (k != null)
                {
                    RegistryKey deviceKey = k.CreateSubKey(DeviceName, RegistryKeyPermissionCheck.ReadWriteSubTree);
                    if (deviceKey != null)
                    {
                        deviceKey.SetValue("HostName", HostName, RegistryValueKind.String);
                        deviceKey.SetValue("TcpPort", 0x00000f9f, RegistryValueKind.DWord);
                        deviceKey.SetValue("FriendlyName", FriendlyName, RegistryValueKind.String);

                        deviceKey.SetValue("SecureMode", 0, RegistryValueKind.DWord);

                        deviceKey.SetValue("PingInterval", 10, RegistryValueKind.DWord);
                        deviceKey.SetValue("PingTimeout", 1, RegistryValueKind.DWord);
                        deviceKey.SetValue("ProbeInterval", 3, RegistryValueKind.DWord);
                        deviceKey.SetValue("ProbeTimeout", 3, RegistryValueKind.DWord);

                        deviceKey.SetValue("SleepAfterDisconnect", 15, RegistryValueKind.DWord);

                        deviceKey.SetValue("BulkInTimeout", 10, RegistryValueKind.DWord);
                        deviceKey.SetValue("RDRtoPCTimeout", 10, RegistryValueKind.DWord);

                        deviceKey.SetValue("EnumInterfaceTimeout", 120, RegistryValueKind.DWord);
                        deviceKey.Close();
                    }
                    k.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("UninstallDevice : {0}" + ex.Message);
            }

            
        }
        public void InstallRuntime(string DeviceName, string HostName, string FriendlyName)
        {            
            RegistryKey key = null;
            try
            {
                if (baseKey64 != null)
                {
                    key = baseKey64.OpenSubKey(_ServiceConfigRuntimeSubKey64, RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryRights.FullControl);
                }
                else if (baseKey32 != null)
                {
                    key = baseKey32.OpenSubKey(_ServiceConfigRuntimeSubKey64, RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryRights.FullControl);
                }
                if (key != null)
                {
                    RegistryKey runtimeKey = key.CreateSubKey(DeviceName, RegistryKeyPermissionCheck.ReadWriteSubTree);
                    if (runtimeKey != null)
                    {
                        runtimeKey.SetValue("DriverId", "", RegistryValueKind.String);
                        runtimeKey.SetValue("SlotCount", "0", RegistryValueKind.String);
                        runtimeKey.SetValue("LastConnection", DateTime.Now.ToString("O"), RegistryValueKind.String);
                        runtimeKey.SetValue("VendorName", "", RegistryValueKind.String);
                        runtimeKey.SetValue("ProductName", "", RegistryValueKind.String);
                        runtimeKey.SetValue("Version", "", RegistryValueKind.String);
                        runtimeKey.SetValue("SerialNumber", "", RegistryValueKind.String);

                        runtimeKey.Close();
                    }
                    key.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("InstallRuntime : {0}" + ex.Message);
            }
        }

        public void UninstallDevice(string deviceName)
		{
			RegistryKey k = null;
            RegistryKey key = null;
            try
            {
                if (baseKey64 != null)
                {
                    k = baseKey64.OpenSubKey(_ServiceConfigDevicesSubKey64, RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryRights.FullControl);
                }
                else if (baseKey32 != null)
                {
                    k = baseKey32.OpenSubKey(_ServiceConfigDevicesSubKey64, RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryRights.FullControl);
                }
                if (k != null)
                {
                    k.DeleteSubKeyTree(deviceName);
                    k.Close();
                }

                if (baseKey64 != null)
                {
                    key = baseKey64.OpenSubKey(_ServiceConfigRuntimeSubKey64, RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryRights.FullControl);
                }
                else if (baseKey32 != null)
                {
                    key = baseKey32.OpenSubKey(_ServiceConfigRuntimeSubKey64, RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryRights.FullControl);
                }
                if (key != null)
                {
                    key.DeleteSubKeyTree(deviceName);
                    key.Close();
                }
            }
            catch( Exception ex )
            {
                Console.WriteLine("UninstallDevice : {0}" + ex.Message);
            }
            
		}
        public void UninstallRuntime(string deviceName)
        {            
            RegistryKey key = null;

            try
            {
                if (baseKey64 != null)
                {
                    key = baseKey64.OpenSubKey(_ServiceConfigRuntimeSubKey64, RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryRights.FullControl);
                }
                else if (baseKey32 != null)
                {
                    key = baseKey32.OpenSubKey(_ServiceConfigRuntimeSubKey64, RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryRights.FullControl);
                }
                if (key != null)
                {
                    key.DeleteSubKeyTree(deviceName);
                    key.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("UninstallRuntime : {0}" + ex.Message);
            }
        }
        public void InstallDebug()
        {
            RegistryKey k = null;
            try
            {
                if (baseKey64 != null)
                {
                    k = baseKey64.OpenSubKey(_ServiceConfigDebugSubKey64, RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryRights.FullControl);
                }
                else if (baseKey32 != null)
                {
                    k = baseKey32.OpenSubKey(_ServiceConfigDebugSubKey64, RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryRights.FullControl);
                }
                if (k != null)
                {

                    string test = (string)k.GetValue("IPSysLog","null");
                    if (test == "null")
                    {
                        k.SetValue("IPSysLog", "0.0.0.0", RegistryValueKind.String);
                        k.SetValue("debugCode", 0x00000003, RegistryValueKind.DWord);
                    }
                    k.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("InstallDebug : {0}" + ex.Message);
            }
        }

        public object LocalMachine { get; private set; }

		public class DeviceConfig
		{
			private CcidNetworkServiceConfig parentServiceConfig;
			private RegistryKey deviceKey;
			private string deviceName;

			public DeviceConfig(CcidNetworkServiceConfig parentServiceConfig, string deviceName)
			{
				this.parentServiceConfig = parentServiceConfig;
				this.deviceKey = parentServiceConfig.DeviceKey(deviceName);
				this.deviceName = deviceName;
			}
            
			public RegistryKey RuntimeKey(CcidNetworkServiceConfig.Mode mode)
			{
				return parentServiceConfig.RuntimeKey(this.deviceName, mode);
			}
			
			public string Name {
				get {
					return this.deviceName;
				}
			}
			
			public string HostName {
				get {
					return (string)deviceKey.GetValue("HostName", this.deviceName);
				}
			}

			public ushort TcpPort {
				get {
					return (ushort)System.Convert.ToUInt16(deviceKey.GetValue("TcpPort", 3999));
				}
			}

            public string FriendlyName
            {
                get
                {
                    return (string)deviceKey.GetValue("FriendlyName", "");
                }
                set
                {
                    deviceKey.SetValue("FriendlyName", value, RegistryValueKind.String);
                }
            }

            public bool SecureMode {
				get {
					return (bool)System.Convert.ToBoolean(deviceKey.GetValue("SecureMode", false));
				}
			}
			
			public byte[] AuthKey {
				get {
					return null;
				}
			}
			
			public int SleepAfterDisconnect {
				get {
					return (int)System.Convert.ToUInt32(deviceKey.GetValue("SleepAfterDisconnect", 15));
				}
			}
			
			public int PingInterval {
				get {
					return (int)System.Convert.ToUInt32(deviceKey.GetValue("PingInterval", 10));
				}				
			}
			public int PingTimeout {
				get {
					return (int)System.Convert.ToUInt32(deviceKey.GetValue("PingTimeout", 1));
				}
			}
			public int ProbeInterval {
				get {
					return (int)System.Convert.ToUInt32(deviceKey.GetValue("ProbeInterval", 3));
				}				
			}
			public int ProbeTimeout {
				get {
					return (int)System.Convert.ToUInt32(deviceKey.GetValue("ProbeTimeout", 3));
				}
			}
			
			public int BulkInTimeout {
				get {
					return (int)System.Convert.ToUInt32(deviceKey.GetValue("BulkInTimeout", 10));
				}
			}
			public int RDRtoPCTimeout {
				get {
					return (int)System.Convert.ToUInt32(deviceKey.GetValue("RDRtoPCTimeout", 10));
				}
			}
            public int EnumInterfaceTimeout
            {
                get
                {
                    return (int)System.Convert.ToUInt32(deviceKey.GetValue("EnumInterfaceTimeout", 120));
                }
            }
        }

		public class DeviceRuntime
		{
			public RegistryKey runtimeKey;
			private string deviceName;

			public DeviceRuntime(CcidNetworkServiceConfig parentServiceConfig, string deviceName)
			{
				this.runtimeKey = parentServiceConfig.RuntimeKey(deviceName);
				this.deviceName = deviceName;
			}
            
			public DeviceRuntime(CcidNetworkServiceConfig.DeviceConfig deviceConfig, CcidNetworkServiceConfig.Mode mode)
			{
				this.runtimeKey = deviceConfig.RuntimeKey(mode);
				this.deviceName = deviceConfig.Name;
			}
            
			public string Name {
				get {
					return this.deviceName;
				}
			}

			public string DriverID {
				get {
					return (string)runtimeKey.GetValue("DriverID", "");
				}
				set {
					runtimeKey.SetValue("DriverID", value, RegistryValueKind.String);
				}
			}
            
			public string LastConnection {
				get {
					return (string)runtimeKey.GetValue("LastConnection", "");
				}
				set {
					runtimeKey.SetValue("LastConnection", value, RegistryValueKind.String);
				}            	
			}
            
			public string VendorName {
				get {
					return (string)runtimeKey.GetValue("VendorName", "");
				}
				set {
					runtimeKey.SetValue("VendorName", value, RegistryValueKind.String);
				}
			}
            
			public string ProductName {
				get {
					return (string)runtimeKey.GetValue("ProductName", "");
				}
				set {
					runtimeKey.SetValue("ProductName", value, RegistryValueKind.String);
				}
			}
            
			public string SerialNumber {
				get {
					return (string)runtimeKey.GetValue("SerialNumber", "");
				}
				set {
					runtimeKey.SetValue("SerialNumber", value, RegistryValueKind.String);
				}
			}
            
			public string Version {
				get {
					return (string)runtimeKey.GetValue("Version", "");
				}
				set {
					runtimeKey.SetValue("Version", value, RegistryValueKind.String);
				}
			}
            
			public int SlotCount {
				get {
					return (int)System.Convert.ToInt32(runtimeKey.GetValue("SlotCount", 0));
				}
				set {
					runtimeKey.SetValue("SlotCount", String.Format("{0}", value), RegistryValueKind.String);
				}
			}
		}

        
        public class DebugConfig
        {
            private CcidNetworkServiceConfig parentServiceConfig;
            private RegistryKey debugKey;

            public DebugConfig(CcidNetworkServiceConfig parentServiceConfig)
            {
                this.parentServiceConfig = parentServiceConfig;
                this.debugKey = parentServiceConfig.DebugKey();
            }

            public string IPSysLog
            {
                get
                {
                    return (string)debugKey.GetValue("IPSysLog", "");
                }
            }

            public int debugCode
            {
                get
                {
                    return (int)System.Convert.ToUInt32(debugKey.GetValue("debugCode", 0));
                }
            }
        }
    }
}
