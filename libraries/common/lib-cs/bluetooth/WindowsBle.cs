using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;
using Windows.Devices.Enumeration;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Foundation;
using SpringCard.LibCs;
using Windows.Storage.Streams;

namespace SpringCard.Bluetooth
{
    public partial class BLE
    {
        internal class WindowsDeviceInfo : DeviceInfo
        {
            public void Update(DeviceInformation eventArgs)
            {
                if (!string.IsNullOrEmpty(eventArgs.Name))
                {
                    this.Name = eventArgs.Name;
                }

                if (eventArgs.Pairing.IsPaired)
                {
                    this.DeviceStatus = BleDeviceStatus.Connected;
                    this.BondingStatus = BleBondingStatus.Bonded;
                }
                else if (eventArgs.Pairing.CanPair)
                {
                    this.DeviceStatus = BleDeviceStatus.Connected;
                    this.BondingStatus = BleBondingStatus.NotBonded;
                }

                foreach (KeyValuePair<string, object> entry in eventArgs.Properties)
                {
                    try
                    {
                        Logger.Debug("\t" + entry.Key + "=" + entry.Value.ToString());
                    }
                    catch
                    {
                        Logger.Debug("\t" + entry.Key + "=???");
                    }
                }
            }

            public WindowsDeviceInfo(BluetoothAddress Address, DeviceInformation eventArgs) : base(Address)
            {
                this.DeviceStatus = BleDeviceStatus.Advertising;
                Update(eventArgs);
            }

            public void Update(BluetoothLEAdvertisementReceivedEventArgs eventArgs)
            {
                if (!string.IsNullOrEmpty(eventArgs.Advertisement.LocalName))
                {
                    this.Name = eventArgs.Advertisement.LocalName;
                }

                if ((eventArgs.Advertisement.ServiceUuids != null) && (eventArgs.Advertisement.ServiceUuids.Count >= 1))
                {
                    this.PrimaryServiceUuid = new BluetoothUuid(eventArgs.Advertisement.ServiceUuids[0]);
                }

                this.rssiValue = eventArgs.RawSignalStrengthInDBm;
                this.rssiValid = true;
            }

            public WindowsDeviceInfo(BluetoothAddress Address, BluetoothLEAdvertisementReceivedEventArgs eventArgs) : base(Address)
            {
                this.DeviceStatus = BleDeviceStatus.Advertising;
                Update(eventArgs);
            }
        }

        #region Device

        public class WindowsDevice : Device
        {
            internal WindowsDevice(BluetoothAddress Address)
            {
                this.Address = Address;
            }

            private class BleDeviceData
            {
                public List<GattDeviceService> services;
                public List<GattCharacteristic> characteristics;
                public List<GattCharacteristic> eventedCharacteristics;
            }

            private static Dictionary<string, BleDeviceData> deviceDataCache = new Dictionary<string, BleDeviceData>();

            private BleDeviceData deviceData;
            private BluetoothLEDevice windowBLEDevice;

            private GattCharacteristic FindCharacteristic(BluetoothUuid characteristicUuid)
            {
                if (deviceData.characteristics == null)
                    return null;

                foreach (GattCharacteristic characteristic in deviceData.characteristics)
                {
                    BluetoothUuid u = new BluetoothUuid(characteristic.Uuid);
                    if (u.Equals(characteristicUuid))
                        return characteristic;
                }

                return null;
            }

            private void HandleCommunicationError(GattCommunicationStatus status)
            {
                Logger.Trace("Error: {0}", status.ToString());
                if (windowBLEDevice != null)
                {
                    Disconnect();
                }
            }

            private void HandleCommunicationException(Exception e)
            {
                Logger.Trace("Exception {0} ({1}/{2:X08})", e.Message, e.HResult, e.HResult);
                if ((windowBLEDevice != null) && (((uint)e.HResult) != 0x806500FF)) // Magic number, not documented, but...
                {
                    Disconnect();
                }
            }

            private void defaultPairingRequestHandler(DeviceInformationCustomPairing CP, DevicePairingRequestedEventArgs DPR)
            {
                //so we get here for custom pairing request.
                //this is the magic place where your pin goes.
                //my device actually does not require a pin but
                //windows requires at least a "0".  So this solved 
                //it.  This does not pull up the Windows UI either.
                DPR.Accept();
            }

