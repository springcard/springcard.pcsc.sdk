using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace SpringCard.LibCs.Windows
{
    public sealed partial class DeviceManagement
    {
        ///<summary >
        // API declarations relating to device management (SetupDixxx and
        // RegisterDeviceNotification functions).
        /// </summary>

        // from dbt.h

        internal const Int32 DBT_DEVICEARRIVAL = 0X8000;
        internal const Int32 DBT_DEVICEREMOVECOMPLETE = 0X8004;
        internal const Int32 DBT_DEVTYP_DEVICEINTERFACE = 5;
        internal const Int32 DBT_DEVTYP_HANDLE = 6;
        internal const Int32 DEVICE_NOTIFY_ALL_INTERFACE_CLASSES = 4;
        internal const Int32 DEVICE_NOTIFY_SERVICE_HANDLE = 1;
        internal const Int32 DEVICE_NOTIFY_WINDOW_HANDLE = 0;
        internal const Int32 WM_DEVICECHANGE = 0X219;

        // from setupapi.h

        internal const Int32 DIGCF_PRESENT = 2;
        internal const Int32 DIGCF_DEVICEINTERFACE = 0X10;

        // Two declarations for the DEV_BROADCAST_DEVICEINTERFACE structure.

        // Use this one in the call to RegisterDeviceNotification() and
        // in checking dbch_devicetype in a DEV_BROADCAST_HDR structure:

        [StructLayout(LayoutKind.Sequential)]
        internal class DEV_BROADCAST_DEVICEINTERFACE
        {
            internal Int32 dbcc_size;
            internal Int32 dbcc_devicetype;
            internal Int32 dbcc_reserved;
            internal Guid dbcc_classguid;
            internal Int16 dbcc_name;
        }

        // Use this to read the dbcc_name String and classguid:

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        internal class DEV_BROADCAST_DEVICEINTERFACE_1
        {
            internal Int32 dbcc_size;
            internal Int32 dbcc_devicetype;
            internal Int32 dbcc_reserved;
            [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 16)]
            internal Byte[] dbcc_classguid;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 255)]
            internal Char[] dbcc_name;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal class DEV_BROADCAST_HDR
        {
            internal Int32 dbch_size;
            internal Int32 dbch_devicetype;
            internal Int32 dbch_reserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct SP_DEVICE_INTERFACE_DATA
        {
            internal Int32 cbSize;
            internal System.Guid InterfaceClassGuid;
            internal Int32 Flags;
            internal IntPtr Reserved;
        }

        const int BUFFER_SIZE = 1024;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        struct SP_DEVICE_INTERFACE_DETAIL_DATA
        {
            public int cbSize;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = BUFFER_SIZE)]
            public string DevicePath;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct SP_DEVINFO_DATA
        {
            internal Int32 cbSize;
            internal System.Guid ClassGuid;
            internal Int32 DevInst;
            internal Int32 Reserved;
        }

        // from pinvoke.net
        private enum SPDRP : uint
        {
            SPDRP_DEVICEDESC = 0x00000000,
            SPDRP_HARDWAREID = 0x00000001,
            SPDRP_COMPATIBLEIDS = 0x00000002,
            SPDRP_NTDEVICEPATHS = 0x00000003,
            SPDRP_SERVICE = 0x00000004,
            SPDRP_CONFIGURATION = 0x00000005,
            SPDRP_CONFIGURATIONVECTOR = 0x00000006,
            SPDRP_CLASS = 0x00000007,
            SPDRP_CLASSGUID = 0x00000008,
            SPDRP_DRIVER = 0x00000009,
            SPDRP_CONFIGFLAGS = 0x0000000A,
            SPDRP_MFG = 0x0000000B,
            SPDRP_FRIENDLYNAME = 0x0000000C,
            SPDRP_LOCATION_INFORMATION = 0x0000000D,
            SPDRP_PHYSICAL_DEVICE_OBJECT_NAME = 0x0000000E,
            SPDRP_CAPABILITIES = 0x0000000F,
            SPDRP_UI_NUMBER = 0x00000010,
            SPDRP_UPPERFILTERS = 0x00000011,
            SPDRP_LOWERFILTERS = 0x00000012,
            SPDRP_MAXIMUM_PROPERTY = 0x00000013,

            SPDRP_ENUMERATOR_NAME = 0x16,
        };

        private enum RegTypes : int
        {
            // incomplete list, these are just the ones used.
            REG_SZ = 1,
            REG_MULTI_SZ = 7
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern IntPtr RegisterDeviceNotification(IntPtr hRecipient, IntPtr NotificationFilter, Int32 Flags);

        [DllImport("setupapi.dll", SetLastError = true)]
        internal static extern Int32 SetupDiCreateDeviceInfoList(ref System.Guid ClassGuid, Int32 hwndParent);

        [DllImport("setupapi.dll", SetLastError = true)]
        internal static extern Int32 SetupDiDestroyDeviceInfoList(IntPtr DeviceInfoSet);

        [DllImport("setupapi.dll", SetLastError = true)]
        internal static extern Boolean SetupDiEnumDeviceInterfaces(IntPtr DeviceInfoSet, IntPtr DeviceInfoData, ref System.Guid InterfaceClassGuid, Int32 MemberIndex, ref SP_DEVICE_INTERFACE_DATA DeviceInterfaceData);

        [DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool SetupDiGetDeviceRegistryProperty(IntPtr DeviceInfoSet, ref SP_DEVINFO_DATA DeviceInfoData, SPDRP Property, IntPtr PropertyRegDataType, IntPtr PropertyBuffer, uint PropertyBufferSize, out UInt32 RequiredSize);

        [DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool SetupDiGetDeviceRegistryProperty(IntPtr DeviceInfoSet, ref SP_DEVINFO_DATA DeviceInfoData, SPDRP Property, out UInt32 PropertyRegDataType, byte[] PropertyBuffer, uint PropertyBufferSize, out UInt32 RequiredSize);

        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern IntPtr SetupDiGetClassDevs(ref System.Guid ClassGuid, IntPtr Enumerator, IntPtr hwndParent, Int32 Flags);

        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool SetupDiGetDeviceInterfaceDetail(IntPtr DeviceInfoSet, ref SP_DEVICE_INTERFACE_DATA DeviceInterfaceData, ref SP_DEVICE_INTERFACE_DETAIL_DATA DeviceInterfaceDetailData, Int32 DeviceInterfaceDetailDataSize, ref Int32 RequiredSize, ref SP_DEVINFO_DATA DeviceInfoData);

        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool SetupDiGetDeviceInterfaceDetail(IntPtr DeviceInfoSet, ref SP_DEVICE_INTERFACE_DATA DeviceInterfaceData, IntPtr DeviceInterfaceDetailData, Int32 DeviceInterfaceDetailDataSize, ref Int32 RequiredSize, IntPtr DeviceInfoData);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern Boolean UnregisterDeviceNotification(IntPtr Handle);

        private const int ERROR_NO_MORE_ITEMS = 259;
        private const int ERROR_INSUFFICIENT_BUFFER = 122;

        ///  <summary>
        ///  Use SetupDi API functions to retrieve the device path name of an
        ///  attached device that belongs to a device interface class.
        ///  </summary>
        ///
        ///  <param name="guid"> an interface class GUID. </param>
        ///  <param name="devicePathName"> a pointer to the device path name
        ///  of an attached device. </param>
        ///
        ///  <returns>
        ///   True if a device is found, False if not.
        ///  </returns>

        public struct DeviceDetails
        {
            public string PathName;
            public string Manufacturer;
            public string Description;
            public ushort VID;
            public ushort PID;
        }

        private static byte[] GetProperty(IntPtr deviceInfoSet, SP_DEVINFO_DATA deviceInfoData, SPDRP property, out UInt32 regType)
        {
            uint requiredSize;
            regType = 0;

            if (!SetupDiGetDeviceRegistryProperty(deviceInfoSet, ref deviceInfoData, property, IntPtr.Zero, IntPtr.Zero, 0, out requiredSize))
            {
                if (Marshal.GetLastWin32Error() != ERROR_INSUFFICIENT_BUFFER)
                {
                    Logger.Debug("SetupDiGetDeviceRegistryProperty(size) failed with err.{0}", Marshal.GetLastWin32Error());
                    return null;
                }
            }

            byte[] buffer = new byte[requiredSize];

            if (!SetupDiGetDeviceRegistryProperty(deviceInfoSet, ref deviceInfoData, property, out regType, buffer, (uint)buffer.Length, out requiredSize))
            {
                Logger.Debug("SetupDiGetDeviceRegistryProperty(content) failed with err.{0}", Marshal.GetLastWin32Error());
                return null;
            }

            return buffer;
        }

        private static string GetStringProperty(IntPtr deviceInfoSet, SP_DEVINFO_DATA deviceInfoData, SPDRP property)
        {
            UInt32 regType;
            byte[] buffer = GetProperty(deviceInfoSet, deviceInfoData, property, out regType);
            if (regType != (int)RegTypes.REG_SZ)
                return null;

            // sizof(char), 2 bytes, are removed to leave out the string terminator
            return System.Text.Encoding.Unicode.GetString(buffer, 0, buffer.Length - sizeof(char));
        }

        private static string[] GetMultiStringProperty(IntPtr deviceInfoSet, SP_DEVINFO_DATA deviceInfoData, SPDRP property)
        {
            UInt32 regType;
            byte[] buffer = GetProperty(deviceInfoSet, deviceInfoData, property, out regType);
            if (regType != (int)RegTypes.REG_MULTI_SZ)
                return null;

            string fullString = System.Text.Encoding.Unicode.GetString(buffer);

            return fullString.Split(new char[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);

        }

        private static DeviceDetails GetDeviceDetails(string devicePath, IntPtr deviceInfoSet, SP_DEVINFO_DATA deviceInfoData)
        {
            DeviceDetails details = new DeviceDetails();

            details.PathName = devicePath;

            if (WinUtils.Verbose)
                Logger.Debug("{0}", details.PathName);

            details.Description = GetStringProperty(deviceInfoSet, deviceInfoData, SPDRP.SPDRP_DEVICEDESC);
            if (details.Description != null)
                if (WinUtils.Verbose)
                    Logger.Debug("\tDescription: {0}", details.Description);

            details.Manufacturer = GetStringProperty(deviceInfoSet, deviceInfoData, SPDRP.SPDRP_MFG);
            if (details.Manufacturer != null)
                if (WinUtils.Verbose)
                    Logger.Debug("\tManufacturer: {0}", details.Manufacturer);

            string[] hardwareIDs = GetMultiStringProperty(deviceInfoSet, deviceInfoData, SPDRP.SPDRP_HARDWAREID);
            if (hardwareIDs != null)
            {
                Regex regex = new Regex("^USB\\\\VID_([0-9A-F]{4})&PID_([0-9A-F]{4})", RegexOptions.IgnoreCase);

                foreach (string hardwareID in hardwareIDs)
                {
                    Match match = regex.Match(hardwareID);
                    if (match.Success)
                    {
                        details.VID = ushort.Parse(match.Groups[1].Value, System.Globalization.NumberStyles.AllowHexSpecifier);
                        details.PID = ushort.Parse(match.Groups[2].Value, System.Globalization.NumberStyles.AllowHexSpecifier);
                        break;
                    }
                }
            }
            else
            {
                Regex regex = new Regex("usb#vid_([0-9A-F]{4})&pid_([0-9A-F]{4})", RegexOptions.IgnoreCase);

                Match match = regex.Match(devicePath);
                if (match.Success)
                {
                    details.VID = ushort.Parse(match.Groups[1].Value, System.Globalization.NumberStyles.AllowHexSpecifier);
                    details.PID = ushort.Parse(match.Groups[2].Value, System.Globalization.NumberStyles.AllowHexSpecifier);
                }
            }

            if (WinUtils.Verbose)
                Logger.Debug("\tVID={0:X02}&PID={1:X02}", details.VID, details.PID);
            return details;
        }

        private static List<DeviceDetails> FindDeviceFromGuid(Guid guid)
		{
			Int32 bufferSize = 0;
			IntPtr deviceInfoSet = new System.IntPtr();
			Int32 memberIndex = 0;
			SP_DEVICE_INTERFACE_DATA MyDeviceInterfaceData = new SP_DEVICE_INTERFACE_DATA();
			Boolean success;
            List<DeviceDetails> result = new List<DeviceDetails>();

            if (WinUtils.Verbose)
                Logger.Debug("FindDeviceFromGuid({0})", guid.ToString());

            try
			{
                // ***
                //  API function

                //  summary
                //  Retrieves a device information set for a specified group of devices.
                //  SetupDiEnumDeviceInterfaces uses the device information set.

                //  parameters
                //  Interface class GUID.
                //  Null to retrieve information for all device instances.
                //  Optional handle to a top-level window (unused here).
                //  Flags to limit the returned information to currently present devices
                //  and devices that expose interfaces in the class specified by the GUID.

                //  Returns
                //  Handle to a device information set for the devices.
                // ***

                deviceInfoSet = SetupDiGetClassDevs(ref guid, IntPtr.Zero, IntPtr.Zero, DIGCF_PRESENT | DIGCF_DEVICEINTERFACE);

                //nbDeviceFound = 0;
                memberIndex = 0;

				// The cbSize element of the MyDeviceInterfaceData structure must be set to
				// the structure's size in bytes.
				// The size is 28 bytes for 32-bit code and 32 bits for 64-bit code.

				MyDeviceInterfaceData.cbSize = Marshal.SizeOf(MyDeviceInterfaceData);

                for (; ; )
				{
					// Begin with 0 and increment through the device information set until
					// no more devices are available.

					// ***
					//  API function

					//  summary
					//  Retrieves a handle to a SP_DEVICE_INTERFACE_DATA structure for a device.
					//  On return, MyDeviceInterfaceData contains the handle to a
					//  SP_DEVICE_INTERFACE_DATA structure for a detected device.

					//  parameters
					//  DeviceInfoSet returned by SetupDiGetClassDevs.
					//  Optional SP_DEVINFO_DATA structure that defines a device instance
					//  that is a member of a device information set.
					//  Device interface GUID.
					//  Index to specify a device in a device information set.
					//  Pointer to a handle to a SP_DEVICE_INTERFACE_DATA structure for a device.

					//  Returns
					//  True on success.
					// ***

					success = SetupDiEnumDeviceInterfaces
						(deviceInfoSet,
						 IntPtr.Zero,
						 ref guid,
						 memberIndex,
						 ref MyDeviceInterfaceData);

                    // Find out if a device information set was retrieved.

                    if (!success)
                        break;

                    // A device is present.

					// ***
					//  API function:

					//  summary:
					//  Retrieves an SP_DEVICE_INTERFACE_DETAIL_DATA structure
					//  containing information about a device.
					//  To retrieve the information, call this function twice.
					//  The first time returns the size of the structure.
					//  The second time returns a pointer to the data.

					//  parameters
					//  DeviceInfoSet returned by SetupDiGetClassDevs
					//  SP_DEVICE_INTERFACE_DATA structure returned by SetupDiEnumDeviceInterfaces
					//  A returned pointer to an SP_DEVICE_INTERFACE_DETAIL_DATA
					//  Structure to receive information about the specified interface.
					//  The size of the SP_DEVICE_INTERFACE_DETAIL_DATA structure.
					//  Pointer to a variable that will receive the returned required size of the
					//  SP_DEVICE_INTERFACE_DETAIL_DATA structure.
					//  Returned pointer to an SP_DEVINFO_DATA structure to receive information about the device.

					//  Returns
					//  True on success.
					// ***

					success = SetupDiGetDeviceInterfaceDetail
						(deviceInfoSet,
							ref MyDeviceInterfaceData,
							IntPtr.Zero,
							0,
							ref bufferSize,
							IntPtr.Zero);

                    // build a DevInfo Data structure
                    SP_DEVINFO_DATA da = new SP_DEVINFO_DATA();
                    da.cbSize = Marshal.SizeOf(da);

                    // build a Device Interface Detail Data structure
                    SP_DEVICE_INTERFACE_DETAIL_DATA didd = new SP_DEVICE_INTERFACE_DETAIL_DATA();
                    if (IntPtr.Size == 8) // for 64 bit operating systems
                        didd.cbSize = 8;
                    else
                        didd.cbSize = 4 + Marshal.SystemDefaultCharSize; // for 32 bit systems

                    // Call SetupDiGetDeviceInterfaceDetail again.
                    // This time, pass a pointer to DetailDataBuffer
                    // and the returned required buffer size.

                    success = SetupDiGetDeviceInterfaceDetail
						(deviceInfoSet,
							ref MyDeviceInterfaceData,
                            ref didd,
                            BUFFER_SIZE,
							ref bufferSize,
							ref da);

                    // Skip over cbsize (4 bytes) to get the address of the devicePathName.

                    DeviceDetails details = GetDeviceDetails(didd.DevicePath, deviceInfoSet, da);

                    result.Add(details);

					memberIndex = memberIndex + 1;
				}

                return result;
			}
			catch (Exception)
			{
				throw;
			}
			finally
			{

				// ***
				//  API function

				//  summary
				//  Frees the memory reserved for the DeviceInfoSet returned by SetupDiGetClassDevs.

				//  parameters
				//  DeviceInfoSet returned by SetupDiGetClassDevs.

				//  returns
				//  True on success.
				// ***

				if (deviceInfoSet != IntPtr.Zero)
				{
					SetupDiDestroyDeviceInfoList(deviceInfoSet);
				}
			}
        }

        public static bool FindDeviceFromGuid(Guid guid, ref List<String> devicePathNames)
        {
            List<DeviceDetails> deviceDetails = FindDeviceFromGuid(guid);
            if (deviceDetails == null)
                return false;
            devicePathNames = new List<string>();
            foreach (DeviceDetails deviceDetail in deviceDetails)
            {
                devicePathNames.Add(deviceDetail.PathName);
            }
            return true;
        }

        public static bool FindDeviceFromGuid(Guid guid, ref string[] devicePathNames)
        {
            List<String> result = null;
            if (!FindDeviceFromGuid(guid, ref result))
                return false;
            devicePathNames = result.ToArray();
            return true;
        }

        public static bool FindDeviceFromGuid(Guid guid, ref List<DeviceDetails> deviceDetails)
        {
            deviceDetails = FindDeviceFromGuid(guid);
            if (deviceDetails == null)
                return false;
            return true;
        }

        public static bool FindDeviceFromGuid(Guid guid, ref DeviceDetails[] deviceDetails)
        {
            List<DeviceDetails> deviceDetailsList = FindDeviceFromGuid(guid);
            if (deviceDetailsList == null)
                return false;
            deviceDetails = deviceDetailsList.ToArray();
            return true;
        }


        ///  <summary>
        ///  Requests to receive a notification when a device is attached or removed.
        ///  </summary>
        ///
        ///  <param name="devicePathName"> handle to a device. </param>
        ///  <param name="formHandle"> handle to the window that will receive device events. </param>
        ///  <param name="classGuid"> device interface GUID. </param>
        ///  <param name="deviceNotificationHandle"> returned device notification handle. </param>
        ///
        ///  <returns>
        ///  True on success.
        ///  </returns>
        ///
        internal Boolean RegisterForDeviceNotifications(String devicePathName, IntPtr formHandle, Guid classGuid, ref IntPtr deviceNotificationHandle)
		{
			// A DEV_BROADCAST_DEVICEINTERFACE header holds information about the request.

			DEV_BROADCAST_DEVICEINTERFACE devBroadcastDeviceInterface = new DEV_BROADCAST_DEVICEINTERFACE();
			IntPtr devBroadcastDeviceInterfaceBuffer = IntPtr.Zero;
			Int32 size = 0;

			try
			{
				// Set the parameters in the DEV_BROADCAST_DEVICEINTERFACE structure.

				// Set the size.

				size = Marshal.SizeOf(devBroadcastDeviceInterface);
				devBroadcastDeviceInterface.dbcc_size = size;

				// Request to receive notifications about a class of devices.

				devBroadcastDeviceInterface.dbcc_devicetype = DBT_DEVTYP_DEVICEINTERFACE;

				devBroadcastDeviceInterface.dbcc_reserved = 0;

				// Specify the interface class to receive notifications about.

				devBroadcastDeviceInterface.dbcc_classguid = classGuid;

				// Allocate memory for the buffer that holds the DEV_BROADCAST_DEVICEINTERFACE structure.

				devBroadcastDeviceInterfaceBuffer = Marshal.AllocHGlobal(size);

				// Copy the DEV_BROADCAST_DEVICEINTERFACE structure to the buffer.
				// Set fDeleteOld True to prevent memory leaks.

				Marshal.StructureToPtr(devBroadcastDeviceInterface, devBroadcastDeviceInterfaceBuffer, true);

				// ***
				//  API function

				//  summary
				//  Request to receive notification messages when a device in an interface class
				//  is attached or removed.

				//  parameters
				//  Handle to the window that will receive device events.
				//  Pointer to a DEV_BROADCAST_DEVICEINTERFACE to specify the type of
				//  device to send notifications for.
				//  DEVICE_NOTIFY_WINDOW_HANDLE indicates the handle is a window handle.

				//  Returns
				//  Device notification handle or NULL on failure.
				// ***

				deviceNotificationHandle = RegisterDeviceNotification(formHandle, devBroadcastDeviceInterfaceBuffer, DEVICE_NOTIFY_WINDOW_HANDLE);

				// Marshal data from the unmanaged block devBroadcastDeviceInterfaceBuffer to
				// the managed object devBroadcastDeviceInterface

				Marshal.PtrToStructure(devBroadcastDeviceInterfaceBuffer, devBroadcastDeviceInterface);



				if ((deviceNotificationHandle.ToInt32() == IntPtr.Zero.ToInt32()))
				{
					return false;
				}
				else
				{
					return true;
				}
			}
			catch (Exception)
			{
				throw;
			}
			finally
			{
				if (devBroadcastDeviceInterfaceBuffer != IntPtr.Zero)
				{
					// Free the memory allocated previously by AllocHGlobal.

					Marshal.FreeHGlobal(devBroadcastDeviceInterfaceBuffer);
				}
			}
		}

		///  <summary>
		///  Requests to stop receiving notification messages when a device in an
		///  interface class is attached or removed.
		///  </summary>
		///
		///  <param name="deviceNotificationHandle"> handle returned previously by
		///  RegisterDeviceNotification. </param>

		internal void StopReceivingDeviceNotifications(IntPtr deviceNotificationHandle)
		{
			try
			{
				// ***
				//  API function

				//  summary
				//  Stop receiving notification messages.

				//  parameters
				//  Handle returned previously by RegisterDeviceNotification.

				//  returns
				//  True on success.
				// ***

				//  Ignore failures.

				DeviceManagement.UnregisterDeviceNotification(deviceNotificationHandle);
			}
			catch (Exception)
			{
				throw;
			}
		}
	}
}
