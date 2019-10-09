using SpringCard.LibCs;
using SpringCard.PCSC;
using SpringCard.PCSC.CardHelpers;
using System;

namespace DesfireTool
{
    partial class Program
    {
        Result FreeMem(Desfire desfire, Parameters parameters)
        {
            uint freeMem = 0;
            long rc = desfire.GetFreeMemory(ref freeMem);
            if (rc != SCARD.S_SUCCESS)
                return OnError("FreeMem", rc);

            if (freeMem == 0)
            {
                Console.WriteLine("There is no free space on this PICC.");
            }
            else
            {
                Console.WriteLine("There is " + freeMem + " bytes available on this PICC.");
            }

            return Result.Success;
        }

        Result GetVersion(Desfire desfire, Parameters parameters)
        {
            Desfire.DF_VERSION_INFO versionInfo;
            long rc = desfire.GetVersion(out versionInfo);
            if (rc != SCARD.S_SUCCESS)
                return OnError("GetVersion", rc);

            Logger.Trace("Desfire 'GetVersion' command done.");
            Console.WriteLine("Hardware=> Vendor Id: " + versionInfo.bHwVendorID + ", Type: " + versionInfo.bHwType + ", SubType: " + versionInfo.bHwSubType + ", Version: " + versionInfo.bHwMajorVersion + "." + versionInfo.bHwMinorVersion);
            Console.WriteLine("Software=> Vendor Id: " + versionInfo.bSwVendorID + ", Type: " + versionInfo.bSwType + ", SubType: " + versionInfo.bSwSubType + ", Version: " + versionInfo.bSwMajorVersion + "." + versionInfo.bSwMinorVersion);

            if ((versionInfo.bHwVendorID != 0x04) || (versionInfo.bSwVendorID != 0x04))
            {
                Console.WriteLine("Manufacturer is not NXP");
            }

            if ((versionInfo.bHwType != 0x01) || (versionInfo.bSwType != 0x01))
            {
                Console.WriteLine("Type is not Desfire");
            }

            if (versionInfo.bSwMajorVersion != 0x01)
            {
                Console.WriteLine("Software version is below EV1");
            }

            return Result.Success;
        }

        Result GetCardUID(Desfire desfire, Parameters parameters)
        {
            byte[] cardUID;
            long rc = desfire.GetCardUID(out cardUID);
            if (rc != SCARD.S_SUCCESS)
                return OnError("GetCardUID", rc);

            Console.WriteLine("Desfire 'GetCardUID' command done: " + BinConvert.ToHex(cardUID));

            return Result.Success;
        }

    }
}