            private async Task<bool> ConnectAsync(BluetoothAddress deviceAddress, bool bondingRequired)
            {
                string strAddress = deviceAddress.ToString();

				Logger.Debug("Connecting to device " + strAddress);
                windowBLEDevice = await BluetoothLEDevice.FromBluetoothAddressAsync(deviceAddress.ToULong());
                if (windowBLEDevice == null)
                {
                    Logger.Trace("Failed to select device " + strAddress);
                    return false;
                }

				deviceData = null;
				if (deviceDataCache.ContainsKey(strAddress))
				{
                    // TODO : JDA check code avec SAL
					deviceData = deviceDataCache[strAddress];
					deviceDataCache.Remove(strAddress);
				}
				if (deviceData == null)
					deviceData = new BleDeviceData();

				Logger.Debug("Requesting access to device " + strAddress);
				DeviceAccessStatus accessStatus = await windowBLEDevice.RequestAccessAsync();
                if (accessStatus != DeviceAccessStatus.Allowed)
                {
                    Logger.Trace("Access to the device " + strAddress + "has been denied (" + accessStatus.ToString() + ")");
                    return false;
                }

				if (bondingRequired)
				{
					Logger.Debug("Bonding with the device " + strAddress);

					windowBLEDevice.DeviceInformation.Pairing.Custom.PairingRequested += defaultPairingRequestHandler;
					DevicePairingResult result = await windowBLEDevice.DeviceInformation.Pairing.Custom.PairAsync(DevicePairingKinds.ConfirmOnly, DevicePairingProtectionLevel.EncryptionAndAuthentication);
					windowBLEDevice.DeviceInformation.Pairing.Custom.PairingRequested -= defaultPairingRequestHandler;

					if ((result.Status == DevicePairingResultStatus.Paired) || (result.Status == DevicePairingResultStatus.AlreadyPaired))
					{
						Logger.Debug("Bonding complete status: " + result.Status.ToString() + " Connection Status: " + windowBLEDevice.ConnectionStatus.ToString());
					}
					else
					{
						Logger.Trace("Bonding failed with error: " + result.Status.ToString() + " Connection Status: " + windowBLEDevice.ConnectionStatus.ToString());
						return false;
					}
				}

				if ((deviceData.services == null) || (deviceData.characteristics == null))
				{
					Logger.Debug("Reading GATT from device " + strAddress);
					if (!ReadGatt(true, true))
					{
						Logger.Trace("Failed to read the Gatt Services from the device");
						return false;
					}
				}

				deviceDataCache[strAddress] = deviceData;

                if (windowBLEDevice.ConnectionStatus != BluetoothConnectionStatus.Connected)
                {
                    Logger.Trace("Connection not active");
                    return false;
                }

                Logger.Debug("Connected!");
                return true;
            }

            protected override bool Connect(bool bondingRequired)
            {
                if (windowBLEDevice != null)
                    return true;

                bool result = ConnectAsync(Address, bondingRequired).Result;

                if (!result)
                {
                    Logger.Debug("Connecting failed, disconnecting");
                    return false;
                }

                windowBLEDevice.ConnectionStatusChanged += WindowBLEDevice_ConnectionStatusChanged;

                return result;
            }

            private void WindowBLEDevice_ConnectionStatusChanged(BluetoothLEDevice sender, object args)
            {
                if (sender == windowBLEDevice)
                {
                    switch (sender.ConnectionStatus)
                    {
                        case BluetoothConnectionStatus.Connected:
                            Logger.Trace("BLE Device {0}: connected", Address.ToString());
                            break;
                        case BluetoothConnectionStatus.Disconnected:
                            Logger.Trace("BLE Device {0}: disconnected", Address.ToString());
                            windowBLEDevice.Dispose();
                            windowBLEDevice = null;
                            ReportDisconnect();
                            break;
                        default:
                            Logger.Trace("BLE Device {0}: invalid connection status event", Address.ToString());
                            break;
                    }
                }
                else
                {
                    Logger.Debug("Unexpected connection status callback!");
                }
            }

