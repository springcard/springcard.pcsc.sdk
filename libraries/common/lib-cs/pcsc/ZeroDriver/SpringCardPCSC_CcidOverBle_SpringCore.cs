using System;
using System.IO.Ports;
using System.Threading;
using SpringCard.LibCs;
using SpringCard.Bluetooth;
using SpringCard.PCSC;

namespace SpringCard.PCSC.ZeroDriver
{
	public class SCardReaderList_CcidOverBle_SpringCore : SCardReaderList_CcidOverBle
	{
		public const string CcidUnbondedServiceUuid = "F91C914F-367C-4108-AC3E-3D30CFDD0A1A";
		public const string CcidUnbondedStatusCharacteristicUuid = "EAB75CAB-C7DC-4DB9-874C-4AD8EE0F180F";
		public const string CcidUnbondedPcToRdrCharacteristicUuid = "281EBED4-86C4-4253-84F1-57FB9AB2F72C";
		public const string CcidUnbondedRdrToPcCharacteristicUuid = "811DC7A6-A573-4E15-89CC-7EFACAE04E3C";

		public const string CcidBondedServiceUuid = "7F20CDC5-A9FC-4C70-9292-3ACF9DE71F73";
		public const string CcidBondedStatusCharacteristicUuid = "DC2AA4CA-76A9-43F9-9FE5-127652837EF5";
		public const string CcidBondedPcToRdrCharacteristicUuid = "CD5BCE75-65FC-4747-AB9A-FF82BFDFA7FB";
		public const string CcidBondedRdrToPcCharacteristicUuid = "94EDE62E-0808-46F8-91EC-AC0272D67796";

		private BluetoothUuid CcidServiceUuid;
		private BluetoothUuid CcidStatusCharacteristicUuid;
		private BluetoothUuid CcidPcToRdrCharacteristicUuid;
		private BluetoothUuid CcidRdrToPcCharacteristicUuid;

		internal SCardReaderList_CcidOverBle_SpringCore(BLE.Adapter BleAdapter, BluetoothAddress DeviceAddress, SecureConnectionParameters secureConnectionParameters, bool UseBonding)
		{
			bleDevice = BleAdapter.CreateDevice(DeviceAddress);
			bleUseBonding = UseBonding;
            this.secureConnectionParameters = secureConnectionParameters;
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

			bool unbondedServiceFound = false;
			bool bondedServiceFound = false;

			foreach (BluetoothUuid service in services)
			{
				if (service.Equals(CcidUnbondedServiceUuid))
				{
					Logger.Debug("+ " + service.ToString() + " (PC/SC, unbonded)");
					unbondedServiceFound = true;
				}
				else
				if (service.Equals(CcidBondedServiceUuid))
				{
					Logger.Debug("+ " + service.ToString() + " (PC/SC bonded)");
					bondedServiceFound = true;
				}
				else
				{
					Logger.Debug("+ " + service.ToString());
				}
			}

			if (!bleUseBonding && !unbondedServiceFound)
			{
				Logger.Trace("PC/SC unbonded Service not found in the device");
				return false;
			}
			if (bleUseBonding && !bondedServiceFound)
			{
				Logger.Trace("PC/SC bonded Service not found in the device");
				return false;
			}

			if (bleUseBonding)
			{
				CcidServiceUuid = new BluetoothUuid(CcidBondedServiceUuid);
				CcidStatusCharacteristicUuid = new BluetoothUuid(CcidBondedStatusCharacteristicUuid);
				CcidPcToRdrCharacteristicUuid = new BluetoothUuid(CcidBondedPcToRdrCharacteristicUuid);
				CcidRdrToPcCharacteristicUuid = new BluetoothUuid(CcidBondedRdrToPcCharacteristicUuid);
			}
			else
			{
				CcidServiceUuid = new BluetoothUuid(CcidUnbondedServiceUuid);
				CcidStatusCharacteristicUuid = new BluetoothUuid(CcidUnbondedStatusCharacteristicUuid);
				CcidPcToRdrCharacteristicUuid = new BluetoothUuid(CcidUnbondedPcToRdrCharacteristicUuid);
				CcidRdrToPcCharacteristicUuid = new BluetoothUuid(CcidUnbondedRdrToPcCharacteristicUuid);
			}


			BluetoothUuid[] characteristics = bleDevice.GetGattCharacteristics(CcidServiceUuid);
			if (characteristics == null)
			{
				Logger.Trace("Failed to read the characteristics from the device's CCID Service");
				return false;
			}

			bool CcidStatusCharacteristicFound = false;
			bool CcidPcToRdrCharacteristicFound = false;
			bool CcidRdrToPcCharacteristicFound = false;

			Logger.Debug("List of characteristics in the CCID Service: ");
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

			if ((!CcidStatusCharacteristicFound) || (!CcidPcToRdrCharacteristicFound) || (!CcidRdrToPcCharacteristicFound))
			{
				Logger.Trace("Mandatory Characteristics not found in the device");
				return false;
			}

			return true;
		}

