using SpringCard.LibCs;
using SpringCard.AppleVas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassKitDecryptCli
{
    class Program : CLIProgram
    {
        const string ProgName = "PassKitDecrypt";
        const string DefaultMerchantName = "pass.com.springcard.springblue.generic";
        const string DefaultPrivateKey = "MHcCAQEEICp+PT7K8FQSOi2HED1Ar5RqxxN2EkiKJMCSfaL4htYNoAoGCCqGSM49AwEHoUQDQgAE9RCZaHxXUIjQFQnwKmq6+cVqFBNO6ZKQmekosMQRZmutPs8szUsiLokILdaiT/7F5qUl8qSfEvlocYy6z98jIw==";

        bool Quiet = false;
        bool Json = false;

        byte[] InputMessage = null;
        byte[] PrivateKey = null;

        string MerchantName = null;
        byte[] MerchantID = null;

        int OutputText = 0;
        int OutputHex = 0;
        int OutputBase64 = 0;

        static int Main(string[] args)
        {
            Program p = new Program();
            return p.Run(args);
        }

        int Run(string[] args)
        {
            if (!ParseArgs(args))
                return 1;

            if (MerchantName != null)
            {
                Logger.Debug("Translating the merchant name");

                MerchantID = AppleVasConfig.ComputeId(MerchantName);
                if (!Quiet)
                    Console.WriteLine("Merchant ID={0}", StrUtils.Base64UrlEncode(MerchantID));
            }

            AppleVasConfig config = new AppleVasConfig(MerchantID);

            config.AddPrivateKey(PrivateKey);

            AppleVasData OutputMessage = AppleVasData.Create(config, InputMessage, out AppleVasError error);

            if (OutputMessage == null)
            {
                OutputError("Failed to extract VAS message", error);
                return 2;
            }

            if (Json)
            {
                Console.WriteLine("{");
                OutputField("result", error.ToString());
            }

            if (OutputText > 0)
                OutputField("nfcMessage", OutputMessage.Text);
            if (OutputHex > 0)
                OutputField("nfcMessage", OutputMessage.HexBytes);
            if (OutputBase64 > 0)
                OutputField("nfcMessage", OutputMessage.Base64Bytes);
            OutputField("nfcTimestamp", OutputMessage.Timestamp.ToString("yyyy-MM-ddTHH:mm:ssZ"), true);

            if (Json)
            {
                Console.WriteLine("}");
            }

            return 0;
        }

        void OutputError(string context, AppleVasError error)
        {
            if (Json)
            {
                Console.WriteLine("{");
                OutputField("result", error.ToString(), true);
                Console.WriteLine("}");
            }
            else
            {
                ConsoleError(context);
                ConsoleError(error.ToString());
            }
        }

        void OutputField(string fieldName, string fieldValue, bool last = false)
        {
            if (Json)
            {
                Console.WriteLine("\t\"{0}\": \"{1}\"{2}", fieldName, fieldValue, last ? "" : ",");
            }
            else
            {
                Console.Write("{0}: ", fieldName);
                ConsoleColor(System.ConsoleColor.White);
                Console.Write("{0}", fieldValue);
                ConsoleColor();
                Console.WriteLine();
            }           
        }

        void Usage()
        {
            Console.WriteLine("Usage: {0} [PARAMETERS] [OUPUT FORMAT] [[OPTIONS]]", ProgName);
            Console.WriteLine();
            ConsoleTitle("PARAMETERS");
            Console.WriteLine("  -N --merchant-name=<MERCHANT NAME>");
            Console.WriteLine("  -I --merchant-id=<MERCHANT ID (base64)>");
            Console.WriteLine("  -M --message=<MESSAGE (base64)>");
            Console.WriteLine("  -K --private-key=<PRIVATE KEY (base64)>");
            Console.WriteLine();
            ConsoleTitle("OUTPUT FORMAT:");
            Console.WriteLine("  -r --raw     : hex");
            Console.WriteLine("  -b --base64  : base64");
            Console.WriteLine("  -t --text    : text (default)");
            Console.WriteLine();
            ConsoleTitle("OPTIONS:");
            Console.WriteLine("  -j --json    : decorate the output as JSON");
            Console.WriteLine("  -q --quiet");
            Console.WriteLine("  -v --verbose");
            Console.WriteLine();
            Console.WriteLine();
        }

        bool ParseArgs(string[] args)
        {
            int c;

            List<LongOpt> options = new List<LongOpt>();
            options.Add(new LongOpt("message", Argument.Required, null, 'M'));
            options.Add(new LongOpt("message-hex", Argument.Required, null, 'H'));
            options.Add(new LongOpt("private-key", Argument.Required, null, 'K'));
            options.Add(new LongOpt("merchant-name", Argument.Required, null, 'N'));
            options.Add(new LongOpt("merchant-id", Argument.Required, null, 'I'));
            options.Add(new LongOpt("text", Argument.No, null, 't'));
            options.Add(new LongOpt("raw", Argument.No, null, 'r'));
            options.Add(new LongOpt("base64", Argument.No, null, 'b'));
            options.Add(new LongOpt("json", Argument.No, null, 'j'));
            options.Add(new LongOpt("quiet", Argument.No, null, 'q'));
            options.Add(new LongOpt("verbose", Argument.Optional, null, 'v'));
            options.Add(new LongOpt("help", Argument.No, null, 'h'));

            Getopt g = new Getopt(ProgName, args, "N:I:M:H:K:trbjqv::h", options.ToArray());
            g.Opterr = true;

            while ((c = g.getopt()) != -1)
            {
                string arg = g.Optarg;

                switch (c)
                {
                    case 'M':
                        if (!StrUtils.Base64UrlTryDecode(arg, out InputMessage))
                        {
                            Console.WriteLine("The MESSAGE parameter must be a base64 string");
                            return false;
                        }
                        break;
                    case 'H':
                        if (!BinConvert.TryHexToBytes(arg, out InputMessage))
                        {
                            Console.WriteLine("The MESSAGE parameter must be an hex string");
                            return false;
                        }
                        break;
                    case 'K':
                        if (!StrUtils.Base64UrlTryDecode(arg, out PrivateKey))
                        {
                            Console.WriteLine("The PRIVATE KEY parameter must be a base64 string");
                            return false;
                        }
                        break;
                    case 'N':
                        MerchantName = arg;
                        break;
                    case 'I':
                        if (!StrUtils.Base64UrlTryDecode(arg, out MerchantID))
                        {
                            Console.WriteLine("The MERCHANT ID parameter must be a base64 string");
                            return false;
                        }
                        if (MerchantID.Length != 32)
                        {
                            Console.WriteLine("The MERCHANT ID parameter must be a 32-byte value");
                            return false;
                        }
                        break;
                    case 't':
                        OutputText = 1;
                        break;
                    case 'r':
                        OutputHex = 1;
                        break;
                    case 'b':
                        OutputBase64 = 1;
                        break;
                    case 'j':
                        Json = true;
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

                    case 'h':
                        Usage();
                        return false;

                    default:
                        Console.WriteLine("Invalid argument on command line");
                        Console.WriteLine("Try " + ProgName + " --help");
                        return false;
                }
            }

            if (InputMessage == null)
            {
                Console.WriteLine("You must specify the input message");
                Console.WriteLine("Try " + ProgName + " --help");
                return false;
            }

            if ((MerchantID != null) && (MerchantName != null))
            {
                Console.WriteLine("You can't both specify a merchant name and a merchant ID");
                Console.WriteLine("Try " + ProgName + " --help");
                return false;
            }

            if ((MerchantID == null) && (MerchantName == null))
            {
                MerchantName = DefaultMerchantName;
                Console.WriteLine("Using default merchant name: {0}", MerchantName);
            }

            if (PrivateKey == null)
            {
                Console.WriteLine("Using default private key");
                PrivateKey = StrUtils.Base64UrlDecode(DefaultPrivateKey);
            }

            switch (OutputText + OutputHex + OutputBase64)
            {
                case 0:
                    /* Default */
                    OutputText = 1;
                    break;

                case 1:
                    /* One value selected */
                    break;

                default:
                    Console.WriteLine("You must specify one output format (and only one)");
                    return false;
            }

            return true;
        }
    }
}