            public override void Disconnect()
            {
                if (windowBLEDevice != null)
                {
                    Logger.Debug("Disconnecting...");

                    if (deviceData.eventedCharacteristics != null)
                    {
                        foreach (GattCharacteristic characteristic in deviceData.eventedCharacteristics)
                        {
                            characteristic.ValueChanged -= CharacteristicNotificationHandler;
                        }
                    }

                    windowBLEDevice.Dispose();
                    windowBLEDevice = null;

                    Logger.Debug("Disconnected");

                    ReportDisconnect();
                }
                else
                {
                    Logger.Debug("(Already disconnected)");
                }

                Connected = false;
            }

            private void CharacteristicNotificationHandler(GattCharacteristic sender, GattValueChangedEventArgs eventArgs)
            {
                BluetoothUuid uuid = new BluetoothUuid(sender.Uuid);

                Logger.Debug("BLE Notification on {0}", uuid.ToString());

                byte[] buffer = new byte[eventArgs.CharacteristicValue.Length];
                DataReader.FromBuffer(eventArgs.CharacteristicValue).ReadBytes(buffer);

                Logger.Debug(">" + BinConvert.ToHex(buffer));

                ReportNotification(uuid, buffer);
            }

            private async Task<bool> EnableCharacteristicEventsAsync(BluetoothUuid characteristicUuid)
            {
                GattCharacteristic characteristic = FindCharacteristic(characteristicUuid);

                if (characteristic == null)
                {
                    Logger.Trace("Characteristic " + characteristicUuid.ToString() + " not found");
                    return false;
                }

                GattCharacteristicProperties properties = characteristic.CharacteristicProperties;

                GattClientCharacteristicConfigurationDescriptorValue mode = GattClientCharacteristicConfigurationDescriptorValue.None;

                if (properties.HasFlag(GattCharacteristicProperties.Indicate))
                {
                    mode = GattClientCharacteristicConfigurationDescriptorValue.Indicate;
                }
                else
                if (properties.HasFlag(GattCharacteristicProperties.Notify))
                {
                    mode = GattClientCharacteristicConfigurationDescriptorValue.Notify;
                }
                else
                {
                    Logger.Trace("Characteristic " + characteristicUuid.ToString() + " has no indication/notification");
                    return false;
                }

                Logger.Debug("Enabling events (" + mode.ToString() + ") for " + characteristicUuid.ToString());

                characteristic.ValueChanged += CharacteristicNotificationHandler;

                GattCommunicationStatus result = await characteristic.WriteClientCharacteristicConfigurationDescriptorAsync(mode);

                if (result != GattCommunicationStatus.Success)
                {
                    Logger.Trace("Enable events (" + mode.ToString() + ") for characteristic " + characteristicUuid.ToString() + " failed");
                    Logger.Debug(result.ToString());
                    HandleCommunicationError(result);
                    return false;
                }

                if (deviceData.eventedCharacteristics == null)
                    deviceData.eventedCharacteristics = new List<GattCharacteristic>();
                deviceData.eventedCharacteristics.Add(characteristic);

                return true;
            }

            public override bool EnableCharacteristicEvents(BluetoothUuid characteristicUuid)
            {
				Logger.Debug("Enable notification on characteristic " + characteristicUuid.ToString());
                return EnableCharacteristicEventsAsync(characteristicUuid).Result;
            }

            private async Task<byte[]> ReadCharacteristicAsync(BluetoothUuid characteristicUuid, BluetoothCacheMode cacheMode)
            {
                try
                {
                    Logger.Debug("BLE Read({0})", characteristicUuid.ToString());

                    GattCharacteristic characteristic = FindCharacteristic(characteristicUuid);

                    if (characteristic == null)
                    {
                        Logger.Trace("Characteristic " + characteristicUuid.ToString() + " not found");
                        return null;
                    }

                    GattCharacteristicProperties properties = characteristic.CharacteristicProperties;

                    if (!properties.HasFlag(GattCharacteristicProperties.Read))
                    {
                        Logger.Trace("Characteristic " + characteristicUuid.ToString() + " is not readable");
                        return null;
                    }

                    GattReadResult result = await characteristic.ReadValueAsync(cacheMode);

					if ((result == null) || (result.Status != GattCommunicationStatus.Success))
                    {
                        Logger.Trace("Read from characteristic " + characteristicUuid.ToString() + " failed");
                        Logger.Debug(result.Status.ToString());
                        HandleCommunicationError(result.Status);
                        return null;
                    }

					if (result.Value.Length == 0)
						return new byte[0];

					byte[] buffer = result.Value.ToArray();
                    Logger.Debug(">" + BinConvert.ToHex(buffer));

                    return buffer;
                }
                catch (Exception e)
                {
                    Logger.Trace("Failed to read " + characteristicUuid.ToString());
                    Logger.Trace(string.Format("(Error {0}/{1:X08}: {2})", e.HResult, e.HResult, e.Message));
                    return null;
                }
            }

