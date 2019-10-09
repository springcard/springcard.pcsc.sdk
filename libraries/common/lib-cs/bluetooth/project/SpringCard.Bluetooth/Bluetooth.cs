using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using SpringCard.LibCs;

namespace SpringCard.Bluetooth
{
    public class BluetoothAddress
    {
        private byte[] value;

        public BluetoothAddress(ulong data)
        {
            value = new byte[6];
            for (int i = 0; i <= 5; i++)
            {
                value[i] = (byte)(data & 0x0FF);
                data >>= 8;
            }
        }

        public BluetoothAddress(byte[] data)
        {
            value = new byte[6];
            Array.Copy(data, value, 6);
        }

        public BluetoothAddress(string addr)
        {
            addr = addr.Replace(":", "");
            value = BinConvert.HexToBytes(addr);
            Array.Reverse(value);
        }

        public byte[] ToBytes()
        {
            return value;
        }

        public ulong ToULong()
        {
            ulong result = 0;
            for (int i = 0; i <= 5; i++)
            {
                result <<= 8;
                result |= value[5 - i];
            }
            return result;
        }

        public override string ToString()
        {
            if (value == null)
                return "";

            if (value.Length == 6)
            {
                return string.Format("{0:X02}:{1:X02}:{2:X02}:{3:X02}:{4:X02}:{5:X02}",
                                        value[5], value[4], value[3], value[2], value[1], value[0]);
            }

            return BinConvert.ToHex(value);
        }

        public static implicit operator byte[] (BluetoothAddress Addr)
        {
            return Addr.ToBytes();
        }
    }

    public class BluetoothUuid
    {
        private const string BluetoothUUIDMark = "0000FFFF-0000-1000-8000-00805F9B34FB";

        protected byte[] value;

        private void CleanValue()
        {
            byte[] mark = FromString(BluetoothUUIDMark);

            for (int i = 0; i < 16; i++)
            {
                if ((i == 12) || (i == 13)) continue;
                if (value[i] != mark[i]) return;
            }

            byte[] truncated = new byte[2];
            truncated[0] = value[12];
            truncated[1] = value[13];
            value = truncated;
        }

        public BluetoothUuid()
        {

        }

        public BluetoothUuid(byte[] data)
        {
            value = new byte[data.Length];
            Array.Copy(data, value, data.Length);
            CleanValue();
        }

        public BluetoothUuid(Guid guid)
        {
            byte[] t = guid.ToByteArray();
            value = new byte[16];

            value[15] = t[3];
            value[14] = t[2];
            value[13] = t[1];
            value[12] = t[0];

            value[11] = t[5];
            value[10] = t[4];

            value[9] = t[7];
            value[8] = t[6];

            value[7] = t[8];
            value[6] = t[9];

            value[5] = t[10];
            value[4] = t[11];
            value[3] = t[12];
            value[2] = t[13];
            value[1] = t[14];
            value[0] = t[15];

            CleanValue();
        }

        private byte[] FromString(string uuidString)
        {
            string t = uuidString;
            t = t.Replace("{", string.Empty);
            t = t.Replace("}", string.Empty);
            t = t.Replace("-", string.Empty);
            t = t.Replace(":", string.Empty);
            byte[] result = BinConvert.HexToBytes(t);
            Array.Reverse(result);
            return result;
        }

        public BluetoothUuid(string uuidString)
        {
            value = FromString(uuidString);
            CleanValue();
        }

        public int Length
        {
            get
            {
                if (value == null) return 0;
                return value.Length;
            }
        }

        public byte[] ToBytes()
        {
            return value;
        }

        public override string ToString()
        {
            if (value == null)
                return "";

            if (value.Length == 2)
            {
                return string.Format("{0:X02}{1:X02}", value[1], value[0]);
            }

            if (value.Length == 16)
            {
                return string.Format("{0:X02}{1:X02}{2:X02}{3:X02}-{4:X02}{5:X02}-{6:X02}{7:X02}-{8:X02}{9:X02}-{10:X02}{11:X02}{12:X02}{13:X02}{14:X02}{15:X02}",
                                        value[15], value[14], value[13], value[12],
                                        value[11], value[10], value[9], value[8],
                                        value[7], value[6], value[5], value[4],
                                        value[3], value[2], value[1], value[0]);
            }

            return BinConvert.ToHex(value);
        }

        public bool Equals(BluetoothUuid uuid)
        {
            string c1 = this.ToString().ToUpper();
            string c2 = uuid.ToString().ToUpper();
            return c1.Equals(c2);
        }

        public bool Equals(string uuid)
        {
            return Equals(new BluetoothUuid(uuid));
        }

        public static implicit operator byte[] (BluetoothUuid Uuid)
        {
            return Uuid.ToBytes();
        }
    }

}
