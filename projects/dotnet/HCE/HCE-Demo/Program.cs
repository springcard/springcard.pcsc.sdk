using SpringCard.LibCs;
using SpringCard.PCSC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HCE_Demo
{
    class Program : CLIProgram
    {
        const string ProgName = "hce-demo";
        bool Quiet = false;
        string PcscReaderName = "SpringCard * Contactless *";

        static void Main(string[] args)
        {
            Program p = new Program();
            p.Run(args);
        }

        void Run(string[] args)
        { 
            Console.WriteLine("HCE-Demo");

            if (!ParseArgs(args))
                return;

            if (!Quiet)
            {
                Banner();
            }

            string actualReaderName = SCARD.ReaderLike(PcscReaderName);

            if (string.IsNullOrEmpty(actualReaderName))
            {
                ConsoleColor(ConsoleColorScheme.Error);
                Console.WriteLine("No reader matching {0}", PcscReaderName);
                ConsoleColor();
                return;
            }

            Console.WriteLine("Running HCE over reader {0}", actualReaderName);

            Cardlet cardlet = new Cardlet(actualReaderName);

            if (!cardlet.SetReaderInListenerMode())
            {
                ConsoleColor(ConsoleColorScheme.Error);
                Console.WriteLine("Failed to set the reader in listener mode");
                ConsoleColor();
                return;
            }

            if (!cardlet.StartEmulation())
            {
                ConsoleColor(ConsoleColorScheme.Error);
                Console.WriteLine("Failed to start card emulation");
                ConsoleColor();
                return;
            }

            Console.WriteLine("Card emulation is running, press any key to exit");
            Console.ReadKey(true);

            Console.WriteLine("Stopping card emulation...");

            cardlet.StopEmulation();
            cardlet.SetReaderInPollerMode();

            ConsoleColor(ConsoleColorScheme.Info);
            Console.WriteLine("Done");
            ConsoleColor();
        }

        void Banner()
        {
            ConsoleColor(ConsoleColorScheme.Title);
            /* http://patorjk.com/software/taag/#p=display&f=Rectangles&t=HCE%20Demo */
            Console.WriteLine(" _____ _____ _____    ____                ");
            Console.WriteLine("|  |  |     |   __|  |    \\ ___ _____ ___ ");
            Console.WriteLine("|     |   --|   __|  |  |  | -_|     | . |");
            Console.WriteLine("|__|__|_____|_____|  |____/|___|_|_|_|___|");
            Console.WriteLine();
            ConsoleColor();
        }

        void Version()
        {
            ConsoleColor(ConsoleColorScheme.Title);
            Console.WriteLine("{0}", ProgName);
            ConsoleColor(ConsoleColorScheme.Banner);
            Console.WriteLine("{0}", AppInfo.VersionInfo.LegalCopyright);
            ConsoleColor(ConsoleColorScheme.Normal);
            Console.WriteLine("Version of tool: {0}", AppInfo.LongVersion);
            Console.WriteLine("Version of SpringCard.PCSC.CardEmulation.dll: {0}", SpringCard.PCSC.CardEmulation.Library.ModuleInfo.LongVersion);
            ConsoleColor();
        }

        void Usage()
        {
            Console.WriteLine("Usage: {0} [PARAMETERS] [[OPTIONS]]", ProgName);
            Console.WriteLine();
            ConsoleTitle("PARAMETERS:");
            Console.WriteLine("  --reader -r <NFC HCE-capable PC/SC reader>");
            Console.WriteLine();
            ConsoleTitle("OPTIONS:");
            Console.WriteLine("  --version -V   : show the version of the tool, and exit");
            Console.WriteLine("  -q --quiet");
            Console.WriteLine("  -v --verbose");
            Console.WriteLine();
        }

        void Hint()
        {
            Console.WriteLine("Try " + ProgName + " --help");
        }

        bool ParseArgs(string[] args)
        {
            /* Read the parameters and options */
            /* ------------------------------- */

            List<LongOpt> options = new List<LongOpt>();
            options.Add(new LongOpt("reader", Argument.Required, null, 'r'));
            options.Add(new LongOpt("quiet", Argument.No, null, 'q'));
            options.Add(new LongOpt("verbose", Argument.Optional, null, 'v'));
            options.Add(new LongOpt("version", Argument.Optional, null, 'V'));
            options.Add(new LongOpt("help", Argument.No, null, 'h'));

            Getopt g = new Getopt(ProgName, args, "r:qv::Vh", options.ToArray());
            g.Opterr = true;

            int c;
            while ((c = g.getopt()) != -1)
            {
                string arg = g.Optarg;

                switch (c)
                {
                    case 'r':
                        PcscReaderName = arg;
                        break;

                    case 'q':
                        Quiet = true;
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

            return true;
        }

    }
}
