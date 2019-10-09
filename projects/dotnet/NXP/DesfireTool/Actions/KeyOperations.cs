using SpringCard.LibCs;
using SpringCard.PCSC;
using SpringCard.PCSC.CardHelpers;
using System;

namespace DesfireTool
{
    partial class Program
    {
        Result GetKeyVersion(Desfire desfire, Parameters parameters)
        {
            if (parameters.KeyId < 0)
            {
                ConsoleError("GetKeyVersion: Key index must be specified");
                ConsoleWriteLine("Use parameter --key-index|-k");
                return Result.MissingArgument;
            }

            byte keyVersion = 0;
            long rc = desfire.GetKeyVersion((byte)parameters.KeyId, ref keyVersion);
            if (rc != SCARD.S_SUCCESS)
                return OnError("GetKeyVersion", rc);

            Console.WriteLine("Key Version: " + BinConvert.ToHex(keyVersion));
            return Result.Success;
        }

        Result GetKeySettings(Desfire desfire, Parameters parameters)
        {
            byte keySettings = 0;
            byte keyCount = 0;

            long rc = desfire.GetKeySettings(ref keySettings, ref keyCount);
            if (rc != SCARD.S_SUCCESS)
                return OnError("GetKeySettings", rc);

            if ((keyCount & 0x0F) != 0)
            {
                Logger.Trace("Desfire 'GetKeySettings' command done at Application level");

                int test = keyCount >> 6;

                switch (test)
                {
                    case 0:
                        Console.WriteLine("Using (3)DES operations");
                        break;
                    case 1:
                        Console.WriteLine("Using 3K3DES operations");
                        break;
                    case 2:
                        Console.WriteLine("Using AES operations");
                        break;
                    default:
                        Console.WriteLine("Using RFU operations");
                        break;
                }
                Console.WriteLine("Maximum number of keys: " + (keyCount >> 4));
            }
            else
            {
                Logger.Trace("Desfire 'GetKeySettings' command done at PICC level");
            }

            if ((keySettings & 0x01) != 0)
                Console.WriteLine("Changing the master key is allowed.");
            if ((keySettings & 0x02) != 0)
                Console.WriteLine("Free directory list access without master key allowed.");
            if ((keySettings & 0x04) != 0)
                Console.WriteLine("Master key not required for create/Delete operations.");
            if ((keySettings & 0x08) != 0)
                Console.WriteLine("Configuration modififications allowed.");

            return Result.Success;
        }

        Result ChangeKey(Desfire desfire, Parameters parameters)
        {
            if (parameters.KeyId < 0)
            {
                ConsoleError("ChangeKey: Key index must be specified");
                ConsoleWriteLine("Use parameter --key-index|-k");
                return Result.MissingArgument;
            }
            if (parameters.KeyValue == null)
            {
                ConsoleError("ChangeKey: New key value must be specified");
                ConsoleWriteLine("Use parameter --key-value|-K");
                return Result.MissingArgument;
            }
            if ((parameters.KeyValue.Length != 8) && (parameters.KeyValue.Length != 16) && (parameters.KeyValue.Length != 24))
            {
                ConsoleError("ChangeKey: New key value must be a DES, 3DES2K or 3DES3K key");
                ConsoleWriteLine("Specify 8, 16 or 24 bytes exactly");
                return Result.InvalidArgument;
            }
            if (parameters.OldKeyValue != null)
            {
                if (parameters.OldKeyValue.Length != parameters.KeyValue.Length)
                {
                    ConsoleError("ChangeKey: Old key value must have the same size as new key value");
                    ConsoleWriteLine(string.Format("Specify {0} bytes exactly", parameters.KeyValue.Length));
                    return Result.InvalidArgument;
                }
            }

            if (parameters.KeyValue.Length != 24)
            {
                long rc = desfire.ChangeKey((byte)parameters.KeyId, parameters.KeyValue, parameters.OldKeyValue);
                if (rc != SCARD.S_SUCCESS)
                    return OnError("ChangeKey", rc);
            }
            else
            {                
                long rc = desfire.ChangeKey24((byte)parameters.KeyId, parameters.KeyValue, parameters.OldKeyValue);
                if (rc != SCARD.S_SUCCESS)
                    return OnError("ChangeKey24", rc);
            }

            return Result.Success;
        }

        Result ChangeKeyAES(Desfire desfire, Parameters parameters)
        {
            if (parameters.KeyId < 0)
            {
                ConsoleError("ChangeKeyAES: Key index must be specified");
                ConsoleWriteLine("Use parameter --key-index|-k");
                return Result.MissingArgument;
            }
            if (parameters.KeyValue == null)
            {
                ConsoleError("ChangeKeyAES: New key value must be specified");
                ConsoleWriteLine("Use parameter --key-value|-K");
                return Result.MissingArgument;
            }
            if (parameters.KeyValue.Length != 16)
            {
                ConsoleError("ChangeKeyAES: New key value must be an AES-128 key");
                ConsoleWriteLine("Specify 16 bytes exactly");
                return Result.InvalidArgument;
            }
            if (parameters.OldKeyValue != null)
            {
                if (parameters.OldKeyValue.Length != 16)
                {
                    ConsoleError("ChangeKeyAES: Old key value must be an AES-128 key");
                    ConsoleWriteLine("Specify 16 bytes exactly");
                    return Result.InvalidArgument;
                }
            }
            if (parameters.KeyVersion < 0)
            {
                ConsoleError("ChangeKeyAES: Key version must be specified");
                ConsoleWriteLine("Use parameter --key-version");
                return Result.MissingArgument;
            }

            long rc = desfire.ChangeKeyAes((byte)parameters.KeyId, (byte)parameters.KeyVersion, parameters.KeyValue, parameters.OldKeyValue);
            if (rc != SCARD.S_SUCCESS)
                return OnError("ChangeKeyAES", rc);

            return Result.Success;
        }
    }
}
