using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace SpringCard.LibCs
{
	sealed internal partial class DEVICEMANAGEMENT
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

		internal struct SP_DEVICE_INTERFACE_DATA
		{
			internal Int32 cbSize;
			internal System.Guid InterfaceClassGuid;
			internal Int32 Flags;
			internal IntPtr Reserved;
		}

		/*
        internal struct SP_DEVICE_INTERFACE_DETAIL_DATA
        {
            internal Int32 cbSize;
            internal String DevicePath;
        }
		 */

		/*
        internal struct SP_DEVINFO_DATA
        {
            internal Int32 cbSize;
            internal System.Guid ClassGuid;
            internal Int32 DevInst;
            internal Int32 Reserved;
        }
		 */

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		internal static extern IntPtr RegisterDeviceNotification(IntPtr hRecipient, IntPtr NotificationFilter, Int32 Flags);

		[DllImport("setupapi.dll", SetLastError = true)]
		internal static extern Int32 SetupDiCreateDeviceInfoList(ref System.Guid ClassGuid, Int32 hwndParent);

		[DllImport("setupapi.dll", SetLastError = true)]
		internal static extern Int32 SetupDiDestroyDeviceInfoList(IntPtr DeviceInfoSet);

		[DllImport("setupapi.dll", SetLastError = true)]
		internal static extern Boolean SetupDiEnumDeviceInterfaces(IntPtr DeviceInfoSet, IntPtr DeviceInfoData, ref System.Guid InterfaceClassGuid, Int32 MemberIndex, ref SP_DEVICE_INTERFACE_DATA DeviceInterfaceData);

		[DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
		internal static extern IntPtr SetupDiGetClassDevs(ref System.Guid ClassGuid, IntPtr Enumerator, IntPtr hwndParent, Int32 Flags);

		[DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
		internal static extern Boolean SetupDiGetDeviceInterfaceDetail(IntPtr DeviceInfoSet, ref SP_DEVICE_INTERFACE_DATA DeviceInterfaceData, IntPtr DeviceInterfaceDetailData, Int32 DeviceInterfaceDetailDataSize, ref Int32 RequiredSize, IntPtr DeviceInfoData);

		[DllImport("user32.dll", SetLastError = true)]
		internal static extern Boolean UnregisterDeviceNotification(IntPtr Handle);

		///  <summary>
		///  Use SetupDi API functions to retrieve the device path name of an
		///  attached device that belongs to a device interface class.
		///  </summary>
		///
		///  <param name="myGuid"> an interface class GUID. </param>
		///  <param name="devicePathName"> a pointer to the device path name
		///  of an attached device. </param>
		///
		///  <returns>
		///   True if a device is found, False if not.
		///  </returns>

		public static bool FindDeviceFromGuid(System.Guid myGuid, ref String[] devicePathName)
		{
			Int32 bufferSize = 0;
			IntPtr detailDataBuffer = IntPtr.Zero;
			Boolean deviceFound;
			//int nbDeviceFound;
			IntPtr deviceInfoSet = new System.IntPtr();
			Boolean lastDevice = false;
			Int32 memberIndex = 0;
			SP_DEVICE_INTERFACE_DATA MyDeviceInterfaceData = new SP_DEVICE_INTERFACE_DATA();
			Boolean success;

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

				deviceInfoSet = SetupDiGetClassDevs(ref myGuid, IntPtr.Zero, IntPtr.Zero, DIGCF_PRESENT | DIGCF_DEVICEINTERFACE);

				deviceFound = false;
				//nbDeviceFound = 0;
				memberIndex = 0;

				// The cbSize element of the MyDeviceInterfaceData structure must be set to
				// the structure's size in bytes.
				// The size is 28 bytes for 32-bit code and 32 bits for 64-bit code.

				MyDeviceInterfaceData.cbSize = Marshal.SizeOf(MyDeviceInterfaceData);

				do
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
						 ref myGuid,
						 memberIndex,
						 ref MyDeviceInterfaceData);

					// Find out if a device information set was retrieved.

					if (!success)
					{
						lastDevice = true;

					}
					else
					{
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

						// Allocate memory for the SP_DEVICE_INTERFACE_DETAIL_DATA structure using the returned buffer size.

						detailDataBuffer = Marshal.AllocHGlobal(bufferSize);

						// Store cbSize in the first bytes of the array. The number of bytes varies with 32- and 64-bit systems.

						Marshal.WriteInt32(detailDataBuffer, (IntPtr.Size == 4) ? (4 + Marshal.SystemDefaultCharSize) : 8);

						// Call SetupDiGetDeviceInterfaceDetail again.
						// This time, pass a pointer to DetailDataBuffer
						// and the returned required buffer size.

						success = SetupDiGetDeviceInterfaceDetail
							(deviceInfoSet,
							 ref MyDeviceInterfaceData,
							 detailDataBuffer,
							 bufferSize,
							 ref bufferSize,
							 IntPtr.Zero);

						// Skip over cbsize (4 bytes) to get the address of the devicePathName.

						IntPtr pDevicePathName = new IntPtr(detailDataBuffer.ToInt32() + 4);

						// Get the String containing the devicePathName.

						devicePathName[memberIndex] = Marshal.PtrToStringAuto(pDevicePathName);

						if (detailDataBuffer != IntPtr.Zero)
						{
							// Free the memory allocated previously by AllocHGlobal.

							Marshal.FreeHGlobal(detailDataBuffer);
						}
						deviceFound = true;
						//nbDeviceFound ++ ;
					}
					memberIndex = memberIndex + 1;
				}
				while (!((lastDevice == true)));



				return deviceFound;
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

				DEVICEMANAGEMENT.UnregisterDeviceNotification(deviceNotificationHandle);
			}
			catch (Exception)
			{
				throw;
			}
		}
	}
}
