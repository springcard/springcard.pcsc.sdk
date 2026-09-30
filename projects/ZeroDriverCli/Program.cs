using SpringCard.LibCs;
using SpringCard.PCSC;
using SpringCard.PCSC.ZeroDriver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ZeroDriverCli
{
    class Program : CLIProgram
    {


    static int Main(string[] args)
        {
            Program program = new Program();
            return program.main(args);
        }

        static readonly byte[] DEFAULT_LED_CMD = { 0x58, 0x1E, 0x00, 0x00, 0x00, 0x05 }; /* Blue is breathing */
        static readonly byte[] GREEN_LED_CMD = { 0x58, 0x1E, 0x00, 0x01, 0x00, 0x00 }; /* Green LED */
        static readonly byte[] RED_LED_CMD = { 0x58, 0x1E, 0x01, 0x00, 0x00, 0x00 }; /* Red LED */
        static readonly byte[] BUZZER_LONG_CMD = { 0x58, 0x1C, 0x02, 0x58 }; /* Beep, 600ms */
        static readonly byte[] BUZZER_SHORT_CMD = { 0x58, 0x1C, 0x00, 0xFA }; /* Beep, 100ms */

        volatile bool toggleAccept = true; /* Accept or reject the card */

        bool DontReconnectOnReaderLost = false; /* Set to true to skip recovery when the reader is lost */
        bool DontRunCardTransaction = false; /* Set to true to skip the card transaction */

        /* ReaderConnectionStrings: List of reader connection strings provided by the command line */
        List<string> ReaderConnectionStrings = new List<string>();

        /* HandledCards: for every reader, remember whether we have processed the card in it or not (do avoid processing twice the same card) */
        List<string> HandledCards = new List<string>();

        /* RestoreLeds: list of reader for which the LEDs shall be restored to their default state, and when */
        Dictionary<SCardReader, DateTime> RestoreLeds = new Dictionary<SCardReader, DateTime>();
#if _LEDS
        /* Thread to restore the LEDs */
        Thread threadRestoreLeds;
        volatile private bool run_led = false;
#endif


        void ExceptionHandler(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e = (Exception)args.ExceptionObject;
            Console.WriteLine("Exception {0} ({1})", e.Message, e.GetType().ToString());
            Console.WriteLine("Source: {0}", e.Source);
            Console.WriteLine("Methode: {0}", e.TargetSite);
            Console.WriteLine("Stacktrace:");
            foreach (string s in e.StackTrace.Split('\n'))
                if (!string.IsNullOrWhiteSpace(s))
                    Console.WriteLine("\t{0}", s);
            Console.WriteLine("Runtime terminating: {0}", args.IsTerminating);
        }

        protected int main(string[] args)
        {

            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(ExceptionHandler);

            /* Parse arguments, retrieve the list of readers */
            /* --------------------------------------------- */

            if (!parseArgs(args))
                return -1;

            /* Initiate a connection to every reader */
            /* ------------------------------------- */

            Logger.Trace("Starting the readers");
            foreach (string readerConnectionString in ReaderConnectionStrings)
            {
                instantiateReader(readerConnectionString);
            }
            Logger.Trace("Ready");

            /* Create a thread to restore the LEDs to their default behaviour after a red / green command */
/* ------------------------------------------------------------------------------------------ */
#if _LEDS
            threadRestoreLeds = new Thread(RestoreLedsProc);
            threadRestoreLeds.Start();
#endif
            /* Wait forever */
            /* ------------- */

            Console.WriteLine("Hit any key to exit.");
            Console.ReadKey();
            Environment.Exit(0);
            return 0;
        }

        void instantiateReader(string readerConnectionString)
        {
            if (!parseReaderConnectionString(readerConnectionString, out string address, out ushort port))
            {
                Logger.Warning("'{0}' is not a valid reader connection string", readerConnectionString);
            }
            else
            {
                Logger.Info("Initiating a connection with reader {0} (port {1})", address, port);

                SCardReaderList_CcidOverNetwork.BackgroundInstantiate(
                    readerConnectionString,
                    new SCardReaderList_CcidOver.BackgroundInstantiateCallbackEx(onNetworkReaderListInstantiated),
                    address,
                    port);
            }
        }
        void tryConnect(SCardReaderList_CcidOver readerList, object instanceTag)
        {
#if _LEDS
            run_led = false;

            if (threadRestoreLeds != null)
            {
                threadRestoreLeds.Interrupt();
                threadRestoreLeds.Join();
                threadRestoreLeds = null;
            }
#endif
            try
            {
                Thread.Sleep(2000);
            }
            catch (ThreadInterruptedException)
            {
                Console.WriteLine("Thread '{0}' awoken.", Thread.CurrentThread.Name);
            }
            catch (ThreadAbortException)
            {
                Console.WriteLine("Thread '{0}' aborted.", Thread.CurrentThread.Name);
            }
            /* Try again to instantiate the reader */
            string readerConnectionString = (string)instanceTag;
            instantiateReader(readerConnectionString);
#if _LEDS
            threadRestoreLeds = new Thread(RestoreLedsProc);
            threadRestoreLeds.Start();
#endif
        }

        void onNetworkReaderListInstantiated(object instanceTag, SCardReaderList_CcidOver readerList)
        {
            string readerConnectionString = (string)instanceTag;

            if (readerList == null)
            {
                Logger.Warning("Failed to instantiate network reader '{0}'", readerConnectionString);
                if (!DontReconnectOnReaderLost)
                {
                    tryConnect(null, instanceTag);
                }
                else
                    return;
            }
            
            /* Add the callback to be notified if the reader disconnects */
            readerList.SetDisconnectedCallback(new SCardReaderList_CcidOver.DisconnectCallbackEx(onNetworkReaderListError));

            Logger.Info("Network reader '{0}' instantiated", readerConnectionString);
            
            SCardReader_CcidOver currentReader = readerList.GetReader(0);
            if (currentReader == null)
            {
                Logger.Warning("No slot 0 for network reader '{0}'", readerConnectionString);
                Logger.Warning("Is the reader running the right firmware???");
                return;
            }

            /* Say beep and set the LEDS to their default behaviour */
            /*currentReader.Control(new CardBuffer(BUZZER_SHORT_CMD));
            Thread.Sleep(1000);
            currentReader.Control(new CardBuffer(BUZZER_LONG_CMD));*/
            currentReader.Control(new CardBuffer(DEFAULT_LED_CMD));

            /* Create the monitor */
            Logger.Trace("Creating a monitor thread for network reader '{0}', slot '{1}'", readerConnectionString, currentReader.Name);
            currentReader.StartMonitor(new SCardReader.StatusChangeCallbackEx(onReaderStatusChanged), new SCardReader.ReaderErrorCallbackEx(onReaderError));
        }

        void onNetworkReaderListError(object instanceTag)
        {
            string readerConnectionString = (string)instanceTag;

            Logger.Warning("Network reader '{0}' lost", readerConnectionString);

            if (!DontReconnectOnReaderLost)
            {
                tryConnect(null, instanceTag);
            }
            else
                return;
        }

        void onReaderStatusChanged(SCardReader reader, uint readerState, CardBuffer cardAtr)
        {
            string readerUniqueId;

            if (reader is SCardReader_CcidOver)
            {
                /* This is a zero-driver reader */
                SCardReader_CcidOver actualReaderObject = (SCardReader_CcidOver)reader;
                /* The reader belongs to a device, that is a reader list object */
                SCardReaderList_CcidOver deviceObject = actualReaderObject.ParentReaderList;
                /* The connection string to the device is the deviceObject's InstanceTag */
                string readerConnectionString = (string)deviceObject.InstanceTag;
                Logger.Info("ReaderStatusChanged for network reader '{0}', slot '{1}' {2:X08}", readerConnectionString, reader.Name, readerState);
                /* Create a unique ID for this reader (for card insertion / removal events, and to control the LEDs & buzzer) */
                readerUniqueId = string.Format("zerodriver\\{0}\\{1}", readerConnectionString, reader.Name);
            }
            else
            {
                /* This is a classical PC/SC reader */
                Logger.Info("ReaderStatusChanged for PC/SC reader '{0}' {1:X08}", reader.Name, readerState);
                /* Create a unique ID for this reader (for card insertion / removal events, and to control the LEDs & buzzer) */
                readerUniqueId = string.Format("pcsc\\{0}" + reader.Name);
            }

            if ((readerState == SCARD.STATE_UNAWARE) || (readerState == SCARD.STATE_UNAVAILABLE))
            {
                Logger.Info("\tReader lost!");
            }
            else if ((readerState & SCARD.STATE_EMPTY) != 0)
            {
                if (HandledCards.Contains(readerUniqueId))
                {
                    /* A card was present (and has been processed) */
                    Logger.Info("\tCard removed");
                    HandledCards.Remove(readerUniqueId);
                }
                else
                {
                    /* No card was present, or the card has not been processed) */
                    Logger.Info("\tCard absent");
                }
            }
            else if ((readerState & SCARD.STATE_PRESENT) != 0)
            {
                if ((readerState & SCARD.STATE_MUTE) != 0)
                {
                    Logger.Info("\tCard present, mute");
                    HandledCards.Remove(readerUniqueId);
                }
                else
                {
                    if ((readerState & SCARD.STATE_INUSE) != 0)
                    {
                        if ((readerState & SCARD.STATE_EXCLUSIVE) != 0)
                            Logger.Info("\tCard present, in use, exclusive");
                        else
                            Logger.Info("\tCard present, in use, shared");
                    }
                    else if ((readerState & SCARD.STATE_UNPOWERED) != 0)
                    {
                        Logger.Info("\tCard present, unpowered");
                    }
                    else
                    {
                        Logger.Info("\tCard present, available");
                        if (cardAtr != null)
                            Logger.Info("\t\tATR={0}", BinConvert.ToHex(cardAtr.Bytes));

                        if (!HandledCards.Contains(readerUniqueId))
                        {
                            if (!DontRunCardTransaction)
                            {
                                /* Do a card session */
                                /* ----------------- */

                                Logger.Trace("Running card transaction");

                                if (cardTransaction(reader))
                                {
                                    /* The card has been processed, remember it so we don't process the same card again -- until it is removed */
                                    HandledCards.Add(readerUniqueId);
                                }

                                /* Drive the LEDs and buzzer */
                                if (toggleAccept)
                                {
                                    reader.Control(new CardBuffer(GREEN_LED_CMD));
                                    reader.Control(new CardBuffer(BUZZER_SHORT_CMD));
                                }
                                else
                                {
                                    reader.Control(new CardBuffer(RED_LED_CMD));
                                    reader.Control(new CardBuffer(BUZZER_LONG_CMD));
                                }
                                toggleAccept = !toggleAccept;
                                /* Schedule to restore the LEDs to their default state after 2 seconds */
                                lock (RestoreLeds)
                                {
                                    RestoreLeds[reader] = DateTime.UtcNow.AddSeconds(3);
                                }
                            }
                            else
                            {
                                /* Remmember we've seen a card -- even if we don't have processed it */
                                HandledCards.Add(readerUniqueId);
                            }
                        }
                    }
                }
            }
        }

        bool cardTransaction(SCardReader reader)
        {
            bool result = false;

            /* Create a card channel from the reader */
            /* ------------------------------------- */

            /* NOTE: we can't use classical method cardChannel = new SCardChannel(reader) since reader is likely to be a SCardReader_CcidOver descendant, not an actual SCardReader */
            SCardChannel cardChannel = reader.CreateChannel();

            /* Connect to the card */
            /* ------------------- */

            if (cardChannel.ConnectExclusive())
            {
                /* Send APDU FFCA000000 to get the UID */
                /* ----------------------------------- */

                RAPDU rapdu = cardChannel.Transmit(new CAPDU(0xFF, 0xCA, 0x00, 0x00, 0x00));
                if ((rapdu != null) && (rapdu.SW == 0x9000))
                {
                    Logger.Info("\t\tUID={0}", BinConvert.ToHex(rapdu.DataBytes));

                    // TODO : write your own card transaction here


                    /* Card has been successfully processed */
                    result = true;
                }
                else
                {
                    Logger.Warning("\tFailed to read card UID");
                }
                cardChannel.DisconnectReset();
            }
            else
            {
                Logger.Warning("\tFailed to connect to the card");
            }

            return result;
        }

        void onReaderError(SCardReader reader)
        {
            if (reader is SCardReader_CcidOver)
            {
                /* This is a zero-driver reader */
                SCardReader_CcidOver actualReaderObject = (SCardReader_CcidOver)reader;
                /* The reader belongs to a device, that is a reader list object */
                SCardReaderList_CcidOver deviceObject = actualReaderObject.ParentReaderList;
                /* The connection string to the device is the deviceObject's InstanceTag */
                string readerConnectionString = (string)deviceObject.InstanceTag;
                Logger.Info("ReaderError for network reader '{0}', slot '{1}'", readerConnectionString, reader.Name);

                /* Try to re-instantiate this reader */
                if (!DontReconnectOnReaderLost)
                {
                    tryConnect(null, deviceObject.InstanceTag);
                }
                else
                    return;
            }
            else
            {
                /* This is a classical PC/SC reader */
                Logger.Debug("ReaderError for PC/SC reader '{0}'", reader.Name);
            }
        }
        void ExceptionLedHandler(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e = (Exception)args.ExceptionObject;
            Console.WriteLine("Exception {0} ({1})", e.Message, e.GetType().ToString());
            Console.WriteLine("Source: {0}", e.Source);
            Console.WriteLine("Methode: {0}", e.TargetSite);
            Console.WriteLine("Stacktrace:");
            foreach (string s in e.StackTrace.Split('\n'))
                if (!string.IsNullOrWhiteSpace(s))
                    Console.WriteLine("\t{0}", s);
            Console.WriteLine("Runtime terminating: {0}", args.IsTerminating);
        }
#if _LEDS
        void RestoreLedsProc()
        {

            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(ExceptionLedHandler);

            run_led = true;
            //for (; ; )
            while ( run_led == true)
            {
                try
                {
                    Thread.Sleep(100);
                }
                catch (ThreadInterruptedException)
                {
                    Console.WriteLine("Thread '{0}' awoken.", Thread.CurrentThread.Name);
                }
                catch (ThreadAbortException)
                {
                    Console.WriteLine("Thread '{0}' aborted.", Thread.CurrentThread.Name);
                }

                lock (RestoreLeds)
                {
                    List<SCardReader> restore = new List<SCardReader>();
                    foreach (KeyValuePair<SCardReader,DateTime> entry in RestoreLeds)
                        if (entry.Value < DateTime.UtcNow) /* Expired */
                            restore.Add(entry.Key);

                    foreach (SCardReader reader in restore)
                    {
                        RestoreLeds.Remove(reader);
                        if (reader is SCardReader_CcidOver)
                        {
                            /* This is a zero-driver reader */
                            SCardReader_CcidOver actualReaderObject = (SCardReader_CcidOver)reader;
                            /* The reader belongs to a device, that is a reader list object */
                            SCardReaderList_CcidOver deviceObject = actualReaderObject.ParentReaderList;
                            /* The connection string to the device is the deviceObject's InstanceTag */
                            string readerConnectionString = (string)deviceObject.InstanceTag;
                            Logger.Debug("Restoring LEDs on network reader '{0}', slot '{1}'", readerConnectionString, reader.Name);
                        }
                        else
                        {
                            Logger.Debug("Restoring LEDs on PC/SC reader '{0}'", reader.Name);
                        }

                        reader.Control(new CardBuffer(DEFAULT_LED_CMD));
                    }
                }
            }
        }
#endif
        protected bool parseReaderConnectionString(string readerConnectionString, out string address, out ushort port)
        {
            address = null;
            port = 3999;

            if (readerConnectionString != null)
            {
                if (readerConnectionString.Contains(":"))
                {
                    string[] e = readerConnectionString.Split(':');
                    if (e.Length != 2)
                        return false;
                    address = e[0];
                }
                else
                {
                    address = readerConnectionString;
                }
            }
            else
                return false;
            return true;
        }

        protected bool parseArgs(string[] args)
        {
            UseColors = true;
            Logger.ConsoleLevel = Logger.Level.None;
            Logger.ConsoleUseColors = true;
            Logger.ConsoleShowContext = false;
            Logger.ConsoleShowTime = false;

            List<LongOpt> longOptions = new List<LongOpt>();

            longOptions.Add(new LongOpt("verbose", Argument.Optional, null, 'v'));
            longOptions.Add(new LongOpt("verbose-time", Argument.No, null, 0x103));
            longOptions.Add(new LongOpt("verbose-context", Argument.No, null, 0x104));
            longOptions.Add(new LongOpt("no-verbose-color", Argument.No, null, 0x105));
            longOptions.Add(new LongOpt("no-verbose-colors", Argument.No, null, 0x105));
            longOptions.Add(new LongOpt("no-console-colors", Argument.No, null, 0x106));

            longOptions.Add(new LongOpt("banner", Argument.No, null, 'B'));
            longOptions.Add(new LongOpt("version", Argument.No, null, 'V'));
            longOptions.Add(new LongOpt("help", Argument.No, null, 'h'));

            string shortOptions = "v::yqVBMyh";

            /* Parse parameters */
            Getopt g = new Getopt(AppInfo.Name, args, shortOptions, longOptions.ToArray());
            g.Opterr = true;

            int c;
            while ((c = g.getopt()) != -1)
            {
                string arg = g.Optarg;
                if ((arg != null) && arg.StartsWith("="))
                    arg = arg.Substring(1);

                switch (c)
                {
                    case 'v':
                        Logger.ConsoleLevel = Logger.Level.Trace;
                        Logger.ConsoleShowContext = true;
                        Logger.ConsoleShowTime = true;
                        Logger.ConsoleUseColors = true;
                        if (arg != null)
                        {
                            int level;
                            if (int.TryParse(arg, out level))
                                Logger.ConsoleLevel = Logger.IntToLevel(level);
                        }
                        break;
                    case 'V':
                        Version();
                        return false;
                    case 'B':
                        Banner();
                        return false;
                    case 'h':
                        Usage();
                        return false;
                    case 0x103:
                        Logger.ConsoleShowTime = true;
                        break;
                    case 0x104:
                        Logger.ConsoleShowContext = true;
                        break;
                    case 0x105:
                        Logger.ConsoleUseColors = false;
                        break;
                    case 0x106:
                        UseColors = false;
                        break;
                    default:
                        Console.WriteLine("Invalid argument on command line");
                        Console.WriteLine("Try " + AppInfo.Name + " --help");
                        return false;
                }
            }

            if (g.Optind < args.Length)
            {
                for (int optind = g.Optind; optind < args.Length; optind++)
                {
                    Logger.Trace("Adding reader {0}", args[optind]);
                    if (!parseReaderConnectionString(args[optind], out string a, out ushort p))
                    {
                        Console.WriteLine("'{0}' is not a valid reader connection string", args[optind]);
                        return false;
                    }
                    ReaderConnectionStrings.Add(args[optind]);
                }
            }

            if (ReaderConnectionStrings.Count == 0)
            {
                Console.WriteLine("Please specify at least one reader");
                Console.WriteLine("Try " + AppInfo.Name + " --help");
                return false;
            }

            return true;
        }

        public void Usage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("  " + AppInfo.Name + " [OPTIONS] <READER 0> <READER 1> ... <READER N> ");
            Console.WriteLine();
        }

        public void Banner()
        {
            ConsoleColor(ConsoleColorScheme.Banner);
            Console.WriteLine(@" _____             ____      _             _____ _ _ ");
            Console.WriteLine(@"|__   |___ ___ ___|    \ ___|_|_ _ ___ ___|     | |_|");
            Console.WriteLine(@"|   __| -_|  _| . |  |  |  _| | | | -_|  _|   --| | |");
            Console.WriteLine(@"|_____|___|_| |___|____/|_| |_|\_/|___|_| |_____|_|_|");
            Console.WriteLine();
            ConsoleColor();
        }

        public void Version()
        {
            ConsoleColor(System.ConsoleColor.White);
            Console.WriteLine(AppInfo.Name + " version " + AppInfo.Version);
            ConsoleColor();
            Console.WriteLine("Build: {0}", AppInfo.FullVersion);
            Console.WriteLine("\tVersion of SpringCard.PCSC.dll: {0}", AppInfo.GetAssemblyInfo("SpringCard.PCSC.dll"));
            Console.WriteLine("\tVersion of SpringCard.PCSC.ZeroDriver.dll: {0}", AppInfo.GetAssemblyInfo("SpringCard.PCSC.ZeroDriver.dll"));
            Console.WriteLine(AppInfo.Copyright);
            Console.WriteLine();
        }

    }
}
