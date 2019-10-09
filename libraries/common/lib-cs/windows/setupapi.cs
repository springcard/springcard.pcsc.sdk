/*
 * Author : herve.t@springcard.com
 * Date: 19/09/2014
 */

using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Management;

namespace SpringCard.LibCs.Windows
{
	/// <summary>
	/// A class used to activate / deactivate and reset USB devices connected to the computer
	/// 
	/// Exemple of use :
	///     setupapi.ActivateDevice("SpringCard Prox'N'Roll");
	///     setupapi.DeactivateDevice("SpringCard Prox'N'Roll");
	///     setupapi.ResetDevice("SpringCard Prox'N'Roll");
	///
	/// </summary>
	public static class setupapi
	{
		#region structures
		const uint DIF_PROPERTYCHANGE = 0x12;
		const uint DICS_ENABLE = 1;
		const uint DICS_DISABLE = 2;        // disable device
		const uint DICS_PROPCHANGE = 3;     // Reset
		const uint DICS_FLAG_GLOBAL = 1;    // not profile-specific

		const uint DICS_FLAG_CONFIGSPECIFIC = (0x00000002);

		const uint DIGCF_ALLCLASSES = 		0x00000004;
		const uint DIGCF_PROFILE = 			0x00000008;
		const uint DIGCF_PRESENT = 			0x00000002;
		const uint DIGCF_DEFAULT = 			0x00000001;
		const uint DIGCF_DEVICEINTERFACE = 	0x00000010;


		const uint ERROR_INVALID_DATA = 13;
		const uint ERROR_NO_MORE_ITEMS = 259;
		const uint ERROR_ELEMENT_NOT_FOUND = 1168;


		static DEVPROPKEY DEVPKEY_Device_DeviceDesc;
		static DEVPROPKEY DEVPKEY_Device_HardwareIds;

		[StructLayout(LayoutKind.Sequential)]
		struct SP_CLASSINSTALL_HEADER
		{
			public UInt32 cbSize;
			public UInt32 InstallFunction;
		}

		[StructLayout(LayoutKind.Sequential)]
		struct SP_PROPCHANGE_PARAMS
		{
			public SP_CLASSINSTALL_HEADER ClassInstallHeader;
			public UInt32 StateChange;
			public UInt32 Scope;
			public UInt32 HwProfile;
		}

		[StructLayout(LayoutKind.Sequential)]
		struct SP_DEVINFO_DATA
		{
			public UInt32 cbSize;
			public Guid classGuid;
			public UInt32 devInst;
			public IntPtr reserved;
		}

		[StructLayout(LayoutKind.Sequential)]
		struct DEVPROPKEY
		{
			public Guid fmtid;
			public UInt32 pid;
		}

		enum DeviceState
		{
			On,
			Off,
			Reset
		}

		private static bool stateChanged = false;
		private static bool canStop = false;
		#endregion

		#region DLL Imports
		[DllImport("setupapi.dll", SetLastError = true)]
		static extern IntPtr SetupDiGetClassDevsW([In] ref Guid ClassGuid, [MarshalAs(UnmanagedType.LPWStr)] string Enumerator, IntPtr parent, UInt32 flags);

		[DllImport("setupapi.dll", SetLastError = true)]
		static extern bool SetupDiDestroyDeviceInfoList(IntPtr handle);

		[DllImport("setupapi.dll", SetLastError = true)]
		static extern bool SetupDiEnumDeviceInfo(IntPtr deviceInfoSet, UInt32 memberIndex, [Out] out SP_DEVINFO_DATA deviceInfoData);

		[DllImport("setupapi.dll", SetLastError = true)]
		static extern bool SetupDiSetClassInstallParams(IntPtr deviceInfoSet, [In] ref SP_DEVINFO_DATA deviceInfoData, [In] ref SP_PROPCHANGE_PARAMS classInstallParams, UInt32 ClassInstallParamsSize);

