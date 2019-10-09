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
        public enum AuthTypeE
        {
            TDES_CRC16 = 0,
            TDES_CRC32 = 1,
            AES = 2,
            MIFARE = 3
        }

        public enum Result
        {
            Success,
            Continue,
            CommunicationError,
            ExecutionFailed,
            UnexpectedStatus,
            InvalidResponseData,
            InvalidParameters
        }
    }
}
