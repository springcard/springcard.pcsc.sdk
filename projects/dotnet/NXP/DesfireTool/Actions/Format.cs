using SpringCard.LibCs;
using SpringCard.PCSC;
using SpringCard.PCSC.CardHelpers;

namespace DesfireTool
{
    partial class Program
    {
        Result Format(Desfire desfire, Parameters parameters)
        {
            long rc = desfire.FormatPICC();
            if (rc != SCARD.S_SUCCESS)
                return OnError("FormatPICC", rc);

            return Result.Success;
        }
    }
}