		[DllImport("setupapi.dll", SetLastError = true)]
		static extern bool SetupDiChangeState(IntPtr deviceInfoSet, [In] ref SP_DEVINFO_DATA deviceInfoData);
		
		[DllImport("setupapi.dll", SetLastError = true)]
		static extern bool SetupDiGetDevicePropertyW(IntPtr deviceInfoSet, [In] ref SP_DEVINFO_DATA DeviceInfoData, [In] ref DEVPROPKEY propertyKey, [Out] out UInt32 propertyType, IntPtr propertyBuffer, UInt32 propertyBufferSize, out UInt32 requiredSize, UInt32 flags);

		[DllImport("setupapi.dll", SetLastError = true)]
		static extern bool SetupDiGetDeviceRegistryPropertyW(IntPtr DeviceInfoSet, [In] ref SP_DEVINFO_DATA DeviceInfoData, UInt32 Property, [Out] out UInt32 PropertyRegDataType, IntPtr PropertyBuffer, UInt32 PropertyBufferSize, [In, Out] ref UInt32 RequiredSize);
		#endregion

		#region Public methods
		static setupapi()
		{
			setupapi.DEVPKEY_Device_DeviceDesc = new DEVPROPKEY();
			DEVPKEY_Device_DeviceDesc.fmtid = new Guid(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67,0xd1, 0x46, 0xa8, 0x50, 0xe0);
			DEVPKEY_Device_DeviceDesc.pid = 2;

			DEVPKEY_Device_HardwareIds = new DEVPROPKEY();
			DEVPKEY_Device_HardwareIds.fmtid = new Guid(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67,0xd1, 0x46, 0xa8, 0x50, 0xe0);
			DEVPKEY_Device_HardwareIds.pid = 3;
		}

		/// <summary>
		/// Activate a device via its parts
		/// </summary>
		/// <param name="deviceName"></param>
		public static void ActivateDevice(string pid, string vid, string serialNumber)
		{
			if (string.IsNullOrEmpty(vid) || string.IsNullOrEmpty(pid) || string.IsNullOrEmpty(serialNumber))
				return;

			ChangeDeviceState(pid, vid, serialNumber, DeviceState.On);
		}

		/// <summary>
		/// Activate a device via its parts
		/// </summary>
		/// <param name="deviceName"></param>
		public static void DeactivateDevice(string pid, string vid, string serialNumber)
		{
			if (string.IsNullOrEmpty(vid) || string.IsNullOrEmpty(pid) || string.IsNullOrEmpty(serialNumber))
				return;

			ChangeDeviceState(pid, vid, serialNumber, DeviceState.Off);
		}

		/// <summary>
		/// Reset a device via its parts
		/// </summary>
		/// <param name="deviceName"></param>
		public static void ResetDevice(string pid, string vid, string serialNumber)
		{
			if (string.IsNullOrEmpty(vid) || string.IsNullOrEmpty(pid) || string.IsNullOrEmpty(serialNumber))
				return;

			ChangeDeviceState(pid, vid, serialNumber, DeviceState.Reset);
		}
		#endregion


