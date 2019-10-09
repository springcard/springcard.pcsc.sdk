using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using SpringCard.LibCs;

namespace SpringCard.Bluetooth
{
    public partial class BLE
    {
        public const string OrgBluetoothServiceDeviceInformationUuid = "180A";
        public const string OrgBluetoothCharacteristicVendorNameUuid = "2A29";
        public const string OrgBluetoothCharacteristicProductNameUuid = "2A24";
        public const string OrgBluetoothCharacteristicSerialNumberUuid = "2A25";
        public const string OrgBluetoothCharacteristicSoftwareRevisionUuid = "2A28";
        public const string OrgBluetoothCharacteristicPnpIdUuid = "2A50";

        #region Utility classes
        public class DeviceInfo
        {
            public enum BleDeviceStatus { Unknown, Absent, Advertising, Connected }
            public enum BleBondingStatus { Unknown, NotBonded, Bonded }

            public BluetoothAddress Address { get; private set; }

            public string Name { get; protected set; }

            public string GetNameString()
            {
                if (Name == null) return "";
                return Name;
            }

            public BluetoothUuid PrimaryServiceUuid { get; protected set; }

            public string GetPrimaryServiceUuidString()
            {
                if (PrimaryServiceUuid == null) return "";
                return PrimaryServiceUuid.ToString();
            }

            protected int rssiValue;
            protected bool rssiValid;

            public string GetRssiString()
            {
                if (!rssiValid) return "";
                return string.Format("{0}dBm", rssiValue);
            }

            public BleDeviceStatus DeviceStatus { get; protected set; }
            public BleBondingStatus BondingStatus { get; protected set; }

            public string GetStatusString()
            {
                string s = "";

                switch (DeviceStatus)
                {
                    case BleDeviceStatus.Advertising:
                        s += "Advertising";
                        break;
                    case BleDeviceStatus.Connected:
                        s += "Connected";
                        break;
                    case BleDeviceStatus.Absent:
                        s += "Absent";
                        break;
                    default:
                        s += "Not available";
                        break;
                }

                if (BondingStatus == BleBondingStatus.Bonded)
                    s += ",Bonded";

                return s;
            }

            public DeviceInfo(BluetoothAddress Address)
            {
                Logger.Debug("Creating new BLE device, Address=" + Address.ToString());
                this.Address = Address;
            }
        }

        #endregion

        #region Device class

        public abstract class Device
        {
            public delegate void DisconnectCallback(Device device);
            private DisconnectCallback OnDisconnect;
            public delegate void NotificationCallback(Device device, BluetoothUuid uuid, byte[] value);
            private NotificationCallback OnNotification;

            public BluetoothAddress Address { get; protected set; }

            public bool Connected { get; protected set; }

            public void ReportDisconnect()
            {
                OnDisconnect?.Invoke(this);
                Connected = false;
            }

            protected void ReportNotification(BluetoothUuid uuid, byte[] buffer)
            {
                OnNotification?.Invoke(this, uuid, buffer);
            }

            public void SetCallbacks(DisconnectCallback onDeviceDisconnect, NotificationCallback onDeviceNotification)
            {
                if (onDeviceDisconnect != null)
                    Logger.Debug("Registering Device Disconnected callback");
                this.OnDisconnect = onDeviceDisconnect;

                if (onDeviceNotification != null)
                    Logger.Debug("Registering Device Notification callback");
                this.OnNotification = onDeviceNotification;
            }

            public bool Connect(bool bondingRequired, DisconnectCallback onDeviceDisconnect, NotificationCallback onDeviceNotification)
            {
                SetCallbacks(onDeviceDisconnect, onDeviceNotification);
                Connected = Connect(bondingRequired);
                return Connected;
            }

            protected abstract bool Connect(bool bondingRequired);
            public abstract void Disconnect();
            public abstract bool EnableCharacteristicEvents(BluetoothUuid characteristicUuid);
            public abstract bool WriteCharacteristic(BluetoothUuid characteristicUuid, byte[] value);
            public abstract byte[] ReadCharacteristic(BluetoothUuid characteristicUuid);
            public virtual byte[] ReadCharacteristic(BluetoothUuid characteristicUuid, bool allowCache)
            {
                return ReadCharacteristic(characteristicUuid);
            }
            public abstract BluetoothUuid[] GetGattServices();
            public abstract BluetoothUuid[] GetGattCharacteristics(BluetoothUuid serviceUuid);

