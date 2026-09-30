/*
 * Created by SharpDevelop.
 * User: johann
 * Date: 02/03/2012
 * Time: 17:56
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using SpringCard.LibCs;
using SpringCard.LibCs.Windows;

namespace PcscDiag2
{
    /// <summary>
    /// Class with program entry point.
    /// </summary>
    internal sealed class Program
    {
        public const string ProgName = "PcscDiag2";

        public static bool allowMulti = false;
        public static bool quiet = false;

        [STAThread]
        private static void Main(string[] args)
        {
            SystemConsole.ReadArgs(args);
            Logger.ReadArgs(args);
            ParseArgs(args);
			if (!AppUtils.VerifyAssemblies())
				return;

            Logger.Info("{0} version {1}", ProgName, AppInfo.Version);

            if (!AppUtils.IsSingleInstance())
            {
                Logger.Info("Another instance is already running");
                if (!allowMulti)
                {
                    Logger.Trace("Restoring earlier instance...");
                    AppUtils.RestorePreviousInstance(true);
                    Logger.Trace("...and exiting this one");
                    return;
                }
            }

            SpringCard.PCSC.SCARD.UseLogger = true;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm(quiet));
        }

        static void ParseArgs(string[] args)
        {
            List<LongOpt> options = new List<LongOpt>();
            options.Add(new LongOpt("multi", Argument.No, null, 'm'));
            options.Add(new LongOpt("quiet", Argument.No, null, 'q'));
            options.Add(new LongOpt("console", Argument.No, null, 'c'));
            options.Add(new LongOpt("verbose", Argument.Optional, null, 'v'));
            options.Add(new LongOpt("help", Argument.No, null, 'h'));

            Getopt g = new Getopt(ProgName, args, "qmcv::h", options.ToArray());
            g.Opterr = true;

            int c;
            while ((c = g.getopt()) != -1)
            {
                string arg = g.Optarg;
                if ((arg != null) && (arg.StartsWith("=")))
                    arg = arg.Substring(1);

                switch (c)
                {
                    case 'q':
                        quiet = true;
                        break;

                    case 'm':
                        allowMulti = true;
                        break;

                    case 'c':
                        SystemConsole.Show();
                        break;

                    case 'v':
                        SystemConsole.Show();
                        Logger.ConsoleLevel = Logger.Level.Trace;
                        if (arg != null)
                        {
                            int level;
                            if (int.TryParse(arg, out level))
                                Logger.ConsoleLevel = Logger.IntToLevel(level);

                        }
                        break;
                }
            }
        }
    }
}