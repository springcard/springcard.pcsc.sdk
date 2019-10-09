using SpringCard.LibCs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesfireTool
{
    partial class Program
    {
        const Int64 NO_VALUE_64 = Int64.MinValue;

        string ReaderName = null;
        string InputFile = null;
        bool LoadContextOnEnter = false;
        bool SaveContextOnExit = false;
        bool KeepCardAlive = false;
        bool ForceAnswerYes = false;

        class Parameters
        {
            public long ApplicationId = -1;
            public long FileId = -1;
            public long Size = -1;
            public long Offset = -1;
            public long Count = -1;
            public long Number = -1;
            public long CommMode = -1;
            public int KeyId = -1;
            public int KeyVersion = -1;
            public long AccessRights;
            public byte[] KeySettings = null;
            public byte[] KeyValue = null;
            public byte[] OldKeyValue = null;
            public byte[] Data = null;
            public Int64 LowerLimit = NO_VALUE_64;
            public Int64 UpperLimit = NO_VALUE_64;
            public Int64 InitialValue = NO_VALUE_64;
            public int ValueFlags = -1;
        }

        bool ParseArgs(string[] args, out Action action, out Parameters parameters)
        {
            action = Action.Unknown;
            parameters = new Parameters();

            /* Read the action */
            /* --------------- */

            int offset = 0;

            for (offset = 0; offset < args.Length; offset++)
            {
                if (!args[offset].StartsWith("-"))
                {
                    string strAction = args[offset].ToLower().Replace("-", "");

                    switch (args[offset].ToLower())
                    {
                        case "listreaders":
                            action = Action.ListReaders;
                            break;

                        case "create":
                            action = Action.Create;
                            break;

                        case "script":
                            action = Action.Script;
                            break;

                        case "auth":
                        case "authenticate":
                            action = Action.Authenticate;
                            break;

                        case "authaes":
                        case "authenticateaes":
                            action = Action.AuthenticateAES;
                            break;

                        case "authev2first":
                        case "authenticateev2first":
                            action = Action.AuthenticateEV2First;
                            break;

                        case "authev2next":
                        case "authenticateev2next":
                            action = Action.AuthenticateEV2Next;
                            break;

                        case "authiso":
                        case "authenticateiso":
                            action = Action.AuthenticateISO;
                            break;

                        case "changefilesettings":
                            action = Action.ChangeFileSettings;
                            break;

                        case "changekey":
                            action = Action.ChangeKey;
                            break;

                        case "changekeyaes":
                            action = Action.ChangeKeyAES;
                            break;

                        case "changekeyev2":
                            action = Action.ChangeKeyEV2;
                            break;

                        case "cleearrecordfile":
                            action = Action.ClearRecordFile;
                            break;

                        case "commitreaderid":
                            action = Action.CommitReaderID;
                            break;

                        case "commit":
                        case "committransaction":
                            action = Action.CommitTransaction;
                            break;

                        case "createapp":
                        case "createapplication":
                            action = Action.CreateApplication;
                            break;

                        case "createbackupfile":
                        case "createbackupdatafile":
                            action = Action.CreateBackupDataFile;
                            break;

                        case "createcyclicfile":
                        case "createcyclicrecordfile":
                            action = Action.CreateCyclicRecordFile;
                            break;

                        case "createdelegatedapplication":
                            action = Action.CreateDelegatedApplication;
                            break;

                        case "createlinearfile":
                        case "createlinearrecordfile":
                            action = Action.CreateLinearRecordFile;
                            break;

                        case "createfile":
                        case "createdatafile":
                        case "createstddatafile":
                            action = Action.CreateStdDataFile;
                            break;

                        case "createmacfile":
                        case "createtransactionmacfile":
                            action = Action.CreateTransactionMacFile;
                            break;

                        case "createvaluefile":
                            action = Action.CreateValueFile;
                            break;

                        case "credit":
                            action = Action.Credit;
                            break;

                        case "debit":
                            action = Action.Debit;
                            break;

                        case "deleteapplication":
                            action = Action.DeleteApplication;
                            break;

                        case "deletefile":
                            action = Action.DeleteFile;
                            break;

                        case "finalizekeyset":
                            action = Action.FinalizeKeySet;
                            break;

                        case "format":
                        case "formatpicc":
                            action = Action.Format;
                            break;

                        case "freemem":
                        case "getfreemem":
                            action = Action.FreeMem;
                            break;

                        case "listapplications":
                        case "applicationids":
                        case "getapplicationids":
                            action = Action.GetApplicationIDs;
                            break;

                        case "carduid":
                        case "getcarduid":
                            action = Action.GetCardUID;
                            break;

                        case "listdf":
                        case "listdfs":
                        case "dfnames":
                        case "getdfnames":
                            action = Action.GetDFNames;
                            break;

                        case "delegatedinfo":
                        case "getdelegatedinfo":
                            action = Action.GetDelegatedInfo;
                            break;

                        case "listfiles":
                        case "fileids":
                        case "getfileids":
                            action = Action.GetFileIDs;
                            break;

                        case "filesettings":
                        case "getfilesettings":
                            action = Action.GetFileSettings;
                            break;

                        case "listef":
                        case "listefs":
                        case "getisofileids":
                            action = Action.GetISOFileIDs;
                            break;

                        case "keysettings":
                        case "getkeysettings":
                            action = Action.GetKeySettings;
                            break;

                        case "keyversion":
                        case "getkeyversion":
                            action = Action.GetKeyVersion;
                            break;

                        case "value":
                        case "getvalue":
                            action = Action.GetValue;
                            break;

                        case "version":
                        case "getversion":
                            action = Action.GetVersion;
                            break;

                        case "initializekeyset":
                            action = Action.InitializeKeySet;
                            break;

                        case "limitedcredit":
                            action = Action.LimitedCredit;
                            break;

                        case "readdata":
                            action = Action.ReadData;
                            break;

                        case "readrecords":
                            action = Action.ReadRecords;
                            break;

                        case "rollkeyset":
                            action = Action.RollKeySet;
                            break;

                        case "selectapplication":
                            action = Action.SelectApplication;
                            break;

                        case "setconfiguration":
                            action = Action.SetConfiguration;
                            break;

                        case "updaterecord":
                            action = Action.UpdateRecord;
                            break;

                        case "writedata":
                            action = Action.WriteData;
                            break;

                        case "writerecord":
                            action = Action.WriteRecord;
                            break;

                        default:
                            Console.WriteLine("'{0}' is not a valid action", args[offset]);
                            Hint();
                            return false;
                    }
                    break;
                }
            }

            /* Read the parameters and options */
            /* ------------------------------- */

            List<LongOpt> options = new List<LongOpt>();
            options.Add(new LongOpt("aid", Argument.Required, null, 'a'));
            options.Add(new LongOpt("application", Argument.Required, null, 'a'));
            options.Add(new LongOpt("application-id", Argument.Required, null, 'a'));
            options.Add(new LongOpt("file", Argument.Required, null, 'f'));
            options.Add(new LongOpt("file-id", Argument.Required, null, 'f'));
            options.Add(new LongOpt("mode", Argument.Required, null, 'm'));
            options.Add(new LongOpt("comm-mode", Argument.Required, null, 'm'));
            options.Add(new LongOpt("file-size", Argument.Required, null, 'z'));
            options.Add(new LongOpt("record-size", Argument.Required, null, 'z'));
            options.Add(new LongOpt("size", Argument.Required, null, 'z'));
            options.Add(new LongOpt("offset", Argument.Required, null, 'o'));
            options.Add(new LongOpt("record-count", Argument.Required, null, 'c'));
            options.Add(new LongOpt("count", Argument.Required, null, 'c'));
            options.Add(new LongOpt("number", Argument.Required, null, 'n'));
            options.Add(new LongOpt("data", Argument.Required, null, 'd'));
            options.Add(new LongOpt("access", Argument.Required, null, 1));
            options.Add(new LongOpt("access-rights", Argument.Required, null, 1));
            options.Add(new LongOpt("kid", Argument.Required, null, 'k'));
            options.Add(new LongOpt("key-id", Argument.Required, null, 'k'));
            options.Add(new LongOpt("key-version", Argument.Required, null, 2));
            options.Add(new LongOpt("key-settings", Argument.Required, null, 3));
            options.Add(new LongOpt("key-value", Argument.Required, null, 'K'));
            options.Add(new LongOpt("old-key-value", Argument.Required, null, 4));
            options.Add(new LongOpt("upper-limit", Argument.Required, null, 5));
            options.Add(new LongOpt("lower-limit", Argument.Required, null, 6));
            options.Add(new LongOpt("initial-value", Argument.Required, null, 7));
            options.Add(new LongOpt("value-flags", Argument.Required, null, 8));
            options.Add(new LongOpt("reader", Argument.Required, null, 'r'));
            options.Add(new LongOpt("reader-name", Argument.Required, null, 'r'));
            options.Add(new LongOpt("input-file", Argument.Required, null, 'i'));
            options.Add(new LongOpt("keep", Argument.No, null, 9));
            options.Add(new LongOpt("no-load-context", Argument.No, null, 10));
            options.Add(new LongOpt("no-save-context", Argument.No, null, 11));
            options.Add(new LongOpt("force", Argument.Optional, null, 'y'));
            options.Add(new LongOpt("verbose", Argument.Optional, null, 'v'));
            options.Add(new LongOpt("version", Argument.Optional, null, 'V'));
            options.Add(new LongOpt("help", Argument.No, null, 'h'));

            Getopt g = new Getopt(AppInfo.Name, args, "r:a:f:m:z:o:c:n:k:K:r:i:yv::Vh", options.ToArray());
            g.Opterr = true;

            int c;
            while ((c = g.getopt()) != -1)
            {
                string arg = g.Optarg;

                switch (c)
                {
                    case 'a':
                        if (arg == "0")
                        {
                            parameters.ApplicationId = 0;
                        }
                        else
                        {
                            if (!BinConvert.TryHexToBytes(arg, out byte[] value) || (value.Length != 3))
                            {
                                Console.WriteLine("application-id shall be a 3-byte value (in hex)");
                                return false;
                            }
                            parameters.ApplicationId = ((value[0] << 16) | (value[1] << 8) | value[2]);
                        }
                        break;
                    case 'f':
                        {
                            if (!BinConvert.TryHexToBytes(arg, out byte[] value) || (value.Length != 1))
                            {
                                Console.WriteLine("file-id shall be a 1-byte value (in hex)");
                                return false;
                            }
                            parameters.FileId = value[0];
                        }
                        break;
                    case 'm':
                        {
                            if (!BinConvert.TryHexToBytes(arg, out byte[] value) || (value.Length != 1))
                            {
                                Console.WriteLine("comm-mode shall be a 1-byte value (in hex)");
                                return false;
                            }
                            parameters.CommMode = value[0];
                        }
                        break;
                    case 1:
                        {
                            if (!BinConvert.TryHexToBytes(arg, out byte[] value) || (value.Length != 2))
                            {
                                Console.WriteLine("access-rights shall be a 2-byte value (in hex)");
                                return false;
                            }
                            parameters.AccessRights = ((value[0] << 8) | value[1]);
                        }
                        break;
                    case 'z':
                        {
                            if (!long.TryParse(arg, out parameters.Size))
                            {
                                Console.WriteLine("size shall be a valid decimal value");
                                return false;
                            }
                        }
                        break;
                    case 'o':
                        {
                            if (!long.TryParse(arg, out parameters.Offset))
                            {
                                Console.WriteLine("offset shall be a valid decimal value");
                                return false;
                            }
                        }
                        break;
                    case 'c':
                        {
                            if (!long.TryParse(arg, out parameters.Count))
                            {
                                Console.WriteLine("count shall be a valid decimal value");
                                return false;
                            }
                        }
                        break;
                    case 'n':
                        {
                            if (!long.TryParse(arg, out parameters.Number))
                            {
                                Console.WriteLine("number shall be a valid decimal value");
                                return false;
                            }
                        }
                        break;
                    case 'd':
                        {
                            if (!BinConvert.TryHexToBytes(arg, out byte[] value))
                            {
                                Console.WriteLine("data shall a valid hex value");
                                return false;
                            }
                            parameters.Data = value;
                        }
                        break;
                    case 'k':
                        {
                            if (!BinConvert.TryHexToBytes(arg, out byte[] value) || (value.Length != 1))
                            {
                                Console.WriteLine("key-id shall be a 1-byte value (in hex)");
                                return false;
                            }
                            parameters.KeyId = value[0];
                        }
                        break;
                    case 2:
                        {
                            if (!BinConvert.TryHexToBytes(arg, out byte[] value) || (value.Length != 1))
                            {
                                Console.WriteLine("key-version shall be a 1-byte value (in hex)");
                                return false;
                            }
                            parameters.KeyVersion = value[0];
                        }
                        break;
                    case 3:
                        {
                            if (!BinConvert.TryHexToBytes(arg, out byte[] value) || (value.Length != 2))
                            {
                                Console.WriteLine("key-settings shall be a 2-byte value (in hex)");
                                return false;
                            }
                            parameters.KeySettings = value;
                        }
                        break;
                    case 'K':
                        if (arg.ToLower() == "blank")
                        {
                            parameters.KeyValue = new byte[16];
                        }
                        else
                        {
                            if (!BinConvert.TryHexToBytes(arg, out byte[] value) || ((value.Length != 8) && (value.Length != 16) && (value.Length != 24)))
                            {
                                Console.WriteLine("key-value shall be a value key (8, 16 or 24 bytes in hex)");
                                return false;
                            }
                            parameters.KeyValue = value;
                        }
                        break;
                    case 4:                        
                        if (arg.ToLower() == "blank")
                        {
                            parameters.OldKeyValue = new byte[16];
                        }
                        else
                        {
                            if (!BinConvert.TryHexToBytes(arg, out byte[] value) || ((value.Length != 8) && (value.Length != 16) && (value.Length != 24)))
                            {
                                Console.WriteLine("old-key-value shall be a value key (8, 16 or 24 bytes in hex)");
                                return false;
                            }
                            parameters.OldKeyValue = value;                            
                        }
                        break;
                    case 5:
                        {
                            if (!Int64.TryParse(arg, out parameters.UpperLimit))
                            {
                                Console.WriteLine("upper-limit shall be a valid decimal value");
                                return false;
                            }
                        }
                        break;
                    case 6:
                        {
                            if (!Int64.TryParse(arg, out parameters.LowerLimit))
                            {
                                Console.WriteLine("lower-limit shall be a valid decimal value");
                                return false;
                            }
                        }
                        break;
                    case 7:
                        {
                            if (!Int64.TryParse(arg, out parameters.InitialValue))
                            {
                                Console.WriteLine("initial-value shall be a valid decimal value");
                                return false;
                            }
                        }
                        break;
                    case 8:
                        {
                            if (!BinConvert.TryHexToBytes(arg, out byte[] value) || (value.Length != 1))
                            {
                                Console.WriteLine("value-flags shall be a 1-byte value (in hex)");
                                return false;
                            }
                            parameters.ValueFlags = value[0];
                        }
                        break;
                    case 'r':
                        ReaderName = arg;
                        break;
                    case 'i':
                        InputFile = arg;
                        break;

                    case 9:
                        KeepCardAlive = true;
                        SaveContextOnExit = true;
                        LoadContextOnEnter = true;
                        break;
                    case 10:
                        SaveContextOnExit = false;
                        break;
                    case 11:
                        LoadContextOnEnter = false;
                        break;

                    case 'v':
                        Logger.ConsoleLevel = Logger.Level.Trace;
                        if (arg != null)
                        {
                            int level;
                            if (int.TryParse(arg, out level))
                                Logger.ConsoleLevel = Logger.IntToLevel(level);
                        }
                        break;

                    case 'y':
                        ForceAnswerYes = true;
                        break;

                    case 'V':
                        Version();
                        return false;

                    case 'h':
                        Usage();
                        return false;

                    default:
                        Console.WriteLine("Invalid argument on command line");
                        Hint();
                        return false;
                }
            }

            /* Verify the action */
            /* ----------------- */

            if (action == Action.Unknown)
            {
                Console.WriteLine("No action specified");
                Hint();
                return false;
            }

            return true;
        }

        void Usage()
        {
            /*                 012345679012345679012345679012345679012345679012345679012345679012345679 */
            Console.WriteLine("Usage: {0} ACTION [PARAMETERS] [[OPTIONS]]", AppInfo.Name);
            Console.WriteLine();
            ConsoleTitle("BASIC CARD DATA:");
            Console.WriteLine("  GetCardUID");
            Console.WriteLine("  GetVersion");
            Console.WriteLine("  FreeMem");
            Console.WriteLine();
            ConsoleTitle("AUTHENTICATION:");
            Console.WriteLine("  Authenticate --key-id=<INDEX> --key-value=<VALUE>");
            Console.WriteLine("  AuthenticateAes --key-id=<INDEX> --key-value=<VALUE>");
            //Console.WriteLine("  AuthenticateEV2First --key-id=<INDEX> --key-value=<VALUE>");
            //Console.WriteLine("  AuthenticateEV2Next --key-id=<INDEX> --key-value=<VALUE>");
            Console.WriteLine("  AuthenticateISO --key-id=<INDEX> --key-value=<VALUE>");
            Console.WriteLine();
            ConsoleTitle("READ/WRITE DATA:");
            Console.WriteLine("  ReadData --file-id=<INDEX> [--comm-mode=<MODE>] --offset=<POSITION IN THE FILE> --size=<NUMBER OF BYTES>");
            Console.WriteLine("  WriteData --file-id=<INDEX> [--comm-mode=<MODE>] --offset=<POSITION IN THE FILE> --data=<FILE DATA>");
            Console.WriteLine("  ReadRecords --file-id=<INDEX> [--comm-mode=<MODE>] --number=<FIRST RECORD NUMBER> --count=<NUMBER OF RECORDS>");
            Console.WriteLine("  ReadRecord --file-id=<INDEX> [--comm-mode=<MODE>] --number=<RECORD NUMBER>");
            Console.WriteLine("  WriteRecord --file-id=<INDEX> [--comm-mode=<MODE>] --number=<RECORD NUMBER> --data=<RECORD DATA>");
            //Console.WriteLine("  UpdateRecord --file-id=<INDEX> [--comm-mode=<MODE>] --number=<RECORD NUMBER> --data=<RECORD DATA>");
            Console.WriteLine("  ClearRecordFile --file-id=<INDEX>");
            Console.WriteLine("  GetValue --file-id=<INDEX>");
            Console.WriteLine("  Debit --file-id=<INDEX> --value=<VALUE>");
            Console.WriteLine("  Credit --file-id=<INDEX> --value=<VALUE>");
            Console.WriteLine("  LimitedCredit --file-id=<INDEX> --value=<VALUE>");
            Console.WriteLine("  AbortTransaction");
            Console.WriteLine("  CommitTransaction");
            Console.WriteLine();
            ConsoleTitle("MANAGEMENT OF CARD, APPLICATIONS AND FILES:");
            Console.WriteLine("  Format");
            //Console.WriteLine("  SetConfiguration");
            Console.WriteLine("  GetDelegatedInfo");
            Console.WriteLine("  GetApplicationIDs");
            Console.WriteLine("  GetDFNames");
            Console.WriteLine("  SelectApplication --application-id=<AID>");
            Console.WriteLine("  CreateApplication --application-id=<AID> --key-settings=<SETTINGS>");
            // Console.WriteLine("  CreateDelegatedApplication --application-id=<AID> --key-settings=<SETTINGS>");
            Console.WriteLine("  DeleteApplication --application-id=<AID>");
            Console.WriteLine("  GetFileIDs");
            Console.WriteLine("  GetISOFileIDs");
            Console.WriteLine("  GetFileSettings --file-id=<INDEX>");
            Console.WriteLine("  CreateBackupDataFile --file-id=<INDEX> --comm-mode=<MODE> --access-rights=<RIGHTS> --size=<FILE SIZE>");
            Console.WriteLine("  CreateCyclicRecordFile --file-id=<INDEX> --comm-mode=<MODE> --access-rights=<RIGHTS> --size=<RECORD SIZE> --count=<NUMBER OF RECORDS>");
            Console.WriteLine("  CreateLinearRecordFile --file-id=<INDEX> --comm-mode=<MODE> --access-rights=<RIGHTS> --size=<RECORD SIZE> --count=<NUMBER OF RECORDS>");
            Console.WriteLine("  CreateStDataFile --file-id=<INDEX> --comm-mode=<MODE> --access-rights=<RIGHTS> --size=<FILE SIZE>");
            //Console.WriteLine("  CreateTransactionMACFile");
            Console.WriteLine("  CreateValueFile --file-id=<INDEX> --comm-mode=<MODE> --access-rights=<RIGHTS> --upper-limit=<VALUE> --lower-limit=<VALUE> --initial-value=<VALUE> --value-flags=<FLAGS>");
            Console.WriteLine("  DeleteFile --file-id=<INDEX>");
            // Console.WriteLine("  ChangeFileSettings ");
            Console.WriteLine("  GetKeySettings --key-id=<INDEX>");
            Console.WriteLine("  GetKeyVersion --key-id=<INDEX>");
            //Console.WriteLine("  ChangeKeyEV2 ");
            Console.WriteLine("  ChangeKey --key-id=<INDEX> --key-value=<VALUE> [--old-key-value=<VALUE>] [--key-version=<VERSION>]");
            Console.WriteLine("  ChangeKeyAES --key-id=<INDEX> --key-value=<VALUE> [--old-key-value=<VALUE>] --key-version=<VERSION>");
            //Console.WriteLine("  ChangeKeySettings");
            //Console.WriteLine("  CommitReaderID");
            //Console.WriteLine("  InitializeKeySet");
            //Console.WriteLine("  FinalizeKeySet");
            //Console.WriteLine("  RollKeySet");
            Console.WriteLine();
            ConsoleTitle("PLUS...");
            Console.WriteLine("  script --input-file=<SCRIPT>");
            Console.WriteLine("  create --input-file=<JSON FILE>");
            Console.WriteLine();
            ConsoleTitle("PARAMETERS:");
            Console.WriteLine("  --reader -r <PC/SC reader with the Desfire card>");
            Console.WriteLine();
            ConsoleTitle("OPTIONS:");
            Console.WriteLine("  --keep         : preserve the card's state and context");
            Console.WriteLine("  --force -y     : overwrite the output file, if it already exists");
            Console.WriteLine("  --version -V   : show the version of the tool, and exit");
            Console.WriteLine("  -q --quiet");
            Console.WriteLine("  -v --verbose");
            Console.WriteLine();
        }

        void Hint()
        {
            Console.WriteLine("Try " + AppInfo.Name + " --help");
        }

        void Version()
        {
            ConsoleColor(ConsoleColorScheme.Title);
            Console.WriteLine("{0}", AppInfo.Name);
            ConsoleColor(ConsoleColorScheme.Banner);
            Console.WriteLine("{0}", AppInfo.VersionInfo.LegalCopyright);
            ConsoleColor(ConsoleColorScheme.Normal);
            Console.WriteLine("Version of tool: {0}", AppInfo.LongVersion);
            ConsoleColor();
        }

    }
}