            public BluetoothUuid[] GetGattCharacteristics(string serviceUuid)
            {
                return GetGattCharacteristics(new BluetoothUuid(serviceUuid));
            }

            public bool HasGattService(BluetoothUuid serviceUuid)
            {
                BluetoothUuid[] l = GetGattServices();
                if (l != null)
                    foreach (BluetoothUuid s in l)
                        if (s.Equals(serviceUuid))
                            return true;
                return false;
            }

            public bool HasGattService(string serviceUuid)
            {
                return HasGattService(new BluetoothUuid(serviceUuid));
            }

            public bool HasGattCharacteristic(BluetoothUuid serviceUuid, BluetoothUuid characteristicUuid)
            {
                BluetoothUuid[] l = GetGattCharacteristics(serviceUuid);
                if (l != null)
                    foreach (BluetoothUuid s in l)
                        if (s.Equals(characteristicUuid))
                            return true;
                return false;
            }

            public bool HasGattCharacteristic(string serviceUuid, string characteristicUuid)
            {
                return HasGattCharacteristic(new BluetoothUuid(serviceUuid), new BluetoothUuid(characteristicUuid));
            }
        }

        #endregion

        public abstract class Adapter
        {

            public delegate void ScanTerminatedCallback(DeviceInfo[] deviceInfoList);
            public delegate void ScanResponseCallback(DeviceInfo deviceInfo);

            private ScanResponseCallback OnScanResponse;

            public abstract bool Open();
            public abstract void Close();
            public abstract bool DeleteAllBondings();

            private Thread asyncThread;

            private class WaitScanEndParameters
            {
                public uint timeout;
                public ScanTerminatedCallback OnScanTerminated;
            }
            private void WaitScanEndAsync(object obj)
            {
                WaitScanEndParameters parameters = (WaitScanEndParameters)obj;

                Logger.Debug(String.Format("Scan running for {0}ms", parameters.timeout));

                Thread.Sleep((int)parameters.timeout);

                StopScan();

                DeviceInfo[] result = ScanResult();

                if (result == null)
                {
                    Logger.Debug("Scan error (result is null)");
                }
                else
                {
                    Logger.Debug(String.Format("Scan returned {0} device(s)", result.Length));
                }

                parameters.OnScanTerminated?.Invoke(result);
            }

            public bool StartScan(ScanResponseCallback OnScanResponse)
            {
                this.OnScanResponse = OnScanResponse;
                return StartScan();
            }

            public bool StartScan(uint timeout, ScanTerminatedCallback onScanTerminated)
            {
                if (!StartScan(null))
                    return false;

                WaitScanEndParameters parameters = new WaitScanEndParameters();
                parameters.timeout = timeout;
                parameters.OnScanTerminated = onScanTerminated;

                asyncThread = new Thread(new ParameterizedThreadStart(WaitScanEndAsync));
                asyncThread.Start(parameters);

                return true;
            }

            protected Dictionary<string, DeviceInfo> scanResults = new Dictionary<string, DeviceInfo>();

            public DeviceInfo GetDeviceInfo(BluetoothAddress address)
            {
                if (scanResults.ContainsKey(address.ToString()))
                    return scanResults[address.ToString()];
                return null;
            }

            protected void DeleteDeviceInfo(BluetoothAddress address)
            {
                if (scanResults.ContainsKey(address.ToString()))
                    scanResults.Remove(address.ToString());
            }

            protected void SetDeviceInfo(DeviceInfo deviceInfo)
            {
                scanResults[deviceInfo.Address.ToString()] = deviceInfo;

                OnScanResponse?.Invoke(deviceInfo);
            }

            public DeviceInfo[] ScanResult()
            {
                return EnumDevices().ToArray();
            }

            public List<DeviceInfo> EnumDevices()
            {
                List<DeviceInfo> result = new List<DeviceInfo>();

                foreach (DeviceInfo deviceInfo in scanResults.Values)
                    result.Add(deviceInfo);

                return result;
            }

            public abstract bool StartScan();
            public abstract void StopScan();

            public abstract Device CreateDevice(BluetoothAddress address);

            public Device CreateDevice(string address)
            {
                return CreateDevice(new BluetoothAddress(address));
            }

        }

        public static Adapter DefaultAdapter
        {
            get
            {
                return WindowsAdapter.Instance;
            }
        }
    }
}
