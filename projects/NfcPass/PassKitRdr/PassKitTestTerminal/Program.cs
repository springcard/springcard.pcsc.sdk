using SpringCard.AppleVas;
using SpringCard.LibCs;
using SpringCard.PCSC;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Timers;
using System.Threading;
using System.Threading.Tasks;

namespace PassKitTestTerminal
{
    class Program : CLIProgram
    {
        const string ProgName = "PassKitTestTerminal";

        enum ActionE
        {
            Unknown,
            Help,
            Version,
            SelfTest,
            KeyId,
            ListReaders,
            Transactions
        }
        ActionE Action = ActionE.Unknown;

        string ConfigFile = null;
        string KeyInputFile = null;
        int ReaderIndex = -1;
        string ReaderName = null;
        bool Once = false;
        bool Stress = false;
        AppleVasTerminalConfig terminalConfig = null;
        SCardReader terminalReader;
        int TransactionCount = 0;
        int TransactionOkCount = 0;
        int TransactionKoCount = 0;
        int FieldOffAfter = 1;
        int FieldOffLength = 3;
        System.Timers.Timer timerFieldOff;
        System.Timers.Timer timerFieldOn;

        static int Main(string[] args)
        {
            Program p = new Program();
            return p.Run(args);
        }

        int Run(string[] args)
        {
            ConsoleTitle(ProgName);

            if (!ParseArgs(args))
            {
                Console.WriteLine("Try " + ProgName + " --help");
                return 1;
            }

            if (Action == ActionE.Help)
            {
                Usage();
                return 0;
            }

            if (Action == ActionE.Version)
            {
                ConsoleColor(ConsoleColorScheme.Info);
                Console.WriteLine("SpringCard.AppleVAS library version: {0}", SpringCard.AppleVas.Library.ModuleInfo.LongVersion);
                ConsoleColor();
                return 0;
            }

            if (Action == ActionE.SelfTest)
            {
                if (!AppleVasTerminal.SelfTest())
                {
                    ConsoleColor(ConsoleColorScheme.Error);
                    Console.WriteLine("SpringCard.AppleVAS self-test failed");
                    ConsoleColor();
                    return 1;
                }
                else
                {
                    ConsoleColor(ConsoleColorScheme.Success);
                    Console.WriteLine("SpringCard.AppleVAS self-test OK");
                    ConsoleColor();
                    return 0;
                }
            }

            if (Action == ActionE.KeyId)
            {
                uint keyId = AppleVasCrypto.ECC.ComputeKeyIdFromPem(KeyInputFile);
                Console.WriteLine("{0:X08}", keyId);
                return 0;
            }

            Logger.Debug("Loading the list of PC/SC readers");

            string[] ReaderNames = (new SCardReaderList()).Readers;

            if (Action == ActionE.ListReaders)
            {
                Console.WriteLine(string.Format("{0} PC/SC Reader(s) found", ReaderNames.Length));
                for (int i = 0; i < ReaderNames.Length; i++)
                    Console.WriteLine(string.Format("{0}: {1}", i, ReaderNames[i]));
                return 0;
            }

            if (!File.Exists(ConfigFile))
            {
                ConsoleColor(ConsoleColorScheme.Error);
                Console.WriteLine("File {0} not found", ConfigFile);
                ConsoleColor();
                return 1;
            }

            try
            {
                terminalConfig = AppleVasTerminalConfig.LoadFromJsonFile(ConfigFile);
            }
            catch (Exception e)
            {
                ConsoleColor(ConsoleColorScheme.Error);
                Console.WriteLine("Failed to load the configuration");
                Console.WriteLine("Error: {0}", e.Message);
                ConsoleColor();
                return 1;
            }

            ConsoleColor(ConsoleColorScheme.Info);
            Console.WriteLine(terminalConfig.Description);
            ConsoleColor();

            if (ReaderName == null)
            {
                if (ReaderIndex < 0)
                    ReaderIndex = 0;

                Logger.Debug("Selecting the PC/SC reader at index {0}", ReaderIndex);

                if ((ReaderIndex >= ReaderNames.Length))
                {
                    ConsoleColor(ConsoleColorScheme.Error);
                    Console.WriteLine("No PC/SC Reader at index {0}", ReaderIndex);
                    Console.WriteLine("Use " + ProgName + " --list-readers to show the available reader(s)");
                    ConsoleColor();
                    return 1;
                }

                ReaderName = ReaderNames[ReaderIndex];
            }

            Logger.Debug("Using PC/SC reader {0}", ReaderName);

            if (Stress)
            {
                timerFieldOff = new System.Timers.Timer(1000 * FieldOffAfter);
                timerFieldOff.Elapsed += RfFieldOff;
                timerFieldOff.AutoReset = true;
                timerFieldOff.Enabled = false;
                timerFieldOn = new System.Timers.Timer(1000 * FieldOffLength);
                timerFieldOn.Elapsed += RfFieldOn;
                timerFieldOn.AutoReset = true;
                timerFieldOn.Enabled = false;
            }

            SCardChannel directChannel = new SCardChannel(ReaderName);
            if (directChannel.ConnectDirect())
            {
                directChannel.Control(new byte[] { 0x58, 0x23, 0x00 });
                directChannel.DisconnectLeave();
            }

            terminalReader = new SCardReader(ReaderName);
            terminalReader.StartWaitCard(new SCardReader.CardConnectedCallback(CardConnectedCallback), new SCardReader.CardRemovedCallback(CardRemovedCallback));

            Console.WriteLine("Press any key to exit.");

            Console.ReadKey(true);

            Console.WriteLine("Exit required...");
            terminalReader.StopWaitCard();
            Console.WriteLine("Bye.");
            return 0;
        }

