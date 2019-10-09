#region references

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

#endregion

namespace SpringCard.LibCs.Windows.NativeUsb
{
    /// <summary>
    /// Usb bus
    /// </summary>
    public class UsbBus : UsbNode
    {
        #region constructor/destructor

        #region constructor


        /// <summary>
        /// Initializes a new instance of the <see cref="UsbBus"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        public UsbBus(UsbNode parent):base(parent, null, -1, null)
        {
            ScanBus();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UsbBus"/> class.
        /// </summary>
        public UsbBus():this(null)
        {
        }

        #endregion

        #region destructor

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="UsbBus"/> is reclaimed by garbage collection.
        /// </summary>
        ~UsbBus()
        {
        }

        #endregion

        #endregion

        #region methodes

        #region ScanBus

        private bool ScanBus()
        {
            bool success = true;
            for (int index = 0; success; index++)
            {

                // Initialize a new controller and save the index of the controller.
                try
                {
                    this.children.Add(new UsbController(this, index));
                }
                catch
                {
                    success = false;
                }
            }

            if (this.children.Count >= 0)
                return true;

            return false;
        }

        #endregion

        #region methode Refresh

        /// <summary>
        /// Refreshes this instance.
        /// </summary>
        /// <returns></returns>
        public bool Refresh()
        {
            this.children.Clear();
            return ScanBus();
        }

        #endregion

        private void ScanHubs(ushort vendorid, ushort productid, string serial, UsbHub hub, ref List<UsbDevice> devices)
        {
            foreach (UsbNode device in hub.Children)
            {
                if (device.IsHub)
                    ScanHubs(vendorid, productid, serial, device as UsbHub, ref devices);

                if (device.DeviceDescriptor != null && serial != null)
                    if (device.DeviceDescriptor.idVendor == vendorid && device.DeviceDescriptor.idProduct == productid && device.SerialNumber.Equals(serial))
                        devices.Add(device as UsbDevice);

                if (device.DeviceDescriptor != null && serial == null)
                    if (device.DeviceDescriptor.idVendor == vendorid && device.DeviceDescriptor.idProduct == productid)
                        devices.Add(device as UsbDevice);
            }
        }

        public List<UsbDevice> GetDeviceByVidPidSerialNumber(ushort vendorid, ushort productid, string serial)
        {
            List<UsbDevice> devices = new List<UsbDevice>();
            foreach (UsbController controller in Controller)
                foreach (UsbHub hub in controller.Hubs)
                    ScanHubs(vendorid, productid, serial, hub, ref devices);

            return devices;
        }

        public List<UsbDevice> GetDeviceByVidPid(ushort vendorid, ushort productid)
        {
            List<UsbDevice> devices = new List<UsbDevice>();
            foreach (UsbController controller in Controller)
                foreach (UsbHub hub in controller.Hubs)
                    ScanHubs(vendorid, productid, null, hub, ref devices);

            return devices;
        }

       #endregion

        #region properties

        #region Controller

        /// <summary>
        /// Gets the controller.
        /// </summary>
        /// <value>The controller.</value>
        public System.Collections.ObjectModel.ReadOnlyCollection<UsbController> Controller
        {
            get
            {
                UsbController[] _cont = new UsbController[children.Count];
                children.CopyTo(_cont);
                return new System.Collections.ObjectModel.ReadOnlyCollection<UsbController>(_cont);
            }
        }

        #endregion

        #endregion
    }
}