		#region Private methods
		/// <summary>
		/// Change a device (via its name) state (activate / deactivate / reset)
		/// </summary>
		/// <param name="pid"></param>
		/// <param name="vid"></param>
		/// <param name="serialNumber"></param>
		/// <param name="status"></param>
		/// <param name="listbox"></param>
		private static void ChangeDeviceState(string pid, string vid, string serialNumber, DeviceState status)
		{
			serialNumber = serialNumber.ToUpper();
			IntPtr info = IntPtr.Zero;
			Guid NullGuid = Guid.Empty;
			stateChanged = false;
			try
			{
				info = SetupDiGetClassDevsW(ref NullGuid, null, IntPtr.Zero, DIGCF_ALLCLASSES | DIGCF_PRESENT);
				CheckError("SetupDiGetClassDevs");

				SP_DEVINFO_DATA devdata = new SP_DEVINFO_DATA();
				devdata.cbSize = (UInt32)Marshal.SizeOf(devdata);

				for (uint i = 0; ; i++)
				{
					SetupDiEnumDeviceInfo(info, i, out devdata);
					if (Marshal.GetLastWin32Error() == ERROR_NO_MORE_ITEMS)
						CheckError("No device found matching filter.", 0xcffff);
					CheckError("SetupDiEnumDeviceInfo");
					if(canStop)
						break;

					string devicepath = GetStringPropertyForDevice(info, devdata, 1);	// 0 & 1

					if (string.IsNullOrEmpty(devicepath))
						continue;

					if (devicepath != null && IsUsbHardwareId(devicepath))
					{
						string reconstPid = "", reconstVid = "", foundedSerialNumber = "";
						ExtractPidAndVid(devicepath, out reconstPid, out reconstVid);
						foundedSerialNumber = GetSerialNumberFromWMI(reconstPid, reconstVid);

						if (serialNumber.Equals(foundedSerialNumber))
						{
							stateChanged = true;
							ChangeReaderStatus(info, devdata, status);
						}
					}
				}
			}
			finally
			{
				if (info != IntPtr.Zero)
					SetupDiDestroyDeviceInfoList(info);
			}
		}

		/// <summary>
		/// Part in charge to really change the device's state
		/// </summary>
		/// <param name="info"></param>
		/// <param name="devdata"></param>
		/// <param name="status"></param>
		private static void ChangeReaderStatus(IntPtr info, SP_DEVINFO_DATA devdata, DeviceState status)
		{
			string devicepathTemp = GetStringPropertyForDevice(info, devdata, 0);
			SP_CLASSINSTALL_HEADER header = new SP_CLASSINSTALL_HEADER();
			header.cbSize = (UInt32)Marshal.SizeOf(header);
			header.InstallFunction = DIF_PROPERTYCHANGE;

			SP_PROPCHANGE_PARAMS propchangeparams = new SP_PROPCHANGE_PARAMS();
			propchangeparams.ClassInstallHeader = header;
			if(status == DeviceState.On || status == DeviceState.Off)
			{
				propchangeparams.StateChange = (status == DeviceState.Off) ? DICS_DISABLE : DICS_ENABLE;
				propchangeparams.Scope = DICS_FLAG_GLOBAL;
			}
			else    // reset
			{
				propchangeparams.StateChange = DICS_PROPCHANGE;
				propchangeparams.Scope = DICS_FLAG_CONFIGSPECIFIC;
			}
			propchangeparams.HwProfile = 0;

			SetupDiSetClassInstallParams(info, ref devdata, ref propchangeparams, (UInt32)Marshal.SizeOf(propchangeparams));
			CheckError("SetupDiSetClassInstallParams");

			SetupDiChangeState(info, ref devdata);
			CheckError("SetupDiChangeState");
		}

		/// <summary>
		/// Check for an error
		/// </summary>
		/// <param name="message"></param>
		/// <param name="lasterror"></param>
		private static void CheckError(string message, int lasterror = -1)
		{

			int code = lasterror == -1 ? Marshal.GetLastWin32Error() : lasterror;
			if (code != 0)
			{
				if (!stateChanged)
					throw new ApplicationException(String.Format("Error disabling hardware device (Code {0}): {1}", code, message));
				else
					canStop = true;
			}
		}

		/// <summary>
		/// Get a device's information
		/// </summary>
		/// <param name="info"></param>
		/// <param name="devdata"></param>
		/// <param name="propId"></param>
		/// <returns></returns>
		private static string GetStringPropertyForDevice(IntPtr info, SP_DEVINFO_DATA devdata, uint propId)
		{
			uint proptype, outsize;
			IntPtr buffer = IntPtr.Zero;
			try
			{
				uint buflen = 512;
				buffer = Marshal.AllocHGlobal((int)buflen);
				outsize = 0;
				SetupDiGetDeviceRegistryPropertyW(
					info,
					ref devdata,
					propId,
					out proptype,
					buffer,
					buflen,
					ref outsize);
				byte[] lbuffer = new byte[outsize];
				Marshal.Copy(buffer, lbuffer, 0, (int)outsize);
				int errcode = Marshal.GetLastWin32Error();
				if (errcode == ERROR_INVALID_DATA) return null;
				CheckError("SetupDiGetDeviceProperty", errcode);
				return Encoding.Unicode.GetString(lbuffer);
			}
			finally
			{
				if (buffer != IntPtr.Zero)
					Marshal.FreeHGlobal(buffer);
			}
		}