        void CardRemovedCallback()
        {
            if (Stress)
            {

            }
            else
            {
                Console.WriteLine("Place the mobile over the reader");
            }
        }

        void CardConnectedCallback(SCardChannel cardChannel)
        {
            if (Stress)
            {
                timerFieldOff.Enabled = false;
                timerFieldOn.Enabled = false;
            }

            Console.WriteLine("NFC link OK - Trying to run the transaction");

            TransactionCount++;

            int field_reset = 0;

            AppleVasTerminal terminal = new AppleVasTerminal(terminalConfig);

            if (terminal.DoTransaction(cardChannel, out AppleVasData data, out AppleVasError error))
            {                
                if (data != null)
                {
                    ConsoleColor(ConsoleColorScheme.Success);
                    Console.WriteLine("Transaction OK, got a message:");
                    ConsoleColor(ConsoleColorScheme.Info);
                    Console.WriteLine("\tMessage: {0}", data.Text);
                    Console.WriteLine("\tTimestamp: {0}", data.Timestamp.ToString());
                    ConsoleColor();
                }
                else
                {
                    ConsoleColor(ConsoleColorScheme.Success);
                    Console.WriteLine("Transaction OK, no message");
                    ConsoleColor();
                }
                field_reset = 2000;
                TransactionOkCount++;
            }
            else if (error == AppleVasError.WaitingForActivation)
            {
                ConsoleColor(ConsoleColorScheme.Warning);
                Console.WriteLine("Waiting for activation -- use biometry or pinpad to activate");
                ConsoleColor();
                field_reset = 100;
            }
            else
            {
                ConsoleColor(ConsoleColorScheme.Error);
                Console.WriteLine("Transaction failed, reason: {0}", error.ToString());
                ConsoleColor();
                TransactionKoCount++;
            }


            if (Once)
            {
                Console.WriteLine("--once specified, exiting...");
                cardChannel.DisconnectLeave();
                terminalReader.StopWaitCard();
                Console.WriteLine("Bye.");
                Environment.Exit(0);
            }
            else
            {
                Console.WriteLine("{0} transactions, OK: {1}, Failed: {2}", TransactionCount, TransactionOkCount, TransactionKoCount);
                Console.WriteLine();
                Console.WriteLine("Remove the mobile from the reader");
                terminal.FieldReset(cardChannel, field_reset);
                cardChannel.DisconnectLeave();
                if (Stress)
                    timerFieldOff.Enabled = true;
            }
        }

        private void RfFieldOff(Object source, ElapsedEventArgs e)
        {
            timerFieldOff.Enabled = false;
            Console.WriteLine("RF field OFF");
            SCardChannel directChannel = new SCardChannel(ReaderName);
            if (directChannel.ConnectDirect())
            {
                directChannel.Control(new byte[] { 0x58, 0x22, 0x00 });
                directChannel.Control(new byte[] { 0x58, 0x0A, 0x00 });
                directChannel.DisconnectLeave();
            }
            timerFieldOn.Enabled = true;
        }

        private void RfFieldOn(Object source, ElapsedEventArgs e)
        {
            timerFieldOn.Enabled = false;
            Console.WriteLine("RF field ON");
            SCardChannel directChannel = new SCardChannel(ReaderName);
            if (directChannel.ConnectDirect())
            {
                directChannel.Control(new byte[] { 0x58, 0x23, 0x00 });
                directChannel.DisconnectLeave();
            }
        }

