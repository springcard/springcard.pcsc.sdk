using System;
using System.Drawing;
using System.Collections.Generic;
using SpringCard.PCSC;
using SpringCard.LibCs;
using System.Security.Cryptography;

namespace SpringCard.PCSC.ReaderHelper
{
    public partial class SpringCore
    {
        public enum Sequences : byte
        {
            Blackout = 0x00,
            NoMotherBoard,
            NoBattery,
            HardwareError,
            Undefined04,
            Cancel,
            Startup,
            Shutdown,
            PleaseWait,
            BootloaderWaiting,
            FirmwareUpgrading,
            FirmwareUpgraded,
            ConfigCleared,
            SelfTest,
            Wink,
            FirmwareData,
            NfcRfidInactive = 0x20,
            NfcRfidActive,
            NfcRfidDiscover,
            NfcRfidTagPresent,
            NfcRfidTagRead,
            NfcRfidFailure
        };
    }
}