		/// <summary>
		/// Returns true is the string looks like an USB hardware ID on the form of "USB\VID_1C34&PID_7141..."
		/// </summary>
		/// <param name="hardwareID"></param>
		/// <returns></returns>
		private static bool IsUsbHardwareId(string hardwareID)
		{
			string normalizedString = hardwareID.ToUpper().Trim();
			if (normalizedString.StartsWith("USB") && normalizedString.Contains("VID_") && normalizedString.Contains("PID_"))
				return true;
			return false;
		}

		/// <summary>
		/// Extracts a Pid and Vid from a string
		/// </summary>
		/// <param name="hardwareID"></param>
		/// <param name="pid"></param>
		/// <param name="vid"></param>
		private static void ExtractPidAndVid(string hardwareID, out string pid, out string vid)
		{
			string normalizedString = hardwareID.ToUpper().Trim();
			vid = pid = "";
			// Vid
			if (normalizedString.StartsWith("USB") && normalizedString.Contains("VID_"))
				vid = normalizedString.Substring(normalizedString.IndexOf("VID_") + 4, 4).ToUpper();

			// Pid
			if (normalizedString.StartsWith("USB") && normalizedString.Contains("PID_"))
				pid = normalizedString.Substring(normalizedString.IndexOf("PID_") + 4, 4).ToUpper();
		}


		/// <summary>
		/// Extract an USB serial's number from a WMI DeviceId or PNPDeviceId
		/// The caller must ensure that the string smells like an hard id...
		/// </summary>
		/// <param name="hardwareId"></param>
		/// <returns></returns>
		private static string ExtractSerialNumber(string hardwareId)
		{
			if (hardwareId.IndexOf(@"\") == -1)
				return "";
			string serialNumber = "";
			try
			{
				serialNumber = hardwareId.Substring(hardwareId.LastIndexOf(@"\") + 1).Trim().ToUpper();
			}
			catch (Exception)
			{
			}
			return serialNumber;
		}

		/// <summary>
		/// Retreive an hardware's serial number from its PID and VID
		/// </summary>
		/// <param name="searchedVid"></param>
		/// <param name="searchedPid"></param>
		/// <returns></returns>
		private static string GetSerialNumberFromWMI(string searchedPid, string searchedVid)
		{
			if (string.IsNullOrEmpty(searchedVid) || string.IsNullOrEmpty(searchedPid))
				throw new ArgumentException("MissingParameters");

			try
			{
				ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * From Win32_PnPEntity WHERE (PNPDeviceID LIKE '%USB%VID%&PID%')  AND (PNPDeviceID LIKE '%VID_" + searchedVid + "%')  AND (PNPDeviceID LIKE '%PID_" + searchedPid + "%')");
				foreach (ManagementObject queryObj in searcher.Get())
				{
					if (queryObj["Status"].ToString().ToUpper().Equals("OK"))
					{
						if (IsUsbHardwareId(queryObj["DeviceID"].ToString()))
						{
							return ExtractSerialNumber(queryObj["DeviceID"].ToString());
						}
						else if (IsUsbHardwareId(queryObj["PNPDeviceID"].ToString()))
						{
							return ExtractSerialNumber(queryObj["PNPDeviceID"].ToString());
						}
					}
				}
			}
			catch (ManagementException e)
			{
				throw e;
			}
			return "";
		}
		#endregion
	}
}
