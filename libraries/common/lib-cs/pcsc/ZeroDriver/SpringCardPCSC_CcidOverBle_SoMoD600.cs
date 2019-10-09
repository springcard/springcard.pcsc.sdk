using System;
using System.IO.Ports;
using System.Threading;
using SpringCard.LibCs;
using SpringCard.Bluetooth;
using SpringCard.PCSC;

namespace SpringCard.PCSC.ZeroDriver
{
	public class SCardReaderList_CcidOverBle_SoMoD600 : SCardReaderList_CcidOverBle
	{
        /*
		public const string ConfigurationServiceUuid = "7A4385C9-F7C7-4E22-9AFD-16D68FC588CA";
		public const string ConfigIoCharacteristicUuid = "1254FC72-336E-4BB2-A0A8-71C7D28D73CE";

		public const string ApplicationServiceUuid = "6CB501B7-96F6-4EEF-ACB1-D7535F153CF0";
		public const string CcidStatusCharacteristicUuid = "7C334BC2-1812-4C7E-A81D-591F92933C37";
		public const string CcidPcToRdrCharacteristicUuid = "91ACE9FD-EDD6-40B1-BA77-050A78CF9BC0";
		public const string CcidRdrToPcCharacteristicUuid = "B4CA2D75-B855-4C1A-BF40-4A72AE46BD5A";
        */
        public const string ConfigurationServiceUuid = "7A4385C9-F7C7-4E22-9AFD-16D68FC5CA88";
        public const string ConfigIoCharacteristicUuid = "1254FC72-336E-4BB2-A0A8-71C7D28DCE73";

        public const string ApplicationServiceUuid = "6CB501B7-96F6-4EEF-ACB1-D7535F15F03C";
        public const string CcidStatusCharacteristicUuid = "7C334BC2-1812-4C7E-A81D-591F9293373C";
        public const string CcidPcToRdrCharacteristicUuid = "91ACE9FD-EDD6-40B1-BA77-050A78CFC09B";
        public const string CcidRdrToPcCharacteristicUuid = "B4CA2D75-B855-4C1A-BF40-4A72AE465ABD";

        protected int ProtocolVersion = 2;

		internal SCardReaderList_CcidOverBle_SoMoD600(BLE.Adapter BleAdapter, BluetoothAddress DeviceAddress)
		{
			bleDevice = BleAdapter.CreateDevice(DeviceAddress);
			//bleUseBonding = true;
            bleUseBonding = false;
        }