            public override byte[] ReadCharacteristic(BluetoothUuid characteristicUuid)
            {
                return ReadCharacteristicAsync(characteristicUuid, BluetoothCacheMode.Uncached).Result;
            }

            public override byte[] ReadCharacteristic(BluetoothUuid characteristicUuid, bool allowCache)
            {
                return ReadCharacteristicAsync(characteristicUuid, allowCache ? BluetoothCacheMode.Cached : BluetoothCacheMode.Uncached).Result;
            }

            private async Task<bool> ReadGattAsync(bool allowCache, bool ignoreAccessErrors)
            {
				Logger.Debug("Retrieving the list of services from the GATT (cache={0}, ignore access errors={1})...", allowCache, ignoreAccessErrors);

                GattDeviceServicesResult gatt = await windowBLEDevice.GetGattServicesAsync(allowCache ? BluetoothCacheMode.Cached : BluetoothCacheMode.Uncached);

                if ((gatt == null) || (gatt.Status != GattCommunicationStatus.Success))
                {
                    Logger.Trace("Retrieve GATT failed");
                    HandleCommunicationError(gatt.Status);
                    return false;
                }

                deviceData.services = new List<GattDeviceService>();
                foreach (GattDeviceService service in gatt.Services)
                    deviceData.services.Add(service);

                Logger.Debug(string.Format("{0} service(s) found, now listing the characteristics...", deviceData.services.Count));

                deviceData.characteristics = new List<GattCharacteristic>();

                foreach (GattDeviceService service in deviceData.services)
                {
                    GattCharacteristicsResult characteristics = await service.GetCharacteristicsAsync();

                    if ((characteristics == null) || (characteristics.Status != GattCommunicationStatus.Success))
                    {
                        if ((characteristics == null) && (characteristics.Status == GattCommunicationStatus.AccessDenied) && ignoreAccessErrors)
                            continue;

                        Logger.Trace("Retrieve Characteristics failed in service " + (new BluetoothUuid(service.Uuid).ToString()));
                        HandleCommunicationError(characteristics.Status);
                        return false;
                    }

                    foreach (GattCharacteristic characteristic in characteristics.Characteristics)
                        deviceData.characteristics.Add(characteristic);
                }

                Logger.Debug(string.Format("A total of {0} characteristic(s) has been found", deviceData.characteristics.Count));

                return true;
            }

            private async Task<bool> WriteCharacteristicAsync(BluetoothUuid characteristicUuid, byte[] buffer)
            {
                try
                {
                    Logger.Debug("BLE Write({0})", characteristicUuid.ToString());

                    GattCharacteristic characteristic = FindCharacteristic(characteristicUuid);

                    if (characteristic == null)
                    {
                        Logger.Trace("Characteristic " + characteristicUuid.ToString() + " not found");
                        return false;
                    }

                    GattCharacteristicProperties properties = characteristic.CharacteristicProperties;

                    if (!properties.HasFlag(GattCharacteristicProperties.Write))
                    {
                        Logger.Trace("Characteristic " + characteristicUuid.ToString() + " is not writable");
                        return false;
                    }

                    DataWriter writer = new DataWriter();
                    writer.WriteBuffer(buffer.AsBuffer());

                    Logger.Debug("<" + BinConvert.ToHex(buffer));

                    GattCommunicationStatus result = await characteristic.WriteValueAsync(writer.DetachBuffer(), GattWriteOption.WriteWithResponse);

                    if (result != GattCommunicationStatus.Success)
                    {
                        Logger.Trace("Write to characteristic " + characteristicUuid.ToString() + " failed");
                        HandleCommunicationError(result);
                        return false;
                    }

                    return true;
                }
                catch (Exception e)
                {
                    Logger.Trace("Failed to write " + characteristicUuid.ToString());
                    HandleCommunicationException(e);
                    Logger.Trace(string.Format("(Error {0}/{1:X08}: {2})", e.HResult, e.HResult, e.Message));
                    return false;
                }
            }

