using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using SpringCard.AppleVas;
using SpringCard.LibCs;
using SpringCard.LibCs.Windows;

namespace PassKitRdr
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

            Logger.Info("SpringCard.AppleVAS library version: {0}", Library.ModuleInfo.LongVersion);

            Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.Run(new MainForm(args));
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