		protected override bool VerifyGattProfile()
		{
			Logger.Trace("Verifying the Gatt profile...");

			BluetoothUuid[] services = bleDevice.GetGattServices();
			if (services == null)
			{
				Logger.Trace("No Gatt Service found in the device?");
				return false;
			}

			bool ConfigurationServiceFound = false;
			bool ApplicationServiceFound = false;

			foreach (BluetoothUuid service in services)
			{
				if (service.Equals(ConfigurationServiceUuid))
				{
					Logger.Debug("+ " + service.ToString() + " (Configuration)");
					ConfigurationServiceFound = true;
				}
				else
				if (service.Equals(ApplicationServiceUuid))
				{
					Logger.Debug("+ " + service.ToString() + " (Application)");
					ApplicationServiceFound = true;
				}
				else
				{
					Logger.Debug("+ " + service.ToString());
				}
			}

			if (!ConfigurationServiceFound)
			{
				Logger.Trace("Configuration Service not found in the device");
				return false;
			}
			if (!ApplicationServiceFound)
			{
				Logger.Trace("Application Service not found in the device");
				return false;
			}

			BluetoothUuid[] characteristics = bleDevice.GetGattCharacteristics(ConfigurationServiceUuid);
			if (characteristics == null)
			{
				Logger.Trace("Failed to read the Gatt characteristics from the device's Configuration Service");
				return false;
			}

			bool ConfigIoCharacteristicFound = false;

			Logger.Debug("List of characteristics in the Configuration Service: ");
			foreach (BluetoothUuid characteristic in characteristics)
			{
				if (characteristic.Equals(ConfigIoCharacteristicUuid))
				{
					Logger.Debug(" +-- " + characteristic.ToString() + " (Config In/Out)");
					ConfigIoCharacteristicFound = true;
				}
				else
				{
					Logger.Debug(" +-- " + characteristic.ToString());
				}
			}

			characteristics = bleDevice.GetGattCharacteristics(ApplicationServiceUuid);
			if (characteristics == null)
			{
				Logger.Trace("Failed to read the Gatt characteristics from the device's Application Service");
				return false;
			}

			bool CcidStatusCharacteristicFound = false;
			bool CcidPcToRdrCharacteristicFound = false;
			bool CcidRdrToPcCharacteristicFound = false;

			Logger.Debug("List of characteristics in the Application Service: ");
			foreach (BluetoothUuid characteristic in characteristics)
			{
				if (characteristic.Equals(CcidStatusCharacteristicUuid))
				{
					Logger.Debug(" +-- " + characteristic.ToString() + " (CCID Status)");
					CcidStatusCharacteristicFound = true;
				}
				else
				if (characteristic.Equals(CcidPcToRdrCharacteristicUuid))
				{
					Logger.Debug(" +-- " + characteristic.ToString() + " (CCID PC to RDR)");
					CcidPcToRdrCharacteristicFound = true;
				}
				else
				if (characteristic.Equals(CcidRdrToPcCharacteristicUuid))
				{
					Logger.Debug(" +-- " + characteristic.ToString() + " (CCID RDR to PC)");
					CcidRdrToPcCharacteristicFound = true;
				}
				else
				{
					Logger.Debug(" +-- " + characteristic.ToString());
				}
			}

			if ((!ConfigIoCharacteristicFound) || (!CcidStatusCharacteristicFound) || (!CcidPcToRdrCharacteristicFound) || (!CcidRdrToPcCharacteristicFound))
			{
				Logger.Trace("Mandatory Characteristics not found in the device");
				return false;
			}

			return true;
		}

        protected override bool GetSlotCount()
        {
#if false
            byte[] buffer = bleDevice.ReadCharacteristic(new BluetoothUuid(CcidStatusCharacteristicUuid));

            Logger.Trace("Pouahhh"+ BinConvert.ToHex(buffer));

            if (buffer == null)
            {
                Logger.Trace("Failed to read CCID_STATUS");
                return false;
            }

            SlotCount = buffer[0] & 0x0F;
#else
            /* only one slot on the d600!! */
            SlotCount = 1;
#endif
            return true;
        }

        protected override bool EnterCcidMode()
		{
			Logger.Trace("Activating the PC/SC profile...");

			if (!bleDevice.EnableCharacteristicEvents(new BluetoothUuid(CcidStatusCharacteristicUuid)))
			{
				Logger.Trace("Failed to enable the notifications on CCID_STATUS");
				return false;
			}

            if (!bleDevice.EnableCharacteristicEvents(new BluetoothUuid(CcidRdrToPcCharacteristicUuid)))
            {
                Logger.Trace("Failed to enable the notifications on CCID_RDR_TO_PC");
                return false;
            }

            byte[] Command = new byte[3];
			Command[0] = 2;
			Command[1] = 0xAE;
			Command[2] = (byte)ProtocolVersion;

			if (!bleDevice.WriteCharacteristic(new BluetoothUuid(ConfigIoCharacteristicUuid), Command))
			{
				Logger.Trace("Failed to start the PC/SC mode");
				return false;
			}

			Logger.Trace("The device is ready");

			deviceState = DeviceState.Active;
			return true;
		}

		protected override bool ExitCcidMode()
		{
			deviceState = DeviceState.NotActive;

			Logger.Trace("CCID over BLE: de-activating the PC/SC profile...  (" + Thread.CurrentThread.ManagedThreadId + ")");

			byte[] Command = new byte[3];
			Command[0] = 2;
			Command[1] = 0xAE;
			Command[2] = 0x00;

			if (!bleDevice.WriteCharacteristic(new BluetoothUuid(ConfigIoCharacteristicUuid), Command))
				return false;

			Logger.Trace("The device has been deactivated");
			return true;
		}


        protected override bool RemoteDisconnect()
        {
            return true;
        }

