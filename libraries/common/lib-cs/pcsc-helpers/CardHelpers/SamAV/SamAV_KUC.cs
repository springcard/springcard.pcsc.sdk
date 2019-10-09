using System;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Reflection;
using SpringCard.PCSC;
using SpringCard.LibCs;

namespace SpringCard.PCSC.CardHelpers
{
    public partial class SamAV
    {
        public const byte KUCEntriesCount = 16;

        public class KUCEntry
        {
            public ulong Limit = 0xFFFFFFFF;
            public byte ChangeKeyIdx = 0xFE;
            public byte ChangeKeyVersion = 0x00;
            public ulong Value = 0x00000000;

            public KUCEntry()
            {

            }
        }

        public Result GetKUCEntry(byte KucIdx, out KUCEntry KucEntry)
        {
            KucEntry = null;
            Result result = Command(INS.GetKUCEntry, KucIdx, 0x00, out byte[] data, Expect.Success);
            if (result != Result.Success)
                return result;

            if ((data == null) || (data.Length != 10))
                return Result.InvalidResponseData;

            KucEntry = new KUCEntry();

            KucEntry.Limit = BinUtils.ToDword(BinUtils.Copy(data, 0, 4), BinUtils.Endianness.LittleEndian);
            KucEntry.ChangeKeyIdx = data[4];
            KucEntry.ChangeKeyVersion = data[5];
            KucEntry.Value = BinUtils.ToDword(BinUtils.Copy(data, 6, 4), BinUtils.Endianness.LittleEndian);

            return Result.Success;
        }
    }
}
