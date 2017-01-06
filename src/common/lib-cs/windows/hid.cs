using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace SpringCard.LibCs
{
	internal sealed partial class HID
	{
		//  API declarations for HID communications.

		//  from hidpi.h
		//  Typedef enum defines a set of integer constants for HidP_Report_Type

		internal const Int16 HidP_Input = 0;
		internal const Int16 HidP_Output = 1;
		internal const Int16 HidP_Feature = 2;

		[StructLayout(LayoutKind.Sequential)]
		internal struct HIDD_ATTRIBUTES
		{
			internal Int32 Size;
			internal UInt16 VendorID;
			internal UInt16 ProductID;
			internal UInt16 VersionNumber;
		}
		
		internal HIDD_ATTRIBUTES DeviceAttributes;

		internal struct HIDP_CAPS
		{
			internal Int16 Usage;
			internal Int16 UsagePage;
			internal Int16 InputReportByteLength;
			internal Int16 OutputReportByteLength;
			internal Int16 FeatureReportByteLength;
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)]
			internal Int16[] Reserved;
			internal Int16 NumberLinkCollectionNodes;
			internal Int16 NumberInputButtonCaps;
			internal Int16 NumberInputValueCaps;
			internal Int16 NumberInputDataIndices;
			internal Int16 NumberOutputButtonCaps;
			internal Int16 NumberOutputValueCaps;
			internal Int16 NumberOutputDataIndices;
			internal Int16 NumberFeatureButtonCaps;
			internal Int16 NumberFeatureValueCaps;
			internal Int16 NumberFeatureDataIndices;
		}
		
		internal HIDP_CAPS Capabilities;

		//  If IsRange is false, UsageMin is the Usage and UsageMax is unused.
		//  If IsStringRange is false, StringMin is the String index and StringMax is unused.
		//  If IsDesignatorRange is false, DesignatorMin is the designator index and DesignatorMax is unused.

		/*
        internal struct HidP_Value_Caps
        {
            internal Int16 UsagePage;
            internal Byte ReportID;
            internal Int32 IsAlias;
            internal Int16 BitField;
            internal Int16 LinkCollection;
            internal Int16 LinkUsage;
            internal Int16 LinkUsagePage;
            internal Int32 IsRange;
            internal Int32 IsStringRange;
            internal Int32 IsDesignatorRange;
            internal Int32 IsAbsolute;
            internal Int32 HasNull;
            internal Byte Reserved;
            internal Int16 BitSize;
            internal Int16 ReportCount;
            internal Int16 Reserved2;
            internal Int16 Reserved3;
            internal Int16 Reserved4;
            internal Int16 Reserved5;
            internal Int16 Reserved6;
            internal Int32 LogicalMin;
            internal Int32 LogicalMax;
            internal Int32 PhysicalMin;
            internal Int32 PhysicalMax;
            internal Int16 UsageMin;
            internal Int16 UsageMax;
            internal Int16 StringMin;
            internal Int16 StringMax;
            internal Int16 DesignatorMin;
            internal Int16 DesignatorMax;
            internal Int16 DataIndexMin;
            internal Int16 DataIndexMax;
        }
		 */

		[DllImport("hid.dll", SetLastError = true)]
		internal static extern Boolean HidD_FlushQueue(SafeFileHandle HidDeviceObject);

		[DllImport("hid.dll", SetLastError = true)]
		internal static extern Boolean HidD_FreePreparsedData(IntPtr PreparsedData);

		[DllImport("hid.dll", SetLastError = true)]
		internal static extern Boolean HidD_GetAttributes(SafeFileHandle HidDeviceObject, ref HIDD_ATTRIBUTES Attributes);

		[DllImport("hid.dll", SetLastError = true)]
		internal static extern Boolean HidD_GetFeature(SafeFileHandle HidDeviceObject, Byte[] lpReportBuffer, Int32 ReportBufferLength);

		[DllImport("hid.dll", SetLastError = true)]
		internal static extern Boolean HidD_GetInputReport(SafeFileHandle HidDeviceObject, Byte[] lpReportBuffer, Int32 ReportBufferLength);

		[DllImport("hid.dll", SetLastError = true)]
		internal static extern void HidD_GetHidGuid(ref System.Guid HidGuid);

		[DllImport("hid.dll", SetLastError = true)]
		internal static extern Boolean HidD_GetNumInputBuffers(SafeFileHandle HidDeviceObject, ref Int32 NumberBuffers);

		[DllImport("hid.dll", SetLastError = true)]
		internal static extern Boolean HidD_GetPreparsedData(SafeFileHandle HidDeviceObject, ref IntPtr PreparsedData);

		[DllImport("hid.dll", SetLastError = true)]
		internal static extern Boolean HidD_SetFeature(SafeFileHandle HidDeviceObject, Byte[] lpReportBuffer, Int32 ReportBufferLength);

		[DllImport("hid.dll", SetLastError = true)]
		internal static extern Boolean HidD_SetNumInputBuffers(SafeFileHandle HidDeviceObject, Int32 NumberBuffers);

		[DllImport("hid.dll", SetLastError = true)]
		internal static extern Boolean HidD_SetOutputReport(SafeFileHandle HidDeviceObject, Byte[] lpReportBuffer, Int32 ReportBufferLength);

		[DllImport("hid.dll", SetLastError = true)]
		internal static extern Int32 HidP_GetCaps(IntPtr PreparsedData, ref HIDP_CAPS Capabilities);

		[DllImport("hid.dll", SetLastError = true)]
		internal static extern Int32 HidP_GetValueCaps(Int32 ReportType, Byte[] ValueCaps, ref Int32 ValueCapsLength, IntPtr PreparsedData);

		//  Used in error messages.



		///  <summary>
		///  Remove any Input reports waiting in the buffer.
		///  </summary>
		///
		///  <param name="hidHandle"> a handle to a device.   </param>
		///
		///  <returns>
		///  True on success, False on failure.
		///  </returns>

		internal Boolean FlushQueue(SafeFileHandle hidHandle)
		{
			Boolean success = false;

			try
			{
				//  ***
				//  API function: HidD_FlushQueue

				//  Purpose: Removes any Input reports waiting in the buffer.

				//  Accepts: a handle to the device.

				//  Returns: True on success, False on failure.
				//  ***

				success = HidD_FlushQueue(hidHandle);

				return success;
			}
			catch (Exception)
			{
				throw;
			}
		}

		///  <summary>
		///  Retrieves a structure with information about a device's capabilities.
		///  </summary>
		///
		///  <param name="hidHandle"> a handle to a device. </param>
		///
		///  <returns>
		///  An HIDP_CAPS structure.
		///  </returns>

		public static HIDP_CAPS GetDeviceCapabilities(SafeFileHandle hidHandle)
		{
			HIDP_CAPS Capabilities = new HID.HIDP_CAPS();
			
			IntPtr preparsedData = new System.IntPtr();
			Int32 result = 0;
			Boolean success = false;

			try
			{
				//  ***
				//  API function: HidD_GetPreparsedData

				//  Purpose: retrieves a pointer to a buffer containing information about the device's capabilities.
				//  HidP_GetCaps and other API functions require a pointer to the buffer.

				//  Requires:
				//  A handle returned by CreateFile.
				//  A pointer to a buffer.

				//  Returns:
				//  True on success, False on failure.
				//  ***

				success = HidD_GetPreparsedData(hidHandle, ref preparsedData);

				//  ***
				//  API function: HidP_GetCaps

				//  Purpose: find out a device's capabilities.
				//  For standard devices such as joysticks, you can find out the specific
				//  capabilities of the device.
				//  For a custom device where the software knows what the device is capable of,
				//  this call may be unneeded.

				//  Accepts:
				//  A pointer returned by HidD_GetPreparsedData
				//  A pointer to a HIDP_CAPS structure.

				//  Returns: True on success, False on failure.
				//  ***

				result = HidP_GetCaps(preparsedData, ref Capabilities);
				if ((result != 0))
				{

					//  ***
					//  API function: HidP_GetValueCaps

					//  Purpose: retrieves a buffer containing an array of HidP_ValueCaps structures.
					//  Each structure defines the capabilities of one value.
					//  This application doesn't use this data.

					//  Accepts:
					//  A report type enumerator from hidpi.h,
					//  A pointer to a buffer for the returned array,
					//  The NumberInputValueCaps member of the device's HidP_Caps structure,
					//  A pointer to the PreparsedData structure returned by HidD_GetPreparsedData.

					//  Returns: True on success, False on failure.
					//  ***

					Int32 vcSize = Capabilities.NumberInputValueCaps;
					Byte[] valueCaps = new Byte[vcSize];

					result = HidP_GetValueCaps(HidP_Input, valueCaps, ref vcSize, preparsedData);

					// (To use this data, copy the ValueCaps byte array into an array of structures.)

				}
			}
			catch (Exception)
			{
				throw;
			}
			finally
			{
				//  ***
				//  API function: HidD_FreePreparsedData

				//  Purpose: frees the buffer reserved by HidD_GetPreparsedData.

				//  Accepts: A pointer to the PreparsedData structure returned by HidD_GetPreparsedData.

				//  Returns: True on success, False on failure.
				//  ***

				if (preparsedData != IntPtr.Zero)
				{
					success = HidD_FreePreparsedData(preparsedData);
				}
			}

			return Capabilities;
		}

		///  <summary>
		///  reads a Feature report from the device.
		///  </summary>
		///
		///  <param name="hidHandle"> the handle for learning about the device and exchanging Feature reports. </param>
		///  <param name="myDeviceDetected"> tells whether the device is currently attached.</param>
		///  <param name="inFeatureReportBuffer"> contains the requested report.</param>
		///  <param name="success"> read success</param>

		internal Boolean GetFeatureReport(SafeFileHandle hidHandle, ref Byte[] inFeatureReportBuffer)
		{
			Boolean success;

			try
			{
				//  ***
				//  API function: HidD_GetFeature
				//  Attempts to read a Feature report from the device.

				//  Requires:
				//  A handle to a HID
				//  A pointer to a buffer containing the report ID and report
				//  The size of the buffer.

				//  Returns: true on success, false on failure.
				//  ***

				success = HidD_GetFeature(hidHandle, inFeatureReportBuffer, inFeatureReportBuffer.Length);

				return success;
			}
			catch (Exception)
			{
				throw;
			}
		}


		///  <summary>
		///  Creates a 32-bit Usage from the Usage Page and Usage ID.
		///  Determines whether the Usage is a system mouse or keyboard.
		///  Can be modified to detect other Usages.
		///  </summary>
		///
		///  <param name="MyCapabilities"> a HIDP_CAPS structure retrieved with HidP_GetCaps. </param>
		///
		///  <returns>
		///  A String describing the Usage.
		///  </returns>

		internal String GetHidUsage(HIDP_CAPS MyCapabilities)
		{
			Int32 usage = 0;
			String usageDescription = "";

			try
			{
				//  Create32-bit Usage from Usage Page and Usage ID.

				usage = MyCapabilities.UsagePage * 256 + MyCapabilities.Usage;

				if (usage == Convert.ToInt32(0X102))
				{
					usageDescription = "mouse";
				}

				if (usage == Convert.ToInt32(0X106))
				{
					usageDescription = "keyboard";
				}
			}
			catch (Exception)
			{
				throw;
			}

			return usageDescription;
		}


		///  <summary>
		///  reads an Input report from the device using a control transfer.
		///  </summary>
		///
		///  <param name="hidHandle"> the handle for learning about the device and exchanging Feature reports. </param>
		///  <param name="myDeviceDetected"> tells whether the device is currently attached. </param>
		///  <param name="inputReportBuffer"> contains the requested report. </param>
		///  <param name="success"> read success </param>

		internal Boolean GetInputReportViaControlTransfer(SafeFileHandle hidHandle, ref Byte[] inputReportBuffer)
		{
			Boolean success;

			try
			{
				//  ***
				//  API function: HidD_GetInputReport

				//  Purpose: Attempts to read an Input report from the device using a control transfer.
				//  Supported under Windows XP and later only.

				//  Requires:
				//  A handle to a HID
				//  A pointer to a buffer containing the report ID and report
				//  The size of the buffer.

				//  Returns: true on success, false on failure.
				//  ***

				success = HidD_GetInputReport(hidHandle, inputReportBuffer, inputReportBuffer.Length + 1);

				return success;
			}
			catch (Exception)
			{
				throw;
			}
		}

		///  <summary>
		///  Retrieves the number of Input reports the host can store.
		///  </summary>
		///
		///  <param name="hidDeviceObject"> a handle to a device  </param>
		///  <param name="numberOfInputBuffers"> an integer to hold the returned value. </param>
		///
		///  <returns>
		///  True on success, False on failure.
		///  </returns>

		internal Boolean GetNumberOfInputBuffers(SafeFileHandle hidDeviceObject, ref Int32 numberOfInputBuffers)
		{
			Boolean success = false;

			try
			{
				if (!((IsWindows98Gold())))
				{
					//  ***
					//  API function: HidD_GetNumInputBuffers

					//  Purpose: retrieves the number of Input reports the host can store.
					//  Not supported by Windows 98 Gold.
					//  If the buffer is full and another report arrives, the host drops the
					//  ldest report.

					//  Accepts: a handle to a device and an integer to hold the number of buffers.

					//  Returns: True on success, False on failure.
					//  ***

					success = HidD_GetNumInputBuffers(hidDeviceObject, ref numberOfInputBuffers);
				}
				else
				{
					//  Under Windows 98 Gold, the number of buffers is fixed at 2.

					numberOfInputBuffers = 2;
					success = true;
				}

				return success;
			}
			catch (Exception)
			{
				throw;
			}
		}
		///  <summary>
		///  writes a Feature report to the device.
		///  </summary>
		///
		///  <param name="outFeatureReportBuffer"> contains the report ID and report data. </param>
		///  <param name="hidHandle"> handle to the device.  </param>
		///
		///  <returns>
		///   True on success. False on failure.
		///  </returns>

		internal Boolean SendFeatureReport(SafeFileHandle hidHandle, Byte[] outFeatureReportBuffer)
		{
			Boolean success = false;

			try
			{
				//  ***
				//  API function: HidD_SetFeature

				//  Purpose: Attempts to send a Feature report to the device.

				//  Accepts:
				//  A handle to a HID
				//  A pointer to a buffer containing the report ID and report
				//  The size of the buffer.

				//  Returns: true on success, false on failure.
				//  ***

				success = HidD_SetFeature(hidHandle, outFeatureReportBuffer, outFeatureReportBuffer.Length);

				return success;
			}
			catch (Exception)
			{
				throw;
			}
		}
		///  <summary>
		///  Writes an Output report to the device using a control transfer.
		///  </summary>
		///
		///  <param name="outputReportBuffer"> contains the report ID and report data. </param>
		///  <param name="hidHandle"> handle to the device.  </param>
		///
		///  <returns>
		///   True on success. False on failure.
		///  </returns>

		internal Boolean SendOutputReportViaControlTransfer(SafeFileHandle hidHandle, Byte[] outputReportBuffer)
		{
			Boolean success = false;

			try
			{
				//  ***
				//  API function: HidD_SetOutputReport

				//  Purpose:
				//  Attempts to send an Output report to the device using a control transfer.
				//  Requires Windows XP or later.

				//  Accepts:
				//  A handle to a HID
				//  A pointer to a buffer containing the report ID and report
				//  The size of the buffer.

				//  Returns: true on success, false on failure.
				//  ***

				success = HidD_SetOutputReport(hidHandle, outputReportBuffer, outputReportBuffer.Length + 1);

				return success;
			}
			catch (Exception)
			{
				throw;
			}
		}

		///  <summary>
		///  sets the number of input reports the host will store.
		///  Requires Windows XP or later.
		///  </summary>
		///
		///  <param name="hidDeviceObject"> a handle to the device.</param>
		///  <param name="numberBuffers"> the requested number of input reports.  </param>
		///
		///  <returns>
		///  True on success. False on failure.
		///  </returns>

		internal Boolean SetNumberOfInputBuffers(SafeFileHandle hidDeviceObject, Int32 numberBuffers)
		{
			try
			{
				if (!IsWindows98Gold())
				{
					//  ***
					//  API function: HidD_SetNumInputBuffers

					//  Purpose: Sets the number of Input reports the host can store.
					//  If the buffer is full and another report arrives, the host drops the
					//  oldest report.

					//  Requires:
					//  A handle to a HID
					//  An integer to hold the number of buffers.

					//  Returns: true on success, false on failure.
					//  ***

					HidD_SetNumInputBuffers(hidDeviceObject, numberBuffers);
					return true;
				}
				else
				{
					//  Not supported under Windows 98 Gold.

					return false;
				}
			}
			catch (Exception)
			{
				throw;
			}
		}

		///  <summary>
		///  Find out if the current operating system is Windows XP or later.
		///  (Windows XP or later is required for HidD_GetInputReport and HidD_SetInputReport.)
		///  </summary>

		internal Boolean IsWindowsXpOrLater()
		{
			try
			{
				OperatingSystem myEnvironment = Environment.OSVersion;

				//  Windows XP is version 5.1.

				System.Version versionXP = new System.Version(5, 1);

				if (myEnvironment.Version >= versionXP)
				{
					return true;
				}
				else
				{
					return false;
				}
			}
			catch (Exception)
			{
				throw;
			}
		}

		///  <summary>
		///  Find out if the current operating system is Windows 98 Gold (original version).
		///  Windows 98 Gold does not support the following:
		///  Interrupt OUT transfers (WriteFile uses control transfers and Set_Report).
		///  HidD_GetNumInputBuffers and HidD_SetNumInputBuffers
		///  (Not yet tested on a Windows 98 Gold system.)
		///  </summary>

		internal Boolean IsWindows98Gold()
		{
			Boolean result = false;
			try
			{
				OperatingSystem myEnvironment = Environment.OSVersion;

				//  Windows 98 Gold is version 4.10 with a build number less than 2183.

				System.Version version98SE = new System.Version(4, 10, 2183);

				if (myEnvironment.Version < version98SE)
				{
					result = true;
				}
				else
				{
					result = false;
				}
				return result;
			}
			catch (Exception)
			{
				throw;
			}
		}
	}
}