            public override bool WriteCharacteristic(BluetoothUuid characteristicUuid, byte[] value)
            {
                return WriteCharacteristicAsync(characteristicUuid, value).Result;
            }

			public bool ReadGatt(bool allowCache,  bool ignoreAccessErrors)
            {
                if (windowBLEDevice == null)
                    return false;

                if (!ReadGattAsync(allowCache, ignoreAccessErrors).Result)
                    return false;

                DumpGatt();
                return true;
            }

            private void DumpGatt()
            {
                Logger.Debug("GATT tree:");
                foreach (GattDeviceService service in deviceData.services)
                {
                    BluetoothUuid serviceUuid = new BluetoothUuid(service.Uuid);
                    Logger.Debug("+-- " + serviceUuid.ToString());

                    foreach (GattCharacteristic characteristic in deviceData.characteristics)
                    {
                        if (characteristic.Service.Uuid == service.Uuid)
                        {
                            BluetoothUuid characteristicUuid = new BluetoothUuid(characteristic.Uuid);
                            Logger.Debug("  +-- " + characteristicUuid.ToString());

                            GattCharacteristicProperties properties = characteristic.CharacteristicProperties;
                            if (properties.HasFlag(GattCharacteristicProperties.AuthenticatedSignedWrites))
                                Logger.Debug("        AuthenticatedSignedWrites");
                            if (properties.HasFlag(GattCharacteristicProperties.Broadcast))
                                Logger.Debug("        Broadcast");
                            if (properties.HasFlag(GattCharacteristicProperties.ExtendedProperties))
                                Logger.Debug("        ExtendedProperties");
                            if (properties.HasFlag(GattCharacteristicProperties.Indicate))
                                Logger.Debug("        Indicate");
                            if (properties.HasFlag(GattCharacteristicProperties.Notify))
                                Logger.Debug("        Notify");
                            if (properties.HasFlag(GattCharacteristicProperties.Read))
                                Logger.Debug("        Read");
                            if (properties.HasFlag(GattCharacteristicProperties.ReliableWrites))
                                Logger.Debug("        ReliableWrites");
                            if (properties.HasFlag(GattCharacteristicProperties.WritableAuxiliaries))
                                Logger.Debug("        WritableAuxiliaries");
                            if (properties.HasFlag(GattCharacteristicProperties.Write))
                                Logger.Debug("        Write");
                            if (properties.HasFlag(GattCharacteristicProperties.WriteWithoutResponse))
                                Logger.Debug("        WriteWithoutResponse");

                            GattProtectionLevel protectionLevel = characteristic.ProtectionLevel;
                            if (protectionLevel.HasFlag(GattProtectionLevel.EncryptionAndAuthenticationRequired))
                                Logger.Debug("        EncryptionAndAuthenticationRequired");
                            if (protectionLevel.HasFlag(GattProtectionLevel.AuthenticationRequired))
                                Logger.Debug("        AuthenticationRequired");
                            if (protectionLevel.HasFlag(GattProtectionLevel.EncryptionRequired))
                                Logger.Debug("        EncryptionRequired");
                        }
                    }
                }
            }

            public override BluetoothUuid[] GetGattServices()
            {
                if (deviceData.services == null)
                    return null;

                BluetoothUuid[] result = new BluetoothUuid[deviceData.services.Count];
                for (int i = 0; i < result.Length; i++)
                    result[i] = new BluetoothUuid(deviceData.services[i].Uuid);

                return result;
            }

            public override BluetoothUuid[] GetGattCharacteristics(BluetoothUuid serviceUuid)
            {
                if (deviceData.characteristics == null)
                    return null;

                int c = 0;
                for (int i = 0; i < deviceData.characteristics.Count; i++)
                    if (new BluetoothUuid(deviceData.characteristics[i].Service.Uuid).Equals(serviceUuid))
                        c++;

                BluetoothUuid[] result = new BluetoothUuid[c];

                c = 0;
                for (int i = 0; i < deviceData.characteristics.Count; i++)
                    if (new BluetoothUuid(deviceData.characteristics[i].Service.Uuid).Equals(serviceUuid))
                        result[c++] = new BluetoothUuid(deviceData.characteristics[i].Uuid);

                return result;
            }
        }

