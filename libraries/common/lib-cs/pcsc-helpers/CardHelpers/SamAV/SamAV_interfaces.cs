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
        public interface DataProvider
        {
            string GetEntryString(byte address);
            void SetEntry(byte address, byte[] value);
            void ProtectEntry(string version, byte address, string value);
            byte GetCounterAndIncrement();
        }
    }

}