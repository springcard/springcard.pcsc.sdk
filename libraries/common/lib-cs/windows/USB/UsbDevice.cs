#region references

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;

#endregion

namespace SpringCard.LibCs.Windows.NativeUsb
{
    /// <summary>
    /// Usb device
    /// </summary>
    public class UsbDevice : UsbNode
    {
        #region enum DeviceControlFlags

        private enum DeviceControlFlags
        {
            Enable,
            Disable
        }

        #endregion

        #region constructor/destructor

        #region constructor
        
        /// <summary>
        /// Initializes a new instance of the <see cref="UsbDevice"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="deviceDescriptor">The device descriptor.</param>
        /// <param name="adapterNumber">The adapter number.</param>
        public UsbDevice(UsbNode parent, UsbApi.USB_DEVICE_DESCRIPTOR deviceDescriptor, int adapterNumber)
            : base(parent, deviceDescriptor, adapterNumber, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UsbDevice"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="deviceDescriptor">The device descriptor.</param>
        /// <param name="adapterNumber">The adapter number.</param>
        /// <param name="devicePath">The device path.</param>
        public UsbDevice(UsbNode parent, UsbApi.USB_DEVICE_DESCRIPTOR deviceDescriptor, int adapterNumber, string devicePath)
            : base(parent, deviceDescriptor, adapterNumber, devicePath)
        {
        }

        #endregion

        #region destructor

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="UsbDevice"/> is reclaimed by garbage collection.
        /// </summary>
        ~UsbDevice()
        {

        }

        #endregion

        #endregion

        #region methodes

        #region OpenDevice

        public bool OpenDevice()
        {
            MessageBox.Show("Not implemented");
            return true;
            /*bool erg = false;
            // SETLASTERROR !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            erg = UsbApi.CloseHandle(m_FileHandle);
            m_FileHandle = UsbApi.CreateFile(m_DevicePath, UsbApi.GENERIC_READ, UsbApi.FILE_SHARE_READ, IntPtr.Zero, UsbApi.OPEN_EXISTING, 0, IntPtr.Zero);
            byte[] buffer = new byte[20];
            uint bytesread = 0;
            int error = 0;
            System.Threading.NativeOverlapped overlapped = new System.Threading.NativeOverlapped();
            IntPtr ptr = Marshal.AllocCoTaskMem(1000);
            UsbApi.SetLastError(0);
            int nBytesReturned = 0;
            erg = UsbApi.DeviceIoControl(m_FileHandle, UsbApi.USBUSER_GET_CONTROLLER_DRIVER_KEY, IntPtr.Zero, 0, ptr, 1000, out nBytesReturned, IntPtr.Zero);
            if (erg)
                Console.WriteLine("true");
            else
                Console.WriteLine("false");
            return true;*/
        }

        #endregion

        #region methode Disable

        /// <summary>
        /// Disables the specified device.
        /// </summary>
        /// <param name="vendorid">The vendorid.</param>
        /// <param name="productid">The productid.</param>
        /// <returns></returns>
        public bool Disable(ushort vendorid, ushort productid)
        {
            return SetDevice(DeviceControlFlags.Disable, vendorid, productid);
        }

        #endregion

        #region methode Enable

        /// <summary>
        /// Enables the specified device.
        /// </summary>
        /// <param name="vendorid">The vendorid.</param>
        /// <param name="productid">The productid.</param>
        /// <returns></returns>
        public bool Enable(ushort vendorid, ushort productid)
        {
            return SetDevice(DeviceControlFlags.Enable, vendorid, productid);
        }

        #endregion

        #region methode SetDevice

        private bool SetDevice(DeviceControlFlags deviceControlFlag, ushort vendorid, ushort productid)
        {
            bool res = false;
            Guid myGUID = System.Guid.Empty;
            UsbApi.SP_DEVINFO_DATA1 DeviceInfoData;
            DeviceInfoData = new UsbApi.SP_DEVINFO_DATA1();
            DeviceInfoData.cbSize = 28;
            DeviceInfoData.DevInst = 0;
            DeviceInfoData.ClassGuid = System.Guid.Empty;
            DeviceInfoData.Reserved = 0;
            UInt32 i = 0;
            StringBuilder DeviceName = new StringBuilder("");
            DeviceName.Capacity = UsbApi.MAX_BUFFER_SIZE;

            //The SetupDiGetClassDevs function returns a handle to a device information set that contains requested device information elements for a local machine.
            IntPtr theDevInfo = UsbApi.SetupDiGetClassDevs(ref myGUID, 0, 0, UsbApi.DIGCF_ALLCLASSES | UsbApi.DIGCF_PRESENT | UsbApi.DIGCF_PROFILE);
            for (; UsbApi.SetupDiEnumDeviceInfo(theDevInfo, i, DeviceInfoData); )
            {

                if (UsbApi.SetupDiGetDeviceRegistryProperty(theDevInfo, DeviceInfoData, (uint)UsbApi.SPDRP.SPDRP_HARDWAREID, 0, DeviceName, UsbApi.MAX_BUFFER_SIZE, IntPtr.Zero))
                {
                    if (DeviceName.ToString().Contains(@"USB\Vid_") && DeviceName.ToString().Contains(vendorid.ToString("x")))
                    {

                        if (deviceControlFlag == DeviceControlFlags.Disable)
                            res = StateChange(UsbApi.DICS_DISABLE, (int)i, theDevInfo);
                        else
                            res = StateChange(UsbApi.DICS_ENABLE, (int)i, theDevInfo);

                        UsbApi.SetupDiDestroyDeviceInfoList(theDevInfo);
                        break;
                    }
                }
                i++;
            }

            return true;
        }

        #endregion

        #region methode GetDeviceInstanceId

        private string GetDeviceInstanceId(IntPtr DeviceInfoSet, UsbApi.SP_DEVINFO_DATA DeviceInfoData)
        {
            StringBuilder strId = new StringBuilder(0);
            Int32 iRequiredSize = 0;
            Int32 iSize = 0;
            bool success = UsbApi.SetupDiGetDeviceInstanceId(DeviceInfoSet, ref DeviceInfoData, strId, iSize, out iRequiredSize);
            strId = new StringBuilder(iRequiredSize);
            iSize = iRequiredSize;
            success = UsbApi.SetupDiGetDeviceInstanceId(DeviceInfoSet, ref DeviceInfoData, strId, iSize, out iRequiredSize);

            if (success)
                return strId.ToString();

            return String.Empty;
        }

        #endregion

        #region methode StateChange

        private bool StateChange(int NewState, int SelectedItem, IntPtr hDevInfo)
        {
            UsbApi.SP_PROPCHANGE_PARAMS PropChangeParams;
            UsbApi.SP_DEVINFO_DATA DeviceInfoData;
            PropChangeParams = new UsbApi.SP_PROPCHANGE_PARAMS();
            PropChangeParams.Init();
            DeviceInfoData = new UsbApi.SP_DEVINFO_DATA();
            PropChangeParams.ClassInstallHeader.cbSize = Marshal.SizeOf(PropChangeParams.ClassInstallHeader);
            DeviceInfoData.cbSize = Marshal.SizeOf(DeviceInfoData);

            if (!UsbApi.SetupDiEnumDeviceInfo(hDevInfo, SelectedItem, ref DeviceInfoData))
                return false;

            PropChangeParams.ClassInstallHeader.InstallFunction = UsbApi.DIF_PROPERTYCHANGE;
            PropChangeParams.Scope = UsbApi.DICS_FLAG_GLOBAL;
            PropChangeParams.StateChange = NewState;

            if (!UsbApi.SetupDiSetClassInstallParams(hDevInfo, ref DeviceInfoData, ref PropChangeParams.ClassInstallHeader, Marshal.SizeOf(PropChangeParams)))
                return false;

            if (!UsbApi.SetupDiCallClassInstaller(UsbApi.DIF_PROPERTYCHANGE, hDevInfo, ref DeviceInfoData))
                return false;

            return true;
        }

        #endregion

        #endregion
    }
}