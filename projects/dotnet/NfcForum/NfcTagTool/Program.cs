using SpringCard.LibCs;
using SpringCard.LibCs.Windows;
using System;
using System.Threading;
using System.Windows.Forms;

namespace SpringCardApplication
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// 
        [STAThread]
        static void Main(string[] args)
        {
            SystemConsole.ReadArgs(args);
            Logger.ReadArgs(args);
            Logger.Debug("Ready");

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }

    }

}