        override protected bool ReadSoftwareVersion()
        {
            if (!ReadAndParseRevisionString())
            {
                Logger.Trace("Could not read or parse revision string");
                return false;

            }

            return true;
        }

        protected override bool GetSlotCount()
        {
            byte[] buffer = bleDevice.ReadCharacteristic(CcidStatusCharacteristicUuid);
            if (buffer == null)
            {
                Logger.Trace("Failed to read CCID_STATUS");
                return false;
            }

            SlotCount = buffer[0] & 0x0F;
            return true;
        }

        protected override bool EnterCcidMode()
		{
			Logger.Trace("Activating the PC/SC profile...");

			if (!bleDevice.EnableCharacteristicEvents(CcidStatusCharacteristicUuid))
			{
				Logger.Trace("Failed to enable the notifications on CCID_STATUS");
				return false;
			}

			if (!bleDevice.EnableCharacteristicEvents(CcidRdrToPcCharacteristicUuid))
			{
				Logger.Trace("Failed to enable the notifications on CCID_RDR_TO_PC");
				return false;
			}

            deviceState = DeviceState.Active;
			return true;
		}

        protected override bool ExitCcidMode()
		{
			return true;
		}

        protected override bool RemoteDisconnect()
        {
            Logger.Trace("Ask the reader to disconnect...");
            byte[] frame = new byte[12];

            frame[0] = CCID.RDR_TO_PC_ESCAPE;
            frame[1] = 2;
            frame[10] = 0x58;
            frame[11] = 0xAE;

            return SendCcidPcToRdr(frame);
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

                if (tosend > MAX_LONG_WRITE)
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
                sent += payload_length;
                tosend -= payload_length;
            }
            Logger.Trace("SendCcidPcToRdr Done!");
            return true;
        }

        private void RecvCcidRdrToPc()
		{
			byte[] buffer;

			buffer = bleDevice.ReadCharacteristic(CcidRdrToPcCharacteristicUuid);
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

		private void OnRdrToPcNotification(byte[] buffer)
		{
            Logger.Debug("BLE>" + BinConvert.ToHex(buffer));
            Recv(CCID.EP_Bulk_RDR_To_PC, buffer);
		}

		private void OnStatusNotification(byte[] buffer)
		{
			if ((buffer == null) || (buffer.Length < 1))
				return;

			int slots = buffer[0] & 0x0F;
			if (slots > Children.Length)
				slots = Children.Length;

			if (buffer.Length < (2 + (slots - 1) / 4))
				return;

			for (int slot = 0; slot < slots; slot++)
			{
				int byte_offset = ( slot / 4 ) + 1;
				int bit_offset = 2 * (slot % 4);

				bool present = ((buffer[byte_offset] & (1 << bit_offset)) != 0);
				bool changed = ((buffer[byte_offset] & (2 << bit_offset)) != 0);

                if (present)
                {
                    Logger.Trace("CCID over BLE:slot{0}:present", slot);
                }

                if (changed)
                {
                    Logger.Trace("CCID over BLE:slot{0}:changed", slot);
                }

                if (present && !Children[slot].IsCardPresent())
				{
                    Logger.Trace("CCID over BLE:slot{0}:card inserted", slot);
					Children[slot].NotifyInsert();
				}
				else if (!present && Children[slot].IsCardPresent())
				{
					Logger.Trace("CCID over BLE:slot{0}:card removed", slot);
					Children[slot].NotifyRemove();
				}
			}
		}

		protected override void BleDeviceNotificationCallback(BLE.Device bleDevice, BluetoothUuid uuid, byte[] value)
		{
			if (uuid.Equals(CcidStatusCharacteristicUuid))
			{
				Logger.Debug("CCID_STATUS={0}", BinConvert.ToHex(value));
				OnStatusNotification(value);
			}
			else if (uuid.Equals(CcidRdrToPcCharacteristicUuid))
			{
				OnRdrToPcNotification(value);
			}
			else
			{
				Logger.Trace("CCID over BLE: unhandled notification for characteristic " + uuid.ToString());
			}
		}
	}
}
