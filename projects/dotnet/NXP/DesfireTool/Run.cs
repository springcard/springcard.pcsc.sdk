using SpringCard.LibCs;
using SpringCard.PCSC;
using SpringCard.PCSC.CardHelpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesfireTool
{
    partial class Program
    {
        Result Run(string[] args)
        {
            if (!ParseArgs(args, out Action action, out Parameters parameters))
            {
                return Result.InvalidArgument;
            }

            if ((action == Action.Create) || (action == Action.Script))
            {
                if (InputFile == null)
                {
                    ConsoleError("Input file must be specified");
                    ConsoleWriteLine("Use parameter --input-file|-i");
                    return Result.InvalidArgument;
                }
            }

            if (action == Action.ListReaders)
            {
                string[] ReaderNames = SCARD.Readers;
                Console.WriteLine(string.Format("{0} PC/SC Reader(s) found", ReaderNames.Length));
                for (int i = 0; i < ReaderNames.Length; i++)
                    Console.WriteLine(string.Format("{0}: {1}", i, ReaderNames[i]));
                return Result.Success;
            }

            if (ReaderName == null)
                ReaderName = "SpringCard *";

            if (ReaderName.Contains("*"))
            {
                string newReaderName = SCARD.ReaderLike(ReaderName);
                if (newReaderName == null)
                {
                    Console.WriteLine("No PC/SC reader matching \"{0}\"", ReaderName);
                    return Result.InvalidArgument;
                }
                ReaderName = newReaderName;
            }
            else
            {
                if (!SCARD.ReaderExists(ReaderName))
                {
                    Console.WriteLine("PC/SC reader \"{0}\" not found", ReaderName);
                    return Result.InvalidArgument;
                }
            }

            Result result = ConnectToCard(out SCardChannel channel, out Desfire desfire);
            if (result != Result.Success)
                return result;

            if (LoadContextOnEnter)
                LoadCardContext(desfire);

            if (action == Action.Script)
            {
                result = RunScript(desfire, InputFile);
            }
            else if (action == Action.Create)
            {
                result = JsonToCard(desfire, InputFile);
            }
            else
            {
                result = RunEx(desfire, action, parameters);
            }

            if (result == Result.Success)
                if (SaveContextOnExit)
                    SaveCardContext(desfire);

            if (KeepCardAlive)
            {
                Logger.Trace("Card stay active, reader is disconnected.");
                channel.DisconnectLeave();
            }
            else
            {
                channel.DisconnectReset();
            }

            return result;
        }

        Result RunEx(Desfire desfire, Action action, Parameters parameters)
        {
            switch (action)
            {
                case Action.Authenticate:
                    return Authenticate(desfire, parameters);
                case Action.AuthenticateAES:
                    return AuthenticateAES(desfire, parameters);
                case Action.AuthenticateISO:
                    return AuthenticateISO(desfire, parameters);
                //                case Action.ChangeFileSettings:
                //                    return ChangeFileSettings(desfire, parameters);
                case Action.ChangeKey:
                    return ChangeKey(desfire, parameters);
                case Action.ChangeKeyAES:
                    return ChangeKeyAES(desfire, parameters);
                case Action.CommitTransaction:
                    return CommitTransaction(desfire, parameters);
                case Action.CreateApplication:
                    return CreateApplication(desfire, parameters);
                case Action.CreateStdDataFile:
                    return CreateStdDataFile(desfire, parameters);
                case Action.CreateCyclicRecordFile:
                    return CreateCyclicRecordFile(desfire, parameters);
                case Action.CreateValueFile:
                    return CreateValueFile(desfire, parameters);
                case Action.Format:
                    return Format(desfire, parameters);
                case Action.FreeMem:
                    return FreeMem(desfire, parameters);
                case Action.GetApplicationIDs:
                    return GetApplicationIDs(desfire, parameters);
                case Action.GetCardUID:
                    return GetCardUID(desfire, parameters);
                case Action.GetKeySettings:
                    return GetKeySettings(desfire, parameters);
                case Action.GetKeyVersion:
                    return GetKeyVersion(desfire, parameters);
                case Action.GetVersion:
                    return GetVersion(desfire, parameters);
                case Action.ReadData:
                    return ReadData(desfire, parameters);
                case Action.ReadRecords:
                    return ReadRecords(desfire, parameters);
                case Action.SelectApplication:
                    return SelectApplication(desfire, parameters);
                case Action.WriteData:
                    return WriteData(desfire, parameters);
                case Action.WriteRecord:
                    return WriteRecord(desfire, parameters);
            }

            Console.WriteLine("Action {0} is not yet implemented", action);
            return Result.ActionNotImplemented;
        }

        Result OnError(string function, long rc)
        {
            string m = string.Format("{0}: command failed, rc={1}", function, rc);
            ConsoleError(m);

            switch (rc)
            {
                case -988: /* WANTS_ADDITIONAL_FRAME */
                    ConsoleError("The PICC requests another frame.");
                    break;

                case -1030: /* DF_INTEGRITY_ERROR */
                    ConsoleError("CRC or MAC does not match, or invalid padding bytes.");
                    break;

                case -1126: /* DF_LENGTH_ERROR */
                    ConsoleError("Wrong length.");
                    break;

                case -1158: /* DF_PARAMETER_ERROR */
                    ConsoleError("Wrong parameters");
                    break;

                case -1174:
                    ConsoleError("Authentification required!");
                    if (InputFile != null)
                    {
                        Console.WriteLine("Don't forget to uses the '--keep' option to keep the current PICC session active between '" + AppInfo.Name + "' calls!");
                    }
                    break;

                case -1175: /* DF_ADDITIONAL_FRAME */
                    ConsoleError("The PICC requests more data.");
                    break;

                case -1160: /* DF_APPLICATION_NOT_FOUND */
                    ConsoleError("The Application ID used may be wrong (or bytes are in the wrong order).");
                    break;
            }

            return Result.CardOperationFailed;
        }

        Result ConnectToCard(out SCardChannel channel, out Desfire desfire)
        {
            desfire = null;

            channel = new SCardChannel(ReaderName);
            if (channel == null)
            {
                ConsoleError("Failed to connect to the reader.");
                return Result.CardOperationFailed;
            }

            if (!channel.ConnectExclusive())
            {
                ConsoleError("Failed to connect to the card.");
                return Result.CardOperationFailed;
            }

            desfire = new Desfire(channel);
            desfire.IsoWrapping(Desfire.DF_ISO_WRAPPING_CARD);

            /* Okay, try to obtain model number (EV0, EV1 and EV2) */
#if false
            long rc = desfire.GetVersion(out Desfire.DF_VERSION_INFO VersionInfo);
            if (rc != SCARD.S_SUCCESS)
            {
                Logger.Warning("Desfire 'GetVersion' command failed - rc={0}.", rc);
            }
            else
            {
                if (VersionInfo.bHwVendorID != 0x04)
                {
                    Logger.Error("Not an Desfire card!");
                }
                switch (VersionInfo.bHwMajorVersion)
                {
                    case 0x00:
                        Logger.Info("Desfire EV0 detected");
                        break;
                    case 0x01:
                        Logger.Info("Desfire EV1 detected");
                        break;
                    case 0x12:
                        Logger.Info("Desfire EV2 detected");
                        break;
                    default:
                        Logger.Info("Unknow Desfire revision!");
                        break;
                }
            }
#endif
            return Result.Success;
        }
    }
}
