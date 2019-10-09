using SpringCard.LibCs;
using SpringCard.PCSC;
using SpringCard.PCSC.CardHelpers;

namespace DesfireTool
{
    partial class Program
    {
        Result Authenticate(Desfire desfire, Parameters parameters)
        {
            if (parameters.KeyId < 0)
            {
                ConsoleError("Authenticate: Key index must be specified");
                ConsoleWriteLine("Use parameter --key-index|-k");
                return Result.MissingArgument;
            }
            if (parameters.KeyValue == null)
            {
                ConsoleError("Authenticate: Key value must be specified");
                ConsoleWriteLine("Use parameter --key-value|-K");
                return Result.MissingArgument;
            }
            if ((parameters.KeyValue.Length != 8) && (parameters.KeyValue.Length != 16))
            {
                ConsoleError("Authenticate: Key value must be a DES or 3DES2K key");
                ConsoleWriteLine("Specify 8 or 16 bytes exactly");
                return Result.InvalidArgument;
            }

            long rc = desfire.Authenticate((byte)parameters.KeyId, parameters.KeyValue);
            if (rc != SCARD.S_SUCCESS)
                return OnError("Authenticate", rc);

            return Result.Success;
        }

        Result AuthenticateISO(Desfire desfire, Parameters parameters)
        {
            if (parameters.KeyId < 0)
            {
                ConsoleError("AuthenticateISO: Key index must be specified");
                ConsoleWriteLine("Use parameter --key-index|-k");
                return Result.MissingArgument;
            }
            if (parameters.KeyValue == null)
            {
                ConsoleError("AuthenticateISO: Key value must be specified");
                ConsoleWriteLine("Use parameter --key-value|-K");
                return Result.MissingArgument;
            }
            if ((parameters.KeyValue.Length != 8) && (parameters.KeyValue.Length != 16) && (parameters.KeyValue.Length != 24))
            {
                ConsoleError("AuthenticateISO: Key value must be a DES, 3DES2K or 3DES3K key");
                ConsoleWriteLine("Specify 8, 16 or 24 bytes exactly");
                return Result.InvalidArgument;
            }

            long rc;
            if (parameters.KeyValue.Length != 24)
            {
                rc = desfire.AuthenticateIso((byte)parameters.KeyId, parameters.KeyValue);
            }
            else
            {
                rc = desfire.AuthenticateIso24((byte)parameters.KeyId, parameters.KeyValue);
            }

            if (rc != SCARD.S_SUCCESS)
                return OnError("AuthenticateISO", rc);

            return Result.Success;
        }

        Result AuthenticateAES(Desfire desfire, Parameters parameters)
        {
            if (parameters.KeyId < 0)
            {
                ConsoleError("AuthenticateAES: Key index must be specified");
                ConsoleWriteLine("Use parameter --key-index|-k");
                return Result.MissingArgument;
            }
            if (parameters.KeyValue == null)
            {
                ConsoleError("AuthenticateAES: Key value must be specified");
                ConsoleWriteLine("Use parameter --key-value|-K");
                return Result.MissingArgument;
            }
            if (parameters.KeyValue.Length != 16)
            {
                ConsoleError("AuthenticateAES: Key value must be a AES-128 key");
                ConsoleWriteLine("Specify 16 bytes exactly");
                return Result.InvalidArgument;
            }

            long rc = desfire.AuthenticateAes((byte)parameters.KeyId, parameters.KeyValue);
            if (rc != SCARD.S_SUCCESS)
                return OnError("AuthenticateAES", rc);

            return Result.Success;
        }
    }
}