        override protected bool ReadSoftwareVersion()
        {
            if(!ReadAndParseRevisionString())
            {
                Logger.Trace("Could not read or parse revision string");
                return false;
          
            }
            else
            {
                if (firmwareVersionMajor >= 1 && firmwareVersionMinor >= 49)
                {
                    ProtocolVersion = 3;
                }
                else
                {
                    ProtocolVersion = 2;
                }

                Logger.Trace($"ProtocolVersion = {ProtocolVersion}");
            }

            return true;
        }

        protected override bool SendCcidPcToRdr(byte[] buffer)
		{
            Logger.Debug("BLE<" + BinConvert.ToHex(buffer));

            BluetoothUuid uuid = new BluetoothUuid(CcidPcToRdrCharacteristicUuid);
			int tosend = buffer.Length;
            int sent = 0;

			while (tosend > 0)
			{
				int payload_length = tosend;

				if ( tosend > MAX_LONG_WRITE)
				{
					payload_length = MAX_LONG_WRITE;
				}

                Logger.Debug("sending {0}/{1}", payload_length, buffer.Length);

                byte[] block = new byte[payload_length];

				Array.Copy(buffer, sent, block, 0, payload_length);               

                Logger.Debug("<<< " + BinConvert.ToHex(block));
				if (!bleDevice.WriteCharacteristic(uuid, block))
				{
					Logger.Trace("Can't write to the device");
					if (deviceState >= DeviceState.Active)
						deviceState = DeviceState.Error;
					return false;
				}
                Logger.Debug("sen!" );

                sent += payload_length;
                tosend -= payload_length;
            }
            Logger.Trace("SendCcidPcToRdr Done!");
            return true;
		}

		private void RecvCcidRdrToPc()
		{
			byte[] buffer;

			Logger.Trace("CCID over BLE: receiving (" + Thread.CurrentThread.ManagedThreadId + "), Protocol v2");

			buffer = bleDevice.ReadCharacteristic(new BluetoothUuid(CcidRdrToPcCharacteristicUuid));

			if (buffer == null)
			{
				Logger.Trace("Can't read from the device!");
				if (deviceState >= DeviceState.Active)
					deviceState = DeviceState.Error;
				return;
			}

            Logger.Debug("BLE>" + BinConvert.ToHex(buffer));
            Recv(CCID.EP_Bulk_RDR_To_PC, buffer);
		}

		private void BackgroundReceiver()
		{
			RecvCcidRdrToPc();
		}

		protected override void BleDeviceNotificationCallback(BLE.Device bleDevice, BluetoothUuid uuid, byte[] value)
		{
			Logger.Trace("CCID over BLE: notified (" + Thread.CurrentThread.ManagedThreadId + ")");

			if (uuid.Equals(new BluetoothUuid(CcidStatusCharacteristicUuid)))
			{
				Logger.Trace("CCID over BLE: the status has changed");

				if (value.Length >= 2)
				{
					byte byte0 = value[0];
					byte byte1 = value[1];

					Logger.Trace("CCID over BLE: the status has changed: " + BinConvert.ToHex(byte0) + BinConvert.ToHex(byte1));

					if ((byte1 & 0x01) != 0)
					{
						if (!Children[0].IsCardPresent())
						{
							Logger.Trace("CCID over BLE: card inserted");
							Children[0].NotifyInsert();
						}
					}
					else
					{
						if (Children[0].IsCardPresent())
						{
							Logger.Trace("CCID over BLE: card removed");
							Children[0].NotifyRemove();
						}
					}

                    /* CCID v3 and up use notication to receive RDR_to_PC */
					if ((byte0 & 0x80) != 0 && ProtocolVersion == 2)
					{
						Logger.Trace("CCID over BLE: a response is ready");
						asyncThread = new Thread(BackgroundReceiver);
						asyncThread.Start();
					}
				}
			}
            else if(uuid.Equals(new BluetoothUuid(CcidRdrToPcCharacteristicUuid)))
            {
                Logger.Trace("CCID over BLE: receiving (" + Thread.CurrentThread.ManagedThreadId + ")");

                if (value == null)
                {
                    Logger.Trace("Can't read from the device!");
                    if (deviceState >= DeviceState.Active)
                        deviceState = DeviceState.Error;
                    return;
                }

                Logger.Debug("BLE>" + BinConvert.ToHex(value));
                Recv(CCID.EP_Bulk_RDR_To_PC, value);

            }
        }
	}
}