        #endregion

        public sealed class WindowsAdapter : Adapter
        {
            private static volatile WindowsAdapter instance;
            private static object locker = new Object();

            public static WindowsAdapter Instance
            {
                get
                {
                    if (instance == null)
                    {
                        lock (locker)
                        {
                            if (instance == null)
                                instance = new WindowsAdapter();
                        }
                    }
                    return instance;
                }
            }

            private WindowsAdapter()
            {

            }

            ~WindowsAdapter()
            {
                Close();
            }

            public override bool DeleteAllBondings()
            {
                Logger.Error("Windows BLE API doesn't offer the Delete All Bondings function");
                return false;
            }

            public override Device CreateDevice(BluetoothAddress Address)
            {
                return new WindowsDevice(Address);
            }


            public const string AllSelectorString = "System.Devices.Aep.ProtocolId:=\"{bb7bb05e-5972-42b5-94fc-76eaa7084d49}\"";

            public enum SelectorFilter { All, Unpaired, Paired };


            #region Override, global

            private bool isScanning;

            private BluetoothLEAdvertisementWatcher advertisementWatcher;

            private TypedEventHandler<BluetoothLEAdvertisementWatcher, BluetoothLEAdvertisementReceivedEventArgs> advertisementReceived = null;
            private TypedEventHandler<BluetoothLEAdvertisementWatcher, BluetoothLEAdvertisementWatcherStoppedEventArgs> advertisementStoppped = null;

            private DeviceWatcher deviceWatcher;

            private TypedEventHandler<DeviceWatcher, DeviceInformation> deviceAdded = null;
            private TypedEventHandler<DeviceWatcher, DeviceInformationUpdate> deviceUpdated = null;
            private TypedEventHandler<DeviceWatcher, DeviceInformationUpdate> deviceRemoved = null;
            private TypedEventHandler<DeviceWatcher, Object> deviceWatcherEnumCompleted = null;
            private TypedEventHandler<DeviceWatcher, Object> deviceWatcherStopped = null;
            private new WindowsDeviceInfo GetDeviceInfo(BluetoothAddress address)
            {
                if (scanResults.ContainsKey(address.ToString()))
                    return (WindowsDeviceInfo)scanResults[address.ToString()];
                return null;
            }

            void OnAdvertisementReceived(BluetoothLEAdvertisementReceivedEventArgs eventArgs)
            {
                try
                {
                    lock (locker)
                    {
                        BluetoothAddress address = new BluetoothAddress(eventArgs.BluetoothAddress);
                        WindowsDeviceInfo deviceInfo = GetDeviceInfo(address);

                        if (deviceInfo == null)
                        {
                            Logger.Trace("BLE Scan <- {0}", address.ToString());
                            deviceInfo = new WindowsDeviceInfo(address, eventArgs);
                        }
                        else
                        {
                            deviceInfo.Update(eventArgs);
                        }

                        SetDeviceInfo(deviceInfo);
                    }
                }
                catch { }
            }

            async void OnDeviceAdded(DeviceInformation eventArgs)
            {
                if (isScanning)
                {
                    BluetoothLEDevice device = await BluetoothLEDevice.FromIdAsync(eventArgs.Id);
                    if (device != null)
                    {
                        lock (locker)
                        {
                            BluetoothAddress address = new BluetoothAddress(device.BluetoothAddress);
                            WindowsDeviceInfo deviceInfo = GetDeviceInfo(address);
                            if (deviceInfo == null)
                            {
                                Logger.Trace("BLE Device {0} added", address.ToString());
                                deviceInfo = new WindowsDeviceInfo(address, eventArgs);
                            }
                            else
                            {
                                deviceInfo.Update(eventArgs);
                            }

                            SetDeviceInfo(deviceInfo);
                        }
                    }
                }
            }

