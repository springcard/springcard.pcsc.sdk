/*
 * Created by SharpDevelop.
 * Author : Hervé Thouzard
 * Date: 19/09/2014
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace SpringCard.LibCs
{
	
	public class Native
	{
		[DllImport("kernel32.dll")]
		public static extern uint GetLastError();
		
		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern IntPtr RegisterDeviceNotification(IntPtr hRecipient, DEV_BROADCAST_DEVICEINTERFACE NotificationFilter, UInt32 Flags);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern uint UnregisterDeviceNotification(IntPtr hHandle);

		[DllImport("setupapi.dll", SetLastError = true)]
		public static extern IntPtr SetupDiGetClassDevs(ref Guid gClass, UInt32 iEnumerator, IntPtr hParent, UInt32 nFlags);

		[DllImport("setupapi.dll", SetLastError = true)]
		public static extern int SetupDiDestroyDeviceInfoList(IntPtr lpInfoSet);

		[DllImport("setupapi.dll", SetLastError = true)]
		public static extern bool SetupDiEnumDeviceInfo(IntPtr lpInfoSet, UInt32 dwIndex, SP_DEVINFO_DATA devInfoData);

		[DllImport("setupapi.dll", SetLastError = true)]
		public static extern bool SetupDiGetDeviceRegistryProperty(IntPtr lpInfoSet, SP_DEVINFO_DATA DeviceInfoData, UInt32 Property, UInt32 PropertyRegDataType, StringBuilder PropertyBuffer, UInt32 PropertyBufferSize, IntPtr RequiredSize);

		[DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
		public static extern bool SetupDiSetClassInstallParams(IntPtr DeviceInfoSet, IntPtr DeviceInfoData, IntPtr ClassInstallParams, int ClassInstallParamsSize);

		[DllImport("setupapi.dll", CharSet = CharSet.Auto)]
		public static extern Boolean SetupDiCallClassInstaller(UInt32 InstallFunction,IntPtr DeviceInfoSet, IntPtr DeviceInfoData);
		
		// Structure with information for RegisterDeviceNotification.
		[StructLayout(LayoutKind.Sequential)]
		public struct DEV_BROADCAST_HANDLE
		{
			public int dbch_size;
			public int dbch_devicetype;
			public int dbch_reserved;
			public IntPtr dbch_handle;
			public IntPtr dbch_hdevnotify;
			public Guid dbch_eventguid;
			public long dbch_nameoffset;
			public byte dbch_data;
			public byte dbch_data1;
		}

		// Struct for parameters of the WM_DEVICECHANGE message
		[StructLayout(LayoutKind.Sequential)]
		public class DEV_BROADCAST_DEVICEINTERFACE
		{
			public int dbcc_size;
			public int dbcc_devicetype;
			public int dbcc_reserved;
		}

		//SP_DEVINFO_DATA
		[StructLayout(LayoutKind.Sequential)]
		public class SP_DEVINFO_DATA
		{
			public int cbSize;
			public Guid classGuid;
			public int devInst;
			public ulong reserved;
		};

		[StructLayout(LayoutKind.Sequential)]
		public class SP_DEVINSTALL_PARAMS
		{
			public int cbSize;
			public int Flags;
			public int FlagsEx;
			public IntPtr hwndParent;
			public IntPtr InstallMsgHandler;
			public IntPtr InstallMsgHandlerContext;
			public IntPtr FileQueue;
			public IntPtr ClassInstallReserved;
			public int Reserved;
			[MarshalAs(UnmanagedType.LPTStr)] public string DriverPath;
		};

		[StructLayout(LayoutKind.Sequential)]
		public class SP_PROPCHANGE_PARAMS
		{
			public SP_CLASSINSTALL_HEADER ClassInstallHeader=new SP_CLASSINSTALL_HEADER();
			public int StateChange;
			public int Scope;
			public int HwProfile;
		};

		[StructLayout(LayoutKind.Sequential)]
		public class SP_CLASSINSTALL_HEADER
		{
			public int cbSize;
			public int InstallFunction;
		};

		//PARMS
		public const int DIGCF_ALLCLASSES = (0x00000004);
		public const int DIGCF_PRESENT = (0x00000002);
		public const int INVALID_HANDLE_VALUE = -1;
		public const int SPDRP_DEVICEDESC = (0x00000000);
		public const int MAX_DEV_LEN = 1000;
		public const int DEVICE_NOTIFY_WINDOW_HANDLE = (0x00000000);
		public const int DEVICE_NOTIFY_SERVICE_HANDLE = (0x00000001);
		public const int DEVICE_NOTIFY_ALL_INTERFACE_CLASSES = (0x00000004);
		public const int DBT_DEVTYP_DEVICEINTERFACE = (0x00000005);
		public const int DBT_DEVNODES_CHANGED = (0x0007);
		public const int WM_DEVICECHANGE = (0x0219);
		public const int DIF_PROPERTYCHANGE = (0x00000012);
		public const int DICS_FLAG_GLOBAL = (0x00000001);
		public const int DICS_FLAG_CONFIGSPECIFIC = (0x00000002);
		public const int DICS_PROPCHANGE = ((0x00000003));
		public const int DICS_ENABLE = (0x00000001);
		public const int DICS_DISABLE = (0x00000002);
	}
	
	/// <summary>
	/// A class used to list / activate / deactivate and reset USB devices connected to the computer
	/// 
	/// Example of use :
	/// 	setupapi usb = new setupapi(this.Handle);
	/// 
	/// 	To act on device's state:
	/// 		usb.ActivateDevice("name_of_the_device");
	/// 		usb.DeactivateDevice("name_of_the_device");
	/// 		usb.ResetDevice("name_of_the_device");
	/// 
	/// 	To list attached devices:
	/// 		foreach (string device in usb.GetDevicesList())
	/// 			listBox1.Items.Add(device);
	/// 
	/// 	If you wish to be notified of devices status change, add this method to your app:
	/// 			protected override void WndProc(ref Message m)
	/// 			{
	/// 							if (m.WParam.ToInt32() == SpringCard.LibCs.Native.DBT_DEVNODES_CHANGED)
	/// 							{
	/// 								listBox1.Items.Clear();
	/// 								string[] HardwareList = usb.GetDevicesList();
	/// 								foreach (string s in HardwareList)
	/// 								{
	/// 									//if(s.ToLower().StartsWith("s"))
	/// 										listBox1.Items.Add(s);
	/// 								}
	/// 								label1.Text = listBox1.Items.Count.ToString() + " Devices Attached";
	/// 							}
	/// 							break;
	/// 						}
	/// 				}
	/// 				base.WndProc(ref m);
	/// 			}
	/// 
	/// </summary>
	public class setupapi
	{
		private IntPtr hookingFormHandle = IntPtr.Zero;
		public enum DeviceAction { Disable, Reset, Enable };
		
		#region Public methods
		public setupapi()
		{
		}
		
		public setupapi(IntPtr formHandle)
		{
			hookingFormHandle = formHandle;
			HookHardwareNotifications(hookingFormHandle, true);
		}
		
		/// <summary>
		/// Used to hook a window on hardware events
		/// </summary>
		/// <param name="form"></param>
		public void HookFormToHardwareChange(IntPtr formHandle)
		{
			hookingFormHandle = formHandle;
		}
		
		~setupapi()
		{
			if(hookingFormHandle != IntPtr.Zero)
				DisconnectFromHardwareNotifications();
		}
		
		public void DisconnectFromHardwareNotifications()
		{
			if(hookingFormHandle == IntPtr.Zero)
				return;
			
			try
			{
				Native.UnregisterDeviceNotification(hookingFormHandle);
			}
			catch
			{
				//Just being extra cautious since the code is unmanged
			}
		}
		
		/// <summary>
		/// Activate a device by its name
		/// </summary>
		/// <param name="deviceName"></param>
		/// <returns></returns>
		public bool ActivateDevice(string deviceName)
		{
			if(string.IsNullOrEmpty(deviceName))
				return false;
			
			if(hookingFormHandle != IntPtr.Zero)
				DisconnectFromHardwareNotifications();
			
			ActionOnDevice(deviceName, DeviceAction.Enable);
			
			if(hookingFormHandle != IntPtr.Zero)
				HookHardwareNotifications(hookingFormHandle, true);
			
			return true;
		}
		
		/// <summary>
		/// Deactivate a device by its name
		/// </summary>
		/// <param name="deviceName"></param>
		/// <returns></returns>
		public bool DeactivateDevice(string deviceName)
		{
			if(string.IsNullOrEmpty(deviceName))
				return false;

			if(hookingFormHandle != IntPtr.Zero)
				DisconnectFromHardwareNotifications();
			
			ActionOnDevice(deviceName, DeviceAction.Disable);
			
			if(hookingFormHandle != IntPtr.Zero)
				HookHardwareNotifications(hookingFormHandle, true);
			
			return true;
		}
		
		/// <summary>
		/// Reset a device by its name
		/// </summary>
		/// <param name="deviceName"></param>
		/// <returns></returns>
		public bool ResetDevice(string deviceName)
		{
			if(string.IsNullOrEmpty(deviceName))
				return false;

			if(hookingFormHandle != IntPtr.Zero)
				DisconnectFromHardwareNotifications();
			
			ActionOnDevice(deviceName, DeviceAction.Reset);
			
			if(hookingFormHandle != IntPtr.Zero)
				HookHardwareNotifications(hookingFormHandle, true);
			
			return true;
		}
		
		/// <summary>
		/// Returns a sorted list of all computer's devices
		/// </summary>
		/// <returns></returns>
		public string[] GetDevicesList()
		{
			List<string> HWList = new List<string>();
			
			try
			{
				Guid myGUID = System.Guid.Empty;
				IntPtr hDevInfo = Native.SetupDiGetClassDevs(ref myGUID, 0, IntPtr.Zero, Native.DIGCF_ALLCLASSES | Native.DIGCF_PRESENT);
				if (hDevInfo.ToInt32() == Native.INVALID_HANDLE_VALUE)
				{
					throw new Exception("Invalid Handle");
				}
				Native.SP_DEVINFO_DATA DeviceInfoData;
				DeviceInfoData = new Native.SP_DEVINFO_DATA();
				DeviceInfoData.cbSize = Marshal.SizeOf(DeviceInfoData); // JDA 28;
				//is devices exist for class
				DeviceInfoData.devInst = 0;
				DeviceInfoData.classGuid = System.Guid.Empty;
				DeviceInfoData.reserved = 0;
				UInt32 i;
				StringBuilder DeviceName = new StringBuilder("");
				DeviceName.Capacity = Native.MAX_DEV_LEN;
				
				if (!Native.SetupDiEnumDeviceInfo(hDevInfo, 0, DeviceInfoData))
				{
					throw new Exception("No device found, are you kidding? " + Native.GetLastError());
				}
				
				for (i = 0; Native.SetupDiEnumDeviceInfo(hDevInfo, i, DeviceInfoData); i++)
				{
					
					//Declare vars
					while (!Native.SetupDiGetDeviceRegistryProperty(hDevInfo,
					                                                DeviceInfoData,
					                                                Native.SPDRP_DEVICEDESC,
					                                                0,
					                                                DeviceName,
					                                                Native.MAX_DEV_LEN,
					                                                IntPtr.Zero))
					{
						//Skip
					}
					HWList.Add(DeviceName.ToString());
				}
				HWList.Sort();
				Native.SetupDiDestroyDeviceInfoList(hDevInfo);
			}
			catch (Exception ex)
			{
				throw new Exception("Failed to enumerate device tree!",ex);
			}
			return HWList.ToArray();
		}
		
		#endregion
		
		#region Private methods
		private bool EnableOrDisableDevice(IntPtr hDevInfo, Native.SP_DEVINFO_DATA devInfoData, bool bEnable)
		{
			try
			{
				//Marshalling vars
				int szOfPcp;
				IntPtr ptrToPcp;
				int szDevInfoData;
				IntPtr ptrToDevInfoData;

				Native.SP_PROPCHANGE_PARAMS pcp = new Native.SP_PROPCHANGE_PARAMS();
				if (bEnable)
				{
					pcp.ClassInstallHeader.cbSize = Marshal.SizeOf(typeof(Native.SP_CLASSINSTALL_HEADER));
					pcp.ClassInstallHeader.InstallFunction = Native.DIF_PROPERTYCHANGE;
					pcp.StateChange = Native.DICS_ENABLE;
					pcp.Scope = Native.DICS_FLAG_GLOBAL;
					pcp.HwProfile = 0;
					
					//Marshal the params
					szOfPcp = Marshal.SizeOf(pcp);
					ptrToPcp = Marshal.AllocHGlobal(szOfPcp);
					Marshal.StructureToPtr(pcp, ptrToPcp, true);
					szDevInfoData = Marshal.SizeOf(devInfoData);
					ptrToDevInfoData = Marshal.AllocHGlobal(szDevInfoData);

					if (Native.SetupDiSetClassInstallParams(hDevInfo, ptrToDevInfoData, ptrToPcp, Marshal.SizeOf(typeof(Native.SP_PROPCHANGE_PARAMS))))
					{
						Native.SetupDiCallClassInstaller(Native.DIF_PROPERTYCHANGE, hDevInfo, ptrToDevInfoData);
					}
					pcp.ClassInstallHeader.cbSize = Marshal.SizeOf(typeof(Native.SP_CLASSINSTALL_HEADER));
					pcp.ClassInstallHeader.InstallFunction = Native.DIF_PROPERTYCHANGE;
					pcp.StateChange = Native.DICS_ENABLE;
					pcp.Scope = Native.DICS_FLAG_CONFIGSPECIFIC;
					pcp.HwProfile = 0;
				}
				else
				{
					pcp.ClassInstallHeader.cbSize = Marshal.SizeOf(typeof(Native.SP_CLASSINSTALL_HEADER));
					pcp.ClassInstallHeader.InstallFunction = Native.DIF_PROPERTYCHANGE;
					pcp.StateChange = Native.DICS_DISABLE;
					pcp.Scope = Native.DICS_FLAG_CONFIGSPECIFIC;
					pcp.HwProfile = 0;
				}
				//Marshal the params
				szOfPcp = Marshal.SizeOf(pcp);
				ptrToPcp = Marshal.AllocHGlobal(szOfPcp);
				Marshal.StructureToPtr(pcp, ptrToPcp, true);
				szDevInfoData = Marshal.SizeOf(devInfoData);
				ptrToDevInfoData = Marshal.AllocHGlobal(szDevInfoData);
				Marshal.StructureToPtr(devInfoData, ptrToDevInfoData,true);

				bool rslt1 = Native.SetupDiSetClassInstallParams(hDevInfo, ptrToDevInfoData, ptrToPcp, Marshal.SizeOf(typeof(Native.SP_PROPCHANGE_PARAMS)));
				bool rstl2 = Native.SetupDiCallClassInstaller(Native.DIF_PROPERTYCHANGE, hDevInfo, ptrToDevInfoData);
				if ((!rslt1) || (!rstl2))
				{
					throw new Exception("Unable to change device state!");
					return false;
				}
				else
				{
					return true;
				}
			}
			catch (Exception ex)
			{
				return false;
			}
		}
		
		private bool RestartDevice( IntPtr hDevInfo, Native.SP_DEVINFO_DATA devInfoData )
		{
			int szOfPcp;
			IntPtr ptrToPcp;
			int szDevInfoData;
			IntPtr ptrToDevInfoData;
			
			Native.SP_PROPCHANGE_PARAMS pcp = new Native.SP_PROPCHANGE_PARAMS();
			pcp.ClassInstallHeader.cbSize = Marshal.SizeOf(typeof(Native.SP_CLASSINSTALL_HEADER));
			pcp.ClassInstallHeader.InstallFunction = Native.DIF_PROPERTYCHANGE;
			pcp.StateChange = Native.DICS_PROPCHANGE; // for reset
			pcp.Scope = Native.DICS_FLAG_CONFIGSPECIFIC;
			pcp.HwProfile = 0;
			
			szOfPcp = Marshal.SizeOf(pcp);
			ptrToPcp = Marshal.AllocHGlobal(szOfPcp);
			Marshal.StructureToPtr(pcp, ptrToPcp, true);
			szDevInfoData = Marshal.SizeOf(devInfoData);
			ptrToDevInfoData = Marshal.AllocHGlobal(szDevInfoData);
			Marshal.StructureToPtr(devInfoData, ptrToDevInfoData, true);
			
			bool rslt1 = Native.SetupDiSetClassInstallParams(hDevInfo, ptrToDevInfoData, ptrToPcp, Marshal.SizeOf(typeof(Native.SP_PROPCHANGE_PARAMS)));
			
			if (!rslt1)
				throw new Exception("SetupDiSetClassInstallParams failed " + Native.GetLastError());
			
			bool rstl2 = Native.SetupDiCallClassInstaller(Native.DIF_PROPERTYCHANGE, hDevInfo, ptrToDevInfoData);

			if (!rstl2)
				throw new Exception("SetupDiCallClassInstaller failed" + Native.GetLastError());
			
			if (rslt1 && rstl2)
			{
				return true;
			}
			
			
			return false;
		}
		
		private bool ActionOnDevice(string deviceName, DeviceAction action)
		{
			bool bFound = false;
			
			Guid myGUID = System.Guid.Empty;
			IntPtr hDevInfo = Native.SetupDiGetClassDevs(ref myGUID, 0, IntPtr.Zero, Native.DIGCF_ALLCLASSES | Native.DIGCF_PRESENT);
			if (hDevInfo.ToInt32() == Native.INVALID_HANDLE_VALUE)
			{
				throw new Exception("Failed to open the device manager");
				return false;
			}
			Native.SP_DEVINFO_DATA DeviceInfoData;
			DeviceInfoData = new Native.SP_DEVINFO_DATA();
			DeviceInfoData.cbSize = Marshal.SizeOf(DeviceInfoData); // JDA 28;
			//is devices exist for class
			DeviceInfoData.devInst = 0;
			DeviceInfoData.classGuid = System.Guid.Empty;
			DeviceInfoData.reserved = 0;
			UInt32 i;
			StringBuilder DeviceName = new StringBuilder("");
			DeviceName.Capacity = Native.MAX_DEV_LEN;
			for (i = 0; Native.SetupDiEnumDeviceInfo(hDevInfo, i, DeviceInfoData); i++)
			{
				//Declare vars
				while (!Native.SetupDiGetDeviceRegistryProperty(hDevInfo, DeviceInfoData, Native.SPDRP_DEVICEDESC, 0, DeviceName, Native.MAX_DEV_LEN, IntPtr.Zero))
				{
					//Skip
				}
				if (DeviceName.ToString().ToLower().Contains(deviceName.ToLower()))
				{
					bFound = true;
					
					switch (action)
					{
						case DeviceAction.Disable :
							if (!EnableOrDisableDevice(hDevInfo, DeviceInfoData, false))
								throw new Exception("Failed to disable the device");
							break;
						case DeviceAction.Enable :
							if (!EnableOrDisableDevice(hDevInfo, DeviceInfoData, true))
								throw new Exception("Failed to enable the device");
							break;
						case DeviceAction.Reset :
							if (!RestartDevice(hDevInfo, DeviceInfoData))
								throw new Exception("Failed to reset the device");
							break;
							default :
								throw new Exception("Invalid action.");
					}
					break;
				}
			}
			Native.SetupDiDestroyDeviceInfoList(hDevInfo);

			if (!bFound)
				throw new Exception("Device not found");
			
			return true;
		}
		
		//Name:     HookHardwareNotifications
		//Inputs:   Handle to a window or service,
		//          Boolean specifying true if the handle belongs to a window
		//Outputs:  false if fail, otherwise true
		//Errors:   This method may log the following errors.
		//          NONE
		//Remarks:  Allow a window or service to receive ALL hardware notifications.
		//          NOTE: I have yet to figure out how to make this work properly
		//          for a service written in C#, though it kicks butt in C++.  At any
		//          rate, it works fine for windows forms in either.
		private bool HookHardwareNotifications(IntPtr callback, bool UseWindowHandle)
		{
			try
			{
				Native.DEV_BROADCAST_DEVICEINTERFACE dbdi = new Native.DEV_BROADCAST_DEVICEINTERFACE();
				dbdi.dbcc_size = Marshal.SizeOf(dbdi);
				dbdi.dbcc_reserved = 0;
				dbdi.dbcc_devicetype = Native.DBT_DEVTYP_DEVICEINTERFACE;
				if (UseWindowHandle)
				{
					Native.RegisterDeviceNotification(callback,
					                                  dbdi,
					                                  Native.DEVICE_NOTIFY_ALL_INTERFACE_CLASSES |
					                                  Native.DEVICE_NOTIFY_WINDOW_HANDLE);
				}
				else
				{
					Native.RegisterDeviceNotification(callback,
					                                  dbdi,
					                                  Native.DEVICE_NOTIFY_ALL_INTERFACE_CLASSES |
					                                  Native.DEVICE_NOTIFY_SERVICE_HANDLE);
				}
				return true;
			}
			catch (Exception ex)
			{
				string err = ex.Message;
				return false;
			}
		}

		#endregion
	}
}