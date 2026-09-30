using SpringCard.GoogleVas;
using SpringCard.LibCs;
using SpringCard.LibCs.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SmartTapRdr
{
    static class Program
    {
        /// <summary>
        /// Point d'entrée principal de l'application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            SystemConsole.ReadArgs(args);
            Logger.ReadArgs(args);

            Logger.Trace("Validating installation...");
			if (!AppUtils.VerifyAssemblies())
				return;

            string ConfigFileName = null;
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].StartsWith("-"))
                    continue;
                if (ConfigFileName == null)
                    ConfigFileName = args[i];
            }

            Logger.Info("SpringCard.GoogleVAS library version: {0}", Library.ModuleInfo.LongVersion);

            Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            MainForm f = new MainForm(args);
            if (ConfigFileName != null)
                f.LoadConfigFromFile(ConfigFileName);
            Application.Run(f);
        }

        static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            // Log the exception, display it, etc
            MessageBox.Show(e.Exception.Message, "Fatal execution error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            // Log the exception, display it, etc
            MessageBox.Show((e.ExceptionObject as Exception).Message, "Fatal execution error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
        }
    }
}