            async void OnDeviceUpdated(DeviceInformationUpdate eventArgs)
            {
                if (isScanning)
                {
                    BluetoothLEDevice device = await BluetoothLEDevice.FromIdAsync(eventArgs.Id);
                    if (device != null)
                    {
                        BluetoothAddress address = new BluetoothAddress(device.BluetoothAddress);
                        Logger.Trace("BLE Device {0} updated", address.ToString());
                        foreach (KeyValuePair<string, object> entry in eventArgs.Properties)
                        {
                            Logger.Debug("Property changed: " + entry.Key);
                        }
                    }
                }
                else
                {
                    Logger.Debug("BLE Device update (ignored, not scanning)");
                }
            }

            async void OnDeviceRemoved(DeviceInformationUpdate eventArgs)
            {
                BluetoothLEDevice device = await BluetoothLEDevice.FromIdAsync(eventArgs.Id);
                if (device != null)
                {
                    BluetoothAddress address = new BluetoothAddress(device.BluetoothAddress);
                    Logger.Trace("BLE Device {0} removed", address.ToString());
                    DeleteDeviceInfo(address);
                }
                else
                {
                    Logger.Debug("BLE Device update (ignored, not connected)");
                }
            }

            public override bool Open()
            {
				return StartDeviceWatcher();
            }

            public override void Close()
            {
                StopDeviceWatcher();
                StopScan();
            }

            private bool StartDeviceWatcher()
            {
                Logger.Debug("BLE:Start device watcher");

                string ProtocolSelector = "System.Devices.Aep.ProtocolId:=\"{bb7bb05e-5972-42b5-94fc-76eaa7084d49}\"";
                string StatusSelector = "System.Devices.Aep.IsPresent:=System.StructuredQueryType.Boolean#True";
                string ConnectedSelector = "System.Devices.Aep.IsConnected:=System.StructuredQueryType.Boolean#True";
                string selector = "(" + ProtocolSelector + ")" + " AND (" + StatusSelector + ") AND (" + ConnectedSelector + ")";
                DeviceInformationKind kind = DeviceInformationKind.AssociationEndpoint;

                if (kind == DeviceInformationKind.Unknown)
                {
                    deviceWatcher = DeviceInformation.CreateWatcher(
                        selector,
                        null
                        );
                }
                else
                {
                    deviceWatcher = DeviceInformation.CreateWatcher(
                        selector,
                        null,
                        kind);
                }

                deviceAdded = new TypedEventHandler<DeviceWatcher, DeviceInformation>(async (watcher, deviceInfo) =>
                {
                    await Task.Run(() =>
                    {
                        Logger.Debug("BLE Device added...");
                        if (watcher == deviceWatcher)
                        {
                            Logger.Debug("DeviceId=" + deviceInfo.Id);
                            Logger.Debug("DeviceName=" + deviceInfo.Name);
                            OnDeviceAdded(deviceInfo);
                        }
                    });
                });
                deviceWatcher.Added += deviceAdded;

                deviceUpdated = new TypedEventHandler<DeviceWatcher, DeviceInformationUpdate>(async (watcher, deviceInfoUpdate) =>
                {
                    await Task.Run(() =>
                    {
                        Logger.Debug("BLE Device update...");
                        if (watcher == deviceWatcher)
                        {
                            Logger.Debug("DeviceId=" + deviceInfoUpdate.Id);
                            OnDeviceUpdated(deviceInfoUpdate);
                        }
                    });
                });
                deviceWatcher.Updated += deviceUpdated;

                deviceRemoved = new TypedEventHandler<DeviceWatcher, DeviceInformationUpdate>(async (watcher, deviceInfoUpdate) =>
                {
                    await Task.Run(() =>
                    {
                        Logger.Debug("BLE Device remove...");
                        if (watcher == deviceWatcher)
                        {
                            Logger.Debug("DeviceId=" + deviceInfoUpdate.Id);
                            OnDeviceRemoved(deviceInfoUpdate);
                        }
                    });
                });
                deviceWatcher.Removed += deviceRemoved;

                deviceWatcherEnumCompleted = new TypedEventHandler<DeviceWatcher, Object>(async (watcher, obj) =>
                {
                    await Task.Run(() =>
                    {
                        Logger.Debug("BLE Device enum completed...");
                        if (watcher == deviceWatcher)
                        {
                            Logger.Trace("Device enumeration completed, now watching for updates");
                        }
                    });
                });
                deviceWatcher.EnumerationCompleted += deviceWatcherEnumCompleted;

                deviceWatcherStopped = new TypedEventHandler<DeviceWatcher, Object>(async (watcher, obj) =>
                {
                    await Task.Run(() =>
                    {
                        Logger.Debug("BLE Device enum stopped...");
                        if (watcher == deviceWatcher)
                        {
                            Logger.Trace("Device watcher has stopped");
                        }
                    });
                });
                deviceWatcher.Stopped += deviceWatcherStopped;
                deviceWatcher.Start();

                return true;
            }