        void Usage()
        {
            Console.WriteLine("Usage: {0} [PARAMETERS] [[OPTIONS]]", ProgName);
            Console.WriteLine();
            ConsoleTitle("PARAMETERS");
            Console.WriteLine("  -c --config=<CONFIG FILE (JSON)>");
            Console.WriteLine();
            ConsoleTitle("To enumerate the PC/SC Reader(s), use");
            Console.WriteLine("  " + ProgName + " --list-readers");
            Console.WriteLine("  " + ProgName + " -L");
            Console.WriteLine();
            ConsoleTitle("OPTIONS:");
            Console.WriteLine("  -i --reader-index=<INDEX> (default is 0)");
            Console.WriteLine("  -n --reader-name=<NAME>");
            Console.WriteLine("  -o --once : exit after a transaction, instead of remaining ready for new transactions");
            Console.WriteLine("  -s --stress : stress mode");
            Console.WriteLine("  -V --version : show version and exit");
            Console.WriteLine("  -w --no-color : do not change the colors of the console");
            Console.WriteLine("  -v --verbose");
            Console.WriteLine();
        }

        bool ParseArgs(string[] args)
        {
            int c;

            List<LongOpt> options = new List<LongOpt>();
            options.Add(new LongOpt("config", Argument.Required, null, 'c'));
            options.Add(new LongOpt("list-readers", Argument.No, null, 'L'));
            options.Add(new LongOpt("reader-index", Argument.Required, null, 'i'));
            options.Add(new LongOpt("reader-name", Argument.Required, null, 'n'));
            options.Add(new LongOpt("once", Argument.No, null, 'o'));
            options.Add(new LongOpt("stress", Argument.No, null, 's'));
            options.Add(new LongOpt("selftest", Argument.No, null, 'T'));
            options.Add(new LongOpt("version", Argument.No, null, 'V'));
            options.Add(new LongOpt("key-id", Argument.Required, null, 'K'));
            options.Add(new LongOpt("off-after", Argument.Required, null, 1));
            options.Add(new LongOpt("off-length", Argument.Required, null, 2));
            options.Add(new LongOpt("no-color", Argument.No, null, 'w'));
            options.Add(new LongOpt("verbose", Argument.Optional, null, 'v'));
            options.Add(new LongOpt("help", Argument.No, null, 'h'));

            Getopt g = new Getopt(ProgName, args, "c:Li:n:K:osTVwv::h", options.ToArray());
            g.Opterr = true;

            while ((c = g.getopt()) != -1)
            {
                string arg = g.Optarg;

                switch (c)
                {
                    case 1:
                        FieldOffAfter = int.Parse(arg);
                        break;
                    case 2:
                        FieldOffLength = int.Parse(arg);
                        break;
                    case 'c':
                        if (Action != ActionE.Unknown)
                        {
                            Console.WriteLine("Please specify a single action");
                            return false;
                        }
                        Action = ActionE.Transactions;
                        ConfigFile = arg;
                        break;
                    case 'L':
                        if (Action != ActionE.Unknown)
                        {
                            Console.WriteLine("Please specify a single action");
                            return false;
                        }
                        Action = ActionE.ListReaders;
                        break;
                    case 'i':
                        ReaderIndex = int.Parse(arg);
                        break;
                    case 'n':
                        ReaderName = arg;
                        break;
                    case 'o':
                        Once = true;
                        break;
                    case 's':
                        Stress = true;
                        break;
                    case 'K':
                        if (Action != ActionE.Unknown)
                        {
                            Console.WriteLine("Please specify a single action");
                            return false;
                        }
                        Action = ActionE.KeyId;
                        KeyInputFile = arg;
                        break;
                    case 'T':
                        if (Action != ActionE.Unknown)
                        {
                            Console.WriteLine("Please specify a single action");
                            return false;
                        }
                        Action = ActionE.SelfTest;
                        break;
                    case 'V':
                        if (Action != ActionE.Unknown)
                        {
                            Console.WriteLine("Please specify a single action");
                            return false;
                        }
                        Action = ActionE.Version;
                        break;
                    case 'w':
                        UseColors = false;
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

                    case 'h':
                        Action = ActionE.Help;
                        return true;

                    default:
                        Console.WriteLine("Invalid argument on command line");
                        return false;
                }
            }

            /* Default Action is Transactions */
            if (Action == ActionE.Unknown)
                Action = ActionE.Transactions;

            if (Action == ActionE.Transactions)
            {
                if ((ReaderIndex != -1) && (ReaderName != null))
                {
                    Console.WriteLine("You can't both specify a reader name and a reader index");
                    return false;
                }

                if (ConfigFile == null)
                {
                    Console.WriteLine("You must specify the configuration file");
                    return false;
                }
            }

            return true;
        }
    }
}