            private void StopDeviceWatcher()
            {
                if (deviceWatcher != null)
                {
					Logger.Debug("BLE:Stop device watcher");

					deviceWatcher.Added -= deviceAdded;
                    deviceWatcher.Updated -= deviceUpdated;
                    deviceWatcher.Removed -= deviceRemoved;
                    deviceWatcher.EnumerationCompleted -= deviceWatcherEnumCompleted;
                    deviceWatcher.Stopped -= deviceWatcherStopped;

                    if (DeviceWatcherStatus.Started == deviceWatcher.Status ||
                        DeviceWatcherStatus.EnumerationCompleted == deviceWatcher.Status)
                    {
                        deviceWatcher.Stop();
                    }

                    deviceWatcher = null;
                }
            }

            public override bool StartScan()
            {
                // StopDeviceWatcher();

                Logger.Debug("BLE:Start scan");

                advertisementWatcher = new BluetoothLEAdvertisementWatcher();
                advertisementWatcher.ScanningMode = BluetoothLEScanningMode.Active;

                advertisementReceived = new TypedEventHandler<BluetoothLEAdvertisementWatcher, BluetoothLEAdvertisementReceivedEventArgs>(async (watcher, eventArgs) =>
                {
                    await Task.Run(() =>
                    {
                        if (watcher == advertisementWatcher)
                        {
                            OnAdvertisementReceived(eventArgs);
                        }
                    });
                });
                advertisementWatcher.Received += advertisementReceived;

                advertisementStoppped = new TypedEventHandler<BluetoothLEAdvertisementWatcher, BluetoothLEAdvertisementWatcherStoppedEventArgs>(async (watcher, eventArgs) =>
                {
                    await Task.Run(() =>
                    {
                        isScanning = false;
                        Logger.Debug("Scan stopped");
                    });
                });
                advertisementWatcher.Stopped += advertisementStoppped;

                isScanning = true;
                advertisementWatcher.Start();

                // StartDeviceWatcher();

                return true;
            }

            public override void StopScan()
            {
                isScanning = false;

                if (advertisementWatcher != null)
                {
					Logger.Debug("BLE:Stop scan");

					advertisementWatcher.Received -= advertisementReceived;
                    advertisementWatcher.Stopped -= advertisementStoppped;
                    advertisementWatcher.Stop();
                    advertisementWatcher = null;
                }
            }

            #endregion







            #region Static
            public static string GetSelectorString(SelectorFilter filter = SelectorFilter.All)
            {
                switch (filter)
                {
                    case SelectorFilter.All: return AllSelectorString;
                    case SelectorFilter.Unpaired: return BluetoothLEDevice.GetDeviceSelectorFromPairingState(false);
                    case SelectorFilter.Paired: return BluetoothLEDevice.GetDeviceSelectorFromPairingState(true);
                    default: break;
                }
                return null;
            }

            public static bool AllowApplication(string ApplicationExeName, string ApplicationGuid)
            {
                RegistryKey key;

                key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Classes\AppID\" + ApplicationGuid);
                key.SetValue(null, "Winforms Application");

                string securityString = "010004809C000000AC00000000000000140000000200880006000000000014000700000001010000000000050A00000000001400030000000101000000000005120000000000180007000000010200000000000520000000200200000000180003000000010200000000000F0200000001000000000014000300000001010000000000051300000000001400030000000101000000000005140000000102000000000005200000002002000001020000000000052000000020020000";
                byte[] securityData = BinConvert.HexToBytes(securityString);

                key.SetValue("AccessPermission", securityData, RegistryValueKind.Binary);

                key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Classes\AppID\" + Path.GetFileName(ApplicationExeName));
                key.SetValue("AppID", ApplicationGuid);

                return true;
            }

            #endregion
        }
    }
}
